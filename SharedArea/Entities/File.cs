using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace SharedArea.Entities
{
    public class File
    {
        [Key]
        [JsonProperty("fileId")]
        public long FileId { get; set; }
        [JsonProperty("isPublic")]
        public bool IsPublic { get; set; }
        [JsonProperty("type")]
        public string Type { get; set; }
        [JsonProperty("fileUsages")]
        public virtual List<FileUsage> FileUsages { get; set; }
    }
}