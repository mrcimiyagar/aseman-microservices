using System.Collections.Generic;
using Microsoft.AspNetCore.Http;

namespace ApiGateway.DbContexts
{
    public class StreamRepo
    {
        public static Dictionary<string, IFormFile> FileStreams { get; set; } = new Dictionary<string, IFormFile>();
        public static Dictionary<string, object> FileStreamLocks { get; set; } = new Dictionary<string, object>();
        public static Dictionary<string, object> FileTransferDoneLocks { get; set; } = new Dictionary<string, object>();
    }
}