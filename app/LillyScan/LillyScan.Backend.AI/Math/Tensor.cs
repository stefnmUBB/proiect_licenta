using LillyScan.Backend.AI.Memory;
using System;
using System.Collections.Generic;
using System.Linq;

namespace LillyScan.Backend.AI.Math
{
    public readonly unsafe struct Tensor : IDisposable
    {
        public readonly Shape Shape;
        internal readonly BufferAccessor BuffAccessor;
        public readonly int ElementsCount;
        public readonly int Rank;
        public bool IsScalar => Rank == 0;

        private Tensor(Shape shape, BufferAccessor buffAccessor)
        {
            Shape = shape;
            BuffAccessor = buffAccessor;
            Rank = shape.DimensionsCount;            
            ElementsCount = Rank == 0 ? 1 : shape.ElementsCount;             
        }

        public Tensor this[params ISequenceAccessor[] acc] => GetSlices(acc);

        public Tensor GetSlices(params ISequenceAccessor[] acc)
        {
            if (acc.Length > Shape.DimensionsCount)
                throw new ArgumentException("More slices than dimensions");

            acc = acc.Select(_ => _ ?? new Slice(null, null, null)).Concat(Enumerable.Repeat(new Slice(null, null, null), Shape.DimensionsCount - acc.Length)).ToArray();

            var newShape = new List<int>();

            var ixList = new List<int[]>();
            for (int i = 0; i < acc.Length; i++)
            {
                var ixs = acc[i].GetIndices(Shape[i]);
                ixList.Add(ixs);
                if (!acc[i].DimReduce)
                    newShape.Add(ixs.Length);
            }

            var iter = new int[Shape.DimensionsCount];

            var buffer = new List<float>();

            while (iter[0] < ixList[0].Length)
            {
                var elem = BuffAccessor[Shape.GetBufferIndex(iter.Select((x, i) => ixList[i][x]).ToArray())];
                buffer.Add(elem);

                for (int i = Shape.DimensionsCount - 1, c = 1; i >= 0 && c > 0; i--)
                {
                    iter[i]++;
                    c = 0;
                    if (i > 0 && iter[i] == ixList[i].Length)
                    {
                        iter[i] = 0;
                        c = 1;
                    }
                }
            }

            return new Tensor(newShape.ToArray(), buffer.ToArray());
        }

        internal Tensor(Shape shape, BufferAccessor buffAccessor, int offset)
            : this(shape, BuffersPool.AccessBuffer(buffAccessor.Buffer, offset)) { }        

        public Tensor(Shape shape, float[] elements)
        {
            Shape = shape;
            Rank = shape.DimensionsCount;
            ElementsCount = Rank==0 ? 1: shape.ElementsCount;
            if (elements.Length != ElementsCount)
                throw new ArgumentException($"Cannot create tensor of shape {shape} from a buffer of length {elements.Length}");
            var buffer = BuffersPool.CreateFromArray(elements);
            BuffAccessor = BuffersPool.AccessBuffer(buffer);            
        }

        public float GetValueAt(params int[] indices) => BuffAccessor[Shape.GetBufferIndex(indices)];

        public void Dispose()
        {
            BuffersPool.ReleaseBufferAccess(BuffAccessor.Buffer);
        }

        public override string ToString()
        {
            if (ElementsCount <= 7) return $"Tensor of shape {Shape}: {{{BuffAccessor.GetSlice(0, ElementsCount).JoinToString(", ")}}}";
            return $"Tensor of shape {Shape}: {{" +
                $"{BuffAccessor.GetSlice(0, 3).JoinToString(", ")},...," +
                $"{BuffAccessor.GetSlice(ElementsCount - 3, 3).JoinToString(", ")}}}";
        }
    }
}
