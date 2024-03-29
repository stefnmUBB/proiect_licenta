using LillyScan.Backend.Parsers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LillyScan.Backend.Parsers.Syntax
{
    public class Rule<T>
    {
        public NonTerminal<T> LeftMember { get; }
        public RuleComponent<T>[] RightMember { get; }
        public int Id { get; }

        public Rule(NonTerminal<T> leftMember, RuleComponent<T>[] rightMember, int id = -1)
        {
            LeftMember = leftMember;
            RightMember = rightMember;
            Id = id;
        }

        public bool WeakEquals(Rule<T> rule)
        {
            return EqualityComparer<NonTerminal<T>>.Default.Equals(LeftMember, rule.LeftMember) &&
                   RightMember.SequenceEqual(rule.RightMember);
        }

        public override bool Equals(object obj)
        {
            return obj is Rule<T> rule &&
                   EqualityComparer<NonTerminal<T>>.Default.Equals(LeftMember, rule.LeftMember) &&
                   RightMember.SequenceEqual(rule.RightMember) &&
                   Id == rule.Id;
        }

        public override int GetHashCode()
        {
            int hashCode = -355120259;
            hashCode = hashCode * -1521134295 + EqualityComparer<NonTerminal<T>>.Default.GetHashCode(LeftMember);
            hashCode = hashCode * -1521134295 + RightMember.GetSequenceHashCodeSum();
            hashCode = hashCode * -1521134295 + Id.GetHashCode();
            return hashCode;
        }

        public override string ToString()
        {
            var str = $"{LeftMember} -> {RightMember.JoinToString(" ")}";
            return Id > 0 ? $"{str} ({Id})" : str;
        }
    }
}
