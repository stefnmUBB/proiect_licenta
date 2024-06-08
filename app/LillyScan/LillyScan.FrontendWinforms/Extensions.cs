using System;
using System.Reflection;
using System.Windows.Forms;

namespace LillyScan.FrontendWinforms
{
    public static class Extensions
    {
        public static void InvokeAction(this Control control, Action action) => control.Invoke(action);

        public static void ThreadSafeInvoke(this Control control, Action action)
        {
            if (control.InvokeRequired)
                control.Invoke(action);
            else action();
        }

        public static void EnableDoubleBuffered(this Control control)=> typeof(Control)
            .GetProperty("DoubleBuffered", BindingFlags.NonPublic | BindingFlags.Instance)
            .SetValue(control, true);
    }
}
