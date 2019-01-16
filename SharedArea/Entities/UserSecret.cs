using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;

namespace SharedArea.Entities
{
    public class UserSecret
    {
        [Key]
        [JsonProperty("userSecretId")]
        public long UserSecretId { get; set; }
        [JsonProperty("homeId")]
        public long? HomeId { get; set; }
        [JsonProperty("home")]
        public virtual Complex Home { get; set; }
        [JsonProperty("email")]
        public string Email { get; set; }
        [JsonProperty("userId")]
        public long? UserId { get; set; }
        [JsonProperty("user")]
        public virtual User User { get; set; }
    }
}