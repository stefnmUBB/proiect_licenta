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
            /*var buf1 = new float[14*32];
            var buf2 = new float[8 * 32];
            var buf3 = new float[32];
            var buf4 = new float[14];
            var buf5 = new float[8];
            var buf6 = new float[8];

            var r = new Random();           
            buf1 = new float[] { 0, 0, 1, 1, 1, 1, 1, 1, 0, 1, 0, 1, 1, 0, 0, 1, 0, 0, 1, 0, 0, 0, 0, 1, 0, 1, 1, 1, 0, 0, 1, 0, 0, 0, 0, 1, 1, 0, 1, 1, 0, 1, 1, 0, 1, 0, 1, 1, 0, 1, 0, 0, 1, 0, 1, 1, 1, 0, 0, 0, 0, 0, 1, 0, 1, 0, 0, 1, 0, 1, 0, 1, 1, 1, 0, 0, 1, 0, 1, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 1, 0, 1, 1, 0, 1, 1, 1, 0, 1, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 1, 0, 0, 0, 0, 0, 1, 1, 0, 0, 1, 0, 0, 1, 1, 0, 0, 0, 0, 0, 1, 1, 0, 0, 1, 1, 1, 1, 0, 0, 1, 0, 0, 0, 1, 1, 1, 0, 1, 0, 1, 0, 1, 1, 0, 0, 1, 0, 1, 0, 0, 1, 1, 1, 1, 1, 1, 0, 0, 0, 0, 0, 0, 1, 1, 0, 0, 0, 0, 0, 0, 1, 1, 0, 1, 0, 0, 0, 0, 1, 0, 0, 0, 1, 0, 0, 1, 0, 0, 1, 1, 1, 1, 0, 1, 1, 0, 1, 0, 1, 1, 0, 0, 0, 0, 0, 1, 1, 0, 1, 1, 1, 0, 1, 1, 1, 1, 1, 0, 0, 1, 1, 0, 0, 0, 0, 1, 0, 0, 1, 1, 0, 1, 0, 1, 1, 0, 1, 1, 1, 0, 1, 0, 1, 0, 0, 1, 0, 0, 1, 0, 0, 1, 0, 1, 0, 0, 1, 0, 1, 1, 1, 0, 0, 1, 0, 1, 1, 1, 0, 1, 1, 1, 1, 1, 1, 0, 0, 0, 1, 0, 0, 1, 0, 1, 1, 1, 1, 0, 1, 1, 1, 1, 1, 0, 1, 1, 1, 1, 0, 0, 0, 1, 0, 0, 1, 1, 1, 0, 0, 1, 1, 0, 0, 0, 0, 1, 1, 1, 0, 0, 0, 0, 1, 0, 0, 1, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 1, 1, 0, 0, 1, 1, 0, 0, 0, 0, 1, 1, 0, 1, 1, 0, 1, 1, 0, 0, 1, 1, 1, 0, 1, 0, 1, 0, 1, 0, 1, 1, 0, 1, 0, 0, 1, 1, 0, 1, 1, 0, 0, 0, 0, 0, 0, 1, 0, 0, 1, 1, 0, 0, 0, 0, 0, 1, 1, 0, 0, 1, 1, 0, 1, 0, 0, 0, 0, 1, 0, 1, 1, 0, 0, 0, 0, 1, 0, 0, 0, 1, 0, 1, 0, 0, 0, 0, 1, 0, 1, 0, 0, 0, 1, 0, 1, 1 };
            buf2 = new float[] { 0, 1, 0, 0, 0, 0, 1, 1, 1, 0, 1, 0, 1, 1, 0, 0, 0, 0, 1, 0, 1, 0, 1, 0, 0, 0, 1, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 1, 1, 1, 1, 1, 0, 0, 0, 1, 1, 0, 1, 1, 1, 1, 0, 1, 1, 1, 1, 0, 1, 0, 1, 1, 0, 0, 1, 1, 1, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 1, 1, 1, 1, 0, 0, 1, 1, 1, 0, 0, 0, 0, 1, 0, 1, 1, 1, 1, 0, 0, 0, 1, 1, 0, 1, 1, 1, 0, 0, 1, 0, 1, 1, 0, 0, 0, 0, 0, 1, 1, 0, 0, 1, 0, 1, 0, 0, 1, 1, 1, 0, 1, 0, 0, 0, 0, 1, 0, 0, 0, 1, 0, 0, 0, 1, 1, 1, 1, 1, 1, 0, 1, 1, 0, 1, 0, 1, 0, 1, 1, 1, 1, 1, 0, 1, 1, 0, 1, 1, 0, 0, 0, 1, 0, 0, 1, 1, 1, 0, 1, 1, 1, 1, 1, 0, 1, 1, 0, 0, 1, 0, 1, 0, 1, 1, 1, 1, 1, 1, 0, 1, 1, 1, 1, 1, 0, 1, 1, 0, 0, 1, 0, 0, 1, 0, 1, 1, 1, 0, 1, 1, 0, 1, 1, 1, 1, 1, 0, 0, 0, 0, 1, 0, 0, 1, 1, 1, 0, 0, 0, 1, 0, 1, 0, 1, 0, 0, 0, 0, 1, 1, 1, 1, 1, 0, 1, 0, 1 };
            buf3 = new float[] { 1, 1, 0, 1, 0, 1, 0, 0, 0, 1, 0, 1, 1, 1, 0, 1, 1, 1, 0, 0, 0, 1, 0, 0, 0, 0, 1, 0, 1, 0, 1, 0 };
            buf4 = new float[] { 0, 0, 1, 1, 0, 0, 1, 0, 1, 1, 0, 1, 0, 1 };
            buf5 = new float[] { 1, 0, 0, 0, 1, 0, 0, 1 };
            buf6 = new float[] { 1, 1, 1, 1, 0, 1, 0, 1 };


            var W = new Tensor<float>((14, 32), buf1);
            var U = new Tensor<float>((8, 32), buf2);
            var B = new Tensor<float>((32), buf3);
            var X = new Tensor<float>((14), buf4);
            var H = new Tensor<float>((8), buf5);
            var C = new Tensor<float>((8), buf6);
            
            using(var f=File.Create("cc\\w.txt"))
            {
                using(var w=new StreamWriter(f))
                {
                    w.WriteLine($"W=[{buf1.JoinToString(", ")}]");
                    w.WriteLine($"U=[{buf2.JoinToString(", ")}]");
                    w.WriteLine($"B=[{buf3.JoinToString(", ")}]");
                    w.WriteLine($"X=[{buf4.JoinToString(", ")}]");
                    w.WriteLine($"H=[{buf5.JoinToString(", ")}]");
                    w.WriteLine($"C=[{buf6.JoinToString(", ")}]");
                }
            }

            var cell = new LSTMCell(14, 8, useBias: true);
            cell.Context.Weights["W"] = W;
            cell.Context.Weights["U"] = U;
            cell.Context.Weights["B"] = B;

            var R = cell.Call(new[] { C, H, X });
            R[1].Print();
            R[0].Print();            


            return;*/

            /*var bff = new float[] { 1, -1, 1, -1, 1, -1, 2, -2, 2, -2, 2, -2, 3, -3, 3, -3, 3, -3 };
            var t = new Tensor<float>((3, 2, 3), bff);
            t.Print();
            t[null, new IndexAccessor(1)].Print();

            return;*/
            /*Measure(() =>
            {
                int B = 1, N = 128, M = 1, C = 1024, F = 82;
                int K1 = 1, K2 = 1;

                var t = Tensors.Ones<float>((B, N, M, C));
                var k = Tensors.Ones<float>((K1, K2, C, F));
                var r = new float[B * N * M * F];
                UnsafeOperations.Conv2D(t.Buffer.Buffer, k.Buffer.Buffer, r, B, N, M, C, K1, K2, F);
                new Tensor<float>((B, N, M, F), r).Print();
                //Console.WriteLine(r.JoinToString(", "));
            });
           
            return;*/
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

            using (var bmp = RawBitmapIO.FromFile(@"D:\anu3\proiect_licenta\app\LillyScan\LillyScan\bin\Debug\cc\IAM1.png")) 
            {
                Layer.X = 0;
                HTR.PredictTextLine(bmp);
            }
            return;

            //var task = Task.Run(() =>
            {
                Measure(() =>
                {                    
                    var lines = HTR.SegmentLines(image, pm, resizeToOriginal: false);
                    foreach(var mask in lines)
                    {
                        var linebmp = mask.CutFromImage(image);
                        linebmp.CheckNaN();
                        linebmp = linebmp.RotateAndCrop((float)-System.Math.Atan2(-mask.LineFit.A, mask.LineFit.B), disposeOriginal: true);
                        linebmp.CheckNaN();
                        //action(linebmp, "");
                        Layer.X = 0;
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
