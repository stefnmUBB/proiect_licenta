using LillyScan.Backend.Math;
using LillyScan.Backend.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace LillyScan
{
    internal static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            var t = new Tensor<float>((2, 3, 2), new float[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12 });
            //t = t.Transpose(new[] { 1,0,2 });
            var t1 = new Tensor<float>((1, 5, 5, 1), new float[] { 1, 2, 3, 4, 5, 2, 3, 4, 5, 6, 3, 4, 5, 6, 7, 4, 5, 6, 7, 8, 5, 6, 7, 8, 9 });
            var t2 = new Tensor<float>((3, 3, 1, 2), new float[]
            {
                1,2, 1,2, 1,2,
                1,2, 1,2, 1,2,
                1,2, 1,2, 1,2,
            });

            //Console.WriteLine(t1.ReduceAxis(0, Operations.Sum<float>()).Print());
            t1.Reshape((5, 5)).Print();

            t = t1.Conv2D(t2).Reshape((5, 5, 2));

            t[null, null, new IndexAccessor(0)].Print();
            t[null, null, new IndexAccessor(1)].Print();

            //t1.Conv2D(t2).Reshape((5, 5, 2)).Print();
                      



            //var t1 = new Tensor<float>((2, 2), new float[] { 1, 2, 3, 4 });
            //var t2 = Tensors.Ones<float>((3, 2, 1));



            //Console.WriteLine(t.Shape.IterateSubDimsIndices(1).ToArray().JoinToString("\n"));

            Console.ReadLine();
            /*Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());*/
        }
    }
}
