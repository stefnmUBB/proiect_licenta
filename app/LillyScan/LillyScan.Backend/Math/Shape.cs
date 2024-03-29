using LillyScan.Backend.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using System.Text;

namespace LillyScan.Backend.Math
{
    public class Shape : ImmutableArray<int>
    {
        public int ElementsCount { get; }
        public int DimsCount { get; }
        
        public Shape(params int[] buffer) : base(buffer)
        {
            ElementsCount = buffer.Length == 0 ? 0 : buffer.Aggregate(1, (x, y) => x * y);
            DimsCount = buffer.Length;
        }

        public static int ResolveIndex(int rank, int index)
        {
            if (index < -rank || index>= rank) 
                throw new IndexOutOfRangeException($"Cannot access dimension {index} of shape with rank {rank}");
            return index < 0 ? rank + index : index;            
        }

        public new int this[int index] => base[ResolveIndex(DimsCount, index)];

        public Shape(IEnumerable<int> dims) : this(dims.ToArray()) { }

        public static implicit operator Shape(int v) => new Shape(v); 
        public static implicit operator Shape(ValueTuple<int> vt) => new Shape(vt.Item1); 
        public static implicit operator Shape(ValueTuple<int,int> vt) => new Shape(vt.Item1, vt.Item2); 
        public static implicit operator Shape(ValueTuple<int,int,int> vt) => new Shape(vt.Item1, vt.Item2, vt.Item3);
        public static implicit operator Shape(ValueTuple<int, int, int, int> vt) => new Shape(vt.Item1, vt.Item2, vt.Item3, vt.Item4);
        public static implicit operator Shape(int[] arr) => new Shape(arr);

        public override string ToString() => base.ToString();


        public IEnumerable<ImmutableArray<int>> IterateIndices()
        {
            var iter = new int[DimsCount];
            while (iter[0] < this[0])
            {
                yield return new ImmutableArray<int>(iter);
                for (int i = DimsCount - 1, c = 1; i >= 0 && c > 0; i--) 
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

        public IEnumerable<ImmutableArray<int>> IterateSubDimsIndices(int subDims)
        {
            if (subDims >= DimsCount)
                throw new InvalidOperationException("Too many dimensions");
            return new Shape(this.Take(DimsCount - subDims)).IterateIndices();
        }

        public int GetBufferIndex(int[] indices)
        {
            int p = 1, result = 0;
            for (int i = DimsCount - 1; i >= 0; i--)
            {
                result += p * indices[i];
                p *= this[i];
            }
            return result;
        }

        public int GetBufferIndex(ImmutableArray<int> indices)
        {
            int p = 1, result = 0;
            for (int i = DimsCount - 1; i >= 0; i--)
            {
                result += p * indices[i];
                p *= this[i];
            }
            return result;
        }        

        public int GetBufferIndex(int[] indices, int[] perm)
        {
            int p = 1, result = 0;
            for (int i = DimsCount - 1; i >= 0; i--) 
            {
                result += p * indices[perm[i]];
                p *= this[perm[i]];
            }
            return result;
        }       

        public int GetBufferIndex(ImmutableArray<int> indices, int[] perm)
        {
            int p = 1, result = 0;
            for (int i = DimsCount - 1; i >= 0; i--)
            {
                result += p * indices[perm[i]];
                p *= this[perm[i]];
            }
            return result;
        }

        public override bool Equals(object obj)
        {
            return obj is Shape shape && this.SequenceEqual(shape);
        }

        public override int GetHashCode()
        {
            return this.Select(_ => _.GetHashCode()).Aggregate(0, (x, y) => unchecked(x + y));
        }

        public Shape AsPlaceholder(int axis) => new Shape(this.Select((d, i) => i == ResolveIndex(DimsCount, axis) ? d : -1));

        public bool MatchesPlaceholder(Shape placeholder)
        {            
            if (placeholder.Length != this.Length)
                return false;
            return placeholder.Zip(this, (p, r) =>
            {
                if (p == -1) return true;
                return p == r;
            }).All(_ => _);
        }

    }
}
