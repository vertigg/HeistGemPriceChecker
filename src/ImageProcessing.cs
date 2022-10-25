using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Forms;
using System.Windows.Media.Imaging;

namespace HeistGemChecker.src
{
    /// <summary>
    /// Image manipulation class (Capturing/Post-processing stuff)
    /// </summary>
    internal class ImageProcessing
    {
        public static Bitmap CaptureScreenshot()
        {
            var activeScreen = Screen.PrimaryScreen;
            var bitmap = new Bitmap(activeScreen.Bounds.Width, activeScreen.Bounds.Height, PixelFormat.Format24bppRgb);
            using (var g = Graphics.FromImage(bitmap))
            {
                g.CopyFromScreen(0, 0, 0, 0,
                bitmap.Size, CopyPixelOperation.SourceCopy);
            }
            return bitmap;
        }

        public static void ApplyThreshold(ref Bitmap bmp, short thresholdValue)
        {
            int MaxVal = 768;

            if (thresholdValue < 0) return;
            else if (thresholdValue > MaxVal) return;

            BitmapData bmpData = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);

            unsafe
            {
                int TotalRGB;

                byte* ptr = (byte*)bmpData.Scan0.ToPointer();
                int stopAddress = (int)ptr + bmpData.Stride * bmpData.Height;

                while ((int)ptr != stopAddress)
                {
                    TotalRGB = ptr[0] + ptr[1] + ptr[2];

                    if (TotalRGB <= thresholdValue)
                    {
                        ptr[2] = 0;
                        ptr[1] = 0;
                        ptr[0] = 0;
                    }
                    else
                    {
                        ptr[2] = 255;
                        ptr[1] = 255;
                        ptr[0] = 255;
                    }

                    ptr += 3;
                }
            }

            bmp.UnlockBits(bmpData);
        }

        public static Bitmap GetProcessedScreenshot()
        {
            var bitmap = CaptureScreenshot();
            ApplyThreshold(ref bitmap, 270);
            return bitmap;
        }

        public static BitmapImage BitmapToImageSource(Bitmap bitmap)
        {
            using MemoryStream memory = new();
            bitmap.Save(memory, ImageFormat.Bmp);
            memory.Position = 0;
            BitmapImage bitmapimage = new();
            bitmapimage.BeginInit();
            bitmapimage.StreamSource = memory;
            bitmapimage.CacheOption = BitmapCacheOption.OnLoad;
            bitmapimage.EndInit();
            return bitmapimage;
        }

    }
}
