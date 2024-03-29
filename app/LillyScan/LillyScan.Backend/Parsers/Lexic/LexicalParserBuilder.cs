using System.Collections.Generic;
using System.Linq;

namespace LillyScan.Backend.Parsers.Lexic
{
    public class LexicalParserBuilder
    {
        private readonly Dictionary<string, TextFiniteAutomaton> Automata = new Dictionary<string, TextFiniteAutomaton>();
        private readonly HashSet<string> IgnoredKeys = new HashSet<string>();

        internal LexicalParserBuilder() { }

        public LexicalParserBuilder Register(string key, TextFiniteAutomaton automaton)
        {
            Automata.Add(key, automaton);
            return this;
        }

        public LexicalParserBuilder Ignore(params string[] keys)
        {
            IgnoredKeys.UnionWith(keys);
            return this;
        }

        public LexicalParser Build() => new LexicalParser(Automata.Select(_ => (_.Key, _.Value)).ToList(), IgnoredKeys);

    }
}
