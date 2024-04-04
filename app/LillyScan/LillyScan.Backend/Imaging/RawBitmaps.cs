using System;
using System.Collections.Generic;
using System.Text;

namespace LillyScan.Backend.Imaging
{
    public static class RawBitmaps
    {
        public static unsafe RawBitmap FromRGB(int width, int height, int[] rgb)
        {
            if (rgb.Length != width * height)
            {
                throw new ArgumentException("RawBitmaps.FromRGB: Invalid RGB array length");
            }
            var bmp = new RawBitmap(width, height, 3);

            float* dbuf = bmp.Buffer;

            fixed (int* s = &rgb[0])
            {
                int* si = s;
                for (int i = 0; i < height; i++)
                {
                    for (int j = 0; j < width; j++)
                    {
                        *dbuf++ = (((*si) >> 0) & 0xFF) / 255.0f;
                        *dbuf++ = (((*si) >> 8) & 0xFF) / 255.0f;
                        *dbuf++ = (((*si) >> 16) & 0xFF) / 255.0f;
                    }
                }
            }            

            return bmp;
        }
    }
}
