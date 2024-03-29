using LillyScan.Backend.Parsers;
using LillyScan.Backend.Parsers.Internal;
using LillyScan.Backend.Parsers.Lexic;
using System;
using System.Collections.Generic;
using System.Linq;

namespace LillyScan.Backend.Utils
{
    public partial class PythonDictionaryParser
    {
        private interface IStackedItem { }        

        private interface IStackedRightValue : IStackedItem { }

        private class StackedChar : IStackedItem
        {
            public char Value { get; }

            public StackedChar(char value)
            {
                Value = value;
            }

            public override string ToString() => $"StackedChar(`{Value}`)";  
        }

        private class StackedWord : IStackedItem
        {
            public string Value { get; }

            public StackedWord(string value)
            {
                Value = value;
            }

            public override string ToString() => $"StackedWord(`{Value}`)";
        }

        private class StackedInt64 : IStackedRightValue
        {
            public long Value { get; }

            public StackedInt64(long value)
            {
                Value = value;
            }

            public override string ToString() => $"StackedInt({Value})";            
        }

        private class StackedNone : IStackedRightValue 
        {
            public override string ToString() => "StackedNone";
        }
        private class StackedTrue : IStackedRightValue 
        {
            public override string ToString() => "StackedTrue";
        }
        private class StackedFalse : IStackedRightValue 
        {
            public override string ToString() => "StackedFalse";
        }

        private class StackedKeyValuePair : IStackedItem
        {
            public string Key { get; }
            public IStackedRightValue Value { get; }

            public StackedKeyValuePair(string key, IStackedRightValue value)
            {
                Key = key;
                Value = value;
            }

            public override string ToString() => $"{Key} -> {Value}";
        }

        private class StackedTuple : IStackedRightValue
        {
            public IStackedRightValue[] Values { get; }

            public StackedTuple(IStackedRightValue[] values)
            {
                Values = values;
            }
            public override string ToString() => $"StackedTuple({Values.JoinToString(", ")})";
        }

        private class StackedDictionary : IStackedRightValue
        {
            public StackedKeyValuePair[] Pairs { get; }

            public StackedDictionary(StackedKeyValuePair[] pairs)
            {
                Pairs = pairs;
            }

            public override string ToString() => $"StackedDictionary({Pairs.JoinToString(", ")})";
        }

        private class StackedString : IStackedRightValue
        {
            public string Value { get; }
            public StackedString(string value) => Value = value;
            public override string ToString() => $"StackedString(`{Value}`)";
        }                        

        public static Dictionary<string, object> Parse(string input)
        {
            var lexResults = Lexic.Parser.Parse(input);
            if(lexResults is ParseResult<LexicalToken[]>.Error err)            
                throw new ParseException(err.Type, err.Message, err.Position);

            var tokens = lexResults.Value.Where(_ => _.Key != Lexic.AtomTypes.Whitespace).Select(Lexic.Token.FromLexicalToken).ToArray();
            //tokens.ForEach(Console.WriteLine);

            var result = Syntax.Parser.Parse<object>(tokens, Syntax.CreateParseAttribute, Syntax.GetTerminalAttribute);

            if (result is ParseResult<object>.Error syn_err)
                throw new ParseException(syn_err.Type, syn_err.Message, syn_err.Position);            

            return result.Value as Dictionary<string, object>;
        }              

    }
}
