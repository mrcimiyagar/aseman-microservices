namespace SharedArea.Commands.File
{
    public class DownloadUserAvatarRequest : Request
    {
        public string StreamCode { get; set; }
        public long UserId { get; set; }
    }
}