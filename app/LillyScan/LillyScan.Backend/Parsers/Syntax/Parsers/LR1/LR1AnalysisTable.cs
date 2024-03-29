using LillyScan.Backend.Parsers;
using LillyScan.Backend.Parsers.Syntax;
using LillyScan.Backend.Parsers.Utils;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace EsotericDevZone.Parsers.Syntax.Parsers.LR1
{
    internal class LR1AnalysisTable<T>
    {
        public class TableColumn
        {
            public RuleComponent<T> Component { get; }
            public bool IsEndOfWord => Component == null;

            private TableColumn(RuleComponent<T> component)
            {
                Component = component;
            }

            public static TableColumn Of(RuleComponent<T> component) => new TableColumn(component);
            public static TableColumn Of(Prediction1<T> pred) =>
                new TableColumn(pred.IsEndOfWord ? null as RuleComponent<T> : new Terminal<T>(pred.Value));
            public static TableColumn EndOfWord() => new TableColumn(null);

            public override string ToString()
            {
                return IsEndOfWord ? "$" : Component.ToString();
            }

            public override bool Equals(object obj)
            {
                return obj is TableColumn column &&
                       EqualityComparer<RuleComponent<T>>.Default.Equals(Component, column.Component) &&
                       IsEndOfWord == column.IsEndOfWord;
            }

            public override int GetHashCode()
            {
                int hashCode = 1831148343;
                hashCode = hashCode * -1521134295 + (Component?.GetHashCode() ?? 0);
                hashCode = hashCode * -1521134295 + IsEndOfWord.GetHashCode();
                return hashCode;
            }
        }

        public class TableItem
        {
            public string Type { get; }
            public int Value { get; }

            public TableItem(string type, int value)
            {
                Type = type;
                Value = value;
            }

            public static TableItem Shift(int value) => new TableItem("s", value);
            public static TableItem Reduce(int value) => new TableItem("r", value);
            public static TableItem Accepted() => new TableItem("a", -1);

            public override bool Equals(object obj)
            {
                return obj is TableItem item &&
                       Type == item.Type &&
                       Value == item.Value;
            }

            public override int GetHashCode()
            {
                int hashCode = 1265339359;
                hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Type);
                hashCode = hashCode * -1521134295 + Value.GetHashCode();
                return hashCode;
            }

            public bool IsShift => Type == "s";
            public bool IsReduce => Type == "r";
            public bool IsAccepted => Type == "a";

            public override string ToString() => Type + (Value >= 0 ? Value.ToString() : "");
        }

        public Grammar<T> Grammar { get; }
        public LR1CanonicalCollection<T> CanonicalCollection { get; }
        private readonly Dictionary<(int, TableColumn), HashSet<TableItem>> Table = new Dictionary<(int, TableColumn), HashSet<TableItem>>();

        public TableItem[] this[int state, RuleComponent<T> pred]
            => Table.TryGetValue((state, pred==null ? TableColumn.EndOfWord() : TableColumn.Of(pred)), out var set)
            ? set.ToArray() : new TableItem[0];

        private void AddToTable((int, TableColumn) key, TableItem item)
        {
            if (!Table.ContainsKey(key)) Table[key] = new HashSet<TableItem>();
            Table[key].Add(item);
        }

        public LR1AnalysisTable(Grammar<T> grammar)
        {
            CanonicalCollection = new LR1CanonicalCollection<T>(grammar);
            Grammar = CanonicalCollection.Grammar;

            // shift
            foreach ((var state, var sym) in CanonicalCollection.Transitions.Keys)
            {
                var key = (state.Id, TableColumn.Of(sym));
                AddToTable(key, TableItem.Shift(CanonicalCollection.Transitions[(state, sym)].Id));
            }

            // reduce/acc
            foreach (var state in CanonicalCollection.States)
            {
                foreach (var elem in state.Closure)
                {
                    if (!elem.IsDotAtEnd) continue;
                    var sym = elem.UPredictions[0];
                    if (sym.IsEndOfWord && object.Equals(elem.Rule.LeftMember, Grammar.pStartSymbol))
                    {
                        var key = (state.Id, TableColumn.Of(sym));
                        AddToTable(key, TableItem.Accepted());
                    }
                    else if (sym.IsEndOfWord || !sym.IsEmpty)
                    {
                        var key = (state.Id, TableColumn.Of(sym));
                        AddToTable(key, TableItem.Reduce(elem.Rule.Id));
                    }
                }
            }
        }


        public DisplayTable GetDisplayTable()
        {
            var columns = Grammar.pNonTerminals.Select(TableColumn.Of)
                .Concat(Grammar.pTerminals.Select(TableColumn.Of))
                .Concat(new[] { TableColumn.EndOfWord() }).ToArray();

            var dispColumns = new[] { "" }.Concat(columns.Select(_ => _.ToString())).ToArray();

            var rows = new List<string[]>();

            foreach (var state in CanonicalCollection.States)
            {
                var row = new List<string>();

                row.Add($"I{state.Id}");

                foreach (var c in columns)
                {
                    var key = (state.Id, c);
                    if (!Table.ContainsKey(key))
                        row.Add("");
                    else
                        row.Add(Table[key].JoinToString(", "));
                }

                rows.Add(row.ToArray());
            }

            return new DisplayTable(dispColumns, rows.ToArray());

        }

        public (Rule<T>[] Rules, TableColumn Prediction, string ConflictType)[] FindConflicts()
        {
            return Table.Where(_ => _.Value.Count > 1)
                .Select(_ => (CanonicalCollection.States[_.Key.Item1].Elements.Select(e => e.Rule).Distinct().ToArray(), _.Key.Item2,
                    _.Value.Select(c => c.IsShift ? "shift" : c.IsReduce ? "reduce" : c.IsAccepted ? "acc" : "err").JoinToString("/")))
                .ToArray();
        }
    }
}
