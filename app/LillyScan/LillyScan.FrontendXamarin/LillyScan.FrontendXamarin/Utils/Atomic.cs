using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Text;

namespace LillyScan.FrontendXamarin.Utils
{
    public class Atomic<T>
    {
        private T Value;
        private readonly object locker=new object();

        public Atomic(T initialValue=default) 
        {
            Value = initialValue;
        }

        public T Get()
        {
            lock (locker)
                return Value;
        }

        public void Set(T value)
        {
            lock (locker)
                Value = value;
        }

    }
}
