
using System.IO;
using MassTransit;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;

namespace ApiGateway
{
    public class Program
    {
        public static IBusControl Bus { get; set; } = null;
        
        public static void Main(string[] args)
        {
            CreateWebHostBuilder(args).Build().Run();
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseUrls("http://*:8080")
                .UseContentRoot(Directory.GetCurrentDirectory())
                .UseIISIntegration()
                .UseStartup<Startup>();
    }
}