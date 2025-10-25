using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GK1_25Z_01189143_Zadanie1
{
    internal class MoveTester
    {
        private List<Vertex> Vertices { get; set; }
        private List<Edge> Edges { get; set; }
        private static MoveTester? instance = null;

        private MoveTester(List<Vertex> vertices, List<Edge> edges)
        {
            Vertices = CloneVertices(vertices);
            Edges = CloneEdges(edges, Vertices);
        }

        internal static MoveTester GetInstance(List<Vertex> vertices, List<Edge> edges)
        {
            if (instance == null)
            {
                var clonedVertices = CloneVertices(vertices);
                var clonedEdges = CloneEdges(edges, clonedVertices);
                instance = new MoveTester(clonedVertices, clonedEdges);
            }
            else
            {
                instance.Vertices = CloneVertices(vertices);
                instance.Edges = CloneEdges(edges, instance.Vertices);
            }

            return instance;
        }
        private static List<Vertex> CloneVertices(List<Vertex> source)
        {
            return source.Select(v =>
            {
                Vertex clone = new Vertex(v.Position.X, v.Position.Y)
                {
                    Type = v.Type,
                    Moved = v.Moved,
                    ContinuityStrategy = v.ContinuityStrategy
                };
                return clone;
            }).ToList();
        }

        private static List<Edge> CloneEdges(List<Edge> source, List<Vertex> newVertices)
        {
            return source.Select(e =>
            {
                Vertex newA = newVertices.First(v => v.Position == e.A.Position && v.Type == TypeOfVertex.Normal);
                Vertex newB = newVertices.First(v => v.Position == e.B.Position && v.Type == TypeOfVertex.Normal);

                newA.AddObserver(e);
                newB.AddObserver(e);

                Edge newEdge = new Edge(newA, newB)
                {
                    Type = e.Type,
                    ConstraintStrategy = e.ConstraintStrategy,
                    LengthConstraint = e.LengthConstraint
                };

                if (e.Ctrl1 != null)
                    newEdge.Ctrl1 = newVertices.First(v => v.Position == e.Ctrl1.Position && v.Type == TypeOfVertex.BCtrl);

                if (e.Ctrl2 != null)
                    newEdge.Ctrl2 = newVertices.First(v => v.Position == e.Ctrl2.Position && v.Type == TypeOfVertex.BCtrl);

                newEdge.ConstraintStrategy = e.ConstraintStrategy;
                return newEdge;
            }).ToList();
        }
        internal bool MakeTest(Vertex v, Point location)
        {
            Vertex? testVertex = Vertices.FirstOrDefault(x => x.Position == v.Position);
            if (testVertex == null) return false;

            testVertex.MoveTo(location);

            if (testVertex.Type == TypeOfVertex.BCtrl)
            {
                Edge edge = Edges.First(o => o.Ctrl1 == testVertex || o.Ctrl2 == testVertex);
                Vertex mainVertex = edge.Ctrl1 == testVertex ? edge.A : edge.B;
                edge.ApplyContinuityCtrlMoved(mainVertex, testVertex, mainVertex == edge.A);
            }   

            return Edges.All(e => e.ConstraintStrategy.CheckConstrain(e));
        }

        internal bool SetConstraintTest(Edge nearest, IConstraintVisitable constraint)
        {
            Edge? testEdge = Edges.FirstOrDefault(e =>
                e.A.Position == nearest.A.Position &&
                e.B.Position == nearest.B.Position);
            if (testEdge == null) return false;
            testEdge.SetConstraint(constraint);
            return Edges.All(e => e.ConstraintStrategy.CheckConstrain(e));
        }
    }

}
