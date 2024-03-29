using LillyScan.Backend.Parsers;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace LillyScan.Backend.Parsers.Syntax
{
    public class Grammar<T>
    {
        internal Rule<T>[] pRules;
        internal NonTerminal<T> pStartSymbol;

        internal Terminal<T>[] pTerminals;
        internal NonTerminal<T>[] pNonTerminals;

        public Grammar(IEnumerable<Rule<T>> rules, NonTerminal<T> startSymbol = null)
        {
            pRules = rules.ToArray();
            Validators.Assert("Grammar must contain at least one rule", pRules.Length > 0);
            pStartSymbol = startSymbol ?? pRules.First().LeftMember;
            Validators.Assert(
                "Start symbol must be defined by a rule",
                pRules.Any(r => Equals(r.LeftMember, pStartSymbol)));

            pNonTerminals = pRules.Select(r => r.LeftMember).Distinct().ToArray();
            pTerminals = pRules.SelectMany(r => r.RightMember)
                .SelectByType<Terminal<T>>()
                .Distinct()
                .ToArray();

            var undefinedNonTerminals = pRules.SelectMany(r => r.RightMember).Distinct()
                .Where(c => c is NonTerminal<T> && !pNonTerminals.Contains(c))
                .Select(c => c as NonTerminal<T>)
                .ToArray();

            Validators.Assert(
                $"No rules to define non-terminal symbols: {undefinedNonTerminals.Select(_ => _.Name).JoinToString(", ")}",
                undefinedNonTerminals.Length == 0
                );

            BuildFirst1Table();
            BuildFollow1Table();
        }

        internal Dictionary<NonTerminal<T>, HashSet<Prediction1<T>>> First1Table = new Dictionary<NonTerminal<T>, HashSet<Prediction1<T>>>();

        public void PrintFirst1()
        {
            foreach (var A in First1Table.Keys)
            {
                Console.WriteLine($"{A} : {First1Table[A].JoinToString(", ")}");
            }
        }

        internal void BuildFirst1Table()
        {
            First1Table.Clear();
            foreach (var A in pNonTerminals)
            {
                var rules = GetDerivationsOf(A);
                First1Table[A] = new HashSet<Prediction1<T>>(from r in rules
                                                             where r.RightMember.Length > 0
                                                             let c = r.RightMember.First()
                                                             where c is Terminal<T>
                                                             select Prediction1<T>.Of(c as Terminal<T>));
                if (rules.Any(r => r.RightMember.Length == 0))
                    First1Table[A].Add(Prediction1<T>.Empty());
            }

            bool running = true;
            while (running)
            {
                var newTable = new Dictionary<NonTerminal<T>, HashSet<Prediction1<T>>>();
                foreach (var A in pNonTerminals)
                    newTable[A] = new HashSet<Prediction1<T>>();
                foreach (var r in pRules)
                {
                    foreach (var f1 in First1Sequence(r.RightMember.ToArray()))
                        newTable[r.LeftMember].Add(f1);
                }
                running = false;
                foreach (var A in pNonTerminals)
                {
                    if (First1Table[A].Count != newTable[A].Count)
                    {
                        running = true;
                        break;
                    }
                }
                First1Table = newTable;
            }
        }

        public List<Prediction1<T>> First1Sequence(params RuleComponent<T>[] components)
        {
            var set = new HashSet<Prediction1<T>>();

            foreach (var comp in components)
            {
                if (comp is Terminal<T> terminal)
                {
                    set.Add(Prediction1<T>.Of(terminal));
                    return set.ToList();
                }
                if (comp is NonTerminal<T> nonTerminal)
                {
                    var f1 = First1Table[nonTerminal];
                    foreach (var c in f1.Where(x => !x.IsEmpty))
                        set.Add(c);
                    if (!f1.Contains(Prediction1<T>.Empty()))
                        return set.ToList();
                }
            }
            set.Add(Prediction1<T>.Empty());
            return set.ToList();
        }

        private readonly Dictionary<NonTerminal<T>, HashSet<Prediction1<T>>> Follow1Table = new Dictionary<NonTerminal<T>, HashSet<Prediction1<T>>>();

        public void BuildFollow1Table()
        {
            foreach (var A in pNonTerminals)
                Follow1Table[A] = new HashSet<Prediction1<T>>();
            Follow1Table[pStartSymbol].Add(Prediction1<T>.EndOfWord());

            int newlyAdded = 1;
            while (newlyAdded > 0)
            {
                newlyAdded = 0;

                foreach (var rule in pRules)
                {
                    var A = rule.LeftMember;
                    for (int i = 0; i < rule.RightMember.Length; i++)
                    {
                        if (!(rule.RightMember[i] is NonTerminal<T> nonTerminal)) continue;

                        var B = nonTerminal;
                        var beta = rule.RightMember.Skip(i + 1).ToArray();
                        var first1Beta = First1Sequence(beta);

                        foreach (var p in first1Beta)
                        {
                            if (p.IsEmpty) continue;
                            if (!Follow1Table[B].Contains(p))
                            {
                                newlyAdded++;
                                Follow1Table[B].Add(p);
                            }
                        }

                        if (first1Beta.Contains(Prediction1<T>.Empty()))
                        {
                            foreach (var p in Follow1Table[A])
                            {
                                if (!Follow1Table[B].Contains(p))
                                {
                                    newlyAdded++;
                                    Follow1Table[B].Add(p);
                                }
                            }
                        }
                    }
                }
            }
        }

        public void PrintFollow1()
        {
            foreach (var A in Follow1Table.Keys)
            {
                Console.WriteLine($"{A} : {Follow1Table[A].JoinToString(", ")}");
            }
        }

        public IEnumerable<Rule<T>> GetDerivationsOf(NonTerminal<T> n) => pRules.Where(r => Equals(r.LeftMember, n));

        private NonTerminal<T> GetUniqueStartSymbol()
        {
            int id = 0;
            while (pNonTerminals.Any(_ => _.Name == $"S{id}"))
                id++;
            return new NonTerminal<T>($"S{id}");
        }

        public Grammar<T> Enrich(NonTerminal<T> _newStartSymbol = null)
        {
            var newStartSymbol = _newStartSymbol ?? GetUniqueStartSymbol();
            return new Grammar<T>(
                new[] { new Rule<T>(newStartSymbol, new[] { pStartSymbol }) }.Concat(pRules)
                .Select((r, i) => new Rule<T>(r.LeftMember, r.RightMember, i)),
                newStartSymbol);
        }


        public static GrammarBuilder<T> CreateBuilder() => new GrammarBuilder<T>();

        public override string ToString() => pRules.JoinToString(";\r\n");

    }
}
