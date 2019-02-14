using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace SharedArea.Entities
{
    public class Message
    {
        [Key]
        [JsonProperty("messageId")]
        public long MessageId { get; set; }
        [JsonProperty("time")]
        public long Time { get; set; }
        [JsonProperty("type")]
        public string Type { get; set; }
        [JsonProperty("authorId")]
        public long? AuthorId { get; set; }
        [JsonProperty("author")]
        public virtual BaseUser Author { get; set; }
        [JsonProperty("roomId")]
        public long? RoomId { get; set; }
        [JsonProperty("room")]
        public virtual Room Room { get; set; }
        [JsonProperty("messageSeens")]
        public virtual List<MessageSeen> MessageSeens { get; set; }
    }
}