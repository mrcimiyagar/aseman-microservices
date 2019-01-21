using Microsoft.AspNetCore.Http;

namespace ApiGateway.Models.Forms
{
    public class TakeFileDSF
    {
        public IFormFile File { get; set; }
        public string StreamCode { get; set; }
    }
}