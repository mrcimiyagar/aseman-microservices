namespace SharedArea.Commands.File
{
    public class DownloadBotAvatarRequest : Request
    {
        public string StreamCode { get; set; }
        public long BotId { get; set; }
    }
}