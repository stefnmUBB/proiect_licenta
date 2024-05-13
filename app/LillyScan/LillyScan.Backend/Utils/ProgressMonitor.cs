using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;

namespace LillyScan.Backend.Utils
{
    public class ProgressMonitor
    {
        public CancellationToken? CancellationToken = null;

        public ProgressMonitor(CancellationToken? cancellationToken = null)
        {
            CancellationToken = cancellationToken;
        }

        class TaskProgress
        {
            public string Name = "";
            public int ExpectedSteps = -1;
            public int FinishedSteps = 0;
            public float Weight = 1;
            public float CurrentProgress = 0;
        }

        private readonly List<TaskProgress> Tasks = new List<TaskProgress>();

        [MethodImpl(MethodImplOptions.Synchronized)]
        public void PushTask(string name, int expectedSteps = -1, bool throwIfCancelled = true)
        {
            float weight;
            var tasksCount = Tasks.Count;
            if(tasksCount==0)
            {
                weight = expectedSteps < 0 ? -1 : 1.0f / expectedSteps;
            }
            else
            {
                var lastTask = Tasks[tasksCount - 1];
                weight = (lastTask.Weight < 0 || expectedSteps < 0) ? -1 : lastTask.Weight / expectedSteps;
            }

            Tasks.Add(new TaskProgress { Name = name, ExpectedSteps = expectedSteps, Weight = weight });
            TriggerProgressChanged();
            if (throwIfCancelled)
                CancellationToken?.ThrowIfCancellationRequested();
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public void AdvanceOneStep(bool throwIfCancelled = true)
        {            
            var lastTask = Tasks[Tasks.Count - 1];
            lastTask.FinishedSteps++;
            lastTask.CurrentProgress = lastTask.FinishedSteps * 100 / lastTask.ExpectedSteps;
            TriggerProgressChanged();
            if (throwIfCancelled)
                CancellationToken?.ThrowIfCancellationRequested();
        }

        public bool TaskCanceled => CancellationToken?.IsCancellationRequested ?? false;

        [MethodImpl(MethodImplOptions.Synchronized)]
        public void PopTask()
        {
            Tasks.RemoveAt(Tasks.Count - 1);
        }

        private void TriggerProgressChanged()
        {
            float progress = 0;
            bool updateProgress = true;
            string description = "";
            for (int i = 0, cnt = Tasks.Count; i < cnt; i++) 
            {
                var t = Tasks[i];
                if(t.Weight<0)                
                    updateProgress = false;
                if (updateProgress)
                    progress += t.Weight * t.FinishedSteps * 100;
                description += $"/{t.Name ?? ""}";
            }
            ProgressChanged?.Invoke(this, progress, description);
        }

        public delegate void OnProgressChanged(object sender, float progress, string description);
        public event OnProgressChanged ProgressChanged;


    }
}
