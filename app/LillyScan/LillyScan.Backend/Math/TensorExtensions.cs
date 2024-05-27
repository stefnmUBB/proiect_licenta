using LillyScan.Backend.AI.Layers;
using LillyScan.Backend.Utils;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO.Compression;
using System.Linq;
using System.Net.Http.Headers;
using System.Runtime.InteropServices;
using System.Text;

namespace LillyScan.Backend.Math
{
    public static class TensorExtensions
    {
        public static Tensor<T> Reshape<T>(this Tensor<T> tensor, Shape newShape)
        {
            if (tensor.IsScalar && newShape.Length == 1 && newShape[0] == 1)
                return new Tensor<T>(newShape, tensor.Buffer);

            if (tensor.Shape.ElementsCount != newShape.ElementsCount)
                throw new InvalidOperationException($"Cannot reshape tensor of shape {tensor.Shape} to {newShape}");
            return new Tensor<T>(newShape, tensor.Buffer);
        }        

        private static Tensor<T> PadConstant<T>(this Tensor<T> tensor, (int Before, int After)[] paddings, T constant_pad_value)
        {
            ValidatePaddings(tensor, paddings);
            var shape = GetPaddedShape(tensor, paddings);
            var buffer = new T[shape.ElementsCount];
            for (int i = 0; i < buffer.Length; i++) buffer[i] = constant_pad_value;
            foreach (var it in tensor.Shape.IterateIndices())
            {
                var it1 = paddings.Zip(it, (x, y) => x.Before + y).ToArray();
                buffer[shape.GetBufferIndex(it1)] = tensor.GetValueAt(it);
            }
            return new Tensor<T>(shape, buffer);
        }

        public static Tensor<T> Pad<T>(
            this Tensor<T> tensor, (int Before, int After)[] paddings,
            PaddingMode paddingMode = PaddingMode.Constant, T constant_pad_value = default)
        {
            switch(paddingMode)
            {
                case PaddingMode.Constant: return PadConstant(tensor, paddings, constant_pad_value);
                default: throw new NotImplementedException(paddingMode.ToString());
            }
        }


        private static void ValidatePaddings<T>(Tensor<T> tensor, (int Before, int After)[] paddings)
        {
            if (paddings.Length != tensor.Rank)
                throw new ArgumentException($"Number of paddings should be the same as the rank of the tensor");
        }

        private static Shape GetPaddedShape<T>(Tensor<T> tensor, (int Before, int After)[] paddings)
        {
            var dims = paddings.Zip(tensor.Shape, (p, s) => p.Before + s + p.After).ToArray();
            return new Shape(dims);
        }        

        public static Tensor<T> Transpose<T>(this Tensor<T> tensor, int[] perm = null)
        {
            perm = perm ?? Enumerable.Range(0, tensor.Rank).Reverse().ToArray();
            ValidatePerm(perm, tensor.Rank);
            var shape = new Shape(perm.Select(_ => tensor.Shape[_]).ToArray());
            var buffer = new T[shape.ElementsCount];
            foreach (var it in tensor.Shape.IterateIndices()) 
            {
                buffer[tensor.Shape.GetBufferIndex(it, perm)] = tensor.GetValueAt(it);
            }            
            return new Tensor<T>(shape, buffer);
        }

        private static void ValidatePerm(int[] perm, int rank)
        {
            if (perm.Length != rank)
                throw new ArgumentException($"Axis permutation length must be equal to tensor rank. Given ({perm.JoinToString(", ")}) for {rank} dimensions");

            var dims = new List<int>();

            for (int i = 0; i < perm.Length; i++)
                if (perm[i].IsBetween(0, rank - 1))
                    dims.Add(perm[i]);

            if (dims.Count != rank)
                throw new ArgumentException($"Invalid permutation ({perm.JoinToString(", ")}) for {rank} dimensions");
        }

        public static Tensor<V> PerformElementWiseBinaryOperation<T, U, V>(this Tensor<T> t1, Tensor<U> t2, Func<T, U, V> op)
        {
            if(t1.Rank==0 && t2.Rank==0)
            {
                return new Tensor<V>(0, new[] { op(t1.Buffer[0], t2.Buffer[0])});                
            }
            if (t2.Rank == 0)
                return PerformElementWiseBinaryOperation(t2, t1, (x, y) => op(y, x));
            if(t1.Rank==0)
            {
                var scalar = t1.Buffer[0];
                var buff = t2.Buffer.Select(_ => op(scalar,_)).ToArray();
                return new Tensor<V>(t2.Shape, buff);
            }


            if (t1.Rank < t2.Rank)
                t1 = t1.Reshape(Enumerable.Repeat(1, t2.Rank - t1.Rank).Concat(t1.Shape).ToArray());
            else if (t2.Rank < t1.Rank)
                t2 = t2.Reshape(Enumerable.Repeat(1, t1.Rank - t2.Rank).Concat(t2.Shape).ToArray());                       
            var newDims = new int[t1.Rank];
            for (int i = 0; i < t1.Rank; i++) 
            {
                if (t1.Shape[i] != 1 && t2.Shape[i] != 1 && t2.Shape[i] != t1.Shape[i]) 
                    throw new InvalidOperationException($"Cannot perform element-wise operations on tensors of shapes {t1.Shape} and {t2.Shape}");
                newDims[i] = System.Math.Max(t1.Shape[i], t2.Shape[i]);
            }
            var shape = new Shape(newDims);
            var buffer = new V[shape.ElementsCount];

            foreach (var (it,i) in shape.IterateIndices().Select((_it,_i)=>(_it,_i)))
            {
                var a = t1.GetValueAt(it.Zip(t1.Shape, (x, y) => System.Math.Min(x, y - 1)).ToArray());
                var b = t2.GetValueAt(it.Zip(t2.Shape, (x, y) => System.Math.Min(x, y - 1)).ToArray());                
                buffer[i] = op(a, b);
            }
            return new Tensor<V>(shape, buffer);
        }

        public static Tensor<U> Map<T, U>(this Tensor<T> t1, Func<T, U> op)
        {
            return new Tensor<U>(t1.Shape, t1.Buffer.Select(op).ToArray());
        }


        public static Tensor<V> SubDimBroadcast<T, U, V>(this Tensor<T> t1, Tensor<U> t2, Func<Tensor<T>, Tensor<U>, Tensor<V>> op, int dims)
        {
            if (t1.Rank < t2.Rank)
                t1 = t1.Reshape(Enumerable.Repeat(1, t2.Rank - t1.Rank).Concat(t1.Shape).ToArray());
            else if (t2.Rank < t1.Rank)
                t2 = t2.Reshape(Enumerable.Repeat(1, t1.Rank - t2.Rank).Concat(t2.Shape).ToArray());

            var newDims = new int[t1.Rank - dims];
            for (int i = 0; i < newDims.Length; i++) 
            {
                if (t1.Shape[i] != 1 && t2.Shape[i] != 1 && t2.Shape[i] != t1.Shape[i])
                    throw new InvalidOperationException($"Cannot perform {dims}-dimensional broadcast operations on tensors of shapes {t1.Shape} and {t2.Shape}");
                newDims[i] = System.Math.Max(t1.Shape[i], t2.Shape[i]);
            }
            var iterShape = new Shape(newDims);

            var results = new List<Tensor<V>>();
            Shape resultShape = null;

            foreach (var it in iterShape.IterateIndices()) 
            {
                var ita = it.Select((_, i) => System.Math.Min(_, t1.Shape[i] - 1)).ToArray();
                var itb = it.Select((_, i) => System.Math.Min(_, t2.Shape[i] - 1)).ToArray();
                var a = t1.GetFromBatches(ita);
                var b = t2.GetFromBatches(itb);
                var r = op(a, b);
                if(resultShape==null)
                {
                    results.Add(r);
                    resultShape = r.Shape;
                }
                else
                {
                    if (!r.Shape.Equals(resultShape))
                        throw new InvalidOperationException("SubDimBroadcast operation outputs must have same shape");
                    results.Add(r);
                }                
            }

            resultShape = new Shape(iterShape.Concat(resultShape).ToArray());
            var buffer = results.SelectMany(_ => _.Buffer).ToArray();
            return new Tensor<V>(resultShape, buffer);
        }

        public static Tensor<U> SubDimMap<T, U>(this Tensor<T> t1, Func<Tensor<T>, Tensor<U>> op, int dims)
        {
            if (t1.Rank == dims)
                return op(t1);

            var newDims = t1.Shape.Take(t1.Rank - dims).ToArray();            
            var iterShape = new Shape(newDims);            

            var results = new List<Tensor<U>>();
            Shape resultShape = null;

            foreach (var it in iterShape.IterateIndices())
            {
                var ita = it.Select((_, i) => System.Math.Min(_, t1.Shape[i] - 1)).ToArray();                
                var a = t1.GetFromBatches(ita);                
                var r = op(a);
                if (resultShape == null)
                {
                    results.Add(r);
                    resultShape = r.Shape;
                }
                else
                {
                    if (!r.Shape.Equals(resultShape))
                        throw new InvalidOperationException("SubDimMap operation outputs must have same shape");
                    results.Add(r);
                }
            }
            resultShape = new Shape(iterShape.Concat(resultShape).ToArray());
            var buffer = results.SelectMany(_ => _.Buffer).ToArray();
            return new Tensor<U>(resultShape, buffer);
        }

        public static Tensor<T>[] Unstack<T>(this Tensor<T> t, int axis = 0)
        {
            axis = Shape.ResolveIndex(t.Rank, axis);
            var accessors = new ISequenceAccessor[t.Rank];

            var tensors = new Tensor<T>[t.Shape[axis]];
            for(int i = 0; i < tensors.Length; i++)
            {
                accessors[axis] = new IndexAccessor(i);
                tensors[i] = t[accessors];
            }
            return tensors;
        }

        private static Tensor<T> ReduceAxis<T>(this Tensor<T> t, Func<T, T, T> op, int axis, bool keepDimensions)
        {
            axis = Shape.ResolveIndex(t.Rank, axis);
            return t.SubDimMap(x =>
            {
                var tensors = x.Unstack(axis: 0);
                var r = tensors[0];                
                for (int i = 1; i < tensors.Length; i++)
                    r = r.PerformElementWiseBinaryOperation(tensors[i], op);
                if (keepDimensions)
                    r = r.Reshape(1 + r.Shape);
                return r;
            }, t.Rank - axis);
        }

        public static Tensor<T> ReduceAxis<T>(this Tensor<T> t, Func<T, T, T> op, AxisCollection axis = null, bool keepDimensions = false)
        {
            axis = axis ?? AxisCollection.AllAxis;
            var axisArr = axis.Resolve(t.Rank);
            for (int i = axisArr.Length - 1; i >= 0; i--)
                t = t.ReduceAxis(op, axisArr[i], keepDimensions);

            return t;
        }  

        public static Tensor<T> Squeeze<T>(this Tensor<T> t)
        {
            return new Tensor<T>(t.Shape.Where(d => d != 1).ToArray(), t.Buffer);
        }

        public static unsafe Tensor<float> Conv2D(this Tensor<float> t, Tensor<float> kernel)
        {
            var validate_shapes = new Func<bool>(() => t.Rank == 4 && kernel.Rank == 4 && t.Shape[3] == kernel.Shape[2]);
            if (!validate_shapes())
                throw new ArgumentException($"Invalid input shapes for Conv2D operation: {t.Shape}, {kernel.Shape}");

            (int B, int n, int m) = (t.Shape[0], t.Shape[1], t.Shape[2]);
            (int K1, int K2) = (kernel.Shape[0], kernel.Shape[1]);
            (int C, int F) = (t.Shape[3], kernel.Shape[3]);

            var result = new float[B * n * m * F];

            fixed (float* tbuff = &t.Buffer.Buffer[0])
            fixed (float* kbuff = &kernel.Buffer.Buffer[0])
            fixed (float* rbuff = &result[0])
            {
                if (PlatformConfig.Conv2DMethod == Conv2DMethod.Img2Col)
                    Img2Col.Conv2D(tbuff, kbuff, rbuff, B, n, m, C, K1, K2, F);
                else
                    UnsafeOperations.Conv2D(tbuff, kbuff, rbuff, B, n, m, C, K1, K2, F);

            }

            return new Tensor<float>((B, n, m, F), result);            
        }        

        public static unsafe Tensor<float> Conv2DTransposeFloat32(this Tensor<float> t, Tensor<float> kernel, (int Rows, int Cols)? strides = null)
        {
            (var strideRows, var strideCols) = strides ?? (1, 1);
            var validate_shapes = new Func<bool>(() => t.Rank == 4 && kernel.Rank == 4 && t.Shape[3] == kernel.Shape[3]);

            if (!validate_shapes())
                throw new ArgumentException($"Invalid input shapes for Conv2DTranspose operation: {t.Shape}, {kernel.Shape}");

            (int B, int n, int m) = (t.Shape[0], t.Shape[1], t.Shape[2]);
            (int K1, int K2) = (kernel.Shape[0], kernel.Shape[1]);
            (int C, int F) = (kernel.Shape[3], kernel.Shape[2]);

            var outH = strideRows * n;
            var outW = strideCols * m;

            var result = new float[B * outH * outW * F];

            fixed (float* tbuff = &t.Buffer.Buffer[0])
            fixed (float* kbuff = &kernel.Buffer.Buffer[0])
            fixed (float* rbuff = &result[0])
                UnsafeOperations.Conv2DTranspose(tbuff, kbuff, rbuff, B, n, m, C, K1, K2, F, strideRows, strideCols);

            return new Tensor<float>((B, outH, outW, F), result);            
        }


        public static IEnumerable<Shape> SelectShapes<T>(this IEnumerable<Tensor<T>> tensors) => tensors.Select(_ => _.Shape);
    }   
}
