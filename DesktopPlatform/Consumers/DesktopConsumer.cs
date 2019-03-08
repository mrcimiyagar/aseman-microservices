using System.Linq;
using System.Threading.Tasks;
using DesktopPlatform.DbContexts;
using MassTransit;
using SharedArea.Commands.Bot;
using SharedArea.Commands.Internal.Notifications;
using SharedArea.Commands.Internal.Requests;
using SharedArea.Commands.Pushes;
using SharedArea.Entities;
using SharedArea.Middles;
using SharedArea.Notifications;

namespace DesktopPlatform.Consumers
{
    public class DesktopConsumer : IConsumer<AddBotToRoomRequest>, IConsumer<UpdateWorkershipRequest>
        , IConsumer<RemoveBotFromRoomRequest>, IConsumer<GetWorkershipsRequest>, IConsumer<BotSubscribedNotif>
        , IConsumer<BotCreatedNotif>, IConsumer<BotProfileUpdatedNotif>, IConsumer<ConsolidateDeleteAccountRequest>
    {
        public async Task Consume(ConsumeContext<AddBotToRoomRequest> context)
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
                    await context.RespondAsync(new AddBotToRoomResponse()
                    {
                        Packet = new Packet {Status = "error_2"}
                    });
                    return;
                }
                dbContext.Entry(membership).Reference(m => m.MemberAccess).Load();
                if (!membership.MemberAccess.CanModifyWorkers)
                {
                    await context.RespondAsync(new AddBotToRoomResponse()
                    {
                        Packet = new Packet() {Status = "error_3"}
                    });
                    return;
                }
                
                dbContext.Entry(membership).Reference(m => m.Complex).Load();
                var complex = membership.Complex;
                dbContext.Entry(complex).Collection(c => c.Rooms).Load();
                var room = complex.Rooms.Find(r => r.RoomId == packet.Room.RoomId);
                if (room == null)
                {
                    await context.RespondAsync(new AddBotToRoomResponse()
                    {
                        Packet = new Packet {Status = "error_4"}
                    });
                    return;
                }
                dbContext.Entry(room).Collection(r => r.Workers).Load();
                var workership = room.Workers.Find(w => w.BotId == packet.Bot.BaseUserId);
                if (workership != null)
                {
                    await context.RespondAsync(new AddBotToRoomResponse()
                    {
                        Packet = new Packet {Status = "error_1"}
                    });
                    return;
                }
                var bot = dbContext.Bots.Find(packet.Bot.BaseUserId);
                if (bot == null)
                {
                    await context.RespondAsync(new AddBotToRoomResponse()
                    {
                        Packet = new Packet {Status = "error_0"}
                    });
                    return;
                }
                workership = new Workership()
                {
                    BotId = bot.BaseUserId,
                    Room = room,
                    PosX = packet.Workership.PosX,
                    PosY = packet.Workership.PosY,
                    Width = packet.Workership.Width,
                    Height = packet.Workership.Height
                };
                dbContext.AddRange(workership);
                dbContext.SaveChanges();
                
                SharedArea.Transport.NotifyService<WorkershipCreatedNotif, WorkershipCreatedNotifResponse>(
                    Program.Bus,
                    new Packet() {Workership = workership},
                    new []
                    {
                        SharedArea.GlobalVariables.MESSENGER_QUEUE_NAME,
                        SharedArea.GlobalVariables.BOT_QUEUE_NAME
                    });
                
                Room finalRoom;
                using (var finalContext = new DatabaseContext())
                {
                    finalRoom = finalContext.Rooms.Find(room.RoomId);
                    finalContext.Entry(finalRoom).Reference(r => r.Complex).Load();
                }
                
                dbContext.Entry(bot).Collection(b => b.Sessions).Load();
                var botSess = bot.Sessions.FirstOrDefault();
                var addition = new BotAdditionToRoomNotification()
                {
                    Room = finalRoom,
                    Session = botSess
                };
                if (botSess != null)
                    SharedArea.Transport.Push<BotAdditionToRoomPush>(
                        Program.Bus,
                        new BotAdditionToRoomPush()
                        {
                            Notif = addition,
                            SessionIds = new[] {botSess.SessionId}.ToList()
                        });

                await context.RespondAsync(new AddBotToRoomResponse()
                {
                    Packet = new Packet
                    {
                        Status = "success",
                        Workership = workership
                    }
                });
            }
        }

        public async Task Consume(ConsumeContext<UpdateWorkershipRequest> context)
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
                    await context.RespondAsync(new UpdateWorkershipResponse()
                    {
                        Packet = new Packet {Status = "error_0"}
                    });
                    return;
                }
                dbContext.Entry(membership).Reference(m => m.Complex).Load();
                var complex = membership.Complex;
                dbContext.Entry(complex).Collection(c => c.Rooms).Load();
                var room = complex.Rooms.Find(r => r.RoomId == packet.Room.RoomId);
                if (room == null)
                {
                    await context.RespondAsync(new UpdateWorkershipResponse()
                    {
                        Packet = new Packet {Status = "error_2"}
                    });
                    return;
                }
                dbContext.Entry(room).Collection(r => r.Workers).Load();
                var workership = room.Workers.Find(w => w.BotId == packet.Bot.BaseUserId);
                if (workership == null)
                {
                    await context.RespondAsync(new UpdateWorkershipResponse()
                    {
                        Packet = new Packet {Status = "error_3"}
                    });
                    return;
                }
                workership.PosX = packet.Workership.PosX;
                workership.PosY = packet.Workership.PosY;
                workership.Width = packet.Workership.Width;
                workership.Height = packet.Workership.Height;
                dbContext.SaveChanges();
                
                SharedArea.Transport.NotifyService<WorkershipUpdatedNotif, WorkershipUpdatedNotifResponse>(
                    Program.Bus,
                    new Packet() {Workership = workership},
                    new []
                    {
                        SharedArea.GlobalVariables.MESSENGER_QUEUE_NAME,
                        SharedArea.GlobalVariables.BOT_QUEUE_NAME
                    });
                
                await context.RespondAsync(new UpdateWorkershipResponse()
                {
                    Packet = new Packet {Status = "success"}
                });
            }
        }

        public async Task Consume(ConsumeContext<RemoveBotFromRoomRequest> context)
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
                    await context.RespondAsync(new RemoveBotFromRoomResponse()
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
                    await context.RespondAsync(new RemoveBotFromRoomResponse()
                    {
                        Packet = new Packet {Status = "error_3"}
                    });
                    return;
                }
                dbContext.Entry(room).Collection(r => r.Workers).Load();
                var workership = room.Workers.Find(w => w.BotId == packet.Bot.BaseUserId);
                if (workership == null)
                {
                    await context.RespondAsync(new RemoveBotFromRoomResponse()
                    {
                        Packet = new Packet {Status = "error_0"}
                    });
                    return;
                }
                room.Workers.Remove(workership);
                dbContext.Workerships.Remove(workership);
                dbContext.SaveChanges();
                
                SharedArea.Transport.NotifyService<WorkershipDeletedNotif, WorkershipDeletedNotifResponse>(
                    Program.Bus,
                    new Packet() {Workership = workership},
                    new []
                    {
                        SharedArea.GlobalVariables.MESSENGER_QUEUE_NAME,
                        SharedArea.GlobalVariables.BOT_QUEUE_NAME
                    });
                
                var bot = dbContext.Bots.Find(workership.BotId);
                dbContext.Entry(bot).Collection(b => b.Sessions).Load();
                var botSess = bot.Sessions.FirstOrDefault();
                var removation = new BotRemovationFromRoomNotification()
                {
                    Room = room
                };
                if (botSess != null)
                    SharedArea.Transport.Push<BotRemovationFromRoomPush>(
                        Program.Bus,
                        new BotRemovationFromRoomPush()
                        {
                            Notif = removation,
                            SessionIds = new[] {botSess.SessionId}.ToList()
                        });

                await context.RespondAsync(new RemoveBotFromRoomResponse()
                {
                    Packet = new Packet {Status = "success"}
                });
            }
        }

        public async Task Consume(ConsumeContext<GetWorkershipsRequest> context)
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
                    await context.RespondAsync(new GetWorkershipsResponse()
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
                    await context.RespondAsync(new GetWorkershipsResponse()
                    {
                        Packet = new Packet {Status = "error_2"}
                    });
                    return;
                }
                dbContext.Entry(room).Collection(r => r.Workers).Load();
                var workers = room.Workers.ToList();
                await context.RespondAsync(new GetWorkershipsResponse()
                {
                    Packet = new Packet {Status = "success", Workerships = workers}
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