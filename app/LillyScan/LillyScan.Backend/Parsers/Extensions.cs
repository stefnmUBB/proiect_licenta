using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LillyScan.Backend.Parsers
{
    internal static class Extensions
    {
        public static string JoinToString<T>(this IEnumerable<T> values, string separator, string left = "", string right = "")
            => left + string.Join(separator, values) + right;

        public static int GetSequenceHashCodeSum<T>(this IEnumerable<T> values)
        {
            return values.Select(_ => { return _.GetHashCode(); }).Aggregate(0, (x, y) => unchecked(x + y));
        }


        public static IEnumerable<ValueTuple<T, T>> MakePairs<T>(this T[] values, bool includeIdentical = false)
        {
            for (int i = 0; i < values.Length; i++)
                for (int j = includeIdentical ? i : i + 1; j < values.Length; j++)
                    yield return (values[i], values[j]);
        }

        public static IEnumerable<ValueTuple<T, U>> PairWith<T, U>(this IEnumerable<T> tValues, IEnumerable<U> uValues)
        {
            foreach (var tVal in tValues)
                foreach (var uVal in uValues)
                    yield return (tVal, uVal);
        }


        public static V GetOrCreate<K, V>(this IDictionary<K, V> dict, K key, Func<K, V> creator)
            => dict.TryGetValue(key, out var result) ? result : (dict[key] = creator(key));

        public static V GetOrDefault<K, V>(this IDictionary<K, V> dict, K key, Func<K, V> creator)
            => dict.TryGetValue(key, out var result) ? result : creator(key);

        public static V GetOrCreate<K, V>(this IDictionary<K, V> dict, K key) where V : new()
            => dict.TryGetValue(key, out var result) ? result : (dict[key] = new V());

        public static V GetOrDefault<K, V>(this IDictionary<K, V> dict, K key) where V : new()
            => dict.TryGetValue(key, out var result) ? result : new V();

        public static IEnumerable<V> MapIndexable<K, V>(this IEnumerable<K> keys, IDictionary<K, V> dict)
            => keys.Select(_ => dict[_]);

        public static IEnumerable<V> SelectValues<K, V>(this IDictionary<K, V> dict, IEnumerable<K> keys)
            => keys.Select(_ => dict[_]);

        public static IEnumerable<T> SelectAt<T>(this IList<T> list, IEnumerable<int> indices)
            => indices.Select(_ => list[_]);

        public static IEnumerable<T> Flatten<T>(this IEnumerable<IEnumerable<T>> values)
            => values.SelectMany(_ => _);

        public static IEnumerable<(T Value, int Index)> EnumerateIndexed<T>(this IEnumerable<T> values)
            => values.Select((v, i) => (v, i));

        public static void ReplaceElements<T>(this T[] values, int start, int end, Func<T, T> modifier)
        {
            for (int i = start; i < end; i++)
                values[i] = modifier(values[i]);
        }

        public static T MaxBy<T, U>(this IEnumerable<T> values, Func<T, U> selector, T whenEmpty = default) where U : IComparable
        {
            var iter = values.GetEnumerator();
            if (!iter.MoveNext())
                return whenEmpty;
            T result = iter.Current;
            U sel = selector(result);
            while (iter.MoveNext())
            {
                var newSel = selector(iter.Current);
                if (newSel.CompareTo(sel) > 0)
                {
                    result = iter.Current;
                    sel = newSel;
                }
            }
            return result;
        }

        public static void ForEach<T>(this T[] array, Action<T> action)
        {
            for (int i = 0; i < array.Length; i++)
                action(array[i]);
        }

        public static IEnumerable<T> SelectByType<T>(this IEnumerable<object> values)
        {
            foreach (var value in values)
                if (value is T valueT)
                    yield return valueT;
        }

        public static string Escape(this string str)
        {
            return str
                .Replace("\n", "\\n")
                .Replace("\t", "\\t")
                .Replace("\r", "\\r")
                .Replace(" ", "\\s")
                .Replace("\"", "\\\"")
                .Replace("\\", "\\\\");
        }

    }
}
