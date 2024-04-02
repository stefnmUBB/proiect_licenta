using System.Collections.Generic;

namespace LillyScan.Backend.AI
{
    internal static class Extensions
    {
        public static string JoinToString<T>(this IEnumerable<T> items, string delim = " ")
            => string.Join(delim, items);

        public static float Clamp(this float x, float a, float b) => x <= a ? a : x >= b ? b : x;
        public static int Clamp(this int x, int a, int b) => x <= a ? a : x >= b ? b : x;
    }
}
