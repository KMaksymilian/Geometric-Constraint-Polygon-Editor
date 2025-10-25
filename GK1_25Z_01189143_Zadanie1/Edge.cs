using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.Xml;
using System.Text;
using System.Threading.Tasks;

namespace GK1_25Z_01189143_Zadanie1
{
    interface IObserver
    {
        void Update(Vertex x);
    }

    public enum TypeOfEdge
    {
        Line,
        Bezier
    }
    internal class Edge : IObserver
    {
        private int _lengthConstraint;
        public Vertex A { get; }
        public Vertex B { get; }
        public IConstraintVisitable ConstraintStrategy { get; set; } = new NoneConstraint();
        public TypeOfEdge Type { get; set; }
        public Vertex? Ctrl1 { get; set; }
        public Vertex? Ctrl2 { get; set; }
        public int LengthConstraint
        {
            get => _lengthConstraint;
            internal set
            {
                if (value <= 0)
                {
                    MessageBox.Show(
                        "Długość musi być większa od zera.",
                        "Błąd wartości",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error
                    );
                    return;
                }

                _lengthConstraint = value;
            }
        }
        internal Vertex OtherVertex(Vertex x) => x == A ? B : A;
        internal void SetConstraint(IConstraintVisitable constraint)
        {
            ConstraintStrategy = constraint;
            ConstraintStrategy.ApplyFirstTime(this);
        }
        public Edge(Vertex a, Vertex b)
        {
            A = a;
            B = b;
            A.AddObserver(this);
            B.AddObserver(this);
            LengthConstraint = (int)Math.Sqrt(Math.Pow(B.Position.X - A.Position.X, 2) + Math.Pow(B.Position.Y - A.Position.Y, 2));
            Type = TypeOfEdge.Line;
        }

        public Point MidPoint => new Point(
            (A.Position.X + B.Position.X) / 2,
            (A.Position.Y + B.Position.Y) / 2
        );

        public void DrawLabel(Graphics g)
        {
            string label = ConstraintStrategy.GetName(this);
            if (label == "None") return;

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
        }


        internal void MakeBezier()
        {
            Type = TypeOfEdge.Bezier;

            Point a = A.Position;
            Point b = B.Position;

            int dx = b.X - a.X;
            int dy = b.Y - a.Y;

            if (Ctrl1 != null && Ctrl2 != null) return;

            Ctrl1 = new Vertex((int)(a.X + dx / 3), (int)(a.Y + dy / 3));    
            Ctrl2 = new Vertex((int)(b.X - dx / 3), (int)(b.Y - dy / 3));
            Ctrl1.ChangeToCtrl();
            Ctrl2.ChangeToCtrl();
            ApplyContinuity();
            return;
        }

        internal void ApplyContinuity()
        {
            if (Ctrl1 != null) ApplyVertexContinuity(A, Ctrl1, true);
            if (Ctrl2 != null) ApplyVertexContinuity(B, Ctrl2, false);
        }

        private void ApplyVertexContinuity(Vertex vertex, Vertex ctrl, bool isStart)
        {
            var visitor = new ContinuityApplierVisitorVertexMoved();
            vertex.ContinuityStrategy.Accept(visitor, this, vertex, ctrl, isStart);
        }
        internal void ApplyContinuityCtrlMoved(Vertex vertex, Vertex ctrl, bool isStart)
        {
            var visitor = new ContinuityApplierVisitorCtrlMoved();
            vertex.ContinuityStrategy.Accept(visitor, this, vertex, ctrl, isStart);
        }

        public void Update(Vertex movedVertex)
        {
            var visitor = new ConstraintApplierVisitor();
            ConstraintStrategy.Accept(visitor, this, movedVertex);
        }

        
    }
}
