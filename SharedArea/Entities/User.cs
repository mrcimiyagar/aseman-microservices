using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace SharedArea.Entities
{
    public class User : BaseUser
    {
        [JsonProperty("memberships")]
        public virtual List<Membership> Memberships { get; set; }
        [JsonProperty("contacts")]
        public virtual List<Contact> Contacts { get; set; }
        [JsonProperty("peereds")]
        public virtual List<Contact> Peereds { get; set; }
        [JsonProperty("invites")]
        public virtual List<Invite> Invites { get; set; }
        [JsonProperty("createdBots")]
        public virtual List<BotCreation> CreatedBots { get; set; }
        [JsonProperty("subscribedBots")]
        public virtual List<BotSubscription> SubscribedBots { get; set; }
        [JsonProperty("messageSeens")]
        public virtual List<MessageSeen> MessageSeens { get; set; }
        [JsonIgnore]
        public virtual UserSecret UserSecret { get; set; }

        public User()
        {
            this.Type = "User";
        }
    }
}