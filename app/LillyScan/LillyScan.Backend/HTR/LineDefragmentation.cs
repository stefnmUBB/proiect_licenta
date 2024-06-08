using LillyScan.Backend.AI.Activations;
using LillyScan.Backend.Imaging;
using LillyScan.Backend.Parsers;
using LillyScan.Backend.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace LillyScan.Backend.HTR
{
    public class LineDefragmentation
    {        
        public static unsafe List<LocalizedMask>[] BuildSubsets(LocalizedMask[] masks, int N, Action<RawBitmap, string> callback = null)
        {
            var result = new List<LocalizedMask>[N];
            for (int j = 0; j < N; j++) result[j] = new List<LocalizedMask>();
            int M = masks.Length;
            float eps = (float)System.Math.Sin(System.Math.PI / (2*N));// * 0.8f;
            
            for(int j=0;j<N;j++)
            {
                (var vx, var vy) = ((float)System.Math.Cos(j * System.Math.PI / N), (float)System.Math.Sin(j * System.Math.PI / N));

                for(int i=0;i<M;i++)
                {
                    var cost = masks[i].OrientationConfidence * masks[i].OrientationConfidence
                        * System.Math.Abs(vx * masks[i].LineFit.B - vy * masks[i].LineFit.A);
                    if (cost < eps)
                        result[j].Add(masks[i]);
                }
            }

            var lineGroups = new List<MasksLineSplitter.LinesSplit[]>();

            var tbmp = new RawBitmap(256, 256, 3);

            for(int j=0;j<N;j++)
            {
                if (result[j].Count == 0) continue;
                Console.WriteLine("_____________________________________________________________________");
                foreach (var r in result[j])
                    Console.WriteLine(r.LineFit);
                var baseVector = ((float)System.Math.Cos(j * System.Math.PI / N), (float)System.Math.Sin(j * System.Math.PI / N));

                Debug.WriteLine("Before FindLines?");
                var lines = MasksLineSplitter.FindLines(result[j].ToArray(), baseVector);
                Debug.WriteLine("After FindLines?");
                lineGroups.Add(lines);
                Debug.WriteLine("After add?");
                Console.WriteLine($"Lines = {lines.Length}");
                
                using(var bmp=new RawBitmap(256,256,3))
                {
                    var rnd = new Random();
                    var r0 = (float)rnd.NextDouble();
                    var g0 = (float)rnd.NextDouble();
                    var b0 = (float)rnd.NextDouble();                    
                    foreach (var line in lines)
                    {
                        var r = (float) rnd.NextDouble();
                        var g = (float) rnd.NextDouble();
                        var b = (float) rnd.NextDouble();
                        line.Masks.ForEach(_ => DrawMask(bmp, _, r, g, b));                        
                        bmp.DrawQuad(line.BoundingPoly, 1, 1, 1);
                        tbmp.DrawQuad(line.BoundingPoly, r0, g0, b0);
                    }
                    callback?.Invoke(bmp, $"{lines.Length}");
                }                
            }
            callback?.Invoke(tbmp, "");
            tbmp.Dispose();
            List<List<LocalizedMask>> finalLines = new List<List<LocalizedMask>>();

            Debug.WriteLine("lineGroups.MaxBy?");
            var dominantGroup = lineGroups.MaxBy(_ => _.Sum(l => l.TotalArea));
            Debug.WriteLine("dominantMasks?");
            var dominantMasks = new HashSet<LocalizedMask>(dominantGroup.SelectMany(_ => _.Masks));
            Debug.WriteLine("rlines?");
            var rlines = dominantGroup.Select(_ => _.Masks.ToList()).ToArray();

            foreach(var mask in masks)
            {
                if (dominantMasks.Contains(mask)) continue;
                var index = dominantGroup.ArgMin(_ => Proximity(_, mask));
                Console.WriteLine($"______________________________________________");
                rlines[index].Add(mask);
            }

            Debug.WriteLine("End?");
            return rlines;
        }

        private static float Proximity(MasksLineSplitter.LinesSplit line, LocalizedMask mask)
        {
            (var a, var b, var c) = (line.LineFit.A, line.LineFit.B, line.LineFit.C);
            var d = a * mask.CenterX + b * mask.CenterY + c;            
            d = System.Math.Abs(d);
            Console.WriteLine($"Proximity = {d} / {(a,b,c)} / {(mask.CenterX, mask.CenterY)}");
            return d;
        }

        public static unsafe int[] BuildLinesMask(List<LocalizedMask>[] lines, Action<RawBitmap, string> callback = null)
        {
            var result = new int[256 * 256];

            for(int i=0;i<lines.Length;i++)
            {
                foreach(var mask in lines[i])
                {
                    for(int y=0;y<mask.Height;y++)
                    {
                        for(int x=0;x<mask.Width;x++)
                        {
                            result[(mask.Y + y) * 256 + (mask.X + x)] = mask.Data[y * mask.Width + x] * (i + 1);
                        }
                    }
                }

                for(int p = 0; p < lines[i].Count-1;p++)
                {
                    var q = p + 1;                                        
                    (var x0, var y0) = (lines[i][p].CenterX, lines[i][p].CenterY);
                    (var x1, var y1) = (lines[i][q].CenterX, lines[i][q].CenterY);
                    for (float t = 0; t < 1; t += 1 / 300f)  
                    {
                        var xx = (int)(x0 + (x1 - x0) * t);
                        var yy = (int)(y0 + (y1 - y0) * t);
                        if (xx < 0 || yy < 0 || xx >= 256 || yy >= 256) continue;
                        result[yy * 256 + xx] = i + 1;
                    }                    
                }

            }

            MaskView(result, callback);

            var tmp = new int[256 * 256];
            int count = 0;
            var dx = new int[] { -1, 0, 1, 0 };
            var dy = new int[] { 0, -1, 0, 1 };
            do
            {
                Array.Clear(tmp, 0, tmp.Length);
                count = 0;
                for (int c = 1; c < lines.Length + 1; c++)
                {
                    for (int y = 0; y < 256; y++)
                    {
                        for (int x = 0; x < 256; x++)
                        {                            
                            if (c == result[y * 256 + x])
                            {
                                for (int i = 0; i < 4; i++)
                                {
                                    int ix = x + dx[i], iy = y + dy[i];
                                    if (ix < 0 || iy < 0 || ix >= 256 || iy >= 256) continue;
                                    if (result[256 * iy + ix] == 0 && tmp[256 * iy + ix] == 0) 
                                    {
                                        tmp[256 * iy + ix] = c;
                                        count++;
                                    }
                                }
                            }
                        }
                    }
                }
                for (int i = 0; i < result.Length; i++) result[i] += tmp[i];                
                Console.WriteLine($"Count={count}");
            }
            while (count > 0);

            if (callback != null)
                MaskView(result, callback);

            return result;
        }

        public static void AndMask(int[] mask, RawBitmap segm)
        {
            for (int y = 0; y < 256; y++) 
            {
                for (int x = 0; x < 256; x++)
                {
                    if (segm[y, x] < 0.5f) 
                        mask[y * 256 + x] = 0;
                }
            }
        }     

        public static void MaskView(int[] mask, Action<RawBitmap, string> callback)
        {
            int L = mask.Max();

            var colors = new (float r, float g, float b)[L];
            var r = new Random();
            for (int i = 0; i < L; i++)
                colors[i] = ((float)r.NextDouble(), (float)r.NextDouble(), (float)r.NextDouble());

            using (var bmp = new RawBitmap(256, 256, 3))
            {
                for (int y = 0; y < 256; y++)
                {
                    for (int x = 0; x < 256; x++)
                    {
                        var c = mask[y * 256 + x];
                        if (c > 0)
                        {
                            bmp[y, x, 0] = colors[c - 1].r;
                            bmp[y, x, 1] = colors[c - 1].g;
                            bmp[y, x, 2] = colors[c - 1].b;
                        }
                    }
                }
                callback?.Invoke(bmp, "");
            }
        }

        public static unsafe void DrawMask(RawBitmap bmp, LocalizedMask mask, float r = 1, float g = 1, float b = 1)
        {
            for (int y = 0; y < mask.Height; y++)
            {
                for (int x = 0; x < mask.Width; x++)
                {
                    if (mask.Data[y*mask.Width+x]!=0)
                    {
                        if(bmp.Channels==1)
                        {
                            bmp[mask.Y + y,mask.X + x,0] = (r+g+b)/2;
                        }
                        else
                        {
                            bmp[mask.Y + y, mask.X + x, 0] = r;
                            bmp[mask.Y + y, mask.X + x, 1] = g;
                            bmp[mask.Y + y, mask.X + x, 2] = b;
                        }                      
                    }
                }
            }
        }        
    }
}
