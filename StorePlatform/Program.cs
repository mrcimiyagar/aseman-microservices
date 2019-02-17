using System;
using System.Linq;
using System.Threading.Tasks;
using GreenPipes;
using GreenPipes.Filters.Log;
using MassTransit;
using MassTransit.Logging;
using MassTransit.NLogIntegration;
using Newtonsoft.Json;
using Serilog;
using SharedArea.Entities;
using SharedArea.Utils;
using StorePlatform.Consumers;
using StorePlatform.DbContexts;

namespace StorePlatform
{
    class Program
    {
        public static IBusControl Bus { get; set; }

        static void Main(string[] args)
        {
            using (var dbContext = new DatabaseContext())
            {
                DatabaseConfig.ConfigDatabase(dbContext);

                if (dbContext.BotStoreHeader.LongCount() == 0)
                {
                    var header = new BotStoreHeader();

                    dbContext.BotStoreHeader.Add(header);

                    dbContext.SaveChanges();
                }
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
                sbc.UseLog(Console.Out, MessageFormatter.Formatter);
                sbc.ReceiveEndpoint(host, SharedArea.GlobalVariables.STORE_QUEUE_NAME, ep =>
                {
                    ep.Consumer<StoreConsumer>();
                    ep.Consumer<NotifConsumer>();
                });
            });

            Bus.Start();
        }
    }
}