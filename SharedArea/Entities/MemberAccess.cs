using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace SharedArea.Entities
{
    public class MemberAccess
    {
        [Key]
        [JsonProperty("memberAccessId")]
        public long MemberAccessId { get; set; }
        [JsonProperty("canCreateMessage")]
        public bool CanCreateMessage { get; set; }
        [JsonProperty("canSendInvite")]
        public bool CanSendInvite { get; set; }
        [JsonProperty("canModifyWorkers")]
        public bool CanModifyWorkers { get; set; }
        [JsonProperty("canUpdateProfiles")]
        public bool CanUpdateProfiles { get; set; }
        [JsonProperty("canModifyAccess")]
        public bool CanModifyAccess { get; set; }
        [JsonProperty("membershipId")]
        public long? MembershipId { get; set; }
        [JsonProperty("membership")]
        public virtual Membership Membership { get; set; }
    }
}