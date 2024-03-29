using LillyScan.Backend.Parsers;
using LillyScan.Backend.Parsers.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EsotericDevZone.Parsers.Syntax.Parsers
{
    internal class CanonicalCollectionNode<T>
    {
        public Grammar<T> Grammar { get; }
        public int Id { get; }
        public AnalysisElement<T>[] Elements { get; }
        public List<AnalysisElement<T>> Closure { get; } = new List<AnalysisElement<T>>();

        public CanonicalCollectionNode(Grammar<T> grammar, int id, IEnumerable<AnalysisElement<T>> elements)
        {
            Grammar = grammar;
            Id = id;
            Elements = elements.ToArray();
        }

        public List<RuleComponent<T>> GetTransitions()
        {
            return Closure.Select(_ => _.GetAfterDot()).Where(_ => _ != null).Distinct().ToList();
        }

        public override string ToString()
        {
            return $"State I{Id}\r\n{Closure.Select(_ => $"  {_}").JoinToString("\r\n")}";
        }

        public bool IsEquivalentTo(CanonicalCollectionNode<T> other)
        {
            var intersect_cnt = Elements.Intersect(other.Elements).Count();
            return intersect_cnt == Elements.Length && intersect_cnt == other.Elements.Length;
        }

        public override bool Equals(object obj)
        {
            return obj is CanonicalCollectionNode<T> node &&
                   Id == node.Id &&
                   Elements.SequenceEqual(node.Elements);
        }

        public override int GetHashCode()
        {
            int hashCode = 2146015724;
            hashCode = hashCode * -1521134295 + Id.GetHashCode();
            hashCode = hashCode * -1521134295 + Elements.GetSequenceHashCodeSum();
            return hashCode;
        }
    }
}
