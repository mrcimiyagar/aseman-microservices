using System;
using System.Threading.Tasks;
using MassTransit;
using SharedArea.Commands.Auth;
using SharedArea.Network;

namespace ApiGateway.Consumers
{
    public class ApiGatewayInternalConsumer : IConsumer<RegisterRequest>
    {
        public async Task Consume(ConsumeContext<RegisterRequest> context)
        {
            Console.WriteLine($"Received: {context.Message.Packet.Message}");
            await context.RespondAsync<RegisterResponse>(
                new RegisterResponse()
                {
                    Packet = new Packet()
                    {
                        Message = "hello keyhan"
                    }
                });
        }
    }
}