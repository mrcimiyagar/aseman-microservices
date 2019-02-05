﻿using System;
using BotPlatform.Consumers;
using BotPlatform.DbContexts;
using MassTransit;
using MassTransit.NLogIntegration;
using Newtonsoft.Json;
using SharedArea.Utils;

namespace BotPlatform
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
                sbc.UseNLog();
                sbc.ReceiveEndpoint(host, SharedArea.GlobalVariables.BOT_QUEUE_NAME, ep =>
                {
                    ep.Consumer<BotConsumer>();
                    ep.Consumer<NotifConsumer>();
                });
            });

            Bus.Start();
            
            Console.WriteLine("Program Loaded.");
        }
    }
}