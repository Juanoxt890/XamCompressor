using System;
using Xamarin.Forms;
using XamCompressor.iOS.DependencyServices;
using XamCompressor.DependencyServices;
using XamCompressor.Models;
using System.Threading.Tasks;
using UIKit;
using System.IO;
using Foundation;
using System.Diagnostics;
using System.Linq;
using Xamarin.Forms.Shapes;

[assembly: Dependency(typeof(ResizeImageService))]
namespace XamCompressor.iOS.DependencyServices
{
    public class ResizeImageService: IResizeImageService
    {
        string[] file;
        string fileName;
        string format;
        string onlyPath;
        string finalPath;

        public ResizeImageService()
        {
        }

        public async Task<ImageOutputModel> ResizeImage(byte[] originalImageBytes, string imagePath)
        {
            var resizedImageBytes = CompressImage(originalImageBytes, imagePath);
            return new ImageOutputModel() { ResizedImageBytes = resizedImageBytes, ResizedImagePath= finalPath, ImageName= fileName};
        }

        public Task<ImageOutputModel> ResizeImage(string filePath)
        {
            throw new NotImplementedException();
        }


        public byte[] CompressImage(byte[] imageData, string destinationPath, int compressionPercentage = 40)
        {
            file = destinationPath.Split("/").Last().Split(".");
            //get Image name with format
            fileName = file[0];

            //get only the format of image
            format = file[1];

            //get only the path destination 
            onlyPath = destinationPath.Replace($"{fileName}.{format}", "");
            

            UIImage originalImage = ImageFromByteArray(imageData);

            UIImage outputImage = CompressImage(originalImage);

            if(originalImage != null)
            {
                nfloat compressionQuality = (nfloat)(compressionPercentage / 100.0);

                byte[] resizedImage = null;
                resizedImage = outputImage.AsJPEG(/*compressionQuality*/).ToArray();

                var newFileName = $"{fileName}{DateTime.Now.ToString("yyyyMMddHHmmssff")}_tiny.{format}";
                finalPath = $"{onlyPath}{newFileName}";
                var stream = new FileStream(finalPath, FileMode.Create);
                stream.Write(resizedImage, 0, resizedImage.Length);
                stream.Flush();
                stream.Close();
                return resizedImage;
            }
            return imageData;
        }



        public UIImage CompressImage(UIImage image)
        {
            var actualHeight = image.Size.Height;
            var actualWidth = image.Size.Width;

            var maxHeight = 1136.0;

            var maxWidth = 640.0;
            var imageRatio = actualWidth / actualHeight;
            var maxRatio = maxWidth / maxHeight;
            var compressionQuality = 0.5;

            if (actualHeight > maxHeight || actualWidth > maxWidth)
            {
                if(imageRatio < maxRatio)
                {
                    //adjust width according to maxHeight
                    imageRatio = (nfloat)(maxHeight / actualHeight);
                    actualWidth = imageRatio * actualHeight;
                    actualWidth = (nfloat)maxWidth;
                }
                else if(imageRatio > maxRatio)
                {
                    //adjust height according to maxWidth
                    imageRatio = (nfloat)(maxWidth / actualWidth);
                    actualHeight = imageRatio * actualHeight;
                    actualWidth = (nfloat)maxWidth;
                }
                else
                {
                    actualHeight = (nfloat)maxHeight;
                    actualWidth = (nfloat)maxWidth;
                    compressionQuality = 1;
                }
            }

            var rect = new CoreGraphics.CGRect(0.0, 0.0, actualWidth, actualHeight);
            UIGraphics.BeginImageContext(rect.Size);
            image.Draw(rect);
            var img = UIGraphics.GetImageFromCurrentImageContext() ?? null;
            UIGraphics.EndImageContext();
            UIImage imageData = ImageFromByteArray(img.AsJPEG((nfloat)compressionQuality).ToArray()) ?? null;
            return imageData;
        }

        /// <summary>
        /// Converts an byte array to an UIImage Object
        /// </summary>
        /// <param name="imageData"></param>
        /// <returns></returns>
        private UIImage ImageFromByteArray(byte[] imageData)
        {
            if(imageData == null)
            {
                return null;
            }

            UIImage image;
            try
            {
                image = new UIImage(NSData.FromArray(imageData));
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
                return null;
            }
            return image;
        }
    }
}

