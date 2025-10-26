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
            toMove.MoveTo(new Point(x.Position.X, toMove.Position.Y));
        }

        public void VisitDiagonal(Edge edge, Vertex x)
        {
            Vertex toMove = edge.OtherVertex(x);
            int dx = toMove.Position.X - x.Position.X;
            int dy = toMove.Position.Y - x.Position.Y;
            int sy = Math.Sign(dy);
            if (dy == 0) sy = 1;
            toMove.MoveTo(new Point(toMove.Position.X, x.Position.Y + sy * Math.Abs(dx)));
        }

        public void VisitConst(Edge edge, Vertex x)
        {
            Vertex toMove = edge.OtherVertex(x);
            double length = MathHelper.Distance(x.Position, toMove.Position);
            if (length == 0) return;

            double scale = edge.LengthConstraint / length;
            int newX = (int)Math.Round(x.Position.X + ((toMove.Position.X - x.Position.X) * scale));
            int newY = (int)Math.Round(x.Position.Y + ((toMove.Position.Y - x.Position.Y) * scale));
            toMove.MoveTo(new Point(newX, newY));
        }

        public void VisitNone(Edge edge, Vertex x) { }
    }

    interface IVisitable
    { }

    interface IConstraintVisitable : IVisitable
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
            int midX = edge.MidPoint.X;
            int half = (int)Math.Round(MathHelper.Distance(edge.A.Position, edge.B.Position) / 2);

            edge.A.MoveToWithoutNotify(new Point(midX, edge.MidPoint.Y + half));
            edge.B.MoveTo(new Point(midX, edge.MidPoint.Y - half));
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
            Point mid = edge.MidPoint;


            int dx = edge.A.Position.X - edge.MidPoint.X;
            int dy = edge.A.Position.Y - edge.MidPoint.Y;

            double length = Math.Sqrt(dx * dx + dy * dy);
            if (length < 1e-5) length = 1; 

            double dist = MathHelper.Distance(edge.A.Position, edge.MidPoint) / Math.Sqrt(2);

            int sx = Math.Sign(dx);
            int sy = Math.Sign(dy);
            if (sx == 0) sx = 1;
            if (sy == 0) sy = 1;

            int newDx = (int)Math.Round(dist * sx);
            int newDy = (int)Math.Round(dist * sy);

            edge.A.MoveToWithoutNotify(new Point(mid.X + newDx, mid.Y + newDy));
            edge.B.MoveTo(new Point(mid.X - newDx, mid.Y - newDy));
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

        public bool CheckConstrain(Edge edge) => Math.Abs(MathHelper.Distance(edge.A.Position, edge.B.Position) - edge.LengthConstraint) < 1;

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
