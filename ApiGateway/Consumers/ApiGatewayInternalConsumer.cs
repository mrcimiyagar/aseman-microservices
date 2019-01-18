using System;
using System.Net;
using System.Threading.Tasks;
using MassTransit;
using SharedArea.Commands.Auth;
using SharedArea.Commands.Internal.Notifications;
using SharedArea.Commands.Internal.Requests;
using SharedArea.Commands.Internal.Responses;
using SharedArea.Middles;

namespace ApiGateway.Consumers
{
    public class ApiGatewayInternalConsumer : IConsumer<UserCreatedNotif>, IConsumer<ComplexCreatedNotif>
        , IConsumer<RoomCreatedNotif>, IConsumer<MembershipCreatedNotif>, IConsumer<SessionCreatedNotif>
        , IConsumer<CreateUserRequest>, IConsumer<CreateComplexRequest>, IConsumer<CreateRoomRequest>
        , IConsumer<CreateMembershipRequest>, IConsumer<CreateSessionRequest>, IConsumer<UpdateUserSecretRequest>
    {
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

        public async Task Consume(ConsumeContext<CreateUserRequest> context)
        {
            var result = await RequestService<CreateUserRequest, CreateUserResponse>(
                context.Message.Destination,
                context.Message.Packet);
            await context.RespondAsync<CreateUserResponse>(result);
        }

        public async Task Consume(ConsumeContext<CreateComplexRequest> context)
        {
            var result = await RequestService<CreateComplexRequest, CreateComplexResponse>(
                context.Message.Destination,
                context.Message.Packet);
            await context.RespondAsync<CreateComplexResponse>(result);
        }

        public async Task Consume(ConsumeContext<CreateRoomRequest> context)
        {
            var result = await RequestService<CreateRoomRequest, CreateRoomResponse>(
                context.Message.Destination,
                context.Message.Packet);
            await context.RespondAsync<CreateRoomResponse>(result);
        }

        public async Task Consume(ConsumeContext<CreateMembershipRequest> context)
        {
            var result = await RequestService<CreateMembershipRequest, CreateMembershipResponse>(
                context.Message.Destination,
                context.Message.Packet);
            await context.RespondAsync<CreateMembershipResponse>(result);
        }
        
        public async Task Consume(ConsumeContext<CreateSessionRequest> context)
        {
            var result = await RequestService<CreateSessionRequest, CreateSessionResponse>(
                context.Message.Destination,
                context.Message.Packet);
            await context.RespondAsync<CreateSessionResponse>(result);
        }
        
        public async Task Consume(ConsumeContext<UpdateUserSecretRequest> context)
        {
            var result = await RequestService<UpdateUserSecretRequest, UpdateUserSecretResponse>(
                context.Message.Destination,
                context.Message.Packet);
            await context.RespondAsync<UpdateUserSecretResponse>(result);
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