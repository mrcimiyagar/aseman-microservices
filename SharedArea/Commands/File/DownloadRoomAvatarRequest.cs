namespace SharedArea.Commands.File
{
    public class DownloadRoomAvatarRequest : Request
    {
        public string StreamCode { get; set; }
        public long ComplexId { get; set; }
        public long RoomId { get; set; }
    }
}