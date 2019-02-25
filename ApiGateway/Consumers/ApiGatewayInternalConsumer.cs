using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ApiGateway.DbContexts;
using MassTransit;
using SharedArea.Commands.Auth;
using SharedArea.Commands.Internal.Notifications;
using SharedArea.Commands.Internal.Requests;
using SharedArea.Commands.Internal.Responses;
using SharedArea.Commands.Pushes;
using SharedArea.Entities;
using SharedArea.Notifications;

namespace ApiGateway.Consumers
{
    public class ApiGatewayInternalConsumer : IConsumer<UserCreatedNotif>, IConsumer<ComplexCreatedNotif>
        , IConsumer<RoomCreatedNotif>, IConsumer<MembershipCreatedNotif>, IConsumer<SessionCreatedNotif>
        , IConsumer<UserProfileUpdatedNotif>, IConsumer<ComplexProfileUpdatedNotif>, IConsumer<ComplexDeletionNotif>
        , IConsumer<RoomProfileUpdatedNotif>, IConsumer<ContactCreatedNotif>, IConsumer<InviteCreatedNotif>
        , IConsumer<InviteCancelledNotif>, IConsumer<InviteAcceptedNotif>, IConsumer<InviteIgnoredNotif>
        , IConsumer<BotProfileUpdatedNotif>, IConsumer<BotSubscribedNotif>, IConsumer<BotCreatedNotif>
        , IConsumer<PhotoCreatedNotif>, IConsumer<AudioCreatedNotif>, IConsumer<VideoCreatedNotif>
        , IConsumer<SessionUpdatedNotif>, IConsumer<RoomDeletionNotif>, IConsumer<WorkershipCreatedNotif>
        , IConsumer<WorkershipUpdatedNotif>, IConsumer<WorkershipDeletedNotif>, IConsumer<AccountCreatedNotif>

        , IConsumer<PutUserRequest>, IConsumer<PutComplexRequest>, IConsumer<PutRoomRequest>
        , IConsumer<PutMembershipRequest>, IConsumer<PutSessionRequest>, IConsumer<UpdateUserSecretRequest>
        , IConsumer<PutServiceMessageRequest>, IConsumer<ConsolidateContactRequest>
        , IConsumer<ConsolidateSessionRequest>
        , IConsumer<MakeAccountRequest>, IConsumer<ConsolidateDeleteAccountRequest>
        , IConsumer<ConsolidateMakeAccountRequest>, IConsumer<ConsolidateCreateComplexRequest>
        , IConsumer<ConsolidateCreateRoomRequest>, IConsumer<ConsolidateLogoutRequest>

        , IConsumer<ComplexDeletionPush>, IConsumer<RoomDeletionPush>, IConsumer<ContactCreationPush>
        , IConsumer<ServiceMessagePush>, IConsumer<InviteCreationPush>, IConsumer<InviteCancellationPush>
        , IConsumer<UserJointComplexPush>, IConsumer<InviteAcceptancePush>, IConsumer<InviteIgnoredPush>
        , IConsumer<BotAdditionToRoomPush>, IConsumer<BotRemovationFromRoomPush>, IConsumer<TextMessagePush>
        , IConsumer<PhotoMessagePush>, IConsumer<AudioMessagePush>, IConsumer<VideoMessagePush>
        , IConsumer<UserRequestedBotViewPush>, IConsumer<BotSentBotViewPush>, IConsumer<BotUpdatedBotViewPush>
        , IConsumer<BotAnimatedBotViewPush>, IConsumer<BotRanCommandsOnBotViewPush>, IConsumer<MessageSeenPush>
    {
        public async Task Consume(ConsumeContext<UserCreatedNotif> context)
        {
            var tasks = new Task[context.Message.Destinations.Length];
            var counter = 0;
            foreach (var destination in context.Message.Destinations)
            {
                tasks[counter] = SharedArea.Transport.DirectService<UserCreatedNotif, UserCreatedNotifResponse>(
                    Program.Bus,
                    destination,
                    context.Message.Packet);
                counter++;
            }
            Task.WaitAll(tasks);
            await context.RespondAsync(new UserCreatedNotifResponse());
        }

        public async Task Consume(ConsumeContext<ComplexCreatedNotif> context)
        {
            var tasks = new Task[context.Message.Destinations.Length];
            var counter = 0;
            foreach (var destination in context.Message.Destinations)
            {
                tasks[counter] = SharedArea.Transport.DirectService<ComplexCreatedNotif, ComplexCreatedNotifResponse>(
                    Program.Bus,
                    destination,
                    context.Message.Packet);
                counter++;
            }
            Task.WaitAll(tasks);
            await context.RespondAsync(new ComplexCreatedNotifResponse());
        }

        public async Task Consume(ConsumeContext<RoomCreatedNotif> context)
        {
            var tasks = new Task[context.Message.Destinations.Length];
            var counter = 0;
            foreach (var destination in context.Message.Destinations)
            {
                tasks[counter] = SharedArea.Transport.DirectService<RoomCreatedNotif, RoomCreatedNotifResponse>(
                    Program.Bus,
                    destination,
                    context.Message.Packet);
                counter++;
            }
            Task.WaitAll(tasks);
            await context.RespondAsync(new RoomCreatedNotifResponse());
        }

        public async Task Consume(ConsumeContext<MembershipCreatedNotif> context)
        {
            var tasks = new Task[context.Message.Destinations.Length];
            var counter = 0;
            foreach (var destination in context.Message.Destinations)
            {
                tasks[counter] = SharedArea.Transport.DirectService<MembershipCreatedNotif, MembershipCreatedNotifResponse>(
                    Program.Bus,
                    destination,
                    context.Message.Packet);
                counter++;
            }
            Task.WaitAll(tasks);
            await context.RespondAsync(new MembershipCreatedNotifResponse());
        }

        public async Task Consume(ConsumeContext<SessionCreatedNotif> context)
        {
            using (var dbContext = new DatabaseContext())
            {
                var session = context.Message.Packet.Session;

                session.BaseUserId = null;
                session.BaseUser = null;

                dbContext.Sessions.Add(session);

                dbContext.SaveChanges();
            }
            
            var tasks = new Task[context.Message.Destinations.Length];
            var counter = 0;
            foreach (var destination in context.Message.Destinations)
            {
                tasks[counter] = SharedArea.Transport.DirectService<SessionCreatedNotif, SessionCreatedNotifResponse>(
                    Program.Bus,
                    destination,
                    context.Message.Packet);
                counter++;
            }
            Task.WaitAll(tasks);
            await context.RespondAsync(new SessionCreatedNotifResponse());
        }

        public async Task Consume(ConsumeContext<UserProfileUpdatedNotif> context)
        {
            var tasks = new Task[context.Message.Destinations.Length];
            var counter = 0;
            foreach (var destination in context.Message.Destinations)
            {
                tasks[counter] = SharedArea.Transport.DirectService<UserProfileUpdatedNotif, UserProfileUpdatedNotifResponse>(
                    Program.Bus,
                    destination,
                    context.Message.Packet);
                counter++;
            }
            Task.WaitAll(tasks);
            await context.RespondAsync(new UserProfileUpdatedNotifResponse());
        }

        public async Task Consume(ConsumeContext<ComplexProfileUpdatedNotif> context)
        {
            var tasks = new Task[context.Message.Destinations.Length];
            var counter = 0;
            foreach (var destination in context.Message.Destinations)
            {
                tasks[counter] = SharedArea.Transport.DirectService<ComplexProfileUpdatedNotif, ComplexProfileUpdatedNotifResponse>(
                    Program.Bus,
                    destination,
                    context.Message.Packet);
                counter++;
            }
            Task.WaitAll(tasks);
            await context.RespondAsync(new ComplexProfileUpdatedNotifResponse());
        }

        public async Task Consume(ConsumeContext<ComplexDeletionNotif> context)
        {
            var tasks = new Task[context.Message.Destinations.Length];
            var counter = 0;
            foreach (var destination in context.Message.Destinations)
            {
                tasks[counter] = SharedArea.Transport.DirectService<ComplexDeletionNotif, ComplexDeletionNotifResponse>(
                    Program.Bus,
                    destination,
                    context.Message.Packet);
                counter++;
            }
            Task.WaitAll(tasks);
            await context.RespondAsync(new ComplexDeletionNotifResponse());
        }

        public async Task Consume(ConsumeContext<RoomProfileUpdatedNotif> context)
        {
            var tasks = new Task[context.Message.Destinations.Length];
            var counter = 0;
            foreach (var destination in context.Message.Destinations)
            {
                tasks[counter] = SharedArea.Transport.DirectService<RoomProfileUpdatedNotif, RoomProfileUpdatedNotifResponse>(
                    Program.Bus,
                    destination,
                    context.Message.Packet);
                counter++;
            }
            Task.WaitAll(tasks);
            await context.RespondAsync(new RoomProfileUpdatedNotifResponse());
        }

        public async Task Consume(ConsumeContext<ContactCreatedNotif> context)
        {
            var tasks = new Task[context.Message.Destinations.Length];
            var counter = 0;
            foreach (var destination in context.Message.Destinations)
            {
                tasks[counter] = SharedArea.Transport.DirectService<ContactCreatedNotif, ContactCreatedNotifResponse>(
                    Program.Bus,
                    destination,
                    context.Message.Packet);
                counter++;
            }
            Task.WaitAll(tasks);
            await context.RespondAsync(new ContactCreatedNotifResponse());
        }

        public async Task Consume(ConsumeContext<InviteCreatedNotif> context)
        {
            var tasks = new Task[context.Message.Destinations.Length];
            var counter = 0;
            foreach (var destination in context.Message.Destinations)
            {
                tasks[counter] = SharedArea.Transport.DirectService<InviteCreatedNotif, InviteCreatedNotifResponse>(
                    Program.Bus,
                    destination,
                    context.Message.Packet);
                counter++;
            }
            Task.WaitAll(tasks);
            await context.RespondAsync(new InviteCreatedNotifResponse());
        }

        public async Task Consume(ConsumeContext<InviteCancelledNotif> context)
        {
            var tasks = new Task[context.Message.Destinations.Length];
            var counter = 0;
            foreach (var destination in context.Message.Destinations)
            {
                tasks[counter] = SharedArea.Transport.DirectService<InviteCancelledNotif, InviteCancelledNotifResponse>(
                    Program.Bus,
                    destination,
                    context.Message.Packet);
                counter++;
            }
            Task.WaitAll(tasks);
            await context.RespondAsync(new InviteCancelledNotifResponse());
        }

        public async Task Consume(ConsumeContext<InviteAcceptedNotif> context)
        {
            var tasks = new Task[context.Message.Destinations.Length];
            var counter = 0;
            foreach (var destination in context.Message.Destinations)
            {
                tasks[counter] = SharedArea.Transport.DirectService<InviteAcceptedNotif, InviteAcceptedNotifResponse>(
                    Program.Bus,
                    destination,
                    context.Message.Packet);
                counter++;
            }
            Task.WaitAll(tasks);
            await context.RespondAsync(new InviteAcceptedNotifResponse());
        }

        public async Task Consume(ConsumeContext<InviteIgnoredNotif> context)
        {
            var tasks = new Task[context.Message.Destinations.Length];
            var counter = 0;
            foreach (var destination in context.Message.Destinations)
            {
                tasks[counter] = SharedArea.Transport.DirectService<InviteIgnoredNotif, InviteIgnoredNotifResponse>(
                    Program.Bus,
                    destination,
                    context.Message.Packet);
                counter++;
            }
            Task.WaitAll(tasks);
            await context.RespondAsync(new InviteIgnoredNotifResponse());
        }

        public async Task Consume(ConsumeContext<BotProfileUpdatedNotif> context)
        {
            var tasks = new Task[context.Message.Destinations.Length];
            var counter = 0;
            foreach (var destination in context.Message.Destinations)
            {
                tasks[counter] = SharedArea.Transport.DirectService<BotProfileUpdatedNotif, BotProfileUpdatedNotifResponse>(
                    Program.Bus,
                    destination,
                    context.Message.Packet);
                counter++;
            }
            Task.WaitAll(tasks);
            await context.RespondAsync(new BotProfileUpdatedNotifResponse());
        }

        public async Task Consume(ConsumeContext<BotSubscribedNotif> context)
        {
            var tasks = new Task[context.Message.Destinations.Length];
            var counter = 0;
            foreach (var destination in context.Message.Destinations)
            {
                tasks[counter] = SharedArea.Transport.DirectService<BotSubscribedNotif, BotSubscribedNotifResponse>(
                    Program.Bus,
                    destination,
                    context.Message.Packet);
                counter++;
            }
            Task.WaitAll(tasks);
            await context.RespondAsync(new BotSubscribedNotifResponse());
        }

        public async Task Consume(ConsumeContext<BotCreatedNotif> context)
        {
            using (var dbContext = new DatabaseContext())
            {
                var session = context.Message.Packet.Bot.Sessions.FirstOrDefault();

                if (session != null)
                {
                    session.BaseUserId = null;
                    session.BaseUser = null;

                    dbContext.Sessions.Add(session);
                }

                dbContext.SaveChanges();
            }
            
            var tasks = new Task[context.Message.Destinations.Length];
            var counter = 0;
            foreach (var destination in context.Message.Destinations)
            {
                tasks[counter] = SharedArea.Transport.DirectService<BotCreatedNotif, BotCreatedNotifResponse>(
                    Program.Bus,
                    destination,
                    context.Message.Packet);
                counter++;
            }
            Task.WaitAll(tasks);
            await context.RespondAsync(new BotCreatedNotifResponse());
        }

        public async Task Consume(ConsumeContext<PhotoCreatedNotif> context)
        {
            var tasks = new Task[context.Message.Destinations.Length];
            var counter = 0;
            foreach (var destination in context.Message.Destinations)
            {
                tasks[counter] = SharedArea.Transport.DirectService<PhotoCreatedNotif, PhotoCreatedNotifResponse>(
                    Program.Bus,
                    destination,
                    context.Message.Packet);
                counter++;
            }
            Task.WaitAll(tasks);
            await context.RespondAsync(new PhotoCreatedNotifResponse());
        }

        public async Task Consume(ConsumeContext<AudioCreatedNotif> context)
        {
            var tasks = new Task[context.Message.Destinations.Length];
            var counter = 0;
            foreach (var destination in context.Message.Destinations)
            {
                tasks[counter] = SharedArea.Transport.DirectService<AudioCreatedNotif, AudioCreatedNotifResponse>(
                    Program.Bus,
                    destination,
                    context.Message.Packet);
                counter++;
            }
            Task.WaitAll(tasks);
            await context.RespondAsync(new AudioCreatedNotifResponse());
        }

        public async Task Consume(ConsumeContext<VideoCreatedNotif> context)
        {
            var tasks = new Task[context.Message.Destinations.Length];
            var counter = 0;
            foreach (var destination in context.Message.Destinations)
            {
                tasks[counter] = SharedArea.Transport.DirectService<VideoCreatedNotif, VideoCreatedNotifResponse>(
                    Program.Bus,
                    destination,
                    context.Message.Packet);
                counter++;
            }
            Task.WaitAll(tasks);
            await context.RespondAsync(new VideoCreatedNotifResponse());
        }

        public async Task Consume(ConsumeContext<SessionUpdatedNotif> context)
        {
            var tasks = new Task[context.Message.Destinations.Length];
            var counter = 0;
            foreach (var destination in context.Message.Destinations)
            {
                tasks[counter] = SharedArea.Transport.DirectService<SessionUpdatedNotif, SessionUpdatedNotifResponse>(
                    Program.Bus,
                    destination,
                    context.Message.Packet);
                counter++;
            }
            Task.WaitAll(tasks);
            await context.RespondAsync(new SessionUpdatedNotifResponse());
        }

        public async Task Consume(ConsumeContext<RoomDeletionNotif> context)
        {
            var tasks = new Task[context.Message.Destinations.Length];
            var counter = 0;
            foreach (var destination in context.Message.Destinations)
            {
                tasks[counter] = SharedArea.Transport.DirectService<RoomDeletionNotif, RoomDeletionNotifResponse>(
                    Program.Bus,
                    destination,
                    context.Message.Packet);
                counter++;
            }
            Task.WaitAll(tasks);
            await context.RespondAsync(new RoomDeletionNotifResponse());
        }

        public async Task Consume(ConsumeContext<WorkershipCreatedNotif> context)
        {
            var tasks = new Task[context.Message.Destinations.Length];
            var counter = 0;
            foreach (var destination in context.Message.Destinations)
            {
                tasks[counter] = SharedArea.Transport.DirectService<WorkershipCreatedNotif, WorkershipCreatedNotifResponse>(
                    Program.Bus,
                    destination,
                    context.Message.Packet);
                counter++;
            }
            Task.WaitAll(tasks);
            await context.RespondAsync(new WorkershipCreatedNotifResponse());
        }

        public async Task Consume(ConsumeContext<WorkershipUpdatedNotif> context)
        {
            var tasks = new Task[context.Message.Destinations.Length];
            var counter = 0;
            foreach (var destination in context.Message.Destinations)
            {
                tasks[counter] = SharedArea.Transport.DirectService<WorkershipUpdatedNotif, WorkershipUpdatedNotifResponse>(
                    Program.Bus,
                    destination,
                    context.Message.Packet);
                counter++;
            }
            Task.WaitAll(tasks);
            await context.RespondAsync(new WorkershipUpdatedNotifResponse());
        }

        public async Task Consume(ConsumeContext<WorkershipDeletedNotif> context)
        {
            var tasks = new Task[context.Message.Destinations.Length];
            var counter = 0;
            foreach (var destination in context.Message.Destinations)
            {
                tasks[counter] = SharedArea.Transport.DirectService<WorkershipDeletedNotif, WorkershipDeletedNotifResponse>(
                    Program.Bus,
                    destination,
                    context.Message.Packet);
                counter++;
            }
            Task.WaitAll(tasks);
            await context.RespondAsync(new WorkershipDeletedNotifResponse());
        }

        public async Task Consume(ConsumeContext<AccountCreatedNotif> context)
        {
            var tasks = new Task[context.Message.Destinations.Length];
            var counter = 0;
            foreach (var destination in context.Message.Destinations)
            {
                tasks[counter] = SharedArea.Transport.DirectService<AccountCreatedNotif, AccountCreatedNotifResponse>(
                    Program.Bus,
                    destination,
                    context.Message.Packet);
                counter++;
            }
            Task.WaitAll(tasks);
            await context.RespondAsync(new AccountCreatedNotifResponse());
        }

        public async Task Consume(ConsumeContext<PutUserRequest> context)
        {
            var result = await SharedArea.Transport.DirectService<PutUserRequest, PutUserResponse>(
                Program.Bus,
                context.Message.Destination,
                context.Message.Packet);
            await context.RespondAsync(result);
        }

        public async Task Consume(ConsumeContext<PutComplexRequest> context)
        {
            var result = await SharedArea.Transport.DirectService<PutComplexRequest, PutComplexResponse>(
                Program.Bus,
                context.Message.Destination,
                context.Message.Packet);
            await context.RespondAsync(result);
        }

        public async Task Consume(ConsumeContext<PutRoomRequest> context)
        {
            var result = await SharedArea.Transport.DirectService<PutRoomRequest, PutRoomResponse>(
                Program.Bus,
                context.Message.Destination,
                context.Message.Packet);
            await context.RespondAsync(result);
        }

        public async Task Consume(ConsumeContext<PutMembershipRequest> context)
        {
            var result = await SharedArea.Transport.DirectService<PutMembershipRequest, PutMembershipResponse>(
                Program.Bus,
                context.Message.Destination,
                context.Message.Packet);
            await context.RespondAsync(result);
        }

        public async Task Consume(ConsumeContext<PutSessionRequest> context)
        {
            var result = await SharedArea.Transport.DirectService<PutSessionRequest, PutSessionResponse>(
                Program.Bus,
                context.Message.Destination,
                context.Message.Packet);
            await context.RespondAsync(result);
        }

        public async Task Consume(ConsumeContext<UpdateUserSecretRequest> context)
        {
            var result = await SharedArea.Transport.DirectService<UpdateUserSecretRequest, UpdateUserSecretResponse>(
                Program.Bus,
                context.Message.Destination,
                context.Message.Packet);
            await context.RespondAsync(result);
        }

        public async Task Consume(ConsumeContext<PutServiceMessageRequest> context)
        {
            var result = await SharedArea.Transport.DirectService<PutServiceMessageRequest, PutServiceMessageResponse>(
                Program.Bus,
                context.Message.Destination,
                context.Message.Packet);
            await context.RespondAsync(result);
        }

        public async Task Consume(ConsumeContext<ConsolidateContactRequest> context)
        {
            var result =
                await SharedArea.Transport.DirectService<ConsolidateContactRequest, ConsolidateContactResponse>(
                    Program.Bus,
                    context.Message.Destination,
                    context.Message.Packet);
            await context.RespondAsync(result);
        }

        public async Task Consume(ConsumeContext<MakeAccountRequest> context)
        {
            var result = await SharedArea.Transport.DirectService<MakeAccountRequest, MakeAccountResponse>(
                Program.Bus,
                context.Message.Destination,
                context.Message.Packet);
            await context.RespondAsync(result);
        }

        public async Task Consume(ConsumeContext<ConsolidateSessionRequest> context)
        {
            var globalSession = context.Message.Packet.Session;

            using (var dbContext = new DatabaseContext())
            {
                var session = dbContext.Sessions.Find(globalSession.SessionId);

                session.Online = globalSession.Online;
                session.ConnectionId = globalSession.ConnectionId;
                session.Token = globalSession.Token;

                dbContext.SaveChanges();
            }

            await context.RespondAsync(new ConsolidateSessionResponse());
        }

        public async Task Consume(ConsumeContext<ConsolidateDeleteAccountRequest> context)
        {
            if (context.Message.Destination == "")
            {
                var gSessions = context.Message.Packet.Sessions;

                using (var dbContext = new DatabaseContext())
                {
                    var lSessions = new List<Session>();
                    foreach (var gSession in gSessions)
                        lSessions.Add(dbContext.Sessions.Find(gSession.SessionId));

                    dbContext.Sessions.RemoveRange(lSessions);

                    dbContext.SaveChanges();
                }

                await context.RespondAsync(new ConsolidateDeleteAccountResponse());
            }
            else
            {
                var result = await SharedArea.Transport
                    .DirectService<ConsolidateDeleteAccountRequest, ConsolidateDeleteAccountResponse>(
                        Program.Bus,
                        context.Message.Destination,
                        context.Message.Packet);
                await context.RespondAsync(result);
            }
        }
        
        public async Task Consume(ConsumeContext<ConsolidateLogoutRequest> context)
        {
            if (context.Message.Destination == "")
            {
                var gSession = context.Message.Packet.Session;

                using (var dbContext = new DatabaseContext())
                {
                    var lSess = dbContext.Sessions.Find(gSession.SessionId);
                    if (lSess != null)
                    {
                        dbContext.Sessions.RemoveRange(lSess);
                        dbContext.SaveChanges();
                    }
                }

                await context.RespondAsync(new ConsolidateLogoutResponse());
            }
            else
            {
                var result = await SharedArea.Transport
                    .DirectService<ConsolidateLogoutRequest, ConsolidateLogoutResponse>(
                        Program.Bus,
                        context.Message.Destination,
                        context.Message.Packet);
                await context.RespondAsync(result);
            }
        }

        public Task Consume(ConsumeContext<ComplexDeletionPush> context)
        {
            using (var dbContext = new DatabaseContext())
            {
                foreach (var sessionId in context.Message.SessionIds)
                {
                    var session = dbContext.Sessions.Find(sessionId);

                    var notification = new ComplexDeletionNotification()
                    {
                        ComplexId = context.Message.Notif.ComplexId,
                        Session = session
                    };

                    using (var mongo = new MongoLayer())
                    {
                        mongo.GetNotifsColl().InsertOne(notification);
                    }
                }

                foreach (var sessionId in context.Message.SessionIds)
                {
                    Startup.Pusher.NextPush(sessionId);
                }
            }

            return Task.CompletedTask;
        }

        public Task Consume(ConsumeContext<RoomDeletionPush> context)
        {
            using (var dbContext = new DatabaseContext())
            {
                foreach (var sessionId in context.Message.SessionIds)
                {
                    var session = dbContext.Sessions.Find(sessionId);

                    var notification = new RoomDeletionNotification()
                    {
                        ComplexId = context.Message.Notif.ComplexId,
                        RoomId = context.Message.Notif.RoomId,
                        Session = session
                    };

                    using (var mongo = new MongoLayer())
                    {
                        mongo.GetNotifsColl().InsertOne(notification);
                    }
                }

                foreach (var sessionId in context.Message.SessionIds)
                {
                    Startup.Pusher.NextPush(sessionId);
                }
            }

            return Task.CompletedTask;
        }

        public Task Consume(ConsumeContext<ContactCreationPush> context)
        {
            using (var dbContext = new DatabaseContext())
            {
                foreach (var sessionId in context.Message.SessionIds)
                {
                    var s = dbContext.Sessions.Find(sessionId);

                    var notification = new ContactCreationNotification()
                    {
                        Contact = context.Message.Notif.Contact,
                        Session = s
                    };

                    using (var mongo = new MongoLayer())
                    {
                        mongo.GetNotifsColl().InsertOne(notification);
                    }
                }

                foreach (var sessionId in context.Message.SessionIds)
                {
                    Startup.Pusher.NextPush(sessionId);
                }
            }

            return Task.CompletedTask;
        }

        public Task Consume(ConsumeContext<ServiceMessagePush> context)
        {
            using (var dbContext = new DatabaseContext())
            {
                foreach (var sessionId in context.Message.SessionIds)
                {
                    var s = dbContext.Sessions.Find(sessionId);

                    var notification = new ServiceMessageNotification()
                    {
                        Message = context.Message.Notif.Message,
                        Session = s
                    };

                    using (var mongo = new MongoLayer())
                    {
                        mongo.GetNotifsColl().InsertOne(notification);
                    }
                }

                foreach (var sessionId in context.Message.SessionIds)
                {
                    Startup.Pusher.NextPush(sessionId);
                }
            }

            return Task.CompletedTask;
        }

        public Task Consume(ConsumeContext<InviteCreationPush> context)
        {
            using (var dbContext = new DatabaseContext())
            {
                foreach (var sessionId in context.Message.SessionIds)
                {
                    var s = dbContext.Sessions.Find(sessionId);

                    var notification = new InviteCreationNotification()
                    {
                        Invite = context.Message.Notif.Invite,
                        Session = s
                    };

                    using (var mongo = new MongoLayer())
                    {
                        mongo.GetNotifsColl().InsertOne(notification);
                    }
                }

                foreach (var sessionId in context.Message.SessionIds)
                {
                    Startup.Pusher.NextPush(sessionId);
                }
            }

            return Task.CompletedTask;
        }

        public Task Consume(ConsumeContext<InviteCancellationPush> context)
        {
            using (var dbContext = new DatabaseContext())
            {
                foreach (var sessionId in context.Message.SessionIds)
                {
                    var s = dbContext.Sessions.Find(sessionId);

                    var notification = new InviteCancellationNotification()
                    {
                        Invite = context.Message.Notif.Invite,
                        Session = s
                    };

                    using (var mongo = new MongoLayer())
                    {
                        mongo.GetNotifsColl().InsertOne(notification);
                    }
                }

                foreach (var sessionId in context.Message.SessionIds)
                {
                    Startup.Pusher.NextPush(sessionId);
                }
            }

            return Task.CompletedTask;
        }

        public Task Consume(ConsumeContext<UserJointComplexPush> context)
        {
            using (var dbContext = new DatabaseContext())
            {
                foreach (var sessionId in context.Message.SessionIds)
                {
                    var s = dbContext.Sessions.Find(sessionId);

                    var notification = new UserJointComplexNotification()
                    {
                        ComplexId = context.Message.Notif.ComplexId,
                        UserId = context.Message.Notif.UserId,
                        Session = s
                    };

                    using (var mongo = new MongoLayer())
                    {
                        mongo.GetNotifsColl().InsertOne(notification);
                    }
                }

                foreach (var sessionId in context.Message.SessionIds)
                {
                    Startup.Pusher.NextPush(sessionId);
                }
            }

            return Task.CompletedTask;
        }

        public Task Consume(ConsumeContext<InviteAcceptancePush> context)
        {
            using (var dbContext = new DatabaseContext())
            {
                foreach (var sessionId in context.Message.SessionIds)
                {
                    var s = dbContext.Sessions.Find(sessionId);

                    var notification = new InviteAcceptanceNotification()
                    {
                        Invite = context.Message.Notif.Invite,
                        Session = s
                    };

                    using (var mongo = new MongoLayer())
                    {
                        mongo.GetNotifsColl().InsertOne(notification);
                    }
                }

                foreach (var sessionId in context.Message.SessionIds)
                {
                    Startup.Pusher.NextPush(sessionId);
                }
            }

            return Task.CompletedTask;
        }

        public Task Consume(ConsumeContext<InviteIgnoredPush> context)
        {
            using (var dbContext = new DatabaseContext())
            {
                foreach (var sessionId in context.Message.SessionIds)
                {
                    var s = dbContext.Sessions.Find(sessionId);

                    var notification = new InviteIgnoranceNotification()
                    {
                        Invite = context.Message.Notif.Invite,
                        Session = s
                    };

                    using (var mongo = new MongoLayer())
                    {
                        mongo.GetNotifsColl().InsertOne(notification);
                    }
                }

                foreach (var sessionId in context.Message.SessionIds)
                {
                    Startup.Pusher.NextPush(sessionId);
                }
            }

            return Task.CompletedTask;
        }

        public Task Consume(ConsumeContext<BotAdditionToRoomPush> context)
        {
            using (var dbContext = new DatabaseContext())
            {
                foreach (var sessionId in context.Message.SessionIds)
                {
                    var session = dbContext.Sessions.Find(sessionId);

                    var notification = new BotAdditionToRoomNotification()
                    {
                        Room = context.Message.Notif.Room,
                        Session = session
                    };

                    using (var mongo = new MongoLayer())
                    {
                        mongo.GetNotifsColl().InsertOne(notification);
                    }
                }

                foreach (var sessionId in context.Message.SessionIds)
                {
                    Startup.Pusher.NextPush(sessionId);
                }
            }

            return Task.CompletedTask;
        }

        public Task Consume(ConsumeContext<BotRemovationFromRoomPush> context)
        {
            using (var dbContext = new DatabaseContext())
            {
                foreach (var sessionId in context.Message.SessionIds)
                {
                    var session = dbContext.Sessions.Find(sessionId);

                    var notification = new BotRemovationFromRoomNotification()
                    {
                        Room = context.Message.Notif.Room,
                        Session = session
                    };

                    using (var mongo = new MongoLayer())
                    {
                        mongo.GetNotifsColl().InsertOne(notification);
                    }
                }

                foreach (var sessionId in context.Message.SessionIds)
                {
                    Startup.Pusher.NextPush(sessionId);
                }
            }

            return Task.CompletedTask;
        }

        public Task Consume(ConsumeContext<TextMessagePush> context)
        {
            using (var dbContext = new DatabaseContext())
            {
                foreach (var sessionId in context.Message.SessionIds)
                {
                    var session = dbContext.Sessions.Find(sessionId);
                    var notification = new TextMessageNotification()
                    {
                        Message = context.Message.Notif.Message,
                        Session = session
                    };

                    using (var mongo = new MongoLayer())
                    {
                        try
                        {
                            mongo.GetNotifsColl().InsertOne(notification);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex.ToString());
                        }
                    }
                }

                foreach (var sessionId in context.Message.SessionIds)
                {
                    Startup.Pusher.NextPush(sessionId);
                }
            }

            return Task.CompletedTask;
        }

        public Task Consume(ConsumeContext<PhotoMessagePush> context)
        {
            using (var dbContext = new DatabaseContext())
            {
                foreach (var sessionId in context.Message.SessionIds)
                {
                    var session = dbContext.Sessions.Find(sessionId);

                    var notification = new PhotoMessageNotification()
                    {
                        Message = context.Message.Notif.Message,
                        Session = session
                    };

                    using (var mongo = new MongoLayer())
                    {
                        mongo.GetNotifsColl().InsertOne(notification);
                    }
                }

                foreach (var sessionId in context.Message.SessionIds)
                {
                    Startup.Pusher.NextPush(sessionId);
                }
            }

            return Task.CompletedTask;
        }

        public Task Consume(ConsumeContext<AudioMessagePush> context)
        {
            using (var dbContext = new DatabaseContext())
            {
                foreach (var sessionId in context.Message.SessionIds)
                {
                    var session = dbContext.Sessions.Find(sessionId);

                    var notification = new AudioMessageNotification()
                    {
                        Message = context.Message.Notif.Message,
                        Session = session
                    };

                    using (var mongo = new MongoLayer())
                    {
                        mongo.GetNotifsColl().InsertOne(notification);
                    }
                }

                foreach (var sessionId in context.Message.SessionIds)
                {
                    Startup.Pusher.NextPush(sessionId);
                }
            }

            return Task.CompletedTask;
        }

        public Task Consume(ConsumeContext<VideoMessagePush> context)
        {
            using (var dbContext = new DatabaseContext())
            {
                foreach (var sessionId in context.Message.SessionIds)
                {
                    var session = dbContext.Sessions.Find(sessionId);

                    var notification = new VideoMessageNotification()
                    {
                        Message = context.Message.Notif.Message,
                        Session = session
                    };

                    using (var mongo = new MongoLayer())
                    {
                        mongo.GetNotifsColl().InsertOne(notification);
                    }
                }

                foreach (var sessionId in context.Message.SessionIds)
                {
                    Startup.Pusher.NextPush(sessionId);
                }
            }

            return Task.CompletedTask;
        }

        public Task Consume(ConsumeContext<UserRequestedBotViewPush> context)
        {
            using (var dbContext = new DatabaseContext())
            {
                foreach (var sessionId in context.Message.SessionIds)
                {
                    var session = dbContext.Sessions.Find(sessionId);

                    var notification = new UserRequestedBotViewNotification()
                    {
                        ComplexId = context.Message.Notif.ComplexId,
                        RoomId = context.Message.Notif.RoomId,
                        BotId = context.Message.Notif.BotId,
                        UserSessionId = context.Message.Notif.UserSessionId,
                        Session = session
                    };

                    using (var mongo = new MongoLayer())
                    {
                        mongo.GetNotifsColl().InsertOne(notification);
                    }
                }

                foreach (var sessionId in context.Message.SessionIds)
                {
                    Startup.Pusher.NextPush(sessionId);
                }
            }

            return Task.CompletedTask;
        }

        public Task Consume(ConsumeContext<BotSentBotViewPush> context)
        {
            using (var dbContext = new DatabaseContext())
            {
                foreach (var sessionId in context.Message.SessionIds)
                {
                    var session = dbContext.Sessions.Find(sessionId);

                    var notification = new BotSentBotViewNotification()
                    {
                        ComplexId = context.Message.Notif.ComplexId,
                        RoomId = context.Message.Notif.RoomId,
                        BotId = context.Message.Notif.BotId,
                        ViewData = context.Message.Notif.ViewData,
                        Session = session
                    };

                    using (var mongo = new MongoLayer())
                    {
                        mongo.GetNotifsColl().InsertOne(notification);
                    }
                }

                foreach (var sessionId in context.Message.SessionIds)
                {
                    Startup.Pusher.NextPush(sessionId);
                }
            }

            return Task.CompletedTask;
        }

        public Task Consume(ConsumeContext<BotUpdatedBotViewPush> context)
        {
            using (var dbContext = new DatabaseContext())
            {
                foreach (var sessionId in context.Message.SessionIds)
                {
                    var session = dbContext.Sessions.Find(sessionId);

                    var notification = new BotUpdatedBotViewNotification()
                    {
                        ComplexId = context.Message.Notif.ComplexId,
                        RoomId = context.Message.Notif.RoomId,
                        BotId = context.Message.Notif.BotId,
                        UpdateData = context.Message.Notif.UpdateData,
                        Session = session
                    };

                    using (var mongo = new MongoLayer())
                    {
                        mongo.GetNotifsColl().InsertOne(notification);
                    }
                }

                foreach (var sessionId in context.Message.SessionIds)
                {
                    Startup.Pusher.NextPush(sessionId);
                }
            }

            return Task.CompletedTask;
        }

        public Task Consume(ConsumeContext<BotAnimatedBotViewPush> context)
        {
            using (var dbContext = new DatabaseContext())
            {
                foreach (var sessionId in context.Message.SessionIds)
                {
                    var session = dbContext.Sessions.Find(sessionId);

                    var notification = new BotAnimatedBotViewNotification()
                    {
                        ComplexId = context.Message.Notif.ComplexId,
                        RoomId = context.Message.Notif.RoomId,
                        BotId = context.Message.Notif.BotId,
                        AnimData = context.Message.Notif.AnimData,
                        Session = session
                    };

                    using (var mongo = new MongoLayer())
                    {
                        mongo.GetNotifsColl().InsertOne(notification);
                    }
                }

                foreach (var sessionId in context.Message.SessionIds)
                {
                    Startup.Pusher.NextPush(sessionId);
                }
            }

            return Task.CompletedTask;
        }

        public Task Consume(ConsumeContext<BotRanCommandsOnBotViewPush> context)
        {
            using (var dbContext = new DatabaseContext())
            {
                foreach (var sessionId in context.Message.SessionIds)
                {
                    var session = dbContext.Sessions.Find(sessionId);

                    var notification = new BotRanCommandsOnBotViewNotification()
                    {
                        ComplexId = context.Message.Notif.ComplexId,
                        RoomId = context.Message.Notif.RoomId,
                        BotId = context.Message.Notif.BotId,
                        CommandsData = context.Message.Notif.CommandsData,
                        Session = session
                    };

                    using (var mongo = new MongoLayer())
                    {
                        mongo.GetNotifsColl().InsertOne(notification);
                    }
                }

                foreach (var sessionId in context.Message.SessionIds)
                {
                    Startup.Pusher.NextPush(sessionId);
                }
            }

            return Task.CompletedTask;
        }

        public Task Consume(ConsumeContext<MessageSeenPush> context)
        {
            using (var dbContext = new DatabaseContext())
            {
                foreach (var sessionId in context.Message.SessionIds)
                {
                    var session = dbContext.Sessions.Find(sessionId);

                    var notification = new MessageSeenNotification()
                    {
                        MessageId = context.Message.Notif.MessageId,
                        MessageSeenCount = context.Message.Notif.MessageSeenCount,
                        Session = session
                    };

                    using (var mongo = new MongoLayer())
                    {
                        mongo.GetNotifsColl().InsertOne(notification);
                    }
                }

                foreach (var sessionId in context.Message.SessionIds)
                {
                    Startup.Pusher.NextPush(sessionId);
                }
            }

            return Task.CompletedTask;
        }

        public async Task Consume(ConsumeContext<ConsolidateMakeAccountRequest> context)
        {
            var result = await SharedArea.Transport
                .DirectService<ConsolidateMakeAccountRequest, ConsolidateMakeAccountResponse>(
                    Program.Bus,
                    context.Message.Destination,
                    context.Message.Packet);
            await context.RespondAsync(result);
        }
        
        public async Task Consume(ConsumeContext<ConsolidateCreateComplexRequest> context)
        {
            var result = await SharedArea.Transport
                .DirectService<ConsolidateCreateComplexRequest, ConsolidateCreateComplexResponse>(
                    Program.Bus,
                    context.Message.Destination,
                    context.Message.Packet);
            await context.RespondAsync(result);
        }

        public async Task Consume(ConsumeContext<ConsolidateCreateRoomRequest> context)
        {
            var result = await SharedArea.Transport
                .DirectService<ConsolidateCreateRoomRequest, ConsolidateCreateRoomResponse>(
                    Program.Bus,
                    context.Message.Destination,
                    context.Message.Packet);
            await context.RespondAsync(result);
        }
    }
}