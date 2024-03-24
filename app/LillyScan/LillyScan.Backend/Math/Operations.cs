using System;
using System.Collections.Generic;
using System.Text;

namespace LillyScan.Backend.Math
{
    public static class Operations
    {
        public static Func<T, T, T> Sum<T>() => (t1, t2) => (T)((dynamic)t1 + t2);
    }
}
