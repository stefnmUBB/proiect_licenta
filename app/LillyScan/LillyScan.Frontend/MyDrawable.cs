using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IImage = Microsoft.Maui.Graphics.IImage;

namespace LillyScan.Frontend
{
    internal class MyDrawable : IDrawable
    {
        public Color Color = new Color(128, 128, 0);

        private IImage _Image = null;

        public IImage Image
        {
            get => _Image;
            set
            {
                _Image?.Dispose();
                _Image = value;
            }
        }

        public void Draw(ICanvas canvas, RectF dirtyRect)
        {
            canvas.FillColor = Color;
            canvas.FillRectangle(dirtyRect);
            if (_Image != null)
                canvas.DrawImage(_Image, 0, 0, dirtyRect.Width, dirtyRect.Height);
        }
    }
}
