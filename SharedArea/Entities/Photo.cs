using Newtonsoft.Json;

namespace SharedArea.Entities
{
    public class Photo : File
    {
        [JsonProperty("width")]
        public int Width { get; set; }
        [JsonProperty("height")]
        public int Height { get; set; }

        public Photo()
        {
            this.Type = "Photo";
        }
    }
}