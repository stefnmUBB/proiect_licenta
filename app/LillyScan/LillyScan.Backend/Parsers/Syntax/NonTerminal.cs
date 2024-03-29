using System;
using System.Collections.Generic;
using System.Text;

namespace LillyScan.Backend.Parsers.Syntax
{
    public sealed class NonTerminal<T> : RuleComponent<T>
    {
        public string Name { get; }
        public NonTerminal(string name) : base() => Name = name;
        public override bool Equals(object obj) => obj is NonTerminal<T> terminal && Name == terminal.Name;
        public override int GetHashCode() => 539060726 + EqualityComparer<string>.Default.GetHashCode(Name);

        public override string ToString() => Name;
    }
}
