using Newtonsoft.Json;

namespace SharedArea.Entities
{
    public class AudioMessage : Message
    {
        [JsonProperty("audioId")]
        public long? AudioId { get; set; }
        [JsonProperty("audio")]
        public virtual Audio Audio { get; set; }

        public AudioMessage()
        {
            this.Type = "AudioMessage";
        }
    }
}