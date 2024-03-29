using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LillyScan.Backend.Math
{
    public static class TensorMathExtensions
    {
        public static Tensor<float> Add(this Tensor<float> t1, Tensor<float> t2)
            => t1.PerformElementWiseBinaryOperation(t2, (x, y) => x + y);

        public static Tensor<float> Subtract(this Tensor<float> t1, Tensor<float> t2)
            => t1.PerformElementWiseBinaryOperation(t2, (x, y) => x - y);

        public static Tensor<float> Multiply(this Tensor<float> t1, Tensor<float> t2)
            => t1.PerformElementWiseBinaryOperation(t2, (x, y) => x * y);

        public static Tensor<float> Divide(this Tensor<float> t1, Tensor<float> t2)
            => t1.PerformElementWiseBinaryOperation(t2, (x, y) => x * y);


        public static Tensor<T> MatMul<T>(this Tensor<T> t1, Tensor<T> t2)
        {
            if (t1.Rank < 2 || t2.Rank < 2)
                throw new ArgumentException("Ranks of matmul tensors must be at least 2");

            if (t1.Shape[-1] != t2.Shape[-2])
                throw new ArgumentException($"Cannot matmul tensors of shapes {t1.Shape} and {t2.Shape}");

            return t1.SubDimBroadcast(t2, (a, b) =>
            {
                int m = a.Shape[-2], n = a.Shape[-1], p = b.Shape[-1];
                var results = new dynamic[m * p];
                for (int i = 0; i < results.Length; i++) results[i] = 0;

                for (int i = 0; i < m; i++)
                    for (int j = 0; j < p; j++)
                        for (int k = 0; k < n; k++)
                            results[i * p + j] += (dynamic)a.GetValueAt(i, k) * (dynamic)b.GetValueAt(k, j);

                var buffer = results.Select(_ => (T)_).ToArray();
                return new Tensor<T>(new Shape(m, p), buffer);
            }, 2);
        }                
    }
}
