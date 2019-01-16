using Newtonsoft.Json;

namespace SharedArea.Entities
{
    public class TextMessage : Message
    {
        [JsonProperty("text")]
        public string Text { get; set; }

        public TextMessage()
        {
            this.Type = "TextMessage";
        }
    }
}