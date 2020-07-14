using System.Windows.Media.Imaging;

namespace SharedWpfControls.Helpers
{
    public static class DpiNormalizer
    {
        public static BitmapSource Normalize(BitmapSource input)
        {
            int stride = input.PixelWidth * 4; // 4 bytes per pixel
            byte[] pixelData = new byte[stride * input.PixelHeight];
            input.CopyPixels(pixelData, stride, 0);
            var dpiNormalizedBitmapImage = BitmapSource.Create(input.PixelWidth, input.PixelHeight,
                96, 96, input.Format, input.Palette, pixelData, stride);
            return dpiNormalizedBitmapImage;
        }
    }
}
