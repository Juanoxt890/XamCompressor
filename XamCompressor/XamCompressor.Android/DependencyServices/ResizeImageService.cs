using Android.App;
using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Runtime;
using Android.Telecom;
using Android.Views;
using Android.Widget;
using AndroidX.ConstraintLayout.Core;
using Java.IO;
using Java.Security.Cert;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;
using XamCompressor.DependencyServices;
using XamCompressor.Droid.DependencyServices;
using XamCompressor.Droid.Helpers;
using XamCompressor.Models;

[assembly: Dependency(typeof(ResizeImageService))]
namespace XamCompressor.Droid.DependencyServices
{

    public class ResizeImageService : IResizeImageService
    {
        public ImageOutputModel ResizeImage(byte[] imageBytes, string imagePath)
        {
            var file = imagePath.Split('/').Last().Split('.');
            string fileName = file[0];
            string format = file[1];
            string path = imagePath.Replace($"{fileName}.{format}", "");
            // Load the bitmap
            Bitmap originalImage = BitmapFactory.DecodeByteArray(imageBytes, 0, imageBytes.Length);
            Bitmap newBitmap = Bitmap.CreateBitmap(originalImage.Width, originalImage.Height, originalImage.GetConfig());

            Canvas canvas = new Canvas(newBitmap);
            canvas.DrawColor(Android.Graphics.Color.White);
            canvas.DrawBitmap(originalImage, 0, 0, null);

            var ms = new System.IO.MemoryStream();
            string finalPath = $"{path}{fileName}light.{format}";
            System.IO.FileStream os = new System.IO.FileStream(finalPath, System.IO.FileMode.Create);
            newBitmap.Compress(Bitmap.CompressFormat.Jpeg, 50, os);
            newBitmap.Compress(Bitmap.CompressFormat.Jpeg, 50, ms);
            ms.Close();
            os.Close();
            return new ImageOutputModel { ResizedImageBytes = ms.ToArray(), ResizedImagePath = path, ImageName = $"{fileName}light.{format}" };
        }

        /// <summary>
        /// Like whatsapp compression method
        /// </summary>
        /// <param name="fileName">the fileName </param>
        /// <param name="path">the path of the file</param>
        /// <returns></returns>
        public async Task<ImageOutputModel> ResizeImage(string filePath)
        {
            ImageCompression imageCompression = new ImageCompression(MainActivity.Instance);
            imageCompression.Execute(filePath);
            //wait for result
            await imageCompression.GetAsync();
            return new ImageOutputModel { ImageName = imageCompression.FileName, ResizedImagePath = imageCompression.FilePath, ResizedImageBytes = imageCompression.FileBytes };
        }
    }
}