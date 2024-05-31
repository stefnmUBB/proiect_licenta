using LillyScan.Backend.AI.Layers;
using LillyScan.Backend.HTR;
using LillyScan.Backend.Imaging;
using LillyScan.Backend.Math;
using LillyScan.Backend.Utils;
using LillyScan.BackendWinforms.Imaging;
using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using static LillyScan.Backend.Imaging.RawBitmapExtensions;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Window;

namespace LillyScan
{ 
    internal static class Program
    {
        static void Measure(Action a)
        {            
            var sw = new Stopwatch();
            sw.Start();
            a();
            sw.Stop();
            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.WriteLine($"Measured time: {sw.Elapsed} | {sw.ElapsedMilliseconds}ms");
            Console.ForegroundColor = ConsoleColor.White;
        }
        
        public static Backend.HTR.IHTREngine HTR = new BuiltInHTREngine();

        static void CompileIAM()
        {
            int q = 0;
            Action<RawBitmap, string> action = (b, c) => b.Save($"cc\\predCC{q++}_{c}.png");
            //(HTR as BuiltInHTREngine).CCAction = action;

            var outDir = @"C:\Users\Stefan\Desktop\perftest\seg_craft\";
            var dir = @"D:\Users\Stefan\Datasets\hw_flex\IAM_full";

            (float R, float G, float B) getColor(int i)
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

            foreach (var imagePath in Directory.EnumerateFiles(dir, "*.png"))
            {
                Console.WriteLine(imagePath);
                var fn = Path.GetFileName(imagePath);
                if (File.Exists(outDir + fn))
                    continue;
                using (var image = RawBitmapIO.FromFile(imagePath))
                {                    
                    var lines = HTR.SegmentLines(image);

                    using (var bmp = new RawBitmap(image.Width, image.Height, 3)) 
                    {
                        bmp.Clear(1);
                        for (int i = 0; i < lines.Length; i++)                         
                        {
                            (var r, var g, var b) = getColor(i);
                            LineDefragmentation.DrawMask(bmp, lines[i], r, g, b);
                        }
                        bmp.Save(outDir + fn);
                    }
                }
                //break;
            }
            Console.ReadLine();
            Environment.Exit(0);
        }

        static void Run()
        {            
            //Img2Col.Run();
            //CompileIAM();
            Metrics.Measure();
            //Metrics.RunIam();
            Console.ReadLine();            
            Environment.Exit(0);
            //CompileIAM();
            //var imagePath = @"D:\Users\Stefan\Datasets\hw_flex\LineSegRaster\IAM_0\322_in.png";
            //var imagePath = @"D:\Users\Stefan\Datasets\hw_flex\LineSegRaster\Compuneri\51_in.png";
            //var imagePath = @"D:\Users\Stefan\Datasets\hw_flex\LineSegRaster\IAM\5_in.png";
            var imagePath = @"D:\Users\Stefan\Datasets\JL\018.jpg";
            //var imagePath = @"D:\Users\Stefan\Datasets\hw_flex\LineSegRaster\nego\002_in.jpg";
            //var imagePath = @"D:\Users\Stefan\Datasets\hw_flex\LineSegRaster\tmp\cap_2_in.jpg";
            //var imagePath = @"D:\Users\Stefan\Datasets\hw_flex\LineSegRaster\tmp\rt2.jpg";
            //var imagePath = @"D:\Users\Stefan\Datasets\hw_flex\IAM_full\1.png";

            var image = RawBitmapIO.FromFile(imagePath);          
            
            var cts = new CancellationTokenSource();

            var pm = new ProgressMonitor(cts.Token);
            pm.ProgressChanged += (o, p, d) => Console.WriteLine($" [ProgressMonitor] [{p:000.00}]: {d}");
            int q = 0;
            Action<RawBitmap, string> action = (b, c) => b.Save($"cc2\\predCC{q++}_{c}.png");
            (HTR as BuiltInHTREngine).CCAction = action;

            /*using (var bmp = RawBitmapIO.FromFile(@"D:\anu3\proiect_licenta\app\LillyScan\LillyScan\bin\Debug\cc\IAM1.png")) 
            {
                Layer.X = 0;
                HTR.PredictTextLine(bmp);
            }
            return;*/

            //var task = Task.Run(() =>
            {
                Measure(() =>
                {                    
                    var lines = HTR.SegmentLines(image, pm);
                    foreach(var mask in lines)
                    {
                        var linebmp = mask.CutFromImage(image);
                        linebmp.CheckNaN();
                        linebmp = linebmp.RotateAndCrop((float)-System.Math.Atan2(-mask.LineFit.A, mask.LineFit.B), disposeOriginal: true);
                        linebmp.CheckNaN();
                        //action(linebmp, "");
                        //Layer.X = 0;                        
                        //HTR.PredictTextLine(linebmp);
                        linebmp.Dispose();
                        break;
                    }                                        
                });
            }
            //);
            //task.Wait();            
        }

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            Thread.CurrentThread.CurrentCulture = System.Globalization.CultureInfo.InvariantCulture;
            Backend.Initializer.Initialize();
            Run();
            Console.ReadLine();
            return;
            //Console.WriteLine("Done"); Console.ReadLine();
            //Application.Run(new MainForm());
        }
    }
}
