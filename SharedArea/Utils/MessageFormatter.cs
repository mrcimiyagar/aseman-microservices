using System.Linq;
using System.Threading.Tasks;
using GreenPipes.Filters.Log;
using MassTransit;

namespace SharedArea.Utils
{
    public class MessageFormatter
    {
        public static async Task<string> Formatter(LogContext<ConsumeContext> context)
        {
            return await Task.Run(() =>
                $"Start-Time: {context.StartTime}, " +
                $"Message-type: {context.Context.SupportedMessageTypes.FirstOrDefault()}, " +
                $"Source-address: {context.Context.SourceAddress}, " +
                $"Destination-address: {context.Context.DestinationAddress}, " +
                $"Fault-address: {context.Context.FaultAddress}");
        }
    }
}