using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GK1_25Z_01189143_Zadanie1
{
    internal class MathHelper
    {
        public static double Distance(Point a, Point b)
        {
            int deltaX = b.X - a.X;
            int deltaY = b.Y - a.Y;
            return Math.Sqrt(deltaX * deltaX + deltaY * deltaY);
        }


        internal static double DistancePointToSegment(Point p, Point a, Point b)
        {
            double dx = b.X - a.X;
            double dy = b.Y - a.Y;

            if (dx == 0 && dy == 0)
                return Distance(p, a);

            double t = ((p.X - a.X) * dx + (p.Y - a.Y) * dy) / (dx * dx + dy * dy);
            t = Math.Max(0, Math.Min(1, t));

            double projX = a.X + t * dx;
            double projY = a.Y + t * dy;

            return Distance(p, new Point((int)projX, (int)projY));
        }
    }
}
