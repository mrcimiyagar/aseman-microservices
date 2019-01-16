using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace SharedArea.Entities
{
    public class Video : File
    {
        [JsonProperty("title")]
        public string Title { get; set; }
        [JsonProperty("duration")]
        public long Duration { get; set; }

        public Video()
        {
            this.Type = "Video";
        }
    }
}