using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MassTransit;
using MessengerPlatform.DbContexts;
using Microsoft.EntityFrameworkCore;
using SharedArea.Commands.Internal.Requests;
using SharedArea.Commands.Internal.Responses;
using SharedArea.Commands.Message;
using SharedArea.Commands.Pushes;
using SharedArea.Entities;
using SharedArea.Middles;
using SharedArea.Notifications;
using Message = SharedArea.Entities.Message;

namespace MessengerPlatform.Consumers
{
    public class MessengerConsumer : IConsumer<GetMessagesRequest>
        , IConsumer<CreateTextMessageRequest>, IConsumer<CreateFileMessageRequest>,
        IConsumer<BotCreateTextMessageRequest>
        , IConsumer<BotCreateFileMessageRequest>
        , IConsumer<PutServiceMessageRequest>, IConsumer<ConsolidateContactRequest>
        
        , IConsumer<ConsolidateDeleteAccountRequest>,
        IConsumer<NotifyMessageSeenRequest>, IConsumer<GetLastActionsRequest>
        , IConsumer<GetMessageSeenCountRequest>, IConsumer<ConsolidateMakeAccountRequest>,
        IConsumer<ConsolidateCreateRoomRequest>, IConsumer<ConsolidateCreateComplexRequest>
    {
        public async Task Consume(ConsumeContext<ConsolidateCreateComplexRequest> context)
        {
            using (var dbContext = new DatabaseContext())
            {
                var admin = (User) dbContext.BaseUsers.Find(context.Message.Packet.User.BaseUserId);
                var complex = context.Message.Packet.Complex;
                var complexSecret = context.Message.Packet.ComplexSecret;

                complex.ComplexSecret = complexSecret;
                complexSecret.Complex = complex;
                complex.Rooms[0].Complex = complex;
                complex.Members[0].Complex = complex;
                complexSecret.Admin = admin;
                complex.Members[0].User = admin;
                complex.Members[0].MemberAccess.Membership = complex.Members[0];

                dbContext.AddRange(complex);

                dbContext.SaveChanges();
            }

            await context.RespondAsync(new ConsolidateCreateComplexResponse());
        }

        public async Task Consume(ConsumeContext<GetMessagesRequest> context)
        {
            using (var dbContext = new DatabaseContext())
            {
                var packet = context.Message.Packet;
                if (packet.FetchNext == null)
                {
                    await context.RespondAsync(new GetMessagesResponse()
                    {
                        Packet = new Packet() {Status = "error_1"}
                    });
                    return;
                }
                
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

                var messages = packet.FetchNext.Value ?
                    room.Messages.Where(m => m.MessageId > packet.Message.MessageId && m.MessageId - packet.Message.MessageId <= 100).ToList() : 
                    room.Messages.Where(m => m.MessageId < packet.Message.MessageId && packet.Message.MessageId - m.MessageId <= 100).ToList();
                
                foreach (var msg in messages)
                {
                    if (msg.GetType() == typeof(PhotoMessage))
                    {
                        dbContext.Entry(msg).Reference(m => ((PhotoMessage) m).Photo).Load();
                        dbContext.Entry(((PhotoMessage) msg).Photo).Collection(f => f.FileUsages).Load();
                    }
                    else if (msg.GetType() == typeof(AudioMessage))
                    {
                        dbContext.Entry(msg).Reference(m => ((AudioMessage) m).Audio).Load();
                        dbContext.Entry(((AudioMessage) msg).Audio).Collection(f => f.FileUsages).Load();
                    }
                    else if (msg.GetType() == typeof(VideoMessage))
                    {
                        dbContext.Entry(msg).Reference(m => ((VideoMessage) m).Video).Load();
                        dbContext.Entry(((VideoMessage) msg).Video).Collection(f => f.FileUsages).Load();
                    }

                    msg.SeenByMe = (dbContext.MessageSeens.Find(user.BaseUserId + "_" + msg.MessageId) != null);
                }

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
                    Time = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
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

                var sessionIds = (from m in complex.Members
                    where m.User.BaseUserId != human.BaseUserId
                    from s in m.User.Sessions
                    select s.SessionId).ToList();
                sessionIds.AddRange((from w in room.Workers
                    from s in dbContext.Bots.Include(b => b.Sessions)
                        .FirstOrDefault(b => b.BaseUserId == w.BotId)
                        ?.Sessions
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
                            Time = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                            Photo = photo
                        };
                        break;
                    case Audio audio:
                        message = new AudioMessage()
                        {
                            Author = human,
                            Room = room,
                            Time = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                            Audio = audio
                        };
                        break;
                    case Video video:
                        message = new VideoMessage()
                        {
                            Author = human,
                            Room = room,
                            Time = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
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
                        case PhotoMessage _:
                            nextContext.Entry(nextMessage).Reference(msg => ((PhotoMessage) msg).Photo).Load();
                            nextContext.Entry(((PhotoMessage) nextMessage).Photo).Collection(f => f.FileUsages)
                                .Query().Where(fu => fu.FileUsageId == fileUsage.FileUsageId).Load();
                            break;
                        case AudioMessage _:
                            nextContext.Entry(nextMessage).Reference(msg => ((AudioMessage) msg).Audio).Load();
                            nextContext.Entry(((AudioMessage) nextMessage).Audio).Collection(f => f.FileUsages)
                                .Query().Where(fu => fu.FileUsageId == fileUsage.FileUsageId).Load();
                            break;
                        case VideoMessage _:
                            nextContext.Entry(nextMessage).Reference(msg => ((VideoMessage) msg).Video).Load();
                            nextContext.Entry(((VideoMessage) nextMessage).Video).Collection(f => f.FileUsages)
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
                var sessionIds = (from m in complex.Members
                    where m.User.BaseUserId != human.BaseUserId
                    from s in m.User.Sessions
                    select s.SessionId).ToList();
                sessionIds.AddRange((from w in room.Workers
                    from s in dbContext.Bots.Include(b => b.Sessions)
                        .FirstOrDefault(b => b.BaseUserId == w.BotId)
                        ?.Sessions
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
                var session = dbContext.Sessions.Find(context.Message.SessionId);
                dbContext.Entry(session).Reference(s => s.BaseUser).Load();
                var bot = (Bot) session.BaseUser;
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
                var workership = room.Workers.Find(w => w.BotId == bot.BaseUserId);
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
                    Time = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
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

                var sessionIds = (from m in complex.Members
                    from s in m.User.Sessions
                    select s.SessionId).ToList();
                sessionIds.AddRange((from w in room.Workers
                    where w.BotId != bot.BaseUserId
                    from s in dbContext.Bots.Include(b => b.Sessions)
                        .FirstOrDefault(b => b.BaseUserId == w.BotId)
                        ?.Sessions
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
                var session = dbContext.Sessions.Find(context.Message.SessionId);
                dbContext.Entry(session).Reference(s => s.BaseUser).Load();
                var bot = (Bot) session.BaseUser;
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
                var workership = room.Workers.Find(w => w.BotId == bot.BaseUserId);
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
                            Time = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                            Photo = photo
                        };
                        break;
                    case Audio audio:
                        message = new AudioMessage()
                        {
                            Author = bot,
                            Room = room,
                            Time = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                            Audio = audio
                        };
                        break;
                    case Video video:
                        message = new VideoMessage()
                        {
                            Author = bot,
                            Room = room,
                            Time = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
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
                        case PhotoMessage _:
                            nextContext.Entry(nextMessage).Reference(msg => ((PhotoMessage) msg).Photo).Load();
                            nextContext.Entry(((PhotoMessage) nextMessage).Photo).Collection(f => f.FileUsages)
                                .Query().Where(fu => fu.FileUsageId == fileUsage.FileUsageId).Load();
                            break;
                        case AudioMessage _:
                            nextContext.Entry(nextMessage).Reference(msg => ((AudioMessage) msg).Audio).Load();
                            nextContext.Entry(((AudioMessage) nextMessage).Audio).Collection(f => f.FileUsages)
                                .Query().Where(fu => fu.FileUsageId == fileUsage.FileUsageId).Load();
                            break;
                        case VideoMessage _:
                            nextContext.Entry(nextMessage).Reference(msg => ((VideoMessage) msg).Video).Load();
                            nextContext.Entry(((VideoMessage) nextMessage).Video).Collection(f => f.FileUsages)
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

                var sessionIds = (from m in complex.Members
                    from s in m.User.Sessions
                    select s.SessionId).ToList();
                sessionIds.AddRange((from w in room.Workers
                    where w.BotId != bot.BaseUserId
                    from s in dbContext.Bots.Include(b => b.Sessions)
                        .FirstOrDefault(b => b.BaseUserId == w.BotId)
                        ?.Sessions
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

        public async Task Consume(ConsumeContext<PutServiceMessageRequest> context)
        {
            using (var dbContext = new DatabaseContext())
            {
                var message = context.Message.Packet.ServiceMessage;

                var room = dbContext.Rooms.Find(message.Room.RoomId);

                message.Room = room;

                dbContext.Messages.Add(message);

                message = (ServiceMessage) dbContext.Messages.Find(message.MessageId);

                dbContext.SaveChanges();

                await context.RespondAsync(new PutServiceMessageResponse()
                {
                    Packet = new Packet() {ServiceMessage = message}
                });
            }
        }

        public async Task Consume(ConsumeContext<ConsolidateContactRequest> context)
        {
            using (var dbContext = new DatabaseContext())
            {
                var me = (User) dbContext.BaseUsers.Find(context.Message.Packet.Users[0].BaseUserId);
                var peer = (User) dbContext.BaseUsers.Find(context.Message.Packet.Users[1].BaseUserId);

                var complex = context.Message.Packet.Complex;
                var complexSecret = context.Message.Packet.ComplexSecret;
                var room = context.Message.Packet.Room;
                var m1 = context.Message.Packet.Memberships[0];
                var m2 = context.Message.Packet.Memberships[1];

                var lComplex = new Complex()
                {
                    ComplexId = complex.ComplexId,
                    Title = complex.Title,
                    Avatar = complex.Avatar,
                    Mode = complex.Mode,
                    ComplexSecret = new ComplexSecret()
                    {
                        ComplexSecretId = complexSecret.ComplexSecretId,
                        Admin = null
                    },
                    Rooms = new List<Room>()
                    {
                        new Room()
                        {
                            RoomId = room.RoomId,
                            Title = room.Title,
                            Avatar = room.Avatar
                        }
                    },
                    Members = new List<Membership>()
                    {
                        new Membership()
                        {
                            MembershipId = m1.MembershipId,
                            User = me
                        },
                        new Membership()
                        {
                            MembershipId = m2.MembershipId,
                            User = peer
                        }
                    }
                };

                dbContext.AddRange(lComplex);
                dbContext.SaveChanges();

                var myContact = context.Message.Packet.Contacts[0];
                myContact.Complex = lComplex;
                myContact.User = me;
                myContact.Peer = peer;
                dbContext.Contacts.Add(myContact);

                var peerContact = context.Message.Packet.Contacts[1];
                peerContact.Complex = lComplex;
                peerContact.User = peer;
                peerContact.Peer = me;
                dbContext.Contacts.Add(peerContact);

                dbContext.SaveChanges();
            }

            await context.RespondAsync(new ConsolidateContactResponse());
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

        public async Task Consume(ConsumeContext<NotifyMessageSeenRequest> context)
        {
            using (var dbContext = new DatabaseContext())
            {
                var session = dbContext.Sessions.Find(context.Message.SessionId);
                dbContext.Entry(session).Reference(s => s.BaseUser).Load();
                var user = (User) session.BaseUser;
                var message = dbContext.Messages.Find(context.Message.Packet.Message.MessageId);
                if (message == null)
                {
                    await context.RespondAsync(new NotifyMessageSeenResponse()
                    {
                        Packet = new Packet() {Status = "error_2"}
                    });
                    return;
                }

                var messageSeen = dbContext.MessageSeens.Find(user.BaseUserId + "_" + message.MessageId);
                if (messageSeen != null)
                {
                    await context.RespondAsync(new NotifyMessageSeenResponse()
                    {
                        Packet = new Packet() {Status = "error_3"}
                    });
                    return;
                }

                dbContext.Entry(message).Reference(m => m.Room).Load();
                var room = message.Room;
                dbContext.Entry(room).Reference(r => r.Complex).Load();
                var complex = room.Complex;
                dbContext.Entry(complex).Collection(c => c.Members).Load();
                if (complex.Members.Any(m => m.UserId == user.BaseUserId))
                {
                    if (message.AuthorId == user.BaseUserId)
                    {
                        await context.RespondAsync(new NotifyMessageSeenResponse()
                        {
                            Packet = new Packet() {Status = "error_5"}
                        });
                        return;
                    }

                    messageSeen = new MessageSeen()
                    {
                        MessageSeenId = user.BaseUserId + "_" + message.MessageId,
                        Message = message,
                        User = user
                    };
                    dbContext.MessageSeens.Add(messageSeen);

                    dbContext.SaveChanges();
                    var notif = new MessageSeenNotification()
                    {
                        MessageId = message.MessageId,
                        MessageSeenCount =
                            dbContext.MessageSeens.LongCount(ms => ms.MessageId == message.MessageId)
                    };
                    dbContext.Entry(complex)
                        .Collection(c => c.Members).Query()
                        .Include(m => m.User)
                        .ThenInclude(u => u.Sessions)
                        .Load();
                    var push = new MessageSeenPush()
                    {
                        Notif = notif,
                        SessionIds = (from m in complex.Members
                            where m.User.BaseUserId != user.BaseUserId
                            from s in m.User.Sessions
                            select s.SessionId).ToList()
                    };
                    SharedArea.Transport.Push<MessageSeenPush>(
                        Program.Bus,
                        push);

                    await context.RespondAsync(new NotifyMessageSeenResponse()
                    {
                        Packet = new Packet() {Status = "success"}
                    });
                }
                else
                {
                    await context.RespondAsync(new NotifyMessageSeenResponse()
                    {
                        Packet = new Packet() {Status = "error_4"}
                    });
                }
            }
        }

        public async Task Consume(ConsumeContext<GetMessageSeenCountRequest> context)
        {
            using (var dbContext = new DatabaseContext())
            {
                var session = dbContext.Sessions.Find(context.Message.SessionId);

                dbContext.Entry(session).Reference(s => s.BaseUser).Load();
                var user = (User) session.BaseUser;

                var message = dbContext.Messages.Find(context.Message.Packet.Message.MessageId);
                if (message == null)
                {
                    await context.RespondAsync(new NotifyMessageSeenResponse()
                    {
                        Packet = new Packet() {Status = "error_2"}
                    });
                    return;
                }

                dbContext.Entry(message).Reference(m => m.Room).Load();
                var room = message.Room;
                dbContext.Entry(room).Reference(r => r.Complex).Load();
                var complex = room.Complex;
                dbContext.Entry(complex).Collection(c => c.Members).Load();
                if (complex.Members.Any(m => m.UserId == user.BaseUserId))
                {
                    var seenCount = dbContext.MessageSeens.LongCount(ms => ms.MessageId == message.MessageId);

                    await context.RespondAsync(new NotifyMessageSeenResponse()
                    {
                        Packet = new Packet() {Status = "success", MessageSeenCount = seenCount}
                    });
                }
                else
                {
                    await context.RespondAsync(new NotifyMessageSeenResponse()
                    {
                        Packet = new Packet() {Status = "error_3"}
                    });
                }
            }
        }

        public async Task Consume(ConsumeContext<ConsolidateMakeAccountRequest> context)
        {
            using (var dbContext = new DatabaseContext())
            {
                var user = context.Message.Packet.User;
                var userSecret = context.Message.Packet.UserSecret;
                var complexSecret = context.Message.Packet.ComplexSecret;

                user.UserSecret = userSecret;
                userSecret.User = user;
                user.Memberships[0].User = user;
                user.Memberships[0].Complex.ComplexSecret = complexSecret;
                user.Memberships[0].Complex.ComplexSecret.Complex = user.Memberships[0].Complex;
                user.Memberships[0].Complex.ComplexSecret.Admin = user;
                user.Memberships[0].Complex.Rooms[0].Complex = user.Memberships[0].Complex;
                user.UserSecret.Home = user.Memberships[0].Complex;
                user.Memberships[0].User = user;
                user.Memberships[0].MemberAccess.Membership = user.Memberships[0];

                dbContext.AddRange(user);

                dbContext.SaveChanges();
            }

            await context.RespondAsync(new ConsolidateMakeAccountResponse());
        }

        public async Task Consume(ConsumeContext<ConsolidateCreateRoomRequest> context)
        {
            using (var dbContext = new DatabaseContext())
            {
                var room = context.Message.Packet.Room;
                var complex = dbContext.Complexes.Find(room.ComplexId);

                room.Complex = complex;

                dbContext.AddRange(room);

                dbContext.SaveChanges();
            }

            await context.RespondAsync(new ConsolidateCreateRoomResponse());
        }

        public async Task Consume(ConsumeContext<GetLastActionsRequest> context)
        {
            using (var dbContext = new DatabaseContext())
            {
                var rooms = context.Message.Packet.Rooms;
                
                var session = dbContext.Sessions.Find(context.Message.SessionId);
                dbContext.Entry(session).Reference(s => s.BaseUser).Load();
                var user = (User) session.BaseUser;
                dbContext.Entry(user).Collection(u => u.Memberships).Load();

                foreach (var roomC in rooms)
                {
                    var membership = user.Memberships.Find(mem => mem.ComplexId == roomC.ComplexId);
                    if (membership == null)
                    {
                        await context.RespondAsync(new GetLastActionsResponse()
                        {
                            Packet = new Packet() {Status = "error_1"}
                        });
                        return;
                    }

                    dbContext.Entry(membership).Reference(m => m.Complex).Load();
                    var complex = membership.Complex;
                    dbContext.Entry(complex).Collection(c => c.Rooms).Load();
                    var room = complex.Rooms.Find(r => r.RoomId == roomC.RoomId);
                    if (room == null)
                    {
                        await context.RespondAsync(new GetLastActionsResponse()
                        {
                            Packet = new Packet() {Status = "error_2"}
                        });
                        return;
                    }

                    roomC.LastAction = dbContext.Messages.Last(m => m.RoomId == room.RoomId);
                    
                    if (roomC.LastAction != null)
                    {
                        dbContext.Entry(roomC.LastAction).Reference(m => m.Author).Load();
                        
                        if (roomC.LastAction.GetType() == typeof(PhotoMessage))
                        {
                            dbContext.Entry(roomC.LastAction).Reference(m => ((PhotoMessage) m).Photo).Load();
                            dbContext.Entry(((PhotoMessage) roomC.LastAction).Photo).Collection(f => f.FileUsages)
                                .Query().Where(fu => fu.RoomId == roomC.RoomId).Load();
                        }
                        else if (roomC.LastAction.GetType() == typeof(AudioMessage))
                        {
                            dbContext.Entry(roomC.LastAction).Reference(m => ((AudioMessage) m).Audio).Load();
                            dbContext.Entry(((AudioMessage) roomC.LastAction).Audio).Collection(f => f.FileUsages)
                                .Query().Where(fu => fu.RoomId == roomC.RoomId).Load();
                        }
                        else if (roomC.LastAction.GetType() == typeof(VideoMessage))
                        {
                            dbContext.Entry(roomC.LastAction).Reference(m => ((VideoMessage) m).Video).Load();
                            dbContext.Entry(((VideoMessage) roomC.LastAction).Video).Collection(f => f.FileUsages)
                                .Query().Where(fu => fu.RoomId == roomC.RoomId).Load();
                        }
                        
                        roomC.LastAction.SeenByMe =
                            (dbContext.MessageSeens.Find(user.BaseUserId + "_" + roomC.LastAction.MessageId) != null);
                        roomC.LastAction.SeenCount =
                            dbContext.MessageSeens.LongCount(ms => ms.MessageId == roomC.LastAction.MessageId);
                    }
                }

                await context.RespondAsync(new GetLastActionsResponse()
                {
                    Packet = new Packet() {Status = "success", Rooms = rooms}
                });
            }
        }
    }
}