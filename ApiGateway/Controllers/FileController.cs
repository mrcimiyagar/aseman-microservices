
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using MessengerPlatform.DbContexts;
using SharedArea.Entities;
using SharedArea.Forms;
using SharedArea.Middles;
using AWP.Utils;
using Microsoft.AspNetCore.Identity.UI;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.EntityFrameworkCore;

namespace AWP.Controllers
{
    [Produces("application/json")]
    [Route("api/[controller]")]
    [ApiController]
    public class FileController : Controller
    {
        public const string DirPath = @"C:\\Aseman\Files";
        
        [Route("~/api/file/upload_photo")]
        [HttpPost]
        public ActionResult<Packet> UploadPhoto([FromForm] PhotoUploadForm form)
        {
            if (form.File == null || form.File.Length == 0) return new Packet {Status = "error_3"};
            using (var context = new DatabaseContext())
            {
                var session = Security.Authenticate(context, Request.Headers[AuthExtracter.AK]);
                if (session == null) return new Packet {Status = "error_0"};
                
                Photo photo;
                FileUsage fileUsage;
                
                if (form.RoomId > 0)
                {
                    context.Entry(session).Reference(s => s.BaseUser).Load();
                    var user = (User) session.BaseUser;
                    context.Entry(user).Collection(u => u.Memberships).Load();
                    var membership = user.Memberships.Find(m => m.ComplexId == form.ComplexId);
                    if (membership == null) return new Packet {Status = "error_1"};
                    context.Entry(membership).Reference(m => m.Complex).Load();
                    var complex = membership.Complex;
                    context.Entry(complex).Collection(c => c.Rooms).Load();
                    var room = complex.Rooms.Find(r => r.RoomId == form.RoomId);
                    if (room == null) return new Packet {Status = "error_2"};
                    photo = new Photo()
                    {
                        Width = form.Width,
                        Height = form.Height,
                        IsPublic = false
                    };
                    context.Files.Add(photo);
                    fileUsage = new FileUsage()
                    {
                        File = photo,
                        Room = room
                    };
                    context.FileUsages.Add(fileUsage);
                }
                else
                {
                    photo = new Photo()
                    {
                        Width = form.Width,
                        Height = form.Height,
                        IsPublic = true
                    };
                    context.Files.Add(photo);
                    fileUsage = null;
                }
                context.SaveChanges();
                Directory.CreateDirectory(DirPath);
                var filePath = DirPath + @"\" + photo.FileId;
                System.IO.File.Create(filePath).Close();
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    form.File.CopyTo(stream);
                }
                return new Packet {Status = "success", File = photo, FileUsage = fileUsage};
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
                
                Audio audio;
                FileUsage fileUsage;
                
                if (form.RoomId > 0)
                {
                    context.Entry(session).Reference(s => s.BaseUser).Load();
                    var user = (User) session.BaseUser;
                    context.Entry(user).Collection(u => u.Memberships).Load();
                    var membership = user.Memberships.Find(m => m.ComplexId == form.ComplexId);
                    if (membership == null) return new Packet {Status = "error_1"};
                    context.Entry(membership).Reference(m => m.Complex).Load();
                    var complex = membership.Complex;
                    context.Entry(complex).Collection(c => c.Rooms).Load();
                    var room = complex.Rooms.Find(r => r.RoomId == form.RoomId);
                    if (room == null) return new Packet {Status = "error_2"};
                    audio = new Audio()
                    {
                        Title = form.Title,
                        Duration = form.Duration,
                        IsPublic = false
                    };
                    context.Files.Add(audio);
                    fileUsage = new FileUsage()
                    {
                        File = audio,
                        Room = room
                    };
                    context.FileUsages.Add(fileUsage);
                }
                else
                {
                    audio = new Audio()
                    {
                        Title = form.Title,
                        Duration = form.Duration,
                        IsPublic = true
                    };
                    context.Files.Add(audio);
                    fileUsage = null;
                }
                context.SaveChanges();
                Directory.CreateDirectory(DirPath);
                var filePath = DirPath + @"\" + audio.FileId;
                System.IO.File.Create(filePath).Close();
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    form.File.CopyTo(stream);
                }
                return new Packet {Status = "success", File = audio, FileUsage = fileUsage};
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
                
                Video photo;
                FileUsage fileUsage;
                
                if (form.RoomId > 0)
                {
                    context.Entry(session).Reference(s => s.BaseUser).Load();
                    var user = (User) session.BaseUser;
                    context.Entry(user).Collection(u => u.Memberships).Load();
                    var membership = user.Memberships.Find(m => m.ComplexId == form.ComplexId);
                    if (membership == null) return new Packet {Status = "error_1"};
                    context.Entry(membership).Reference(m => m.Complex).Load();
                    var complex = membership.Complex;
                    context.Entry(complex).Collection(c => c.Rooms).Load();
                    var room = complex.Rooms.Find(r => r.RoomId == form.RoomId);
                    if (room == null) return new Packet {Status = "error_2"};
                    photo = new Video()
                    {
                        Title = form.Title,
                        Duration = form.Duration,
                        IsPublic = false
                    };
                    context.Files.Add(photo);
                    fileUsage = new FileUsage()
                    {
                        File = photo,
                        Room = room
                    };
                    context.FileUsages.Add(fileUsage);
                }
                else
                {
                    photo = new Video()
                    {
                        Title = form.Title,
                        Duration = form.Duration,
                        IsPublic = true
                    };
                    context.Files.Add(photo);
                    fileUsage = null;
                }
                context.SaveChanges();
                Directory.CreateDirectory(DirPath);
                var filePath = DirPath + @"\" + photo.FileId;
                System.IO.File.Create(filePath).Close();
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    form.File.CopyTo(stream);
                }
                return new Packet {Status = "success", File = photo, FileUsage = fileUsage};
            }
        }

        [Route("~/api/file/download_bot_avatar")]
        [HttpPost]
        public ActionResult DownloadBotAvatar([FromBody] Packet packet)
        {
            using (var context = new DatabaseContext())
            {
                var bot = context.Bots.Find(packet.Bot.BaseUserId);
                var file = context.Files.Find(bot.Avatar);
                if (file.IsPublic)
                {
                    var stream = System.IO.File.OpenRead(DirPath + @"\" + bot.Avatar);
                    return File(stream, "application/octet-stream");
                }
                else
                {
                    return NotFound(new Packet {Status = "error_0"});
                }
            }
        }
        
        [Route("~/api/file/download_room_avatar")]
        [HttpGet]
        public ActionResult DownloadRoomAvatar(long complexId, long roomId)
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
                var file = context.Files.Find(room.Avatar);
                if (file.IsPublic)
                {
                    var stream = System.IO.File.OpenRead(DirPath + @"\" + room.Avatar);
                    return File(stream, "application/octet-stream");
                }
                else
                {
                    return NotFound(new Packet {Status = "error_0"});
                }
            }
        }
        
        [Route("~/api/file/download_complex_avatar")]
        [HttpGet]
        public ActionResult DownloadComplexAvatar(long complexId)
        {
            using (var context = new DatabaseContext())
            {
                var complex = context.Complexes.Find(complexId);
                var file = context.Files.Find(complex.Avatar);
                if (file.IsPublic)
                {
                    var stream = System.IO.File.OpenRead(DirPath + @"\" + complex.Avatar);
                    return File(stream, "application/octet-stream");
                }
                else
                {
                    return NotFound(new Packet {Status = "error_0"});
                }
            }
        }
        
        [Route("~/api/file/download_user_avatar")]
        [HttpGet]
        public ActionResult DownloadUserAvatar(long userId)
        {
            using (var context = new DatabaseContext())
            {
                var user = context.Users.Find(userId);
                var file = context.Files.Find(user.Avatar);
                if (file.IsPublic)
                {
                    var stream = System.IO.File.OpenRead(DirPath + @"\" + user.Avatar);
                    return File(stream, "application/octet-stream");
                }
                else
                {
                    return NotFound(new Packet {Status = "error_0"});
                }
            }
        }

        [Route("~/api/file/download_file")]
        [HttpGet]
        public ActionResult DownloadFile(long fileId)
        {
            if (fileId == 0)
            {
                var stream = System.IO.File.OpenRead(DirPath + @"\" + 0);
                return File(stream, "application/octet-stream");
            }
            using (var context = new DatabaseContext())
            {
                var session = Security.Authenticate(context, Request.Headers[AuthExtracter.AK]);
                if (session == null) return NotFound(new Packet {Status = "error_0"});
                var file = context.Files.Find(fileId);
                if (file == null) return NotFound(new Packet {Status = "error_1"});
                context.Entry(session).Reference(s => s.BaseUser).Load();
                var user = (User) session.BaseUser;
                context.Entry(user).Collection(u => u.Memberships).Load();
                if (file.IsPublic)
                {
                    var stream = System.IO.File.OpenRead(DirPath + @"\" + file.FileId);
                    return File(stream, "application/octet-stream");
                }
                context.Entry(file).Collection(f => f.FileUsages).Query().Include(fu => fu.Room).Load();
                var foundPath = (from fu in file.FileUsages select fu.Room.ComplexId)
                    .Intersect(from mem in user.Memberships select mem.ComplexId).Any();
                if (foundPath)
                {
                    var stream = System.IO.File.OpenRead(DirPath + @"\" + file.FileId);
                    return File(stream, "application/octet-stream");
                }
                else
                {
                    return NotFound(new Packet {Status = "error_2"});
                }
            }
        }
    }
}