﻿using System;
using MassTransit;
using MassTransit.NLogIntegration;
using Newtonsoft.Json;
using ProfilePlatform.Consumers;
using ProfilePlatform.DbContexts;

namespace ProfilePlatform
{
    class Program
    {
        public static IBusControl Bus { get; set; }
        
        static void Main(string[] args)
        {
            using (var dbContext = new DatabaseContext())
            {
                dbContext.Database.EnsureCreated();
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
                sbc.ReceiveEndpoint(host, SharedArea.GlobalVariables.PROFILE_QUEUE_NAME, ep =>
                {
                    ep.Consumer<ProfileConsumer>();
                });
            });

            Bus.Start();
        }
    }
}