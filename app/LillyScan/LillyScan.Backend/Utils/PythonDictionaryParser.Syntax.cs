using EsotericDevZone.Parsers.Syntax.Parsers;
using LillyScan.Backend.Parsers.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;


namespace LillyScan.Backend.Utils
{
    public partial class PythonDictionaryParser
    {        
        private class Syntax
        {
            private static readonly NonTerminal<Lexic.Token> dict = new NonTerminal<Lexic.Token>(nameof(dict));
            private static readonly NonTerminal<Lexic.Token> kvPairsList = new NonTerminal<Lexic.Token>(nameof(kvPairsList));
            private static readonly NonTerminal<Lexic.Token> kvPair = new NonTerminal<Lexic.Token>(nameof(kvPair));
            private static readonly NonTerminal<Lexic.Token> tkString = new NonTerminal<Lexic.Token>(nameof(tkString));
            private static readonly NonTerminal<Lexic.Token> rightValue = new NonTerminal<Lexic.Token>(nameof(rightValue));
            private static readonly NonTerminal<Lexic.Token> tuple = new NonTerminal<Lexic.Token>(nameof(tuple));
            private static readonly NonTerminal<Lexic.Token> rightValuesList = new NonTerminal<Lexic.Token>(nameof(rightValuesList));

            private static readonly Terminal<Lexic.Token> lbrak = new Terminal<Lexic.Token>(new Lexic.Token(Lexic.AtomTypes.LeftBracket));
            private static readonly Terminal<Lexic.Token> rbrak = new Terminal<Lexic.Token>(new Lexic.Token(Lexic.AtomTypes.RightBracket));
            private static readonly Terminal<Lexic.Token> comma = new Terminal<Lexic.Token>(new Lexic.Token(Lexic.AtomTypes.Comma));
            private static readonly Terminal<Lexic.Token> colon = new Terminal<Lexic.Token>(new Lexic.Token(Lexic.AtomTypes.Colon));
            private static readonly Terminal<Lexic.Token> lparen = new Terminal<Lexic.Token>(new Lexic.Token(Lexic.AtomTypes.LeftParen));
            private static readonly Terminal<Lexic.Token> rparen = new Terminal<Lexic.Token>(new Lexic.Token(Lexic.AtomTypes.RightParen));
            private static readonly Terminal<Lexic.Token> str = new Terminal<Lexic.Token>(new Lexic.Token(Lexic.AtomTypes.String));
            private static readonly Terminal<Lexic.Token> integerNumber = new Terminal<Lexic.Token>(new Lexic.Token(Lexic.AtomTypes.Integer));
            private static readonly Terminal<Lexic.Token> decimalNumber = new Terminal<Lexic.Token>(new Lexic.Token(Lexic.AtomTypes.Decimal));
            private static readonly Terminal<Lexic.Token> kwNone = new Terminal<Lexic.Token>(new Lexic.Token(Lexic.AtomTypes.None));
            private static readonly Terminal<Lexic.Token> kwTrue = new Terminal<Lexic.Token>(new Lexic.Token(Lexic.AtomTypes.True));
            private static readonly Terminal<Lexic.Token> kwFalse = new Terminal<Lexic.Token>(new Lexic.Token(Lexic.AtomTypes.False));


            enum RuleType
            {
                EmptyDict,
                Dict,
                KeyValuePairsList,
                KeyValuePairsListOneItem,
                KeyValuePair,
                RValTrue,
                RValFalse,
                RValNone,
                RValDict,
                RValString,
                RValInteger,
                RValDecimal,
                RValTupleEmpty,
                RValTuple,
                RValTupleCommaEnded,
                RValsList,
                RValsListOneItem,
            }

            private static Dictionary<RuleType, Rule<Lexic.Token>> Rules = new Dictionary<RuleType, Rule<Lexic.Token>>
            {
                { RuleType.EmptyDict, new Rule<Lexic.Token>(dict, new RuleComponent<Lexic.Token>[] { lbrak, rbrak }) },
                { RuleType.Dict, new Rule<Lexic.Token>(dict, new RuleComponent<Lexic.Token>[] { lbrak, kvPairsList, rbrak }) },
                { RuleType.KeyValuePairsList, new Rule<Lexic.Token>(kvPairsList, new RuleComponent<Lexic.Token>[] { kvPair, comma, kvPairsList }) },
                { RuleType.KeyValuePairsListOneItem, new Rule<Lexic.Token>(kvPairsList, new RuleComponent<Lexic.Token>[] { kvPair }) },
                { RuleType.KeyValuePair, new Rule<Lexic.Token>(kvPair, new RuleComponent<Lexic.Token>[] { str, colon, rightValue }) },
                { RuleType.RValNone, new Rule<Lexic.Token>(rightValue, new RuleComponent<Lexic.Token>[] { kwNone }) },
                { RuleType.RValTrue, new Rule<Lexic.Token>(rightValue, new RuleComponent<Lexic.Token>[] { kwTrue }) },
                { RuleType.RValFalse, new Rule<Lexic.Token>(rightValue, new RuleComponent<Lexic.Token>[] { kwFalse }) },
                { RuleType.RValDict, new Rule<Lexic.Token>(rightValue, new RuleComponent<Lexic.Token>[] { dict }) },
                { RuleType.RValString, new Rule<Lexic.Token>(rightValue, new RuleComponent<Lexic.Token>[] { str }) },
                { RuleType.RValInteger, new Rule<Lexic.Token>(rightValue, new RuleComponent<Lexic.Token>[] { integerNumber }) },
                { RuleType.RValDecimal, new Rule<Lexic.Token>(rightValue, new RuleComponent<Lexic.Token>[] { decimalNumber }) },
                { RuleType.RValTupleEmpty, new Rule<Lexic.Token>(rightValue, new RuleComponent<Lexic.Token>[] { lparen, rparen }) },
                { RuleType.RValTuple, new Rule<Lexic.Token>(rightValue, new RuleComponent<Lexic.Token>[] { lparen, rightValuesList, rparen }) },
                { RuleType.RValTupleCommaEnded, new Rule<Lexic.Token>(rightValue, new RuleComponent<Lexic.Token>[] { lparen, rightValuesList, comma, rparen }) },
                { RuleType.RValsList, new Rule<Lexic.Token>(rightValuesList, new RuleComponent<Lexic.Token>[] { rightValue, comma, rightValuesList }) },
                { RuleType.RValsListOneItem, new Rule<Lexic.Token>(rightValuesList, new RuleComponent<Lexic.Token>[] { rightValue }) },
            };

            public static Grammar<Lexic.Token> Grammar = new Grammar<Lexic.Token>(Rules.Values, startSymbol: dict);


            public static LR1Parser<Lexic.Token> Parser = new LR1Parser<Lexic.Token>(Grammar);



            public static object GetTerminalAttribute(Lexic.Token token)
            {
                if(token.Key == Lexic.AtomTypes.String)                
                    return token.Value.Substring(1, token.Value.Length - 2);
                if (token.Key == Lexic.AtomTypes.Integer)
                    return int.Parse(token.Value);
                if (token.Key == Lexic.AtomTypes.Decimal)
                    return double.Parse(token.Value);
                return null;
            }
           
            private static T Expect<T>(object obj)
            {                
                if (!(obj is T t))
                    throw new InvalidOperationException($"Expected {typeof(T)}, got {obj?.GetType()?.ToString() ?? "null"}");
                return t;
            }

            public static object CreateParseAttribute(Rule<Lexic.Token> rule, object[] attributes)
            {
                var ruleTypeMatch = Rules.Where(_ => rule.WeakEquals(_.Value)).Select(_ => _.Key).ToArray();
                if (!ruleTypeMatch.Any())
                    throw new InvalidOperationException($"Rule not found: {rule}");

                var ruleType = ruleTypeMatch[0];

                switch (ruleType)
                {
                    case RuleType.EmptyDict:
                        return new Dictionary<string, object>();                                                
                    case RuleType.Dict:
                        {                            
                            var keyValuePairs = Expect<KeyValuePair<string, object>[]>(attributes[1]);
                            return keyValuePairs.ToDictionary(_ => _.Key, _ => _.Value);
                        }                        
                    case RuleType.KeyValuePairsList:
                        {                            
                            var keyValuePair = Expect<KeyValuePair<string, object>>(attributes[0]);
                            var keyValuePairs = Expect<KeyValuePair<string, object>[]>(attributes[2]);
                            return new[] { keyValuePair }.Concat(keyValuePairs).ToArray();
                        }
                    case RuleType.KeyValuePairsListOneItem:
                        {
                            var keyValuePair = Expect<KeyValuePair<string, object>>(attributes[0]);
                            return new[] { keyValuePair };
                        }
                    case RuleType.KeyValuePair:
                        {
                            var key = Expect<string>(attributes[0]);
                            var value = attributes[2];
                            return new KeyValuePair<string, object>(key, value);
                        }
                    case RuleType.RValTrue: return true;                        
                    case RuleType.RValFalse: return false;                        
                    case RuleType.RValNone: return null;                        
                    case RuleType.RValDict: return Expect<Dictionary<string, object>>(attributes[0]);                        
                    case RuleType.RValString: return Expect<string>(attributes[0]);                        
                    case RuleType.RValInteger: return Expect<int>(attributes[0]);                        
                    case RuleType.RValDecimal: return Expect<double>(attributes[0]);                                                
                    case RuleType.RValTupleEmpty: return new object[0];

                    case RuleType.RValTuple:                                                
                    case RuleType.RValTupleCommaEnded:
                        return Expect<object[]>(attributes[1]);                        
                    case RuleType.RValsList:                        
                        var head = attributes[0];
                        var tail = Expect<object[]>(attributes[2]);
                        return new[] { head }.Concat(tail).ToArray();                        
                    case RuleType.RValsListOneItem: return new[] { attributes[0] };                        
                }

                return null;
            }
        }
    }
}
