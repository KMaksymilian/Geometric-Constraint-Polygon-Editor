using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GK1_25Z_01189143_Zadanie1
{
    public enum LineConstraint
    {
        None,
        Vertical,
        Diagonal,
        Const
    }

    public enum EdgeKind
    {
        Line,
        Bezier
    }
    internal class Edge
    {
        public VertexButton A { get; }
        public VertexButton B { get; }
        public LineConstraint Constraint { get; set; }

        public EdgeKind Kind { get; set; }

        public VertexButton? Ctrl1 { get; set; }
        public VertexButton? Ctrl2 { get; set; }

        public Point MidPoint => new Point(
            (A.Center.X + B.Center.X) / 2,
            (A.Center.Y + B.Center.Y) / 2
        );

        public double Length { get; internal set; }

        public Edge(VertexButton a, VertexButton b)
        {
            A = a;
            B = b;
            Constraint = LineConstraint.None;
            Length = Math.Sqrt(Math.Pow(B.Center.X - A.Center.X, 2) + Math.Pow(B.Center.Y - A.Center.Y, 2));
            Kind = EdgeKind.Line;
        }
        

        

        public void DrawLabel(Graphics g)
        {
            if (Constraint == LineConstraint.None) return;

            string label = Constraint switch
            {
                LineConstraint.Vertical => "V",
                LineConstraint.Diagonal => "D",
                LineConstraint.Const => $"{Length}",
                _ => ""
            };

            using Font font = new Font("Segoe UI", 9, FontStyle.Bold);
            SizeF textSize = g.MeasureString(label, font);

            Rectangle box = new Rectangle(
                MidPoint.X - (int)textSize.Width / 2 - 3,
                MidPoint.Y - (int)textSize.Height / 2 - 1,
                (int)textSize.Width + 6,
                (int)textSize.Height + 2
            );

            g.FillRectangle(Brushes.White, box);
            g.DrawRectangle(Pens.Black, box);
            g.DrawString(label, font, Brushes.Black, MidPoint.X - textSize.Width / 2, MidPoint.Y - textSize.Height / 2);

            //Length = Math.Sqrt(Math.Pow(B.Center.X - A.Center.X, 2) + Math.Pow(B.Center.Y - A.Center.Y, 2));
        }

        public double CalculateLength()
        {
            int dx = B.Center.X - A.Center.X;
            int dy = B.Center.Y - A.Center.Y;
            return Math.Sqrt(dx * dx + dy * dy);
        }
        internal void SetConstraint(LineConstraint constraint)
        {
            Constraint = constraint;
        }

        internal (VertexButton,VertexButton) MakeBezier()
        {
            Kind = EdgeKind.Bezier;

            Point P0 = A.Center;
            Point P3 = B.Center;

            int dx = P3.X - P0.X;
            int dy = P3.Y - P0.Y;
           
            if(Ctrl1 != null && Ctrl2 != null)
                return (Ctrl1, Ctrl2);
            Ctrl1 = new VertexButton((int)(P0.X + dx / 3), (int)(P0.Y + dy / 3));    
            Ctrl2 = new VertexButton((int)(P3.X - dx / 3), (int)(P3.Y - dy / 3));
            Ctrl1.changeToCtrl();
            Ctrl2.changeToCtrl();
            return (Ctrl1, Ctrl2);
        }
    }
}
