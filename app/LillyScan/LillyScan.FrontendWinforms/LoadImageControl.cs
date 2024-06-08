using LillyScan.Backend.Imaging;
using LillyScan.BackendWinforms.Imaging;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace LillyScan.FrontendWinforms
{
    public partial class LoadImageControl : UserControl
    {
        public LoadImageControl()
        {
            InitializeComponent();
        }

        private Bitmap PreviewImage = null;
        private RawBitmap ResultImage;

        private void LoadImageControl_DragDrop(object sender, DragEventArgs e)
        {            
            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
            if (files.Length < 1) return;
            PreviewImage?.Dispose();
            PreviewImage = null;
            Bitmap bitmap = null;
            try
            {
                bitmap = new Bitmap(files[0]);
            }
            catch { return; }
            var s = Math.Max(bitmap.Width, bitmap.Height);
            PreviewImage = new Bitmap(bitmap, bitmap.Width * 512 / s, bitmap.Height * 512 / s);
            ResultImage = RawBitmapIO.FromBitmap(bitmap);
            Content.Hide();
            bitmap.Dispose();
            Invalidate();

            InputChanged?.Invoke(this, ResultImage);
        }

        public delegate void OnInputChanged(object sender, RawBitmap bitmap);
        public event OnInputChanged InputChanged;

        private void LoadImageControl_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop)) e.Effect = DragDropEffects.Copy;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            if (PreviewImage == null) return;
            var s1 = Width * PreviewImage.Height - PreviewImage.Width * Height < 0 ? Width : Height;
            var s2 = Width * PreviewImage.Height - PreviewImage.Width * Height < 0 ? PreviewImage.Width : PreviewImage.Height;            
            var w = PreviewImage.Width * s1 / s2;
            var h = PreviewImage.Height * s1 / s2;
            e.Graphics.Clear(BackColor);
            e.Graphics.DrawImage(PreviewImage, (Width - w) / 2, (Height - h) / 2, w, h);
        }
    }
}
