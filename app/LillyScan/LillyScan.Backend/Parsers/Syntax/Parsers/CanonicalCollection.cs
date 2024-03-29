using LillyScan.Backend.Parsers;
using LillyScan.Backend.Parsers.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EsotericDevZone.Parsers.Syntax.Parsers
{
    internal class CanonicalCollection<T>
    {
        public Grammar<T> Grammar { get; }
        public Action<Grammar<T>, CanonicalCollectionNode<T>> BuildClosure;
        Func<CanonicalCollectionNode<T>, RuleComponent<T>, CanonicalCollectionNode<T>> Goto;

        public List<CanonicalCollectionNode<T>> States { get; } = new List<CanonicalCollectionNode<T>>();
        public Dictionary<(CanonicalCollectionNode<T> State, RuleComponent<T> Symbol), CanonicalCollectionNode<T>>
            Transitions
        { get; }
            = new Dictionary<(CanonicalCollectionNode<T> State, RuleComponent<T> Symbol), CanonicalCollectionNode<T>>();

        int TransitionsCount = 0;

        public CanonicalCollection(Grammar<T> grammar,
            Action<Grammar<T>, CanonicalCollectionNode<T>> buildClosure,
            Func<CanonicalCollectionNode<T>, RuleComponent<T>, CanonicalCollectionNode<T>> @goto,
            Func<Grammar<T>, AnalysisElement<T>> firstElement)
        {
            Grammar = grammar.Enrich();
            BuildClosure = buildClosure;
            Goto = @goto;

            var I0 = new CanonicalCollectionNode<T>(Grammar, 0, new List<AnalysisElement<T>> { firstElement(Grammar) });
            States.Add(I0);

            buildClosure(Grammar, I0);

            int trCount = TransitionsCount;
            int stCount = 0;

            while (trCount != TransitionsCount || stCount != States.Count)
            {
                trCount = TransitionsCount;
                stCount = States.Count;

                foreach (var node in States.ToArray())
                {
                    foreach (var X in node.GetTransitions())
                    {
                        var newNode = GetOrCreate(Goto(node, X));
                        if (!Transitions.ContainsKey((node, X)))
                        {
                            Transitions[(node, X)] = newNode;
                            TransitionsCount++;
                        }
                        else
                        {
                            Validators.Assert("Invalid transition?", object.Equals(newNode, Transitions[(node, X)]));
                        }
                    }

                }
            }
        }

        private CanonicalCollectionNode<T> GetOrCreate(CanonicalCollectionNode<T> state)
        {
            var equiv = States.Where(_ => _.IsEquivalentTo(state)).ToList();
            if (equiv.Count == 0)
            {
                var newState = new CanonicalCollectionNode<T>(Grammar, States.Count, state.Elements);
                States.Add(newState);
                BuildClosure(Grammar, newState);
                return newState;
            }
            return equiv.First();
        }

        public override string ToString()
        {
            var sb = new StringBuilder();

            foreach (var s in States) sb.AppendLine(s.ToString());

            foreach (var t in Transitions)
                sb.AppendLine($"(I{t.Key.State.Id}, {t.Key.Symbol}) -> I{t.Value.Id}");

            return sb.ToString();
        }

    }
}
