using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace LillyScan.Backend.Parsers
{
    public struct CharsRange
    {
        public char Start { get; }
        public char End { get; }
        public int Length { get; }

        public CharsRange(char start, char end)
        {
            Start = start;
            End = end;
            Length = System.Math.Max(0, End - Start + 1);
        }

        public CharsRange(char singleCharacter) : this(singleCharacter, singleCharacter) { }

        private static string CharToString(char c)
        {
            var code = $"\\x{(int)c:X4}";
            if (c < 128 && !char.IsControl(c))
            {
                switch (c)
                {
                    case '\'': return code + " ('\\'')";
                    case '\\': return code + " ('\\\\')";
                    case '\n': return code + " ('\\n')";
                    case '\r': return code + " ('\\r')";
                    case '\t': return code + " ('\\t')";
                    default: break;
                }
                return code + $" ('{c}')";
            }
            return code;
        }

        public override string ToString()
        {
            if (Length == 0) return "[]";
            if (Length == 1) return $"[{CharToString(Start)}]";
            return $"[{CharToString(Start)}, {CharToString(End)}]";
        }

        public bool Contains(char c) => Start <= c && c <= End;

        public static CharsRange Empty() => new CharsRange('\u0001', '\u0000');

        public static CharsRange Intersect(CharsRange r1, CharsRange r2)
        {
            if (r1.Length == 0 || r2.Length == 0) return Empty();
            if (r1.End < r2.Start || r2.End < r1.Start) return Empty();
            return new CharsRange((char)System.Math.Max(r1.Start, r2.Start), (char)System.Math.Min(r1.End, r2.End));
        }

        public static bool TryUnion(CharsRange r1, CharsRange r2, out CharsRange result)
        {
            if (r1.Length == 0)
            {
                result = r2;
                return true;
            }
            if (r2.Length == 0)
            {
                result = r1;
                return true;
            }
            if (Intersect(r1, r2).Length == 0)
            {
                result = Empty();
                return false;
            }
            result = new CharsRange((char)System.Math.Min(r1.Start, r2.Start), (char)System.Math.Max(r1.End, r2.End));
            return true;
        }

        public static CharsRange[] Except(CharsRange r1, CharsRange r2)
        {
            var intersection = Intersect(r1, r2);
            if (intersection.Length == 0) return new[] { r1 };
            var left = new CharsRange(r1.Start, (char)(intersection.Start - 1));
            var right = new CharsRange((char)(intersection.End + 1), r1.End);
            return new[] { left, right }.Where(_ => _.Length > 0).ToArray();
        }

        public override bool Equals(object obj)
        {
            return obj is CharsRange range && Start == range.Start && End == range.End;
        }

        public override int GetHashCode()
        {
            int hashCode = -1676728671;
            hashCode = hashCode * -1521134295 + Start.GetHashCode();
            hashCode = hashCode * -1521134295 + End.GetHashCode();
            return hashCode;
        }

        internal class DefaultComparer : IComparer<CharsRange>
        {
            public int Compare(CharsRange x, CharsRange y) => x.Start - y.Start;
        }
    }
}
