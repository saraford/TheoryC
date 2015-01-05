using System;
using System.Windows.Media;
using Microsoft.Kinect;
using System.IO;
using System.Windows.Media.Imaging;
using System.Windows;
using System.Runtime.InteropServices;

namespace TheoryC
{
    /// <summary>
    /// This class comes from the following article 
    /// http://www.codeproject.com/Articles/730842/Kinect-for-Windows-version-Color-depth-and-infra
    /// licensed under The Code Project Open License 1.02 http://www.codeproject.com/info/cpol10.aspx
    /// </summary>
    public static class ColorExtensions
    {

        public static BitmapSource ToBitmap(this ColorFrame frame)
        {
            int width = frame.FrameDescription.Width;
            int height = frame.FrameDescription.Height;
            PixelFormat format = PixelFormats.Bgr32;

            byte[] pixels = new byte[width * height * ((format.BitsPerPixel + 7) / 8)];

            if (frame.RawColorImageFormat == ColorImageFormat.Bgra)
            {
                frame.CopyRawFrameDataToArray(pixels);
            }
            else
            {
                frame.CopyConvertedFrameDataToArray(pixels, ColorImageFormat.Bgra);
            }

            int stride = width * format.BitsPerPixel / 8;

            return BitmapSource.Create(width, height, 96, 96, format, null, pixels, stride);
        }
    }
}
