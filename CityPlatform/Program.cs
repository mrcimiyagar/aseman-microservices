using System;
using CityPlatform.Consumers;
using MassTransit;

namespace CityPlatform
{
    class Program
    {
        public static IBusControl Bus { get; set; }
        
        static void Main(string[] args)
        {
            Bus = MassTransit.Bus.Factory.CreateUsingRabbitMq(sbc =>
            {
                var host = sbc.Host(new Uri(SharedArea.GlobalVariables.RABBITMQ_SERVER_PATH), h =>
                {
                    h.Username(SharedArea.GlobalVariables.RABBITMQ_USERNAME);
                    h.Password(SharedArea.GlobalVariables.RABBITMQ_PASSWORD);
                });
                sbc.ReceiveEndpoint(host, SharedArea.GlobalVariables.CITY_QUEUE_NAME, ep =>
                {
                    ep.Consumer<CityConsumer>();
                });
            });

            Bus.Start();
        }
    }
}