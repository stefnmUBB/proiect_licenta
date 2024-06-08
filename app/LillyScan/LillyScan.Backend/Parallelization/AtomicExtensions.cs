namespace LillyScan.Backend.Parallelization
{
    public static class AtomicExtensions
    {
        public static void Increment(this Atomic<int> at)
        {
            lock (at.locker)
                at.Value++;           
        }

        public static int PreIncrement(this Atomic<int> a)
        {
            return a.With(v => v + 1);
        }

    }
}
