using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
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
        private readonly CanvasManager canvas;
        public List<Vertex> Vertices { get; } = new();
        public List<Edge> Edges { get; } = new();
        public PolygonManager(CanvasManager canvas) => this.canvas = canvas;
        internal void Dispose()
        {
            Edges.Clear();
            Vertices.Clear();
        }
        public void RedrawAll()
        {
            canvas.Clear();
            foreach (var edge in Edges)
            {
                if (edge.Type == TypeOfEdge.Line) canvas.DrawEdge(edge, Color.Red);
                else if (edge.Type == TypeOfEdge.Bezier) canvas.DrawBezierEdge(edge, Color.Red);
                Vertices.ForEach(v => canvas.DrawVertex(v, Color.Blue));
            }
            Vertices.ForEach(v => v.Moved = false);
        }
        public void AddVertex(Vertex v)
        {
            Vertices.Add(v);
            if (Vertices.Count > 1) Edges.Add(new Edge(Vertices[^2], Vertices[^1]));
            if (Vertices.Count > 2) Edges.Add(new Edge(Vertices[^1], Vertices[0]));
            RedrawAll();
        }

        public void AddVertexOnEdge(Vertex vertex, Edge edge)
        {
            int index = Edges.IndexOf(edge);
            if (index == -1) return;

            var first = new Edge(edge.A, vertex);
            var second = new Edge(vertex, edge.B);

            Edges[index] = first;
            Edges.Insert(index + 1, second);
            Vertices.Add(vertex);

            RedrawAll();
        }

        public void RemoveVertex(Vertex v)
        {
            Edge? prev = Edges.FirstOrDefault(e => e.B == v);
            Edge? next = Edges.FirstOrDefault(e => e.A == v);

            if (prev == null || next == null)
                return;

            Vertices.Remove(v);

            var newEdge = new Edge(prev.A, next.B);

            RemoveControlPoints(prev);
            RemoveControlPoints(next);

            int insertIndex = Edges.IndexOf(prev);
            Edges.Remove(prev);
            Edges.Remove(next);
            Edges.Insert(insertIndex, newEdge);

            RedrawAll();
        }

        private void RemoveControlPoints(Edge e)
        {
            if (e.Ctrl1 != null) Vertices.Remove(e.Ctrl1);
            if (e.Ctrl2 != null) Vertices.Remove(e.Ctrl2);
        }


        internal void MoveAllVertex(int dx, int dy)
        {
            Vertices.ForEach(v => v.MoveToWithoutNotify(dx, dy));
            RedrawAll();
        }

        internal Edge? FindNearestLine(Point click, int maxDistance)
        {
            Edge? nearest = null;
            double minDist = maxDistance;

            foreach (var edge in Edges)
            {
                double dist = MathHelper.DistancePointToSegment(click, edge.A.Position, edge.B.Position);
                if (dist <= minDist)
                {
                    minDist = dist;
                    nearest = edge;
                }
            }
            return nearest;
        }
        internal Vertex? FindNearestVertex(Point click, int maxDistance)
        {
            Vertex? nearest = null;
            double minDist = maxDistance;
            foreach (var vertex in Vertices)
            {
                double dist = MathHelper.Distance(click, vertex.Position);
                if (dist <= minDist)
                {
                    minDist = dist;
                    nearest = vertex;
                }
            }
            return nearest;
        }

        internal void MoveVertex(Vertex v, Point cursor)
        { 
            if(MoveTester.GetInstance(Vertices, Edges).MakeTest(v, cursor))
            {
                v.MoveTo(cursor);

                if (v.Type == TypeOfVertex.BCtrl)
                {
                    Edge edge = Edges.First(o => o.Ctrl1 == v || o.Ctrl2 == v);
                    Vertex mainVertex = edge.Ctrl1 == v ? edge.A : edge.B;
                    edge.ApplyContinuityCtrlMoved(mainVertex, v, mainVertex == edge.A);
                }
                foreach (var item in Edges.Where(e => e.Type == TypeOfEdge.Bezier))
                    item.ApplyContinuity();
            }
            else
            {
                int dx = cursor.X - v.Position.X;
                int dy = cursor.Y - v.Position.Y;
                MoveAllVertex(dx, dy);
            }

            RedrawAll();
        }
        internal void MakeEdgeLine(Edge e)
        {
            (Vertex? c1, Vertex? c2) = e.MakeLine();
            if (c1 != null) Vertices.Remove(c1);
            if (c2 != null) Vertices.Remove(c2);
            RedrawAll();
        }
        internal void MakeEdgeBezier(Edge e)
        {
            (Vertex c1, Vertex c2) = e.MakeBezier();
            Vertices.Add(c1);
            Vertices.Add(c2);
            RedrawAll();
        }
        internal void SetContinuity(Vertex v, IContinuityVisitable continuity)
        {
            v.ContinuityStrategy = continuity;

            Edge? bezierEdge = Edges.FirstOrDefault(e =>
                e.Type == TypeOfEdge.Bezier && (e.A == v || e.B == v));

            if (bezierEdge != null)
            {
                bezierEdge.ApplyContinuity();
                RedrawAll();
            }
        }


        internal void SetConstraintOnEdge(Edge nearest, IConstraintVisitable constraint)
        {
            if (MoveTester.GetInstance(Vertices, Edges).SetConstraintTest(nearest, constraint))
            {
                nearest.SetConstraint(constraint);
                Edges.Where(e => e.Type == TypeOfEdge.Bezier).ToList().ForEach(e => e.ApplyContinuity());
                RedrawAll();
            }
            else MessageBox.Show(
                    "Nadanie tego ograniczenia spowoduje konflikt z innym ograniczeniem.",
                    "Błąd ograniczenia",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );

        }

    }
}
