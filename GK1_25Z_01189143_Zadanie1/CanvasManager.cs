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
        internal void DrawTo(Graphics graphics)
        {
            using (Graphics g = Graphics.FromImage(Canvas))
            {
                g.DrawImage(Offscreen, 0, 0);
            }
            if (Canvas != null)
                graphics.DrawImage(Canvas, 0, 0);
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
            Point a = edge.A.Position;
            Point b = edge.B.Position;
            DrawLine(a, b, color);
            string txt = edge.ConstraintStrategy.GetName(edge);
            if (txt != "None")
            {
                using (Graphics g = Graphics.FromImage(Offscreen))
                {
                    DrawLabel(g, edge.MidPoint, txt);
                }
            }
        }

        public void DrawLine(Point a, Point b, Color color)
        {
            if (useLibDrawing)
            {
                using Graphics g = Graphics.FromImage(Offscreen);
                using Pen pen = new Pen(color, 1);
                g.DrawLine(pen, a, b);
            }
            else MyDrawLine(a, b, color);
        }
        public void MyDrawLine(Point a, Point b, Color color)
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

        internal void DrawBezierEdge(Edge edge, Color color)
        {
            if (edge.Ctrl1 == null || edge.Ctrl2 == null) throw new Exception();
            DrawEdge(edge, Color.Gray);
            DrawLine(edge.A.Position, edge.Ctrl1.Position, Color.LightGray);
            DrawLine(edge.Ctrl2.Position, edge.B.Position, Color.LightGray);
            DrawLine(edge.Ctrl1.Position, edge.Ctrl2.Position, Color.LightGray);
            

            PointF p0 = edge.A.Position;
            PointF p1 = edge.Ctrl1.Position;
            PointF p2 = edge.Ctrl2.Position;
            PointF p3 = edge.B.Position;

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
                
                DrawLabel(g, edge.A.Position, edge.A.ContinuityStrategy.GetName());
                DrawLabel(g, edge.B.Position, edge.B.ContinuityStrategy.GetName());
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

        private void DrawLabel(Graphics g, PointF pos, string text)
        {
            using Font font = new Font("Segoe UI", 9, FontStyle.Bold);
            SizeF size = g.MeasureString(text, font);

            RectangleF box = new RectangleF(
                pos.X - size.Width / 2 - 4,
                pos.Y - 15 - size.Height / 2,
                size.Width + 8,
                size.Height + 4
            );

            g.FillRectangle(Brushes.White, box);
            g.DrawRectangle(Pens.Black, Rectangle.Round(box));
            g.DrawString(text, font, Brushes.Black, box.X + 4, box.Y + 2);
        }

        internal void DrawVertex(Vertex v, Color color)
        {
            const int radius = 5; 
            int x = v.Position.X - radius;
            int y = v.Position.Y - radius;

            using (Graphics g = Graphics.FromImage(Offscreen))
            {
                using (Brush brush = new SolidBrush(color))
                    g.FillEllipse(brush, x, y, radius * 2, radius * 2);

                using (Pen pen = new Pen(color, 1))
                    g.DrawEllipse(pen, x, y, radius * 2, radius * 2);
            }
        }
    }
}

