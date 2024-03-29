using LillyScan.Backend.Parsers.Lexic;
using LillyScan.Backend.Parsers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LillyScan.Backend.Parsers.Lexic
{
    internal static class CharTransitionsSetUtils
    {
        private static void OnNonEmptySet(Charset s, Action<Charset> action)
        {
            if (s.Count == 0) return;
            action(s);
        }

        private static (Charset[] Charsets, Dictionary<Charset, int> IndexMap) GatherSets(IEnumerable<Charset> charsets)
        {
            var sets = charsets.Distinct().ToArray();
            return (sets, sets.EnumerateIndexed().ToDictionary(_ => _.Value, _ => _.Index));
        }

        public static Dictionary<(Q State, Charset Symbols), HashSet<Q>> FixDisjointSymbolsSets<Q>(
            Dictionary<(Q State, Charset Symbols), HashSet<Q>> transitions)
        {
            var (sets, indexSet) = GatherSets(transitions.Keys.Select(_ => _.Symbols));
            if (indexSet.Count == 0) return transitions;

            var partitionsList = new List<Charset> { sets[0] };
            var partitionIndices = new List<int>[sets.Length];
            partitionIndices[0] = new List<int> { 0 };
            var reunion = sets[0];

            for (int i = 1; i < sets.Length; i++)
            {
                Dictionary<int, List<int>> partIx = new Dictionary<int, List<int>>();

                var newPartitionsList = new List<Charset>();
                partitionIndices[i] = new List<int>();

                foreach (var (p, j) in partitionsList.EnumerateIndexed())
                {
                    OnNonEmptySet(p & sets[i], s =>
                    {
                        newPartitionsList.Add(s);
                        partIx.GetOrCreate(j).Add(newPartitionsList.Count - 1);
                        partitionIndices[i].Add(newPartitionsList.Count - 1);
                    });
                    OnNonEmptySet(p - sets[i], s =>
                    {
                        newPartitionsList.Add(s);
                        partIx.GetOrCreate(j).Add(newPartitionsList.Count - 1);
                    });
                }
                OnNonEmptySet(sets[i] - reunion, s =>
                {
                    newPartitionsList.Add(s);
                    partitionIndices[i].Add(newPartitionsList.Count - 1);
                });
                reunion += sets[i];

                partitionIndices.ReplaceElements(0, i, l => partIx.SelectValues(l).Flatten().ToList());
                partitionsList = newPartitionsList;
            }

            var newTransitions = new Dictionary<(Q State, Charset Symbols), HashSet<Q>>();
            transitions.ForeachTransition((q0, s, q1) =>
            {
                foreach (var part in partitionsList.SelectAt(partitionIndices[indexSet[s]]))
                    newTransitions.GetOrCreate((q0, part)).Add(q1);
            });
            return newTransitions;
        }
    }
}
