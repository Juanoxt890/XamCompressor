using System;
using System.Collections.Generic;
using System.Text;

namespace XamCompressor.Models
{
    public class ImageOutputModel
    {
        public byte[] ResizedImageBytes { get; set; }
        public string ResizedImagePath { get; set; }
        public string ImageName { get; set; }
    }
}
