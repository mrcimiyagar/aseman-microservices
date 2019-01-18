using System;
using System.IO;
using ApiGateway.Consumers;
using MassTransit;
using MassTransit.NLogIntegration;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Newtonsoft.Json;

namespace ApiGateway
{
    public class Program
    {
        public static IBusControl Bus { get; set; } = null;
        
        public static void Main(string[] args)
        {
            RunEndpoint();
            
            CreateWebHostBuilder(args).Build().Run();
        }
        
        static async void RunEndpoint()
        {
            Bus = MassTransit.Bus.Factory.CreateUsingRabbitMq(sbc =>
            {
                var host = sbc.Host(new Uri("rabbitmq://localhost?prefetch=32"), h =>
                {
                    h.Username("guest");
                    h.Password("guest");
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
                sbc.ReceiveEndpoint(host, "ApiGateWayInternalQueue", ep =>
                {
                    ep.Consumer<ApiGatewayInternalConsumer>();
                });
            });

            Bus.Start();
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseUrls("http://*:8080")
                .UseContentRoot(Directory.GetCurrentDirectory())
                .UseIISIntegration()
                .UseStartup<Startup>();
    }
}