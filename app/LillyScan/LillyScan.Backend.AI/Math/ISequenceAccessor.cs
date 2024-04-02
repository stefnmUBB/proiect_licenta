﻿using System;
using System.Collections.Generic;
using System.Text;

namespace LillyScan.Backend.AI.Math
{
    public interface ISequenceAccessor
    {
        bool DimReduce { get; }
        int[] GetIndices(int length);
    }

    public class IndexAccessor : ISequenceAccessor
    {
        public bool DimReduce => true;

        public int Index { get; }

        public IndexAccessor(int index)
        {
            Index = index;
        }

        public static implicit operator IndexAccessor(int index) => new IndexAccessor(index);

        public int[] GetIndices(int length)
        {
            if (Index < -length || Index >= length)
                throw new IndexOutOfRangeException($"Accessing element {Index} of an array with {length} elements");

            return new int[] { Index < 0 ? length - Index : Index };
        }
    }

    public class Slice : ISequenceAccessor
    {
        public bool DimReduce => false;

        private int? Start, End, Stride;

        public Slice(int? start, int? end, int? stride = 1)
        {
            (Start, End, Stride) = (start, end, stride);
        }

        public static implicit operator Slice(ValueTuple<int, int> vt) => new Slice(vt.Item1, vt.Item2);
        public static implicit operator Slice(ValueTuple<int, int, int> vt) => new Slice(vt.Item1, vt.Item2, vt.Item3);

        public int[] GetIndices(int length)
        {
            var indices = new List<int>();

            int stride = Stride.GetValueOrDefault(1);

            if (!Start.HasValue && !End.HasValue && stride < 0)
            {
                for (int i = length; i >= 0; i += stride)
                    indices.Add(i);
            }
            else
            {
                int start = Start.GetValueOrDefault(0);
                int end = End.GetValueOrDefault(length);

                int index = start;

                if (stride > 0)
                {
                    while (index < end)
                    {
                        indices.Add(index);
                        index += stride;
                    }
                }
                else
                {
                    while (index > end)
                    {
                        indices.Add(index);
                        index += stride;
                    }
                }
            }

            return indices.ToArray();
        }
    }
}
