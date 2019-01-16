using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace SharedArea.Entities
{
    public class Complex
    {
        [Key]
        [JsonProperty("complexId")]
        public long ComplexId { get; set; }
        [JsonProperty("title")]
        public string Title { get; set; }
        [JsonProperty("avatar")]
        public long Avatar { get; set; }
        [JsonProperty("mode")]
        public short Mode { get; set; }
        [JsonProperty("members")]
        public virtual List<Membership> Members { get; set; }
        [JsonProperty("rooms")]
        public virtual List<Room> Rooms { get; set; }
        [JsonProperty("invites")]
        public virtual List<Invite> Invites { get; set; }
        [JsonIgnore]
        public virtual ComplexSecret ComplexSecret { get; set; }
    }
}