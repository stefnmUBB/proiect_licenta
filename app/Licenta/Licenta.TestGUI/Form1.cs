using Licenta.Commons.Math;
using Licenta.Commons.Math.Arithmetics;
using Licenta.Imaging;
using Licenta.TestGUI.Properties;
using Licenta.TraceOver;
using Licenta.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Licenta.TestGUI
{
    public partial class Form1 : Form
    {
        Bitmap SourceImage = Resources.TestImage;
        Bitmap RenderedImage = Resources.TestImage;
        Matrix<DoubleNumber> ImageMatrix;

        TraceOverDevice TraceOverDevice;

        public Form1()
        {
            InitializeComponent();
            typeof(Control)
                .GetProperty("DoubleBuffered", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .SetValue(Canvas, true);

            Canvas.Size = SourceImage.Size;
            ImageMatrix = new ImageRGB(SourceImage).ToGrayScaleMatrixLinear();
            ImageMatrix = Matrices.Convolve(ImageMatrix, Kernels.GaussianFilter(21, 21));

            TraceOverDevice = new TraceOverDevice(ImageMatrix);

            RenderedImage = new ImageRGB(ImageMatrix).ToBitmap();

        }

        private void Canvas_Paint(object sender, PaintEventArgs e)
        {
            e.Graphics.DrawImageUnscaled(RenderedImage, 0, 0);
        }
    }
}
