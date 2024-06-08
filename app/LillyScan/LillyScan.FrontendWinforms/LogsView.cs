using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace LillyScan.FrontendWinforms
{
    public partial class LogsView : UserControl
    {
        public LogsView()
        {
            InitializeComponent();
            Out = new OutDevice(TextBox);
            TextBox.EnableDoubleBuffered();
        }

        public TextWriter Out;

        public void Clear() { TextBox.Text = ""; }

        class OutDevice : TextWriter
        {
            private TextBox TextBox;
            private string Buffer="";
            public OutDevice(TextBox textbox) =>  TextBox = textbox;
            public override void Write(char value) => TextBox.ThreadSafeInvoke(() => TextBox.AppendText("" + value));
            public override void Write(string value)
            {
                Buffer += value;
                if (Buffer.Length >= 512) 
                {
                    Flush();
                }                
            }
            public override void Flush()
            {
                TextBox.ThreadSafeInvoke(() => TextBox.AppendText(Buffer));
                Buffer = "";
            }

            public override Encoding Encoding => Encoding.ASCII;

        }
    }
}
