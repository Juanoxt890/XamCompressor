using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace XamCompressor.Helpers
{
    public static class StreamHelpers
    {
        public static byte[] ReadFully(this Stream input)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                input.CopyTo(ms);
                return ms.ToArray();
            }
        }
    }
}
