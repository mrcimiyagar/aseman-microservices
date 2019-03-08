using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ApiGateway.DbContexts;
using ApiGateway.Models.Forms;
using ApiGateway.Utils;
using MassTransit;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using SharedArea.Forms;
using SharedArea.Middles;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Server.Kestrel.Core.Adapter.Internal;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Net.Http.Headers;
using MongoDB.Bson.IO;
using SharedArea.Commands.File;
using SharedArea.Utils;
using File = SharedArea.Entities.File;

namespace ApiGateway.Controllers
{
    [Produces("application/json")]
    [Route("api/[controller]")]
    [ApiController]
    public class FileController : Controller
    {
        private static readonly FormOptions DefaultFormOptions = new FormOptions();

        [Route("~/api/file/write_to_file")]
        [RequestSizeLimit(bytes: 4294967296)]
        [HttpPost]
        public async Task<ActionResult<Packet>> WriteToFile()
        {
            try
            {
                Console.WriteLine("Hello 0");
                if (!MultipartRequestHelper.IsMultipartContentType(Request.ContentType))
                {
                    return new Packet {Status = "error_0"};
                }
                Console.WriteLine("Hello 1");

                using (var dbContext = new DatabaseContext())
                {
                    Console.WriteLine("Hello 2");

                    var session = Security.Authenticate(dbContext, Request.Headers[AuthExtracter.AK]);
                    if (session == null) return new Packet() {Status = "error_1"};
                    Console.WriteLine("Hello 3");

                    var formParts = new Dictionary<string, string>();

                    var boundary = MultipartRequestHelper.GetBoundary(
                        MediaTypeHeaderValue.Parse(Request.ContentType),
                        DefaultFormOptions.MultipartBoundaryLengthLimit);
                    var reader = new MultipartReader(boundary, HttpContext.Request.Body);
                    Console.WriteLine("Hello 4");

                    var section = await reader.ReadNextSectionAsync();
                    while (section != null)
                    {
                        Console.WriteLine("Hello 5");

                        var hasContentDispositionHeader =
                            ContentDispositionHeaderValue.TryParse(section.ContentDisposition,
                                out var contentDisposition);

                        if (hasContentDispositionHeader)
                        {
                            Console.WriteLine("Hello 6");

                            if (MultipartRequestHelper.HasFileContentDisposition(contentDisposition))
                            {
                                Console.WriteLine("Hello 7");

                                var guid = Guid.NewGuid().ToString();

                                StreamRepo.FileStreams.Add(guid, section.Body);

                                var address = new Uri(SharedArea.GlobalVariables.RABBITMQ_SERVER_URL + "/" +
                                                      SharedArea.GlobalVariables.FILE_QUEUE_NAME +
                                                      SharedArea.GlobalVariables.RABBITMQ_SERVER_URL_EXTENSIONS);
                                Console.WriteLine("Hello 8");

                                var requestTimeout = TimeSpan.FromDays(35);
                                IRequestClient<WriteToFileRequest, WriteToFileResponse> client =
                                    new MessageRequestClient<WriteToFileRequest, WriteToFileResponse>(
                                        Program.Bus, address, requestTimeout);
                                Console.WriteLine("Hello 9");

                                var result = await client.Request<WriteToFileRequest, WriteToFileResponse>(new
                                {
                                    Headers = Request.Headers.ToDictionary(a => a.Key, a => a.Value.ToString()),
                                    SessionId = session.SessionId,
                                    StreamCode = guid,
                                    Packet = new Packet()
                                    {
                                        File = new File() {FileId = Convert.ToInt64(formParts["FileId"])}
                                    }
                                });
                                Console.WriteLine("Hello 10");

                                StreamRepo.FileStreams.Remove(guid);
                                
                                Console.WriteLine("Hello 11");

                                return result.Packet;
                            }
                            else if (MultipartRequestHelper.HasFormDataContentDisposition(contentDisposition))
                            {
                                Console.WriteLine("Hello 12");

                                var key = HeaderUtilities.RemoveQuotes(contentDisposition.Name);
                                var encoding = GetEncoding(section);
                                using (var streamReader = new StreamReader(
                                    section.Body,
                                    encoding,
                                    detectEncodingFromByteOrderMarks: true,
                                    bufferSize: 1024,
                                    leaveOpen: true))
                                {
                                    Console.WriteLine("Hello 13");

                                    var value = await streamReader.ReadToEndAsync();

                                    formParts[key.ToString()] = value;

                                    if (formParts.Count > DefaultFormOptions.ValueCountLimit)
                                    {
                                        throw new InvalidDataException(
                                            $"Form key count limit {DefaultFormOptions.ValueCountLimit} exceeded.");
                                    }
                                    Console.WriteLine("Hello 14");

                                }
                            }
                        }
                        Console.WriteLine("Hello 15");

                        section = await reader.ReadNextSectionAsync();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
            Console.WriteLine("Hello 16");

            return new Packet() {Status = "error_2"};
        }
        
        private static Encoding GetEncoding(MultipartSection section)
        {
            var hasMediaTypeHeader = MediaTypeHeaderValue.TryParse(section.ContentType, out var mediaType);
            if (!hasMediaTypeHeader || Encoding.UTF7.Equals(mediaType.Encoding))
            {
                return Encoding.UTF8;
            }
            return mediaType.Encoding;
        }

        [Route("~/api/file/get_file_size")]
        [HttpPost]
        public async Task<ActionResult<Packet>> GetFileSize([FromBody] Packet packet)
        {
            using (var dbContext = new DatabaseContext())
            {
                var session = Security.Authenticate(dbContext, Request.Headers[AuthExtracter.AK]);
                if (session == null) return new Packet() {Status = "error_0"};

                var result = await SharedArea.Transport.DirectService<GetFileSizeRequest, GetFileSizeResponse>(
                    Program.Bus,
                    SharedArea.GlobalVariables.FILE_QUEUE_NAME,
                    session.SessionId,
                    Request.Headers.ToDictionary(a => a.Key, a => a.Value.ToString()),
                    packet);

                return result.Packet;
            }
        }

        [Route("~/api/file/get_file_upload_stream")]
        [HttpPost]
        public ActionResult GetFileUploadStream([FromBody] Packet packet)
        {
            Console.WriteLine("Keyhan 0");
            if (packet.Username == SharedArea.GlobalVariables.FILE_TRANSFER_USERNAME
                && packet.Password == SharedArea.GlobalVariables.FILE_TRANSFER_PASSWORD)
            {
                return File(StreamRepo.FileStreams[packet.StreamCode], "application/octet-stream");
            }
            else
            {
                return NotFound();
            }
        }

        [Route("~/api/file/upload_photo")]
        [HttpPost]
        public async Task<ActionResult<Packet>> UploadPhoto([FromForm] PhotoUploadForm form)
        {
            using (var context = new DatabaseContext())
            {
                var session = Security.Authenticate(context, Request.Headers[AuthExtracter.AK]);
                if (session == null) return new Packet {Status = "error_0"};

                var address = new Uri(SharedArea.GlobalVariables.RABBITMQ_SERVER_URL + "/" +
                                      SharedArea.GlobalVariables.FILE_QUEUE_NAME +
                                      SharedArea.GlobalVariables.RABBITMQ_SERVER_URL_EXTENSIONS);
                var requestTimeout = TimeSpan.FromSeconds(SharedArea.GlobalVariables.RABBITMQ_REQUEST_TIMEOUT);
                IRequestClient<UploadPhotoRequest, UploadPhotoResponse> client =
                    new MessageRequestClient<UploadPhotoRequest, UploadPhotoResponse>(
                        Program.Bus, address, requestTimeout);

                var puf = new PhotoUF()
                {
                    ComplexId = form.ComplexId,
                    RoomId = form.RoomId,
                    Width = form.Width,
                    Height = form.Height,
                    IsAvatar = form.IsAvatar
                };

                var result = await client.Request<UploadPhotoRequest, UploadPhotoResponse>(new
                {
                    Headers = Request.Headers.ToDictionary(a => a.Key, a => a.Value.ToString()),
                    Form = puf,
                    SessionId = session.SessionId
                });

                return result.Packet;
            }
        }

        [Route("~/api/file/upload_audio")]
        [HttpPost]
        public async Task<ActionResult<Packet>> UploadAudio([FromForm] AudioUploadForm form)
        {
            using (var context = new DatabaseContext())
            {
                var session = Security.Authenticate(context, Request.Headers[AuthExtracter.AK]);
                if (session == null) return new Packet {Status = "error_0"};

                var address = new Uri(SharedArea.GlobalVariables.RABBITMQ_SERVER_URL + "/" +
                                      SharedArea.GlobalVariables.FILE_QUEUE_NAME +
                                      SharedArea.GlobalVariables.RABBITMQ_SERVER_URL_EXTENSIONS);
                var requestTimeout = TimeSpan.FromSeconds(SharedArea.GlobalVariables.RABBITMQ_REQUEST_TIMEOUT);
                IRequestClient<UploadAudioRequest, UploadAudioResponse> client =
                    new MessageRequestClient<UploadAudioRequest, UploadAudioResponse>(
                        Program.Bus, address, requestTimeout);
                var auf = new AudioUF()
                {
                    ComplexId = form.ComplexId,
                    RoomId = form.RoomId,
                    Title = form.Title,
                    Duration = form.Duration
                };
                var result = await client.Request(new
                {
                    Headers = Request.Headers.ToDictionary(a => a.Key, a => a.Value.ToString()),
                    Form = auf,
                    SessionId = session.SessionId
                });

                return result.Packet;
            }
        }

        [Route("~/api/file/upload_video")]
        [HttpPost]
        public async Task<ActionResult<Packet>> UploadVideo([FromForm] VideoUploadForm form)
        {
            using (var context = new DatabaseContext())
            {
                var session = Security.Authenticate(context, Request.Headers[AuthExtracter.AK]);
                if (session == null) return new Packet {Status = "error_0"};

                var address = new Uri(SharedArea.GlobalVariables.RABBITMQ_SERVER_URL + "/" +
                                      SharedArea.GlobalVariables.FILE_QUEUE_NAME +
                                      SharedArea.GlobalVariables.RABBITMQ_SERVER_URL_EXTENSIONS);
                var requestTimeout = TimeSpan.FromSeconds(SharedArea.GlobalVariables.RABBITMQ_REQUEST_TIMEOUT);
                IRequestClient<UploadVideoRequest, UploadVideoResponse> client =
                    new MessageRequestClient<UploadVideoRequest, UploadVideoResponse>(
                        Program.Bus, address, requestTimeout);
                var vuf = new VideoUF()
                {
                    ComplexId = form.ComplexId,
                    RoomId = form.RoomId,
                    Title = form.Title,
                    Duration = form.Duration
                };
                var result = await client.Request(new
                {
                    Headers = Request.Headers.ToDictionary(a => a.Key, a => a.Value.ToString()),
                    Form = vuf,
                    SessionId = session.SessionId
                });

                return result.Packet;
            }
        }

        [Route("~/api/file/download_bot_avatar")]
        [HttpGet]
        public ActionResult DownloadBotAvatar(long botId)
        {
            var guid = Guid.NewGuid().ToString();

            var address = new Uri(SharedArea.GlobalVariables.RABBITMQ_SERVER_URL + "/" +
                                  SharedArea.GlobalVariables.FILE_QUEUE_NAME +
                                  SharedArea.GlobalVariables.RABBITMQ_SERVER_URL_EXTENSIONS);
            var requestTimeout = TimeSpan.FromSeconds(SharedArea.GlobalVariables.RABBITMQ_REQUEST_TIMEOUT);
            IRequestClient<DownloadBotAvatarRequest, DownloadBotAvatarResponse> client =
                new MessageRequestClient<DownloadBotAvatarRequest, DownloadBotAvatarResponse>(
                    Program.Bus, address, requestTimeout);

            var lockObj = new object();

            StreamRepo.FileStreamLocks.Add(guid, lockObj);

            lock (lockObj)
            {
                client.Request(new
                {
                    Headers = Request.Headers.ToDictionary(a => a.Key, a => a.Value.ToString()),
                    BotId = botId,
                    StreamCode = guid
                });

                Monitor.Wait(lockObj);
            }

            var stream = StreamRepo.FileStreams[guid];

            Response.OnCompleted(() =>
            {
                StreamRepo.FileStreams.Remove(guid);
                StreamRepo.FileStreamLocks.Remove(guid);

                lock (lockObj)
                {
                    Monitor.Pulse(lockObj);
                }

                return Task.CompletedTask;
            });

            return File(stream, "application/octet-stream");
        }

        [Route("~/api/file/download_room_avatar")]
        [HttpGet]
        public ActionResult DownloadRoomAvatar(long complexId, long roomId)
        {
            using (var dbContext = new DatabaseContext())
            {
                var session = Security.Authenticate(dbContext, Request.Headers[AuthExtracter.AK]);
                if (session == null) return NotFound();

                var guid = Guid.NewGuid().ToString();

                var address = new Uri(SharedArea.GlobalVariables.RABBITMQ_SERVER_URL + "/" +
                                      SharedArea.GlobalVariables.FILE_QUEUE_NAME +
                                      SharedArea.GlobalVariables.RABBITMQ_SERVER_URL_EXTENSIONS);
                var requestTimeout = TimeSpan.FromSeconds(SharedArea.GlobalVariables.RABBITMQ_REQUEST_TIMEOUT);
                IRequestClient<DownloadRoomAvatarRequest, DownloadRoomAvatarResponse> client =
                    new MessageRequestClient<DownloadRoomAvatarRequest, DownloadRoomAvatarResponse>(
                        Program.Bus, address, requestTimeout);

                var lockObj = new object();

                StreamRepo.FileStreamLocks.Add(guid, lockObj);

                lock (lockObj)
                {
                    client.Request(new
                    {
                        Headers = Request.Headers.ToDictionary(a => a.Key, a => a.Value.ToString()),
                        ComplexId = complexId,
                        RoomId = roomId,
                        StreamCode = guid,
                        SessionId = session.SessionId
                    });

                    Monitor.Wait(lockObj);
                }

                var stream = StreamRepo.FileStreams[guid];

                Response.OnCompleted(() =>
                {
                    StreamRepo.FileStreams.Remove(guid);
                    StreamRepo.FileStreamLocks.Remove(guid);

                    lock (lockObj)
                    {
                        Monitor.Pulse(lockObj);
                    }

                    return Task.CompletedTask;
                });

                return File(stream, "application/octet-stream");
            }
        }

        [Route("~/api/file/download_complex_avatar")]
        [HttpGet]
        public ActionResult DownloadComplexAvatar(long complexId)
        {
            var guid = Guid.NewGuid().ToString();

            var address = new Uri(SharedArea.GlobalVariables.RABBITMQ_SERVER_URL + "/" +
                                  SharedArea.GlobalVariables.FILE_QUEUE_NAME +
                                  SharedArea.GlobalVariables.RABBITMQ_SERVER_URL_EXTENSIONS);
            var requestTimeout = TimeSpan.FromSeconds(SharedArea.GlobalVariables.RABBITMQ_REQUEST_TIMEOUT);
            IRequestClient<DownloadComplexAvatarRequest, DownloadComplexAvatarResponse> client =
                new MessageRequestClient<DownloadComplexAvatarRequest, DownloadComplexAvatarResponse>(
                    Program.Bus, address, requestTimeout);

            var lockObj = new object();

            StreamRepo.FileStreamLocks.Add(guid, lockObj);

            lock (lockObj)
            {
                client.Request(new
                {
                    Headers = Request.Headers.ToDictionary(a => a.Key, a => a.Value.ToString()),
                    ComplexId = complexId,
                    StreamCode = guid
                });

                Monitor.Wait(lockObj);
            }

            var stream = StreamRepo.FileStreams[guid];

            Response.OnCompleted(() =>
            {
                StreamRepo.FileStreams.Remove(guid);
                StreamRepo.FileStreamLocks.Remove(guid);

                lock (lockObj)
                {
                    Monitor.Pulse(lockObj);
                }

                return Task.CompletedTask;
            });

            return File(stream, "application/octet-stream");
        }

        [Route("~/api/file/download_user_avatar")]
        [HttpGet]
        public ActionResult DownloadUserAvatar(long userId)
        {
            var guid = Guid.NewGuid().ToString();

            var address = new Uri(SharedArea.GlobalVariables.RABBITMQ_SERVER_URL + "/" +
                                  SharedArea.GlobalVariables.FILE_QUEUE_NAME +
                                  SharedArea.GlobalVariables.RABBITMQ_SERVER_URL_EXTENSIONS);
            var requestTimeout = TimeSpan.FromSeconds(SharedArea.GlobalVariables.RABBITMQ_REQUEST_TIMEOUT);
            IRequestClient<DownloadUserAvatarRequest, DownloadUserAvatarResponse> client =
                new MessageRequestClient<DownloadUserAvatarRequest, DownloadUserAvatarResponse>(
                    Program.Bus, address, requestTimeout);

            var lockObj = new object();

            StreamRepo.FileStreamLocks.Add(guid, lockObj);

            lock (lockObj)
            {
                client.Request(new
                {
                    Headers = Request.Headers.ToDictionary(a => a.Key, a => a.Value.ToString()),
                    StreamCode = guid,
                    UserId = userId
                });

                Monitor.Wait(lockObj);
            }

            var stream = StreamRepo.FileStreams[guid];

            Response.OnCompleted(() =>
            {
                StreamRepo.FileStreams.Remove(guid);
                StreamRepo.FileStreamLocks.Remove(guid);

                lock (lockObj)
                {
                    Monitor.Pulse(lockObj);
                }

                return Task.CompletedTask;
            });

            return File(stream, "application/octet-stream");
        }

        [Route("~/api/file/take_file_download_stream")]
        [RequestSizeLimit(bytes: 4294967296)]
        [HttpPost]
        public ActionResult TakeFileDownloadStream([FromForm] TakeFileDSF form)
        {
            var file = form.File;
            var streamCode = form.StreamCode;

            StreamRepo.FileStreams.Add(streamCode, file.OpenReadStream());

            var lockObj = StreamRepo.FileStreamLocks[streamCode];

            lock (lockObj)
            {
                Monitor.Pulse(lockObj);
            }

            lock (lockObj)
            {
                Monitor.Wait(lockObj);
            }

            return Ok();
        }

        [Route("~/api/file/download_file")]
        [HttpGet]
        public ActionResult DownloadFile(long fileId, long offset)
        {
            using (var context = new DatabaseContext())
            {
                var session = Security.Authenticate(context, Request.Headers[AuthExtracter.AK]);
                if (session == null) return NotFound();

                var guid = Guid.NewGuid().ToString();

                var address = new Uri(SharedArea.GlobalVariables.RABBITMQ_SERVER_URL + "/" +
                                      SharedArea.GlobalVariables.FILE_QUEUE_NAME +
                                      SharedArea.GlobalVariables.RABBITMQ_SERVER_URL_EXTENSIONS);
                var requestTimeout = TimeSpan.FromSeconds(SharedArea.GlobalVariables.RABBITMQ_REQUEST_TIMEOUT);
                IRequestClient<DownloadFileRequest, DownloadFileResponse> client =
                    new MessageRequestClient<DownloadFileRequest, DownloadFileResponse>(
                        Program.Bus, address, requestTimeout);

                var lockObj = new object();

                StreamRepo.FileStreamLocks.Add(guid, lockObj);

                lock (lockObj)
                {
                    client.Request(new
                    {
                        Headers = Request.Headers.ToDictionary(a => a.Key, a => a.Value.ToString()),
                        SessionId = session.SessionId,
                        StreamCode = guid,
                        Offset = offset,
                        FileId = fileId
                    });

                    Monitor.Wait(lockObj);
                }

                var stream = StreamRepo.FileStreams[guid];

                Response.OnCompleted(() =>
                {
                    StreamRepo.FileStreams.Remove(guid);
                    StreamRepo.FileStreamLocks.Remove(guid);

                    lock (lockObj)
                    {
                        Monitor.Pulse(lockObj);
                    }

                    return Task.CompletedTask;
                });

                Response.ContentLength = stream.Length;

                return File(stream, "application/octet-stream");
            }
        }
    }
}