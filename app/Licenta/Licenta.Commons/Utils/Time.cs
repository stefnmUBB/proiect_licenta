using System;
using System.Diagnostics;

namespace HelpersCurveDetectorDataSetGenerator.Commons.Utils
{
    public static class Time
    {
        public static long Measure(Action action)
        {
            Stopwatch stopwatch = Stopwatch.StartNew();
            action();
            stopwatch.Stop();
            return stopwatch.ElapsedMilliseconds;
        }
    }
}
