using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace SharedArea.Entities
{
    public class BotStoreHeader
    {
        [Key]
        [JsonProperty("botStoreHeaderId")]
        public long BotStoreHeaderId { get; set; }
        [JsonProperty("banners")]
        public virtual List<BotStoreBanner> Banners { get; set; }
    }
}