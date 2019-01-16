﻿using Microsoft.AspNetCore.Http;

namespace SharedArea.Forms
{
    public class VideoUploadForm
    {
        public long ComplexId { get; set; }
        public long RoomId { get; set; }
        public string Title { get; set; }
        public long Duration { get; set; }
        public IFormFile File { get; set; }
    }
}