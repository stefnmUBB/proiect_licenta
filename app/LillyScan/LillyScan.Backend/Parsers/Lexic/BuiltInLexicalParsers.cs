using LillyScan.Backend.Parsers;
using System;
using System.Collections.Generic;
using System.Text;

namespace LillyScan.Backend.Parsers.Lexic
{
    public static class BuiltInLexicalParsers
    {
        private static readonly TextFiniteAutomaton FA_RE_Character = TextFiniteAutomaton.CreateBuilder()
            .InitialState("q0")
            .Transition("q0", Charsets.All - Charsets.Chars("\\[]()+-\""), "q1")
            .Transition("q0", '\\', "q2")
            .Transition("q2", Charsets.Chars("\\nrts\"[]()+-"), "q1")
            .FinalStates("q1")
            .Build(forceDeterministic: true);


        public static LexicalParser RegularExpression() => LexicalParser.CreateBuilder()
            .Register("character", FA_RE_Character)
            .Register("lbracket", BuiltInFiniteAutomata.Literal("["))
            .Register("rbracket", BuiltInFiniteAutomata.Literal("]"))
            .Register("lparen", BuiltInFiniteAutomata.Literal("("))
            .Register("rparen", BuiltInFiniteAutomata.Literal(")"))
            .Register("lbrace", BuiltInFiniteAutomata.Literal("{"))
            .Register("rbrace", BuiltInFiniteAutomata.Literal("}"))
            .Register("plus", BuiltInFiniteAutomata.Literal("+"))
            .Register("hyphen", BuiltInFiniteAutomata.Literal("-"))
            .Build();

    }
}
