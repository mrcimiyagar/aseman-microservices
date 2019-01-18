using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using MassTransit;
using SharedArea.Middles;

namespace SharedArea
{
    public static class Transport
    {
        public static void NotifyService<TA>(IBusControl bus, Packet packet, IEnumerable destinations) where TA : class
        {
            var address = new Uri(SharedArea.GlobalVariables.RABBITMQ_SERVER_URL + "/" + SharedArea.GlobalVariables.API_GATEWAY_INTERNAL_QUEUE_NAME);
            bus.GetSendEndpoint(address).Result.Send<TA>(new
            {
                Packet = packet,
                Destinations = destinations
            });
        }

        public static async Task<TB> RequestService<TA, TB>(IBusControl bus, string queueName, Packet packet)
            where TA : class
            where TB : class
        {
            var address = new Uri(SharedArea.GlobalVariables.RABBITMQ_SERVER_URL + "/" + SharedArea.GlobalVariables
                                      .API_GATEWAY_INTERNAL_QUEUE_NAME);
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
            var address = new Uri(SharedArea.GlobalVariables.RABBITMQ_SERVER_URL + "/" + queueName);
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
            var address = new Uri(SharedArea.GlobalVariables.RABBITMQ_SERVER_URL + "/" + queueName);
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
            var address = new Uri(SharedArea.GlobalVariables.RABBITMQ_SERVER_URL + "/" + queueName);
            var requestTimeout = TimeSpan.FromSeconds(SharedArea.GlobalVariables.RABBITMQ_REQUEST_TIMEOUT);
            IRequestClient<TA, TB> client = new MessageRequestClient<TA, TB>(bus, address, requestTimeout);
            var result = await client.Request<TA, TB>(new
            {
                SessionId = sessionId,
                Headers = headers
            });
            return result;
        }
    }
}