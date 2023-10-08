using HelpersCurveDetectorDataSetGenerator.Commons.Math;
using HelpersCurveDetectorDataSetGenerator.Commons.Math.Arithmetics;
using HelpersCurveDetectorDataSetGenerator.Commons.Parallelization;
using HelpersCurveDetectorDataSetGenerator.Imaging;
using HelpersCurveDetectorDataSetGenerator.Utils.ParallelModels;

namespace HelpersCurveDetectorDataSetGenerator.Utils
{
    public static class Images
    {
        public static Matrix<DoubleNumber> ToGrayScaleMatrixLinear(this ImageRGB img, double factorR = 0.299, double factorG = 0.587, double factorB = 0.114)
        {
            return Matrices.DoEachItem(img, c => (DoubleNumber)(factorR * c.R.Value + factorG * c.G.Value + factorB * c.B.Value));
        }

        public static ImageRGB CannyEdgeDetection(this ImageRGB img, CannyEdgeDetectionOptions options = null, TaskManager tm = null)
        {
            return new ImageRGB(new CannyEdgeDetectionParallelModel(options ?? new CannyEdgeDetectionOptions())
                .Run(img, tm));
        }

    }
}
