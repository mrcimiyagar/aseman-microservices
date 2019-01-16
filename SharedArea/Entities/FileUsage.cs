using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace SharedArea.Entities
{
    public class FileUsage
    {
        [Key]
        [JsonProperty("fileUsageId")]
        public long FileUsageId { get; set; }
        [JsonProperty("fileId")]
        public long? FileId { get; set; }
        [JsonProperty("file")]
        public virtual File File { get; set; }
        [JsonProperty("roomId")]
        public long? RoomId { get; set; }
        [JsonProperty("room")]
        public virtual Room Room { get; set; }
    }
}