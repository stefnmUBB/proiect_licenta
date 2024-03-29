using LillyScan.Backend.Parsers;
using System;
using System.Collections.Generic;
using System.Text;
using static LillyScan.Backend.Parsers.Lexic.TextFiniteAutomataUtils;

namespace LillyScan.Backend.Parsers.Lexic
{
    public sealed class TextFiniteAutomatonBuilder<Q>
    {
        private readonly Dictionary<(Q State, Charset Symbols), HashSet<Q>> pTransitions
            = new Dictionary<(Q State, Charset Symbols), HashSet<Q>>();

        private Q pInitialState;
        private readonly HashSet<Q> pFinalStates = new HashSet<Q>();

        public TextFiniteAutomatonBuilder<Q> Transition(Q state0, Charset symbols, Q state1)
        {
            pTransitions.GetOrCreate((state0, symbols)).Add(state1);
            return this;
        }

        public TextFiniteAutomatonBuilder<Q> Transition(Q state0, CharsRange range, Q state1)
        {
            pTransitions.GetOrCreate((state0, new Charset(range))).Add(state1);
            return this;
        }

        public TextFiniteAutomatonBuilder<Q> Transition(Q state0, char c, Q state1)
        {
            pTransitions.GetOrCreate((state0, new Charset(new CharsRange(c)))).Add(state1);
            return this;
        }

        public TextFiniteAutomatonBuilder<Q> InitialState(Q state)
        {
            pInitialState = state;
            return this;
        }

        public TextFiniteAutomatonBuilder<Q> FinalStates(params Q[] states)
        {
            pFinalStates.UnionWith(states);
            return this;
        }

        public TextFiniteAutomaton Build(bool forceDeterministic = false)
        {
            var ap = new AutomatonProps<Q>(pTransitions, pInitialState, pFinalStates)
                .ToIdentifiedStates();
            var (t, i, f) = ap.PropsTuple;
            var tSet = new CharTransitionsSet<int>(t);

            if (forceDeterministic)
            {
                ap = new AutomatonProps<int>(tSet.GetAllTransitions(), i, f).AsDeterministic();
                (t, i, f) = ap.PropsTuple;
                tSet = new CharTransitionsSet<int>(t);
            }

            return new TextFiniteAutomaton(tSet, i, f, tSet.IsDeterministic);
        }

    }
}
