using System;
using System.Linq;
using System.Text;

namespace DataAPI.DataStructures
{
    /// <summary>
    /// From this stackoverflow answer:
    /// https://stackoverflow.com/questions/1397512/find-image-format-using-bitmap-object-in-c-sharp/9446083#9446083
    /// </summary>
    public static class ImageFormatDetector
    {
        public enum ImageFormat
        {
            Bmp,
            Jpeg,
            Gif,
            Tiff,
            Png,
            Unknown
        }

        public static ImageFormat Detect(byte[] bytes)
        {
            // see http://www.mikekunz.com/image_file_header.html  
            var bmp    = Encoding.ASCII.GetBytes("BM");     // BMP
            var gif    = Encoding.ASCII.GetBytes("GIF");    // GIF
            var png    = new byte[] { 137, 80, 78, 71 };    // PNG
            var tiff   = new byte[] { 73, 73, 42 };         // TIFF
            var tiff2  = new byte[] { 77, 77, 42 };         // TIFF
            var jpeg   = new byte[] { 255, 216, 255, 224 }; // jpeg
            var jpeg2  = new byte[] { 255, 216, 255, 225 }; // jpeg canon

            if (bmp.SequenceEqual(bytes.Take(bmp.Length)))
                return ImageFormat.Bmp;

            if (gif.SequenceEqual(bytes.Take(gif.Length)))
                return ImageFormat.Gif;

            if (png.SequenceEqual(bytes.Take(png.Length)))
                return ImageFormat.Png;

            if (tiff.SequenceEqual(bytes.Take(tiff.Length)))
                return ImageFormat.Tiff;

            if (tiff2.SequenceEqual(bytes.Take(tiff2.Length)))
                return ImageFormat.Tiff;

            if (jpeg.SequenceEqual(bytes.Take(jpeg.Length)))
                return ImageFormat.Jpeg;

            if (jpeg2.SequenceEqual(bytes.Take(jpeg2.Length)))
                return ImageFormat.Jpeg;

            return ImageFormat.Unknown;
        }
    }

    public static class ImageFormatExtensions
    {
        public static string GetFileExtension(this ImageFormatDetector.ImageFormat imageFormat)
        {
            switch (imageFormat)
            {
                case ImageFormatDetector.ImageFormat.Bmp:
                    return ".bmp";
                case ImageFormatDetector.ImageFormat.Jpeg:
                    return ".jpg";
                case ImageFormatDetector.ImageFormat.Gif:
                    return ".gif";
                case ImageFormatDetector.ImageFormat.Tiff:
                    return ".tiff";
                case ImageFormatDetector.ImageFormat.Png:
                    return ".png";
                default:
                    throw new ArgumentOutOfRangeException(nameof(imageFormat), imageFormat, null);
            }
        }
    }
}
