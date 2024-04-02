using System;
using System.Collections.Generic;
using System.Text;

namespace LillyScan.Backend.AI.Memory
{
    internal unsafe readonly struct Buffer
    {
        internal readonly float* Pointer;
        internal readonly int Size;

        internal Buffer(float* pointer, int size)
        {
            Pointer = pointer;
            Size = size;            
        }

        internal float this[int index] => Pointer[index];

        public override bool Equals(object obj)
        {
            return obj is Buffer buffer &&
                Pointer - buffer.Pointer == 0 &&
                Size == buffer.Size;
        }

        public override int GetHashCode()
        {
            int hashCode = -2143676970;
            hashCode = hashCode * -1521134295 + ((int)Pointer).GetHashCode();
            hashCode = hashCode * -1521134295 + Size.GetHashCode();
            return hashCode;
        }

        public override unsafe string ToString() => $"Buffer {new IntPtr(Pointer)} of length {Size}";
    }
}
