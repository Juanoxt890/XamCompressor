using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using XamCompressor.Models;

namespace XamCompressor.DependencyServices
{
    /// <summary>
    /// The resize image service.
    /// </summary>
    public interface IResizeImageService
    {

        /// <summary>
        /// Resizes the image.
        /// </summary>
        /// <param name="imageBytes">The image bytes.</param>
        /// <returns>An array of byte.</returns>
        Task<ImageOutputModel> ResizeImage(byte[] imageBytes, string imagePath);


        /// <summary>
        /// Like whatsapp compression method
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="path"></param>
        /// <returns></returns>
        Task<ImageOutputModel> ResizeImage(string filePath);
    }
}
