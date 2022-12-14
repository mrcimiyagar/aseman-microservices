using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using FileService.DbContexts;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using SharedArea.Commands.File;
using SharedArea.Commands.Internal.Notifications;
using SharedArea.Commands.Internal.Requests;
using SharedArea.Entities;
using SharedArea.Middles;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using File = System.IO.File;
using JsonSerializer = SharedArea.Utils.JsonSerializer;

namespace FileService.Consumers
{
    public class FileConsumer : IConsumer<UploadPhotoRequest>, IConsumer<UploadAudioRequest>
        , IConsumer<UploadVideoRequest>, IConsumer<DownloadFileRequest>, IConsumer<DownloadBotAvatarRequest>
        , IConsumer<DownloadRoomAvatarRequest>, IConsumer<DownloadComplexAvatarRequest>
        , IConsumer<DownloadUserAvatarRequest>, IConsumer<ConsolidateDeleteAccountRequest>,
        IConsumer<WriteToFileRequest>
        , IConsumer<GetFileSizeRequest>
    {
        public const string DirPath = @"C:\\Aseman\Files";

        public async Task Consume(ConsumeContext<GetFileSizeRequest> context)
        {
            using (var dbContext = new DatabaseContext())
            {
                var file = dbContext.Files.Find(context.Message.Packet.File.FileId);
                if (file == null)
                {
                    await context.RespondAsync(new GetFileSizeResponse()
                    {
                        Packet = new Packet() {Status = "error_1"}
                    });
                    return;
                }

                await context.RespondAsync(new GetFileSizeResponse()
                {
                    Packet = new Packet() {Status = "success", File = file}
                });
            }
        }

        public async Task Consume(ConsumeContext<WriteToFileRequest> context)
        {
            using (var dbContext = new DatabaseContext())
            {
                var session = dbContext.Sessions.Find(context.Message.SessionId);
                var streamCode = context.Message.StreamCode;

                var file = dbContext.Files.Find(context.Message.Packet.File.FileId);
                if (file == null)
                {
                    await context.RespondAsync(new WriteToFileResponse()
                    {
                        Packet = new Packet() {Status = "error_1"}
                    });
                    return;
                }

                if (file.UploaderId != session.BaseUserId)
                {
                    await context.RespondAsync(new WriteToFileResponse()
                    {
                        Packet = new Packet() {Status = "error_2"}
                    });
                    return;
                }

                var myContent = JsonConvert.SerializeObject(new Packet()
                {
                    Username = SharedArea.GlobalVariables.FILE_TRANSFER_USERNAME,
                    Password = SharedArea.GlobalVariables.FILE_TRANSFER_PASSWORD,
                    StreamCode = streamCode
                });
                var buffer = System.Text.Encoding.UTF8.GetBytes(myContent);

                var client = WebRequest.Create(SharedArea.GlobalVariables.SERVER_URL +
                                               SharedArea.GlobalVariables.FILE_TRANSFER_GET_UPLOAD_STREAM_URL);
                client.Method = "POST";
                client.ContentType = "application/json";
                var encoding = new ASCIIEncoding();

                var newStream = client.GetRequestStream();
                newStream.Write(buffer, 0, buffer.Length);
                newStream.Close();

                var res = client.GetResponse();
                var responseStream = res.GetResponseStream();

                if (responseStream != null)
                {
                    Directory.CreateDirectory(DirPath);
                    var filePath = DirPath + @"\" + file.FileId;
                    File.Create(filePath).Close();

                    using (var stream = new FileStream(filePath, FileMode.Append))
                    {
                        var b = new byte[128 * 1024];
                        int read;
                        while ((read = responseStream.Read(b, 0, b.Length)) > 0)
                        {
                            stream.Write(b, 0, read);
                            file.Size += read;
                            dbContext.SaveChanges();
                        }
                    }

                    if (file is Photo photo && photo.IsAvatar)
                    {
                        using (var image = Image.Load(filePath))
                        {
                            float width, height;
                            if (image.Width > image.Height)
                            {
                                width = image.Width > 256 ? 256 : image.Width;
                                height = (float) image.Height / (float) image.Width * width;
                            }
                            else
                            {
                                height = image.Height > 256 ? 256 : image.Height;
                                width = (float) image.Width / (float) image.Height * height;
                            }

                            image.Mutate(x => x.Resize((int) width, (int) height));
                            File.Delete(filePath);
                            image.Save(filePath + ".png");
                        }
                    }
                }


                await context.RespondAsync(new WriteToFileResponse()
                {
                    Packet = new Packet() {Status = "success"}
                });
            }
        }

        public async Task Consume(ConsumeContext<UploadPhotoRequest> context)
        {
            using (var dbContext = new DatabaseContext())
            {
                var form = context.Message.Form;
                var session = dbContext.Sessions.Find(context.Message.SessionId);

                Photo photo;
                FileUsage fileUsage;

                dbContext.Entry(session).Reference(s => s.BaseUser).Load();
                var user = (User) session.BaseUser;

                if (form.RoomId > 0)
                {
                    dbContext.Entry(user).Collection(u => u.Memberships).Load();
                    var membership = user.Memberships.Find(m => m.ComplexId == form.ComplexId);
                    if (membership == null)
                    {
                        await context.RespondAsync(new UploadPhotoResponse()
                        {
                            Packet = new Packet {Status = "error_1"}
                        });
                        return;
                    }

                    dbContext.Entry(membership).Reference(m => m.Complex).Load();
                    var complex = membership.Complex;
                    dbContext.Entry(complex).Collection(c => c.Rooms).Load();
                    var room = complex.Rooms.Find(r => r.RoomId == form.RoomId);
                    if (room == null)
                    {
                        await context.RespondAsync(new UploadPhotoResponse()
                        {
                            Packet = new Packet {Status = "error_2"}
                        });
                        return;
                    }

                    photo = new Photo()
                    {
                        Width = form.Width,
                        Height = form.Height,
                        IsPublic = false,
                        Uploader = user,
                        IsAvatar = form.IsAvatar
                    };
                    dbContext.Files.Add(photo);
                    fileUsage = new FileUsage()
                    {
                        File = photo,
                        Room = room
                    };
                    dbContext.FileUsages.Add(fileUsage);
                }
                else
                {
                    photo = new Photo()
                    {
                        Width = form.Width,
                        Height = form.Height,
                        IsPublic = true,
                        Uploader = user,
                        IsAvatar = form.IsAvatar
                    };
                    dbContext.Files.Add(photo);
                    fileUsage = null;
                }

                dbContext.SaveChanges();

                var filePath = DirPath + @"\" + photo.FileId;
                File.Create(filePath).Close();

                SharedArea.Transport.NotifyService<PhotoCreatedNotif, PhotoCreatedNotifResponse>(
                    Program.Bus,
                    new Packet() {Photo = photo, FileUsage = fileUsage, BaseUser = user},
                    new[]
                    {
                        SharedArea.GlobalVariables.MESSENGER_QUEUE_NAME
                    });

                await context.RespondAsync(new UploadPhotoResponse()
                {
                    Packet = new Packet {Status = "success", File = photo, FileUsage = fileUsage}
                });
            }
        }

        public async Task Consume(ConsumeContext<UploadAudioRequest> context)
        {
            using (var dbContext = new DatabaseContext())
            {
                var form = context.Message.Form;
                var session = dbContext.Sessions.Find(context.Message.SessionId);
                dbContext.Entry(session).Reference(s => s.BaseUser).Load();
                var user = (User) session.BaseUser;

                Audio audio;
                FileUsage fileUsage;

                if (form.RoomId > 0)
                {
                    dbContext.Entry(user).Collection(u => u.Memberships).Load();
                    var membership = user.Memberships.Find(m => m.ComplexId == form.ComplexId);
                    if (membership == null)
                    {
                        await context.RespondAsync(new UploadAudioResponse()
                        {
                            Packet = new Packet {Status = "error_1"}
                        });
                        return;
                    }

                    dbContext.Entry(membership).Reference(m => m.Complex).Load();
                    var complex = membership.Complex;
                    dbContext.Entry(complex).Collection(c => c.Rooms).Load();
                    var room = complex.Rooms.Find(r => r.RoomId == form.RoomId);
                    if (room == null)
                    {
                        await context.RespondAsync(new UploadAudioResponse()
                        {
                            Packet = new Packet {Status = "error_2"}
                        });
                        return;
                    }

                    audio = new Audio()
                    {
                        Title = form.Title,
                        Duration = form.Duration,
                        IsPublic = false,
                        Uploader = user,
                    };
                    dbContext.Files.Add(audio);
                    fileUsage = new FileUsage()
                    {
                        File = audio,
                        Room = room
                    };
                    dbContext.FileUsages.Add(fileUsage);
                }
                else
                {
                    audio = new Audio()
                    {
                        Title = form.Title,
                        Duration = form.Duration,
                        IsPublic = true,
                        Uploader = user
                    };
                    dbContext.Files.Add(audio);
                    fileUsage = null;
                }

                dbContext.SaveChanges();

                var filePath = DirPath + @"\" + audio.FileId;
                File.Create(filePath).Close();

                SharedArea.Transport.NotifyService<AudioCreatedNotif, AudioCreatedNotifResponse>(
                    Program.Bus,
                    new Packet() {Audio = audio, FileUsage = fileUsage, BaseUser = user},
                    new[]
                    {
                        SharedArea.GlobalVariables.MESSENGER_QUEUE_NAME
                    });

                await context.RespondAsync(new UploadAudioResponse()
                {
                    Packet = new Packet {Status = "success", File = audio, FileUsage = fileUsage}
                });
            }
        }

        public async Task Consume(ConsumeContext<UploadVideoRequest> context)
        {
            using (var dbContext = new DatabaseContext())
            {
                var form = context.Message.Form;
                var session = dbContext.Sessions.Find(context.Message.SessionId);
                dbContext.Entry(session).Reference(s => s.BaseUser).Load();
                var user = (User) session.BaseUser;

                Video video;
                FileUsage fileUsage;

                if (form.RoomId > 0)
                {
                    dbContext.Entry(user).Collection(u => u.Memberships).Load();
                    var membership = user.Memberships.Find(m => m.ComplexId == form.ComplexId);
                    if (membership == null)
                    {
                        await context.RespondAsync(new UploadVideoResponse()
                        {
                            Packet = new Packet {Status = "error_1"}
                        });
                        return;
                    }

                    dbContext.Entry(membership).Reference(m => m.Complex).Load();
                    var complex = membership.Complex;
                    dbContext.Entry(complex).Collection(c => c.Rooms).Load();
                    var room = complex.Rooms.Find(r => r.RoomId == form.RoomId);
                    if (room == null)
                    {
                        await context.RespondAsync(new UploadVideoResponse()
                        {
                            Packet = new Packet {Status = "error_2"}
                        });
                        return;
                    }

                    video = new Video()
                    {
                        Title = form.Title,
                        Duration = form.Duration,
                        IsPublic = false,
                        Uploader = user
                    };
                    dbContext.Files.Add(video);
                    fileUsage = new FileUsage()
                    {
                        File = video,
                        Room = room
                    };
                    dbContext.FileUsages.Add(fileUsage);
                }
                else
                {
                    video = new Video()
                    {
                        Title = form.Title,
                        Duration = form.Duration,
                        IsPublic = true,
                        Uploader = user
                    };
                    dbContext.Files.Add(video);
                    fileUsage = null;
                }

                dbContext.SaveChanges();

                var filePath = DirPath + @"\" + video.FileId;
                File.Create(filePath).Close();

                SharedArea.Transport.NotifyService<VideoCreatedNotif, VideoCreatedNotifResponse>(
                    Program.Bus,
                    new Packet() {Video = video, FileUsage = fileUsage, BaseUser = user},
                    new[]
                    {
                        SharedArea.GlobalVariables.MESSENGER_QUEUE_NAME
                    });

                await context.RespondAsync(new UploadVideoResponse()
                {
                    Packet = new Packet {Status = "success", File = video, FileUsage = fileUsage}
                });
            }
        }

        public async Task Consume(ConsumeContext<DownloadFileRequest> context)
        {
            var fileId = context.Message.FileId;
            var streamCode = context.Message.StreamCode;
            var offset = context.Message.Offset;
            using (var dbContext = new DatabaseContext())
            {
                var session = dbContext.Sessions.Find(context.Message.SessionId);
                if (session == null)
                {
                    await context.RespondAsync(new DownloadFileResponse()
                    {
                        Packet = new Packet {Status = "error_0"}
                    });
                    return;
                }

                var file = dbContext.Files.Find(fileId);
                if (file == null)
                {
                    await context.RespondAsync(new DownloadFileResponse()
                    {
                        Packet = new Packet {Status = "error_1"}
                    });
                    return;
                }

                dbContext.Entry(session).Reference(s => s.BaseUser).Load();
                var user = (User) session.BaseUser;
                dbContext.Entry(user).Collection(u => u.Memberships).Load();
                if (file.IsPublic)
                {
                    await UploadFileToApiGateWay(streamCode, file.FileId, offset);
                    await context.RespondAsync(new DownloadFileResponse()
                    {
                        Packet = new Packet {Status = "success"}
                    });
                    return;
                }

                dbContext.Entry(file).Collection(f => f.FileUsages).Query().Include(fu => fu.Room).Load();
                var foundPath = (from fu in file.FileUsages select fu.Room.ComplexId)
                    .Intersect(from mem in user.Memberships select mem.ComplexId).Any();
                if (foundPath)
                {
                    await UploadFileToApiGateWay(streamCode, file.FileId, offset);
                    await context.RespondAsync(new DownloadFileResponse()
                    {
                        Packet = new Packet {Status = "success"}
                    });
                }
                else
                {
                    await context.RespondAsync(new DownloadFileResponse()
                    {
                        Packet = new Packet {Status = "error_2"}
                    });
                }
            }
        }

        private static async Task UploadFileToApiGateWay(string streamCode, long fileId, long offset)
        {
            using (var client = new HttpClient())
            {
                using (var content = new MultipartFormDataContent())
                {
                    using (var stream =
                        File.Exists(DirPath + @"\" + fileId)
                            ? File.OpenRead(DirPath + @"\" + fileId)
                            : File.OpenRead(DirPath + @"\" + fileId + ".png"))
                    {
                        Console.WriteLine(offset);
                        stream.Seek(offset, SeekOrigin.Current);

                        content.Add(new StreamContent(stream), "File", "File");
                        content.Add(new StringContent(streamCode), "StreamCode");

                        using (var message = await client.PostAsync(
                            SharedArea.GlobalVariables.SERVER_URL +
                            SharedArea.GlobalVariables.FILE_TRANSFER_TAKE_DOWNLOAD_STREAM_URL, content))
                        {
                            await message.Content.ReadAsStringAsync();
                        }
                    }
                }
            }
        }

        public async Task Consume(ConsumeContext<DownloadBotAvatarRequest> context)
        {
            using (var dbContext = new DatabaseContext())
            {
                var botId = context.Message.BotId;
                var streamCode = context.Message.StreamCode;
                var bot = dbContext.Bots.Find(botId);
                var file = dbContext.Files.Find(bot.Avatar);
                if (file == null)
                {
                    await context.RespondAsync(new DownloadBotAvatarResponse()
                    {
                        Packet = new Packet() {Status = "error_0"}
                    });
                    return;
                }

                if (file.IsPublic)
                {
                    await UploadFileToApiGateWay(streamCode, file.FileId, 0);
                    await context.RespondAsync(new DownloadBotAvatarResponse()
                    {
                        Packet = new Packet() {Status = "success"}
                    });
                }
                else
                {
                    await context.RespondAsync(new DownloadBotAvatarResponse()
                    {
                        Packet = new Packet() {Status = "error_1"}
                    });
                }
            }
        }

        public async Task Consume(ConsumeContext<DownloadRoomAvatarRequest> context)
        {
            using (var dbContext = new DatabaseContext())
            {
                var complexId = context.Message.ComplexId;
                var roomId = context.Message.RoomId;
                var streamCode = context.Message.StreamCode;
                var session = dbContext.Sessions.Find(context.Message.SessionId);
                dbContext.Entry(session).Reference(s => s.BaseUser).Load();
                var user = (User) session.BaseUser;
                dbContext.Entry(user).Collection(u => u.Memberships).Load();
                var membership = user.Memberships.Find(mem => mem.ComplexId == complexId);
                if (membership == null)
                {
                    await context.RespondAsync(new DownloadRoomAvatarResponse()
                    {
                        Packet = new Packet {Status = "error_1"}
                    });
                    return;
                }

                dbContext.Entry(membership).Reference(mem => mem.Complex).Load();
                var complex = membership.Complex;
                dbContext.Entry(complex).Collection(c => c.Rooms);
                var room = complex.Rooms.Find(r => r.RoomId == roomId);
                var file = dbContext.Files.Find(room.Avatar);
                if (file == null)
                {
                    await context.RespondAsync(new DownloadRoomAvatarResponse()
                    {
                        Packet = new Packet() {Status = "error_0"}
                    });
                    return;
                }

                if (file.IsPublic)
                {
                    await UploadFileToApiGateWay(streamCode, file.FileId, 0);
                    await context.RespondAsync(new DownloadRoomAvatarResponse()
                    {
                        Packet = new Packet() {Status = "success"}
                    });
                }
                else
                {
                    await context.RespondAsync(new DownloadRoomAvatarResponse()
                    {
                        Packet = new Packet() {Status = "error_1"}
                    });
                }
            }
        }

        public async Task Consume(ConsumeContext<DownloadComplexAvatarRequest> context)
        {
            using (var dbContext = new DatabaseContext())
            {
                var complexId = context.Message.ComplexId;
                var streamCode = context.Message.StreamCode;
                var complex = dbContext.Complexes.Find(complexId);
                var file = dbContext.Files.Find(complex.Avatar);
                if (file == null)
                {
                    await context.RespondAsync(new DownloadComplexAvatarResponse()
                    {
                        Packet = new Packet() {Status = "error_0"}
                    });
                    return;
                }

                if (file.IsPublic)
                {
                    await UploadFileToApiGateWay(streamCode, file.FileId, 0);
                    await context.RespondAsync(new DownloadComplexAvatarResponse()
                    {
                        Packet = new Packet() {Status = "success"}
                    });
                }
                else
                {
                    await context.RespondAsync(new DownloadComplexAvatarResponse()
                    {
                        Packet = new Packet() {Status = "error_1"}
                    });
                }
            }
        }

        public async Task Consume(ConsumeContext<DownloadUserAvatarRequest> context)
        {
            using (var dbContext = new DatabaseContext())
            {
                var userId = context.Message.UserId;
                var streamCode = context.Message.StreamCode;
                var user = (User) dbContext.BaseUsers.Find(userId);
                var file = dbContext.Files.Find(user.Avatar);
                if (file == null)
                {
                    await context.RespondAsync(new DownloadUserAvatarResponse()
                    {
                        Packet = new Packet() {Status = "error_0"}
                    });
                    return;
                }

                if (file.IsPublic)
                {
                    await UploadFileToApiGateWay(streamCode, file.FileId, 0);
                    await context.RespondAsync(new DownloadUserAvatarResponse()
                    {
                        Packet = new Packet() {Status = "success"}
                    });
                }
                else
                {
                    await context.RespondAsync(new DownloadUserAvatarResponse()
                    {
                        Packet = new Packet() {Status = "error_1"}
                    });
                }
            }
        }

        public async Task Consume(ConsumeContext<ConsolidateDeleteAccountRequest> context)
        {
            var gUser = context.Message.Packet.User;

            using (var dbContext = new DatabaseContext())
            {
                var user = (User) dbContext.BaseUsers.Find(gUser.BaseUserId);

                if (user != null)
                {
                    dbContext.Entry(user).Collection(u => u.Sessions).Load();
                    dbContext.Entry(user).Reference(u => u.UserSecret).Load();

                    user.Title = "Deleted User";
                    user.Avatar = -1;
                    user.UserSecret.Email = "";
                    dbContext.Sessions.RemoveRange(user.Sessions);

                    dbContext.SaveChanges();
                }
            }

            await context.RespondAsync(new ConsolidateDeleteAccountResponse());
        }
    }
}