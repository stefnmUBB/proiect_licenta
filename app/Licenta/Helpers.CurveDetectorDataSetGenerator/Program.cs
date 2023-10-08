using Licenta.Commons.AI;
using Licenta.Commons.AI.Perceptrons;
using System;
using System.Linq;
using System.Windows.Forms;

namespace Helpers.CurveDetectorDataSetGenerator
{
    internal static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            var ann = new AnnModel();
            ann.InputLength = 2;
            ann.OutputLength = 1;

            ann.HiddenLayers.Add(new AnnLayerModel(typeof(Self), 3));
            ann.HiddenLayers.Add(new AnnLayerModel(typeof(Relu), 4));

            Func<double, double, double> f = (x, y) => 2 * x + 3 * y;

            var r = new Random();

            var inputs = Enumerable.Range(0, 10).Select(_ => new double[2] { r.Next(0, 5) * r.NextDouble(), r.Next(0, 5) * r.NextDouble() })
                .ToArray();
            var outputs = inputs.Select(_ => new double[1] { f(_[0], _[1]) }).ToArray();

            var runtimeAnn = ann.Compile();

            var h = runtimeAnn.Train(inputs, outputs, epochsCount: 500);

            var outs = runtimeAnn.ComputeOutput(new double[] { 1, 2 });

            Console.WriteLine("Result = " + string.Join(", ", outs));
            Console.WriteLine("History:");
            h.Loss.ForEach(Console.WriteLine);



            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1());
        }
    }
}
