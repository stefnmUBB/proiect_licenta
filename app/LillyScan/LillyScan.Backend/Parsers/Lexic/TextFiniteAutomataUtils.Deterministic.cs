using LillyScan.Backend.Parsers;
using System;
using System.Collections.Generic;
using System.Linq;

namespace LillyScan.Backend.Parsers.Lexic
{
    internal static partial class TextFiniteAutomataUtils
    {                
        class StatesSet<Q> : HashSet<Q>
        {
            public override bool Equals(object obj) => obj is StatesSet<Q> set && this.SetEquals(set);
            public override int GetHashCode() => unchecked(-1537116874 * 1521134295 + Count.GetHashCode());

            public StatesSet(){ }
            public StatesSet(params Q[] states) => UnionWith(states);
            public StatesSet(HashSet<Q> states) => UnionWith(states);
            public StatesSet(IEnumerable<Q> states) => UnionWith(states);

            public override string ToString() => this.JoinToString(", ", "{", "}");
        }

        public static AutomatonProps<int> AsDeterministic<Q>(this AutomatonProps<Q> ap)
        {
            var (transitions, initialState, finalStates) = ap.PropsTuple;
            var dap = new AutomatonProps<StatesSet<Q>>(
                initialState: new StatesSet<Q>(initialState),
                finalStates: new HashSet<StatesSet<Q>>(finalStates.Select(_ => new StatesSet<Q>(_)))
                );

            var charsets = transitions.Select(_ => _.Key.Symbols).Distinct().ToArray();

            var newStates = new HashSet<StatesSet<Q>>();
            int newStatesCount = 0;

            foreach (var (q, s, qs) in transitions.EnumerateTransitions())
            {
                var q0 = new StatesSet<Q>(q);
                var q1 = new StatesSet<Q>(qs);
                dap.Transitions.AddTransition(q0, s, q1);
                if (qs.Count > 1)
                {
                    if (newStates.Add(q1))
                        newStatesCount++;
                }
            }

            while (newStatesCount > 0)
            {
                var tmpStates = new HashSet<StatesSet<Q>>();
                newStatesCount = 0;


                foreach (var q0 in newStates)
                {
                    if (q0.Intersect(finalStates).Any())
                        dap.FinalStates.Add(q0);

                    foreach (var s in charsets)
                    {
                        var q1 = new StatesSet<Q>(q0.SelectMany(_ => transitions.GetOrDefault((_, s))));
                        newStatesCount += Convert.ToInt32(q1.Count > 1 && !newStates.Contains(q1) && tmpStates.Add(q1));
                        dap.Transitions.AddTransition(q0, s, q1);
                    }
                }
                newStates.UnionWith(tmpStates);
                tmpStates.Clear();
            }
            return dap.DropUnreachableStates().DropUnproductiveStates().ToIdentifiedStates();
        }
    }
}
