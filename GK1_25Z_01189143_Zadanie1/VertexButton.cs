using System;
using System.Collections.Generic;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GK1_25Z_01189143_Zadanie1
{
    public enum TypeOfVertex
    {
        Normal,
        BCtrl
    }

    internal class Vertex
    {
        public readonly List<IObserver> Observers = new();
        internal TypeOfVertex Type { get;  set; } = TypeOfVertex.Normal;
        public IContinuityVisitable ContinuityStrategy { get; set; } = new ContinuityC1();
        internal Point Position { get; private set; }
        internal bool Moved { get; set; } = false;
        public Vertex(int x, int y) => Position = new Point(x, y);
        public void MoveToWithoutNotify(int dx, int dy) => Position = new Point(Position.X + dx, Position.Y + dy);
        internal void MoveToWithoutNotify(Point point) => Position = point;
        private void NotifyObservers() => Observers.ForEach(o => o.Update(this));
        public void ChangeToCtrl() => Type = TypeOfVertex.BCtrl;
        public void AddObserver(IObserver observer) => Observers.Add(observer);

        public void MoveTo(Point newPos)
        {
            if (!Moved) return;
            Position = newPos;
            Moved = true;
            NotifyObservers();
        }
       
    }
}
