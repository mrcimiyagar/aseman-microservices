using Microsoft.AspNetCore.Http;

namespace ApiGateway.Models.Forms
{
    public class PhotoUploadForm
    {
        public long ComplexId { get; set; }
        public long RoomId { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public bool IsAvatar { get; set; }
    }
}