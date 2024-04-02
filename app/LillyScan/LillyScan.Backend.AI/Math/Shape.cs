using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace LillyScan.Backend.AI.Math
{
    public readonly struct Shape : IEnumerable<int>
    {
        private readonly int[] pDimensions;
        internal readonly int[] DimMultipliers;

        public readonly int DimensionsCount;
        public readonly int ElementsCount;

        public Shape(int[] dims)
        {
            pDimensions = dims ?? throw new ArgumentNullException(nameof(dims));
            DimensionsCount = dims.Length;
            var elemsCount = 1;
            for (int i = 0; i < dims.Length; i++)
                elemsCount *= dims[i];
            ElementsCount = elemsCount;

            DimMultipliers = new int[DimensionsCount];
            int m = 1;
            for (int i = dims.Length - 1; i >= 0; i--)
            {
                DimMultipliers[i] = m;
                m *= dims[i];
            }
        }

        public int this[int axis] => pDimensions[SolveIndex(DimensionsCount, axis)];

        public static int SolveIndex(int rank, int index)
        {
#if DEBUG
            if (index < -rank || index >= rank)
                throw new IndexOutOfRangeException($"Cannot access dimension {index} of shape with rank {rank}");
#endif                        
            return index < 0 ? rank + index : index;
        }

        public override string ToString() => $"[{pDimensions.JoinToString(", ")}]";


        public static implicit operator Shape(int[] values) => new Shape(values);
        public static implicit operator Shape(int value) => new Shape(new[] { value });
        public static implicit operator Shape((int, int) values) => new Shape(new[] { values.Item1, values.Item2 });
        public static implicit operator Shape((int, int, int) values) => new Shape(new[] { values.Item1, values.Item2, values.Item3 });
        public static implicit operator Shape((int, int, int, int) values) => new Shape(new[] { values.Item1, values.Item2, values.Item3, values.Item4 });

        public static Shape operator +(Shape s1, Shape s2) => new Shape(s1.pDimensions.Concat(s2.pDimensions).ToArray());
        public static Shape Ones(int count) => new Shape(Enumerable.Repeat(1, count).ToArray());

        public void IterateIndicesWithBufferCounter(Action<int[], int> action)
        {
            var iter = new int[DimensionsCount];
            int k = 0;
            while (iter[0] < this[0])
            {
                action(iter, k++);                
                for (int i = DimensionsCount - 1, c = 1; i >= 0 && c > 0; i--)
                {
                    iter[i]++;
                    c = 0;
                    if (i > 0 && iter[i] == this[i])
                    {
                        iter[i] = 0;
                        c = 1;
                    }
                }
            }
        }

        public IEnumerable<(int[], int)> IterateIndicesWithBufferCounter()
        {
            var iter = new int[DimensionsCount];
            int k = 0;
            while (iter[0] < this[0])
            {
                yield return (iter, k++);
                for (int i = DimensionsCount - 1, c = 1; i >= 0 && c > 0; i--)
                {
                    iter[i]++;
                    c = 0;
                    if (i > 0 && iter[i] == this[i])
                    {
                        iter[i] = 0;
                        c = 1;
                    }
                }
            }
        }

        public IEnumerable<int[]> IterateIndices()
        {
            var iter = new int[DimensionsCount];
            while (iter[0] < this[0])
            {
                yield return iter; // new ImmutableArray<int>(iter);
                for (int i = DimensionsCount - 1, c = 1; i >= 0 && c > 0; i--)
                {
                    iter[i]++;
                    c = 0;
                    if (i > 0 && iter[i] == this[i])
                    {
                        iter[i] = 0;
                        c = 1;
                    }
                }
            }
        }

        public int GetBufferIndex(params int[] indices)
        {
            int result = 0;
            for (int i = 0; i < DimensionsCount; i++)
                result += DimMultipliers[i] * indices[i];       
            return result;
        }

        public override bool Equals(object obj)
        {
            return obj is Shape shape && pDimensions.SequenceEqual(shape.pDimensions);
        }

        public override int GetHashCode()
        {
            return this.pDimensions.Select(_ => _.GetHashCode()).Aggregate(0, (x, y) => unchecked(x + y));
        }

        public IEnumerator<int> GetEnumerator() => ((IEnumerable<int>)pDimensions).GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
