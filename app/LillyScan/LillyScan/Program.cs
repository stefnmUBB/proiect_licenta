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
using LillyScan.BackendWinforms.Utils;
using LillyScan.Properties;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;

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

        static IHTREngine HTR = new DefaultHTREngine();

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Backend.Initializer.Initialize();                        

            var img = ImageRGBIO.FromBitmap(new Bitmap(new Bitmap(@"D:\Users\Stefan\Datasets\hw_flex\LineSegRaster\tmp\tmp_002_buruianasergiu_ofaptabuna.jpg"), new Size(256, 256)))
                .Select(x => (float)((x.R.Value + x.G.Value + x.B.Value) / 3)).Items;

            float[] o = null;
            Measure(() => o = HTR.Segment(img));
                      
            var ocolors = o.GroupChunks(3).Select(x => new ColorRGB(x[0], x[1], x[2])).ToArray();
            var oimg = new ImageRGB(new Matrix<ColorRGB>(256, 256, ocolors));
            oimg.ToBitmap().Save("holy2.png");

            Console.WriteLine("Done");
            Console.ReadLine();
        }
    }
}
