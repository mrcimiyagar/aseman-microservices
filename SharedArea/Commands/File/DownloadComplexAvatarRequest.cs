namespace SharedArea.Commands.File
{
    public class DownloadComplexAvatarRequest : Request
    {
        public string StreamCode { get; set; }
        public long ComplexId { get; set; }
    }
}