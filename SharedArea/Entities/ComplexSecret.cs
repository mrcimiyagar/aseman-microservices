using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace SharedArea.Entities
{
    public class ComplexSecret
    {
        [Key]
        [JsonProperty("complexSecretId")]
        public long ComplexSecretId { get; set; }
        [JsonProperty("complexId")]
        public long? ComplexId { get; set; }
        [JsonProperty("complex")]
        public virtual Complex Complex { get; set; }
        [JsonProperty("adminId")]
        public long? AdminId { get; set; }
        [JsonProperty("admin")]
        public virtual User Admin { get; set; }
    }
}