using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;
using SharedArea.Entities;

namespace SharedArea.Notifications
{
    public class Notification
    {
        [Key]
        [JsonProperty("notificationId")]
        public long NotificationId { get; set; }
        [JsonProperty("type")]
        public string Type { get; set; }
        [JsonProperty("session")]
        public Session Session { get; set; }
    }
}