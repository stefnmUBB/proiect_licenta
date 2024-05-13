using System;
using System.Collections.Generic;
using System.Text;

namespace LillyScan.Backend.Imaging
{
    public static class RawBitmapDrawing
    {        
        public static void DrawQuad(this RawBitmap bitmap, (float X0, float Y0, float X1, float Y1, float X2, float Y2, float X3, float Y3) quad, float r, float g, float b)
        {
            bitmap.DrawLine((int)quad.X0, (int)quad.Y0, (int)quad.X1, (int)quad.Y1, r, g, b);
            bitmap.DrawLine((int)quad.X1, (int)quad.Y1, (int)quad.X2, (int)quad.Y2, r, g, b);
            bitmap.DrawLine((int)quad.X2, (int)quad.Y2, (int)quad.X3, (int)quad.Y3, r, g, b);
            bitmap.DrawLine((int)quad.X3, (int)quad.Y3, (int)quad.X0, (int)quad.Y0, r, g, b);
        }

        public static void DrawLine(this RawBitmap bitmap, int x0, int y0, int x1, int y1, float r, float g, float b)
        {
            int T = System.Math.Abs(x0 - x1) + System.Math.Abs(y0 - y1);
            for (int t = 0; t < T; t++) 
            {
                var tx = x0 + t * (x1 - x0) / T;
                var ty = y0 + t * (y1 - y0) / T;
                if (tx < 0 || tx >= bitmap.Width || ty < 0 || ty >= bitmap.Height) continue;
                if(bitmap.Channels==3)
                {
                    bitmap[ty, tx, 0] = r;
                    bitmap[ty, tx, 1] = g;
                    bitmap[ty, tx, 2] = b;
                }
                else
                {
                    bitmap[ty, tx, 0] = (r + g + b) / 3;
                }
            }            
        }

        public static void FillCircle(this RawBitmap bitmap, int x0, int y0, int rad, float r, float g, float b)
        {
            for (int y = y0 - rad; y <= y0 + rad; y++) 
            {
                if (y < 0 || y >= bitmap.Height) continue;
                for (int x = x0 - rad; x <= x0 + rad; x++) 
                {
                    if (x < 0 || x >= bitmap.Width) continue;
                    int dx = x - x0, dy = y - y0;
                    if (dx * dx + dy * dy > r * r) continue;
                    if (bitmap.Channels == 3)
                    {
                        bitmap[y, x, 0] = r;
                        bitmap[y, x, 1] = g;
                        bitmap[y, x, 2] = b;
                    }
                    else
                    {
                        bitmap[y, x, 0] = (r + g + b) / 3;
                    }
                }
            }
        }

    }
}
