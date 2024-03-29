using System.Collections.Generic;

namespace LillyScan.Backend.Parsers.Syntax
{
    public class Terminal<T> : RuleComponent<T>
    {
        public T Value { get; }
        public Terminal(T value) : base() => Value = value;
        public override bool Equals(object obj) => obj is Terminal<T> terminal && EqualityComparer<T>.Default.Equals(Value, terminal.Value);
        public override int GetHashCode() => -1937169414 + EqualityComparer<T>.Default.GetHashCode(Value);
        public override string ToString() => $"'{Value}'";
    }
}
