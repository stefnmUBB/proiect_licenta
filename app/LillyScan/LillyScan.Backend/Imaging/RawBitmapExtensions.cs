using LillyScan.Backend.Utils;
using System;
using System.Collections.Generic;
using System.IO;

namespace LillyScan.Backend.Imaging
{
    public static class RawBitmapExtensions
    {
        public static unsafe void CheckNaN(this RawBitmap bitmap)
        {
            for (int i = 0; i < bitmap.Stride * bitmap.Height; i++)
                if (float.IsNaN(bitmap.Buffer[i]))
                    throw new InvalidDataException("Found NaN");
        }

        public static unsafe void Clear(this RawBitmap bitmap)
        {
            for (int i = 0, len = bitmap.Stride * bitmap.Height; i < len; i++)
                bitmap.Buffer[i] = 0;
        }

        public static unsafe void Clear(this RawBitmap bitmap, float value)
        {
            for (int i = 0, len = bitmap.Stride * bitmap.Height; i < len; i++)
                bitmap.Buffer[i] = value;
        }

        public static RawBitmap DrawImage(this RawBitmap target, RawBitmap src, int x, int y, bool inPlace = false)
        {
            var dest = inPlace ? target : new RawBitmap(target);            

            for(int iy=0;iy<src.Height;iy++)
            {
                int dy = y + iy;
                if (dy < 0 || dy >= dest.Height) continue;
                for(int ix=0;ix<src.Width;ix++)
                {
                    int dx = x + ix;
                    if (dx < 0 || dx >= dest.Width) continue;
                    for (int c = 0; c < dest.Channels; c++)
                        dest[dy, dx, c] = src[iy, ix, c];
                }
            }
            return dest;
        }

        public static unsafe RawBitmap Threshold(this RawBitmap bmp, float threshold = 0.5f, float lowerValue = 0, float higherValue = 1, bool inPlace = false, bool disposeOriginal = false)
        {
            if (inPlace && disposeOriginal)
                throw new InvalidOperationException("inPlace and disposeOriginal cannot be true simultaneously");
            var dest = inPlace ? bmp : new RawBitmap(bmp);

            for (int i = 0; i < bmp.Stride * bmp.Height; i++)
                dest.Buffer[i] = dest.Buffer[i] < threshold ? lowerValue : higherValue;

            if (disposeOriginal)
                bmp.Dispose();
            return dest;
        }

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

        public static unsafe RawBitmap CropUnrestricted(this RawBitmap bmp, int x, int y, int width, int height, bool disposeOriginal = false)
        {            
            int C = bmp.Channels;
            var res = new RawBitmap(width, height, C);

            float* sbuf = bmp.Buffer;
            float* dbuf = res.Buffer;

            for (int iy = 0; iy < height; iy++)
            {
                int sy = y + iy;
                if (sy < 0 || sy >= bmp.Height)
                {
                    for (int ix = 0, cnt = width * C; ix < cnt; ix++)
                        dbuf[iy * cnt + ix] = 0;
                }
                else
                {
                    for (int ix = 0; ix < width; ix++)
                    {
                        int sx = x + ix;
                        if (sx < 0 || sx >= bmp.Width) 
                        {
                            for (int c = 0; c < C; c++)
                                dbuf[iy * res.Stride + ix * res.Channels + c] = 0;
                        }
                        else
                        {
                            for (int c = 0; c < C; c++)
                                dbuf[iy * res.Stride + ix * res.Channels + c] = sbuf[sy * bmp.Stride + sx * bmp.Channels + c];
                        }
                    }
                }
            }

            if (disposeOriginal) bmp.Dispose();
            return res;
        }

        public static RawBitmap CropCenteredPercent(this RawBitmap bmp, int pwidth, int pheight, bool disposeOriginal=false)
        {
            var rw = bmp.Width * pwidth / 100;
            var rh = bmp.Height * pheight / 100;
            return Crop(bmp, (bmp.Width - rw) / 2, (bmp.Height - rh) / 2, rw, rh, disposeOriginal);
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

            if (disposeOriginal)
                bmp.Dispose();

            return res;
        }

        public static unsafe void InplaceCopyFrom(this RawBitmap bmp, float* source)
        {
            for (int i = 0, count = bmp.Stride * bmp.Height; i < count; i++)
                bmp.Buffer[i] = source[i];            
        }
        public static unsafe void InplaceCopyFrom(this RawBitmap bmp, float[] source, int startIndex = 0)
        {
            int count = bmp.Stride * bmp.Height;
            if (startIndex + count > source.Length) 
                throw new IndexOutOfRangeException($"Could not copy {count} elements from {source.Length}-sized array starting at {startIndex}");
            fixed (float* src = &source[startIndex])
                InplaceCopyFrom(bmp, src);
        }

        public static RawBitmap Filter3x3(this RawBitmap bmp, float[] kernel, bool disposeOriginal = false)
        {
            var result = new RawBitmap(bmp.Width, bmp.Height, bmp.Channels);

            for (int c = 0; c < bmp.Channels; c++)
            {
                for (int i = 1; i < bmp.Height - 1; i++)
                {
                    for (int j = 1; j < bmp.Width - 1; j++)
                    {
                        float s = 0;
                        for(int ky=0;ky<3;ky++)
                        {
                            for(int kx=0;kx<3;kx++)
                            {
                                s += kernel[ky * 3 + kx] * bmp[i + ky - 1, j + kx - 1, c];
                            }
                        }
                        result[i, j, c] = s;
                    }
                }
            }
            if(disposeOriginal)
                bmp.Dispose();
            return result;
        }

        public static unsafe RawBitmap RotateAndCrop(this RawBitmap bitmap, float angle, bool disposeOriginal=false)
        {
            var cosa = (float)System.Math.Cos(angle);
            var sina = (float)System.Math.Sin(angle);
            (var w, var h) = (bitmap.Width, bitmap.Height);
            var rdx = new float[] { cosa, -sina, w / 2 - w * cosa / 2 + h * sina / 2 };
            var rdy = new float[] { sina, cosa, h / 2 - w * sina / 2 - h * cosa / 2 };
            var rix = new float[] { cosa, sina, w / 2 - w * cosa / 2 - h * sina / 2 };
            var riy = new float[] { -sina, cosa, h / 2 + w * sina / 2 - h * cosa / 2 };

            (var bx0, var by0) = (rdx[0] * 0 + rdx[1] * 0 + rdx[2], rdy[0] * 0 + rdy[1] * 0 + rdy[2]);  
            (var bx1, var by1) = (rdx[0] * w + rdx[1] * 0 + rdx[2], rdy[0] * w + rdy[1] * 0 + rdy[2]);  
            (var bx2, var by2) = (rdx[0] * w + rdx[1] * h + rdx[2], rdy[0] * w + rdy[1] * h + rdy[2]);  
            (var bx3, var by3) = (rdx[0] * 0 + rdx[1] * h + rdx[2], rdy[0] * 0 + rdy[1] * h + rdy[2]);

            (var x0, var x1) = new[] { bx0, bx1, bx2, bx3 }.MinAndMax();
            (var y0, var y1) = new[] { by0, by1, by2, by3 }.MinAndMax();

            Console.WriteLine($"{(bx0, by0)}, {(bx1, by1)}, {(bx2, by2)}, {(bx3, by3)}");
            Console.WriteLine($"{(x0, y0)}, {(x1, y1)}");

            int rw = (int)System.Math.Ceiling(x1 - x0);
            int rh = (int)System.Math.Ceiling(y1 - y0);
            Console.WriteLine($"{(rw, rh)}");

            var result = new RawBitmap(rw, rh, bitmap.Channels);

            float A, B, C, D;

            for (int y = 0; y < rh; y++) 
            {
                for (int x = 0; x < rw; x++) 
                {
                    float ry = y0 + y, rx = x0 + x;
                    (rx, ry) = (rix[0] * rx + rix[1] * ry + rix[2], riy[0] * rx + riy[1] * ry + riy[2]);
                    int xx0 = (int)(rx), yy0 = (int)(ry);
                    int xx1 = (int)(rx + 1), yy1 = (int)(ry + 1);
                    float fx = rx - xx0, fy = ry - yy0;
                    for(int c=0;c<bitmap.Channels;c++)
                    {
                        A = (0 <= xx0 && xx0 < w && 0 <= yy0 && yy0 < h) ? bitmap[yy0, xx0, c] : 0;
                        B = (0 <= xx1 && xx1 < w && 0 <= yy0 && yy0 < h) ? bitmap[yy0, xx1, c] : 0;
                        C = (0 <= xx1 && xx1 < w && 0 <= yy1 && yy1 < h) ? bitmap[yy1, xx1, c] : 0;
                        D = (0 <= xx0 && xx0 < w && 0 <= yy1 && yy1 < h) ? bitmap[yy1, xx0, c] : 0;
                        var R = (1 - fy) * (1 - fx) * A + (1 - fy) * fx * B + fy * (1 - fx) * D + fy * fx * C;
                        result[y, x, c] = R;
                    }
                    
                }
            }

            if (disposeOriginal) bitmap.Dispose();

            return result;
        }        
    }
}
