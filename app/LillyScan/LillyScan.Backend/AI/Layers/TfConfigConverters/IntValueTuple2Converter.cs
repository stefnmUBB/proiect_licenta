using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace LillyScan.Backend.AI.Layers.TfConfigConverters
{
    public static class IntValueTuple2Converter
    {
        public static (int,int) Convert(object[] input)
        {
            if(input.Length==2 && input[0] is int a && input[1] is int b)            
                return (a, b);
            throw new TfConfigConverterFailedException(input, typeof((int, int)));
        }
    }
}
