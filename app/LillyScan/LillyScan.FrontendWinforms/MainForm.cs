using LillyScan.Backend.HTR;
using System;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;
using LillyScan.Backend.Imaging;
using static System.Net.Mime.MediaTypeNames;
using LillyScan.BackendWinforms.Imaging;
using System.Collections.Generic;
using System.Drawing.Drawing2D;

namespace LillyScan.FrontendWinforms
{
    public partial class MainForm : Form
    {
        private static IHTREngine HTR = new BuiltInHTREngine();

        public MainForm()
        {
            InitializeComponent();
            DoubleBuffered = true;
        }

        private void LineRecogLoadImageControl_InputChanged(object sender, Backend.Imaging.RawBitmap bitmap)
        {
            LineRecogLoadImageControl.Enabled = false;
            Console.SetOut(LineRecogLogsView.Out);            
            LineRecogLogsView.Clear();
            LineRecogOutBox.Text = "";            

            Task.Run(() =>
            {
                var text = HTR.PredictTextLine(bitmap);                
                this.InvokeAction(() =>
                {
                    LineRecogOutBox.Text = text;
                    LineRecogLoadImageControl.Enabled = true;
                });
            });            
        }

        private void MainLoadImageControlmageControl2_InputChanged(object sender, Backend.Imaging.RawBitmap bitmap)
        {                   
            PredictionsListView.Clear();
            MainLoadImageControlmageControl2.Enabled = false;
            Console.SetOut(MainLogsView.Out);
            MainLogsView.Clear();
            var list = new List<RawBitmap>();
            Task.Run(() =>
            {
                var lines = HTR.SegmentLines(bitmap);
                this.InvokeAction(() => PredictionsListView.SetScrollSize(lines.Length));                
                foreach (var mask in lines)
                {
                    var linebmp = mask.CutFromImage(bitmap);                           
                    linebmp = linebmp.RotateAndCrop((float)-System.Math.Atan2(-mask.LineFit.A, mask.LineFit.B), disposeOriginal: true);
                    this.InvokeAction(() => PredictionsListView.AddItem(linebmp.ToBitmap()));
                    list.Add(linebmp);                                        
                    mask.Dispose();                                        
                }
                Console.Out.Flush();

                for(int i=0, l=list.Count;i<l;i++)
                {
                    var text = HTR.PredictTextLine(list[i]);
                    list[i].Dispose();
                    this.InvokeAction(() => PredictionsListView.SetPrediction(i, text));
                }
                
                this.InvokeAction(() => MainLoadImageControlmageControl2.Enabled = true); 
            });                




        }
    }
}
