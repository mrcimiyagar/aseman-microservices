using System;
using MassTransit;
using MessengerPlatform.Consumers;
using MessengerPlatform.DbContexts;
using Newtonsoft.Json;
using SharedArea.Utils;

namespace MessengerPlatform
{
    class Program
    {
        public static IBusControl Bus { get; set; }
        
        static void Main(string[] args)
        {
            using (var dbContext = new DatabaseContext())
            {
                DatabaseConfig.ConfigDatabase(dbContext);
            }
            
            Bus = MassTransit.Bus.Factory.CreateUsingRabbitMq(sbc =>
            {
                var host = sbc.Host(new Uri(SharedArea.GlobalVariables.RABBITMQ_SERVER_PATH), h =>
                {
                    h.Username(SharedArea.GlobalVariables.RABBITMQ_USERNAME);
                    h.Password(SharedArea.GlobalVariables.RABBITMQ_PASSWORD);
                });
                sbc.UseJsonSerializer();
                sbc.ConfigureJsonSerializer(options =>
                {
                    options.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
                    options.NullValueHandling = NullValueHandling.Ignore;
                    return options;
                });
                sbc.ConfigureJsonDeserializer(options =>
                {
                    options.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
                    options.NullValueHandling = NullValueHandling.Ignore;
                    return options;
                });
                sbc.ReceiveEndpoint(host, SharedArea.GlobalVariables.MESSENGER_QUEUE_NAME, ep =>
                {
                    EndpointConfigurator.ConfigEndpoint(ep);
                    ep.Consumer<MessengerConsumer>(EndpointConfigurator.ConfigConsumer);
                    ep.Consumer<NotifConsumer>(EndpointConfigurator.ConfigConsumer);
                });
            });

            Program.Bus.ConnectSendObserver(new SendObserver());
            Program.Bus.ConnectConsumeObserver(new ConsumeObserver());
            Program.Bus.ConnectReceiveObserver(new ReceiveObserver());

            Bus.Start();
            
            Console.WriteLine("Bus loaded");
        }
    }
}