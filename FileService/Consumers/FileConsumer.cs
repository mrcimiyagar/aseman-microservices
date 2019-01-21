using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using FileService.DbContexts;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using SharedArea.Commands.File;
using SharedArea.Consumers;
using SharedArea.Entities;
using SharedArea.Middles;
using SharedArea.Utils;

namespace FileService.Consumers
{
    public class FileConsumer : NotifConsumer, IConsumer<UploadPhotoRequest>, IConsumer<UploadAudioRequest>
        , IConsumer<UploadVideoRequest>, IConsumer<DownloadFileRequest>
    {
        public const string DirPath = @"C:\\Aseman\Files";
        
        public async Task Consume(ConsumeContext<UploadPhotoRequest> context)
        {
            using (var dbContext = new DatabaseContext())
            {
                var form = context.Message.Form;
                var session = dbContext.Sessions.Find(context.Message.SessionId);
                var streamCode = context.Message.StreamCode;
                
                Photo photo;
                FileUsage fileUsage;

                if (form.RoomId > 0)
                {
                    dbContext.Entry(session).Reference(s => s.BaseUser).Load();
                    var user = (User) session.BaseUser;
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
                        IsPublic = false
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
                        IsPublic = true
                    };
                    dbContext.Files.Add(photo);
                    fileUsage = null;
                }

                dbContext.SaveChanges();
                
                var myContent = JsonConvert.SerializeObject(new Packet()
                {
                    Username = SharedArea.GlobalVariables.FILE_TRANSFER_USERNAME,
                    Password = SharedArea.GlobalVariables.FILE_TRANSFER_PASSWORD,
                    StreamCode = streamCode
                });
                var buffer = System.Text.Encoding.UTF8.GetBytes(myContent);
                var byteContent = new ByteArrayContent(buffer);
                byteContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");

                using (var client = new HttpClient())
                {
                    client.BaseAddress = new Uri(SharedArea.GlobalVariables.SERVER_URL);
                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers
                        .MediaTypeWithQualityHeaderValue("application/octet-stream"));
                    
                    var response = await client.PostAsync(
                        SharedArea.GlobalVariables.FILE_TRANSFER_GET_UPLOAD_STREAM_URL,
                        byteContent);
                    if (response.IsSuccessStatusCode)
                    {
                        var content = response.Content;
                        var contentStream = await content.ReadAsStreamAsync();
                        Directory.CreateDirectory(DirPath);
                        var filePath = DirPath + @"\" + photo.FileId;
                        System.IO.File.Create(filePath).Close();
                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            contentStream.CopyTo(stream);
                        }
                    }
                }

                using (var client = new HttpClient())
                {
                    client.BaseAddress = new Uri(SharedArea.GlobalVariables.SERVER_URL);
                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers
                        .MediaTypeWithQualityHeaderValue("application/json"));
                    
                    await client.PostAsync(
                        SharedArea.GlobalVariables.FILE_TRANSFER_NOTIFY_GET_UPLOAD_STREAM_FINISHED_URL,
                        byteContent);
                }

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
                var streamCode = context.Message.StreamCode;
                
                Audio audio;
                FileUsage fileUsage;
                
                if (form.RoomId > 0)
                {
                    dbContext.Entry(session).Reference(s => s.BaseUser).Load();
                    var user = (User) session.BaseUser;
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
                        IsPublic = false
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
                        IsPublic = true
                    };
                    dbContext.Files.Add(audio);
                    fileUsage = null;
                }
                dbContext.SaveChanges();
                
                var myContent = JsonConvert.SerializeObject(new Packet()
                {
                    Username = SharedArea.GlobalVariables.FILE_TRANSFER_USERNAME,
                    Password = SharedArea.GlobalVariables.FILE_TRANSFER_PASSWORD,
                    StreamCode = streamCode
                });
                var buffer = System.Text.Encoding.UTF8.GetBytes(myContent);
                var byteContent = new ByteArrayContent(buffer);
                byteContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");

                using (var client = new HttpClient())
                {
                    client.BaseAddress = new Uri(SharedArea.GlobalVariables.SERVER_URL);
                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers
                        .MediaTypeWithQualityHeaderValue("application/octet-stream"));
                    
                    var response = await client.PostAsync(
                        SharedArea.GlobalVariables.FILE_TRANSFER_GET_UPLOAD_STREAM_URL,
                        byteContent);
                    if (response.IsSuccessStatusCode)
                    {
                        var content = response.Content;
                        var contentStream = await content.ReadAsStreamAsync();
                        Directory.CreateDirectory(DirPath);
                        var filePath = DirPath + @"\" + audio.FileId;
                        System.IO.File.Create(filePath).Close();
                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            contentStream.CopyTo(stream);
                        }
                    }
                }

                using (var client = new HttpClient())
                {
                    client.BaseAddress = new Uri(SharedArea.GlobalVariables.SERVER_URL);
                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers
                        .MediaTypeWithQualityHeaderValue("application/json"));
                    
                    await client.PostAsync(
                        SharedArea.GlobalVariables.FILE_TRANSFER_NOTIFY_GET_UPLOAD_STREAM_FINISHED_URL,
                        byteContent);
                }

                await context.RespondAsync(new UploadPhotoResponse()
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
                var streamCode = context.Message.StreamCode;
                
                Video video;
                FileUsage fileUsage;
                
                if (form.RoomId > 0)
                {
                    dbContext.Entry(session).Reference(s => s.BaseUser).Load();
                    var user = (User) session.BaseUser;
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
                        IsPublic = false
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
                        IsPublic = true
                    };
                    dbContext.Files.Add(video);
                    fileUsage = null;
                }
                dbContext.SaveChanges();
                
                var myContent = JsonConvert.SerializeObject(new Packet()
                {
                    Username = SharedArea.GlobalVariables.FILE_TRANSFER_USERNAME,
                    Password = SharedArea.GlobalVariables.FILE_TRANSFER_PASSWORD,
                    StreamCode = streamCode
                });
                var buffer = System.Text.Encoding.UTF8.GetBytes(myContent);
                var byteContent = new ByteArrayContent(buffer);
                byteContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");

                using (var client = new HttpClient())
                {
                    client.BaseAddress = new Uri(SharedArea.GlobalVariables.SERVER_URL);
                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers
                        .MediaTypeWithQualityHeaderValue("application/octet-stream"));
                    
                    var response = await client.PostAsync(
                        SharedArea.GlobalVariables.FILE_TRANSFER_GET_UPLOAD_STREAM_URL,
                        byteContent);
                    if (response.IsSuccessStatusCode)
                    {
                        var content = response.Content;
                        var contentStream = await content.ReadAsStreamAsync();
                        Directory.CreateDirectory(DirPath);
                        var filePath = DirPath + @"\" + video.FileId;
                        System.IO.File.Create(filePath).Close();
                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            contentStream.CopyTo(stream);
                        }
                    }
                }

                using (var client = new HttpClient())
                {
                    client.BaseAddress = new Uri(SharedArea.GlobalVariables.SERVER_URL);
                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers
                        .MediaTypeWithQualityHeaderValue("application/json"));
                    
                    await client.PostAsync(
                        SharedArea.GlobalVariables.FILE_TRANSFER_NOTIFY_GET_UPLOAD_STREAM_FINISHED_URL,
                        byteContent);
                }

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
            
            if (fileId == 0)
            {
                var stream = System.IO.File.OpenRead(DirPath + @"\" + 0);
                UploadFileToApiGateWay(streamCode, stream);
                await context.RespondAsync(new DownloadFileResponse()
                {
                    Packet = new Packet {Status = "success"}
                });
                return;
            }
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
                    var stream = System.IO.File.OpenRead(DirPath + @"\" + file.FileId);
                    UploadFileToApiGateWay(streamCode, stream);
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
                    var stream = System.IO.File.OpenRead(DirPath + @"\" + file.FileId);
                    UploadFileToApiGateWay(streamCode, stream);
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

        private async void UploadFileToApiGateWay(string streamCode, Stream stream)
        {
            using (var client = new HttpClient())
            {
                using (var content = new MultipartFormDataContent())
                {
                    content.Add(new StreamContent(stream), "File");
                    content.Add(new StringContent(streamCode), "StreamCode");

                    using (var message = await client.PostAsync(SharedArea.GlobalVariables.SERVER_URL + SharedArea.GlobalVariables.FILE_TRANSFER_TAKE_DOWNLOAD_STREAM_URL, content))
                    {
                        await message.Content.ReadAsStringAsync();
                    }
                }
            }
        }
    }
}