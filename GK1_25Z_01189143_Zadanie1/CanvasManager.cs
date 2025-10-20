using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GK1_25Z_01189143_Zadanie1
{
    internal class CanvasManager
    {
        public Bitmap Canvas { get; private set; }
        public Bitmap Offscreen { get; private set; }
        private readonly Color background;

        public CanvasManager(int width, int height, Color bg)
        {
            background = bg;
            Canvas = new Bitmap(width, height);
            Offscreen = new Bitmap(width, height);
        }

        public void Resize(int width, int height)
        {
            Bitmap oldCanvas = Canvas;
            Offscreen.Dispose();

            Canvas = new Bitmap(width, height);
            Offscreen = new Bitmap(width, height);

            if (oldCanvas != null)
            {
                using (Graphics g = Graphics.FromImage(Canvas))
                {
                    g.DrawImage(oldCanvas, 0, 0);
                }
                using (Graphics og = Graphics.FromImage(Offscreen))
                {
                    og.DrawImage(oldCanvas, 0, 0);
                }
                oldCanvas.Dispose();
            }
        }

        public void Clear()
        {
            using (Graphics g = Graphics.FromImage(Offscreen))
                g.Clear(background);
        }

        public void DrawEdge(Edge edge, Color color)
        {

            var (x0, y0) = (edge.A.Center.X, edge.A.Center.Y);
            var (x1, y1) = (edge.B.Center.X, edge.B.Center.Y);
            int dx = Math.Abs(x1 - x0), dy = Math.Abs(y1 - y0);
            int sx = x0 < x1 ? 1 : -1, sy = y0 < y1 ? 1 : -1;

            int x = x0;
            int y = y0;

            if(x >= 0 && x < Offscreen.Width && y >= 0 && y < Offscreen.Height)
                Offscreen.SetPixel(x, y, color);

            int e;

            if (dx >= dy)
            {
                e = dx / 2;
                for (int i = 0; i < dx; i++)
                {
                    x += sx;
                    e -= dy;
                    if (e < 0)
                    {
                        y += sy;
                        e += dx;
                    }

                    if (x >= 0 && x < Offscreen.Width && y >= 0 && y < Offscreen.Height)
                        Offscreen.SetPixel(x, y, color);
                }
            }
            else
            {
                e = dy / 2;
                for (int i = 0; i < dy; i++)
                {
                    y += sy;
                    e -= dx;
                    if (e < 0)
                    {
                        x += sx;
                        e += dy;
                    }

                    if (x >= 0 && x < Offscreen.Width && y >= 0 && y < Offscreen.Height)
                        Offscreen.SetPixel(x, y, color);

                }
            }

            using Graphics g = Graphics.FromImage(Offscreen);
            edge.DrawLabel(g);
        }

        internal void DrawTo(Graphics grphics)
        {
            using (Graphics g = Graphics.FromImage(Canvas))
            {
                g.DrawImage(Offscreen, 0, 0);
            }
            if (Canvas != null)
                grphics.DrawImage(Canvas, 0, 0);
        }
    }
}

