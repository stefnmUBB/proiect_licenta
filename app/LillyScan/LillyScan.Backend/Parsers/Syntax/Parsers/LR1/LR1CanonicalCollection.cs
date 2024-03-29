using LillyScan.Backend.Parsers.Syntax;
using System.Collections.Generic;
using System.Linq;

namespace EsotericDevZone.Parsers.Syntax.Parsers.LR1
{
    internal class LR1CanonicalCollection<T> : CanonicalCollection<T>
    {
        private static new void BuildClosure(Grammar<T> g, CanonicalCollectionNode<T> node)
        {
            var tmpElems = node.Elements.ToList();
            var newElems = new List<AnalysisElement<T>>();

            //Console.WriteLine("CLOSURE!!!");
            //Console.WriteLine($"  intTmpElems: {tmpElems.JoinToString(" ")}");

            while (true)
            {
                foreach (var elem in tmpElems)
                {
                    var next = elem.GetAfterDot();
                    if (next == null) continue;

                    if (!(next is NonTerminal<T> nonTerminal)) continue;                    
                    
                    var beta = g.First1Sequence(elem.Rule.RightMember.Skip(elem.DotPosition + 1).ToArray());
                    //Console.Write($"  F1Seq of `{elem.Rule.RightMember.Skip(elem.DotPosition + 1).JoinToString(" ")}` ");
                    //Console.WriteLine($"is {beta.JoinToString(" ")}");                    
                    if (beta.Count == 0 || beta.Contains(Prediction1<T>.Empty()))
                        beta.AddRange(elem.UPredictions.ToList());
                    beta.RemoveAll(p => object.Equals(p, Prediction1<T>.Empty()));
                    beta = beta.Distinct().ToList();
                    //Console.WriteLine($"  Final beta = {beta.JoinToString(" ")}");

                    foreach (var rule in g.GetDerivationsOf(nonTerminal))
                    {
                        foreach (var b in beta)
                        {
                            var a_elem = new AnalysisElement<T>(rule, 0, b);

                            if (!tmpElems.Contains(a_elem))
                                newElems.Add(a_elem);
                        }
                    }
                }

                tmpElems = tmpElems.Concat(newElems).Distinct().ToList();
                //Console.WriteLine($"  newTmpElems: {tmpElems.JoinToString(" ")}");

                if (newElems.Count == 0)
                    break;
                newElems.Clear();
            }

            node.Closure.Clear();
            node.Closure.AddRange(tmpElems);
        }

        private static CanonicalCollectionNode<T> Goto(CanonicalCollectionNode<T> n, RuleComponent<T> r)
        {
            var elems = n.Closure.Where(a => object.Equals(a.GetAfterDot(), r)).Select(_ => _.AdvanceDot()).ToList();
            return new CanonicalCollectionNode<T>(n.Grammar, -1, elems);
        }

        private static AnalysisElement<T> FirstElement(Grammar<T> g)
        {
            return new AnalysisElement<T>(g.pRules[0], 0, Prediction1<T>.EndOfWord());
        }

        public LR1CanonicalCollection(Grammar<T> grammar) : base(grammar, BuildClosure, Goto, FirstElement) { }        
    }
}
