using System;
using System.Runtime.InteropServices;

namespace LillyScan.Backend.Imaging
{
    public static class RawBitmaps
    {
        public static unsafe RawBitmap[] ToTiles(this RawBitmap bmp, int tileWidth, int tileHeight, bool padToFit = false)
        {
            if(!padToFit)
            {
                if (bmp.Width % tileWidth != 0 || bmp.Height % tileHeight != 0)
                    throw new ArgumentException("Cannot perform tiling: invalid dimensions");
            }

            int tilesOnWidth = (bmp.Width + tileWidth - 1) / tileWidth;
            int tilesOnHeight = (bmp.Height + tileHeight - 1) / tileHeight;
            var result = new RawBitmap[tilesOnHeight * tilesOnWidth];

            int k = 0;
            for(int i=0;i<tilesOnHeight;i++)
            {
                for(int j=0;j<tilesOnWidth;j++)
                {
                    result[k++] = bmp.Crop(j * tileWidth, i * tileHeight, tileWidth, tileHeight);
                }
            }

            return result;
        }

        public static unsafe float[] ToBufferedTiles(this RawBitmap bmp, int tileWidth, int tileHeight, bool padToFit = false)
        {
            if (!padToFit)
            {
                if (bmp.Width % tileWidth != 0 || bmp.Height % tileHeight != 0)
                    throw new ArgumentException("Cannot perform tiling: invalid dimensions");
            }

            int tilesOnWidth = (bmp.Width + tileWidth - 1) / tileWidth;
            int tilesOnHeight = (bmp.Height + tileHeight - 1) / tileHeight;
            int tileLength = tileWidth * tileHeight * bmp.Channels;
            var result = new float[tilesOnHeight * tilesOnWidth * tileLength];

            int resPos = 0;                        
            for (int i = 0; i < tilesOnHeight; i++)
            {
                for (int j = 0; j < tilesOnWidth; j++)
                {
                    using (var crop = bmp.Crop(j * tileWidth, i * tileHeight, tileWidth, tileHeight))
                        Marshal.Copy((IntPtr)crop.Buffer, result, resPos, tileLength);
                    resPos += tileLength;
                }
            }            

            return result;
        }


        public static unsafe RawBitmap FromTiles(RawBitmap[] tiles, int tilesOnWidth, int tilesOnHeight)
        {
            if (tiles.Length != tilesOnWidth * tilesOnHeight)
                throw new ArgumentException("Invalid tiles count");
            (var w, var h, var c) = (tiles[0].Width, tiles[0].Height, tiles[0].Channels);
            for(int i=1;i<tiles.Length;i++)
            {
                if (tiles[i].Width != w || tiles[i].Height != h || tiles[i].Channels != c)
                    throw new ArgumentException($"All tiles must have the same size and number of channels");
            }

            var result = new RawBitmap(w * tilesOnWidth, h * tilesOnHeight, c);

            int k = 0;
            for(int i=0;i<tilesOnHeight;i++)
            {
                for (int j = 0; j < tilesOnWidth; j++)
                {
                    result = result.DrawImage(tiles[k++], j * w, i * h, inPlace: true);
                }
            }
            return result;
        }

        public static unsafe int[] ToRGB(this RawBitmap bmp)
        {
            var result = new int[bmp.Width * bmp.Height];
            float* src = bmp.Buffer;
            fixed (int* dst = &result[0])
            {
                int* idst = dst;
                if (bmp.Channels == 1)
                {                    
                    for (int i = 0; i < bmp.Height; i++)
                    {
                        for (int j = 0; j < bmp.Width; j++)
                        {
                            byte color = (byte)(src[i * bmp.Stride + j] * 255);
                            *idst++ = color | (color << 8) | (color << 16) | (0xFF << 24);
                        }
                    }
                }
                else if (bmp.Channels == 3)
                {
                    for (int i = 0; i < bmp.Height; i++)
                    {
                        for (int j = 0; j < bmp.Width; j++)
                        {
                            byte r = (byte)(src[i * bmp.Stride + 3 * j + 0] * 255);
                            byte g = (byte)(src[i * bmp.Stride + 3 * j + 1] * 255);
                            byte b = (byte)(src[i * bmp.Stride + 3 * j + 2] * 255);
                            *idst++ = r | (g << 8) | (b << 16) | (0xFF << 24);
                        }
                    }
                }
                else if (bmp.Channels == 4)
                {
                    for (int i = 0; i < bmp.Height; i++)
                    {
                        for (int j = 0; j < bmp.Width; j++)
                        {
                            byte r = (byte)(src[i * bmp.Stride + 4 * j + 0] * 255);
                            byte g = (byte)(src[i * bmp.Stride + 4 * j + 1] * 255);
                            byte b = (byte)(src[i * bmp.Stride + 4 * j + 2] * 255);
                            byte a = (byte)(src[i * bmp.Stride + 4 * j + 3] * 255);
                            *idst++ = r | (g << 8) | (b << 16) | (a << 24);
                        }
                    }
                }
                else throw new NotImplementedException();
            }

            return result;
        }

        public static unsafe RawBitmap FromRGB(int width, int height, int[] rgb, bool includeAlpha = false)
        {
            if (rgb.Length != width * height)
            {
                throw new ArgumentException("RawBitmaps.FromRGB: Invalid RGB array length");
            }
            var bmp = new RawBitmap(width, height, includeAlpha ? 4 : 3);

            float* dbuf = bmp.Buffer;

            fixed (int* s = &rgb[0])
            {
                int* si = s;
                if (!includeAlpha)
                {
                    for (int i = 0; i < height; i++)
                    {
                        for (int j = 0; j < width; j++)
                        {
                            *dbuf++ = (((*si) >> 0) & 0xFF) / 255.0f;
                            *dbuf++ = (((*si) >> 8) & 0xFF) / 255.0f;
                            *dbuf++ = (((*si) >> 16) & 0xFF) / 255.0f;
                            si++;
                        }
                    }
                }
                else
                {
                    for (int i = 0; i < height; i++)
                    {
                        for (int j = 0; j < width; j++)
                        {
                            *dbuf++ = (((*si) >> 0) & 0xFF) / 255.0f;
                            *dbuf++ = (((*si) >> 8) & 0xFF) / 255.0f;
                            *dbuf++ = (((*si) >> 16) & 0xFF) / 255.0f;
                            *dbuf++ = (((*si) >> 24) & 0xFF) / 255.0f;
                            si++;
                        }
                    }
                }
            }            

            return bmp;
        }

        public static unsafe RawBitmap GrayscaleToAlpha(this RawBitmap bmp, float r, float g, float b, float a = 1, bool disposeOriginal=false)
        {
            if (bmp.Channels != 1)
                throw new ArgumentException("Grayscale bitmap must be single channel");

            int width = bmp.Width, height = bmp.Height;
            var result = new RawBitmap(width, height, 4);
            float* dbuf = result.Buffer;
            float* sbuf = bmp.Buffer;

            for (int i = 0; i < height; i++)
            {
                for (int j = 0; j < width; j++)
                {
                    *dbuf++ = r;
                    *dbuf++ = g;
                    *dbuf++ = b;
                    *dbuf++ = a * (*sbuf++);
                }
            }
            if (disposeOriginal)
                bmp.Dispose();
            return result;
        }


        public static unsafe RawBitmap[] FromStackedBuffer(float[] buffer, int width, int height, int channels)
        {
            int imageLen = width * height * channels;
            int imagesCount = System.Math.DivRem(buffer.Length, imageLen, out var rem);
            if (rem != 0)
                throw new InvalidOperationException($"Could not load {width}x{height}x{3} images from a {buffer.Length} length buffer");
            var images = new RawBitmap[imagesCount];
            fixed(float* ptr = buffer)
            {
                float* iptr = ptr;
                for (int i = 0; i < imagesCount; i++)
                {
                    images[i] = new RawBitmap(width, height, channels, iptr);
                    iptr += imageLen;
                }
            }
            return images;
        }
    }
}
