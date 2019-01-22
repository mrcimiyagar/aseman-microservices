using System;
using System.Threading.Tasks;
using ApiGateway.DbContexts;
using ApiGateway.Hubs;
using MassTransit;
using Microsoft.AspNetCore.SignalR;
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
        , IConsumer<InviteCancelledNotif>, IConsumer<InviteAcceptedNotif>, IConsumer<InvitedIgnoredNotif>
        , IConsumer<BotProfileUpdatedNotif>, IConsumer<BotSubscribedNotif>, IConsumer<BotCreatedNotif>
        , IConsumer<PhotoCreatedNotif>, IConsumer<AudioCreatedNotif>, IConsumer<VideoCreatedNotif>

        , IConsumer<PutUserRequest>, IConsumer<PutComplexRequest>, IConsumer<PutRoomRequest>
        , IConsumer<PutMembershipRequest>, IConsumer<PutSessionRequest>, IConsumer<UpdateUserSecretRequest>
        
        , IConsumer<ComplexDeletionPush>, IConsumer<RoomDeletionPush>, IConsumer<ContactCreationPush>
        , IConsumer<ServiceMessagePush>, IConsumer<InviteCreationPush>, IConsumer<InviteCancellationPush>
        , IConsumer<UserJointComplexPush>, IConsumer<InviteAcceptancePush>, IConsumer<InviteIgnoredPush>
        , IConsumer<BotAdditionToRoomPush>, IConsumer<BotRemovationFromRoomPush>, IConsumer<TextMessagePush>
        , IConsumer<PhotoMessagePush>, IConsumer<AudioMessagePush>, IConsumer<VideoMessagePush>
    {
        private readonly IHubContext<NotificationsHub> _notifsHub;
        
        public ApiGatewayInternalConsumer(IHubContext<NotificationsHub> notifsHub)
        {
            _notifsHub = notifsHub;
        }
        
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
        
        public Task Consume(ConsumeContext<ComplexDeletionPush> context)
        {
            using (var dbContext = new DatabaseContext())
            {
                foreach (var sessionId in context.Message.SessionIds)
                {
                    var session = dbContext.Sessions.Find(sessionId);

                    var notif = new ComplexDeletionNotification()
                    {
                        ComplexId = context.Message.Notif.ComplexId,
                        Session = session
                    };
                    
                    if (session.Online)
                        _notifsHub.Clients.Client(session.ConnectionId)
                            .SendAsync("NotifyComplexDeleted", notif);
                    else
                    {
                        dbContext.Notifications.Add(notif);
                    }
                }

                dbContext.SaveChanges();
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

                    var notif = new RoomDeletionNotification()
                    {
                        ComplexId = context.Message.Notif.ComplexId,
                        RoomId = context.Message.Notif.RoomId,
                        Session = session
                    };
                    
                    if (session.Online)
                        _notifsHub.Clients.Client(session.ConnectionId)
                            .SendAsync("NotifyRoomDeleted", notif);
                    else
                    {
                        dbContext.Notifications.Add(notif);
                    }
                }

                dbContext.SaveChanges();
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

                    var ccn = new ContactCreationNotification()
                    {
                        Contact = context.Message.Notif.Contact,
                        Session = s
                    };                    
                    if (s.Online)
                    {
                        _notifsHub.Clients.Client(s.ConnectionId)
                            .SendAsync("NotifyContactCreated", ccn);
                    } 
                    else
                    {
                        dbContext.Notifications.Add(ccn);
                    }   
                }

                dbContext.SaveChanges();
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

                    var mcn = new ServiceMessageNotification()
                    {
                        Message = context.Message.Notif.Message,
                        Session = s
                    };                    
                    if (s.Online)
                    {
                        _notifsHub.Clients.Client(s.ConnectionId)
                            .SendAsync("NotifyServiceMessageReceived", mcn);
                    } 
                    else
                    {
                        dbContext.Notifications.Add(mcn);
                    }   
                }

                dbContext.SaveChanges();
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

                    var icn = new InviteCreationNotification()
                    {
                        Invite = context.Message.Notif.Invite,
                        Session = s
                    };
                    if (s.Online)
                    {
                        _notifsHub.Clients.Client(s.ConnectionId)
                            .SendAsync("NotifyInviteCreated", icn);
                    }
                    else
                    {
                        dbContext.Notifications.Add(icn);
                    }  
                }

                dbContext.SaveChanges();
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

                    var icn = new InviteCancellationNotification()
                    {
                        Invite = context.Message.Notif.Invite,
                        Session = s
                    };
                    if (s.Online)
                    {
                        _notifsHub.Clients.Client(s.ConnectionId)
                            .SendAsync("NotifyInviteCancelled", icn);
                    }
                    else
                    {
                        dbContext.Notifications.Add(icn);
                    }  
                }

                dbContext.SaveChanges();
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

                    var icn = new UserJointComplexNotification()
                    {
                        ComplexId = context.Message.Notif.ComplexId,
                        UserId = context.Message.Notif.UserId,
                        Session = s
                    };
                    if (s.Online)
                    {
                        _notifsHub.Clients.Client(s.ConnectionId)
                            .SendAsync("NotifyUserJointComplex", icn);
                    }
                    else
                    {
                        dbContext.Notifications.Add(icn);
                    }  
                }

                dbContext.SaveChanges();
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

                    var icn = new InviteAcceptanceNotification()
                    {
                        Invite = context.Message.Notif.Invite,
                        Session = s
                    };
                    if (s.Online)
                    {
                        _notifsHub.Clients.Client(s.ConnectionId)
                            .SendAsync("NotifyInviteAccepted", icn);
                    }
                    else
                    {
                        dbContext.Notifications.Add(icn);
                    }  
                }

                dbContext.SaveChanges();
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

                    var icn = new InviteIgnoranceNotification()
                    {
                        Invite = context.Message.Notif.Invite,
                        Session = s
                    };
                    if (s.Online)
                    {
                        _notifsHub.Clients.Client(s.ConnectionId)
                            .SendAsync("NotifyInviteIgnored", icn);
                    }
                    else
                    {
                        dbContext.Notifications.Add(icn);
                    }  
                }

                dbContext.SaveChanges();
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

                    var addition = new BotAdditionToRoomNotification()
                    {
                        Room = context.Message.Notif.Room,
                        Session = session
                    };
                    
                    if (session.Online)
                    {
                        _notifsHub.Clients.Client(session.ConnectionId)
                            .SendAsync("NotifyBotAddedToRoom", addition);
                    }
                    else
                    {
                        dbContext.Notifications.Add(addition);
                    }
                }

                dbContext.SaveChanges();
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

                    var removation = new BotRemovationFromRoomNotification()
                    {
                        Room = context.Message.Notif.Room,
                        Session = session
                    };
                    
                    if (session.Online)
                    {
                        _notifsHub.Clients.Client(session.ConnectionId)
                            .SendAsync("NotifyBotRemovedFromRoom", removation);
                    }
                    else
                    {
                        dbContext.Notifications.Add(removation);
                    }
                }

                dbContext.SaveChanges();
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
                    
                    if (session.Online)
                    {
                        _notifsHub.Clients.Client(session.ConnectionId)
                            .SendAsync("NotifyTextMessageReceived", notification);
                    }
                    else
                    {
                        dbContext.Notifications.Add(notification);
                    }
                }

                dbContext.SaveChanges();
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
                    
                    if (session.Online)
                    {
                        _notifsHub.Clients.Client(session.ConnectionId)
                            .SendAsync("NotifyPhotoMessageReceived", notification);
                    }
                    else
                    {
                        dbContext.Notifications.Add(notification);
                    }
                }

                dbContext.SaveChanges();
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
                    
                    if (session.Online)
                    {
                        _notifsHub.Clients.Client(session.ConnectionId)
                            .SendAsync("NotifyAudioMessageReceived", notification);
                    }
                    else
                    {
                        dbContext.Notifications.Add(notification);
                    }
                }

                dbContext.SaveChanges();
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
                    
                    if (session.Online)
                    {
                        _notifsHub.Clients.Client(session.ConnectionId)
                            .SendAsync("NotifyVideoMessageReceived", notification);
                    }
                    else
                    {
                        dbContext.Notifications.Add(notification);
                    }
                }

                dbContext.SaveChanges();
            }
            
            return Task.CompletedTask;
        }
    }
}