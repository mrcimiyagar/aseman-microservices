using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace SharedArea.Entities
{
    public class BotStoreBanner
    {
        [Key]
        [JsonProperty("botStoreBannerId")]
        public long BotStoreBannerId { get; set; }
        [JsonProperty("botId")]
        public long? BotId { get; set; }
        [JsonProperty("bot")]
        public virtual Bot Bot { get; set; }
        [JsonProperty("title")]
        public string Title { get; set; }
        [JsonProperty("imagePath")]
        public string ImagePath { get; set; }
    }
}