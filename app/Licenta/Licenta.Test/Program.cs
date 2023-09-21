using Licenta.Commons.Math;
using Licenta.Commons.Math.Arithmetics;
using Licenta.Imaging;
using Licenta.Utils;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Licenta.Test
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var img = new Image24(new Bitmap(@"C:\Users\Stefan\Desktop\016.jpg"));
            img.ToBitmap().Save("res.png");
            Console.WriteLine("Done");
            Console.ReadLine();
        }
    }
}
