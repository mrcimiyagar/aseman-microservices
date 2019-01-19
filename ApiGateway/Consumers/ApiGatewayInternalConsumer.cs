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
using SharedArea.Middles;
using SharedArea.Notifications;

namespace ApiGateway.Consumers
{
    public class ApiGatewayInternalConsumer : IConsumer<UserCreatedNotif>, IConsumer<ComplexCreatedNotif>
        , IConsumer<RoomCreatedNotif>, IConsumer<MembershipCreatedNotif>, IConsumer<SessionCreatedNotif>
        , IConsumer<UserProfileUpdatedNotif>, IConsumer<ComplexProfileUpdatedNotif>, IConsumer<ComplexDeletionNotif>

        , IConsumer<PutUserRequest>, IConsumer<PutComplexRequest>, IConsumer<PutRoomRequest>
        , IConsumer<PutMembershipRequest>, IConsumer<PutSessionRequest>, IConsumer<UpdateUserSecretRequest>
        
        , IConsumer<ComplexDeletionPush>
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
                    .Result.Send<UserCreatedNotif>(context.Message);
            }
            return Task.CompletedTask;
        }

        public Task Consume(ConsumeContext<ComplexCreatedNotif> context)
        {
            foreach (var destination in context.Message.Destinations)
            {
                Program.Bus.GetSendEndpoint(new Uri(SharedArea.GlobalVariables.RABBITMQ_SERVER_URL + "/" + destination))
                    .Result.Send<ComplexCreatedNotif>(context.Message);
            }
            return Task.CompletedTask;
        }

        public Task Consume(ConsumeContext<RoomCreatedNotif> context)
        {
            foreach (var destination in context.Message.Destinations)
            {
                Program.Bus.GetSendEndpoint(new Uri(SharedArea.GlobalVariables.RABBITMQ_SERVER_URL + "/" + destination))
                    .Result.Send<RoomCreatedNotif>(context.Message);
            }
            return Task.CompletedTask;
        }

        public Task Consume(ConsumeContext<MembershipCreatedNotif> context)
        {
            foreach (var destination in context.Message.Destinations)
            {
                Program.Bus.GetSendEndpoint(new Uri(SharedArea.GlobalVariables.RABBITMQ_SERVER_URL + "/" + destination))
                    .Result.Send<MembershipCreatedNotif>(context.Message);
            }
            return Task.CompletedTask;
        }
        
        public Task Consume(ConsumeContext<SessionCreatedNotif> context)
        {
            foreach (var destination in context.Message.Destinations)
            {
                Program.Bus.GetSendEndpoint(new Uri(SharedArea.GlobalVariables.RABBITMQ_SERVER_URL + "/" + destination))
                    .Result.Send<SessionCreatedNotif>(context.Message);
            }
            return Task.CompletedTask;
        }
        
        public Task Consume(ConsumeContext<UserProfileUpdatedNotif> context)
        {
            foreach (var destination in context.Message.Destinations)
            {
                Program.Bus.GetSendEndpoint(new Uri(SharedArea.GlobalVariables.RABBITMQ_SERVER_URL + "/" + destination))
                    .Result.Send<UserProfileUpdatedNotif>(context.Message);
            }
            return Task.CompletedTask;
        }
        
        public Task Consume(ConsumeContext<ComplexProfileUpdatedNotif> context)
        {
            foreach (var destination in context.Message.Destinations)
            {
                Program.Bus.GetSendEndpoint(new Uri(SharedArea.GlobalVariables.RABBITMQ_SERVER_URL + "/" + destination))
                    .Result.Send<ComplexProfileUpdatedNotif>(context.Message);
            }
            return Task.CompletedTask;
        }

        public Task Consume(ConsumeContext<ComplexDeletionNotif> context)
        {
            foreach (var destination in context.Message.Destinations)
            {
                Program.Bus.GetSendEndpoint(new Uri(SharedArea.GlobalVariables.RABBITMQ_SERVER_URL + "/" + destination))
                    .Result.Send<ComplexDeletionNotif>(context.Message);
            }
            return Task.CompletedTask;
        }

        public async Task Consume(ConsumeContext<PutUserRequest> context)
        {
            var result = await RequestService<PutUserRequest, PutUserResponse>(
                context.Message.Destination,
                context.Message.Packet);
            await context.RespondAsync<PutUserResponse>(result);
        }

        public async Task Consume(ConsumeContext<PutComplexRequest> context)
        {
            var result = await RequestService<PutComplexRequest, PutComplexResponse>(
                context.Message.Destination,
                context.Message.Packet);
            await context.RespondAsync<PutComplexResponse>(result);
        }

        public async Task Consume(ConsumeContext<PutRoomRequest> context)
        {
            var result = await RequestService<PutRoomRequest, PutRoomResponse>(
                context.Message.Destination,
                context.Message.Packet);
            await context.RespondAsync<PutRoomResponse>(result);
        }

        public async Task Consume(ConsumeContext<PutMembershipRequest> context)
        {
            var result = await RequestService<PutMembershipRequest, PutMembershipResponse>(
                context.Message.Destination,
                context.Message.Packet);
            await context.RespondAsync<PutMembershipResponse>(result);
        }
        
        public async Task Consume(ConsumeContext<PutSessionRequest> context)
        {
            var result = await RequestService<PutSessionRequest, PutSessionResponse>(
                context.Message.Destination,
                context.Message.Packet);
            await context.RespondAsync<PutSessionResponse>(result);
        }
        
        public async Task Consume(ConsumeContext<UpdateUserSecretRequest> context)
        {
            var result = await RequestService<UpdateUserSecretRequest, UpdateUserSecretResponse>(
                context.Message.Destination,
                context.Message.Packet);
            await context.RespondAsync<UpdateUserSecretResponse>(result);
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
        
        private static async Task<TB> RequestService<TA, TB>(string queueName, Packet packet)
            where TA : class
            where TB : class
        {
            var address = new Uri(SharedArea.GlobalVariables.RABBITMQ_SERVER_URL + "/" + queueName);
            var requestTimeout = TimeSpan.FromSeconds(SharedArea.GlobalVariables.RABBITMQ_REQUEST_TIMEOUT);
            IRequestClient<TA, TB> client = new MessageRequestClient<TA, TB>(Program.Bus, address, requestTimeout);
            var result = await client.Request<TA, TB>(new
            {
                Packet = packet
            });
            return result;
        }
    }
}