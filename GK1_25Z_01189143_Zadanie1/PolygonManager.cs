using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Drawing;
using System.Linq;
using System.Net.NetworkInformation;
using System.Runtime.ConstrainedExecution;
using System.Security.Cryptography;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms.VisualStyles;
using System.Xml;
using static System.Net.Mime.MediaTypeNames;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

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
            {
                if (edge.Kind == EdgeKind.Line)
                    canvas.DrawEdge(edge, Color.Red);
                else if (edge.Kind == EdgeKind.Bezier)
                    canvas.DrawBezierEdge(edge, Color.Red);
            }         
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
            if(selectedVertexButton.type == typeOfVertex.Normal)
            {
                int index = Vertices.IndexOf(selectedVertexButton);
                ApplyConstraint(index);
            }
            RedrawAll();
        }

        private void ApplyConstraint(int startIndex)
        {
            int clockwisePtr = startIndex;
            int counterClockwisePtr = (startIndex - 1 + Edges.Count) % Edges.Count;
            Edge Clockwise = Edges[startIndex];
            Edge CounterClockwise = Edges[(startIndex - 1 + Edges.Count) % Edges.Count];
            while (true)
            {
                if (Clockwise.Constraint == LineConstraint.None && CounterClockwise.Constraint == LineConstraint.None) return;
                else if(Clockwise == CounterClockwise)
                {
                    if (Edges.Count == 1) ApplyConstraintOnEdge(Clockwise);
                    else
                    {
                        switch (Clockwise.Constraint)
                        {
                            case LineConstraint.Vertical:
                                if (Clockwise.A.Left != Clockwise.B.Left)
                                    Clockwise.SetConstraint(LineConstraint.None);
                                break;
                            case LineConstraint.Diagonal:
                                if(Math.Abs(Clockwise.B.Center.X - Clockwise.A.Center.X) != Math.Abs(Clockwise.B.Center.Y - Clockwise.A.Center.Y))
                                    Clockwise.SetConstraint(LineConstraint.None);
                                break;
                            case LineConstraint.Const:
                                double currentLength = Clockwise.CalculateLength();
                                if (Math.Abs(currentLength - Clockwise.Length) > 1e-6)
                                    Clockwise.SetConstraint(LineConstraint.None);
                                break;
                        }
                    }
                    return;
                }
                else if(Clockwise.B == CounterClockwise.A && CounterClockwise.Constraint != LineConstraint.None && Clockwise.Constraint != LineConstraint.None)
                {
                    ApplyFinalConstraint(Clockwise, CounterClockwise);
                    return;
                }
                else if (Clockwise.Constraint != LineConstraint.None)
                {
                    ApplyConstraintOnEdge(Clockwise);
                    clockwisePtr = (clockwisePtr + 1) % Edges.Count;
                    Clockwise = Edges[clockwisePtr];
                }
                else if (CounterClockwise.Constraint != LineConstraint.None)
                {
                    ApplyConstraintOnEdgeCounter(CounterClockwise);
                    counterClockwisePtr = (counterClockwisePtr - 1 + Edges.Count) % Edges.Count;
                    CounterClockwise = Edges[counterClockwisePtr];
                }
            }

           // if(ApplyConstraintReqClockwise(startIndex, startIndex) == 1)
            //    ApplyConstraintReqCounterClockwise((startIndex - 1 + Edges.Count)% Edges.Count,startIndex);
        }

        private void ApplyConstraintOnEdgeCounter(Edge counterClockwise)
        {
            VertexButton a = counterClockwise.A;
            VertexButton b = counterClockwise.B;
            switch (counterClockwise.Constraint)
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
                    double L = counterClockwise.Length;
                    dx = a.Center.X - b.Center.X;
                    dy = a.Center.Y - b.Center.Y;

                    float currentLength = MathF.Sqrt(dx * dx + dy * dy);
                    if (currentLength < 1e-6) break;

                    double scale = L / currentLength;

                    int newX = (int)(b.Center.X + dx * scale - b.Width / 2);
                    int newY = (int)(b.Center.Y + dy * scale - b.Height / 2);

                    a.Left = newX;
                    a.Top = newY;
                    break;
            }
        }

        private void ApplyFinalConstraint(Edge clockwise, Edge counterClockwise)
        {
            VertexButton middleV = clockwise.B;

            if (clockwise.Constraint == LineConstraint.Vertical && counterClockwise.Constraint == LineConstraint.Vertical)
                throw new Exception("Cannot have two consecutive vertical constraints.");
            else if (clockwise.Constraint == LineConstraint.Diagonal && counterClockwise.Constraint == LineConstraint.Diagonal)
            {
                int dxPrev = middleV.Center.X - clockwise.A.Center.X;
                int dyPrev = Math.Sign(dxPrev) * Math.Abs(dxPrev);

                int dxNext = counterClockwise.B.Center.X - middleV.Center.X;
                int dyNext = -Math.Sign(dxNext) * Math.Abs(dxNext);

                int newY = (clockwise.A.Center.Y + dyPrev + counterClockwise.B.Center.Y + dyNext) / 2;
                middleV.Top = newY - middleV.Height / 2;
            }
            else if (clockwise.Constraint == LineConstraint.Const && counterClockwise.Constraint == LineConstraint.Const)
            {
                double L1 = clockwise.Length;
                double L2 = counterClockwise.Length;
                double avg = (L1 + L2) / 2;
                int dx = counterClockwise.B.Center.X - clockwise.A.Center.X;
                int dy = counterClockwise.B.Center.Y - clockwise.A.Center.Y;

                double total = Math.Sqrt(dx * dx + dy * dy);
                if (total == 0) return;

                double ratio = L1 / (L1 + L2);
                middleV.Left = (int)(clockwise.A.Center.X + dx * ratio - middleV.Width / 2);
                middleV.Top = (int)(clockwise.A.Center.Y + dy * ratio - middleV.Height / 2);
            }
            else if (clockwise.Constraint == LineConstraint.Vertical && counterClockwise.Constraint == LineConstraint.Diagonal)
            {
                middleV.Left = clockwise.A.Left;
                int dx = counterClockwise.B.Center.X - middleV.Center.X;
                int sy = Math.Sign(counterClockwise.B.Center.Y - middleV.Center.Y);
                middleV.Top = counterClockwise.B.Center.Y - sy * Math.Abs(dx) - middleV.Height / 2;

            }
            else if (clockwise.Constraint == LineConstraint.Vertical && counterClockwise.Constraint == LineConstraint.Const)
            {
                middleV.Left = clockwise.A.Left;
                double L = counterClockwise.Length;
                int dx = counterClockwise.B.Center.X - middleV.Center.X;
                int sy = Math.Sign(counterClockwise.B.Center.Y - middleV.Center.Y);
                int dy = (int)Math.Sqrt((L * L - dx * dx));
                middleV.Top = counterClockwise.B.Center.Y - sy * dy - middleV.Height / 2;
                if (L != Math.Sqrt(Math.Pow(counterClockwise.B.Center.X - middleV.Center.X, 2) + Math.Pow(counterClockwise.B.Center.Y - middleV.Center.Y, 2)))
                {
                    counterClockwise.SetConstraint(LineConstraint.None);
                }
            }
            else if (clockwise.Constraint == LineConstraint.Diagonal && counterClockwise.Constraint == LineConstraint.Vertical)
            {
                middleV.Left = counterClockwise.B.Left;
                int dx = clockwise.A.Center.X - middleV.Center.X;
                int sy = Math.Sign(clockwise.A.Center.Y - middleV.Center.Y);
                middleV.Top = clockwise.A.Center.Y - sy * Math.Abs(dx) - middleV.Height / 2;

            }
            else if (clockwise.Constraint == LineConstraint.Diagonal && counterClockwise.Constraint == LineConstraint.Const)
            {
                double L = counterClockwise.Length;
                int dx = counterClockwise.B.Center.X - middleV.Center.X;
                int sy = Math.Sign(counterClockwise.B.Center.Y - middleV.Center.Y);
                int dy = (int)Math.Sqrt(L * L - dx * dx);
                middleV.Top = counterClockwise.B.Center.Y - sy * dy - middleV.Height / 2;
            }
            else if (clockwise.Constraint == LineConstraint.Const && counterClockwise.Constraint == LineConstraint.Vertical)
            {
                middleV.Left = counterClockwise.B.Left;
                double L = clockwise.Length;
                int dx = middleV.Center.X - clockwise.A.Center.X;
                int sy = Math.Sign(middleV.Center.Y - clockwise.A.Center.Y);
                int dy = (int)Math.Sqrt((L * L - dx * dx));
                middleV.Top = clockwise.A.Center.Y + sy * dy - middleV.Height / 2;
                if (L != Math.Sqrt(Math.Pow(middleV.Center.X - clockwise.A.Center.X, 2) + Math.Pow(middleV.Center.Y - clockwise.A.Center.Y, 2)))
                {
                    clockwise.SetConstraint(LineConstraint.None);
                }
            }
            else if (clockwise.Constraint == LineConstraint.Const && counterClockwise.Constraint == LineConstraint.Diagonal)
            {
                double L = clockwise.Length;
                int dx = middleV.Center.X - clockwise.A.Center.X;
                int sy = Math.Sign(middleV.Center.Y - clockwise.A.Center.Y);
                int dy = (int)Math.Sqrt(L * L - dx * dx);
                middleV.Top = clockwise.A.Center.Y + sy * dy - middleV.Height / 2;
            }
        }

        private void ApplyConstraintOnEdge(Edge clockwise)
        {
            VertexButton a = clockwise.A;
            VertexButton b = clockwise.B;
            switch (clockwise.Constraint)
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
                    double L = clockwise.Length;
                    dx = b.Center.X - a.Center.X;
                    dy = b.Center.Y - a.Center.Y;

                    double currentLength = Math.Sqrt(dx * dx + dy * dy);
                    if (currentLength < 1e-6) break;

                    double scale = L / currentLength;

                    int newX = (int)(a.Center.X + dx * scale - a.Width / 2);
                    int newY = (int)(a.Center.Y + dy * scale - a.Height / 2);
                

                    b.Left = newX;
                    b.Top = newY;
                    break;
            }
        }

        
        internal void MakeEdgeBezier(Edge nearest)
        {
            nearest.MakeBezier();
            RedrawAll();
        }


        internal void SetConstraintOnEdge(Edge nearest, LineConstraint constraint)
        {
            nearest.SetConstraint(constraint);
            ApplyConstraint(Edges.IndexOf(nearest));
            RedrawAll();
        }

        //private void ApplyFinalCstraint(VertexButton firstMovedV, VertexButton middleV, VertexButton prevV)
        //{
        //    Edge prev = Edges.First(e => e.B == middleV);
        //    Edge next = Edges.First(e => e.A == middleV);

        //    if (prev.Constraint == LineConstraint.None || next.Constraint == LineConstraint.None)
        //        return;


        //    if (prev.Constraint == LineConstraint.Vertical && next.Constraint == LineConstraint.Vertical)
        //        throw new Exception("Cannot have two consecutive vertical constraints.");
        //    else if (prev.Constraint == LineConstraint.Diagonal && next.Constraint == LineConstraint.Diagonal)
        //    {
        //        int dxPrev = middleV.Center.X - prev.A.Center.X;
        //        int dyPrev = Math.Sign(dxPrev) * Math.Abs(dxPrev);

        //        int dxNext = next.B.Center.X - middleV.Center.X;
        //        int dyNext = -Math.Sign(dxNext) * Math.Abs(dxNext);

        //        int newY = (prev.A.Center.Y + dyPrev + next.B.Center.Y + dyNext) / 2;
        //        middleV.Top = newY - middleV.Height / 2;
        //    }
        //    else if (prev.Constraint == LineConstraint.Const && next.Constraint == LineConstraint.Const)
        //    {
        //        double L1 = prev.Length;
        //        double L2 = next.Length;
        //        double avg = (L1 + L2) / 2;
        //        int dx = next.B.Center.X - prev.A.Center.X;
        //        int dy = next.B.Center.Y - prev.A.Center.Y;

        //        double total = Math.Sqrt(dx * dx + dy * dy);
        //        if (total == 0) return;

        //        double ratio = L1 / (L1 + L2);
        //        middleV.Left = (int)(prev.A.Center.X + dx * ratio - middleV.Width / 2);
        //        middleV.Top = (int)(prev.A.Center.Y + dy * ratio - middleV.Height / 2);
        //    }
        //    else if (prev.Constraint == LineConstraint.Vertical && next.Constraint == LineConstraint.Diagonal)
        //    {
        //        middleV.Left = prev.A.Left;
        //        int dx = next.B.Center.X - middleV.Center.X;
        //        int sy = Math.Sign(next.B.Center.Y - middleV.Center.Y);
        //        middleV.Top = next.B.Center.Y - sy * Math.Abs(dx) - middleV.Height / 2;

        //    }
        //    else if (prev.Constraint == LineConstraint.Vertical && next.Constraint == LineConstraint.Const)
        //    {
        //        middleV.Left = prev.A.Left;
        //        double L = next.Length;
        //        int dx = next.B.Center.X - middleV.Center.X;
        //        int sy = Math.Sign(next.B.Center.Y - middleV.Center.Y);
        //        int dy = (int)Math.Sqrt((L * L - dx * dx));
        //        middleV.Top = next.B.Center.Y - sy * dy - middleV.Height / 2;
        //        if (L != Math.Sqrt(Math.Pow(next.B.Center.X - middleV.Center.X, 2) + Math.Pow(next.B.Center.Y - middleV.Center.Y, 2)))
        //        {
        //            next.SetConstraint(LineConstraint.None);
        //        }
        //    }
        //    else if (prev.Constraint == LineConstraint.Diagonal && next.Constraint == LineConstraint.Vertical)
        //    {
        //        middleV.Left = next.B.Left;
        //        int dx = prev.A.Center.X - middleV.Center.X;
        //        int sy = Math.Sign(prev.A.Center.Y - middleV.Center.Y);
        //        middleV.Top = prev.A.Center.Y - sy * Math.Abs(dx) - middleV.Height / 2;

        //    }
        //    else if (prev.Constraint == LineConstraint.Diagonal && next.Constraint == LineConstraint.Const)
        //    {
        //        double L = next.Length;
        //        int dx = next.B.Center.X - middleV.Center.X;
        //        int sy = Math.Sign(next.B.Center.Y - middleV.Center.Y);
        //        int dy = (int)Math.Sqrt(L * L - dx * dx);
        //        middleV.Top = next.B.Center.Y - sy * dy - middleV.Height / 2;
        //    }
        //    else if (prev.Constraint == LineConstraint.Const && next.Constraint == LineConstraint.Vertical)
        //    {
        //        middleV.Left = next.B.Left;
        //        double L = prev.Length;
        //        int dx = middleV.Center.X - prev.A.Center.X;
        //        int sy = Math.Sign(middleV.Center.Y - prev.A.Center.Y);
        //        int dy = (int)Math.Sqrt((L * L - dx * dx));
        //        middleV.Top = prev.A.Center.Y + sy * dy - middleV.Height / 2;
        //        if (L != Math.Sqrt(Math.Pow(middleV.Center.X - prev.A.Center.X, 2) + Math.Pow(middleV.Center.Y - prev.A.Center.Y, 2)))
        //        {
        //            prev.SetConstraint(LineConstraint.None);
        //        }
        //    }
        //    else if (prev.Constraint == LineConstraint.Const && next.Constraint == LineConstraint.Diagonal)
        //    {
        //        double L = prev.Length;
        //        int dx = middleV.Center.X - prev.A.Center.X;
        //        int sy = Math.Sign(middleV.Center.Y - prev.A.Center.Y);
        //        int dy = (int)Math.Sqrt(L * L - dx * dx);
        //        middleV.Top = prev.A.Center.Y + sy * dy - middleV.Height / 2;
        //    }


        //}


        //private int ApplyConstraintReqClockwise(int prev, int firstMoved)
        //{
        //    if (prev == (firstMoved - 1 + Edges.Count) % Edges.Count)
        //    {
        //        VertexButton firstMovedV = Edges[prev].B;
        //        VertexButton middleV = Edges[prev].A;
        //        VertexButton prevV = Edges[(prev - 1 + Edges.Count) % Edges.Count].A;

        //        ApplyFinalCstraint(firstMovedV, middleV, prevV);

        //        return 0;
        //    }

        //    VertexButton a = Edges[prev].A;
        //    VertexButton b = Edges[prev].B;
        //    switch (Edges[prev].Constraint)
        //    {
        //        case LineConstraint.Vertical:
        //            b.Left = a.Left;
        //            break;
        //        case LineConstraint.Diagonal:
        //            int dx = b.Center.X - a.Center.X;
        //            int dy = b.Center.Y - a.Center.Y;
        //            int sy = Math.Sign(dy);
        //            b.Top = a.Center.Y + sy * Math.Abs(dx) - b.Height / 2;
        //            break;
        //        case LineConstraint.Const:
        //            double L = Edges[prev].Length; // zadana długość
        //            dx = b.Center.X - a.Center.X;
        //            dy = b.Center.Y - a.Center.Y;

        //            double currentLength = Math.Sqrt(dx * dx + dy * dy);
        //            if (currentLength < 1e-6) break; // unikaj dzielenia przez zero

        //            double scale = L / currentLength;

        //            // Nowa pozycja b = a + (wektor * L)
        //            int newX = (int)(a.Center.X + dx * scale - a.Width / 2);
        //            int newY = (int)(a.Center.Y + dy * scale - a.Height / 2);

        //            b.Left = newX;
        //            b.Top = newY;
        //            break;

        //        default:
        //            return 1;
        //    }
        //    return ApplyConstraintReqClockwise((prev + 1) % Edges.Count, firstMoved);
        //}



        //private int ApplyConstraintReqCounterClockwise(int next, int firstMoved)
        //{
        //    if (next == firstMoved)
        //    {
        //        VertexButton firstMovedV = Edges[next].A;
        //        VertexButton middleV = Edges[next].B;
        //        VertexButton prevV = Edges[(next + 1) % Edges.Count].B;

        //        ApplyFinalCstraint(firstMovedV, middleV, prevV);
        //        return 0;
        //    }

        //    VertexButton a = Edges[next].A;
        //    VertexButton b = Edges[next].B;
        //    switch (Edges[next].Constraint)
        //    {
        //        case LineConstraint.Vertical:
        //            a.Left = b.Left;
        //            break;
        //        case LineConstraint.Diagonal:
        //            int dx = b.Center.X - a.Center.X;
        //            int dy = b.Center.Y - a.Center.Y;
        //            int sy = Math.Sign(dy);
        //            a.Top = b.Center.Y - sy * Math.Abs(dx) - a.Height / 2;
        //            break;
        //        case LineConstraint.Const:
        //            double L = Edges[next].Length; // zadana długość
        //            dx = b.Center.X - a.Center.X;
        //            dy = b.Center.Y - a.Center.Y;

        //            float currentLength = MathF.Sqrt(dx * dx + dy * dy);
        //            if (currentLength == 0) throw new Exception("Invalid lenght");

        //            double scale = L / currentLength;

        //            int newX = b.Center.X + (int)(dx * scale) - b.Width / 2;
        //            int newY = b.Center.Y + (int)(dy * scale) - b.Height / 2;

        //            a.Left = newX;
        //            a.Top = newY;
        //            break;
        //        default:
        //            return 1;
        //    }
        //    return ApplyConstraintReqCounterClockwise((next - 1 + Edges.Count) % Edges.Count, firstMoved);
        //}
    }
}
