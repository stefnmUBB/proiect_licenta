using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LillyScan.Frontend
{
    internal class MyDrawable : IDrawable
    {
        public void Draw(ICanvas canvas, RectF dirtyRect)
        {
            canvas.FillColor = new Color(128, 0, 0);
            canvas.FillCircle(20, 20, 20);
            // Drawing code goes here
        }
    }
}
