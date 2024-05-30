using LillyScan.Backend.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace LillyScan.Backend.Imaging
{
    public unsafe class LocalizedMask : IDisposable
    {
        public readonly int X;
        public readonly int Y;
        public readonly int Width;
        public readonly int Height;
        public readonly byte* Data;

        public (float A, float B, float C, float D) LineFit;
        public float MaxLineDistanceError;
        public float FitSegmentLength;
        public float OrientationConfidence;
        public float CenterX;
        public float CenterY;
        public int Area;


        public LocalizedMask((int X, int Y)[] pixels)
        {
            if(pixels.Length==0)
            {
                X = Y = Width = Height = 0;
                Console.WriteLine("[LocalizedMask] Alloc 0 bytes!");
                Data = (byte*)Marshal.AllocHGlobal(0);
                return;
            }
            (var x0, var x1) = pixels.MinAndMax(_=>_.X);
            (var y0, var y1) = pixels.MinAndMax(_=>_.Y);
            (X, Y, Width, Height) = (x0, y0, x1 - x0 + 1, y1 - y0 + 1);
            Data = (byte*)Marshal.AllocHGlobal(Width * Height);
            for (int i = 0; i < Width * Height; i++) Data[i] = 0;
            for(int i=0;i<pixels.Length;i++)
            {
                Data[(pixels[i].Y - Y) * Width + pixels[i].X - X] = 1;
            }

        }        

        public LocalizedMask Rescale(float scaleX, float scaleY)
        {
            var x0 = (int)(X * scaleX);
            var y0 = (int)(Y * scaleY);
            var x1 = (int)((X + Width) * scaleX);
            var y1 = (int)((Y + Height) * scaleY);
            int w = x1 - x0, h = y1 - y0;
            if (w == 0 || h == 0) return new LocalizedMask(x0, y0, w, h, new byte[0]);

            var data = new byte[w * h];
            for (int y = 0; y < h; y++) 
            {
                int yy = (int)((y0 + y) / scaleY - Y);
                for (int x = 0; x < w; x++) 
                {
                    int xx = (int)((x0 + x) / scaleX - X);
                    data[w * y + x] = Data[Width * yy + xx];
                }
            }
            return new LocalizedMask(x0, y0, w, h, data);
        }

        public byte this[int y, int x]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => Data[y * Width + x];
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set => Data[y * Width + x] = value;
        }

        public LocalizedMask(int x, int y, int width, int height, byte* data)
        {
            X = x;
            Y = y;
            Width = width;
            Height = height;
            Data = (byte*)Marshal.AllocHGlobal(width * height);
            for (int i = 0, cnt = width * height; i < cnt; i++)
                Data[i] = data[i];
        }

        public LocalizedMask(int x, int y, int width, int height, byte[] data)
        {
            X = x;
            Y = y;
            Width = width;
            Height = height;
            Data = (byte*)Marshal.AllocHGlobal(width * height);
            fixed (byte* pdata = &data[0])
                for (int i = 0, cnt = width * height; i < cnt; i++)
                    Data[i] = pdata[i];
        }

        public RawBitmap CutFromImage(RawBitmap source)
        {
            if (Width == 0 || Height == 0)
                throw new InvalidDataException("Localized mask has size 0");
            var result = new RawBitmap(Width, Height, source.Channels);

            for(int y=0;y<Height;y++)
            {
                int iy = Y + y;
                if (iy < 0 || iy >= source.Height) continue;
                for(int x=0;x<Width;x++)
                {
                    int ix = X + x;
                    if (ix < 0 || ix >= source.Width) continue;
                    if (Data[y * Width + x] == 0)
                    {
                        for (int c = 0; c < source.Channels; c++) result[y, x, c] = 0;
                    }
                    else
                    {
                        for (int c = 0; c < source.Channels; c++) result[y, x, c] = source[iy, ix, c];
                    }
                }
            }

            return result;
        }

        public void ComputeMetadata()   
        {
            List<int> pointsX = new List<int>();
            List<int> pointsY = new List<int>();
            CenterX = CenterY = 0;
            LineFit = (0, 0, 0, 0);
            int cx = 0, cy = 0;
            for(int y=0;y<Height;y++)
                for(int x=0;x<Width;x++)
                {
                    if (Data[y*Width+x]!=0)
                    {
                        pointsX.Add(X + x);
                        pointsY.Add(Y + y);                        
                        cx += x;
                        cy += y;
                    }
                }

            Area = pointsX.Count;
            if (pointsX.Count == 0)
                return;
            CenterX = X + 1.0f * cx / pointsX.Count;
            CenterY = Y + 1.0f * cy / pointsX.Count;
            
            if(Width>=Height)
            {
                (float b0, float b1, float d) = LinearRegression.LeastSquare2D(pointsX, pointsY);
                //Console.WriteLine($"W>H: {(b0,b1)}");
                // y=b0*x+b1 => b0*x - y + b1 = 0
                double norm = System.Math.Sqrt(b0 * b0 + 1);
                LineFit = ((float)(b0 / norm), (float)(-1 / norm), (float)(b1 / norm), d);
            }
            else // prevent b0=inf for a vertical line
            {
                (float b0, float b1, float d) = LinearRegression.LeastSquare2D(pointsY, pointsX);
                //Console.WriteLine($"W<H: {(b0, b1)}");
                // x = b0*y + b1 => -x + b0*y+b1 = 0
                double norm = System.Math.Sqrt(b0 * b0 + 1);
                LineFit = ((float)(-1 / norm), (float)(b0 / norm), (float)(b1 / norm), d);
            }
            if (LineFit.B < 0 || (LineFit.B == 0 && LineFit.A > 0)) 
            {
                LineFit = (-LineFit.A, -LineFit.B, -LineFit.C, LineFit.D);
            }

            float minPx = X + Width, minPy = Y + Height, minCost = minPx * minPx + minPy * minPy;
            float maxPx = X, maxPy = Y, maxCost=maxPx*maxPx+maxPy*maxPy;

            MaxLineDistanceError = 0;
            for (int i = 0; i < pointsX.Count; i++)
            {
                float err = LineFit.A * pointsX[i] + LineFit.B * pointsY[i] + LineFit.C;
                float px = pointsX[i] - LineFit.A * err;
                float py = pointsY[i] - LineFit.B * err;

                float pcost = px * px + py * py;

                if (pcost < minCost)
                    (minPx, minPy, minCost) = (px, py, pcost);
                if (pcost > maxCost)
                    (maxPx, maxPy, maxCost) = (px, py, pcost);                

                float dst = System.Math.Abs(err);
                if (dst > MaxLineDistanceError)
                    MaxLineDistanceError = dst;
            }

            float dx = maxPx - minPx; 
            float dy = maxPy - minPy;
            FitSegmentLength = (float)System.Math.Sqrt(dx * dx + dy * dy);

            if (FitSegmentLength == 0 && MaxLineDistanceError == 0)
                OrientationConfidence = 0.5f;
            else
                OrientationConfidence = System.Math.Max(FitSegmentLength, MaxLineDistanceError) / (FitSegmentLength + MaxLineDistanceError);

            //Console.WriteLine($"{LineFit} Err={MaxLineDistanceError}, SegLen={FitSegmentLength}, Conf={OrientationConfidence}");

            /*if(MaxLineDistanceError>50)
            {
                string s = $"x=[{pointsX.JoinToString(", ")}]\r\n";
                s += $"y=[{pointsY.JoinToString(", ")}]\r\n";
                File.WriteAllText("points.txt", s);
            }*/
        }

        private int GetData(int y, int x) => Data[y * Width + x];

        public IEnumerable<(int X, int Y)> EnumeratePixels()
        {
            for (int y = 0; y < Height; y++)
                for (int x = 0; x < Width; x++)
                    if (GetData(y, x) != 0)
                        yield return (X + x, Y + y);
        }

        public override string ToString() => $"LocalizedMask({X},{Y},{Width},{Height}, Area={Area})";

        public void Dispose()
        {
            Marshal.FreeHGlobal((IntPtr)Data);
#if DEBUG
            IsDisposed = true;
#endif
        }


#if DEBUG
        private bool IsDisposed = false;
        ~LocalizedMask()
        {
            if (!IsDisposed)
                Debug.WriteLine($"Localized mask leaked: {(X, Y, Width, Height)}");
        }
#endif
    }
}
