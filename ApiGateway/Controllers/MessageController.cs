using System;
using System.Linq;
using MessengerPlatform.DbContexts;
using AWP.Hubs;
using SharedArea.Entities;
using SharedArea.Middles;
using SharedArea.Notifications;
using AWP.Utils;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;

namespace AWP.Controllers
{
    [Produces("application/json")]
    [Route("api/[controller]")]
    [ApiController]
    public class MessageController : Controller
    {
        private readonly IHubContext<NotificationsHub> _notifsHub;
        
        public MessageController(IHubContext<NotificationsHub> notifsHub)
        {
            _notifsHub = notifsHub;
        }
        
        [Route("~/api/message/get_messages")]
        [HttpPost]
        public ActionResult<Packet> GetMessages([FromBody] Packet packet)
        {
            using (var context = new DatabaseContext())
            {
                var session = Security.Authenticate(context, Request.Headers[AuthExtracter.AK]);
                if (session == null) return new Packet {Status = "error_0U0"};
                context.Entry(session).Reference(s => s.BaseUser).Load();
                var user = (User) session.BaseUser;
                context.Entry(user).Collection(u => u.Memberships).Load();
                var membership = user.Memberships.Find(m => m.ComplexId == packet.Complex.ComplexId);
                if (membership == null) return new Packet {Status = "error_0U1"};
                context.Entry(membership).Reference(m => m.Complex).Load();
                var complex = membership.Complex;
                context.Entry(complex).Collection(c => c.Rooms).Load();
                var room = complex.Rooms.Find(r => r.RoomId == packet.Room.RoomId);
                if (room == null) return new Packet {Status = "error_0U2"};
                context.Entry(room).Collection(r => r.Messages).Load();
                var messages = room.Messages.Skip(room.Messages.Count() - 100).ToList();
                return new Packet {Status = "success", Messages = messages};
            }
        }

        [Route("~/api/message/create_text_message")]
        [HttpPost]
        public ActionResult<Packet> CreateTextMessage([FromBody] Packet packet)
        {
            using (var context = new DatabaseContext())
            {
                var session = Security.Authenticate(context, Request.Headers[AuthExtracter.AK]);
                if (session == null) return new Packet {Status = "error_0"};
                context.Entry(session).Reference(s => s.BaseUser).Load();
                var human = (User) session.BaseUser;
                context.Entry(human).Collection(h => h.Memberships).Load();
                var membership = human.Memberships.Find(m => m.ComplexId == packet.Complex.ComplexId);
                if (membership == null) return new Packet {Status = "error_1"};
                context.Entry(membership).Reference(m => m.Complex).Load();
                var complex = membership.Complex;
                context.Entry(complex).Collection(c => c.Rooms).Load();
                var room = complex.Rooms.Find(r => r.RoomId == packet.Room.RoomId);
                if (room == null) return new Packet {Status = "error_2"};
                var message = new TextMessage()
                {
                    Author = human,
                    Room = room,
                    Text = packet.TextMessage.Text,
                    Time = Convert.ToInt64((DateTime.Now - DateTime.MinValue).TotalMilliseconds)
                };
                context.Messages.Add(message);
                context.SaveChanges();
                context.Entry(human).Collection(h => h.Sessions).Load();
                context.Entry(complex).Collection(c => c.Members).Load();
                TextMessage nextMessage;
                using (var nextContext = new DatabaseContext())
                {
                    nextMessage = (TextMessage) nextContext.Messages.Find(message.MessageId);
                    nextContext.Entry(nextMessage).Reference(m => m.Room).Load();
                    nextContext.Entry(nextMessage.Room).Reference(r => r.Complex).Load();
                    nextContext.Entry(nextMessage).Reference(m => m.Author).Load();
                }
                foreach (var m in complex.Members)
                {
                    if (m.UserId == session.BaseUserId) continue;
                    context.Entry(m).Reference(mem => mem.User).Load();
                    var member = m.User;
                    context.Entry(member).Collection(u => u.Sessions).Load();
                    foreach (var s in member.Sessions) 
                    {
                        var mcn = new TextMessageNotification()
                        {
                            Message = nextMessage,
                            Session = s
                        };
                        if (s.Online)
                        {
                            _notifsHub.Clients.Client(s.ConnectionId)
                                .SendAsync("NotifyTextMessageReceived", mcn);
                        } 
                        else
                        {
                            context.Notifications.Add(mcn);
                        }
                    }
                }
                context.Entry(room).Collection(r => r.Workers).Load();
                foreach (var w in room.Workers)
                {
                    var worker = context.Bots.Find(w.BotId);
                    context.Entry(worker).Collection(wor => wor.Sessions).Load();
                    var worSession = worker.Sessions.FirstOrDefault();
                    var mcn = new TextMessageNotification()
                    {
                        Message = nextMessage,
                        Session = worSession
                    };
                    if (worSession.Online)
                    {
                        _notifsHub.Clients.Client(worSession.ConnectionId)
                            .SendAsync("NotifyTextMessageReceived", mcn);
                    } 
                    else
                    {
                        context.Notifications.Add(mcn);
                    }
                }
                context.SaveChanges();
                return new Packet {Status = "success", TextMessage = nextMessage};
            }
        }

        [Route("~/api/message/create_file_message")]
        [HttpPost]
        public ActionResult<Packet> CreateFileMessage([FromBody] Packet packet)
        {
            using (var context = new DatabaseContext())
            {
                var session = Security.Authenticate(context, Request.Headers[AuthExtracter.AK]);
                if (session == null) return new Packet {Status = "error_0"};
                context.Entry(session).Reference(s => s.BaseUser).Load();
                var human = (User) session.BaseUser;
                context.Entry(human).Collection(h => h.Memberships).Load();
                var membership = human.Memberships.Find(mem => mem.ComplexId == packet.Complex.ComplexId);
                if (membership == null) return new Packet {Status = "error_1"};
                context.Entry(membership).Reference(mem => mem.Complex).Load();
                var complex = membership.Complex;
                context.Entry(complex).Collection(c => c.Rooms).Load();
                var room = complex.Rooms.Find(r => r.RoomId == packet.Room.RoomId);
                if (room == null) return new Packet {Status = "error_2"};
                Message message = null;
                context.Entry(room).Collection(r => r.Files).Load();
                var fileUsage = room.Files.Find(fu => fu.FileId == packet.File.FileId);
                if (fileUsage == null) return new Packet {Status = "error_3"};
                context.Entry(fileUsage).Reference(fu => fu.File).Load();
                var file = fileUsage.File;
                switch (file)
                {
                    case Photo photo:
                        message = new PhotoMessage()
                        {
                            Author = human,
                            Room = room,
                            Time = Convert.ToInt64((DateTime.Now - DateTime.MinValue).TotalMilliseconds),
                            Photo = photo
                        };
                        break;
                    case Audio audio:
                        message = new AudioMessage()
                        {
                            Author = human,
                            Room = room,
                            Time = Convert.ToInt64((DateTime.Now - DateTime.MinValue).TotalMilliseconds),
                            Audio = audio
                        };
                        break;
                    case Video video:
                        message = new VideoMessage()
                        {
                            Author = human,
                            Room = room,
                            Time = Convert.ToInt64((DateTime.Now - DateTime.MinValue).TotalMilliseconds),
                            Video = video
                        };
                        break;
                }
                if (message == null) return new Packet {Status = "error_4"};
                context.Messages.Add(message);
                context.SaveChanges();
                
                Message nextMessage;
                using (var nextContext = new DatabaseContext())
                {
                    nextMessage = nextContext.Messages
                        .Include(msg => msg.Room)
                        .ThenInclude(r => r.Complex)
                        .Include(msg => msg.Author)
                        .FirstOrDefault(msg => msg.MessageId == message.MessageId);
                    switch (nextMessage)
                    {
                        case PhotoMessage _ :
                            nextContext.Entry(nextMessage).Reference(msg => ((PhotoMessage)msg).Photo).Load();
                            nextContext.Entry(((PhotoMessage)nextMessage).Photo).Collection(f => f.FileUsages)
                                .Query().Where(fu => fu.FileUsageId == fileUsage.FileUsageId).Load();
                            break;
                        case AudioMessage _ :
                            nextContext.Entry(nextMessage).Reference(msg => ((AudioMessage)msg).Audio).Load();
                            nextContext.Entry(((AudioMessage)nextMessage).Audio).Collection(f => f.FileUsages)
                                .Query().Where(fu => fu.FileUsageId == fileUsage.FileUsageId).Load();
                            break;
                        case VideoMessage _ :
                            nextContext.Entry(nextMessage).Reference(msg => ((VideoMessage)msg).Video).Load();
                            nextContext.Entry(((VideoMessage)nextMessage).Video).Collection(f => f.FileUsages)
                                .Query().Where(fu => fu.FileUsageId == fileUsage.FileUsageId).Load();
                            break;
                    }
                }
                    
                context.Entry(complex).Collection(c => c.Members).Load();
                foreach (var m in complex.Members)
                {
                    if (m.UserId == session.BaseUserId) continue;
                    context.Entry(m).Reference(mem => mem.User).Load();
                    var member = m.User;
                    context.Entry(member).Collection(u => u.Sessions).Load();
                    foreach (var s in member.Sessions)
                    {
                        Notification notif;
                        switch (nextMessage)
                        {
                            case PhotoMessage msg:
                                notif = new PhotoMessageNotification()
                                {
                                    Message = msg,
                                    Session = s
                                };
                                if (s.Online)
                                {
                                    _notifsHub.Clients.Client(s.ConnectionId)
                                        .SendAsync("NotifyPhotoMessageReceived", notif);
                                }
                                else
                                {
                                    context.Notifications.Add(notif);
                                }
                                break;
                            case AudioMessage msg:
                                notif = new AudioMessageNotification()
                                {
                                    Message = msg,
                                    Session = s
                                };
                                if (s.Online)
                                {
                                    _notifsHub.Clients.Client(s.ConnectionId)
                                        .SendAsync("NotifyAudioMessageReceived", notif);
                                }
                                else
                                {
                                    context.Notifications.Add(notif);
                                }
                                break;
                            case VideoMessage msg:
                                notif = new VideoMessageNotification()
                                {
                                    Message = msg,
                                    Session = s
                                };
                                if (s.Online)
                                {
                                    _notifsHub.Clients.Client(s.ConnectionId)
                                        .SendAsync("NotifyVideoMessageReceived", notif);
                                }
                                else
                                {
                                    context.Notifications.Add(notif);
                                }
                                break;
                        }
                    }
                }
                context.Entry(room).Collection(r => r.Workers).Load();
                foreach (var w in room.Workers)
                {
                    var worker = context.Bots.Find(w.BotId);
                    context.Entry(worker).Collection(wor => wor.Sessions).Load();
                    var worSession = worker.Sessions.FirstOrDefault();
                    Notification notif;
                        switch (nextMessage)
                        {
                            case PhotoMessage msg:
                                notif = new PhotoMessageNotification()
                                {
                                    Message = msg,
                                    Session = worSession
                                };
                                if (worSession.Online)
                                {
                                    _notifsHub.Clients.Client(worSession.ConnectionId)
                                        .SendAsync("NotifyPhotoMessageReceived", notif);
                                }
                                else
                                {
                                    context.Notifications.Add(notif);
                                }
                                break;
                            case AudioMessage msg:
                                notif = new AudioMessageNotification()
                                {
                                    Message = msg,
                                    Session = worSession
                                };
                                if (worSession.Online)
                                {
                                    _notifsHub.Clients.Client(worSession.ConnectionId)
                                        .SendAsync("NotifyAudioMessageReceived", notif);
                                }
                                else
                                {
                                    context.Notifications.Add(notif);
                                }
                                break;
                            case VideoMessage msg:
                                notif = new VideoMessageNotification()
                                {
                                    Message = msg,
                                    Session = worSession
                                };
                                if (worSession.Online)
                                {
                                    _notifsHub.Clients.Client(worSession.ConnectionId)
                                        .SendAsync("NotifyVideoMessageReceived", notif);
                                }
                                else
                                {
                                    context.Notifications.Add(notif);
                                }
                                break;
                        }
                }
                context.SaveChanges();

                switch (nextMessage)
                {
                    case PhotoMessage msg:
                        return new Packet {Status = "success", PhotoMessage = msg};
                    case AudioMessage msg:
                        return new Packet {Status = "success", AudioMessage = msg};
                    case VideoMessage msg:
                        return new Packet {Status = "success", VideoMessage = msg};
                    default:
                        return new Packet {Status = "error_5"};
                }
            }
        }
        
        [Route("~/api/message/bot_create_text_message")]
        [HttpPost]
        public ActionResult<Packet> BotCreateTextMessage([FromBody] Packet packet)
        {
            using (var context = new DatabaseContext())
            {
                string authHeader = Request.Headers[AuthExtracter.AK];
                var parts = authHeader.Split(" ");
                var botId = long.Parse(parts[0]);
                var token = parts[1];
                var bot = context.Bots.Find(botId);
                if (bot == null) return new Packet {Status = "error_1"};
                context.Entry(bot).Reference(b => b.BotSecret).Load();
                if (bot.BotSecret.Token != token) return new Packet {Status = "error_2"};
                var complex = context.Complexes.Find(packet.Complex.ComplexId);
                if (complex == null) return new Packet {Status = "error_3"};
                context.Entry(complex).Collection(c => c.Rooms).Load();
                var room = complex.Rooms.Find(r => r.RoomId == packet.Room.RoomId);
                if (room == null) return new Packet {Status = "error_4"};
                context.Entry(room).Collection(r => r.Workers).Load();
                var workership = room.Workers.Find(w => w.BotId == botId);
                if (workership == null) return new Packet {Status = "error_5"};
                var message = new TextMessage()
                {
                    Author = bot,
                    Room = room,
                    Text = packet.TextMessage.Text,
                    Time = Convert.ToInt64((DateTime.Now - DateTime.MinValue).TotalMilliseconds)
                };
                context.Messages.Add(message);
                context.SaveChanges();
                TextMessage nextMessage;
                using (var nextContext = new DatabaseContext())
                {
                    nextMessage = (TextMessage) nextContext.Messages.Find(message.MessageId);
                    nextContext.Entry(nextMessage).Reference(m => m.Room).Load();
                    nextContext.Entry(nextMessage.Room).Reference(r => r.Complex).Load();
                    nextContext.Entry(nextMessage).Reference(m => m.Author).Load();
                }
                context.Entry(complex).Collection(c => c.Members).Load();
                foreach (var m in complex.Members)
                {
                    context.Entry(m).Reference(mem => mem.User).Load();
                    var member = m.User;
                    context.Entry(member).Collection(u => u.Sessions).Load();
                    foreach (var s in member.Sessions) 
                    {
                        var mcn = new TextMessageNotification()
                        {
                            Message = nextMessage,
                            Session = s
                        };
                        if (s.Online)
                        {
                            _notifsHub.Clients.Client(s.ConnectionId)
                                .SendAsync("NotifyTextMessageReceived", mcn);
                        } 
                        else
                        {
                            context.Notifications.Add(mcn);
                        }
                    }
                }
                context.Entry(room).Collection(r => r.Workers).Load();
                foreach (var w in room.Workers)
                {
                    if (w.BotId == bot.BaseUserId) continue;
                    var worker = context.Bots.Find(w.BotId);
                    context.Entry(worker).Collection(wor => wor.Sessions).Load();
                    var worSession = worker.Sessions.FirstOrDefault();
                    var mcn = new TextMessageNotification()
                    {
                        Message = nextMessage,
                        Session = worSession
                    };
                    if (worSession.Online)
                    {
                        _notifsHub.Clients.Client(worSession.ConnectionId)
                            .SendAsync("NotifyTextMessageReceived", mcn);
                    } 
                    else
                    {
                        context.Notifications.Add(mcn);
                    }
                }
                context.SaveChanges();
                return new Packet {Status = "success", TextMessage = nextMessage};
            }
        }
        
        [Route("~/api/message/bot_create_file_message")]
        [HttpPost]
        public ActionResult<Packet> BotCreateFileMessage([FromBody] Packet packet)
        {
            using (var context = new DatabaseContext())
            {
                string authHeader = Request.Headers[AuthExtracter.AK];
                var parts = authHeader.Split(" ");
                var botId = long.Parse(parts[0]);
                var token = parts[1];
                var bot = context.Bots.Find(botId);
                if (bot == null) return new Packet {Status = "error_1"};
                context.Entry(bot).Reference(b => b.BotSecret).Load();
                if (bot.BotSecret.Token != token) return new Packet {Status = "error_2"};
                var complex = context.Complexes.Find(packet.Complex.ComplexId);
                if (complex == null) return new Packet {Status = "error_3"};
                context.Entry(complex).Collection(c => c.Rooms).Load();
                var room = complex.Rooms.Find(r => r.RoomId == packet.Room.RoomId);
                if (room == null) return new Packet {Status = "error_4"};
                context.Entry(room).Collection(r => r.Workers).Load();
                var workership = room.Workers.Find(w => w.BotId == botId);
                if (workership == null) return new Packet {Status = "error_5"};
                Message message = null;
                context.Entry(room).Collection(r => r.Files).Load();
                var fileUsage = room.Files.Find(fu => fu.FileId == packet.File.FileId);
                if (fileUsage == null) return new Packet {Status = "error_6"};
                context.Entry(fileUsage).Reference(fu => fu.File).Load();
                var file = fileUsage.File;
                switch (file)
                {
                    case Photo photo:
                        message = new PhotoMessage()
                        {
                            Author = bot,
                            Room = room,
                            Time = Convert.ToInt64((DateTime.Now - DateTime.MinValue).TotalMilliseconds),
                            Photo = photo
                        };
                        break;
                    case Audio audio:
                        message = new AudioMessage()
                        {
                            Author = bot,
                            Room = room,
                            Time = Convert.ToInt64((DateTime.Now - DateTime.MinValue).TotalMilliseconds),
                            Audio = audio
                        };
                        break;
                    case Video video:
                        message = new VideoMessage()
                        {
                            Author = bot,
                            Room = room,
                            Time = Convert.ToInt64((DateTime.Now - DateTime.MinValue).TotalMilliseconds),
                            Video = video
                        };
                        break;
                }
                if (message == null) return new Packet {Status = "error_7"};
                context.Messages.Add(message);
                context.SaveChanges();
                
                Message nextMessage;
                using (var nextContext = new DatabaseContext())
                {
                    nextMessage = nextContext.Messages.Find(message.MessageId);
                    nextContext.Entry(nextMessage).Reference(msg => msg.Room).Load();
                    nextContext.Entry(nextMessage.Room).Reference(r => r.Complex).Load();
                    nextContext.Entry(nextMessage).Reference(msg => msg.Author).Load();
                    switch (nextMessage)
                    {
                        case PhotoMessage _ :
                            nextContext.Entry(nextMessage).Reference(msg => ((PhotoMessage)msg).Photo).Load();
                            nextContext.Entry(((PhotoMessage)nextMessage).Photo).Collection(f => f.FileUsages)
                                .Query().Where(fu => fu.FileUsageId == fileUsage.FileUsageId).Load();
                            break;
                        case AudioMessage _ :
                            nextContext.Entry(nextMessage).Reference(msg => ((AudioMessage)msg).Audio).Load();
                            nextContext.Entry(((AudioMessage)nextMessage).Audio).Collection(f => f.FileUsages)
                                .Query().Where(fu => fu.FileUsageId == fileUsage.FileUsageId).Load();
                            break;
                        case VideoMessage _ :
                            nextContext.Entry(nextMessage).Reference(msg => ((VideoMessage)msg).Video).Load();
                            nextContext.Entry(((VideoMessage)nextMessage).Video).Collection(f => f.FileUsages)
                                .Query().Where(fu => fu.FileUsageId == fileUsage.FileUsageId).Load();
                            break;
                    }
                }
                
                context.Entry(complex).Collection(c => c.Members).Load();
                foreach (var m in complex.Members)
                {
                    context.Entry(m).Reference(mem => mem.User).Load();
                    var member = m.User;
                    context.Entry(member).Collection(u => u.Sessions).Load();
                    foreach (var s in member.Sessions)
                    {
                        Notification notif;
                        switch (nextMessage)
                        {
                            case PhotoMessage msg:
                                notif = new PhotoMessageNotification()
                                {
                                    Message = msg,
                                    Session = s
                                };
                                if (s.Online)
                                {
                                    _notifsHub.Clients.Client(s.ConnectionId)
                                        .SendAsync("NotifyPhotoMessageReceived", notif);
                                }
                                else
                                {
                                    context.Notifications.Add(notif);
                                }
                                break;
                            case AudioMessage msg:
                                notif = new AudioMessageNotification()
                                {
                                    Message = msg,
                                    Session = s
                                };
                                if (s.Online)
                                {
                                    _notifsHub.Clients.Client(s.ConnectionId)
                                        .SendAsync("NotifyAudioMessageReceived", notif);
                                }
                                else
                                {
                                    context.Notifications.Add(notif);
                                }
                                break;
                            case VideoMessage msg:
                                notif = new VideoMessageNotification()
                                {
                                    Message = msg,
                                    Session = s
                                };
                                if (s.Online)
                                {
                                    _notifsHub.Clients.Client(s.ConnectionId)
                                        .SendAsync("NotifyVideoMessageReceived", notif);
                                }
                                else
                                {
                                    context.Notifications.Add(notif);
                                }
                                break;
                        }
                    }
                }
                context.Entry(room).Collection(r => r.Workers).Load();
                foreach (var w in room.Workers)
                {
                    if (w.BotId == bot.BaseUserId) continue;
                    var worker = context.Bots.Find(w.BotId);
                    context.Entry(worker).Collection(wor => wor.Sessions).Load();
                    var worSession = worker.Sessions.FirstOrDefault();
                    Notification notif;
                        switch (nextMessage)
                        {
                            case PhotoMessage msg:
                                notif = new PhotoMessageNotification()
                                {
                                    Message = msg,
                                    Session = worSession
                                };
                                if (worSession.Online)
                                {
                                    _notifsHub.Clients.Client(worSession.ConnectionId)
                                        .SendAsync("NotifyPhotoMessageReceived", notif);
                                }
                                else
                                {
                                    context.Notifications.Add(notif);
                                }
                                break;
                            case AudioMessage msg:
                                notif = new AudioMessageNotification()
                                {
                                    Message = msg,
                                    Session = worSession
                                };
                                if (worSession.Online)
                                {
                                    _notifsHub.Clients.Client(worSession.ConnectionId)
                                        .SendAsync("NotifyAudioMessageReceived", notif);
                                }
                                else
                                {
                                    context.Notifications.Add(notif);
                                }
                                break;
                            case VideoMessage msg:
                                notif = new VideoMessageNotification()
                                {
                                    Message = msg,
                                    Session = worker.Sessions.FirstOrDefault()
                                };
                                if (worSession.Online)
                                {
                                    _notifsHub.Clients.Client(worSession.ConnectionId)
                                        .SendAsync("NotifyVideoMessageReceived", notif);
                                }
                                else
                                {
                                    context.Notifications.Add(notif);
                                }
                                break;
                        }
                }
                context.SaveChanges();

                switch (nextMessage)
                {
                    case PhotoMessage msg:
                        return new Packet {Status = "success", PhotoMessage = msg};
                    case AudioMessage msg:
                        return new Packet {Status = "success", AudioMessage = msg};
                    case VideoMessage msg:
                        return new Packet {Status = "success", VideoMessage = msg};
                    default:
                        return new Packet {Status = "error_5"};
                }
            }
        }
    }
}