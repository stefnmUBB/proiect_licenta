using Licenta.Commons.Math;
using Licenta.Commons.Math.Arithmetics;
using Licenta.Imaging;
using Licenta.Utils;
using System;
using System.Drawing;

namespace Licenta.Test
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var bmp = new Bitmap(@"C:\Users\Stefan\Desktop\016.jpg");
            //bmp = new Bitmap(bmp, 512, 512);
            var img = new ImageRGB(bmp);

            var c = new Matrix<DoubleNumber>(3, 3, new DoubleNumber[]
            {
                0,1,0,
                1,-4,1,
                0,1,0
            });

            img = img.Convolute(c);// Matrices.Multiply(c, new DoubleNumber(0.1 / 9)));
            img.ToBitmap().Save("res.png");
            Console.WriteLine("Done");
            Console.ReadLine();
        }
    }
}
