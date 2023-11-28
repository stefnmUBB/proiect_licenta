using Licenta.Commons.Math;
using Licenta.Commons.Math.Arithmetics;
using Licenta.Commons.Parallelization;
using Licenta.Commons.Utils;
using Licenta.Imaging;
using System;
using System.Diagnostics;
using System.Linq;

namespace Licenta.Utils.ParallelModels
{
    using DoubleMatrix = Matrix<double>;
    internal class MonochromeParallelModel : ParallelGraphModel
    {
        private readonly double Threshold;

        public MonochromeParallelModel(double threshold = 0.5)
        {
            Threshold = threshold;

            var inputImage = CreateInput<ImageRGB>();            
            var grayScaleFilter = CreateNode<ImageRGB>(ToGrayscaleHSL, inputImage);            
            var gaussianFilter = CreateNode<DoubleMatrix>(GaussianFilter, grayScaleFilter);                       
            var increasedConstrast = CreateNode<DoubleMatrix>(IncreaseContrast, gaussianFilter);                       
            var thresholdFilter = CreateOutput<DoubleMatrix>(ApplyThreshold, increasedConstrast);            
            //var thresholdFilter = CreateOutput<DoubleMatrix>(ApplyThreshold, grayScaleFilter);            
        }

        static DoubleMatrix IncreaseContrast(DoubleMatrix m)
        {
            Console.WriteLine("IncreaseContrast");
            try
            {
                m = Matrices.SelectChunks(m, 32, 32, s =>
                {
                    s = Matrices.DoEachItem(s, (x, i, j) =>
                    {
                        double[] n =
                        {
                        s[(i-1).Clamp(0,s.RowsCount-1), (j-1).Clamp(0,s.ColumnsCount-1)],
                        s[(i-1).Clamp(0,s.RowsCount-1), (j+1).Clamp(0,s.ColumnsCount-1)],
                        s[(i+1).Clamp(0,s.RowsCount-1), (j-1).Clamp(0,s.ColumnsCount-1)],
                        s[(i+1).Clamp(0,s.RowsCount-1), (j+1).Clamp(0,s.ColumnsCount-1)],
                        };

                        var a = n.Where(q => q <= x).Count();
                        if (a == 2) return n.Average();

                        if (a > 2) return (x + n.Min()) / 2;
                        return (x + n.Max()) / 2;
                    });
                    return s;
                });

                /*m = Matrices.SelectChunks(m, 256, 128, s =>
                {
                    var avg = Matrices.ItemsSum(s) / s.Items.Length;
                    s = Matrices.DoEachItem(s, (x) =>
                    {
                        return (2 * (x - 0.7) + 0.3).Clamp(0, 1);
                    });
                    return s;
                });*/
            }
            catch(Exception e)
            {
                Debug.WriteLine(e.Message);
                Debug.WriteLine(e.StackTrace);

            }

            return m;
        }

        public DoubleMatrix ApplyThreshold(DoubleMatrix m)
        {
            Console.WriteLine("ApplyThreshold");            
            var th = Threshold;

            if (th == 0)
            {
                th = m.Items.Select(_ => (int)(_ * 255)).Distinct().Average() / 255d;
            }

            Debug.WriteLine($"MyThreshold = {th}");

            var result = Matrices.DoEachItem(m, x => (x < th ? 0.0 : 1.0));            
            return result;

        }        

        public DoubleMatrix Run(ImageRGB image, TaskManager tm = null) => Run(tm, new object[] { image })[0] as DoubleMatrix;
        public DoubleMatrix RunSync(ImageRGB image) => RunSync(new object[] { image })[0] as DoubleMatrix;


        private static DoubleMatrix ToGrayscaleHSL(ImageRGB img)
        {
            var m2 = Matrices.DoEachItem(img.ToHSL(), c => new ColorHSL(c.H, c.S.Clamp(0.2f, 1), ((float)Math.Pow(c.S - 0.5f, 2) * 0.5f + c.L * c.L).Clamp(0.2f, 0.9f) + 0.1f));

            var max = m2.Items.Max(_ => _.L);
            var min = m2.Items.Min(_ => _.L);
            var avg = m2.Items.Average(_ => _.L / max);
            var l0 = avg.Clamp(0.5f, 1);
            var l1 = (avg * 3 / 2).Clamp(l0, 1);

            m2 = Matrices.DoEachItem(m2, x =>
            {
                return new ColorHSL(x.H, x.S, x.L);
            });

            var m = Matrices.DoEachItem(m2, x => x.ToRGB());
            var r = Matrices.DoEachItem(m, _ => (_.R.Value + _.G.Value + _.B.Value) / 3);
            
            //Display.Show(new ImageRGB(r));

            return r;
        }            

        private static DoubleMatrix ToGrayscaleDefault(ImageRGB img)
            => Matrices.DoEachItem(img.ToGrayScaleMatrixLinear(), x => x.Value);
        private static DoubleMatrix Average(DoubleMatrix m1, DoubleMatrix m2)
            => Matrices.DoItemByItem(m1, m2, (a, b) => (a + b) * 0.5);
        private static DoubleMatrix GaussianFilter(DoubleMatrix m) => Matrices.Convolve(m, Kernels.GaussianFilter(5, 5, 1.0), Matrices.ConvolutionBorder.Extend);       

    }
}
