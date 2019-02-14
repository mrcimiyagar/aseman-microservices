using System.ComponentModel.DataAnnotations;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;
using SharedArea.Entities;

namespace SharedArea.Notifications
{
    public class Notification
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string NotificationId { get; set; }
        [JsonProperty("type")]
        public string Type { get; set; }
        [JsonProperty("session")]
        public Session Session { get; set; }
    }
}