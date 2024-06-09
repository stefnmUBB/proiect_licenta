using LillyScan.Backend.Math;
using LillyScan.FrontentWinforms;
using System;
using System.Threading;
using System.Windows.Forms;

namespace LillyScan.FrontendWinforms
{
    internal static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Thread.CurrentThread.CurrentCulture = System.Globalization.CultureInfo.InvariantCulture;
            Backend.Initializer.Initialize();
            CLBinding.Init();
            PlatformConfig.DotMul = CLBinding.DotMul;

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());
        }
    }
}
