using Android.App;
using Android.Content;
using Android.Graphics;
using Android.Media;
using Android.Net;
using Android.OS;
using Android.Provider;
using Android.Runtime;
using Android.Systems;
using Android.Util;
using Android.Views;
using Android.Widget;
using Java.IO;
using Java.Lang;
using Java.Lang.Reflect;
using Xamarin.Forms.PlatformConfiguration.AndroidSpecific;
using XamCompressor.Models;
using Environment = Android.OS.Environment;

namespace XamCompressor.Droid.Helpers
{
    public class ImageResizerPlus
    {
        public static ImageOutputModel compressImage(string imageUri)
        {

            string filePath = getRealPathFromURI(imageUri);
            Bitmap scaledBitmap = null;

            BitmapFactory.Options options = new BitmapFactory.Options();

            //      by setting this field as true, the actual bitmap pixels are not loaded in the memory. Just the bounds are loaded. If
            //      you try the use the bitmap here, you will get null.
            options.InJustDecodeBounds = true;
            Bitmap bmp = BitmapFactory.DecodeFile(filePath, options);

            int actualHeight = options.OutHeight;
            int actualWidth = options.OutWidth;

            //      max Height and width values of the compressed image is taken as 816x612

            float maxHeight = 816.0f;
            float maxWidth = 612.0f;
            float imgRatio = (float)actualWidth / actualHeight;
            float maxRatio = maxWidth / maxHeight;

            //      width and height values are set maintaining the aspect ratio of the image

            if (actualHeight > maxHeight || actualWidth > maxWidth)
            {
                if (imgRatio < maxRatio)
                {
                    imgRatio = maxHeight / actualHeight;
                    actualWidth = (int)(imgRatio * actualWidth);
                    actualHeight = (int)maxHeight;
                }
                else if (imgRatio > maxRatio)
                {
                    imgRatio = maxWidth / actualWidth;
                    actualHeight = (int)(imgRatio * actualHeight);
                    actualWidth = (int)maxWidth;
                }
                else
                {
                    actualHeight = (int)maxHeight;
                    actualWidth = (int)maxWidth;

                }
            }

            //      setting inSampleSize value allows to load a scaled down version of the original image

            options.InSampleSize = calculateInSampleSize(options, actualWidth, actualHeight);

            //      inJustDecodeBounds set to false to load the actual bitmap
            options.InJustDecodeBounds = false;

            //      this options allow android to claim the bitmap memory if it runs low on memory
            if (Build.VERSION.SdkInt >= BuildVersionCodes.Lollipop)
            {
                //options.InBitmap = bmp;
            }
            else
            {
                options.InPurgeable = true;
                options.InInputShareable = true;
            }
            options.InTempStorage = new byte[16 * 1024];

            try
            {
                //          load the bitmap from its path
                bmp = BitmapFactory.DecodeFile(filePath, options);
            }
            catch (OutOfMemoryError exception)
            {
                exception.PrintStackTrace();

            }
            try
            {
                scaledBitmap = Bitmap.CreateBitmap(actualWidth, actualHeight, Bitmap.Config.Argb8888);
            }
            catch (OutOfMemoryError exception)
            {
                exception.PrintStackTrace();
            }

            float ratioX = actualWidth / (float)options.OutWidth;
            float ratioY = actualHeight / (float)options.OutHeight;
            float middleX = actualWidth / 2.0f;
            float middleY = actualHeight / 2.0f;

            Matrix scaleMatrix = new Matrix();
            scaleMatrix.SetScale(ratioX, ratioY, middleX, middleY);

            Canvas canvas = new Canvas(scaledBitmap);
            canvas.Matrix = scaleMatrix;
            canvas.DrawBitmap(bmp, middleX - bmp.Width / 2, middleY - bmp.Height / 2, new Paint());

            //      check the rotation of the image and display it properly
            ExifInterface exif;
            try
            {
                exif = new ExifInterface(filePath);

                int orientation = exif.GetAttributeInt(
                        ExifInterface.TagOrientation, 0);
                Log.Debug("EXIF", "Exif: " + orientation);
                Matrix matrix = new Matrix();
                if (orientation == 6)
                {
                    matrix.PostRotate(90);
                    Log.Debug("EXIF", "Exif: " + orientation);
                }
                else if (orientation == 3)
                {
                    matrix.PostRotate(180);
                    Log.Debug("EXIF", "Exif: " + orientation);
                }
                else if (orientation == 8)
                {
                    matrix.PostRotate(270);
                    Log.Debug("EXIF", "Exif: " + orientation);
                }
                scaledBitmap = Bitmap.CreateBitmap(scaledBitmap, 0, 0,
                        scaledBitmap.Width, scaledBitmap.Height, matrix,
                        true);
            }
            catch (IOException e)
            {
                e.PrintStackTrace();
            }

            System.IO.FileStream output = null;
            string filename = getFilename();
            var ms = new System.IO.MemoryStream();
            try
            {

                output = new System.IO.FileStream(filename, System.IO.FileMode.OpenOrCreate);

                //          write the compressed bitmap at the destination specified by filename.
                scaledBitmap.Compress(Bitmap.CompressFormat.Jpeg, 80, output);
                scaledBitmap.Compress(Bitmap.CompressFormat.Jpeg, 80, ms);
            }
            catch (FileNotFoundException e)
            {
                e.PrintStackTrace();
            }
            ms.Close();
            return new ImageOutputModel { ImageName = filename, ResizedImageBytes = ms.ToArray(), ResizedImagePath = filename };
        }

        public static string getFilename()
        {
            string directoryPath = "";
            if (Build.VERSION.SdkInt < BuildVersionCodes.Q)
            {
                directoryPath = MediaStore.Images.Media.InternalContentUri.Path;
            }
            else
            {
                directoryPath = Xamarin.Essentials.FileSystem.AppDataDirectory;
            }
            File file = new File(directoryPath, "MTE");
            if (!file.Exists())
            {
                bool created = file.Mkdirs();
            }
            string uriSting = ($"{file.AbsolutePath}/{JavaSystem.CurrentTimeMillis()}light.jpg");
            return uriSting;

        }

        private static string getRealPathFromURI(string contentURI)
        {
            Uri contentUri = Uri.Parse(contentURI);
            Android.Database.ICursor cursor = MainActivity.Instance.ContentResolver.Query(contentUri, null, null, null, null);
            if (cursor == null)
            {
                return contentUri.Path;
            }
            else
            {
                cursor.MoveToFirst();
                int index = cursor.GetColumnIndex(MediaStore.Images.ImageColumns.Data);
                return cursor.GetString(index);
            }
        }

        public static int calculateInSampleSize(BitmapFactory.Options options, int reqWidth, int reqHeight)
        {
            int height = options.OutHeight;
            int width = options.OutWidth;
            int inSampleSize = 1;

            if (height > reqHeight || width > reqWidth)
            {
                int heightRatio = Math.Round((float)height / (float)reqHeight);
                int widthRatio = Math.Round((float)width / (float)reqWidth);
                inSampleSize = heightRatio < widthRatio ? heightRatio : widthRatio;
            }
            float totalPixels = width * height;
            float totalReqPixelsCap = reqWidth * reqHeight * 2;
            while (totalPixels / (inSampleSize * inSampleSize) > totalReqPixelsCap)
            {
                inSampleSize++;
            }

            return inSampleSize;
        }
    }
}