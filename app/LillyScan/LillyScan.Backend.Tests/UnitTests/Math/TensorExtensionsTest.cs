using LillyScan.Backend.Math;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LillyScan.Backend.Tests.UnitTests.Math
{
    public class TensorExtensionsTest
    {
        static readonly Shape shape1 = new Shape(2,2,3);
        static readonly Tensor<int> tensor1 = new Tensor<int>(shape1, Enumerable.Range(1, shape1.ElementsCount).ToArray());
        static readonly Func<int, int, int> SumRed = (int x, int y) => x + y;

        [Fact]
        public void Test_ReduceAxis_FirstAxis()
        {
            var computed = tensor1.ReduceAxis(SumRed, 0);
            var real = new Tensor<int>((2, 3), new[] { 8, 10, 12, 14, 16, 18 });
            Assert.True(real.Equals(computed));            
        }
    }
}
