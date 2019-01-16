using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace SharedArea.Entities
{
    public class Bot : BaseUser
    {
        [JsonProperty("viewURL")]
        public string ViewURL { get; set; }
        [JsonProperty("botSecret")]
        public BotSecret BotSecret { get; set; }

        public Bot()
        {
            this.Type = "Bot";
        }
    }
}