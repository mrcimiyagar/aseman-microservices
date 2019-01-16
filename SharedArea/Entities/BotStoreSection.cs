using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace SharedArea.Entities
{
    public class BotStoreSection
    {
        [Key]
        [JsonProperty("botStoreSectionId")]
        public long BotStoreSectionId { get; set; }
        [JsonProperty("botStoreBots")]
        public virtual List<BotStoreBot> BotStoreBots { get; set; }
    }
}