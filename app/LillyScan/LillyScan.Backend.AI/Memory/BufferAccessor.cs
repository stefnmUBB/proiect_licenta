using System;
using System.Collections.Generic;
using System.Text;

namespace LillyScan.Backend.AI.Memory
{
    internal readonly struct BufferAccessor
    {
        internal readonly Buffer Buffer;
        internal readonly int Offset;
        internal BufferAccessor(Buffer buffer, int offset)
        {
            Buffer = buffer;
            Offset = offset;
        }
        internal float this[int index] => Buffer[Offset + index];

        internal float[] GetSlice(int start,  int count)
        {
            var slice = new float[count];
            for (int i = 0; i < count; i++) slice[i] = Buffer[Offset + start + i];
            return slice;
        }
        
        public void CopyTo(int startIndex, float[] dest, int destIndex, int length)
        {
            for(int i=0;i<length;i++)
            {
                dest[destIndex + i] = Buffer[Offset + startIndex + i];
            }
        }
    }
}
