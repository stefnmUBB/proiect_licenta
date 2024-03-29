using LillyScan.Backend.Parsers;
using System;
using System.Collections.Generic;
using System.Linq;

namespace LillyScan.Backend.Parsers.Lexic
{
    internal sealed class CharTransitionsSet<Q>
    {
        internal Dictionary<(Q State, Charset Symbols), HashSet<Q>> Transitions;
        private readonly Q[] pStates;
        private bool pIsDeterministic { get; }

        internal CharTransitionsSet(Dictionary<(Q State, Charset Symbols), HashSet<Q>> transitions)
        {
            Transitions = transitions;
            pStates = transitions.Keys.Select(_ => _.State).Concat(transitions.Values.Flatten()).Distinct().ToArray();
            Transitions = CharTransitionsSetUtils.FixDisjointSymbolsSets(Transitions);
            pIsDeterministic = Transitions.Values.All(_ => _.Count <= 1);
        }

        internal Q[] States => pStates;
        internal bool IsDeterministic => pIsDeterministic;

        internal HashSet<Q> GetTransitions(Q state, Charset symbols)
            => Transitions.TryGetValue((state, symbols), out var result) ? result : new HashSet<Q>();

        internal IEnumerable<(Charset Symbols, HashSet<Q> States)> GetAllTransitions(Q state)
            => Transitions.Where(_ => Equals(_.Key.State, state)).Select(_ => (_.Key.Symbols, _.Value));

        internal Dictionary<(Q State, Charset Symbols), HashSet<Q>> GetAllTransitions()
            => Transitions.ToDictionary(_ => _.Key, _ => new HashSet<Q>(_.Value));

        public override string ToString()
        {
            return Transitions
                .Select(_ => $"({_.Key.State}, {_.Key.Symbols}) -> {_.Value.JoinToString(", ")}")
                .JoinToString("\n    ", "{\n    ", "\n}");
        }

    }
}