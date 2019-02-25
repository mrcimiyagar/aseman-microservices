using GreenPipes;
using MassTransit.ConsumeConfigurators;
using MassTransit.RabbitMqTransport;

namespace SharedArea.Utils
{
    public static class EndpointConfigurator
    {
        public static void ConfigEndpoint(IRabbitMqReceiveEndpointConfigurator ep)
        {
            ep.UseConcurrencyLimit(32);
            ep.PrefetchCount = 32;
            ep.Durable = false;
            ep.AutoDelete = true;
        }

        public static void ConfigConsumer<T>(IConsumerConfigurator<T> cons) where T : class
        {
            cons.UseConcurrencyLimit(32);
        }
    }
}