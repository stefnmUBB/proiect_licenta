using LillyScan.Backend.Parsers;
using System;
using System.Collections.Generic;
using System.Linq;

namespace LillyScan.Backend.Parsers.Lexic
{
    public class LexicalParser
    {
        private readonly List<(string Key, TextFiniteAutomaton Automaton)> Automata;
        private readonly HashSet<string> IgnoredKeys;

        internal LexicalParser(List<(string Key, TextFiniteAutomaton Automaton)> automata, HashSet<string> ignoredKeys)
        {
            Automata = automata;
            IgnoredKeys = ignoredKeys;
        }

        private LexicalToken FindNextToken(string input, int startIndex)
        {
            var result = Automata
                .Select(_ =>
                {
                    var len = _.Automaton.FindLongestAcceptedSequenceLength(input, startIndex);
                    if (len < 0) return null;
                    return new LexicalToken(_.Key, input.Substring(startIndex, len), startIndex);
                })
                .Where(_ => _ != null)
                .MaxBy(_ => _.Value.Length);
            return result;
        }

        public ParseResult<LexicalToken[]> Parse(string input)
        {
            var result = new List<LexicalToken>();
            var position = 0;

            while (position < input.Length)
            {                
                var token = FindNextToken(input, position);

                if (token == null)
                    return new ParseResult<LexicalToken[]>.Error("Lexical", result.ToArray(), "Unrecognized token", position);


                if (token.Value.Length == 0)
                    return new ParseResult<LexicalToken[]>.Error("Lexical", result.ToArray(),
                        $"Empty string identified as token: {token.Key}", position);

                result.Add(token);
                position += token.Value.Length;
            }

            return new ParseResult<LexicalToken[]>.Success("Lexical", result.ToArray());
        }

        public static LexicalParserBuilder CreateBuilder() => new LexicalParserBuilder();
    }
}
