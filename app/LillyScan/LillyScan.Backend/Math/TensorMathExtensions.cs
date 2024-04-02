using LillyScan.Backend.AI.Layers;
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


        public static Tensor<T> ReduceSum<T>(this Tensor<T> t, AxisCollection axis = null, bool keepDimensions=false)
        {
            if (typeof(T) == typeof(float))
            {
                var f = t as Tensor<float>;
                return f.ReduceAxis((x, y) => x + y, axis, keepDimensions) as Tensor<T>;
            }
            return t.ReduceAxis((x, y) => (dynamic)x + y, axis, keepDimensions);
        }            

        private static Tensor<T> MatMul2<T>(this Tensor<T> a, Tensor<T> b)
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
        }

        private static unsafe Tensor<float> MatMul2Float32(this Tensor<float> a, Tensor<float> b)
        {
            int m= a.Shape[-2], n = a.Shape[-1], p = b.Shape[-1];
            var results = new float[m * p];
            fixed (float* abuf = &a.Buffer.Buffer[0])
            fixed (float* bbuf = &b.Buffer.Buffer[0])
            fixed (float* rbuf = &results[0])
                UnsafeOperations.MatMul(abuf, bbuf, rbuf, m, n, p);
            return new Tensor<float>(new Shape(m, p), results);
        }

        public static Tensor<T> MatMul<T>(this Tensor<T> t1, Tensor<T> t2)
        {
            if (t1.Rank < 2 || t2.Rank < 2)
                throw new ArgumentException("Ranks of matmul tensors must be at least 2");

            if (t1.Shape[-1] != t2.Shape[-2])
                throw new ArgumentException($"Cannot matmul tensors of shapes {t1.Shape} and {t2.Shape}");

            if(typeof(T)==typeof(float))
            {                
                if (t1.Rank == 2 && t2.Rank == 2)
                    return MatMul2Float32(t1 as Tensor<float>, t2 as Tensor<float>) as Tensor<T>;
                return (t1 as Tensor<float>).SubDimBroadcast(t2 as Tensor<float>, MatMul2Float32, 2) as Tensor<T>;
            }

            if (t1.Rank == 2 && t2.Rank == 2)
                return MatMul2(t1, t2);
            return t1.SubDimBroadcast(t2, MatMul2, 2);
        }
        
    }
}
