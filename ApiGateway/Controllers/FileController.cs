using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ApiGateway.DbContexts;
using ApiGateway.Models.Forms;
using ApiGateway.Utils;
using MassTransit;
using SharedArea.Forms;
using SharedArea.Middles;
using Microsoft.AspNetCore.Mvc;
using SharedArea.Commands.File;
using SharedArea.Entities;
using SharedArea.Utils;

namespace ApiGateway.Controllers
{
    [Produces("application/json")]
    [Route("api/[controller]")]
    [ApiController]
    public class FileController : Controller
    {
        public const string DirPath = @"C:\\Aseman\Files";

        [Route("~/api/file/write_to_file")]
        [HttpPost]
        public async Task<ActionResult<Packet>> WriteToFile([FromForm] WriteToFileForm form)
        {
            if (form.File == null || form.File.Length == 0) return new Packet {Status = "error_0"};

            using (var dbContext = new DatabaseContext())
            {
                var session = Security.Authenticate(dbContext, Request.Headers[AuthExtracter.AK]);
                if (session == null) return new Packet() {Status = "error_1"};
                
                var guid = Guid.NewGuid().ToString();

                StreamRepo.FileStreams.Add(guid, form.File);
                
                var address = new Uri(SharedArea.GlobalVariables.RABBITMQ_SERVER_URL + "/" +
                                      SharedArea.GlobalVariables.FILE_QUEUE_NAME + "?autodelete=true&durable=false&temporary=true");
                var requestTimeout = TimeSpan.FromSeconds(SharedArea.GlobalVariables.RABBITMQ_REQUEST_TIMEOUT);
                IRequestClient<WriteToFileRequest, WriteToFileResponse> client =
                    new MessageRequestClient<WriteToFileRequest, WriteToFileResponse>(
                        Program.Bus, address, requestTimeout);
                
                var result = await client.Request<WriteToFileRequest, WriteToFileResponse>(new
                {
                    Headers = Request.Headers.ToDictionary(a => a.Key, a => a.Value.ToString()),
                    SessionId = session.SessionId,
                    StreamCode = guid,
                    Packet = new Packet() {File = new File() {FileId = form.FileId}}
                });

                StreamRepo.FileStreams.Remove(guid);

                return result.Packet;
            }
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
            if (packet.Username == SharedArea.GlobalVariables.FILE_TRANSFER_USERNAME
                && packet.Password == SharedArea.GlobalVariables.FILE_TRANSFER_PASSWORD)
            {
                return File(StreamRepo.FileStreams[packet.StreamCode].OpenReadStream()
                    , "application/octet-stream");
            }
            else
            {
                return NotFound();
            }
        }

        [Route("~/api/file/upload_photo")]
        [RequestSizeLimit(1024 * 1024 * 1024)]
        [HttpPost]
        public async Task<ActionResult<Packet>> UploadPhoto([FromForm] PhotoUploadForm form)
        {
            using (var context = new DatabaseContext())
            {
                var session = Security.Authenticate(context, Request.Headers[AuthExtracter.AK]);
                if (session == null) return new Packet {Status = "error_0"};

                var address = new Uri(SharedArea.GlobalVariables.RABBITMQ_SERVER_URL + "/" +
                                      SharedArea.GlobalVariables.FILE_QUEUE_NAME + "?autodelete=true&durable=false&temporary=true");
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
        [RequestSizeLimit(1024 * 1024 * 1024)]
        [HttpPost]
        public async Task<ActionResult<Packet>> UploadAudio([FromForm] AudioUploadForm form)
        {
            using (var context = new DatabaseContext())
            {
                var session = Security.Authenticate(context, Request.Headers[AuthExtracter.AK]);
                if (session == null) return new Packet {Status = "error_0"};

                var address = new Uri(SharedArea.GlobalVariables.RABBITMQ_SERVER_URL + "/" +
                                      SharedArea.GlobalVariables.FILE_QUEUE_NAME + "?autodelete=true&durable=false&temporary=true");
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
        [RequestSizeLimit(1024 * 1024 * 1024)]
        [HttpPost]
        public async Task<ActionResult<Packet>> UploadVideo([FromForm] VideoUploadForm form)
        {
            
            using (var context = new DatabaseContext())
            {
                var session = Security.Authenticate(context, Request.Headers[AuthExtracter.AK]);
                if (session == null) return new Packet {Status = "error_0"};

                var address = new Uri(SharedArea.GlobalVariables.RABBITMQ_SERVER_URL + "/" +
                                      SharedArea.GlobalVariables.FILE_QUEUE_NAME + "?autodelete=true&durable=false&temporary=true");
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
                                  SharedArea.GlobalVariables.FILE_QUEUE_NAME + "?autodelete=true&durable=false&temporary=true");
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
                
            return File(stream.OpenReadStream(), "application/octet-stream");
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
                                      SharedArea.GlobalVariables.FILE_QUEUE_NAME + "?autodelete=true&durable=false&temporary=true");
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
                
                return File(stream.OpenReadStream(), "application/octet-stream");
            }
        }

        [Route("~/api/file/download_complex_avatar")]
        [HttpGet]
        public ActionResult DownloadComplexAvatar(long complexId)
        {
            var guid = Guid.NewGuid().ToString();

            var address = new Uri(SharedArea.GlobalVariables.RABBITMQ_SERVER_URL + "/" +
                                  SharedArea.GlobalVariables.FILE_QUEUE_NAME + "?autodelete=true&durable=false&temporary=true");
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
                
            return File(stream.OpenReadStream(), "application/octet-stream");
        }

        [Route("~/api/file/download_user_avatar")]
        [HttpGet]
        public ActionResult DownloadUserAvatar(long userId)
        {
            var guid = Guid.NewGuid().ToString();

            var address = new Uri(SharedArea.GlobalVariables.RABBITMQ_SERVER_URL + "/" +
                                  SharedArea.GlobalVariables.FILE_QUEUE_NAME + "?autodelete=true&durable=false&temporary=true");
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
                
            return File(stream.OpenReadStream(), "application/octet-stream");
        }

        [Route("~/api/file/take_file_download_stream")]
        [RequestSizeLimit(1024 * 1024 * 1024)]
        [HttpPost]
        public ActionResult TakeFileDownloadStream([FromForm] TakeFileDSF form)
        {
            var file = form.File;
            var streamCode = form.StreamCode;
            
            StreamRepo.FileStreams.Add(streamCode, file);

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
        public ActionResult DownloadFile(long fileId)
        {
            using (var context = new DatabaseContext())
            {
                var session = Security.Authenticate(context, Request.Headers[AuthExtracter.AK]);
                if (session == null) return NotFound();

                var guid = Guid.NewGuid().ToString();

                var address = new Uri(SharedArea.GlobalVariables.RABBITMQ_SERVER_URL + "/" +
                                      SharedArea.GlobalVariables.FILE_QUEUE_NAME + "?autodelete=true&durable=false&temporary=true");
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
                
                return File(stream.OpenReadStream(), "application/octet-stream");
            }
        }
    }
}