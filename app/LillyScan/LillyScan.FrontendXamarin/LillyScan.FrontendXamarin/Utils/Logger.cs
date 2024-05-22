using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace LillyScan.FrontendXamarin.Utils
{
    internal class Logger
    {
        private string Prefix;

        public Logger(string prefix)
        {
            Prefix = prefix;
        }
        public Logger(Type type) : this($"[{type.Name}]: ") { }
        public void WriteLine(string message) => Debug.WriteLine(Prefix + message);

        public static Logger Create<T>()
        {
#if DEBUG
        return new Logger(typeof(T));
#else
        return null;
#endif
        }

    }
}
