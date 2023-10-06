using Licenta.Commons.Math;
using Licenta.Commons.Math.Arithmetics;
using Licenta.Commons.Parallelization;
using Licenta.Commons.Utils;
using Licenta.Imaging;
using System;

namespace Licenta.Utils.ParallelModels
{
    using DoubleMatrix = Matrix<DoubleNumber>;
    internal class CannyEdgeDetectionParallelModel : ParallelGraphModel
    {
        public CannyEdgeDetectionParallelModel()
        {
            var inputImage = CreateInput<ImageRGB>();

            var grayScaleFilter = CreateNode<ImageRGB>(ToGrayscale, inputImage);
            var gaussianFilter = CreateNode<DoubleMatrix>(GaussianFilter, grayScaleFilter);

            var gradX = CreateNode<DoubleMatrix>(ComputeGradX, gaussianFilter);
            var gradY = CreateNode<DoubleMatrix>(ComputeGradY, gaussianFilter);

            var magnitude = CreateNode<DoubleMatrix, DoubleMatrix>(ComputeMagnitude, gradX, gradY);
            var direction = CreateNode<DoubleMatrix, DoubleMatrix>(ComputeDirection, gradX, gradY);

            var refined = CreateNode<DoubleMatrix, DoubleMatrix>(Refine, magnitude, direction);

            var doubleThreshold = CreateOutput<DoubleMatrix>(_ => Matrices.ApplyDoubleThreshold(_, 0, 1, 0.2, 0.8), refined);
        }

        public DoubleMatrix Run(ImageRGB image, TaskManager tm = null) => Run(tm, new object[] { image })[0] as DoubleMatrix;

        private static DoubleMatrix ToGrayscale(ImageRGB img) => img.ToGrayScaleMatrix();
        private static DoubleMatrix GaussianFilter(DoubleMatrix m) => Matrices.Convolve(m, Kernels.GaussianFilter(5, 5, 1.0));
        private static DoubleMatrix ComputeGradX(DoubleMatrix m) => Matrices.Convolve(m, Kernels.SobelX());
        private static DoubleMatrix ComputeGradY(DoubleMatrix m) => Matrices.Convolve(m, Kernels.SobelY());
        private static DoubleMatrix ComputeMagnitude(DoubleMatrix gx, DoubleMatrix gy) => Matrices.DoItemByItem(gx, gy, Functions.Hypot);
        private static DoubleMatrix ComputeDirection(DoubleMatrix gx, DoubleMatrix gy) => Matrices.DoItemByItem(gx, gy, Functions.Atan2);

        private static DoubleMatrix Refine(DoubleMatrix magn, DoubleMatrix dir)
        {
            magn = new DoubleMatrix(magn);
            (int X, int Y)[] dirs = { (1, 0), (1, 1), (0, 1), (-1, -1), (-1, 0) };

            (int X, int Y) getGradientDirection(double angle)
            {
                var i = (int)(angle * 4 / Math.PI);
                var d = dirs[(i < 0 ? i : -i).Clamp(0, 4)];
                if (i < 0) d.Y = -d.Y;
                return d;
            }

            for (int r = 0; r < magn.RowsCount; r++)
            {
                for (int c = 0; c < magn.ColumnsCount; c++)
                {
                    var d = getGradientDirection(dir[r, c].Value);
                    var r2 = r + d.Y;
                    var c2 = c + d.X;
                    if (0 <= r2 && r2 < magn.RowsCount && 0 <= c2 && c2 < magn.ColumnsCount)
                    {
                        if (magn[r, c].Value < magn[r2, c2].Value)
                        {
                            magn[r, c] = new DoubleNumber(0);
                            continue;
                        }
                    }
                    d = (-d.X, -d.Y);
                    r2 = r + d.Y;
                    c2 = c + d.X;
                    if (0 <= r2 && r2 < magn.RowsCount && 0 <= c2 && c2 < magn.ColumnsCount)
                    {
                        if (magn[r, c].Value < magn[r2, c2].Value)
                        {
                            magn[r, c] = new DoubleNumber(0);
                            continue;
                        }
                    }
                }
            }
            return magn;
        }
    }
}
