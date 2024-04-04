using System;
using System.Collections.Generic;
using System.Text;

namespace LillyScan.Backend.Imaging
{
    public static class RawBitmapExtensions
    {         
        public static unsafe RawBitmap Crop(this RawBitmap bmp, int x, int y, int width, int height, bool disposeOriginal=false)
        {
            if (x < 0 || y < 0 || x + width > bmp.Width || y + height > bmp.Height)
                throw new ArgumentException("Invalid crop area");
            int C = bmp.Channels;

            var res = new RawBitmap(width, height, C);

            float* sbuf = bmp.Buffer;
            float* dbuf = res.Buffer;

            for(int iy=0;iy<height;iy++)
            {
                int sy = y + iy;
                for(int ix=0;ix<width;ix++)
                {
                    int sx = x + ix;
                    for (int c = 0; c < C; c++)
                        dbuf[iy * res.Stride + ix * res.Channels + c] = sbuf[sy * bmp.Stride + sx * bmp.Channels + c];
                }
            }

            if (disposeOriginal) bmp.Dispose();
            return res;
        }

        public static RawBitmap CropCenteredPercent(this RawBitmap bmp, int pwidth, int pheight)
        {
            var rw = bmp.Width * pwidth / 100;
            var rh = bmp.Height * pheight / 100;
            return Crop(bmp, (bmp.Width - rw) / 2, (bmp.Height - rh) / 2, rw, rh);
        }

        public static unsafe RawBitmap Resize(this RawBitmap bmp, int width, int height, bool disposeOriginal = false)
        {
            int C = bmp.Channels;
            var res = new RawBitmap(width, height, C);
            float* sbuf = bmp.Buffer;
            float* dbuf = res.Buffer;
            
            float xRatio = width > 1 ? (1.0f * (bmp.Width - 1) / (width - 1)) : 0.0f;
            float yRatio = height > 1 ? (1.0f * (bmp.Height - 1) / (height - 1)) : 0.0f;

            for(int i=0;i<height;i++)
            {
                for(int j=0;j<width;j++)
                {
                    int xl = (int)System.Math.Floor(xRatio * j);
                    int yl = (int)System.Math.Floor(yRatio * i);
                    int xh = System.Math.Min((int)System.Math.Ceiling(xRatio * j), bmp.Width - 1);
                    int yh = System.Math.Min((int)System.Math.Ceiling(yRatio * i), bmp.Height - 1);

                    var xw = xRatio * j - xl;
                    var yw = yRatio * i - yl;

                    for (int ch = 0; ch < C; ch++) 
                    {
                        var a = sbuf[yl * bmp.Stride + xl * C + ch];
                        var b = sbuf[yl * bmp.Stride + xh * C + ch];
                        var c = sbuf[yh * bmp.Stride + xl * C + ch];
                        var d = sbuf[yh * bmp.Stride + xh * C + ch];
                        var r = a * (1 - xw) * (1 - yw) + b * xw * (1 - yw) + c * yw * (1 - xw) + d * xw * yw;
                        dbuf[i * res.Stride + j * C + ch] = r;
                    }
                }
            }

            if (disposeOriginal)
                bmp.Dispose();
            return res;
        }

        public static unsafe RawBitmap AverageChannels(this RawBitmap bmp, bool disposeOriginal = false)
        {
            int C = bmp.Channels;
            var res = new RawBitmap(bmp.Width, bmp.Height, 1);
            float* sbuf = bmp.Buffer;
            float* dbuf = res.Buffer;

            for (int y = 0; y < bmp.Height; y++) 
            {
                for (int x = 0; x < bmp.Width; x++)
                {
                    float sum = 0;
                    for (int c = 0; c < C; c++)
                        sum += *sbuf++;
                    *dbuf++ = sum / C;
                }
            }
            return res;
        }
    }
}
