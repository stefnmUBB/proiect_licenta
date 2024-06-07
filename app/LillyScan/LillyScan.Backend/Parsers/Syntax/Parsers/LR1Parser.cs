using EsotericDevZone.Parsers.Syntax.Parsers.LR1;
using LillyScan.Backend.Parsers;
using LillyScan.Backend.Parsers.Syntax;
using LillyScan.Backend.Parsers.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace EsotericDevZone.Parsers.Syntax.Parsers
{
    public class LR1Parser<T>
    {
        public Grammar<T> Grammar { get; }
        private LR1AnalysisTable<T> AnalysisTable { get; }

        public LR1Parser(Grammar<T> grammar)
        {
            AnalysisTable = new LR1AnalysisTable<T>(grammar);
            Grammar = AnalysisTable.Grammar;
        }

        public void PrintCanonicalCollection(TextWriter w = null)
        {
            w?.WriteLine(AnalysisTable.CanonicalCollection);
        }

        public DisplayTable DisplayAnalysisTable => AnalysisTable.GetDisplayTable();

        private bool IsTestedForConflicts = false;
        public void TestForConflicts()
        {
            if (IsTestedForConflicts) return;
            var conflicts = AnalysisTable.FindConflicts();
            var asText = conflicts
                .Select(_ => $"{_.Rules.JoinToString(Environment.NewLine)}, {_.ConflictType} encountering '{_.Prediction}'")
                .JoinToString(Environment.NewLine);

            if (conflicts.Length > 0)
            {
                throw new ConflictGrammarException("Conflicts found:" + Environment.NewLine + asText);
            }
            IsTestedForConflicts = true;
        }        

        internal class StackRuleComponent : IParserStackElement
        {
            public RuleComponent<T> RuleComponent { get; }
            public StackRuleComponent(RuleComponent<T> ruleComponent) => RuleComponent = ruleComponent;
            public override string ToString() => RuleComponent?.ToString();            

            public class NonTerminal<U> : StackRuleComponent
            {
                public U Value { get; }
                public NonTerminal(RuleComponent<T> ruleComponent, U value) : base(ruleComponent)
                {
                    Value = value;
                }                
            }
        }

        public ParseResult<U> Parse<U>(T[] input, Func<Rule<T>, U[], U> attr = null, Func<T, U> terminalAttr = null,
            bool verbose = false, TextWriter log = null)
        {
            log = verbose ? (log ?? Console.Out) : null;
            attr = attr ?? new Func<Rule<T>, U[], U>((r, u) => default);
            terminalAttr = terminalAttr ?? new Func<T, U>(t => default);
            var workStack = new Stack<IParserStackElement>(new IParserStackElement[] { new StateId(0) });
            var outStack = new Stack<object>();

            LR1AnalysisTable<T>.TableItem getNextAction(RuleComponent<T> s)
            {
                var workTop = workStack.Peek();
                if (!(workTop is StateId lastId)) return null;

                var next = AnalysisTable[lastId.Id, s];
                if (next.Length == 0) return null;
                return next[0];
            }

            LR1AnalysisTable<T>.TableItem push(StackRuleComponent s)
            {
                var next = getNextAction(s.RuleComponent);
                if (next == null) return null;
                if (!next.IsShift) return null;
                workStack.Push(s);
                workStack.Push(new StateId(next.Value));
                return next;
            }

            bool? pop(int N, out StackRuleComponent[] poped)
            {
                poped = new StackRuleComponent[0];
                if (workStack.Count < 2 * N + 1) return null;

                var lPoped = new List<StackRuleComponent>();
                for (int i = 2 * N; i > 0; i--)
                {
                    var it = workStack.Pop();
                    if(it is StackRuleComponent comp) lPoped.Add(comp);
                }
                lPoped.Reverse();
                poped = lPoped.ToArray();
                return true;
            }

            LR1AnalysisTable<T>.TableItem doShift()
            {
                var c = input.Length > 0 ? new Terminal<T>(input[0]) : null as RuleComponent<T>;
                var pushResult = push(new StackRuleComponent(c));
                if (pushResult == null) return null;
                if (input.Length > 0) input = input.Skip(1).ToArray();
                return pushResult;
            }

            LR1AnalysisTable<T>.TableItem doReduce()
            {
                var c = input.Length > 0 ? new Terminal<T>(input[0]) : null as RuleComponent<T>;
                var next = getNextAction(c);
                if (next == null) return null;
                if (!next.IsReduce) return null;
                var rule = Grammar.pRules[next.Value];
                var popResult = pop(rule.RightMember.Length, out StackRuleComponent[] poped);
                if (popResult == null) return null;

                var attributes = poped                    
                    .Select(_ => _ is StackRuleComponent.NonTerminal<U> uHolder ? uHolder.Value : terminalAttr((_.RuleComponent as Terminal<T>).Value))
                    .ToArray();
                
                var pushResult = push(new StackRuleComponent.NonTerminal<U>(rule.LeftMember, attr(rule, attributes)));
                if (pushResult == null)
                {
                    poped.ForEach(_ => push(_));
                    //rule.RightMember.ForEach(_ => push(_));
                    return null;
                }
                outStack.Push(rule.Id);
                return next;
            }

            LR1AnalysisTable<T>.TableItem doAccept()
            {
                var c = input.Length > 0 ? new Terminal<T>(input[0]) : null as RuleComponent<T>;
                var next = getNextAction(c);
                if (next == null) return null;
                if (!next.IsAccepted) return null;
                return next;
            }

            string doNext() => doShift()?.ToString() ?? doReduce()?.ToString() ?? doAccept()?.ToString() ?? "err";

            StringBuilder sb = new StringBuilder();

            while (true)
            {
                sb.Append("(");
                sb.Append(workStack.Reverse().JoinToString(""));
                sb.Append(", ");
                sb.Append($"`{input.JoinToString(",")}$`");
                sb.Append(", ");
                sb.Append(outStack.Reverse().JoinToString(" "));
                sb.Append(") |-");

                var r = doNext();
                sb.AppendLine(r);

                log?.WriteLine(sb.ToString());
                sb.Clear();

                if(r=="err")
                {
                    return new ParseResult<U>.Error("Syntax", default, "Parse failed", -1);
                }

                if (r == "a")
                {
                    workStack.Pop(); // $1
                    var S = workStack.Pop() as StackRuleComponent.NonTerminal<U>;
                    return new ParseResult<U>.Success("Syntax", S.Value);
                }
            }            
        }

    }
}
