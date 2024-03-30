using LillyScan.Backend.Math;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LillyScan.Backend.Tests.UnitTests.Math
{
    public class TensorTest
    {
        [Fact]
        public void Test_CreateNormal()
        {
            var shape = new Shape(2, 3);
            var buffer = new[] { 1, 2, 3, 4, 5, 6 };

            var t = new Tensor<int>(shape, buffer);
            Assert.True(t.Rank == 2);
            Assert.True(t.Shape.SequenceEqual(shape));
            Assert.True(t.Buffer.SequenceEqual(buffer));
        }

    }
}
