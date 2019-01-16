using Newtonsoft.Json;

namespace SharedArea.Entities
{
    public class PhotoMessage : Message
    {
        [JsonProperty("photoId")]
        public long? PhotoId { get; set; }
        [JsonProperty("photo")]
        public virtual Photo Photo { get; set; }

        public PhotoMessage()
        {
            this.Type = "PhotoMessage";
        }
    }
}