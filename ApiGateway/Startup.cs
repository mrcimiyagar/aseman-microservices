using System;
using ApiGateway.Consumers;
using ApiGateway.DbContexts;
using ApiGateway.Hubs;
using ApiGateway.Middleware;
using ApiGateway.Utils;
using MassTransit;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MongoDB.Driver.Core.Misc;
using Newtonsoft.Json;
using SharedArea.Utils;

namespace ApiGateway
{
    public class Startup
    {
        public static Pusher Pusher { get; set; }
        
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services
                .AddMvc()
                .SetCompatibilityVersion(CompatibilityVersion.Version_2_2)
                .AddJsonOptions(options => options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore)
                .AddJsonOptions(options => options.SerializerSettings.NullValueHandling = NullValueHandling.Ignore);
            services.Configure<FormOptions>(x =>
            {
                x.ValueLengthLimit = int.MaxValue;
                x.MultipartBodyLengthLimit = 4294967296;
            });
            services.AddSignalR(hubOptions =>
            {
                hubOptions.EnableDetailedErrors = true;
            }).AddJsonProtocol((options) =>
            {
                options.PayloadSerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
                options.PayloadSerializerSettings.NullValueHandling = NullValueHandling.Ignore;
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(
            IApplicationBuilder app, 
            IHostingEnvironment env, 
            ILoggerFactory loggerFactory, 
            IServiceProvider serviceProvider,
            IHubContext<NotificationsHub> notifsHub)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }
            
            app.UseMiddleware<LoggerMiddleware>();

            app.UseHttpsRedirection();
            app.UseMvc();
            app.UseSignalR(route =>
            {
                route.MapHub<NotificationsHub>("/NotificationsHub");
            });

            using (var dbContext = new DatabaseContext())
            {
                DatabaseConfig.ConfigDatabase(dbContext);

                foreach (var session in dbContext.Sessions)
                {
                    session.Online = false;
                    session.ConnectionId = "";
                }

                dbContext.SaveChanges();
            }
            
            Logger.Setup();
            MongoLayer.Setup();
            
            DatabaseConfig.ConfigMongoNotifDb(new MongoLayer().GetNotifsColl());
            
            Pusher = new Pusher(notifsHub);
                        
            Program.Bus = MassTransit.Bus.Factory.CreateUsingRabbitMq(sbc =>
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
                    options.Converters.Add(new MessageConverter());
                    return options;
                });
                sbc.ConfigureJsonDeserializer(options =>
                {
                    options.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
                    options.NullValueHandling = NullValueHandling.Ignore;
                    options.Converters.Add(new MessageConverter());
                    return options;
                });
                sbc.ReceiveEndpoint(host, SharedArea.GlobalVariables.API_GATEWAY_INTERNAL_QUEUE_NAME, ep =>
                {
                    EndpointConfigurator.ConfigEndpoint(ep);
                    ep.Consumer<ApiGatewayInternalConsumer>(EndpointConfigurator.ConfigConsumer);
                });
            });

            Program.Bus.ConnectSendObserver(new SendObserver());
            Program.Bus.ConnectConsumeObserver(new ConsumeObserver());
            Program.Bus.ConnectReceiveObserver(new ReceiveObserver());
            
            Program.Bus.Start();
            
            Console.WriteLine("Bus loaded");

            using (var dbContext = new DatabaseContext())
            {
                foreach (var session in dbContext.Sessions)
                {
                    Pusher.NextPush(session.SessionId);
                }
            }
        }
    }
}