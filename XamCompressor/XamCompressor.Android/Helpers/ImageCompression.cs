using Android.Content;
using Android.Graphics;
using Android.Media;
using Android.OS;
using Java.Lang;
using Java.Sql;
using System;
using System.IO;
using System.Linq;
using Exception = Java.Lang.Exception;
using File = Java.IO.File;
using Math = System.Math;
using Void = Java.Lang.Void;

namespace XamCompressor.Droid.Helpers
{
    public class ImageCompression : AsyncTask<string, Void, string>
    {
        private Context context;
        private static float maxHeight = 1280.0f;
        private static float maxWidth = 1280.0f;
        public string FileName;
        public string FilePath;
        public byte[] FileBytes;
        public ImageCompression(Context context)
        {
            this.context = context;
        }


        protected override string RunInBackground(params string[] @params)
        {
            if (@params.Length == 0 || @params[0] == null)
                return null;

            return compressImage(@params[0]);
        }

        protected override void OnPostExecute(string imagePath)
        {
            // imagePath is path of new compressed image.
        }


        public string compressImage(string imagePath)
        {
            Bitmap scaledBitmap = null;

            BitmapFactory.Options options = new BitmapFactory.Options();
            options.InJustDecodeBounds = true;
            Bitmap bmp = BitmapFactory.DecodeFile(imagePath, options);

            int actualHeight = options.OutHeight;
            int actualWidth = options.OutWidth;

            float imgRatio = (float)actualWidth / (float)actualHeight;
            float maxRatio = maxWidth / maxHeight;

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

            options.InSampleSize = calculateInSampleSize(options, actualWidth, actualHeight);
            options.InJustDecodeBounds = false;
            options.InDither = false;
            options.InPurgeable = true;
            options.InInputShareable = true;
            options.InTempStorage = new byte[16 * 1024];

            try
            {
                bmp = BitmapFactory.DecodeFile(imagePath, options);
            }
            catch (OutOfMemoryError exception)
            {
                exception.PrintStackTrace();

            }
            try
            {
                scaledBitmap = Bitmap.CreateBitmap(actualWidth, actualHeight, Bitmap.Config.Rgb565);
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
            canvas.DrawBitmap(bmp, middleX - bmp.Width / 2, middleY - bmp.Height / 2, new Paint(PaintFlags.FilterBitmap));

            if (bmp != null)
            {
                bmp.Recycle();
            }

            ExifInterface exif;
            try
            {
                exif = new ExifInterface(imagePath);
                int orientation = exif.GetAttributeInt(ExifInterface.TagOrientation, 0);
                Matrix matrix = new Matrix();
                if (orientation == 6)
                {
                    matrix.PostRotate(90);
                }
                else if (orientation == 3)
                {
                    matrix.PostRotate(180);
                }
                else if (orientation == 8)
                {
                    matrix.PostRotate(270);
                }
                scaledBitmap = Bitmap.CreateBitmap(scaledBitmap, 0, 0, scaledBitmap.Width, scaledBitmap.Height, matrix, true);
            }
            catch (Exception e)
            {
                e.PrintStackTrace();
            }
            FileStream output = null;
            string filepath = getFilename(imagePath);
            try
            {
                output = new FileStream(filepath, FileMode.Create);

                //write the compressed bitmap at the destination specified by filename.
                scaledBitmap.Compress(Bitmap.CompressFormat.Jpeg, 80, output);
                FileBytes = System.IO.File.ReadAllBytes(filepath);
            }
            catch (Exception e)
            {
                e.PrintStackTrace();
            }

            return filepath;
        }
        public static int calculateInSampleSize(BitmapFactory.Options options, int reqWidth, int reqHeight)
        {
            int height = options.OutHeight;
            int width = options.OutWidth;
            int inSampleSize = 1;

            if (height > reqHeight || width > reqWidth)
            {
                int heightRatio = (int)Math.Round(height / (float)reqHeight);
                int widthRatio = (int)Math.Round(width / (float)reqWidth);
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
        public string getFilename(string path)
        {

            File mediaStorageDir = new File(path);
            string[] file = mediaStorageDir.Name.Split(".");
            string onlyFileName = file.First();
            string format = file.Last();
            // Create the storage directory if it does not exist
            if (!mediaStorageDir.Exists())
            {
                mediaStorageDir.Mkdirs();
            }

            string mImageName = $"{onlyFileName}{DateTime.Now.ToString("yyyyMMddHHmmssff")}_tiny.{format}";
            string uriString = $"{mediaStorageDir.Parent}/{mImageName}";
            FileName = mImageName;
            FilePath = uriString;
            return uriString;

        }
    }
}