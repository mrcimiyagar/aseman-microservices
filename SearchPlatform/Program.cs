using System;
using MassTransit;
using Newtonsoft.Json;
using SearchPlatform.Consumers;
using SearchPlatform.DbContexts;
using SharedArea.Utils;

namespace SearchPlatform
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
                sbc.ReceiveEndpoint(host, SharedArea.GlobalVariables.SEARCH_QUEUE_NAME, ep =>
                {
                    EndpointConfigurator.ConfigEndpoint(ep);
                    ep.Consumer<SearchConsumer>(EndpointConfigurator.ConfigConsumer);
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