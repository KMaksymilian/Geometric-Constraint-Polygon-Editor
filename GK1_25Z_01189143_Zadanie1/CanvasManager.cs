using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging.Effects;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms.VisualStyles;

namespace GK1_25Z_01189143_Zadanie1
{
    internal class CanvasManager
    {
        public Bitmap Canvas { get; private set; }
        public Bitmap Offscreen { get; private set; }
        private readonly Color background;
        public bool useLibDrawing = true;

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
            Point a = new Point(x0, y0);
            Point b = new Point(x1, y1);
            using Graphics g = Graphics.FromImage(Offscreen);
            if (useLibDrawing)
            {
                using Pen pen = new Pen(color, 1);
                g.DrawLine(pen, a, b);
            }
            else DrawLine(a, b, color);
                edge.DrawLabel(g);  
        }

        public void DrawLine(Point a, Point b, Color color)
        {

            var (x0, y0) = (a.X, a.Y);
            var (x1, y1) = (b.X, b.Y);
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
        }

        internal void DrawTo(Graphics graphics)
        {
            using (Graphics g = Graphics.FromImage(Canvas))
            {
                g.DrawImage(Offscreen, 0, 0);
            }
            if (Canvas != null)
                graphics.DrawImage(Canvas, 0, 0);
        }

        internal void DrawBezierEdge(Edge edge, Color color)
        {
            if (edge.Ctrl1 == null || edge.Ctrl2 == null) throw new Exception();
            DrawEdge(edge, Color.Gray);
            DrawEdge(new Edge(edge.A, edge.Ctrl1), Color.LightGray);
            DrawEdge(new Edge(edge.Ctrl2, edge.B), Color.LightGray);
            DrawEdge(new Edge(edge.Ctrl1, edge.Ctrl2), Color.LightGray);
            

            PointF p0 = edge.A.Center;
            PointF p1 = edge.Ctrl1.Center;
            PointF p2 = edge.Ctrl2.Center;
            PointF p3 = edge.B.Center;

            const int segments = 1000;
            PointF prev = p0;

            using (var pen = new Pen(color, 2))
            {
                for (int i = 1; i <= segments; i++)
                {
                    float t = i / (float)segments;
                    PointF current = CalculateBezierPoint(t, p0, p1, p2, p3);
                    DrawLine(new Point((int)prev.X, (int)prev.Y), new Point((int)current.X, (int)current.Y), color);
                    prev = current;
                }
            }

            using (Graphics g = Graphics.FromImage(Offscreen))
            {
                
                DrawContinuityLabel(g, edge.A.Center, edge.A.continuity);
                DrawContinuityLabel(g, edge.B.Center, edge.B.continuity);
            }
                

        }

        private PointF CalculateBezierPoint(float t, PointF p0, PointF p1, PointF p2, PointF p3)
        {
            float u = 1 - t;
            float tt = t * t;
            float uu = u * u;
            float uuu = uu * u;
            float ttt = tt * t;

            float x = uuu * p0.X + 3 * uu * t * p1.X + 3 * u * tt * p2.X + ttt * p3.X;
            float y = uuu * p0.Y + 3 * uu * t * p1.Y + 3 * u * tt * p2.Y + ttt * p3.Y;

            return new PointF(x, y);
        }

        private void DrawContinuityLabel(Graphics g, PointF pos, typeOfContinuity cont)
        {
          
            string text = cont.ToString(); // "G0", "G1", "C1"
            using Font font = new Font("Segoe UI", 9, FontStyle.Bold);
            SizeF size = g.MeasureString(text, font);

            RectangleF box = new RectangleF(
                pos.X - size.Width / 2 - 4,
                pos.Y - 25 - size.Height / 2, // nad punktem
                size.Width + 8,
                size.Height + 4
            );

            g.FillRectangle(Brushes.White, box);
            g.DrawRectangle(Pens.Black, Rectangle.Round(box));
            g.DrawString(text, font, Brushes.Black, box.X + 4, box.Y + 2);
        }
    }
}

