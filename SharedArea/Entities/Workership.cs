using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace SharedArea.Entities
{
    public class Workership
    {
        [Key]
        [JsonProperty("workershipId")]
        public long WorkershipId { get; set; }
        [JsonProperty("botId")]
        public long BotId { get; set; }
        [JsonProperty("roomId")]
        public long? RoomId { get; set; }
        [JsonProperty("room")]
        public virtual Room Room { get; set; }
        [JsonProperty("posX")]
        public int PosX { get; set; }
        [JsonProperty("posY")]
        public int PosY { get; set; }
        [JsonProperty("width")]
        public int Width { get; set; }
        [JsonProperty("height")]
        public int Height { get; set; }
    }
}