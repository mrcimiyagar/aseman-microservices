using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace SharedArea.Entities
{
    public class MessageSeen
    {
        [Key]
        [JsonProperty("messageSeenId")]
        public string MessageSeenId { get; set; }
        [JsonProperty("userId")]
        public long? UserId { get; set; }
        [JsonProperty("User")]
        public virtual User User { get; set; }
        [JsonProperty("messageId")]
        public long? MessageId { get; set; }
        [JsonProperty("Message")]
        public virtual Message Message { get; set; }
    }
}