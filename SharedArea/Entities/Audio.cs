using Newtonsoft.Json;

namespace SharedArea.Entities
{
    public class Audio : File
    {
        [JsonProperty("title")]
        public string Title { get; set; }
        [JsonProperty("duration")]
        public long Duration { get; set; }

        public Audio()
        {
            this.Type = "Audio";
        }
    }
}