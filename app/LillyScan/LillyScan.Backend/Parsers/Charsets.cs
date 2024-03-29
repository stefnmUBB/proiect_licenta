using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LillyScan.Backend.Parsers
{
    public static class Charsets
    {
        public static readonly Charset LowerAlpha = new Charset(new CharsRange('a', 'z'));
        public static readonly Charset UpperAlpha = new Charset(new CharsRange('A', 'Z'));
        public static readonly Charset Digits = new Charset(new CharsRange('0', '9'));
        public static readonly Charset Alpha = LowerAlpha + UpperAlpha;
        public static readonly Charset AlphaNum = LowerAlpha + UpperAlpha + Digits;
        public static readonly Charset All = new Charset(new CharsRange(char.MinValue, char.MaxValue));
        public static readonly Charset Empty = new Charset();
        public static Charset SingleChar(char c) => new Charset(new CharsRange(c));
        public static Charset Chars(params char[] chars) => new Charset(chars.Select(_ => new CharsRange(_)).ToArray());
        public static Charset Chars(string chars) => new Charset(chars.Select(_ => new CharsRange(_)).ToArray());
        public static Charset Range(char c0, char c1) => new Charset(new CharsRange(c0, c1));
    }
}
