﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace LillyScan.Backend.Utils
{
    public class ImmutableArray<T> : IEnumerable<T>
    {
        private readonly T[] Buffer;        

        public ImmutableArray(params T[] buffer)
        {
            if (buffer == null)
                throw new ArgumentNullException();
            Buffer = buffer.ToArray();
        }

        public ImmutableArray(IEnumerable<T> buffer)
        {
            if (buffer == null)
                throw new ArgumentNullException();
            Buffer = buffer.ToArray();
        }

        public T this[int index]
        {
            get
            {
                if (index < 0 || index >= Buffer.Length)
                    throw new IndexOutOfRangeException($"Accessing index {index} of array with length  {Buffer.Length}");
                return Buffer[index];
            }
        }

        public int Length => Buffer.Length;

        public IEnumerator<T> GetEnumerator() => ((IEnumerable<T>)Buffer).GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public override string ToString() => $"[{Buffer.JoinToString(",")}]";

        public void CopyTo(int sourceIndex, T[] destinationArray, int destinationIndex, int length)
        {
            Array.Copy(Buffer, sourceIndex, destinationArray, destinationIndex, length);
        }

        public override bool Equals(object obj)
        {
            return obj is ImmutableArray<T> array &&
                   Length == array.Length &&
                   Buffer.SequenceEqual(array.Buffer);                   
        }

        public override int GetHashCode()
        {
            int hashCode = 1896606984;
            hashCode = hashCode * -1521134295 + Buffer.Select(_ => _.GetHashCode()).Aggregate(0, (x, y) => unchecked(x + y));
            return hashCode;
        }
    }
}
