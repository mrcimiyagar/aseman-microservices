using SharedArea.Forms;

namespace SharedArea.Commands.File
{
    public class UploadVideoRequest : Request
    {
        public VideoUF Form { get; set; }
        public string StreamCode { get; set; }
    }
}