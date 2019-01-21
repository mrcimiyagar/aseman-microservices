﻿
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using ApiGateway.DbContexts;
using ApiGateway.Models;
using ApiGateway.Models.Forms;
using MassTransit;
using Microsoft.AspNetCore.Http;
using SharedArea.Entities;
using SharedArea.Forms;
using SharedArea.Middles;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using SharedArea.Commands.File;
using SharedArea.Utils;

namespace ApiGateway.Controllers
{
    [Produces("application/json")]
    [Route("api/[controller]")]
    [ApiController]
    public class FileController : Controller
    {
        public const string DirPath = @"C:\\Aseman\Files";

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
        
        [Route("~/api/file/notify_file_transffered")]
        [HttpPost]
        public ActionResult NotifyFileTransfered([FromBody] Packet packet)
        {
            if (packet.Username == SharedArea.GlobalVariables.FILE_TRANSFER_USERNAME
                && packet.Password == SharedArea.GlobalVariables.FILE_TRANSFER_PASSWORD)
            {
                var exitLock = StreamRepo.FileStreamLocks[packet.StreamCode];
            
                lock(exitLock)
                {
                    Monitor.Pulse(exitLock);
                }

                return Ok();
            }
            else
            {
                return NotFound();
            }
        }

        [Route("~/api/file/upload_photo")]
        [HttpPost]
        public ActionResult<Packet> UploadPhoto([FromForm] PhotoUploadForm form)
        {
            if (form.File == null || form.File.Length == 0) return new Packet {Status = "error_3"};
            using (var context = new DatabaseContext())
            {
                var session = Security.Authenticate(context, Request.Headers[AuthExtracter.AK]);
                if (session == null) return new Packet {Status = "error_0"};

                var lockObj = new object();

                var guid = Guid.NewGuid().ToString();
                                
                lock(lockObj)
                {
                    StreamRepo.FileStreamLocks.Add(guid, lockObj);
                    StreamRepo.FileStreams.Add(guid, form.File);
                    
                    var address = new Uri(SharedArea.GlobalVariables.RABBITMQ_SERVER_URL + "/" + SharedArea.GlobalVariables.FILE_QUEUE_NAME);
                    var requestTimeout = TimeSpan.FromSeconds(SharedArea.GlobalVariables.RABBITMQ_REQUEST_TIMEOUT);
                    IRequestClient<UploadPhotoRequest, UploadPhotoResponse> client = 
                        new MessageRequestClient<UploadPhotoRequest, UploadPhotoResponse>(
                            Program.Bus, address, requestTimeout);
                    var puf = new PhotoUF()
                    {
                        ComplexId = form.ComplexId,
                        RoomId = form.RoomId,
                        Width = form.Width,
                        Height = form.Height
                    };
                    var result = client.Request<UploadPhotoRequest, UploadPhotoResponse>(new
                    {
                        Headers = Request.Headers.ToDictionary(a => a.Key, a => a.Value.ToString()),
                        Form = puf,
                        SessionId = session.SessionId,
                        StreamCode = guid
                    }).Result;

                    Monitor.Wait(lockObj);

                    StreamRepo.FileStreamLocks.Remove(guid);
                    StreamRepo.FileStreams.Remove(guid);

                    return result.Packet;
                }
            }
        }
        
        [Route("~/api/file/upload_audio")]
        [HttpPost]
        public ActionResult<Packet> UploadAudio([FromForm] AudioUploadForm form)
        {
            if (form.File == null || form.File.Length == 0) return new Packet {Status = "error_3"};
            using (var context = new DatabaseContext())
            {
                var session = Security.Authenticate(context, Request.Headers[AuthExtracter.AK]);
                if (session == null) return new Packet {Status = "error_0"};

                var lockObj = new object();

                var guid = Guid.NewGuid().ToString();
                                
                lock(lockObj)
                {
                    StreamRepo.FileStreamLocks.Add(guid, lockObj);
                    StreamRepo.FileStreams.Add(guid, form.File);
                    
                    var address = new Uri(SharedArea.GlobalVariables.RABBITMQ_SERVER_URL + "/" + SharedArea.GlobalVariables.FILE_QUEUE_NAME);
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
                    var result = client.Request<UploadAudioRequest, UploadAudioResponse>(new
                    {
                        Headers = Request.Headers.ToDictionary(a => a.Key, a => a.Value.ToString()),
                        Form = auf,
                        SessionId = session.SessionId,
                        StreamCode = guid
                    }).Result;

                    Monitor.Wait(lockObj);

                    StreamRepo.FileStreamLocks.Remove(guid);
                    StreamRepo.FileStreams.Remove(guid);

                    return result.Packet;
                }
            }
        }
        
        [Route("~/api/file/upload_video")]
        [HttpPost]
        public ActionResult<Packet> UploadVideo([FromForm] VideoUploadForm form)
        {
            if (form.File == null || form.File.Length == 0) return new Packet {Status = "error_3"};
            using (var context = new DatabaseContext())
            {
                var session = Security.Authenticate(context, Request.Headers[AuthExtracter.AK]);
                if (session == null) return new Packet {Status = "error_0"};

                var lockObj = new object();

                var guid = Guid.NewGuid().ToString();
                                
                lock(lockObj)
                {
                    StreamRepo.FileStreamLocks.Add(guid, lockObj);
                    StreamRepo.FileStreams.Add(guid, form.File);
                    
                    var address = new Uri(SharedArea.GlobalVariables.RABBITMQ_SERVER_URL + "/" + SharedArea.GlobalVariables.FILE_QUEUE_NAME);
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
                    var result = client.Request<UploadVideoRequest, UploadVideoResponse>(new
                    {
                        Headers = Request.Headers.ToDictionary(a => a.Key, a => a.Value.ToString()),
                        Form = vuf,
                        SessionId = session.SessionId,
                        StreamCode = guid
                    }).Result;

                    Monitor.Wait(lockObj);

                    StreamRepo.FileStreamLocks.Remove(guid);
                    StreamRepo.FileStreams.Remove(guid);

                    return result.Packet;
                }
            }
        }

        [Route("~/api/file/download_bot_avatar")]
        [HttpPost]
        public HttpResponseMessage DownloadBotAvatar([FromBody] Packet packet)
        {
            using (var context = new DatabaseContext())
            {
                var bot = context.Bots.Find(packet.Bot.BaseUserId);
                return DownloadFile(bot.Avatar);
            }
        }
        
        [Route("~/api/file/download_room_avatar")]
        [HttpGet]
        public HttpResponseMessage DownloadRoomAvatar(long complexId, long roomId)
        {
            using (var context = new DatabaseContext())
            {
                var session = Security.Authenticate(context, Request.Headers[AuthExtracter.AK]);
                context.Entry(session).Reference(s => s.BaseUser).Load();
                var user = (User) session.BaseUser;
                context.Entry(user).Collection(u => u.Memberships).Load();
                var membership = user.Memberships.Find(mem => mem.ComplexId == complexId);
                if (membership == null) return NotFound(new Packet {Status = "error_1"});
                context.Entry(membership).Reference(mem => mem.Complex).Load();
                var complex = membership.Complex;
                context.Entry(complex).Collection(c => c.Rooms);
                var room = complex.Rooms.Find(r => r.RoomId == roomId);
                return downloa
            }
        }
        
        [Route("~/api/file/download_complex_avatar")]
        [HttpGet]
        public HttpResponseMessage DownloadComplexAvatar(long complexId)
        {
            using (var context = new DatabaseContext())
            {
                var complex = context.Complexes.Find(complexId);
                return DownloadFile(complex.Avatar);
            }
        }
        
        [Route("~/api/file/download_user_avatar")]
        [HttpGet]
        public HttpResponseMessage DownloadUserAvatar(long userId)
        {
            using (var context = new DatabaseContext())
            {
                var user = context.Users.Find(userId);
                return DownloadFile(user.Avatar);
            }
        }

        [Route("~/api/file/take_file_download_stream")]
        [HttpPost]
        public ActionResult TakeFileDownloadStream(TakeFileDSF form)
        {
            var file = form.File;
            var streamCode = form.StreamCode;

            var doneLockObj = new object();

            lock (doneLockObj)
            {
                var lockObj = StreamRepo.FileStreamLocks[streamCode];
            
                lock (lockObj)
                {
                    StreamRepo.FileStreams.Add(streamCode, file);
                
                    Monitor.Pulse(lockObj);
                }
                
                StreamRepo.FileTransferDoneLocks.Add(streamCode, doneLockObj);

                Monitor.Wait(doneLockObj);
            }

            return Ok();
        }

        [Route("~/api/file/download_file")]
        [HttpGet]
        public HttpResponseMessage DownloadFile(long fileId)
        {
            using (var context = new DatabaseContext())
            {
                var session = Security.Authenticate(context, Request.Headers[AuthExtracter.AK]);
                if (session == null)
                    return new HttpResponseMessage(HttpStatusCode.NotFound);
                
                var lockObj = new object();

                var guid = Guid.NewGuid().ToString();
                                
                lock(lockObj)
                {
                    StreamRepo.FileStreamLocks.Add(guid, lockObj);
                    
                    var address = new Uri(SharedArea.GlobalVariables.RABBITMQ_SERVER_URL + "/" + SharedArea.GlobalVariables.FILE_QUEUE_NAME);
                    var requestTimeout = TimeSpan.FromSeconds(SharedArea.GlobalVariables.RABBITMQ_REQUEST_TIMEOUT);
                    IRequestClient<DownloadFileRequest, DownloadFileResponse> client = 
                        new MessageRequestClient<DownloadFileRequest, DownloadFileResponse>(
                            Program.Bus, address, requestTimeout);
                    var result = client.Request<DownloadFileRequest, DownloadFileResponse>(new
                    {
                        Headers = Request.Headers.ToDictionary(a => a.Key, a => a.Value.ToString()),
                        SessionId = session.SessionId,
                        StreamCode = guid
                    }).Result;

                    if (result.Packet.Status != "success")
                    {
                        var resp = new HttpResponseMessage(HttpStatusCode.InternalServerError);
                        var myContent = JsonConvert.SerializeObject(result.Packet);
                        var buffer = System.Text.Encoding.UTF8.GetBytes(myContent);
                        var byteContent = new ByteArrayContent(buffer);
                        byteContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                        resp.Content = byteContent;
                        return resp;
                    }

                    Monitor.Wait(lockObj);

                    StreamRepo.FileStreamLocks.Remove(guid);
                    var stream = StreamRepo.FileStreams[guid];
                    StreamRepo.FileStreams.Remove(guid);
                    
                    var response = new HttpResponseMessage(HttpStatusCode.OK)
                    {
                        Content = new NotifierStreamContent(stream.OpenReadStream(), ex =>
                        {
                            var doneLockObj = StreamRepo.FileTransferDoneLocks[guid];
                            lock (doneLockObj)
                            {
                                Monitor.Pulse(doneLockObj);
                            }

                            StreamRepo.FileTransferDoneLocks.Remove(guid);
                        })
                    };
                    response.Content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");    
                    return response;
                }
            }
        }
    }
}