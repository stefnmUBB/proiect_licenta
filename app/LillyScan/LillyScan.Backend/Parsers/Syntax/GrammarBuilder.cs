using System;
using System.Collections.Generic;
using System.Text;

namespace LillyScan.Backend.Parsers.Syntax
{
    public class GrammarBuilder<T>
    {
        private readonly List<Rule<T>> pRules = new List<Rule<T>>();
        private NonTerminal<T> pStartSymbol = null;

        internal GrammarBuilder() { }

        public GrammarBuilder<T> Rule(Rule<T> rule)
        {
            pRules.Add(rule);
            return this;
        }

        public GrammarBuilder<T> Rule(NonTerminal<T> left, RuleComponent<T>[] right)
            => Rule(new Rule<T>(left, right));

        public GrammarBuilder<T> Rule(int id, NonTerminal<T> left, RuleComponent<T>[] right)
            => Rule(new Rule<T>(left, right, id));

        public GrammarBuilder<T> StartSymbol(NonTerminal<T> symbol)
        {
            pStartSymbol = symbol;
            return this;
        }

        public Grammar<T> Build() => new Grammar<T>(pRules, pStartSymbol);

    }
}
