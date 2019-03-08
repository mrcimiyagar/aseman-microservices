using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MassTransit;
using SharedArea.Commands;
using SharedArea.Commands.File;
using SharedArea.Forms;
using SharedArea.Middles;

namespace SharedArea
{
    public static class Transport
    {
        public static void NotifyService<TA, TB>(IBusControl bus, Packet packet, IEnumerable destinations)
            where TA : class
            where TB : class
        {
            var address = new Uri(SharedArea.GlobalVariables.RABBITMQ_SERVER_URL + "/" + SharedArea.GlobalVariables.API_GATEWAY_INTERNAL_QUEUE_NAME
                                  + "?autodelete=true&durable=false&temporary=true");
            var requestTimeout = TimeSpan.FromSeconds(SharedArea.GlobalVariables.RABBITMQ_REQUEST_TIMEOUT);
            IRequestClient<TA, TB> client = new MessageRequestClient<TA, TB>(bus, address, requestTimeout);
            var enumerable = destinations.Cast<object>().ToArray();
            client.Request(new
            {
                Packet = packet,
                Destinations = enumerable
            });
        }

        public static async Task<TB> RequestService<TA, TB>(IBusControl bus, string queueName, Packet packet)
            where TA : class
            where TB : class
        {
            var address = new Uri(SharedArea.GlobalVariables.RABBITMQ_SERVER_URL + "/" + SharedArea.GlobalVariables
                                      .API_GATEWAY_INTERNAL_QUEUE_NAME+ "?autodelete=true&durable=false&temporary=true");
            var requestTimeout = TimeSpan.FromSeconds(SharedArea.GlobalVariables.RABBITMQ_REQUEST_TIMEOUT);
            IRequestClient<TA, TB> client = new MessageRequestClient<TA, TB>(bus, address, requestTimeout);
            var result = await client.Request<TA, TB>(new
            {
                Packet = packet,
                Destination = queueName
            });
            return result;
        }
        
        public static async Task<TB> DirectService<TA, TB>(IBusControl bus, string queueName, long sessionId
            , Dictionary<string, string> headers, Packet packet)
            where TA : class
            where TB : class
        {
            var address = new Uri(SharedArea.GlobalVariables.RABBITMQ_SERVER_URL + "/" + queueName + "?autodelete=true&durable=false&temporary=true");
            var requestTimeout = TimeSpan.FromSeconds(SharedArea.GlobalVariables.RABBITMQ_REQUEST_TIMEOUT);
            IRequestClient<TA, TB> client = new MessageRequestClient<TA, TB>(bus, address, requestTimeout);
            var result = await client.Request<TA, TB>(new
            {
                SessionId = sessionId,
                Headers = headers,
                Packet = packet
            });
            return result;
        }
        
        public static async Task<TB> DirectService<TA, TB>(IBusControl bus, string queueName
            , Dictionary<string, string> headers, Packet packet)
            where TA : class
            where TB : class
        {
            var address = new Uri(SharedArea.GlobalVariables.RABBITMQ_SERVER_URL + "/" + queueName + "?autodelete=true&durable=false&temporary=true");
            var requestTimeout = TimeSpan.FromSeconds(SharedArea.GlobalVariables.RABBITMQ_REQUEST_TIMEOUT);
            IRequestClient<TA, TB> client = new MessageRequestClient<TA, TB>(bus, address, requestTimeout);
            var result = await client.Request<TA, TB>(new
            {
                Headers = headers,
                Packet = packet
            });
            return result;
        }
        
        public static async Task<TB> DirectService<TA, TB>(IBusControl bus, string queueName, long sessionId
            , Dictionary<string, string> headers)
            where TA : class
            where TB : class
        {
            var address = new Uri(SharedArea.GlobalVariables.RABBITMQ_SERVER_URL + "/" + queueName + "?autodelete=true&durable=false&temporary=true");
            var requestTimeout = TimeSpan.FromSeconds(SharedArea.GlobalVariables.RABBITMQ_REQUEST_TIMEOUT);
            IRequestClient<TA, TB> client = new MessageRequestClient<TA, TB>(bus, address, requestTimeout);
            var result = await client.Request<TA, TB>(new
            {
                SessionId = sessionId,
                Headers = headers
            });
            return result;
        }
        
        public static async Task<TB> DirectService<TA, TB>(IBusControl bus, string queueName, Packet packet)
            where TA : class
            where TB : class
        {
            var address = new Uri(SharedArea.GlobalVariables.RABBITMQ_SERVER_URL + "/" + queueName + "?autodelete=true&durable=false&temporary=true");
            var requestTimeout = TimeSpan.FromSeconds(SharedArea.GlobalVariables.RABBITMQ_REQUEST_TIMEOUT);
            IRequestClient<TA, TB> client = new MessageRequestClient<TA, TB>(bus, address, requestTimeout);
            var result = await client.Request<TA, TB>(new
            {
                Packet = packet
            });
            return result;
        }
        
        public static async Task<TB> DirectService<TA, TB>(IBusControl bus, string queueName
            , Dictionary<string, string> headers)
            where TA : class
            where TB : class
        {
            var address = new Uri(SharedArea.GlobalVariables.RABBITMQ_SERVER_URL + "/" + queueName + "?autodelete=true&durable=false&temporary=true");
            var requestTimeout = TimeSpan.FromSeconds(SharedArea.GlobalVariables.RABBITMQ_REQUEST_TIMEOUT);
            IRequestClient<TA, TB> client = new MessageRequestClient<TA, TB>(bus, address, requestTimeout);
            var result = await client.Request<TA, TB>(new
            {
                Headers = headers
            });
            return result;
        }
        
        public static void Push<TA>(IBusControl bus, Push push) where TA : class
        {
            var address = new Uri(SharedArea.GlobalVariables.RABBITMQ_SERVER_URL + "/" + SharedArea.GlobalVariables.API_GATEWAY_INTERNAL_QUEUE_NAME
                                  + "?autodelete=true&durable=false&temporary=true");
            bus.GetSendEndpoint(address).Result.Send<TA>(push);
        }
    }
}