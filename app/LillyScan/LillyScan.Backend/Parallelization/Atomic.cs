using System;

namespace LillyScan.Backend.Parallelization
{
    public class Atomic<T>
    {        

        internal T Value;
        internal readonly object locker = new object();

        public Atomic(T initialValue = default)
        {
            Value = initialValue;
        }

        public static implicit operator Atomic<T>(T value) => new Atomic<T>(value);

        public T Get()
        {
            lock (locker) return Value;
        }

        public void Set(T value)
        {
            lock (locker) Value = value;
        }

        public void With(Action<T> action)
        {
            lock (locker) action(Value);
        }

        public override string ToString() => Get()?.ToString() ?? "null";        

    }
}
