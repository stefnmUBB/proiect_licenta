using LillyScan.Backend.Parsers;
using System;
using System.Collections.Generic;
using System.Linq;
using static LillyScan.Backend.Parsers.Lexic.TextFiniteAutomataUtils;

namespace LillyScan.Backend.Parsers.Lexic
{
    public sealed class TextFiniteAutomaton
    {
        internal CharTransitionsSet<int> Transitions { get; }
        internal int InitialState { get; }
        internal HashSet<int> FinalStates { get; }
        public bool IsDeterministic { get; }

        internal TextFiniteAutomaton(CharTransitionsSet<int> transitions, int initialState, HashSet<int> finalStates, bool isDeterministic)
        {
            Transitions = transitions;
            InitialState = initialState;
            FinalStates = finalStates;
            IsDeterministic = isDeterministic;
        }


        public static TextFiniteAutomatonBuilder<Q> CreateBuilder<Q>() => new TextFiniteAutomatonBuilder<Q>();
        public static TextFiniteAutomatonBuilder<string> CreateBuilder() => CreateBuilder<string>();

        public override string ToString()
        {
            return $"InitialState: {InitialState}\nFinalState: {FinalStates.JoinToString(",")}\nTrantisions:{Transitions}";
        }


        private int[] GetNextStates(int q0, char c)
        {
            return Transitions.GetAllTransitions(q0)
                .Where(_ => _.Symbols.Contains(c))
                .SelectMany(_ => _.States)
                .Distinct()
                .ToArray();
        }

        private int GetNextStateNoThrow(int q0, char c)
        {
            var states = GetNextStates(q0, c);
            return states.Length > 0 ? states[0] : -1;
        }

        public bool IsAcceptedSequence(string sequence)
        {
            ValidateSequenceProcessing();
            int q = InitialState, i;
            for (i = 0; i < sequence.Length; i++)
            {
                q = GetNextStateNoThrow(q, sequence[i]);
                if (q < 0) return false;
            }
            return i == sequence.Length && FinalStates.Contains(q);
        }

        public int FindLongestAcceptedSequenceLength(string sequence, int startIndex = 0)
        {
            ValidateSequenceProcessing();

            if (startIndex < 0)
                throw new IndexOutOfRangeException($"Invalid access index {startIndex}");

            var q = InitialState;
            int maxLen = FinalStates.Contains(q) ? 0 : -1, steps = 0;

            for (int i = startIndex; i < sequence.Length; i++)
            {
                q = GetNextStateNoThrow(q, sequence[i]);
                if (q < 0) break;

                if (FinalStates.Contains(q))
                    maxLen = i + 1;
                steps++;
            }

            return steps == 0 ? maxLen : maxLen - startIndex;
        }

        private void ValidateSequenceProcessing()
        {
            if (!IsDeterministic)
                throw new InvalidOperationException(
                    "Could not check a sequence against an NFA. Make the automaton deterministic first. " +
                    "Use the IFiniteAutomaton<T>.AsDeterministic() method");
        }

        private AutomatonProps<int> AsProps() => new AutomatonProps<int>(Transitions.Transitions, InitialState, FinalStates);

        private TextFiniteAutomaton TransitionsCombine(
            Func<AutomatonProps<int>, AutomatonProps<int>, AutomatonProps<int>> combiner,
            TextFiniteAutomaton other
            )
        {
            var (d, i, f) = combiner(AsProps(), other.AsProps()).AsDeterministic().PropsTuple;
            return new TextFiniteAutomaton(new CharTransitionsSet<int>(d), i, f, true);
        }


        public TextFiniteAutomaton ZeroOrMany()
        {
            var (d, i, f) = TextFiniteAutomataUtils.ZeroOrManyOf(AsProps()).AsDeterministic().PropsTuple;
            return new TextFiniteAutomaton(new CharTransitionsSet<int>(d), i, f, true);
        }

        public TextFiniteAutomaton OneOrMany() => this * ZeroOrMany();

        public TextFiniteAutomaton UnionWith(TextFiniteAutomaton fa) => TransitionsCombine(TextFiniteAutomataUtils.TransitionsUnion, fa);
        public TextFiniteAutomaton ConcatWith(TextFiniteAutomaton fa) => TransitionsCombine(TextFiniteAutomataUtils.TransitionsConcat, fa);
        public static TextFiniteAutomaton operator +(TextFiniteAutomaton fa1, TextFiniteAutomaton fa2) => fa1.UnionWith(fa2);
        public static TextFiniteAutomaton operator *(TextFiniteAutomaton fa1, TextFiniteAutomaton fa2) => fa1.ConcatWith(fa2);
        public static TextFiniteAutomaton operator *(TextFiniteAutomaton fa1, int n)
        {
            if (n < 0) return fa1.ZeroOrMany();
            if (n == 0) return BuiltInFiniteAutomata.Empty();
            var fa = BuiltInFiniteAutomata.Empty();
            for (int i = 0; i < n; i++)
                fa *= fa1;
            return fa;
        }
    }
}
