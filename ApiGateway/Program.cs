using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ApiGateway.Consumers;
using MassTransit;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SharedArea.Commands.Auth;

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
                sbc.ReceiveEndpoint(host, "ApiGateWayInternalQueue", ep =>
                {
                    ep.Consumer<ApiGatewayInternalConsumer>();
                });
            });

            Bus.Start();
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>();
    }
}