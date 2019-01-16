using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using SharedArea.Notifications;
using Newtonsoft.Json;

namespace SharedArea.Entities
{
    public class Session
    {
        [Key]
        [JsonProperty("sessionId")]
        public long SessionId { get; set; }
        [JsonProperty("token")]
        public string Token { get; set; }
        [JsonProperty("baseUserId")]
        public long? BaseUserId { get; set; }
        [JsonProperty("baseUser")]
        public virtual BaseUser BaseUser { get; set; }
        [JsonProperty("online")]
        public bool Online { get; set; }
        [JsonProperty("connectionId")]
        public string ConnectionId { get; set; }
        [JsonProperty("notifications")]
        public virtual List<Notification> Notifications { get; set; }
    }
}