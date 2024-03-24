using System;

namespace LillyScan.Backend.Utils
{
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
}
