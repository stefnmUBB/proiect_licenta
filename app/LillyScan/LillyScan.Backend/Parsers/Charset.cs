using System.Collections.Generic;
using System.Linq;

namespace LillyScan.Backend.Parsers
{
    public sealed class Charset
    {
        private static IComparer<CharsRange> Comparer = new CharsRange.DefaultComparer();

        private readonly CharsRange[] Ranges;
        private readonly int HashCode;
        private readonly int pCount;

        public Charset(params CharsRange[] ranges)
        {
            ranges = ranges.OrderBy(_ => _, Comparer).ToArray();
            var tmpRanges = new List<CharsRange>();
            var tmpRange = CharsRange.Empty();

            for (int i = 0; i < ranges.Length; i++)
            {
                if (ranges[i].Length == 0) continue;

                if (CharsRange.TryUnion(tmpRange, ranges[i], out var union))
                {
                    tmpRange = union;
                }
                else
                {
                    tmpRanges.Add(tmpRange);
                    tmpRange = ranges[i];
                }
            }
            tmpRanges.Add(tmpRange);
            Ranges = tmpRanges.ToArray();

            HashCode = Ranges.GetSequenceHashCodeSum();
            pCount = Ranges.Sum(_ => _.Length);
        }

        public bool Contains(char c) => Ranges.Any(_ => _.Contains(c));

        public Charset Except(Charset c)
        {
            if (Ranges.Length == 0) return new Charset();
            if (c.Ranges.Length == 0) return new Charset(Ranges);

            var tmpRanges = new List<CharsRange>();
            var difIndex = 0;

            foreach (var range in Ranges)
            {
                while (difIndex < c.Ranges.Length && c.Ranges[difIndex].End < range.Start)
                    difIndex++;
                var rgs = new[] { range };
                for (int i = difIndex; i < c.Ranges.Length; i++)
                {
                    if (c.Ranges[i].Start > range.End) break;
                    rgs = rgs.SelectMany(_ => CharsRange.Except(_, c.Ranges[i])).ToArray();
                }
                tmpRanges.AddRange(rgs);
            }

            return new Charset(tmpRanges.ToArray());
        }

        public bool IsEmpty => pCount == 0;
        public int Count => pCount;


        public Charset IntersectWith(Charset c)
        {
            var tmpRanges = new List<CharsRange>();

            foreach (var range0 in Ranges)
            {
                foreach (var range1 in c.Ranges)
                {
                    var i = CharsRange.Intersect(range0, range1);
                    if (i.Length > 0)
                    {
                        tmpRanges.Add(i);
                    }
                }
            }
            return new Charset(tmpRanges.ToArray());
        }

        public static Charset operator -(Charset c1, Charset c2) => c1.Except(c2);
        public static Charset operator -(Charset c1, CharsRange r) => c1.Except(new Charset(r));
        public static Charset operator -(Charset c1, char c) => c1.Except(new Charset(new CharsRange(c)));

        public static Charset operator +(Charset c1, Charset c2) => new Charset(c1.Ranges.Concat(c2.Ranges).ToArray());
        public static Charset operator +(Charset c1, CharsRange r) => new Charset(c1.Ranges.Append(r).ToArray());
        public static Charset operator +(Charset c1, char c) => c1 + new CharsRange(c);

        public static Charset operator |(Charset c1, Charset c2) => new Charset(c1.Ranges.Concat(c2.Ranges).ToArray());
        public static Charset operator |(Charset c1, CharsRange r) => new Charset(c1.Ranges.Append(r).ToArray());
        public static Charset operator |(Charset c1, char c) => c1 + new CharsRange(c);

        public static Charset operator &(Charset c1, Charset c2) => c1.IntersectWith(c2);
        public static Charset operator &(Charset c1, CharsRange r) => c1.IntersectWith(new Charset(r));
        public static Charset operator &(Charset c1, char c) => c1.IntersectWith(new Charset(new CharsRange(c)));

        public override string ToString() => Ranges.JoinToString(", ", "{", "}");
        public override bool Equals(object obj) => obj is Charset charset && Ranges.SequenceEqual(charset.Ranges);
        public override int GetHashCode() => HashCode;

        public Charset Complementary() => Charsets.All - this;
    }
}
