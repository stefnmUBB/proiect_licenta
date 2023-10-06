using System;

namespace Licenta.Commons.Parallelization
{
    public class ParallelForLoop
    {
        public static void Run(Action<int> iterationLogic, int start, int end, int step = 1, int tasksCount = 3, int maxTasksLength = -1)
        {
            var tm = new TaskManager(maxTasksLength < 0 ? tasksCount : maxTasksLength).RunAsync();
            var tg = tm.CreateTaskGroup();

            Action oneTask(int i0) => new Action(() =>
            {
                for (int i = i0; i < end; i += step * tasksCount)
                    iterationLogic(i);
            });

            for (int t = 0; t < tasksCount; t++)
            {
                tg.AddTask(oneTask(start + t * step));
            }

            tg.WaitAll();
            tm.Stop();
        }
    }
}
