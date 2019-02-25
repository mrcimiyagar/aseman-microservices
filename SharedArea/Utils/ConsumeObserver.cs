using System;
using System.Threading.Tasks;
using MassTransit;

namespace SharedArea.Utils
{
    public class ConsumeObserver : IConsumeObserver
    {    
        Task IConsumeObserver.PreConsume<T>(ConsumeContext<T> context)
        {            
            var content = JsonSerializer.SerializeObject(context.Message);
            Console.WriteLine($"== Consuming ===========================================" + Environment.NewLine +
                              $"Message-type: {context.Message}, " + Environment.NewLine +
                              $"Content: " + (content.Length > 500 ? "body too big to be printed" : content) + Environment.NewLine +
                              $"Source-address: {context.SourceAddress}, " + Environment.NewLine +
                              $"Destination-address: {context.DestinationAddress}, " + Environment.NewLine +
                              $"Fault-address: {context.FaultAddress}" + Environment.NewLine +
                              $"========================================================" + Environment.NewLine);
            return Task.CompletedTask;
        }

        Task IConsumeObserver.PostConsume<T>(ConsumeContext<T> context)
        {
            // called after the consumer's Consume method is called
            // if an exception was thrown, the ConsumeFault method is called instead
            return Task.CompletedTask;
        }

        Task IConsumeObserver.ConsumeFault<T>(ConsumeContext<T> context, Exception exception)
        {         
            var content = JsonSerializer.SerializeObject(context.Message);
            Console.WriteLine($"== Error Consuming =====================================" + Environment.NewLine +
                              $"Message-type: {context.Message}, " + Environment.NewLine +
                              $"Content: " + (content.Length > 500 ? "body too big to be printed" : content) + Environment.NewLine +
                              $"Source-address: {context.SourceAddress}, " + Environment.NewLine +
                              $"Destination-address: {context.DestinationAddress}, " + Environment.NewLine +
                              $"Fault-address: {context.FaultAddress}" + Environment.NewLine);
            Console.WriteLine("");
            Console.WriteLine(exception.ToString());
            Console.WriteLine($"========================================================");
            return Task.CompletedTask;
        }
    }
}