using LillyScan.Backend.Parsers;
using System;
using System.Collections.Generic;
using System.Linq;

namespace LillyScan.Backend.Parsers.Lexic
{
    internal static partial class TextFiniteAutomataUtils
    {       
        class CummulativeStatePool<Q>
        {
            public Dictionary<(object, Q), int> States = new Dictionary<(object, Q), int>();
            private int NextId = 0;
            public int this[object d, Q state] => States[(d, state)];
            public int this[(object d, Q state) pair] => States[pair];
            public void AddIdentical(params (object, Q)[] states)
            {
                foreach (var (d, s) in states)
                    States[(d, s)] = NextId;
                NextId++;
            }
            public void Add(object d, Q state) => States[(d, state)] = NextId++;
            public void Add((object d, Q state) pair) => States[pair] = NextId++;
        }

        public static AutomatonProps<int> TransitionsUnion<Q>(AutomatonProps<Q> ap1, AutomatonProps<Q> ap2)            
        {
            var (d1, q01, f1) = ap1.PropsTuple;
            var (d2, q02, f2) = ap2.PropsTuple;

            var d = new Dictionary<(int State, Charset Symbol), HashSet<int>>();
            var Q1 = new HashSet<Q>(ap1.GetAllStates());
            var Q2 = new HashSet<Q>(ap2.GetAllStates());

            Q1.ExceptWith(new[] { q01 });
            Q2.ExceptWith(new[] { q02 });

            var statePool = new CummulativeStatePool<Q>();
            statePool.AddIdentical((d1, q01), (d2, q02));

            foreach (var q in Q1) statePool.Add(d1, q);
            foreach (var q in Q2) statePool.Add(d2, q);

            d1.ForeachTransition((q0, s, q1) => d.AddTransition(statePool[d1, q0], s, statePool[d1, q1]));
            d2.ForeachTransition((q0, s, q1) => d.AddTransition(statePool[d2, q0], s, statePool[d2, q1]));


            var finalStates = new HashSet<int>(f1.Select(_ => (d1 as object, _))
                .Concat(f2.Select(_ => (d2 as object, _)))                
                .MapIndexable(statePool.States));

            return new AutomatonProps<int>(d, statePool[d1, q01], finalStates);
        }

        public static AutomatonProps<int> TransitionsConcat<Q>(AutomatonProps<Q> ap1, AutomatonProps<Q> ap2)            
        {
            var (d1, q01, f1) = ap1.PropsTuple;
            var (d2, q02, f2) = ap2.PropsTuple;

            var d = new Dictionary<(int State, Charset Symbol), HashSet<int>>();
            var Q1 = new HashSet<Q>(ap1.GetAllStates());
            var Q2 = new HashSet<Q>(ap2.GetAllStates());

            var statePool = new CummulativeStatePool<Q>();
            foreach (var q in Q1) statePool.Add(d1, q);
            foreach (var q in Q2) statePool.Add(d2, q);

            var usedStates = new HashSet<int>();
            int use(int x) { usedStates.Add(x); return x; }

            d1.ForeachTransition((q0, s, q1) => d.AddTransition(use(statePool[d1, q0]), s, use(statePool[d1, q1])));

            d2.ForeachTransition((q0, s, q1) =>
            {
                if (object.Equals(q0, q02))
                {
                    if (object.Equals(q1, q02))
                    {
                        foreach (var qf in f1)
                            d.AddTransition(use(statePool[d1, qf]), s, use(statePool[d1, qf]));
                    }
                    else
                    {
                        foreach (var qf in f1)
                            d.AddTransition(use(statePool[d1, qf]), s, use(statePool[d2, q1]));
                    }
                }
                else
                    d.AddTransition(use(statePool[d2, q0]), s, use(statePool[d2, q1]));
            });

            var finalStates = f2.Select(_ => statePool[d2, _]).Intersect(usedStates).ToList();
            if (f2.Contains(q02))
            {
                foreach (var qf in f1)
                    finalStates.Add(statePool[d1, qf]);
            }
            return new AutomatonProps<int>(d, statePool[d1, q01], new HashSet<int>(finalStates));            
        }

        public static AutomatonProps<int> ZeroOrManyOf<Q>(AutomatonProps<Q> ap0)
        {
            var (d0, q0, f0) = ap0.PropsTuple;
            var d = new Dictionary<(int State, Charset Symbol), HashSet<int>>();
            HashSet<Q> Qs = new HashSet<Q>(ap0.GetAllStates());
            var statePool = new CummulativeStatePool<Q>();
            foreach (var q in Qs) statePool.Add(d0, q);
            var newQ0 = (null as object, q0);
            statePool.Add(newQ0);
            d0.ForeachTransition((qi, s, qj) =>
            {
                if (object.Equals(q0, qi))
                {
                    d.AddTransition(statePool[newQ0], s, statePool[d0, qj]);
                    foreach (var qf in f0)
                        d.AddTransition(statePool[d0, qf], s, statePool[d0, qj]);
                }
                else
                    d.AddTransition(statePool[(d0, qi)], s, statePool[d0, qj]);
            });
            var finalStates = f0.Select(q => (d0 as object, q)).Concat(new[] { newQ0 }).MapIndexable(statePool.States);
            return new AutomatonProps<int>(d, statePool[newQ0], new HashSet<int>(finalStates));
        }

    }
}
