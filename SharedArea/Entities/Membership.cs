using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace SharedArea.Entities
{
    public class Membership
    {
        [Key]
        [JsonProperty("membershipId")]
        public long MembershipId { get; set; }
        [JsonProperty("userId")]
        public long? UserId { get; set; }
        [JsonProperty("user")]
        public virtual User User { get; set; }
        [JsonProperty("complexId")]
        public long? ComplexId { get; set; }
        [JsonProperty("complex")]
        public virtual Complex Complex { get; set; }
    }
}