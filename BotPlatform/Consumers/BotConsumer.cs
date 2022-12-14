using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using BotPlatform.DbContexts;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using SharedArea.Commands.Internal.Notifications;
using SharedArea.Commands.Internal.Requests;
using SharedArea.Commands.Pulse;
using SharedArea.Commands.Pushes;
using SharedArea.Entities;
using SharedArea.Middles;
using SharedArea.Notifications;

namespace BotPlatform.Consumers
{
    public class BotConsumer : IConsumer<BotSubscribedNotif>, IConsumer<BotCreatedNotif>, IConsumer<WorkershipCreatedNotif>
        , IConsumer<WorkershipUpdatedNotif>, IConsumer<WorkershipDeletedNotif>, IConsumer<RequestBotViewRequest>
        , IConsumer<SendBotViewRequest>, IConsumer<UpdateBotViewRequest>, IConsumer<AnimateBotViewRequest>
        , IConsumer<RunCommandsOnBotViewRequest>, IConsumer<BotProfileUpdatedNotif>, IConsumer<ConsolidateDeleteAccountRequest>
    {
        public async Task Consume(ConsumeContext<RequestBotViewRequest> context)
        {
            using (var dbContext = new DatabaseContext())
            {
                try
                {
                    var packet = context.Message.Packet;
                    var session = dbContext.Sessions.Find(context.Message.SessionId);

                    dbContext.Entry(session).Reference(s => s.BaseUser).Load();
                    var user = (User) session.BaseUser;

                    var complexId = packet.Complex.ComplexId;
                    var roomId = packet.Room.RoomId;
                    var botId = packet.Bot.BaseUserId;
                    var sessionId = context.Message.SessionId;

                    dbContext.Entry(user).Collection(u => u.Memberships).Load();
                    var mem = user.Memberships.Find(m => m.ComplexId == complexId);
                    if (mem == null)
                    {
                        await context.RespondAsync(new RequestBotViewResponse()
                        {
                            Packet = new Packet() {Status = "error_1"}
                        });
                        return;
                    }

                    dbContext.Entry(mem).Reference(m => m.Complex).Load();
                    var complex = mem.Complex;
                    if (complex == null)
                    {
                        await context.RespondAsync(new RequestBotViewResponse()
                        {
                            Packet = new Packet() {Status = "error_2"}
                        });
                        return;
                    }

                    dbContext.Entry(complex).Collection(c => c.Rooms).Load();
                    var room = complex.Rooms.Find(r => r.RoomId == roomId);
                    if (room == null)
                    {
                        await context.RespondAsync(new RequestBotViewResponse()
                        {
                            Packet = new Packet() {Status = "error_3"}
                        });
                        return;
                    }

                    dbContext.Entry(room).Collection(r => r.Workers).Load();
                    var worker = room.Workers.Find(w => w.BotId == botId);
                    if (worker == null)
                    {
                        await context.RespondAsync(new RequestBotViewResponse()
                        {
                            Packet = new Packet() {Status = "error_4"}
                        });
                        return;
                    }

                    var bot = dbContext.Bots.Find(botId);
                    dbContext.Entry(bot).Collection(b => b.Sessions).Load();

                    User finalUser;
                    using (var finalContext = new DatabaseContext())
                    {
                        finalUser = (User) finalContext.BaseUsers.Find(user.BaseUserId);
                    }

                    var notif = new UserRequestedBotViewNotification()
                    {
                        ComplexId = complexId,
                        RoomId = roomId,
                        BotId = botId,
                        User = finalUser
                    };

                    SharedArea.Transport.Push<UserRequestedBotViewPush>(
                        Program.Bus,
                        new UserRequestedBotViewPush()
                        {
                            Notif = notif,
                            SessionIds = new List<long> {bot.Sessions.FirstOrDefault().SessionId}
                        });

                    await context.RespondAsync(new RequestBotViewResponse()
                    {
                        Packet = new Packet() {Status = "success"}
                    });
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                }
            }
        }

        public async Task Consume(ConsumeContext<SendBotViewRequest> context)
        {
            using (var dbContext = new DatabaseContext())
            {
                var packet = context.Message.Packet;
                var session = dbContext.Sessions.Find(context.Message.SessionId);

                var complexId = packet.Complex.ComplexId;
                var roomId = packet.Room.RoomId;
                var userId = packet.User.BaseUserId;
                var viewData = packet.RawJson;

                dbContext.Entry(session).Reference(s => s.BaseUser).Load();
                var bot = (Bot) session.BaseUser;
                
                var complex = dbContext.Complexes.Find(complexId);
                if (complex == null)
                {
                    await context.RespondAsync(new SendBotViewResponse()
                    {
                        Packet = new Packet() {Status = "error_1"}
                    });
                    return;
                }
                dbContext.Entry(complex).Collection(c => c.Rooms).Load();
                var room = complex.Rooms.Find(r => r.RoomId == roomId);
                if (room == null)
                {
                    await context.RespondAsync(new SendBotViewResponse()
                    {
                        Packet = new Packet() {Status = "error_2"}
                    });
                    return;
                }
                dbContext.Entry(room).Collection(r => r.Workers).Load();
                var worker = room.Workers.Find(w => w.BotId == bot.BaseUserId);
                if (worker == null)
                {
                    await context.RespondAsync(new SendBotViewResponse()
                    {
                        Packet = new Packet() {Status = "error_3"}
                    });
                    return;
                }

                User user = null;
                if (userId > 0)
                {
                    dbContext.Entry(complex).Collection(c => c.Members).Load();
                    user = (User) dbContext.BaseUsers.Find(userId);
                    dbContext.Entry(user).Collection(u => u.Sessions).Load();
                    var membership = complex.Members.Find(m => m.UserId == user.BaseUserId);
                    if (membership == null)
                    {
                        await context.RespondAsync(new UpdateBotViewResponse()
                        {
                            Packet = new Packet() {Status = "error_4"}
                        });
                        return;
                    }
                }
                else
                {
                    dbContext.Entry(complex).Collection(c => c.Members).Query()
                        .Include(m => m.User).ThenInclude(u => u.Sessions).Load();  
                }
                
                var notif = new BotSentBotViewNotification()
                {
                    ComplexId = complexId,
                    RoomId = roomId,
                    BotId = bot.BaseUserId,
                    ViewData = viewData
                };

                var sessionIds = user == null ? (from m in complex.Members from s in m.User.Sessions 
                    select s.SessionId).ToList() : user.Sessions.Select(s => s.SessionId).ToList();
                
                SharedArea.Transport.Push<BotSentBotViewPush>(
                    Program.Bus,
                    new BotSentBotViewPush()
                    {
                        SessionIds = sessionIds,
                        Notif = notif
                    });

                await context.RespondAsync(new SendBotViewResponse()
                {
                    Packet = new Packet() {Status = "success"}
                });
            }
        }
        
        public async Task Consume(ConsumeContext<UpdateBotViewRequest> context)
        {
            using (var dbContext = new DatabaseContext())
            {
                var packet = context.Message.Packet;
                var session = dbContext.Sessions.Find(context.Message.SessionId);

                var complexId = packet.Complex.ComplexId;
                var roomId = packet.Room.RoomId;
                var userId = packet.User.BaseUserId;
                var viewData = packet.RawJson;
                var batchData = packet.BatchData;

                dbContext.Entry(session).Reference(s => s.BaseUser).Load();
                var bot = (Bot) session.BaseUser;

                var complex = dbContext.Complexes.Find(complexId);
                if (complex == null)
                {
                    await context.RespondAsync(new UpdateBotViewResponse()
                    {
                        Packet = new Packet() {Status = "error_1"}
                    });
                    return;
                }
                dbContext.Entry(complex).Collection(c => c.Rooms).Load();
                var room = complex.Rooms.Find(r => r.RoomId == roomId);
                if (room == null)
                {
                    await context.RespondAsync(new UpdateBotViewResponse()
                    {
                        Packet = new Packet() {Status = "error_2"}
                    });
                    return;
                }
                dbContext.Entry(room).Collection(r => r.Workers).Load();
                var worker = room.Workers.Find(w => w.BotId == bot.BaseUserId);
                if (worker == null)
                {
                    await context.RespondAsync(new UpdateBotViewResponse()
                    {
                        Packet = new Packet() {Status = "error_3"}
                    });
                    return;
                }

                User user = null;
                if (userId > 0)
                {
                    dbContext.Entry(complex).Collection(c => c.Members).Load();
                    user = (User) dbContext.BaseUsers.Find(userId);
                    dbContext.Entry(user).Collection(u => u.Sessions).Load();
                    var membership = complex.Members.Find(m => m.UserId == user.BaseUserId);
                    if (membership == null)
                    {
                        await context.RespondAsync(new UpdateBotViewResponse()
                        {
                            Packet = new Packet() {Status = "error_4"}
                        });
                        return;
                    }
                }
                else
                {
                    dbContext.Entry(complex).Collection(c => c.Members).Query()
                        .Include(m => m.User).ThenInclude(u => u.Sessions).Load();     
                }

                var notif = new BotUpdatedBotViewNotification()
                {
                    ComplexId = complexId,
                    RoomId = roomId,
                    BotId = bot.BaseUserId,
                    UpdateData = viewData,
                    BatchData = batchData ?? false
                };
                
                var sessionIds = user == null ? (from m in complex.Members from s in m.User.Sessions 
                    select s.SessionId).ToList() : user.Sessions.Select(s => s.SessionId).ToList();
                
                SharedArea.Transport.Push<BotUpdatedBotViewPush>(
                    Program.Bus,
                    new BotUpdatedBotViewPush()
                    {
                        SessionIds = sessionIds,
                        Notif = notif
                    });

                await context.RespondAsync(new UpdateBotViewResponse()
                {
                    Packet = new Packet() {Status = "success"}
                });
            }
        }

        public async Task Consume(ConsumeContext<AnimateBotViewRequest> context)
        {
            using (var dbContext = new DatabaseContext())
            {
                var packet = context.Message.Packet;
                var session = dbContext.Sessions.Find(context.Message.SessionId);

                var complexId = packet.Complex.ComplexId;
                var roomId = packet.Room.RoomId;
                var userId = packet.User.BaseUserId;
                var viewData = packet.RawJson;
                var batchData = packet.BatchData;

                dbContext.Entry(session).Reference(s => s.BaseUser).Load();
                var bot = (Bot) session.BaseUser;

                var complex = dbContext.Complexes.Find(complexId);
                if (complex == null)
                {
                    await context.RespondAsync(new AnimateBotViewResponse()
                    {
                        Packet = new Packet() {Status = "error_1"}
                    });
                    return;
                }
                dbContext.Entry(complex).Collection(c => c.Rooms).Load();
                var room = complex.Rooms.Find(r => r.RoomId == roomId);
                if (room == null)
                {
                    await context.RespondAsync(new AnimateBotViewResponse()
                    {
                        Packet = new Packet() {Status = "error_2"}
                    });
                    return;
                }
                dbContext.Entry(room).Collection(r => r.Workers).Load();
                var worker = room.Workers.Find(w => w.BotId == bot.BaseUserId);
                if (worker == null)
                {
                    await context.RespondAsync(new AnimateBotViewResponse()
                    {
                        Packet = new Packet() {Status = "error_3"}
                    });
                    return;
                }

                User user = null;
                if (userId > 0)
                {
                    dbContext.Entry(complex).Collection(c => c.Members).Load();
                    user = (User) dbContext.BaseUsers.Find(userId);
                    dbContext.Entry(user).Collection(u => u.Sessions).Load();
                    var membership = complex.Members.Find(m => m.UserId == user.BaseUserId);
                    if (membership == null)
                    {
                        await context.RespondAsync(new UpdateBotViewResponse()
                        {
                            Packet = new Packet() {Status = "error_4"}
                        });
                        return;
                    }
                }
                else
                {
                    dbContext.Entry(complex).Collection(c => c.Members).Query()
                        .Include(m => m.User).ThenInclude(u => u.Sessions).Load();  
                }

                var notif = new BotAnimatedBotViewNotification()
                {
                    ComplexId = complexId,
                    RoomId = roomId,
                    BotId = bot.BaseUserId,
                    AnimData = viewData,
                    BatchData = batchData ?? false
                };
                
                var sessionIds = user == null ? (from m in complex.Members from s in m.User.Sessions 
                    select s.SessionId).ToList() : user.Sessions.Select(s => s.SessionId).ToList();
                
                SharedArea.Transport.Push<BotAnimatedBotViewPush>(
                    Program.Bus,
                    new BotAnimatedBotViewPush()
                    {
                        SessionIds = sessionIds,
                        Notif = notif
                    });

                await context.RespondAsync(new AnimateBotViewResponse()
                {
                    Packet = new Packet() {Status = "success"}
                });
            }
        }

        public async Task Consume(ConsumeContext<RunCommandsOnBotViewRequest> context)
        {
            using (var dbContext = new DatabaseContext())
            {
                var packet = context.Message.Packet;
                var session = dbContext.Sessions.Find(context.Message.SessionId);

                var complexId = packet.Complex.ComplexId;
                var roomId = packet.Room.RoomId;
                var userId = packet.User.BaseUserId;
                var viewData = packet.RawJson;
                var batchData = packet.BatchData;

                dbContext.Entry(session).Reference(s => s.BaseUser).Load();
                var bot = (Bot) session.BaseUser;

                var complex = dbContext.Complexes.Find(complexId);
                if (complex == null)
                {
                    await context.RespondAsync(new RunCommandsOnBotViewResponse()
                    {
                        Packet = new Packet() {Status = "error_1"}
                    });
                    return;
                }
                dbContext.Entry(complex).Collection(c => c.Rooms).Load();
                var room = complex.Rooms.Find(r => r.RoomId == roomId);
                if (room == null)
                {
                    await context.RespondAsync(new RunCommandsOnBotViewResponse()
                    {
                        Packet = new Packet() {Status = "error_2"}
                    });
                    return;
                }
                dbContext.Entry(room).Collection(r => r.Workers).Load();
                var worker = room.Workers.Find(w => w.BotId == bot.BaseUserId);
                if (worker == null)
                {
                    await context.RespondAsync(new RunCommandsOnBotViewResponse()
                    {
                        Packet = new Packet() {Status = "error_3"}
                    });
                    return;
                }

                User user = null;
                if (userId > 0)
                {
                    dbContext.Entry(complex).Collection(c => c.Members).Load();
                    user = (User) dbContext.BaseUsers.Find(userId);
                    dbContext.Entry(user).Collection(u => u.Sessions).Load();
                    var membership = complex.Members.Find(m => m.UserId == user.BaseUserId);
                    if (membership == null)
                    {
                        await context.RespondAsync(new UpdateBotViewResponse()
                        {
                            Packet = new Packet() {Status = "error_4"}
                        });
                        return;
                    }
                }
                else
                {
                    dbContext.Entry(complex).Collection(c => c.Members).Query()
                        .Include(m => m.User).ThenInclude(u => u.Sessions).Load();  
                }

                var notif = new BotRanCommandsOnBotViewNotification()
                {
                    ComplexId = complexId,
                    RoomId = roomId,
                    BotId = bot.BaseUserId,
                    CommandsData = viewData,
                    BatchData = batchData ?? false
                };

                var sessionIds = user == null
                    ? (from m in complex.Members
                        from s in m.User.Sessions
                        select s.SessionId).ToList()
                    : user.Sessions.Select(s => s.SessionId).ToList();
                
                SharedArea.Transport.Push<BotRanCommandsOnBotViewPush>(
                    Program.Bus,
                    new BotRanCommandsOnBotViewPush()
                    {
                        SessionIds = sessionIds,
                        Notif = notif
                    });

                await context.RespondAsync(new RunCommandsOnBotViewResponse()
                {
                    Packet = new Packet() {Status = "success"}
                });
            }
        }
        
        public async Task Consume(ConsumeContext<BotSubscribedNotif> context)
        {
            using (var dbContext = new DatabaseContext())
            {
                var subscription = context.Message.Packet.BotSubscription;
                var globalBot = context.Message.Packet.Bot;
                var globalUser = context.Message.Packet.User;

                var localUser = dbContext.Users.Find(globalUser.BaseUserId);
                var localBot = dbContext.Bots.Find(globalBot.BaseUserId);

                subscription.Bot = localBot;
                subscription.Subscriber = localUser;

                dbContext.BotSubscriptions.Add(subscription);

                dbContext.SaveChanges();
            }

            await context.RespondAsync(new BotSubscribedNotifResponse());
        }
        
        public async Task Consume(ConsumeContext<BotCreatedNotif> context)
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

            await context.RespondAsync(new BotCreatedNotifResponse());
        }

        public async Task Consume(ConsumeContext<WorkershipCreatedNotif> context)
        {
            using (var dbContext = new DatabaseContext())
            {
                var workership = context.Message.Packet.Workership;
                workership.Room = dbContext.Rooms.Find(workership.Room.RoomId);

                dbContext.Workerships.Add(workership);

                dbContext.SaveChanges();
            }
            
            await context.RespondAsync(new WorkershipCreatedNotifResponse());
        }

        public async Task Consume(ConsumeContext<WorkershipUpdatedNotif> context)
        {
            using (var dbContext = new DatabaseContext())
            {
                var globalWs = context.Message.Packet.Workership;

                var localWs = dbContext.Workerships.Find(globalWs.WorkershipId);

                localWs.PosX = globalWs.PosX;
                localWs.PosY = globalWs.PosY;
                localWs.Width = globalWs.Width;
                localWs.Height = globalWs.Height;

                dbContext.SaveChanges();
            }
            
            await context.RespondAsync(new WorkershipUpdatedNotifResponse());
        }

        public async Task Consume(ConsumeContext<WorkershipDeletedNotif> context)
        {
            using (var dbContext = new DatabaseContext())
            {
                var globalWs = context.Message.Packet.Workership;

                var localWs = dbContext.Workerships.Find(globalWs.WorkershipId);
                dbContext.Entry(localWs).Reference(ws => ws.Room).Load();
                var room = localWs.Room;

                room.Workers.Remove(localWs);
                dbContext.Workerships.Remove(localWs);

                dbContext.SaveChanges();
            }
            
            await context.RespondAsync(new WorkershipDeletedNotifResponse());
        }

        public async Task Consume(ConsumeContext<BotProfileUpdatedNotif> context)
        {
            using (var dbContext = new DatabaseContext())
            {
                var globalBot = context.Message.Packet.Bot;

                var localBot = dbContext.Bots.Find(globalBot.BaseUserId);

                localBot.Title = globalBot.Title;
                localBot.Avatar = globalBot.Avatar;
                localBot.Description = globalBot.Description;
                localBot.ViewURL = globalBot.ViewURL;

                dbContext.SaveChanges();
            }
            
            await context.RespondAsync(new BotProfileUpdatedNotifResponse());
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