using Licenta.Commons.Math;
using Licenta.Commons.Math.Arithmetics;
using Licenta.Commons.Parallelization;
using Licenta.Commons.Utils;
using Licenta.Imaging;
using Licenta.Utils;
using Licenta.Utils.ParallelModels;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Licenta.Test
{
    internal class Program
    {
        private static Bitmap Rescale(Bitmap bmp, int percent)
        {
            return new Bitmap(bmp, bmp.Width * percent / 100, bmp.Height * percent / 100);
        }

        private static TaskManager TaskManager = new TaskManager(10).RunAsync();

        static void Main(string[] args)
        {           

            var dataset = new string[] { @"D:\Users\Stefan\Datasets\reteta.png" };

            var img = new ImageRGB(new Bitmap(dataset[0]));
            for (int t0 = 0; t0 < 50; t0 += 10) 
            {
                for (int t1 = 50; t1 < 100; t1 += 10) 
                {
                    var opts = new CannyEdgeDetectionOptions(t0 * 0.01, t1 * 0.01);
                    Console.WriteLine(opts);
                    var newImg = Images.CannyEdgeDetection(img, options: opts, tm: TaskManager);

                    Console.WriteLine("Saving"); 
                    using (var bmp = newImg.ToBitmap())
                        bmp.Save($@"dttest\{t0}_{t1}.png");
                }
            }                 
        }

        private static void DefaultMain()
        {
            List<long> times = new List<long>();
            var dataset = Directory.GetFiles(@"D:\Users\Stefan\Datasets\JL");
            ImageRGB img;

            int i = 0;
            foreach (var arg in dataset)
            {
                Console.WriteLine(arg);
                Console.WriteLine("Loading");                
                using (var bmp = new Bitmap(arg))
                    img = new ImageRGB(bmp);
                Console.WriteLine("Detecting edges");
                var time = Time.Measure(() => img = Images.CannyEdgeDetection(img, tm: TaskManager));
                times.Add(time);

                //var k = new Matrix<DoubleNumber>(3, 3, new DoubleNumber[] { 1, 1, 1, 1, 0, 1, 1, 1, 1 });
                //img = img.Convolve(Matrices.Multiply(k, new DoubleNumber(1.0 / 8)));

                //img = img.Convolve(Kernels.GaussianFilter(3, 3));

                Console.WriteLine("Saving");
                img.ToBitmap().Save($"res\\{i++}.png");
            }

            Console.WriteLine("Times:");
            times.ForEach(Console.WriteLine);

            Console.WriteLine($"Average = {times.Average()}");

            Console.WriteLine("Done");
            Console.ReadLine();
        }
    }
}
