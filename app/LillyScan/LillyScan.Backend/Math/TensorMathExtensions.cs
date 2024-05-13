using LillyScan.Backend.AI.Layers;
using LillyScan.Backend.Parsers;
using LillyScan.Backend.Utils;
using System;
using System.Collections.Generic;
using System.Data;
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

        public static Tensor<float> ClipByValue(this Tensor<float> t, float minValue, float maxValue)
        {
            var buffer = t.Buffer.ToArray();
            for(int i=0;i<buffer.Length;i++)
            {
                buffer[i] = buffer[i].Clamp(minValue, maxValue);
            }
            return new Tensor<float>(t.Shape, buffer);
        }

        private static void RunAdd(Tensor<float> t, float[] buffer, int[] shape, int[] mul)
        {
            int sRank = shape.Length;
            var dimul = new int[t.Rank];
            int p = 1;
            for (int i = t.Rank - 1; i >= 0; i--) 
            {
                dimul[i] = p;
                p *= t.Shape[i];
            }                                            
            var rank = sRank;

            int k = 0;

            var iter = new int[rank];
            while (iter[0] < shape[0]) 
            {
                int index = 0;
                for (int i = 0; i < t.Rank; i++)
                    index += dimul[i] * (iter[rank - t.Rank + i] / mul[i]);
                buffer[k++] += t.Buffer.Buffer[index];                
                for (int i = rank - 1, c = 1; i >= 0 && c > 0; i--)
                {
                    iter[i]++;
                    c = 0;
                    if (i > 0 && iter[i] == shape[i])
                    {
                        iter[i] = 0;
                        c = 1;
                    }
                }
            }
        }

        private static void RunSub(Tensor<float> t, float[] buffer, int[] shape, int[] mul)
        {
            int sRank = shape.Length;
            var dimul = new int[t.Rank];
            int p = 1;
            for (int i = t.Rank - 1; i >= 0; i--)
            {
                dimul[i] = p;
                p *= t.Shape[i];
            }
            var rank = sRank;

            int k = 0;

            var iter = new int[rank];
            while (iter[0] < shape[0])
            {
                int index = 0;
                for (int i = 0; i < t.Rank; i++)
                    index += dimul[i] * (iter[rank - t.Rank + i] / mul[i]);
                buffer[k++] -= t.Buffer.Buffer[index];
                for (int i = rank - 1, c = 1; i >= 0 && c > 0; i--)
                {
                    iter[i]++;
                    c = 0;
                    if (i > 0 && iter[i] == shape[i])
                    {
                        iter[i] = 0;
                        c = 1;
                    }
                }
            }
        }

        private static void RunMul(Tensor<float> t, float[] buffer, int[] shape, int[] mul)
        {
            int sRank = shape.Length;
            var dimul = new int[t.Rank];
            int p = 1;
            for (int i = t.Rank - 1; i >= 0; i--)
            {
                dimul[i] = p;
                p *= t.Shape[i];
            }
            var rank = sRank;

            int k = 0;

            var iter = new int[rank];
            while (iter[0] < shape[0])
            {
                int index = 0;
                for (int i = 0; i < t.Rank; i++)
                    index += dimul[i] * (iter[rank - t.Rank + i] / mul[i]);
                buffer[k++] *= t.Buffer.Buffer[index];
                for (int i = rank - 1, c = 1; i >= 0 && c > 0; i--)
                {
                    iter[i]++;
                    c = 0;
                    if (i > 0 && iter[i] == shape[i])
                    {
                        iter[i] = 0;
                        c = 1;
                    }
                }
            }
        }

        private static void RunDiv(Tensor<float> t, float[] buffer, int[] shape, int[] mul)
        {
            int sRank = shape.Length;
            var dimul = new int[t.Rank];
            int p = 1;
            for (int i = t.Rank - 1; i >= 0; i--)
            {
                dimul[i] = p;
                p *= t.Shape[i];
            }
            var rank = sRank;

            int k = 0;

            var iter = new int[rank];
            while (iter[0] < shape[0])
            {
                int index = 0;
                for (int i = 0; i < t.Rank; i++)
                    index += dimul[i] * (iter[rank - t.Rank + i] / mul[i]);
                buffer[k++] /= t.Buffer.Buffer[index];
                for (int i = rank - 1, c = 1; i >= 0 && c > 0; i--)
                {
                    iter[i]++;
                    c = 0;
                    if (i > 0 && iter[i] == shape[i])
                    {
                        iter[i] = 0;
                        c = 1;
                    }
                }
            }
        }

        private static (int[] resultShape, int[] m1, int[] m2) GetBinaryOpsData(Tensor<float> t1, Tensor<float> t2)
        {
            var resultRank = System.Math.Max(t1.Rank, t2.Rank);
            var resultShape = new int[resultRank];
            for (int i = 0; i < resultRank; i++) resultShape[i] = 1;
            for (int i = t1.Rank - 1; i >= 0; i--)
                resultShape[resultRank - t1.Rank + i] = t1.Shape[i];
            //Console.WriteLine(resultShape.JoinToString(", "));
            for (int i = t2.Rank - 1; i >= 0; i--)
            {
                var ri = resultRank - t2.Rank + i;
                if (resultShape[ri] != 1 && (t2.Shape[i] != resultShape[ri] && t2.Shape[i] != 1))
                    throw new InvalidOperationException($"Cannot perform element-wise operations on tensors of shapes {t1.Shape} and {t2.Shape}");
                if (t2.Shape[i] != 1)
                    resultShape[ri] = t2.Shape[i];
            }
            var m1 = new int[t1.Rank];
            var m2 = new int[t2.Rank];

            for (int i = t1.Rank - 1; i >= 0; i--)
            {
                var d = resultShape[resultRank - t1.Rank + i];
                m1[i] = d == t1.Shape[i] ? 1 : d;
            }

            for (int i = t2.Rank - 1; i >= 0; i--)
            {
                var d = resultShape[resultRank - t2.Rank + i];
                m2[i] = d == t2.Shape[i] ? 1 : d;
            }

            return (resultShape, m1, m2);
        }

        public static Tensor<float> FastFloatAdd(this Tensor<float> t1, Tensor<float> t2)
        {
            if (t1.Rank == 0 && t2.Rank == 0)            
                return new Tensor<float>(0, new[] { t1.Buffer[0] + t2.Buffer[0] });
            if (t2.Rank == 0)
                return FastFloatAdd(t2, t1);
            if (t1.Rank == 0)
            {
                var len = t2.Buffer.Length;
                var scalar = t1.Buffer[0];
                var buffer = new float[len];
                for (int i = 0; i < len; i++)                 
                    buffer[i] = t2.Buffer[i] + scalar;
                return new Tensor<float>(t2.Shape, buffer);
            }

            (var resultShape, var m1, var m2) = GetBinaryOpsData(t1, t2);
            var shape = new Shape(resultShape);
            var result = new float[shape.ElementsCount];            
            RunAdd(t1, result, resultShape, m1);
            RunAdd(t2, result, resultShape, m2);            
            return new Tensor<float>(shape, result);
        }

        public static Tensor<float> FastFloatSub(this Tensor<float> t1, Tensor<float> t2)
        {
            if (t1.Rank == 0 && t2.Rank == 0)
                return new Tensor<float>(0, new[] { t1.Buffer[0] + t2.Buffer[0] });            
            if (t1.Rank == 0)
            {
                var len = t2.Buffer.Length;
                var scalar = t1.Buffer[0];
                var buffer = new float[len];
                for (int i = 0; i < len; i++)
                    buffer[i] = scalar - t2.Buffer[i];
                return new Tensor<float>(t2.Shape, buffer);
            }
            if (t2.Rank == 0)
            {
                var len = t1.Buffer.Length;
                var scalar = t2.Buffer[0];
                var buffer = new float[len];
                for (int i = 0; i < len; i++)
                    buffer[i] = t1.Buffer[i] - scalar;
                return new Tensor<float>(t1.Shape, buffer);
            }

            (var resultShape, var m1, var m2) = GetBinaryOpsData(t1, t2);
            var shape = new Shape(resultShape);
            var result = new float[shape.ElementsCount];
            RunAdd(t1, result, resultShape, m1);
            RunSub(t2, result, resultShape, m2);
            return new Tensor<float>(shape, result);
        }

        public static Tensor<float> FastFloatMul(this Tensor<float> t1, Tensor<float> t2)
        {
            if (t1.Rank == 0 && t2.Rank == 0)
                return new Tensor<float>(0, new[] { t1.Buffer[0] + t2.Buffer[0] });
            if (t2.Rank == 0)
                return FastFloatMul(t2, t1);
            if (t1.Rank == 0)
            {
                var len = t2.Buffer.Length;
                var scalar = t1.Buffer[0];
                var buffer = new float[len];
                for (int i = 0; i < len; i++)
                    buffer[i] = t2.Buffer[i] * scalar;
                return new Tensor<float>(t2.Shape, buffer);
            }

            (var resultShape, var m1, var m2) = GetBinaryOpsData(t1, t2);
            var shape = new Shape(resultShape);
            var result = new float[shape.ElementsCount];
            RunAdd(t1, result, resultShape, m1);
            RunMul(t2, result, resultShape, m2);
            return new Tensor<float>(shape, result);
        }

        public static Tensor<float> FastFloatDiv(this Tensor<float> t1, Tensor<float> t2)
        {
            if (t1.Rank == 0 && t2.Rank == 0)
                return new Tensor<float>(0, new[] { t1.Buffer[0] + t2.Buffer[0] });
            if (t1.Rank == 0)
            {
                var len = t2.Buffer.Length;
                var scalar = t1.Buffer[0];
                var buffer = new float[len];
                for (int i = 0; i < len; i++)
                    buffer[i] = scalar / t2.Buffer[i];
                return new Tensor<float>(t2.Shape, buffer);
            }
            if (t2.Rank == 0)
            {
                var len = t1.Buffer.Length;
                var scalar = t2.Buffer[0];
                var buffer = new float[len];
                for (int i = 0; i < len; i++)
                    buffer[i] = t1.Buffer[i] / scalar;
                return new Tensor<float>(t1.Shape, buffer);
            }

            (var resultShape, var m1, var m2) = GetBinaryOpsData(t1, t2);
            var shape = new Shape(resultShape);
            var result = new float[shape.ElementsCount];
            RunAdd(t1, result, resultShape, m1);
            RunDiv(t2, result, resultShape, m2);
            return new Tensor<float>(shape, result);
        }

        public static Tensor<float> Sqrt(this Tensor<float> t)
        {
            int len = t.Buffer.Length;
            var buffer = new float[len];
            for (int i = 0; i < len; i++)
                buffer[i] = (float)System.Math.Sqrt(t.Buffer[i]);
            return new Tensor<float>(t.Shape, buffer);
        }

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
