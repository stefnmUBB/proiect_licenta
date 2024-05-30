using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LillyScan.Backend.Utils
{
    public static class Extensions
    {
        public static float Clamp(this float x, float a, float b) => x <= a ? a : x >= b ? b : x;
        public static double Clamp(this double x, double a, double b) => x <= a ? a : x >= b ? b : x;
        public static int Clamp(this int x, int a, int b) => x <= a ? a : x >= b ? b : x;
        public static bool IsBetween(this int x, int a, int b) => a <= x && x <= b;
        public static bool IsBetween(this double x, double a, double b) => a <= x && x <= b;

        public static int Squared(this int x) => x * x;


        public static IEnumerable<T[]> GroupChunks<T>(this IEnumerable<T> items, int chunkSize) =>
            items.Select((x, i) => (x, c: i / chunkSize)).GroupBy(_ => _.c).Select(g => g.Select(_ => _.x).ToArray());        

        public static string JoinToString<T>(this IEnumerable<T> items, string delimiter)
            => string.Join(delimiter, items.Select(_ => _.ToString()));

        public static int ArgMax<T>(this IEnumerable<T> sequence) where T : IComparable<T>
        {
            int maxIndex = -1;
            T maxValue = default(T);

            int index = 0;
            foreach (T value in sequence)
            {
                if (value.CompareTo(maxValue) > 0 || maxIndex == -1)
                {
                    maxIndex = index;
                    maxValue = value;
                }
                index++;
            }
            return maxIndex;
        }

        public static int ArgMin<T>(this IEnumerable<T> sequence) where T : IComparable<T>
        {
            int maxIndex = -1;
            T maxValue = default(T);

            int index = 0;
            foreach (T value in sequence)
            {
                if (value.CompareTo(maxValue) < 0 || maxIndex == -1) 
                {
                    maxIndex = index;
                    maxValue = value;
                }
                index++;
            }
            return maxIndex;
        }

        public static int ArgMin<T, U>(this IEnumerable<T> sequence, Func<T,U> selector) where U : IComparable<U>
        {
            int minIndex = -1;
            U minValue = default(U);

            int index = 0;
            foreach (T value in sequence)
            {
                var s = selector(value);
                if (s.CompareTo(minValue) < 0 || minIndex == -1) 
                {
                    minIndex = index;
                    minValue = s;
                }
                index++;
            }
            return minIndex;
        }

        public static void ForEach<T>(this IEnumerable<T> items, Action<T> action)
        {
            foreach (var item in items)
            {
                action(item);
            }
        }

        public static IEnumerable<T> WhereNot<T>(this IEnumerable<T> items, Predicate<T> predicate)
        {
            foreach (var item in items)
                if (!predicate(item))
                    yield return item;
        }

        public static IEnumerable<T> Peek<T>(this IEnumerable<T> items, Action<T> action)
        {
            foreach (var item in items)
            {
                action(item);
                yield return item;
            }
        }

        public static void DeepPrint(this object o)
        {
            if (o is Dictionary<string, object> d)
            {
                foreach (var kv in d)
                {
                    Console.Write($"{{{kv.Key}: ");
                    DeepPrint(kv.Value);
                    Console.WriteLine($"}}");
                }
                return;
            }
            if (o is object[] a)
            {
                Console.Write("[");
                foreach (var x in a)
                {
                    DeepPrint(x);
                    Console.Write(",");
                }
                Console.Write("]");
                return;
            }
            Console.Write(o ?? "None");
        }

        public static (U min, U max) MinAndMax<T,U>(this IEnumerable<T> values, Func<T,U> selector)
        {
            using(var enumerator =  values.GetEnumerator())
            {
                enumerator.MoveNext();
                U current = selector(enumerator.Current);
                U min = current, max = current;
                var comparer = Comparer<U>.Default;
                while (enumerator.MoveNext()) 
                {
                    current = selector(enumerator.Current);
                    if (comparer.Compare(current, min) < 0)
                        min = current;
                    if (comparer.Compare(max, current) < 0)
                        max = current;
                }
                return (min, max);
            }
        }

        public static (T min, T max) MinAndMax<T>(this IEnumerable<T> values)
        {
            using (var enumerator = values.GetEnumerator())
            {
                enumerator.MoveNext();
                T current = enumerator.Current;
                T min = current, max = current;
                var comparer = Comparer<T>.Default;
                while (enumerator.MoveNext())
                {
                    current = enumerator.Current;
                    if (comparer.Compare(current, min) < 0)
                        min = current;
                    if (comparer.Compare(max, current) < 0)
                        max = current;
                }
                return (min, max);
            }
        }
    }
}
