using Licenta.Commons.Math;
using Licenta.Commons.Math.Arithmetics;
using Licenta.Commons.Parallelization;
using Licenta.Commons.Utils;
using Licenta.Imaging;
using Licenta.Utils.ParallelModels;
using System;

namespace Licenta.Utils
{
    public static class Images
    {
        public static Matrix<DoubleNumber> ToGrayScaleMatrix(this ImageRGB img)
        {
            return Matrices.DoEachItem(img, c => (DoubleNumber)(0.299 * c.R.Value + 0.587 * c.G.Value + 0.114 * c.B.Value));
        }

        public static ImageRGB CannyEdgeDetection(this ImageRGB img, TaskManager tm = null)
        {
            return new ImageRGB(new CannyEdgeDetectionParallelModel().Run(img, tm));
        }

    }
}
