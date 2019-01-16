using Newtonsoft.Json;

namespace SharedArea.Entities
{
    public class ServiceMessage : Message
    {
        [JsonProperty("text")]
        public string Text { get; set; }

        public ServiceMessage()
        {
            this.Type = "ServiceMessage";
        }
    }
}