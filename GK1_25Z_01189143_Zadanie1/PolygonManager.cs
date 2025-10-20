using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms.VisualStyles;
using System.Xml;
using static System.Net.Mime.MediaTypeNames;

namespace GK1_25Z_01189143_Zadanie1
{
    internal class PolygonManager
    {
        public List<VertexButton> Vertices { get; } = new();
        public List<Edge> Edges { get; } = new();
        private readonly CanvasManager canvas;

        public PolygonManager(CanvasManager canvas)
        {
            this.canvas = canvas;
        }

        public void AddVertex(VertexButton v)
        {
            Vertices.Add(v);
            if (Vertices.Count > 1)
                Edges.Add(new Edge(Vertices[^2], Vertices[^1]));
            if (Vertices.Count > 2)
            {
                Edges.Add(new Edge(Vertices[^1], Vertices[0]));
            }
                RedrawAll();
        }

        public void AddVertexOnEdge(VertexButton v, Edge edge)
        {
            int index = Edges.IndexOf(edge);
            if (index < 0) return;

            Edge a = new Edge(edge.A, v);
            Edge b = new Edge(v, edge.B);

            Vertices.Insert(index + 1, v);
            Edges.RemoveAt(index);
            Edges.Insert(index, a);
            Edges.Insert(index + 1, b);
            RedrawAll();
        }

        public void RedrawAll()
        {
            canvas.Clear();
            foreach (var edge in Edges)
                canvas.DrawEdge(edge, Color.Red);
        }

        public void RemoveVertex(VertexButton v)
        {
            int index = Vertices.IndexOf(v);
            if (index < 0) return;

            Vertices.RemoveAt(index);
            Edges.Clear();

            for (int i = 0; i < Vertices.Count; i++)
                Edges.Add(new Edge(Vertices[i], Vertices[(i + 1) % Vertices.Count]));

            RedrawAll();
        }

        internal void MoveAllVertex(int dx, int dy)
        {

            foreach (var btn in Vertices)
            {
                btn.Left += dx;
                btn.Top += dy;
            }
            RedrawAll();
        }

        internal Edge? FindNearestLine(Point click, int maxDistance)
        {
            Edge? nearest = null;
            double minDist = maxDistance;

            foreach (var edge in Edges)
            {
                Point p1 = edge.A.Center;
                Point p2 = edge.B.Center;
                double dist = DistancePointToSegment(click, p1, p2);
                if (dist <= minDist)
                {
                    minDist = dist;
                    nearest = edge;
                }
            }

            return nearest;

        }

        private double DistancePointToSegment(Point p, Point a, Point b)
        {
            double dx = b.X - a.X;
            double dy = b.Y - a.Y;

            if (dx == 0 && dy == 0)
                return Math.Sqrt((p.X - a.X) * (p.X - a.X) + (p.Y - a.Y) * (p.Y - a.Y));

            double t = ((p.X - a.X) * dx + (p.Y - a.Y) * dy) / (dx * dx + dy * dy);
            t = Math.Max(0, Math.Min(1, t));

            double projX = a.X + t * dx;
            double projY = a.Y + t * dy;

            return Math.Sqrt((p.X - projX) * (p.X - projX) + (p.Y - projY) * (p.Y - projY));
        }

        internal void Dispose()
        {
            Edges.Clear();
            Vertices.Clear();
        }

        internal void MoveVertex(VertexButton selectedVertexButton, Point cursor)
        { 
            selectedVertexButton.Left = cursor.X - selectedVertexButton.Width / 2;
            selectedVertexButton.Top = cursor.Y - selectedVertexButton.Height / 2;
            int index = Vertices.IndexOf(selectedVertexButton);
            ApplyConstraint(index);
            RedrawAll();
          
        }

        private void ApplyConstraint(int startIndex)
        {
            if(ApplyConstraintReqClockwise(startIndex, startIndex) == 1)
                ApplyConstraintReqCounterClockwise((startIndex - 1 + Edges.Count)% Edges.Count,startIndex);
        }
        private int ApplyConstraintReqClockwise(int prev, int firstMoved)
        {
            if (prev == (firstMoved - 1 + Edges.Count) % Edges.Count)
            {
                VertexButton firstMovedV = Edges[prev].B;
                VertexButton middleV = Edges[prev].A;
                VertexButton prevV = Edges[(prev - 1 + Edges.Count) % Edges.Count].A;

                ApplyFinalCstraint(firstMovedV, middleV, prevV);

                return 0;
            }

            VertexButton a = Edges[prev].A;
            VertexButton b = Edges[prev].B;
            switch (Edges[prev].Constraint)
            {
                case LineConstraint.Vertical:
                    b.Left = a.Left;
                    break;
                case LineConstraint.Diagonal:
                    int dx = b.Center.X - a.Center.X;
                    int dy = b.Center.Y - a.Center.Y;
                    int sy = Math.Sign(dy);
                    b.Top = a.Center.Y + sy * Math.Abs(dx) - b.Height / 2;
                    break;
                case LineConstraint.Const:
                    double L = Edges[prev].Length; // zadana długość
                    dx = b.Center.X - a.Center.X;
                    dy = b.Center.Y - a.Center.Y;

                    double currentLength = Math.Sqrt(dx * dx + dy * dy);
                    if (currentLength < 1e-6) break; // unikaj dzielenia przez zero

                    double scale = L / currentLength;

                    // Nowa pozycja b = a + (wektor * L)
                    int newX = (int)(a.Center.X + dx * scale - a.Width / 2);
                    int newY = (int)(a.Center.Y + dy * scale - a.Height / 2);

                    b.Left = newX;
                    b.Top = newY;
                    break;

                default:
                    return 1;
            }
            return ApplyConstraintReqClockwise((prev + 1) % Edges.Count, firstMoved);
        }

       

        private int ApplyConstraintReqCounterClockwise(int next, int firstMoved)
        {
            if (next == firstMoved)
            {
                VertexButton firstMovedV = Edges[next].A;
                VertexButton middleV = Edges[next].B;
                VertexButton prevV = Edges[(next + 1) % Edges.Count].B;

                ApplyFinalCstraint(firstMovedV, middleV, prevV);
                return 0;
            }
              
            VertexButton a = Edges[next].A;
            VertexButton b = Edges[next].B;
            switch (Edges[next].Constraint)
            {
                case LineConstraint.Vertical:
                    a.Left = b.Left;
                    break;
                case LineConstraint.Diagonal:
                    int dx = b.Center.X - a.Center.X;
                    int dy = b.Center.Y - a.Center.Y;
                    int sy = Math.Sign(dy);
                    a.Top = b.Center.Y - sy * Math.Abs(dx) - a.Height / 2;
                    break;
                case LineConstraint.Const:
                    double L = Edges[next].Length; // zadana długość
                    dx = b.Center.X - a.Center.X;
                    dy = b.Center.Y - a.Center.Y;

                    float currentLength = MathF.Sqrt(dx * dx + dy * dy);
                    if (currentLength == 0) throw new Exception("Invalid lenght");

                    double scale = L / currentLength;

                    int newX = b.Center.X + (int)(dx * scale) - b.Width / 2;
                    int newY = b.Center.Y + (int)(dy * scale) - b.Height / 2;

                    a.Left = newX;
                    a.Top = newY;
                    break;
                default:
                    return 1;
            }
            return ApplyConstraintReqCounterClockwise((next - 1 + Edges.Count) % Edges.Count, firstMoved);
        }

        private void ApplyFinalCstraint(VertexButton firstMovedV, VertexButton middleV, VertexButton prevV)
        {
            return;
        }

        internal void SetConstraintOnEdge(Edge nearest, LineConstraint constraint)
        {
            nearest.SetConstraint(constraint);
            ApplyConstraint(Edges.IndexOf(nearest));
            RedrawAll();
        }
    }
}
