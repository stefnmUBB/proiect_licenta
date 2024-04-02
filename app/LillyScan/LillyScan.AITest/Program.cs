using LillyScan.Backend.AI.Math;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace LillyScan.AITest
{
    internal class Program
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

        static void Run()
        {
            var a = Tensors.Ones((1, 4, 4, 1));
            var b = Tensors.Ones((3, 3, 1, 1));
            Tensor? c = null;
            Measure(() => c = a.Conv2D(b));
            c?.Print("c=");


            a.Dispose();
            b.Dispose();
            c?.Dispose();            
        }

        static void Main(string[] args)
        {
            Console.WriteLine("Starting");
            Thread.Sleep(5000);
            Run();
            Console.WriteLine("Done");
            Console.ReadLine();
        }
    }
}
