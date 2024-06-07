using LillyScan.Backend.Parsers;
using LillyScan.Backend.Parsers.Lexic;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace LillyScan.Backend.Parsers.Internal
{
    using BIFA = BuiltInFiniteAutomata;
    public class GrammarDefinitionParser
    {
        static readonly Charset RE_MustEscape = Charsets.Chars("\\+-[]()*?");
        static readonly Charset RE_CanEscape = Charsets.Chars("ntsr");

        static readonly TextFiniteAutomaton FA_RE_Character =
            BIFA.Chars(Charsets.AlphaNum + Charsets.Chars("_")) +
            (BIFA.Literal("\\") * BIFA.Chars(RE_MustEscape + RE_CanEscape)) +
            BIFA.EscapedUnicode();

        static readonly TextFiniteAutomaton FA_RE_CharsRange =
            FA_RE_Character * BIFA.Chars(Charsets.SingleChar('-')) * FA_RE_Character;


        public static readonly TextFiniteAutomaton FA_RE_Selector =
            (BIFA.Literal("[") * (FA_RE_Character + FA_RE_CharsRange).OneOrMany() * BIFA.Literal("]"));

        public static readonly TextFiniteAutomaton FA_Regex =
            (FA_RE_Selector * (BIFA.Chars("+*?") + BIFA.Empty())).OneOrMany();            


        static readonly LexicalParser LexicalParser = LexicalParser.CreateBuilder()
            .Register("kw_head_lex", BuiltInFiniteAutomata.Literal("[[lex]]"))
            .Register("kw_head_syn", BuiltInFiniteAutomata.Literal("[[syn]]"))
            .Register("op_assign", BuiltInFiniteAutomata.Literal("::="))
            .Register("op_define", BuiltInFiniteAutomata.Literal("->"))            
            .Register("sc", BuiltInFiniteAutomata.Literal(";"))
            .Register("ws", BuiltInFiniteAutomata.Chars(Charsets.Chars(' ', '\n', '\r', '\t')))
            .Register("identifier", BuiltInFiniteAutomata.FirstCharacterConstrain(
                Charsets.UpperAlpha + Charsets.LowerAlpha + '_',
                Charsets.UpperAlpha + Charsets.LowerAlpha + '_' + Charsets.Digits))
            .Register("regex", FA_Regex)

            .Build();


        class Token : LexicalToken
        {
            public Token(LexicalToken lt) : base(lt.Key, lt.Value, lt.Position) { }            
            public override bool Equals(object obj) => obj is Token token && Key == token.Key;            
            public override int GetHashCode() => 990326508 + EqualityComparer<string>.Default.GetHashCode(Key);

            public static Token FromLexicalToken(LexicalToken lt) => new Token(lt);
        }

        public void Parse(string input)
        {
            var lexParseResult = LexicalParser.Parse(input);

            if (lexParseResult is ParseResult<LexicalToken[]>.Error err)
            {
                Debug.WriteLine(err.Value.JoinToString("\n"));
                throw new ParseException(err.Type, err.Message, err.Position);
            }

            var tokens = lexParseResult.Value
                .Where(_ => _.Key != "ws")
                .Select(Token.FromLexicalToken)
                .ToArray();

            // Debug.WriteLine(tokens.JoinToString("\n"));
        }


    }
}
