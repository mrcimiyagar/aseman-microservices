using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace SharedArea.Entities
{
    public class BotSubscription
    {
        [Key]
        [JsonProperty("botSubscriptionId")]
        public long BotSubscriptionId { get; set; }
        [JsonProperty("botId")]
        public long? BotId { get; set; }
        [JsonProperty("bot")]
        public virtual Bot Bot { get; set; }
        [JsonProperty("subscriberId")]
        public long? SubscriberId { get; set; }
        [JsonProperty("subscriber")]
        public virtual User Subscriber { get; set; }
    }
}