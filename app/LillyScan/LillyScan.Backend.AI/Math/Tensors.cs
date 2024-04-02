using System;
using System.Collections.Generic;
using System.Text;

namespace LillyScan.Backend.AI.Math
{
    public static class Tensors
    {
        public static Tensor Constant(Shape shape, float value)
        {
            var buffer = new float[shape.ElementsCount];
            for (int i = 0; i < buffer.Length; i++) buffer[i] = value;
            return new Tensor(shape, buffer);
        }

        public static Tensor Ones(Shape shape) => Constant(shape, 1);
        public static Tensor Zeros(Shape shape) => Constant(shape, 0);
    }
}
