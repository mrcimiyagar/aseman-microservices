using Newtonsoft.Json;

namespace SharedArea.Entities
{
    public class Bot : BaseUser
    {
        [JsonProperty("description")]
        public string Description { get; set; }
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