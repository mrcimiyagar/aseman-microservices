using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace SharedArea.Entities
{
    public class BaseUser
    {
        [Key]
        [JsonProperty("baseUserId")]
        public long BaseUserId { get; set; }
        [JsonProperty("title")]
        public string Title { get; set; }
        [JsonProperty("avatar")]
        public long Avatar { get; set; }
        [JsonProperty("sessions")]
        public virtual List<Session> Sessions { get; set; }
        [JsonProperty("type")]
        public string Type { get; set; } 
    }
}