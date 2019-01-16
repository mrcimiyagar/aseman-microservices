namespace SharedArea
{
    public class GlobalVariables
    {
        public const string RABBITMQ_SERVER_URL = "rabbitmq://localhost";
        public const string RABBITMQ_SERVER_PATH = RABBITMQ_SERVER_URL + "?prefetch=32";
        public const string RABBITMQ_USERNAME = "guest", RABBITMQ_PASSWORD = "guest";
        
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
        
        public const string PROFILE_QUEUE_NAME = "ProfileQueue";
        public const string PROFILE_QUEUE_PATH = RABBITMQ_SERVER_URL + "/" + PROFILE_QUEUE_NAME;
        
        public const string SEARCH_QUEUE_NAME = "SearchQueue";
        public const string SEARCH_QUEUE_PATH = RABBITMQ_SERVER_URL + "/" + SEARCH_QUEUE_NAME;
        
        public const int RABBITMQ_REQUEST_TIMEOUT = 30;
    }
}