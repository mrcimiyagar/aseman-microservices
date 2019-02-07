﻿using System;
using System.Linq;
using System.Threading.Tasks;
using ApiGateway.DbContexts;
using MassTransit;
using SharedArea.Commands.Internal.Notifications;
using SharedArea.Commands.Internal.Requests;
using SharedArea.Commands.Internal.Responses;
using SharedArea.Commands.Pushes;
using SharedArea.Notifications;

namespace ApiGateway.Consumers
{
    public class ApiGatewayInternalConsumer : IConsumer<UserCreatedNotif>, IConsumer<ComplexCreatedNotif>
        , IConsumer<RoomCreatedNotif>, IConsumer<MembershipCreatedNotif>, IConsumer<SessionCreatedNotif>
        , IConsumer<UserProfileUpdatedNotif>, IConsumer<ComplexProfileUpdatedNotif>, IConsumer<ComplexDeletionNotif>
        , IConsumer<RoomProfileUpdatedNotif>, IConsumer<ContactCreatedNotif>, IConsumer<InviteCreatedNotif>
        , IConsumer<InviteCancelledNotif>, IConsumer<InviteAcceptedNotif>, IConsumer<InvitedIgnoredNotif>
        , IConsumer<BotProfileUpdatedNotif>, IConsumer<BotSubscribedNotif>, IConsumer<BotCreatedNotif>
        , IConsumer<PhotoCreatedNotif>, IConsumer<AudioCreatedNotif>, IConsumer<VideoCreatedNotif>
        , IConsumer<SessionUpdatedNotif>, IConsumer<RoomDeletionNotif>, IConsumer<WorkershipCreatedNotif>
        , IConsumer<WorkershipUpdatedNotif>, IConsumer<WorkershipDeletedNotif>

        , IConsumer<PutUserRequest>, IConsumer<PutComplexRequest>, IConsumer<PutRoomRequest>
        , IConsumer<PutMembershipRequest>, IConsumer<PutSessionRequest>, IConsumer<UpdateUserSecretRequest>
        , IConsumer<PutServiceMessageRequest>, IConsumer<ConsolidateContactRequest>, IConsumer<ConsolidateSessionRequest>

        , IConsumer<ComplexDeletionPush>, IConsumer<RoomDeletionPush>, IConsumer<ContactCreationPush>
        , IConsumer<ServiceMessagePush>, IConsumer<InviteCreationPush>, IConsumer<InviteCancellationPush>
        , IConsumer<UserJointComplexPush>, IConsumer<InviteAcceptancePush>, IConsumer<InviteIgnoredPush>
        , IConsumer<BotAdditionToRoomPush>, IConsumer<BotRemovationFromRoomPush>, IConsumer<TextMessagePush>
        , IConsumer<PhotoMessagePush>, IConsumer<AudioMessagePush>, IConsumer<VideoMessagePush>
        , IConsumer<UserRequestedBotViewPush>, IConsumer<BotSentBotViewPush>, IConsumer<BotUpdatedBotViewPush>
        , IConsumer<BotAnimatedBotViewPush>, IConsumer<BotRanCommandsOnBotViewPush>
    {   
        public Task Consume(ConsumeContext<UserCreatedNotif> context)
        {
            foreach (var destination in context.Message.Destinations)
            {
                Program.Bus.GetSendEndpoint(new Uri(SharedArea.GlobalVariables.RABBITMQ_SERVER_URL + "/" + destination))
                    .Result.Send(context.Message);
            }
            return Task.CompletedTask;
        }

        public Task Consume(ConsumeContext<ComplexCreatedNotif> context)
        {
            foreach (var destination in context.Message.Destinations)
            {
                Program.Bus.GetSendEndpoint(new Uri(SharedArea.GlobalVariables.RABBITMQ_SERVER_URL + "/" + destination))
                    .Result.Send(context.Message);
            }
            return Task.CompletedTask;
        }

        public Task Consume(ConsumeContext<RoomCreatedNotif> context)
        {
            foreach (var destination in context.Message.Destinations)
            {
                Program.Bus.GetSendEndpoint(new Uri(SharedArea.GlobalVariables.RABBITMQ_SERVER_URL + "/" + destination))
                    .Result.Send(context.Message);
            }
            return Task.CompletedTask;
        }

        public Task Consume(ConsumeContext<MembershipCreatedNotif> context)
        {
            foreach (var destination in context.Message.Destinations)
            {
                Program.Bus.GetSendEndpoint(new Uri(SharedArea.GlobalVariables.RABBITMQ_SERVER_URL + "/" + destination))
                    .Result.Send(context.Message);
            }
            return Task.CompletedTask;
        }
        
        public Task Consume(ConsumeContext<SessionCreatedNotif> context)
        {
            foreach (var destination in context.Message.Destinations)
            {
                Program.Bus.GetSendEndpoint(new Uri(SharedArea.GlobalVariables.RABBITMQ_SERVER_URL + "/" + destination))
                    .Result.Send(context.Message);
            }

            using (var dbContext = new DatabaseContext())
            {
                var session = context.Message.Packet.Session;

                session.BaseUserId = null;
                session.BaseUser = null;

                dbContext.Sessions.Add(session);

                dbContext.SaveChanges();
            }
            
            return Task.CompletedTask;
        }
        
        public Task Consume(ConsumeContext<UserProfileUpdatedNotif> context)
        {
            foreach (var destination in context.Message.Destinations)
            {
                Program.Bus.GetSendEndpoint(new Uri(SharedArea.GlobalVariables.RABBITMQ_SERVER_URL + "/" + destination))
                    .Result.Send(context.Message);
            }
            return Task.CompletedTask;
        }
        
        public Task Consume(ConsumeContext<ComplexProfileUpdatedNotif> context)
        {
            foreach (var destination in context.Message.Destinations)
            {
                Program.Bus.GetSendEndpoint(new Uri(SharedArea.GlobalVariables.RABBITMQ_SERVER_URL + "/" + destination))
                    .Result.Send(context.Message);
            }
            return Task.CompletedTask;
        }

        public Task Consume(ConsumeContext<ComplexDeletionNotif> context)
        {
            foreach (var destination in context.Message.Destinations)
            {
                Program.Bus.GetSendEndpoint(new Uri(SharedArea.GlobalVariables.RABBITMQ_SERVER_URL + "/" + destination))
                    .Result.Send(context.Message);
            }
            return Task.CompletedTask;
        }
        
        public Task Consume(ConsumeContext<RoomProfileUpdatedNotif> context)
        {
            foreach (var destination in context.Message.Destinations)
            {
                Program.Bus.GetSendEndpoint(new Uri(SharedArea.GlobalVariables.RABBITMQ_SERVER_URL + "/" + destination))
                    .Result.Send(context.Message);
            }
            return Task.CompletedTask;
        }
        
        public Task Consume(ConsumeContext<ContactCreatedNotif> context)
        {
            foreach (var destination in context.Message.Destinations)
            {
                Program.Bus.GetSendEndpoint(new Uri(SharedArea.GlobalVariables.RABBITMQ_SERVER_URL + "/" + destination))
                    .Result.Send(context.Message);
            }
            return Task.CompletedTask;
        }
        
        public Task Consume(ConsumeContext<InviteCreatedNotif> context)
        {
            foreach (var destination in context.Message.Destinations)
            {
                Program.Bus.GetSendEndpoint(new Uri(SharedArea.GlobalVariables.RABBITMQ_SERVER_URL + "/" + destination))
                    .Result.Send(context.Message);
            }
            return Task.CompletedTask;
        }
        
        public Task Consume(ConsumeContext<InviteCancelledNotif> context)
        {
            foreach (var destination in context.Message.Destinations)
            {
                Program.Bus.GetSendEndpoint(new Uri(SharedArea.GlobalVariables.RABBITMQ_SERVER_URL + "/" + destination))
                    .Result.Send(context.Message);
            }
            return Task.CompletedTask;
        }
        
        public Task Consume(ConsumeContext<InviteAcceptedNotif> context)
        {
            foreach (var destination in context.Message.Destinations)
            {
                Program.Bus.GetSendEndpoint(new Uri(SharedArea.GlobalVariables.RABBITMQ_SERVER_URL + "/" + destination))
                    .Result.Send(context.Message);
            }
            return Task.CompletedTask;
        }
        
        public Task Consume(ConsumeContext<InvitedIgnoredNotif> context)
        {
            foreach (var destination in context.Message.Destinations)
            {
                Program.Bus.GetSendEndpoint(new Uri(SharedArea.GlobalVariables.RABBITMQ_SERVER_URL + "/" + destination))
                    .Result.Send(context.Message);
            }
            return Task.CompletedTask;
        }
        
        public Task Consume(ConsumeContext<BotProfileUpdatedNotif> context)
        {
            foreach (var destination in context.Message.Destinations)
            {
                Program.Bus.GetSendEndpoint(new Uri(SharedArea.GlobalVariables.RABBITMQ_SERVER_URL + "/" + destination))
                    .Result.Send(context.Message);
            }
            return Task.CompletedTask;
        }
        
        public Task Consume(ConsumeContext<BotSubscribedNotif> context)
        {
            foreach (var destination in context.Message.Destinations)
            {
                Program.Bus.GetSendEndpoint(new Uri(SharedArea.GlobalVariables.RABBITMQ_SERVER_URL + "/" + destination))
                    .Result.Send(context.Message);
            }
            return Task.CompletedTask;
        }
        
        public Task Consume(ConsumeContext<BotCreatedNotif> context)
        {
            foreach (var destination in context.Message.Destinations)
            {
                Program.Bus.GetSendEndpoint(new Uri(SharedArea.GlobalVariables.RABBITMQ_SERVER_URL + "/" + destination))
                    .Result.Send(context.Message);
            }

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

            return Task.CompletedTask;
        }
        
        public Task Consume(ConsumeContext<PhotoCreatedNotif> context)
        {
            foreach (var destination in context.Message.Destinations)
            {
                Program.Bus.GetSendEndpoint(new Uri(SharedArea.GlobalVariables.RABBITMQ_SERVER_URL + "/" + destination))
                    .Result.Send(context.Message);
            }
            return Task.CompletedTask;
        }
        
        public Task Consume(ConsumeContext<AudioCreatedNotif> context)
        {
            foreach (var destination in context.Message.Destinations)
            {
                Program.Bus.GetSendEndpoint(new Uri(SharedArea.GlobalVariables.RABBITMQ_SERVER_URL + "/" + destination))
                    .Result.Send(context.Message);
            }
            return Task.CompletedTask;
        }
        
        public Task Consume(ConsumeContext<VideoCreatedNotif> context)
        {
            foreach (var destination in context.Message.Destinations)
            {
                Program.Bus.GetSendEndpoint(new Uri(SharedArea.GlobalVariables.RABBITMQ_SERVER_URL + "/" + destination))
                    .Result.Send(context.Message);
            }
            return Task.CompletedTask;
        }
        
        public Task Consume(ConsumeContext<SessionUpdatedNotif> context)
        {
            foreach (var destination in context.Message.Destinations)
            {
                Program.Bus.GetSendEndpoint(new Uri(SharedArea.GlobalVariables.RABBITMQ_SERVER_URL + "/" + destination))
                    .Result.Send(context.Message);
            }
            
            return Task.CompletedTask;
        }
        
        public Task Consume(ConsumeContext<RoomDeletionNotif> context)
        {
            foreach (var destination in context.Message.Destinations)
            {
                Program.Bus.GetSendEndpoint(new Uri(SharedArea.GlobalVariables.RABBITMQ_SERVER_URL + "/" + destination))
                    .Result.Send(context.Message);
            }
            
            return Task.CompletedTask;
        }

        public Task Consume(ConsumeContext<WorkershipCreatedNotif> context)
        {
            foreach (var destination in context.Message.Destinations)
            {
                Program.Bus.GetSendEndpoint(new Uri(SharedArea.GlobalVariables.RABBITMQ_SERVER_URL + "/" + destination))
                    .Result.Send(context.Message);
            }
            
            return Task.CompletedTask;
        }
        
        public Task Consume(ConsumeContext<WorkershipUpdatedNotif> context)
        {
            foreach (var destination in context.Message.Destinations)
            {
                Program.Bus.GetSendEndpoint(new Uri(SharedArea.GlobalVariables.RABBITMQ_SERVER_URL + "/" + destination))
                    .Result.Send(context.Message);
            }
            
            return Task.CompletedTask;
        }
        
        public Task Consume(ConsumeContext<WorkershipDeletedNotif> context)
        {
            foreach (var destination in context.Message.Destinations)
            {
                Program.Bus.GetSendEndpoint(new Uri(SharedArea.GlobalVariables.RABBITMQ_SERVER_URL + "/" + destination))
                    .Result.Send(context.Message);
            }
            
            return Task.CompletedTask;
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
            var result = await SharedArea.Transport.DirectService<ConsolidateContactRequest, ConsolidateContactResponse>(
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
                    
                    dbContext.Notifications.Add(notification);
                }

                dbContext.SaveChanges();

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
                    
                    dbContext.Notifications.Add(notification);
                }

                dbContext.SaveChanges();

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
                    
                    dbContext.Notifications.Add(notification);
                }

                dbContext.SaveChanges();

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
                    
                    dbContext.Notifications.Add(notification);
                }

                dbContext.SaveChanges();

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
                    
                    dbContext.Notifications.Add(notification);
                }

                dbContext.SaveChanges();

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
                    
                    dbContext.Notifications.Add(notification);
                }

                dbContext.SaveChanges();

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
                    
                    dbContext.Notifications.Add(notification);
                }

                dbContext.SaveChanges();

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
                    
                    dbContext.Notifications.Add(notification);
                }

                dbContext.SaveChanges();

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
                    
                    dbContext.Notifications.Add(notification);
                }

                dbContext.SaveChanges();

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
                    
                    dbContext.Notifications.Add(notification);
                }

                dbContext.SaveChanges();

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
                    
                    dbContext.Notifications.Add(notification);
                }

                dbContext.SaveChanges();

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
                    
                    dbContext.Notifications.Add(notification);
                }

                dbContext.SaveChanges();

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
                    
                    dbContext.Notifications.Add(notification);
                }

                dbContext.SaveChanges();

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
                    
                    dbContext.Notifications.Add(notification);
                }

                dbContext.SaveChanges();

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
                    
                    dbContext.Notifications.Add(notification);
                }

                dbContext.SaveChanges();

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
                    
                    dbContext.Notifications.Add(notification);
                }

                dbContext.SaveChanges();

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
                    
                    dbContext.Notifications.Add(notification);
                }

                dbContext.SaveChanges();

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
                    
                    dbContext.Notifications.Add(notification);
                }

                dbContext.SaveChanges();

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
                    
                    dbContext.Notifications.Add(notification);
                }

                dbContext.SaveChanges();

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
                    
                    dbContext.Notifications.Add(notification);
                }

                dbContext.SaveChanges();

                foreach (var sessionId in context.Message.SessionIds)
                {
                    Startup.Pusher.NextPush(sessionId);                    
                }
            }
            
            return Task.CompletedTask;
        }
    }
}