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
    public partial class PredictionsListView : UserControl
    {
        public PredictionsListView()
        {
            InitializeComponent();
            this.EnableDoubleBuffered();            
        }

        private readonly List<(Bitmap Bitmap, string Text)> Items = new List<(Bitmap Bitmap, string Text)>();

        public void Clear()
        {
            foreach (var item in Items) item.Bitmap.Dispose();
            Items.Clear();
            Invalidate();
        }

        public void SetScrollSize(int itemsCount)
        {
            Scrollbar.Maximum = (64 + Font.Height + 10) * itemsCount;
        }

        public int AddItem(Bitmap bitmap)
        {
            Items.Add((bitmap, "..."));
            Invalidate();
            return Items.Count;
        }

        public void SetPrediction(int id, string text)
        {
            Items[id] = (Items[id].Bitmap, text);
            Invalidate();
        }

        private void Render(Graphics g, Size bounds, int scrollY)
        {            
            int textHeight = Font.Height + 10;            
            int currentY = 0;
            for(int i=0;i<Items.Count;i++)
            {                
                (var bitmap, var text) = Items[i];
                float bitmapScale = (bitmap.Width < bounds.Width ? 1 : 1.0f * bounds.Width / bitmap.Width);
                (var scaledWidth, var scaledHeight) = (bitmap.Width * bitmapScale, bitmap.Height * bitmapScale);
                if (currentY - scrollY >= bounds.Height || currentY + scaledHeight + textHeight - scrollY < 0)
                {
                    currentY += (int)scaledHeight + textHeight;
                    continue;
                }
                var bitmapX = (int)(bounds.Width - scaledWidth) / 2;
                g.DrawImage(bitmap, bitmapX, currentY - scrollY, scaledWidth, scaledHeight);
                currentY += (int)scaledHeight;
                g.DrawString(text, Font, Brushes.Black, new Rectangle(0, currentY-scrollY, bounds.Width, textHeight));
                currentY += textHeight;
            }            
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Render(e.Graphics, new Size(Width - Scrollbar.Width, Height), Scrollbar.Value);
        }

        private void Scrollbar_Scroll(object sender, ScrollEventArgs e)
        {
            Invalidate();
        }
    }
}
