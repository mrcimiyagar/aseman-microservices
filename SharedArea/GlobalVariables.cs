using System.Data;
using System.Linq;
using System.Runtime.Serialization;

namespace SharedArea
{
    public class GlobalVariables
    {
        public const string RABBITMQ_SERVER_URL = "rabbitmq://localhost";
        public const string RABBITMQ_SERVER_PATH = RABBITMQ_SERVER_URL + "?prefetch=32";
        public const string RABBITMQ_USERNAME = "guest", RABBITMQ_PASSWORD = "guest";
        public const string FILE_TRANSFER_USERNAME = "guest", FILE_TRANSFER_PASSWORD = "guest";
        public const string SERVER_URL = "http://localhost:8080/";
        public const string FILE_TRANSFER_GET_UPLOAD_STREAM_URL = "api/file/get_file_upload_stream";
        public const string FILE_TRANSFER_NOTIFY_GET_UPLOAD_STREAM_FINISHED_URL = "api/file/notify_file_transffered";
        public const string FILE_TRANSFER_TAKE_DOWNLOAD_STREAM_URL = "api/file/take_file_download_stream";
        
        public const string API_GATEWAY_INTERNAL_QUEUE_NAME = "ApiGateWayInternalQueue";
        public const string API_GATEWAY_INTERNAL_QUEUE_PATH = RABBITMQ_SERVER_URL + "/" + API_GATEWAY_INTERNAL_QUEUE_NAME;
        
        public const string CITY_QUEUE_NAME = "CityQueue";
        public const string CITY_QUEUE_PATH = RABBITMQ_SERVER_URL + "/" + CITY_QUEUE_NAME;
        
        public const string BOT_QUEUE_NAME = "BotQueue";
        public const string Bot_QUEUE_PATH = RABBITMQ_SERVER_URL + "/" + BOT_QUEUE_NAME;
        
        public const string DESKTOP_QUEUE_NAME = "DesktopQueue";
        public const string DESKTOP_QUEUE_PATH = RABBITMQ_SERVER_URL + "/" + DESKTOP_QUEUE_NAME;
        
        public const string ENTRY_QUEUE_NAME = "EntryQueue";
        public const string ENTRY_QUEUE_PATH = RABBITMQ_SERVER_URL + "/" + ENTRY_QUEUE_NAME;
        
        public const string MESSENGER_QUEUE_NAME = "MessengerQueue";
        public const string MESSENGER_QUEUE_PATH = RABBITMQ_SERVER_URL + "/" + MESSENGER_QUEUE_NAME;
        
        public const string STORE_QUEUE_NAME = "StoreQueue";
        public const string STORE_QUEUE_PATH = RABBITMQ_SERVER_URL + "/" + STORE_QUEUE_NAME;
        
        public const string SEARCH_QUEUE_NAME = "SearchQueue";
        public const string SEARCH_QUEUE_PATH = RABBITMQ_SERVER_URL + "/" + SEARCH_QUEUE_NAME;
        
        public const string FILE_QUEUE_NAME = "FileQueue";
        public const string FILE_QUEUE_PATH = RABBITMQ_SERVER_URL + "/" + FILE_QUEUE_NAME;
        
        public const int RABBITMQ_REQUEST_TIMEOUT = 30;

        public static string[] AllQueuesExcept(string[] queueNames)
        {
            var queues = new string[]
            {
                BOT_QUEUE_NAME, CITY_QUEUE_NAME, DESKTOP_QUEUE_NAME, ENTRY_QUEUE_NAME, MESSENGER_QUEUE_NAME,
                STORE_QUEUE_NAME, SEARCH_QUEUE_NAME
            }.ToList();
            foreach (var queueName in queueNames)
            {
                queues.Remove(queueName);
            }
            return queues.ToArray();
        }
    }
}