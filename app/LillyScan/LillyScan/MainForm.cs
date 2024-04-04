using LillyScan.Backend.Imaging;
using LillyScan.Backend.Math;
using LillyScan.BackendWinforms.Imaging;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace LillyScan
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
            Task.Run(() =>
            {
                Execute(@"D:\Users\Stefan\Datasets\hw_flex\LineSegRaster\IAM\1_in.png");
            });
        }

        Bitmap ResultBmp;

        void Execute(string path)
        {
            try
            {
                using (var bmp = new Bitmap(path))
                {
                    ResultBmp?.Dispose();
                    ResultBmp = new Bitmap(bmp,512,512);
                    using (var g = Graphics.FromImage(ResultBmp))
                    {
                        var img = RawBitmapIO.FromBitmap(bmp);
                        img = img.Resize(256, 256);
                        img = img.AverageChannels();

                        var th = img.Height / 64;
                        var tw = img.Width / 64;


                        for (int i = 0; i < th; i++)
                        {
                            for (int j = 0; j < tw; j++)
                            {
                                var tile = img.Crop(64 * j, 64 * i, 64, 64);
                                var mask = Program.HTR.Segment64(tile.ToArray());
                                var m = new RawBitmap(64, 64, 1, mask).ToBitmap();

                                ColorMatrix matrix = new ColorMatrix();
                                matrix.Matrix33 = 0.5f;
                                matrix.Matrix20 = 0.6f;
                                ImageAttributes attributes = new ImageAttributes();
                                attributes.SetColorMatrix(matrix, ColorMatrixFlag.Default, ColorAdjustType.Bitmap);

                                g.DrawImage(m, new Rectangle(128 * j, 128 * i, 128, 128), 0, 0, 64, 64, GraphicsUnit.Pixel, attributes);
                                g.Flush(System.Drawing.Drawing2D.FlushIntention.Sync);

                                Invoke(new Action(() => ImgPanel.Invalidate()));
                            }
                        }                        
                        
                        ImgPanel.Size = ResultBmp.Size;
                        ImgPanel.Invalidate();
                    }
                }
            }
            catch(Exception e)
            {
                MessageBox.Show(e.Message);
            }
        }

        private void ImgPanel_Paint(object sender, PaintEventArgs e)
        {
            if (ResultBmp != null)
                e.Graphics.DrawImageUnscaled(ResultBmp, 0, 0);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Task.Run(() =>
            {
                Execute(textBox1.Text);
            });            
        }
    }
}
