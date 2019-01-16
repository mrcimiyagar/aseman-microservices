using System.Threading.Tasks;
using MassTransit;
using SharedArea.Commands.Internal;
using SharedArea.Commands.Internal.Notifications;
using SharedArea.Entities;

namespace CityPlatform.Consumers
{
    public class CityConsumer : IConsumer<NotifyComplexCreatedRequest>, IConsumer<NotifyRoomCreatedRequest>
        , IConsumer<NotifyUserCreatedRequest>
    {
        public async Task Consume(ConsumeContext<NotifyComplexCreatedRequest> context)
        {
            
        }

        public async Task Consume(ConsumeContext<NotifyRoomCreatedRequest> context)
        {
            
        }

        public async Task Consume(ConsumeContext<NotifyUserCreatedRequest> context)
        {
            
        }
    }
}