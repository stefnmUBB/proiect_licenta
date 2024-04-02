using System;
using System.IO;
using System.Text;

namespace LillyScan.Backend.AI.Math
{
    public static partial class TensorExtensions
    {
        public static Tensor Print(this Tensor t, string message = "", TextWriter writer = null)
        {
            writer = writer ?? Console.Out;

            var sb = new StringBuilder();
            var iter = new int[t.Shape.DimensionsCount];

            sb.Append(message);
            sb.AppendLine($"Tensor of shape {t.Shape}:");
            if (t.IsScalar)
            {
                sb.Append(t.BuffAccessor[0]);
            }
            else
            {

                sb.Append(new string('[', t.Shape.DimensionsCount));
                while (iter[0] < t.Shape[0])
                {
                    var elem = t.BuffAccessor[t.Shape.GetBufferIndex(iter)];
                    sb.Append(elem);
                    //Console.WriteLine(iter.JoinToString(" "));
                    int b = 0;
                    for (int i = t.Shape.DimensionsCount - 1, c = 1; i >= 0 && c > 0; i--)
                    {
                        iter[i]++;
                        c = 0;
                        if (i > 0 && iter[i] == t.Shape[i])
                        {
                            iter[i] = 0;
                            c = 1;
                            b++;
                            sb.Append("]");
                        }
                    }
                    if (b == 0)
                        sb.Append(" ");
                    if (b > 0 && iter[0] < t.Shape[0])
                        sb.Append("\n" + new string('[', b));
                }
                sb.Append(']');
            }
            writer.WriteLine(sb);            

            return t;
        }
    }
}
