using LillyScan.Backend;
using LillyScan.Backend.AI.Activations;
using LillyScan.Backend.AI.Layers;
using LillyScan.Backend.AI.Models;
using LillyScan.Backend.API;
using LillyScan.Backend.Imaging;
using LillyScan.Backend.Math;
using LillyScan.Backend.Math.Arithmetics.BuiltInTypeWrappers;
using LillyScan.Backend.Types;
using LillyScan.Backend.Utils;
using LillyScan.BackendWinforms.Imaging;
using LillyScan.BackendWinforms.Utils;
using LillyScan.Properties;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;

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

        //private static Model model64 = ModelLoader.LoadFromBytes(File.ReadAllBytes(@"D:\Public\CNNLSTMLineSeg64\train_model_1.txt.lsm"));
        public static HTREngine HTR = new DefaultHTREngine();

        static void Run()
        {
            var inf = @"D:\Users\Stefan\Datasets\hw_flex\LineSegRaster\JL\20240406_021410.jpg";

            //foreach (var inf in Directory.GetFiles(@"D:\Users\Stefan\Datasets\JL")) 
            {
                //if (!inf.Contains("3")) continue;
                var fn = Path.GetFileNameWithoutExtension(inf);
                using (var img0 = RawBitmapIO.FromFile(inf))
                {
                    var img = img0.CropCenteredPercent(70, 70);
                    Console.WriteLine($"IMG: {img.Width}, {img.Height}");
                    using (var bmp = img.ToBitmap())
                        bmp.Save($"JL2\\{fn}_in.jpg", System.Drawing.Imaging.ImageFormat.Jpeg);
                    using (var pred = HTR.SelectTiled64(img, parallel: true))
                    using (var bmp = pred.ToBitmap())
                        bmp.Save($"JL2\\{fn}_out.png");
                }
            }

            return;

            /*
            //var m = ModelLoader.LoadFromString(File.ReadAllText("D:\\Public\\model_saver\\model64.txt"));
            //var b = ModelLoader.StreamToBytes(File.OpenRead("D:\\Public\\model_saver\\model64.txt"));
            //File.WriteAllBytes("seg_model_64.lsm", b);

            //var img = RawBitmapIO.FromFile(@"D:\Users\Stefan\Datasets\hw_flex\LineSegRaster\IAM\1_in.png");
            var img = RawBitmapIO.FromFile(@"D:\Users\Stefan\Datasets\hw_flex\LineSegRaster\tmp\IMG-20210209-WA0004.jpg");
            //var img = RawBitmapIO.FromFile(@"D:\Users\Stefan\Datasets\hw_flex\LineSegRaster\tmp\tmp_002_buruianasergiu_ofaptabuna.jpg");
            img = img.Resize(256, 256);
            img = img.AverageChannels();
            img.ToBitmap().Save("holy0.png");
            float[] o = null;
            Measure(() => o = HTR.Segment(img.ToArray()));

            img = new RawBitmap(256, 256, 3, o);
            RawBitmapIO.ToBitmap(img).Save("holy3.png");

            Console.WriteLine("Done");
            Console.ReadLine();*/
        }

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Backend.Initializer.Initialize();
            Run();
            Console.WriteLine("Done"); Console.ReadLine();
            //Application.Run(new MainForm());
        }
    }
}
