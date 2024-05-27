using LillyScan.Backend.AI.Layers;
using LillyScan.Backend.Math;
using LillyScan.Backend.Tests.TestTemplates;

namespace LillyScan.Backend.Tests.UnitTests.AI.Layers
{
    using Conv2DFun = Action<float[], float[], float[], int, int, int, int, int, int, int>;
    public class UnsafeOperationsTest
    {
        static Func<(Tensor<float> Source, Tensor<float> Kernel), Tensor<float>> UnsafeConv2DWrapper(Conv2DFun conv)
            => input =>
            {
                var inShape = input.Source.Shape;
                var kerShape = input.Kernel.Shape;
                int b = inShape[0], n = inShape[1], m = inShape[2], c = inShape[3];
                int k1 = kerShape[0], k2 = kerShape[1], f = kerShape[3];
                var result = new float[b * n * m * f];
                conv(input.Source.Buffer.Buffer, input.Kernel.Buffer.Buffer, result, b, n, m, c, k1, k2, f);
                return new Tensor<float>((b, n, m, f), result);
            };        

        [Fact]
        public void Test_Conv2D_Unsafe()
        {
            TestTemplate.RunBatchTestFiles(Conv2DTestTemplate.FromTestFiles, "FileTests\\conv", 1, 4, UnsafeConv2DWrapper(UnsafeOperations.Conv2D));
        }

        [Fact]
        public void Test_Conv2D_Img2Col()
        {
            TestTemplate.RunBatchTestFiles(Conv2DTestTemplate.FromTestFiles, "FileTests\\conv", 1, 4, UnsafeConv2DWrapper(Img2Col.Conv2D));
        }

        [Fact]
        public void Test_Conv2D_Unsafe_Big()
        {
            TestTemplate.RunBatchTestFiles(Conv2DTestTemplate.FromTestFiles, "FileTests\\big_conv", 1, 2, UnsafeConv2DWrapper(UnsafeOperations.Conv2D));
        }

        [Fact]
        public void Test_Conv2D_Img2Col_Big()
        {
            TestTemplate.RunBatchTestFiles(Conv2DTestTemplate.FromTestFiles, "FileTests\\big_conv", 1, 2, UnsafeConv2DWrapper(Img2Col.Conv2D));
        }

    }
}
