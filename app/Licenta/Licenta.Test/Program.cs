using Licenta.Commons.Math;
using Licenta.Commons.Math.Arithmetics;
using Licenta.Commons.Parallelization;
using Licenta.Commons.Utils;
using Licenta.Imaging;
using Licenta.Utils;
using System;
using System.Collections.Generic;
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
            List<long> times = new List<long>();            

            int i = 0;
            foreach (var arg in Directory.GetFiles(@"D:\Users\Stefan\Datasets\JL").Take(10)) 
            {               
                Console.WriteLine(arg);
                var bmp = new Bitmap(arg);// @"C:\Users\Stefan\Desktop\017.jpg");
                bmp = Rescale(bmp, 50);
                Console.WriteLine("Loading");
                var img = new ImageRGB(bmp);
                Console.WriteLine("Detecting edges");                
                var time = Time.Measure(() => img = Images.CannyEdgeDetection(img, TaskManager));
                times.Add(time);
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
