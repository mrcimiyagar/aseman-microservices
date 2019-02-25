using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;

namespace SharedArea.Entities
{
    public class Room
    {
        [Key]
        [JsonProperty("roomId")]
        public long RoomId { get; set; }
        [JsonProperty("title")]
        public string Title { get; set; }
        [JsonProperty("avatar")]
        public long Avatar { get; set; }
        [JsonProperty("complexId")]
        public long? ComplexId { get; set; }
        [JsonProperty("complex")]
        public virtual Complex Complex { get; set; }
        [JsonProperty("workers")]
        public virtual List<Workership> Workers { get; set; }
        [JsonProperty("messages")]
        public virtual List<Message> Messages { get; set; }
        [JsonProperty("files")]
        public virtual List<FileUsage> Files { get; set; }
        [NotMapped]
        [JsonProperty("lastAction")]
        public virtual Message LastAction { get; set; }
    }
}