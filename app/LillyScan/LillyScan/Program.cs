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

        static void Run()
        {            
            var imagePath = @"D:\Users\Stefan\Datasets\hw_flex\LineSegRaster\IAM_0\322_in.png";
            //var imagePath = @"D:\Users\Stefan\Datasets\hw_flex\LineSegRaster\Compuneri\51_in.png";
            //var imagePath = @"D:\Users\Stefan\Datasets\hw_flex\LineSegRaster\IAM\5_in.png";
            //var imagePath = @"D:\Users\Stefan\Datasets\JL\018.jpg";
            //var imagePath = @"D:\Users\Stefan\Datasets\hw_flex\LineSegRaster\nego\002_in.jpg";
            //var imagePath = @"D:\Users\Stefan\Datasets\hw_flex\LineSegRaster\tmp\cap_2_in.jpg";
            //var imagePath = @"D:\Users\Stefan\Datasets\hw_flex\LineSegRaster\tmp\rt2.jpg";
            var image = RawBitmapIO.FromFile(imagePath);          
            
            var cts = new CancellationTokenSource();

            var pm = new ProgressMonitor(cts.Token);
            pm.ProgressChanged += (o, p, d) => Console.WriteLine($" [ProgressMonitor] [{p:000.00}]: {d}");
            int q = 0;
            Action<RawBitmap, string> action = (b, c) => b.Save($"cc\\predCC{q++}_{c}.png");
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
                        HTR.PredictTextLine(linebmp);
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
