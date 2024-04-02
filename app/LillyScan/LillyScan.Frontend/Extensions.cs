using LillyScan.Backend.Imaging;
using LillyScan.Backend.Math;
using Microsoft.Maui.Graphics.Platform;
using IImage = Microsoft.Maui.Graphics.IImage;

namespace LillyScan.Frontend
{
    internal static class Extensions
    {
        public static IImage CropCentered(this IImage image, int percentWidth, int percentHeight)
        {
#if ANDROID
            var newWidth = (int)image.Width * percentWidth / 100;
            var newHeight = (int)image.Height * percentHeight / 100;            
                       
            using(var bmp = image.AsBitmap())
            {
                var buffer = new int[bmp.Width*bmp.Height];
                bmp.GetPixels(buffer, 0, bmp.Width, 0, 0, bmp.Width, bmp.Height);
                var x0 = (bmp.Width - newWidth) / 2;
                var y0 = (bmp.Height - newHeight) / 2;
                var output = new int[newWidth * newHeight];                
                for (int i = 0; i < newHeight; i++)
                    for (int j = 0; j < newWidth; j++)
                    {
                        output[i * newWidth + j] = buffer[(y0 + i) * bmp.Width + x0 + j];
                    }                
                var bitmap = Android.Graphics.Bitmap.CreateBitmap(output, newWidth, newHeight, Android.Graphics.Bitmap.Config.Argb8888);
                return new PlatformImage(bitmap);                
            }
#else
            return null;
#endif

        }

        public static byte[] ToBytesRGB(this IImage image)
        {
#if ANDROID
            using (var bmp = image.AsBitmap())
            {
                var pixels = new int[bmp.Width*bmp.Height];
                bmp.GetPixels(pixels, 0, bmp.Width, 0, 0, bmp.Width, bmp.Height);
                Console.WriteLine($"Got pixels: {bmp.Width}, {bmp.Height}");
                Console.WriteLine("Cls: " + string.Join(", ", pixels.Take(10)));
                var colors = pixels.Select(_ => BitConverter.GetBytes(_)).Select(_ => (byte)((_[2]+ _[1]+ _[0])/3)).ToArray();
                Console.WriteLine($"....{colors.Length}");
                Console.WriteLine("Cls: " + string.Join(", ", colors.Take(5)));
                return colors;
            }
#elif WINDOWS
            return null;
#else
            return null;
#endif
        }
    }
}
