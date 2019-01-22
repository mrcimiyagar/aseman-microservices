using System;
using System.Linq;
using System.Threading.Tasks;
using MassTransit;
using MessengerPlatform.DbContexts;
using Microsoft.EntityFrameworkCore;
using SharedArea.Commands.Internal.Notifications;
using SharedArea.Commands.Message;
using SharedArea.Commands.Pushes;
using SharedArea.Consumers;
using SharedArea.Entities;
using SharedArea.Middles;
using SharedArea.Notifications;
using SharedArea.Utils;

namespace MessengerPlatform.Consumers
{
    public class MessengerConsumer : NotifConsumer, IConsumer<BotCreatedNotif>, IConsumer<GetMessagesRequest>
        , IConsumer<CreateTextMessageRequest>, IConsumer<CreateFileMessageRequest>, IConsumer<BotCreateTextMessageRequest>
        , IConsumer<BotCreateFileMessageRequest>, IConsumer<PhotoCreatedNotif>, IConsumer<AudioCreatedNotif>
        , IConsumer<VideoCreatedNotif>
    {
        public Task Consume(ConsumeContext<BotCreatedNotif> context)
        {
            using (var dbContext = new DatabaseContext())
            {
                var globalUser = context.Message.Packet.User;
                var localUser = (User) dbContext.BaseUsers.Find(globalUser.BaseUserId);
                var bot = context.Message.Packet.Bot;
                var botSecret = bot.BotSecret;
                var session = bot.Sessions.FirstOrDefault();
                var creation = context.Message.Packet.BotCreation;
                var subscription = context.Message.Packet.BotSubscription;

                creation.Bot = bot;
                creation.Creator = localUser;

                subscription.Bot = bot;
                subscription.Subscriber = localUser;
                
                dbContext.AddRange(bot, botSecret, session, creation, subscription);

                dbContext.SaveChanges();
            }
            
            return Task.CompletedTask;
        }

        public async Task Consume(ConsumeContext<GetMessagesRequest> context)
        {
            using (var dbContext = new DatabaseContext())
            {
                var packet = context.Message.Packet;
                var session = dbContext.Sessions.Find(context.Message.SessionId);

                dbContext.Entry(session).Reference(s => s.BaseUser).Load();
                var user = (User) session.BaseUser;
                dbContext.Entry(user).Collection(u => u.Memberships).Load();
                var membership = user.Memberships.Find(m => m.ComplexId == packet.Complex.ComplexId);
                if (membership == null)
                {
                    await context.RespondAsync(new GetMessagesResponse()
                    {
                        Packet = new Packet {Status = "error_0U1"}
                    });
                    return;
                }
                dbContext.Entry(membership).Reference(m => m.Complex).Load();
                var complex = membership.Complex;
                dbContext.Entry(complex).Collection(c => c.Rooms).Load();
                var room = complex.Rooms.Find(r => r.RoomId == packet.Room.RoomId);
                if (room == null)
                {
                    await context.RespondAsync(new GetMessagesResponse()
                    {
                        Packet = new Packet {Status = "error_0U2"}
                    });
                    return;
                }
                dbContext.Entry(room).Collection(r => r.Messages).Load();
                var messages = room.Messages.Skip(room.Messages.Count() - 100).ToList();
                
                await context.RespondAsync(new GetMessagesResponse()
                {
                    Packet = new Packet {Status = "success", Messages = messages}
                });
            }
        }

        public async Task Consume(ConsumeContext<CreateTextMessageRequest> context)
        {
            using (var dbContext = new DatabaseContext())
            {
                var packet = context.Message.Packet;
                var session = dbContext.Sessions.Find(context.Message.SessionId);

                dbContext.Entry(session).Reference(s => s.BaseUser).Load();
                var human = (User) session.BaseUser;
                dbContext.Entry(human).Collection(h => h.Memberships).Load();
                var membership = human.Memberships.Find(m => m.ComplexId == packet.Complex.ComplexId);
                if (membership == null)
                {
                    await context.RespondAsync(new CreateTextMessageResponse()
                    {
                        Packet = new Packet {Status = "error_1"}
                    });
                    return;
                }
                dbContext.Entry(membership).Reference(m => m.Complex).Load();
                var complex = membership.Complex;
                dbContext.Entry(complex).Collection(c => c.Rooms).Load();
                var room = complex.Rooms.Find(r => r.RoomId == packet.Room.RoomId);
                if (room == null)
                {
                    await context.RespondAsync(new CreateTextMessageResponse()
                    {
                        Packet = new Packet {Status = "error_2"}
                    });
                    return;
                }
                var message = new TextMessage()
                {
                    Author = human,
                    Room = room,
                    Text = packet.TextMessage.Text,
                    Time = Convert.ToInt64((DateTime.Now - DateTime.MinValue).TotalMilliseconds)
                };
                dbContext.Messages.Add(message);
                dbContext.SaveChanges();
                dbContext.Entry(human).Collection(h => h.Sessions).Load();
                dbContext.Entry(complex)
                    .Collection(c => c.Members)
                    .Query().Include(m => m.User)
                    .ThenInclude(u => u.Sessions)
                    .Load();
                dbContext.Entry(room).Collection(r => r.Workers).Load();
                TextMessage nextMessage;
                using (var nextContext = new DatabaseContext())
                {
                    nextMessage = (TextMessage) nextContext.Messages.Find(message.MessageId);
                    nextContext.Entry(nextMessage).Reference(m => m.Room).Load();
                    nextContext.Entry(nextMessage.Room).Reference(r => r.Complex).Load();
                    nextContext.Entry(nextMessage).Reference(m => m.Author).Load();
                }
                var sessionIds = (from m in complex.Members from s in m.User.Sessions 
                        select s.SessionId).ToList();
                sessionIds.AddRange((from w in room.Workers from s in dbContext.Bots.Find(w.BotId).Sessions
                        select s.SessionId).ToList());
                var mcn = new TextMessageNotification()
                {
                    Message = nextMessage
                };
                SharedArea.Transport.Push<TextMessagePush>(
                    Program.Bus,
                    new TextMessagePush()
                    {
                        Notif = mcn,
                        SessionIds = sessionIds
                    });

                await context.RespondAsync(new CreateTextMessageResponse()
                {
                    Packet = new Packet {Status = "success", TextMessage = nextMessage}
                });
            }
        }

        public async Task Consume(ConsumeContext<CreateFileMessageRequest> context)
        {
            using (var dbContext = new DatabaseContext())
            {
                var packet = context.Message.Packet;
                var session = dbContext.Sessions.Find(context.Message.SessionId);

                dbContext.Entry(session).Reference(s => s.BaseUser).Load();
                var human = (User) session.BaseUser;
                dbContext.Entry(human).Collection(h => h.Memberships).Load();
                var membership = human.Memberships.Find(mem => mem.ComplexId == packet.Complex.ComplexId);
                if (membership == null)
                {
                    await context.RespondAsync(new CreateFileMessageResponse()
                    {
                        Packet = new Packet {Status = "error_1"}
                    });
                    return;
                }
                dbContext.Entry(membership).Reference(mem => mem.Complex).Load();
                var complex = membership.Complex;
                dbContext.Entry(complex).Collection(c => c.Rooms).Load();
                var room = complex.Rooms.Find(r => r.RoomId == packet.Room.RoomId);
                if (room == null)
                {
                    await context.RespondAsync(new CreateFileMessageResponse()
                    {
                        Packet = new Packet {Status = "error_2"}
                    });
                    return;
                }
                Message message = null;
                dbContext.Entry(room).Collection(r => r.Files).Load();
                var fileUsage = room.Files.Find(fu => fu.FileId == packet.File.FileId);
                if (fileUsage == null)
                {
                    await context.RespondAsync(new CreateFileMessageResponse()
                    {
                        Packet = new Packet {Status = "error_3"}
                    });
                    return;
                }
                dbContext.Entry(fileUsage).Reference(fu => fu.File).Load();
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

                if (message == null)
                {
                    await context.RespondAsync(new CreateFileMessageResponse()
                    {
                        Packet = new Packet {Status = "error_4"}
                    });
                    return;
                }
                dbContext.Messages.Add(message);
                dbContext.SaveChanges();
                
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
                    
                dbContext.Entry(complex)
                    .Collection(c => c.Members)
                    .Query().Include(m => m.User)
                    .ThenInclude(u => u.Sessions)
                    .Load();
                dbContext.Entry(room).Collection(r => r.Workers).Load();
                var sessionIds = (from m in complex.Members from s in m.User.Sessions 
                    select s.SessionId).ToList();
                sessionIds.AddRange((from w in room.Workers from s in dbContext.Bots.Find(w.BotId).Sessions
                    select s.SessionId).ToList());
                
                switch (nextMessage)
                        {
                            case PhotoMessage msg:
                            {
                                var notif = new PhotoMessageNotification()
                                {
                                    Message = msg,
                                };
                                SharedArea.Transport.Push<PhotoMessagePush>(
                                    Program.Bus,
                                    new PhotoMessagePush()
                                    {
                                        Notif = notif,
                                        SessionIds = sessionIds
                                    });
                                break;
                            }
                            case AudioMessage msg:
                            {
                                var notif = new AudioMessageNotification()
                                {
                                    Message = msg
                                };
                                SharedArea.Transport.Push<AudioMessagePush>(
                                    Program.Bus,
                                    new AudioMessagePush()
                                    {
                                        Notif = notif,
                                        SessionIds = sessionIds
                                    });
                                break;
                            }
                            case VideoMessage msg:
                            {
                                var notif = new VideoMessageNotification()
                                {
                                    Message = msg
                                };
                                SharedArea.Transport.Push<VideoMessagePush>(
                                    Program.Bus,
                                    new VideoMessagePush()
                                    {
                                        Notif = notif,
                                        SessionIds = sessionIds
                                    });
                                break;
                            }
                        }

                switch (nextMessage)
                {
                    case PhotoMessage msg:
                        await context.RespondAsync(new CreateFileMessageResponse()
                        {
                            Packet = new Packet {Status = "success", PhotoMessage = msg}
                        });
                        break;
                    case AudioMessage msg:
                        await context.RespondAsync(new CreateFileMessageResponse()
                        {
                            Packet = new Packet {Status = "success", AudioMessage = msg}
                        });
                        break;
                    case VideoMessage msg:
                        await context.RespondAsync(new CreateFileMessageResponse()
                        {
                            Packet = new Packet {Status = "success", VideoMessage = msg}
                        });
                        break;
                    default:
                        await context.RespondAsync(new CreateFileMessageResponse()
                        {
                            Packet = new Packet {Status = "error_5"}
                        });
                        break;
                }
            }
        }

        public async Task Consume(ConsumeContext<BotCreateTextMessageRequest> context)
        {
            using (var dbContext = new DatabaseContext())
            {
                var packet = context.Message.Packet;
                var authHeader = context.Message.Headers[AuthExtracter.AK];
                var parts = authHeader.Split(" ");
                var botId = long.Parse(parts[0]);
                var bot = dbContext.Bots.Find(botId);
                dbContext.Entry(bot).Reference(b => b.BotSecret).Load();
                var complex = dbContext.Complexes.Find(packet.Complex.ComplexId);
                if (complex == null)
                {
                    await context.RespondAsync(new BotCreateTextMessageResponse()
                    {
                        Packet = new Packet {Status = "error_3"}
                    });
                    return;
                }
                dbContext.Entry(complex).Collection(c => c.Rooms).Load();
                var room = complex.Rooms.Find(r => r.RoomId == packet.Room.RoomId);
                if (room == null)
                {
                    await context.RespondAsync(new BotCreateTextMessageResponse()
                    {
                        Packet = new Packet {Status = "error_4"}
                    });
                    return;
                }
                dbContext.Entry(room).Collection(r => r.Workers).Load();
                var workership = room.Workers.Find(w => w.BotId == botId);
                if (workership == null)
                {
                    await context.RespondAsync(new BotCreateTextMessageResponse()
                    {
                        Packet = new Packet {Status = "error_5"}
                    });
                    return;
                }
                var message = new TextMessage()
                {
                    Author = bot,
                    Room = room,
                    Text = packet.TextMessage.Text,
                    Time = Convert.ToInt64((DateTime.Now - DateTime.MinValue).TotalMilliseconds)
                };
                dbContext.Messages.Add(message);
                dbContext.SaveChanges();
                TextMessage nextMessage;
                using (var nextContext = new DatabaseContext())
                {
                    nextMessage = (TextMessage) nextContext.Messages.Find(message.MessageId);
                    nextContext.Entry(nextMessage).Reference(m => m.Room).Load();
                    nextContext.Entry(nextMessage.Room).Reference(r => r.Complex).Load();
                    nextContext.Entry(nextMessage).Reference(m => m.Author).Load();
                }
                dbContext.Entry(complex)
                    .Collection(c => c.Members)
                    .Query().Include(m => m.User)
                    .ThenInclude(u => u.Sessions)
                    .Load();
                dbContext.Entry(room).Collection(r => r.Workers).Load();
                
                var sessionIds = (from m in complex.Members from s in m.User.Sessions 
                    select s.SessionId).ToList();
                sessionIds.AddRange((from w in room.Workers from s in dbContext.Bots.Find(w.BotId).Sessions
                    select s.SessionId).ToList());
                var mcn = new TextMessageNotification()
                {
                    Message = nextMessage
                };
                SharedArea.Transport.Push<TextMessagePush>(
                    Program.Bus,
                    new TextMessagePush()
                    {
                        Notif = mcn,
                        SessionIds = sessionIds
                    });
                
                await context.RespondAsync(new BotCreateTextMessageResponse()
                {
                    Packet = new Packet {Status = "success", TextMessage = nextMessage}
                });
            }
        }

        public async Task Consume(ConsumeContext<BotCreateFileMessageRequest> context)
        {
            using (var dbContext = new DatabaseContext())
            {
                var packet = context.Message.Packet;
                var authHeader = context.Message.Headers[AuthExtracter.AK];
                var parts = authHeader.Split(" ");
                var botId = long.Parse(parts[0]);
                var bot = dbContext.Bots.Find(botId);
                dbContext.Entry(bot).Reference(b => b.BotSecret).Load();
                var complex = dbContext.Complexes.Find(packet.Complex.ComplexId);
                if (complex == null)
                {
                    await context.RespondAsync(new BotCreateFileMessageResponse()
                    {
                        Packet = new Packet {Status = "error_3"}
                    });
                    return;
                }
                dbContext.Entry(complex).Collection(c => c.Rooms).Load();
                var room = complex.Rooms.Find(r => r.RoomId == packet.Room.RoomId);
                if (room == null)
                {
                    await context.RespondAsync(new BotCreateFileMessageResponse()
                    {
                        Packet = new Packet {Status = "error_4"}
                    });
                    return;
                }
                dbContext.Entry(room).Collection(r => r.Workers).Load();
                var workership = room.Workers.Find(w => w.BotId == botId);
                if (workership == null)
                {
                    await context.RespondAsync(new BotCreateFileMessageResponse()
                    {
                        Packet = new Packet {Status = "error_5"}
                    });
                    return;
                }
                Message message = null;
                dbContext.Entry(room).Collection(r => r.Files).Load();
                var fileUsage = room.Files.Find(fu => fu.FileId == packet.File.FileId);
                if (fileUsage == null)
                {
                    await context.RespondAsync(new BotCreateFileMessageResponse()
                    {
                        Packet = new Packet {Status = "error_6"}
                    });
                    return;
                }
                dbContext.Entry(fileUsage).Reference(fu => fu.File).Load();
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

                if (message == null)
                {
                    await context.RespondAsync(new BotCreateFileMessageResponse()
                    {
                        Packet = new Packet {Status = "error_7"}
                    });
                    return;
                }
                dbContext.Messages.Add(message);
                dbContext.SaveChanges();
                
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
                
                dbContext.Entry(complex)
                    .Collection(c => c.Members)
                    .Query().Include(m => m.User)
                    .ThenInclude(u => u.Sessions)
                    .Load();
                dbContext.Entry(room).Collection(r => r.Workers).Load();
             
                var sessionIds = (from m in complex.Members from s in m.User.Sessions 
                    select s.SessionId).ToList();
                sessionIds.AddRange((from w in room.Workers from s in dbContext.Bots.Find(w.BotId).Sessions
                    select s.SessionId).ToList());
                
                switch (nextMessage)
                        {
                            case PhotoMessage msg:
                            {
                                var notif = new PhotoMessageNotification()
                                {
                                    Message = msg
                                };
                                SharedArea.Transport.Push<PhotoMessagePush>(
                                    Program.Bus,
                                    new PhotoMessagePush()
                                    {
                                        Notif = notif,
                                        SessionIds = sessionIds
                                    });
                                break;
                            }
                            case AudioMessage msg:
                            {
                                var notif = new AudioMessageNotification()
                                {
                                    Message = msg
                                };
                                SharedArea.Transport.Push<AudioMessagePush>(
                                    Program.Bus,
                                    new AudioMessagePush()
                                    {
                                        Notif = notif,
                                        SessionIds = sessionIds
                                    });
                                break;
                            }
                            case VideoMessage msg:
                            {
                                var notif = new VideoMessageNotification()
                                {
                                    Message = msg
                                };
                                SharedArea.Transport.Push<VideoMessagePush>(
                                    Program.Bus,
                                    new VideoMessagePush()
                                    {
                                        Notif = notif,
                                        SessionIds = sessionIds
                                    });
                                break;
                            }
                        }

                switch (nextMessage)
                {
                    case PhotoMessage msg:
                        await context.RespondAsync(new BotCreateFileMessageResponse()
                        {
                            Packet = new Packet {Status = "success", PhotoMessage = msg}
                        });
                        return;
                    case AudioMessage msg:
                        await context.RespondAsync(new BotCreateFileMessageResponse()
                        {
                            Packet = new Packet {Status = "success", AudioMessage = msg}
                        });
                        return;
                    case VideoMessage msg:
                        await context.RespondAsync(new BotCreateFileMessageResponse()
                        {
                            Packet = new Packet {Status = "success", VideoMessage = msg}
                        });
                        return;
                    default:
                        await context.RespondAsync(new BotCreateFileMessageResponse()
                        {
                            Packet = new Packet {Status = "error_5"}
                        });
                        return;
                }
            }
        }

        public Task Consume(ConsumeContext<PhotoCreatedNotif> context)
        {
            using (var dbContext = new DatabaseContext())
            {
                var photo = context.Message.Packet.Photo;

                dbContext.Files.Add(photo);

                dbContext.SaveChanges();
            }
            
            return Task.CompletedTask;
        }

        public Task Consume(ConsumeContext<AudioCreatedNotif> context)
        {
            using (var dbContext = new DatabaseContext())
            {
                var audio = context.Message.Packet.Audio;

                dbContext.Files.Add(audio);

                dbContext.SaveChanges();
            }
            
            return Task.CompletedTask;
        }

        public Task Consume(ConsumeContext<VideoCreatedNotif> context)
        {
            using (var dbContext = new DatabaseContext())
            {
                var video = context.Message.Packet.Video;

                dbContext.Files.Add(video);

                dbContext.SaveChanges();
            }
            
            return Task.CompletedTask;
        }
    }
}