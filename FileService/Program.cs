﻿using System;
using FileService.Consumers;
using FileService.DbContexts;
using GreenPipes;
using MassTransit;
using MassTransit.NLogIntegration;
using Newtonsoft.Json;
using SharedArea.Utils;

namespace FileService
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
                sbc.UseLog(Console.Out, MessageFormatter.Formatter);
                sbc.ReceiveEndpoint(host, SharedArea.GlobalVariables.FILE_QUEUE_NAME, ep =>
                {
                    ep.UseConcurrencyLimit(1024);
                    ep.PrefetchCount = 1024;
                    ep.Consumer<FileConsumer>();
                    ep.Consumer<NotifConsumer>();
                });
            });

            Bus.Start();
        }
    }
}