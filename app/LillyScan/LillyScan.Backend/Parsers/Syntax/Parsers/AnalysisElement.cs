using LillyScan.Backend.Parsers;
using LillyScan.Backend.Parsers.Syntax;
using System.Collections.Generic;
using System.Linq;

namespace EsotericDevZone.Parsers.Syntax.Parsers
{
    internal class AnalysisElement<T>
    {
        public Rule<T> Rule { get; }
        public int DotPosition { get; }
        public Prediction1<T>[] UPredictions { get; }

        public AnalysisElement(Rule<T> rule, int dotPosition, params Prediction1<T>[] uPredictions)
        {
            Validators.Assert(
                $"Invalid dot position: {dotPosition} (rule length is {rule.RightMember.Length})",
                0 <= dotPosition && dotPosition <= rule.RightMember.Length);

            Rule = rule;            
            DotPosition = dotPosition;
            UPredictions = uPredictions.OrderBy(_ => _.ToString() + _.Value).ToArray();
        }

        public RuleComponent<T> GetAfterDot()
        {
            if (DotPosition == Rule.RightMember.Length)
                return null;
            return Rule.RightMember[DotPosition];
        }


        public AnalysisElement<T> AdvanceDot()
        {
            return new AnalysisElement<T>(Rule, DotPosition + 1, UPredictions);
        }

        public override bool Equals(object obj)
        {
            return obj is AnalysisElement<T> element &&
                   EqualityComparer<Rule<T>>.Default.Equals(Rule, element.Rule) &&
                   DotPosition == element.DotPosition &&
                   UPredictions.SequenceEqual(element.UPredictions);
        }

        public override int GetHashCode()
        {
            int hashCode = 1123170682;
            hashCode = hashCode * -1521134295 + EqualityComparer<Rule<T>>.Default.GetHashCode(Rule);
            hashCode = hashCode * -1521134295 + DotPosition.GetHashCode();
            return hashCode;
        }

        public bool IsDotAtEnd => DotPosition == Rule.RightMember.Length;

        public override string ToString()
        {
            var r = $"{Rule.LeftMember} -> ";
            for (int i = 0; i < Rule.RightMember.Length; i++)
            {
                if (DotPosition == i)
                    r += ". ";
                r += Rule.RightMember[i].ToString() + " ";
            }
            if (DotPosition == Rule.RightMember.Length)
                r += ". ";
            return $"[{r}, {UPredictions.JoinToString(" ")}]";
        }
    }
}
