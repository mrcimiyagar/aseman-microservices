using System;
using MassTransit;
using SharedArea.Commands.Auth;

namespace MessengerPlatform
{
    class Program
    {
        public static IBusControl Bus { get; set; }
        
        static void Main(string[] args)
        {
            Bus = MassTransit.Bus.Factory.CreateUsingRabbitMq(sbc =>
            {
                var host = sbc.Host(new Uri(SharedArea.GlobalVariables.RABBITMQ_SERVER_PATH), h =>
                {
                    h.Username(SharedArea.GlobalVariables.RABBITMQ_USERNAME);
                    h.Password(SharedArea.GlobalVariables.RABBITMQ_PASSWORD);
                });
                
            });

            Bus.Start();
        }
    }
}