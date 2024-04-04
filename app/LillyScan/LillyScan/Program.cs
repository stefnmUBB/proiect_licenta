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

        public static IHTREngine HTR = new DefaultHTREngine();

        static void Run()
        {
            //var m = ModelLoader.LoadFromString(File.ReadAllText("D:\\Public\\model_saver\\model64.txt"));
            //var b = ModelLoader.StreamToBytes(File.OpenRead("D:\\Public\\model_saver\\model64.txt"));
            //File.WriteAllBytes("seg_model_64.lsm", b);

            var img = RawBitmapIO.FromFile(@"D:\Users\Stefan\Datasets\hw_flex\LineSegRaster\IAM\1_in.png");
            //var img = RawBitmapIO.FromFile(@"D:\Users\Stefan\Datasets\hw_flex\LineSegRaster\tmp\tmp_002_buruianasergiu_ofaptabuna.jpg");
            img = img.Resize(64, 64);
            img = img.AverageChannels();
            img.ToBitmap().Save("holy0.png");
            float[] o = null;
            Measure(() => o = HTR.Segment64(img.ToArray()));

            img = new RawBitmap(64, 64, 1, o);
            RawBitmapIO.ToBitmap(img).Save("holy3.png");

            Console.WriteLine("Done");
            Console.ReadLine();
        }

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Backend.Initializer.Initialize();

            Application.Run(new MainForm());
        }
    }
}
