using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LillyScan.Backend.AI.Math
{
    public static partial class TensorExtensions
    {
        public static Tensor GetFromBatches(this Tensor t, int[] indices)
        {
            if (indices.Length >= t.Rank)
                throw new ArgumentException("Invalid batch: too many indices");

            var shape = new Shape(t.Shape.Skip(indices.Length).ToArray());
            var buffStartIndex = t.Shape.GetBufferIndex(indices.Concat(new int[t.Rank - indices.Length]).ToArray());
            var buffer = new float[shape.ElementsCount];
            t.BuffAccessor.CopyTo(buffStartIndex, buffer, 0, buffer.Length);
            return new Tensor(shape, buffer);
        }

        public static Tensor SubDimBroadcast(this Tensor t1, Tensor t2, Func<Tensor, Tensor, Tensor> op, int dims)
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

            var results = new List<Tensor>();
            Shape? resultShape = null;

            foreach (var it in iterShape.IterateIndices())
            {
                var ita = it.Select((_, i) => System.Math.Min(_, t1.Shape[i] - 1)).ToArray();
                var itb = it.Select((_, i) => System.Math.Min(_, t2.Shape[i] - 1)).ToArray();
                var a = t1.GetFromBatches(ita);
                var b = t2.GetFromBatches(itb);
                var r = op(a, b);
                if (resultShape == null)
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
            var buffer = results.SelectMany(_ => _.BuffAccessor.GetSlice(0, _.ElementsCount)).ToArray();
            return new Tensor(resultShape.Value, buffer);
        }

        public static Tensor SubDimMap(this Tensor t1, Func<Tensor, Tensor> op, int dims)
        {
            if (t1.Rank == dims)
                return op(t1);

            var newDims = t1.Shape.Take(t1.Rank - dims).ToArray();
            var iterShape = new Shape(newDims);

            var results = new List<Tensor>();
            Shape? resultShape = null;

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
            var buffer = results.SelectMany(_ => _.BuffAccessor.GetSlice(0, _.ElementsCount)).ToArray();
            return new Tensor(resultShape.Value, buffer);
        }
    }
}
