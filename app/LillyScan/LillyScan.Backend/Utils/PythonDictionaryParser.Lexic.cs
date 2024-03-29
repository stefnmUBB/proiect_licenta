using LillyScan.Backend.Parsers.Lexic;
using LillyScan.Backend.Parsers;
using System;
using System.Collections.Generic;
using System.Text;

using BIFA = LillyScan.Backend.Parsers.Lexic.BuiltInFiniteAutomata;

namespace LillyScan.Backend.Utils
{
    public partial class PythonDictionaryParser
    {
        private class Lexic
        {
            public class AtomTypes
            {
                public static readonly string LeftBracket = "lbrak";
                public static readonly string RightBracket = "rbrak";
                public static readonly string Comma = "comma";
                public static readonly string Colon = "colon";
                public static readonly string LeftParen = "lpar";
                public static readonly string RightParen = "rpar";
                public static readonly string String = "string";
                public static readonly string Decimal = "decimal";
                public static readonly string Integer = "integer";
                public static readonly string None = "none";
                public static readonly string True = "true";
                public static readonly string False = "false";
                public static readonly string Whitespace = "ws";
            }

            public static readonly LexicalParser Parser = LexicalParser.CreateBuilder()
                .Register(AtomTypes.LeftBracket, BIFA.Chars('{'))
                .Register(AtomTypes.RightBracket, BIFA.Chars('}'))
                .Register(AtomTypes.Comma, BIFA.Chars(','))
                .Register(AtomTypes.Colon, BIFA.Chars(':'))
                .Register(AtomTypes.LeftParen, BIFA.Chars('('))
                .Register(AtomTypes.RightParen, BIFA.Chars(')'))
                .Register(AtomTypes.String, BIFA.Chars("'") * BIFA.Chars(Charsets.AlphaNum + Charsets.Chars("_.")).ZeroOrMany() * BIFA.Chars("'"))
                .Register(AtomTypes.Decimal, (BIFA.Empty() + BIFA.Chars("+-")) * BIFA.Chars(Charsets.Digits).OneOrMany() * BIFA.Chars(".") * BIFA.Chars(Charsets.Digits).OneOrMany())
                .Register(AtomTypes.Integer, (BIFA.Empty() + BIFA.Chars("+-")) * BIFA.Chars(Charsets.Digits).OneOrMany())
                .Register(AtomTypes.None, BIFA.Literal("None"))
                .Register(AtomTypes.True, BIFA.Literal("True"))
                .Register(AtomTypes.False, BIFA.Literal("False"))
                .Register(AtomTypes.Whitespace, BIFA.Chars(" \t\r\n"))
                .Build();



            public class Token : LexicalToken
            {
                public Token(LexicalToken lt) : base(lt.Key, lt.Value, lt.Position) { }
                public Token(string key) : base(key, "placeholder", -1) { }
                public override bool Equals(object obj) => obj is Token token && Key == token.Key;
                public override int GetHashCode() => 990326508 + EqualityComparer<string>.Default.GetHashCode(Key);
                public static Token FromLexicalToken(LexicalToken lt) => new Token(lt);
            }
        }
    }
}
