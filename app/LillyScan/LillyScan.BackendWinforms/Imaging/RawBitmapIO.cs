using LillyScan.Backend.Imaging;
using LillyScan.BackendWinforms.Utils;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
using static LillyScan.Backend.Utils.Extensions;

namespace LillyScan.BackendWinforms.Imaging
{
    public static class RawBitmapIO
    {
        public static RawBitmap FromBitmap(Bitmap bmp)
        {
            Console.WriteLine($"BM {bmp.Width} {bmp.Height}");
            using (var b = new Bitmap(bmp.Width, bmp.Height, PixelFormat.Format24bppRgb))
            {                
                Console.WriteLine($"BB {b.Width} {b.Height}");
                using (var g = Graphics.FromImage(b))
                    g.DrawImage(bmp, 0, 0, bmp.Width, bmp.Height);                
                var bmpdata = b.LockBits(new Rectangle(0, 0, b.Width, b.Height), ImageLockMode.ReadOnly, b.PixelFormat);
                Console.WriteLine(bmpdata.Stride);
                int numbytes = bmpdata.Stride * bmp.Height;
                byte[] bytedata = new byte[numbytes];                
                IntPtr ptr = bmpdata.Scan0;
                Marshal.Copy(ptr, bytedata, 0, numbytes);
                b.UnlockBits(bmpdata);                               

                var res = new RawBitmap(bmp.Width, bmp.Height, 3);                

                int k = 0;
                for(int y=0;y<bmp.Height;y++)
                {
                    for(int x=0;x<bmp.Width;x++)
                    {                        
                        res[k++] = bytedata[y * bmpdata.Stride + 3 * x + 2] / 255f;
                        res[k++] = bytedata[y * bmpdata.Stride + 3 * x + 1] / 255f;
                        res[k++] = bytedata[y * bmpdata.Stride + 3 * x + 0] / 255f;
                        
                    }
                }                

                return res;
            }
        }

        public static RawBitmap FromFile(string path)
        {
            using (var bmp0 = new Bitmap(path))
            using (var bmp = new Bitmap(bmp0)) 
            {                
                return FromBitmap(bmp);
            }
        }

        public static void Save(this RawBitmap rawBitmap, string path)
        {
            using (var bmp = rawBitmap.ToBitmap())
                bmp.Save(path);
        }

        public static Bitmap ToBitmap(this RawBitmap rbmp)
        {
            var stride = (rbmp.Stride + 3) / 4 * 4;
            byte[] bytes = new byte[stride * rbmp.Height];
            int k = 0;
            for(int y=0;y<rbmp.Height;y++)
            {
                for(int x=0;x<rbmp.Width;x++)
                {
                    for(int c=0;c<rbmp.Channels;c++)
                    {
                        bytes[y * stride + rbmp.Channels * x + rbmp.Channels - 1 - c] = (byte)(rbmp[k++] * 255).Clamp(0, 255);
                    }
                }
            }
            
            var bmp = new Bitmap(rbmp.Width, rbmp.Height, rbmp.Channels == 3 ? PixelFormat.Format24bppRgb : PixelFormat.Format8bppIndexed);
            if (bmp.PixelFormat == PixelFormat.Format8bppIndexed) 
            {
                ColorPalette palette = bmp.Palette;
                for (int i = 0; i < 256; i++)
                    palette.Entries[i] = Color.FromArgb(i, i, i);
                bmp.Palette = palette;
            }

            var bmpdata = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.WriteOnly, bmp.PixelFormat);
            int numbytes = bmpdata.Stride * bmp.Height;            
            IntPtr ptr = bmpdata.Scan0;
            Marshal.Copy(bytes, 0, ptr, numbytes);            
            bmp.UnlockBits(bmpdata);
            return bmp;
        }
    }
}
