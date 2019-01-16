using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace SharedArea.Entities
{
    public class BotStoreBot
    {
        [Key]
        [JsonProperty("botStoreBotId")]
        public long BotStoreBotId { get; set; }
        [JsonProperty("botId")]
        public long? BotId { get; set; }
        [JsonProperty("bot")]
        public virtual Bot Bot { get; set; }
        [JsonProperty("botStoreSectionId")]
        public long? BotStoreSectionId { get; set; }
        [JsonProperty("botStoreSection")]
        public virtual BotStoreSection BotStoreSection { get; set; }
    }
}