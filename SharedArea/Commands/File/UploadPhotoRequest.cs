using SharedArea.Forms;

namespace SharedArea.Commands.File
{
    public class UploadPhotoRequest : Request
    {
        public PhotoUF Form { get; set; }
    }
}