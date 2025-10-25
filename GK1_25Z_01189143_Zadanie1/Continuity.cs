using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GK1_25Z_01189143_Zadanie1
{
    interface IContinuityVisitor
    {
        void VisitG0(Edge edge, Vertex vertex, Vertex ctrl, bool isStart);
        void VisitG1(Edge edge, Vertex vertex, Vertex ctrl, bool isStart);
        void VisitC1(Edge edge, Vertex vertex, Vertex ctrl, bool isStart);
    }

    class ContinuityApplierVisitorVertexMoved : IContinuityVisitor
    {
        public void VisitG0(Edge edge, Vertex vertex, Vertex ctrl, bool isStart) {}
        public void VisitG1(Edge edge, Vertex vertex, Vertex ctrl, bool isStart)
        {
            Edge? adjacent = (Edge?)vertex.Observers.FirstOrDefault(e => e != edge);
            if (adjacent == null) return;

            Vertex refPoint = adjacent.Type == TypeOfEdge.Line
                ? (adjacent.A == vertex ? adjacent.B : adjacent.A)
                : (isStart ? adjacent.Ctrl2 ?? adjacent.B : adjacent.Ctrl1 ?? adjacent.A);

            int refDx = vertex.Position.X - refPoint.Position.X;
            int refDy = vertex.Position.Y - refPoint.Position.Y;

            double scale = MathHelper.Distance(vertex.Position, ctrl.Position) /
                           MathHelper.Distance(refPoint.Position, vertex.Position);

            ctrl.MoveTo(new Point(
                vertex.Position.X + (int)(refDx * scale),
                vertex.Position.Y + (int)(refDy * scale)
            ));
        }

        public void VisitC1(Edge edge, Vertex vertex, Vertex ctrl, bool isStart)
        {
            Edge? adjacent = (Edge?)vertex.Observers.FirstOrDefault(e => e != edge);
            if (adjacent == null) return;

            Vertex refPoint = adjacent.Type == TypeOfEdge.Line
                ? (adjacent.A == vertex ? adjacent.B : adjacent.A)
                : (isStart ? adjacent.Ctrl2 ?? adjacent.B : adjacent.Ctrl1 ?? adjacent.A);

            int refDx = vertex.Position.X - refPoint.Position.X;
            int refDy = vertex.Position.Y - refPoint.Position.Y;

            double scale = 1.0 / 3.0;
            ctrl.MoveTo(new Point(
                vertex.Position.X + (int)(refDx * scale),
                vertex.Position.Y + (int)(refDy * scale)
            ));
        }
    }

    class ContinuityApplierVisitorCtrlMoved : IContinuityVisitor
    {
        public void VisitC1(Edge edge, Vertex vertex, Vertex ctrl, bool isStart)
        {
            Edge? otherEdge = (Edge?)vertex.Observers.FirstOrDefault(e => e != edge);
            if (otherEdge == null) return;

            if (otherEdge.Type == TypeOfEdge.Bezier) return;

            int dx, dy;
            switch (otherEdge.ConstraintStrategy)
            {
                case VerticalConstraint _:
                    vertex.MoveTo(new Point(ctrl.Position.X, vertex.Position.Y));
                    break;
                case DiagonalConstraint _:
                    dx = vertex.Position.X - ctrl.Position.X;
                    dy = vertex.Position.Y - ctrl.Position.Y;
                    int sy = Math.Sign(dy);
                    vertex.MoveTo(new Point(vertex.Position.X, ctrl.Position.Y + sy * Math.Abs(dx)));
                    break;
                default:
                    dx = vertex.Position.X - ctrl.Position.X;
                    dy = vertex.Position.Y - ctrl.Position.Y;
                    Vertex toMove = otherEdge.OtherVertex(vertex);
                    double scale = MathHelper.Distance(ctrl.Position, vertex.Position) / MathHelper.Distance(vertex.Position, toMove.Position);
                    toMove.MoveTo(new Point(
                        vertex.Position.X + (int)(dx * scale),
                        vertex.Position.Y + (int)(dy * scale)
                    ));
                    break;

            }
        }

        public void VisitG0(Edge edge, Vertex vertex, Vertex ctrl, bool isStart) { }

        public void VisitG1(Edge edge, Vertex vertex, Vertex ctrl, bool isStart)
        {
            Edge? otherEdge = (Edge?)vertex.Observers.FirstOrDefault(e => e != edge);
            if (otherEdge == null) return;

            if (otherEdge.Type == TypeOfEdge.Bezier) return;

            switch (otherEdge.ConstraintStrategy)
            {
                case VerticalConstraint _:
                    vertex.MoveToWithoutNotify(new Point(ctrl.Position.X, vertex.Position.Y));
                    
                    break;
                case DiagonalConstraint _:
                    int dxh = vertex.Position.X - ctrl.Position.X;
                    int dyh = vertex.Position.Y - ctrl.Position.Y;
                    int sy = Math.Sign(dyh);
                    vertex.MoveToWithoutNotify(new Point(vertex.Position.X, ctrl.Position.Y + sy * Math.Abs(dxh)));
                    break;
                default:
                    break;

            }
            int dx = vertex.Position.X - ctrl.Position.X;
            int dy = vertex.Position.Y - ctrl.Position.Y;
            Vertex toMove = otherEdge.OtherVertex(vertex);
            double scale = 3.0;
            toMove.MoveTo(new Point(
                vertex.Position.X + (int)(dx * scale),
                vertex.Position.Y + (int)(dy * scale)
            ));
        }
    }

    interface IContinuityVisitable
    {
        void Accept(IContinuityVisitor visitor, Edge edge, Vertex vertex, Vertex ctrl, bool isStart);
        string GetName();
    }

    class ContinuityG0 : IContinuityVisitable
    {
        public void Accept(IContinuityVisitor visitor, Edge edge, Vertex vertex, Vertex ctrl, bool isStart)
            => visitor.VisitG0(edge, vertex, ctrl, isStart);

        public string GetName() => "G0";
    }

    class ContinuityG1 : IContinuityVisitable
    {
        public void Accept(IContinuityVisitor visitor, Edge edge, Vertex vertex, Vertex ctrl, bool isStart)
            => visitor.VisitG1(edge, vertex, ctrl, isStart);

        public string GetName() => "G1";
    }

    class ContinuityC1 : IContinuityVisitable
    {
        public void Accept(IContinuityVisitor visitor, Edge edge, Vertex vertex, Vertex ctrl, bool isStart)
            => visitor.VisitC1(edge, vertex, ctrl, isStart);

        public string GetName() => "C1";
    }

}
