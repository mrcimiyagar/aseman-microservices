using Newtonsoft.Json;

namespace SharedArea.Entities
{
    public class VideoMessage : Message
    {
        [JsonProperty("videoId")]
        public long? VideoId { get; set; }
        [JsonProperty("video")]
        public virtual Video Video { get; set; }

        public VideoMessage()
        {
            this.Type = "VideoMessage";
        }
    }
}