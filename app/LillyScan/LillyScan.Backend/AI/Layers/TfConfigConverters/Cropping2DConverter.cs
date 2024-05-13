using System;
using System.Collections.Generic;
using System.Text;

namespace LillyScan.Backend.AI.Layers.TfConfigConverters
{
    public static class Cropping2DConverter
    {
        public static (int, int, int, int) Convert(object[] input)
        {
            if (input.Length == 2 && input[0] is object[] tb && input[1] is object[] lr && tb.Length == 2 && lr.Length == 2
                && tb[0] is int t && tb[1] is int b && lr[0] is int l && lr[1] is int r)
                return (t, b, l, r);
            throw new TfConfigConverterFailedException(input, typeof((int, int, int, int)));
        }
    }
}
