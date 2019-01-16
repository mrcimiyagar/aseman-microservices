using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace SharedArea.Entities
{
    public class Contact
    {
        [Key]
        [JsonProperty("contactId")]
        public long ContactId { get; set; }
        [JsonProperty("complexId")]
        public long? ComplexId { get; set; }
        [JsonProperty("complex")]
        public virtual Complex Complex { get; set; }
        [JsonProperty("userId")]
        public long? UserId { get; set; }
        [JsonProperty("user")]
        public virtual User User { get; set; }
        [JsonProperty("peerId")]
        public long? PeerId { get; set; }
        [JsonProperty("peer")]
        public virtual User Peer { get; set; }
    }
}