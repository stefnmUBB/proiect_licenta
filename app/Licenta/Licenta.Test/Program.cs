using Licenta.Commons.Math;
using Licenta.Commons.Math.Arithmetics;
using Licenta.Imaging;
using Licenta.Utils;
using System;
using System.Diagnostics;
using System.Drawing;

namespace Licenta.Test
{
    internal class Program
    {
        static void Main(string[] args)
        {            
            var bmp = new Bitmap(@"C:\Users\Stefan\Desktop\017.jpg");
            //bmp = new Bitmap(bmp, 512, 512);

            Console.WriteLine("Loading");
            var img = new ImageRGB(bmp);

            Console.WriteLine("Convoluting");
            //img = img.Convolute(Kernels.GaussianFilter(5, 5, normalize: true));
            //img = img.Convolute(Kernels.Laplacian()); 

            img = Images.CannyEdgeDetection(img);

            Console.WriteLine("Saving");
            img.ToBitmap().Save("res.png");
            Console.WriteLine("Done");
            Console.ReadLine();
        }
    }
}
