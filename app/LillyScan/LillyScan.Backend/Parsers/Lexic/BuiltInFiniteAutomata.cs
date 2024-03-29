using LillyScan.Backend.Parsers;
using System;
using System.Collections.Generic;
using System.Text;

namespace LillyScan.Backend.Parsers.Lexic
{
    public class BuiltInFiniteAutomata
    {
        public static TextFiniteAutomaton Empty() => TextFiniteAutomaton
            .CreateBuilder()
            .InitialState("q0")
            .FinalStates("q0")
            .Build(forceDeterministic: true);

        public static TextFiniteAutomaton Chars(params char[] chars) => Chars(Charsets.Chars(chars));
        public static TextFiniteAutomaton Chars(string chars) => Chars(Charsets.Chars(chars));

        public static TextFiniteAutomaton Chars(Charset charset) => TextFiniteAutomaton
            .CreateBuilder()
            .InitialState("q0")
            .Transition("q0", charset, "q1")
            .FinalStates("q1")
            .Build(forceDeterministic: true);

        public static TextFiniteAutomaton EscapedUnicode() =>
            Literal("\\u") *
            (Chars(Charsets.Digits + new CharsRange('a', 'f') + new CharsRange('A', 'F')) * 4);

        public static TextFiniteAutomaton Literal(string sequence)
        {
            var builder = TextFiniteAutomaton.CreateBuilder().InitialState("q0");
            for (int i = 0; i < sequence.Length; i++)
            {
                builder.Transition($"q{i}", sequence[i], $"q{i + 1}");
            }
            builder.FinalStates($"q{sequence.Length}");
            return builder.Build(forceDeterministic: true);
        }

        public static TextFiniteAutomaton UnsignedIntegers(int maxDigitsCount = 0, bool allowFirstZero = false)
            => FirstCharacterConstrain(
                new Charset(new CharsRange(allowFirstZero ? '0' : '1', '9')), Charsets.Digits, maxDigitsCount);

        public static TextFiniteAutomaton FirstCharacterConstrain(
            Charset firstCharacter, Charset nextCharacters, int maxLen = 0
            )
        {
            var builder = TextFiniteAutomaton.CreateBuilder()
                .InitialState("q0")
                .Transition("q0", firstCharacter, "q1");
            if (maxLen <= 0)
            {
                builder.Transition("q1", nextCharacters, "q1").FinalStates("q1");
            }
            else
            {
                for (int i = 1; i < maxLen; i++)
                    builder.Transition($"q{i}", nextCharacters, $"q{i + 1}").FinalStates($"q{i}");
                builder.FinalStates($"q{maxLen}");
            }
            return builder.Build(forceDeterministic: true);
        }

    }
}
