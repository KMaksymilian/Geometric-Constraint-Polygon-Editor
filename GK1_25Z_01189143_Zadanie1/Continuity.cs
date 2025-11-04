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
                ? adjacent.OtherVertex(vertex)
                : (isStart ? adjacent.Ctrl2 ?? adjacent.B : adjacent.Ctrl1 ?? adjacent.A);

            int refDx = vertex.Position.X - refPoint.Position.X;
            int refDy = vertex.Position.Y - refPoint.Position.Y;

            double refLength = MathHelper.Distance(vertex.Position, refPoint.Position);


            double ctrlLength = MathHelper.Distance(vertex.Position, ctrl.Position);

            if (refLength < 1e-5 || ctrlLength < 1e-5)
                return; 

            double dirX = refDx / refLength;
            double dirY = refDy / refLength;

            double targetLength = ctrlLength; 

            int newX = (int)Math.Round(vertex.Position.X + dirX * targetLength);
            int newY = (int)Math.Round(vertex.Position.Y + dirY * targetLength);

            double dot = (ctrl.Position.X - vertex.Position.X) * refDx +
                         (ctrl.Position.Y - vertex.Position.Y) * refDy;
            if (dot > 0) 
            {
                newX = (int)Math.Round(vertex.Position.X + dirX * targetLength);
                newY = (int)Math.Round(vertex.Position.Y + dirY * targetLength);
            }


            ctrl.MoveToWithoutNotify(new Point(newX, newY));
        }

        public void VisitC1(Edge edge, Vertex vertex, Vertex ctrl, bool isStart)
        {
            double C1Scale = 1.0 / 3.0;
            Edge? adjacent = (Edge?)vertex.Observers.FirstOrDefault(e => e != edge);
            if (adjacent == null) return;

            Vertex refPoint = adjacent.Type == TypeOfEdge.Line
                ? (adjacent.A == vertex ? adjacent.B : adjacent.A)
                : (isStart ? adjacent.Ctrl2 ?? adjacent.B : adjacent.Ctrl1 ?? adjacent.A);

            int refDx = vertex.Position.X - refPoint.Position.X;
            int refDy = vertex.Position.Y - refPoint.Position.Y;

            if (refPoint.Type == TypeOfVertex.BCtrl)
            {
                C1Scale = 1;
            }

            ctrl.MoveTo(new Point(
                vertex.Position.X + (int)(refDx * C1Scale),
                vertex.Position.Y + (int)(refDy * C1Scale)
            ));
        }
    }

    class ContinuityApplierVisitorCtrlMoved : IContinuityVisitor
    {
        public void VisitG0(Edge edge, Vertex vertex, Vertex ctrl, bool isStart) { }

        public void VisitG1(Edge edge, Vertex vertex, Vertex ctrl, bool isStart)
        {
            ApplyContinuityForVertexMoved(edge, vertex, ctrl, isStart, isC1: false);
        }

        public void VisitC1(Edge edge, Vertex vertex, Vertex ctrl, bool isStart)
        {
            ApplyContinuityForVertexMoved(edge, vertex, ctrl, isStart, isC1: true);
        }

        private void ApplyContinuityForVertexMoved(Edge edge, Vertex vertex, Vertex ctrl, bool isStart, bool isC1)
        {
            Edge? otherEdge = vertex.Observers.FirstOrDefault(e => e != edge) as Edge;
            if (otherEdge == null || otherEdge.Type == TypeOfEdge.Bezier) return;

            switch (otherEdge.ConstraintStrategy)
            {
                case VerticalConstraint _:
                    vertex.MoveToWithoutNotify(new Point(ctrl.Position.X, vertex.Position.Y));
                    break;

                case DiagonalConstraint _:
                    int dx = vertex.Position.X - ctrl.Position.X;
                    int dy = vertex.Position.Y - ctrl.Position.Y;
                    int sy = Math.Sign(dy);
                    vertex.MoveToWithoutNotify(new Point(vertex.Position.X, ctrl.Position.Y + sy * Math.Abs(dx)));
                    break;

                
                default:
                    break;
            }

            int vecX = vertex.Position.X - ctrl.Position.X;
            int vecY = vertex.Position.Y - ctrl.Position.Y;

            Vertex toMove = otherEdge.OtherVertex(vertex);

            double length = MathHelper.Distance(ctrl.Position, vertex.Position);
            double refLength = MathHelper.Distance(vertex.Position, toMove.Position);
            if (length < 1e-5) return;
            double scale = isC1 ? 3.0 : refLength / length;

            if (otherEdge.ConstraintStrategy is ConstConstraint && isC1)
            {
                int dx = toMove.Position.X - vertex.Position.X;
                int dy = toMove.Position.Y - vertex.Position.Y;
                int newX = (int)Math.Round(ctrl.Position.X + dx / scale);
                int newY = (int)Math.Round(ctrl.Position.Y + dy / scale);
                vertex.MoveTo(new Point(newX, newY));
            }
            else
            {

               

                int newX = (int)Math.Round(vertex.Position.X + vecX * scale);
                int newY = (int)Math.Round(vertex.Position.Y + vecY * scale);

                toMove.MoveTo(new Point(newX, newY));
            }

        }
    }


    interface IContinuityVisitable : IVisitable
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
