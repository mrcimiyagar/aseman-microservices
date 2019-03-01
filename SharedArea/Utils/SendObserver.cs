using System;
using System.Threading.Tasks;
using MassTransit;

namespace SharedArea.Utils
{
    public class SendObserver : ISendObserver
    {
        public Task PreSend<T>(SendContext<T> context)
            where T : class
        {
            var content = JsonSerializer.SerializeObject(context.Message);
            Logger.Log("Microservices Bus", $"== Sending =============================================" + Environment.NewLine +
                              $"Message-type: {context.Message}, " + Environment.NewLine +
                              $"Content: " + (content.Length > 500 ? "body too big to be printed" : content) + Environment.NewLine +
                              $"Source-address: {context.SourceAddress}, " + Environment.NewLine +
                              $"Destination-address: {context.DestinationAddress}, " + Environment.NewLine +
                              $"Fault-address: {context.FaultAddress}" + Environment.NewLine +
                              $"========================================================");
            return Task.CompletedTask;
        }

        public Task PostSend<T>(SendContext<T> context)
            where T : class
        {
            // called just after a message it sent to the transport and acknowledged (RabbitMQ)
            return Task.CompletedTask;
        }

        public Task SendFault<T>(SendContext<T> context, Exception exception)
            where T : class
        {
            var content = JsonSerializer.SerializeObject(context.Message);
            Logger.Log("Microservices Bus", $"== Error Sending =======================================" + Environment.NewLine +
                              $"Message-type: {context.Message}, " + Environment.NewLine +
                              $"Content: " + (content.Length > 500 ? "body too big to be printed" : content) + Environment.NewLine +
                              $"Source-address: {context.SourceAddress}, " + Environment.NewLine +
                              $"Destination-address: {context.DestinationAddress}, " + Environment.NewLine +
                              $"Fault-address: {context.FaultAddress}" + Environment.NewLine+
                              $"" + Environment.NewLine +
                              $"" + exception.ToString() + Environment.NewLine +
                              $"========================================================");
            return Task.CompletedTask;
        }
    }
}