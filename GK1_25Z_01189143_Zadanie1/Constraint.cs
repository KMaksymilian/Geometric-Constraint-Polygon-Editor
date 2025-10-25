using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;

namespace GK1_25Z_01189143_Zadanie1
{
    interface IConstraintVisitor
    {
        void VisitVertical(Edge edge, Vertex movedVertex);
        void VisitDiagonal(Edge edge, Vertex movedVertex);
        void VisitConst(Edge edge, Vertex movedVertex);
        void VisitNone(Edge edge, Vertex movedVertex);
    }

    class ConstraintApplierVisitor : IConstraintVisitor
    {
        public void VisitVertical(Edge edge, Vertex x)
        {
            Vertex toMove = edge.OtherVertex(x);
            toMove.MoveTo(new Point(toMove.Position.X, x.Position.Y));
        }

        public void VisitDiagonal(Edge edge, Vertex x)
        {
            Vertex toMove = edge.OtherVertex(x);
            int dx = toMove.Position.X - x.Position.X;
            int dy = toMove.Position.Y - x.Position.Y;
            int sy = Math.Sign(dy);
            toMove.MoveTo(new Point(toMove.Position.X, x.Position.Y + sy * Math.Abs(dx)));
        }

        public void VisitConst(Edge edge, Vertex x)
        {
            Vertex toMove = edge.OtherVertex(x);
            double length = MathHelper.Distance(x.Position, toMove.Position);
            if (length == 0) return;

            double scale = edge.LengthConstraint / length;
            int newX = x.Position.X + (int)((toMove.Position.X - x.Position.X) * scale);
            int newY = x.Position.Y + (int)((toMove.Position.Y - x.Position.Y) * scale);
            toMove.MoveTo(new Point(newX, newY));
        }

        public void VisitNone(Edge edge, Vertex x) { }
    }

    interface IConstraintVisitable
    {
        void Accept(IConstraintVisitor visitor, Edge edge, Vertex movedVertex);
        string GetName(Edge edge);
        void ApplyFirstTime(Edge edge);
        bool CheckConstrain(Edge edge);
    }

    class VerticalConstraint : IConstraintVisitable
    {
        public void Accept(IConstraintVisitor visitor, Edge edge, Vertex movedVertex)
            => visitor.VisitVertical(edge, movedVertex);

        public void ApplyFirstTime(Edge edge)
        {
            int dist = (int) MathHelper.Distance(edge.A.Position, edge.B.Position)/2;
            int sign = Math.Sign(edge.B.Position.Y - edge.A.Position.Y);
            edge.B.MoveToWithoutNotify(new Point(edge.MidPoint.X, edge.B.Position.Y + sign * dist));
            edge.A.MoveTo(new Point(edge.MidPoint.X, edge.A.Position.Y - sign * dist));
        }

        public bool CheckConstrain(Edge edge) => edge.A.Position.X == edge.B.Position.X;
        public string GetName(Edge edge) => "V";
    }

    class DiagonalConstraint : IConstraintVisitable
    {
        public void Accept(IConstraintVisitor visitor, Edge edge, Vertex movedVertex)
            => visitor.VisitDiagonal(edge, movedVertex);

        public void ApplyFirstTime(Edge edge)
        {
            int dist = (int)Math.Sqrt(MathHelper.Distance(edge.A.Position, edge.B.Position));
            int signX = Math.Sign(edge.B.Position.X - edge.A.Position.X);
            int signY = Math.Sign(edge.B.Position.Y - edge.A.Position.Y);
            edge.B.MoveToWithoutNotify(new Point(edge.MidPoint.X + signX * dist, edge.MidPoint.Y + signY * dist));
            edge.A.MoveTo(new Point(edge.MidPoint.X - signX * dist, edge.MidPoint.Y - signY * dist));
        }

        public bool CheckConstrain(Edge edge)
        {
            int dx = edge.B.Position.X - edge.A.Position.X;
            int dy = edge.B.Position.Y - edge.A.Position.Y;

         
            return Math.Abs(Math.Abs(dx) - Math.Abs(dy)) <= 1;
        }

        public string GetName(Edge edge) => "D";
    }

    class ConstConstraint : IConstraintVisitable
    {
        public void Accept(IConstraintVisitor visitor, Edge edge, Vertex movedVertex)
            => visitor.VisitConst(edge, movedVertex);

        public void ApplyFirstTime(Edge edge)
        {
            double scale = edge.LengthConstraint / MathHelper.Distance(edge.A.Position, edge.B.Position);
            int dx = (int)((edge.B.Position.X - edge.A.Position.X) * scale / 2);
            int dy = (int)((edge.B.Position.Y - edge.A.Position.Y) * scale / 2);
            edge.A.MoveToWithoutNotify(new Point(edge.MidPoint.X - dx, edge.MidPoint.Y - dy));
            edge.B.MoveTo(new Point(edge.MidPoint.X + dx, edge.MidPoint.Y + dy));
        }

        public bool CheckConstrain(Edge edge) => Math.Abs(MathHelper.Distance(edge.A.Position, edge.B.Position) - edge.LengthConstraint) < 1e-5;

        public string GetName(Edge edge) => $"{edge.LengthConstraint}";
    }

    class NoneConstraint : IConstraintVisitable
    {
        public void Accept(IConstraintVisitor visitor, Edge edge, Vertex movedVertex)
            => visitor.VisitNone(edge, movedVertex);
        public void ApplyFirstTime(Edge edge) { }

        public bool CheckConstrain(Edge edge) => true;

        public string GetName(Edge edge) => "None";
    }
}
