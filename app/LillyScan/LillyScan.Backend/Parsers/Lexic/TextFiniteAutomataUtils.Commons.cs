using LillyScan.Backend.Parsers;
using System;
using System.Collections.Generic;
using System.Linq;

namespace LillyScan.Backend.Parsers.Lexic
{
    internal static partial class TextFiniteAutomataUtils
    {
        internal class AutomatonProps<Q>
        {
            public Dictionary<(Q State, Charset Symbols), HashSet<Q>> Transitions;
            public Q InitialState;
            public HashSet<Q> FinalStates;
            public AutomatonProps(Dictionary<(Q State, Charset Symbols), HashSet<Q>> transitions = null, Q initialState = default, HashSet<Q> finalStates = null)
            {
                Transitions = transitions ?? new Dictionary<(Q State, Charset Symbols), HashSet<Q>>();
                InitialState = initialState;
                FinalStates = finalStates ?? new HashSet<Q>();
            }          

            public (Dictionary<(Q State, Charset Symbols), HashSet<Q>>, Q, HashSet<Q>) PropsTuple
                => (Transitions, InitialState, FinalStates);

        }

        public static IEnumerable<Q> GetAllStates<Q>(this AutomatonProps<Q> ap) => ap
            .Transitions
            .Keys.Select(_ => _.State)
            .Concat(ap.Transitions.SelectMany(_ => _.Value))
            .Concat(ap.FinalStates)
            .Append(ap.InitialState)
            .Distinct(); 

        public static AutomatonProps<int> ToIdentifiedStates<Q>(this AutomatonProps<Q> ap)            
        {
            var (transitions, initialState, finalStates) = ap.PropsTuple;
            var ids = GetAllStates(new AutomatonProps<Q>(transitions, initialState, finalStates))
                .Select((q, i) => (q, i))
                .ToDictionary(_ => _.q, _ => _.i);
            return new AutomatonProps<int>
                (
                    transitions.ToDictionary(_ => (ids[_.Key.State], _.Key.Symbols), _ => new HashSet<int>(ids.SelectValues(_.Value))),
                    ids[initialState],
                    new HashSet<int>(ids.SelectValues(finalStates))
                );
        }
        
        public static IEnumerable<(Q State, Charset Symbols, HashSet<Q> NextStates)> EnumerateTransitions<Q>(
            this Dictionary<(Q State, Charset Symbols), HashSet<Q>> transitions)
        {
            foreach (var (q0, s) in transitions.Keys)
                yield return (q0, s, transitions[(q0, s)]);
        }

        public static void ForeachTransition<Q>(
            this Dictionary<(Q State, Charset Symbols), HashSet<Q>> transitions, Action<Q, Charset, Q> action)
        {
            foreach (var (q0, s) in transitions.Keys)
                foreach (var q1 in transitions[(q0, s)]) 
                    action(q0, s, q1);
        }

        private static void AddTransition<Q>(this Dictionary<(Q, Charset), HashSet<Q>> d, Q q0, Charset s, Q q1)
            => d.GetOrCreate((q0, s)).Add(q1);


        public static AutomatonProps<Q> DropUnreachableStates<Q>(this AutomatonProps<Q> props)
        {
            var (transitions, initialState, finalStates) = props.PropsTuple;

            var queue = new Queue<Q>(new[] { initialState });
            var visited = new HashSet<Q>(new[] { initialState });
            var result = new Dictionary<(Q State, Charset Symbols), HashSet<Q>>();

            while (queue.Count > 0)
            {
                var q = queue.Dequeue();
                foreach (var kv in transitions) 
                {
                    var state = kv.Key.State;
                    var symbol = kv.Key.Symbols;
                    var neighbors = kv.Value;
                    if (!object.Equals(state, q)) continue;

                    var set = result.GetOrCreate((state, symbol));

                    foreach (var n in neighbors)
                    {
                        set.Add(n);
                        if (visited.Add(n))
                            queue.Enqueue(n);
                    }
                }
            }
            var resultFinalStates = new HashSet<Q>(finalStates);
            resultFinalStates.IntersectWith(visited);
            return new AutomatonProps<Q>(result, initialState, resultFinalStates);
        }

        public static AutomatonProps<Q> DropUnproductiveStates<Q>(this AutomatonProps<Q> ap)
        {
            var states = new HashSet<Q>(ap.GetAllStates());

            var (d, initialState, finalStates) = ap.PropsTuple;

            while (true)
            {
                var deadEndStates = states.Where(s => !d.Any(_ => object.Equals(_.Key.State, s))).Except(finalStates).ToArray();

                if (deadEndStates.Length == 0) break;

                d = d.ToDictionary(_ => _.Key, _ => new HashSet<Q>(_.Value.Except(deadEndStates)))
                    .Where(_ => _.Value.Count > 0)
                    .ToDictionary(_ => _.Key, _ => _.Value);                

                states.ExceptWith(deadEndStates);
            }
            return new AutomatonProps<Q>(d, initialState, new HashSet<Q>(finalStates));
        }

    }    
}
