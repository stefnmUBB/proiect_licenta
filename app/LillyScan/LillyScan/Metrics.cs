using LillyScan.Backend.HTR;
using LillyScan.Backend.Imaging;
using LillyScan.BackendWinforms.Imaging;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LillyScan
{
    internal class Metrics
    {
        static (float R, float G, float B) getColor(int i)
        {
            float t = (float)(i * 2 * System.Math.PI / 32);
            var ct = (float)System.Math.Cos(t);
            var st = (float)System.Math.Sin(t);
            var sqrt = new Func<float, float>(x => (float)System.Math.Sqrt(x));
            var r = 0.5f + (0 * ct / sqrt(2) - 2 * st / sqrt(6)) / 2;
            var g = 0.5f + (1 * ct / sqrt(2) + 1 * st / sqrt(6)) / 2;
            var b = 0.5f + (-1 * ct / sqrt(2) + 1 * st / sqrt(6)) / 2;
            return (r, g, b);
        }

        public static RawBitmap SegmentIam(RawBitmap bmp)
        {
            var C = new float[bmp.Height];
            for(int y=0;y<bmp.Height;y++)
            {
                for (int x = 0; x < bmp.Width; x++)
                    if (bmp[y, x] <= 0.5) C[y]++;
            }
            MasksLineSplitter.Conv(C, 3, 4, 3);
            MasksLineSplitter.Conv(C, 2, 6, 2);
            MasksLineSplitter.Conv(C, 1, 8, 1);
            var linesY = new List<int>();            
            int tr = 50;
            for(int y=0;y<bmp.Height;y++)
            {
                if (C[y] < tr)
                    linesY.Add(y);
            }
            var lY = new List<int>();

            int first = linesY[0], last = linesY[0];

            for (int i = 1; i < linesY.Count; i++) 
            {
                if (linesY[i] == linesY[i - 1] + 1)
                {
                    last = linesY[i];
                }
                else
                {
                    lY.Add((first + last) / 2);
                    first = last = linesY[i];                    
                }
            }
            lY.Add((first + last) / 2);

            int lindex(int y)
            {
                for (int i = 0; i < lY.Count; i++)
                    if (y < lY[i]) return i;
                return lY.Count;
            }

            var result = new RawBitmap(bmp.Width, bmp.Height, 3);

            var dx = new int[] { -1, 0, 1, 0 };
            var dy = new int[] { 0, -1, 0, 1 };

            IEnumerable<(int Y, int X)> ForCircle(int x0, int y0, int r, int bx, int by, int bw, int bh)
            {
                for (int y = -r; y <= r; y++)
                {
                    if (y0 + y < by || y0 + y >= bh) continue;
                    for (int x = -r; x <= r; x++)
                    {
                        if (x0 + x < bx || x0 + x >= bw) continue;
                        if (x * x + y * y > r * r) continue;
                        yield return (y0 + y, x0 + x);
                    }
                }
            }

            result.Clear(1);
            for (int y = 0; y < bmp.Height; y++)
            {
                int lix = lindex(y);
                var color = getColor(lix);
                for (int x = 0; x < bmp.Width; x++)
                {
                    if (bmp[y, x] < 0.5)
                    {
                        foreach ((int iy, int ix) in ForCircle(x, y, 5, 0, 0, bmp.Width, bmp.Height))
                        {
                            result[iy, ix, 0] = color.R;
                            result[iy, ix, 1] = color.G;
                            result[iy, ix, 2] = color.B;
                        }
                    }
                }
            }

            return result;
        }

        public static void RunIam()
        {
            var dir = @"D:\Users\Stefan\Datasets\hw_flex\IAM_full";
            var outDir = @"C:\Users\Stefan\Desktop\perftest\iam\";

            foreach (var imagePath in Directory.EnumerateFiles(dir, "*.png"))
            {
                Console.WriteLine(imagePath);
                var fn = Path.GetFileName(imagePath);
                if (File.Exists(outDir + fn))
                    continue;
                using (var image = RawBitmapIO.FromFile(imagePath).AverageChannels(disposeOriginal: true)) 
                using (var bmp = SegmentIam(image))
                    bmp.Save(outDir + fn);                          
                //break;
            }
        }

        public static void RunIam0()
        {
            var dir = @"D:\Users\Stefan\Datasets\hw_flex\IAM_full";
            var outDir = @"C:\Users\Stefan\Desktop\perftest\iam\";

            foreach (var imagePath in Directory.EnumerateFiles(dir, "*.png"))
            {
                Console.WriteLine(imagePath);
                var fn = Path.GetFileName(imagePath);
                if (File.Exists(outDir + fn))
                    continue;
                using (var image = RawBitmapIO.FromFile(imagePath).AverageChannels(disposeOriginal: true))
                using (var bmp = SegmentIam(image))
                    bmp.Save(outDir + fn);
                //break;
            }
        }

        public static IEnumerable<(RawBitmap Real, RawBitmap Pred)> GetPairs()
        {
            string rPath = @"C:\Users\Stefan\Desktop\perftest\iam\";
            string pPath = @"C:\Users\Stefan\Desktop\perftest\seg_craft\";

            foreach(var rf in Directory.EnumerateFiles(rPath))
            {
                var fname = Path.GetFileName(rf);
                var pf = pPath + fname;
                if (!File.Exists(pf) || !File.Exists(rf)) continue;

                Console.WriteLine($"{rf} || {pf}");
                var real = RawBitmapIO.FromFile(rf);
                var pred = RawBitmapIO.FromFile(pf);

                yield return (real, pred);
            }
        }

        private static double MaskAccuracy(RawBitmap real, RawBitmap pred)
        {
            int cnt = 0;
            if (real.Width != pred.Width || real.Height != pred.Height) 
            {
                Console.WriteLine($"Dims not equal! {(real.Width, real.Height)}, {(pred.Width, pred.Height)}");
            }
            var w = Math.Min(real.Width, pred.Width);
            var h = Math.Min(real.Height, pred.Height);



            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++) 
                {
                    var c1 = real[y, x, 0] > 0.9 && real[y, x, 1] > 0.9 && real[y, x, 2] > 0.9;
                    var c2 = pred[y, x, 0] > 0.9 && pred[y, x, 1] > 0.9 && pred[y, x, 2] > 0.9;

                    if (c1 == c2) cnt++;
                }
            }

            return 1.0 * cnt / (w * h);
        }


        private static (double R, double P) MaskPR(RawBitmap real, RawBitmap pred)
        {
            int tp = 0, tn = 0, fp = 0, fn = 0;

            if (real.Width != pred.Width || real.Height != pred.Height)
            {
                Console.WriteLine($"Dims not equal! {(real.Width, real.Height)}, {(pred.Width, pred.Height)}");
            }
            var w = Math.Min(real.Width, pred.Width);
            var h = Math.Min(real.Height, pred.Height);



            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    var c1 = real[y, x, 0] > 0.9 && real[y, x, 1] > 0.9 && real[y, x, 2] > 0.9;
                    var c2 = pred[y, x, 0] > 0.9 && pred[y, x, 1] > 0.9 && pred[y, x, 2] > 0.9;
                    if (c1 && c2) tp++;
                    if (!c1 && !c2) fn++;
                    if (c1 && !c2) tn++;
                    if (!c1 && c2) fp++;
                }
            }

            return (1.0 * tp / (tp + fn), 1.0 * tp / (tp + tn));

        }

        private static (double D, double J) IoU(RawBitmap real, RawBitmap pred)
        {
            int tp = 0, tn = 0, fp = 0, fn = 0;

            if (real.Width != pred.Width || real.Height != pred.Height)
            {
                Console.WriteLine($"Dims not equal! {(real.Width, real.Height)}, {(pred.Width, pred.Height)}");
            }
            var w = Math.Min(real.Width, pred.Width);
            var h = Math.Min(real.Height, pred.Height);

            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    var c1 = real[y, x, 0] > 0.9 && real[y, x, 1] > 0.9 && real[y, x, 2] > 0.9;
                    var c2 = pred[y, x, 0] > 0.9 && pred[y, x, 1] > 0.9 && pred[y, x, 2] > 0.9;
                    if (c1 && c2) tp++;
                    if (!c1 && !c2) fn++;
                    if (c1 && !c2) tn++;
                    if (!c1 && c2) fp++;
                }
            }

            return (2.0 * tp / (2 * tp + fp + fn), 1.0 * tp / (tp + fp + fn));

        }

        private static (double R, double P) LinePR(RawBitmap real, RawBitmap pred)
        {            
            var G = FindMasks(real);            
            var E = FindMasks(pred);

            double R = 0, P = 0;
            int dp = 0, dr = 0;
            foreach(var g in G)
            {
                double n = 0, s = 0;
                int interCount = 0;

                foreach(var e in E)
                {
                    var intersect = MaskIntersect(g, e);
                    n += intersect;
                    if (intersect > 0)
                    {
                        s += e.Area;
                        interCount++;
                    }
                }
                double d = g.Area - 1;

                //Console.WriteLine($"{n}, {interCount}, {n - interCount}, {d}, {s}");

                R += d == 0 ? 0 : (n - interCount) / d;
                dr += d == 0 ? 0 : 1;
                P += s - 1 == 0 ? 0 : (n - 1) / (s - 1);
                dp += s - 1 == 0 ? 0 : 1;
            }
            foreach (var g in G) g.Dispose();
            foreach (var e in E) e.Dispose();

            return (dr == 0 ? 1 : R / dr, dp == 0 ? 1 : P / dp);
        }

        private static double LineAccuracy(RawBitmap real, RawBitmap pred)
        {
            var G = FindMasks(real);
            var E = FindMasks(pred);

            double A = 0;            
            foreach (var g in G)
            {
                double s = 0;
                double t = 0;                

                foreach (var e in E)
                {                    
                    var intersect = MaskIntersect(g, e);                    
                    if (intersect > 0)
                    {
                        s += intersect + MaskCpl(g, e);
                        t += MaskTotal(g, e);                        
                    }
                }
                A += t == 0 ? 0 : s / t;                
            }
            foreach (var g in G) g.Dispose();
            foreach (var e in E) e.Dispose();

            return G.Length == 0 ? 1 : A / G.Length;
        }


        public static void Measure()
        {
            var ma = new Metric("MaskAccuracy");
            var mp = new Metric("MaskPrecision");
            var mr = new Metric("MaskRecall");
            var md = new Metric("MaskDice");
            var mj = new Metric("MaskIoU");
            var la = new Metric("LineAccuracy");
            var lp = new Metric("LinePrecision");
            var lr = new Metric("LineRecall");            
            foreach (var (real, pred) in GetPairs())
            {
                ma.Add(MaskAccuracy(real, pred));
                (var d, var j) = IoU(real, pred);
                md.Add(d);
                mj.Add(j);
                (var r, var p) = MaskPR(real, pred);
                mp.Add(p);
                mr.Add(r);
                Console.WriteLine($"Mask F-m = {2 * mp.Get() * mr.Get() / (mp.Get() + mr.Get())}");
                (r, p) = LinePR(real, pred);
                la.Add(LineAccuracy(real, pred));
                lp.Add(p);
                lr.Add(r);
                Console.WriteLine($"Line F-m = {2 * lp.Get() * lr.Get() / (lp.Get() + lr.Get())}");
                real.Dispose();
                pred.Dispose();
            }
        }


        class Metric
        {
            string Name;
            double Value = 0;
            int Count = 0;

            public Metric(string name) { Name = name; }

            public void Add(double v)
            {
                Value += v;
                Count++;
                Console.WriteLine($"{Name}: {v} / {Get()}");
            }

            public double Get() => Count == 0 ? 0 : Value / Count;            
        }        

        private static unsafe int MaskIntersect(LocalizedMask m1, LocalizedMask m2)
        {
            int r = 0;
            //Console.WriteLine($"MaskIntersect {m1.Area} {m2.Area}");
            for (int y = 0; y < m1.Height; y++) 
            {                
                for (int x = 0; x < m1.Width; x++) 
                {
                    if (m1.Data[y * m1.Width + x] == 0) continue;
                    int y2 = m1.Y + y - m2.Y;
                    int x2 = m1.X + x - m2.X;
                    if (y2 < 0 || x2 < 0 || y2 >= m2.Height || x2 >= m2.Width) continue;
                    if (m2.Data[y2 * m2.Width + x2] != 0)
                        r++;
                }
            }
            return r;
        }

        private static unsafe int MaskUnion(LocalizedMask m1, LocalizedMask m2)
        {
            int r = 0;
            //Console.WriteLine($"MaskIntersect {m1.Area} {m2.Area}");
            for (int y = 0; y < m1.Height; y++)
            {
                for (int x = 0; x < m1.Width; x++)
                {
                    if (m1.Data[y * m1.Width + x] != 0)
                    {
                        r++;
                        continue;
                    }
                    int y2 = m1.Y + y - m2.Y;
                    int x2 = m1.X + x - m2.X;
                    if (y2 < 0 || x2 < 0 || y2 >= m2.Height || x2 >= m2.Width) continue;

                    if (m2.Data[y2 * m2.Width + x2] != 0)
                        r++;
                }
            }
            return r;
        }

        private static unsafe int MaskCpl(LocalizedMask m1, LocalizedMask m2)
        {
            int r = 0;
            var d = new HashSet<(int, int)>();
            for (int y = 0; y < m1.Height; y++)
                for (int x = 0; x < m1.Width; x++)
                    d.Add((m1.X + x, m1.Y + y));
            for (int y = 0; y < m2.Height; y++)
                for (int x = 0; x < m2.Width; x++)
                    d.Add((m2.X + x, m2.Y + y));

            foreach(var (x,y) in d)
            {
                int ix = x - m1.X, iy = y - m1.Y;
                if (0 <= ix && ix < m1.Width && 0 <= iy && iy < m1.Height && m1.Data[iy * m1.Width + ix] != 0) continue;
                ix = x - m2.X; iy = y - m2.Y;
                if (0 <= ix && ix < m2.Width && 0 <= iy && iy < m2.Height && m2.Data[iy * m2.Width + ix] != 0) continue;
                r++;
            }
            return r;
        }

        private static unsafe int MaskTotal(LocalizedMask m1, LocalizedMask m2)
        {
            var d = new HashSet<(int, int)>();
            for (int y = 0; y < m1.Height; y++)            
                for (int x = 0; x < m1.Width; x++)                
                    d.Add((m1.X + x, m1.Y + y));
            for (int y = 0; y < m2.Height; y++)
                for (int x = 0; x < m2.Width; x++)
                    d.Add((m2.X + x, m2.Y + y));
            return d.Count;
        }

        private static LocalizedMask[] FindMasks(RawBitmap bmp)
        {
            Dictionary<(double, double, double), List<(int, int)>> m = new Dictionary<(double, double, double), List<(int, int)>>();
            
            for(int y=0;y<bmp.Height;y++)
            {
                for (int x = 0; x < bmp.Width; x++)
                {
                    var rgb = (bmp[y, x, 0], bmp[y, x, 1], bmp[y, x, 2]);
                    if (rgb == (1, 1, 1))
                        continue;
                    if (!m.ContainsKey(rgb))
                        m[rgb] = new List<(int, int)>();
                    m[rgb].Add((x, y));
                }
            }            
            var result = m.Values.Select(_ => new LocalizedMask(_.ToArray())).ToArray();
            foreach (var r in result) r.ComputeMetadata();
            return result;
        }
        

    }
}
