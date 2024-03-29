using LillyScan.Backend.Parsers;
using System;
using System.Linq;
using System.Text;

namespace LillyScan.Backend.Parsers.Utils
{
    public class DisplayTable
    {
        public string[] ColumnHeaders { get; }
        public string[][] Rows { get; }

        public DisplayTable(string[] columnHeaders, string[][] rows)
        {
            ColumnHeaders = columnHeaders;
            Rows = rows;
        }
        public override string ToString()
        {
            var sb = new StringBuilder();

            var colWidth = ColumnHeaders.Select(_ => System.Math.Max(2, (_ ?? "null").Length)).ToArray();

            foreach (var row in Rows)
            {
                if (row.Length != ColumnHeaders.Length)
                    throw new InvalidOperationException($"Columns count {ColumnHeaders.Length} different from row size {row.Length}");
                for (int j = 0; j < row.Length; j++)
                    colWidth[j] = System.Math.Max(colWidth[j], (row[j] ?? "null").Length);
            }

            var line = ColumnHeaders.Zip(colWidth, (s, i) => (s ?? "null").PadLeft(i, ' ')).JoinToString(" | ");
            sb.AppendLine(line);

            foreach (var row in Rows)
            {
                line = row.Zip(colWidth, (s, i) => (s ?? "null").PadLeft(i, ' ')).JoinToString(" | ");
                sb.AppendLine(line);
            }

            return sb.ToString();
        }

    }
}
