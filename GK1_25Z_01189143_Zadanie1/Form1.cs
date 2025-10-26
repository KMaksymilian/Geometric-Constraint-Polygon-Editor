using System;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using System.Xml;

namespace GK1_25Z_01189143_Zadanie1
{
    public partial class Form1 : Form
    {
        private CanvasManager canvas;
        private PolygonManager polygon;

        bool isMovingPolygon = false;
        Point lastMousePos;

        bool isDragging = false;
        Vertex? selectedVertex = null;
        public Form1()
        {
            InitializeComponent();
            this.DoubleBuffered = true;
            canvas = new CanvasManager(ClientSize.Width, ClientSize.Height, this.BackColor);
            polygon = new PolygonManager(canvas);
            this.Resize += Form1_Resize;
            this.MouseDown += Form1_MouseDown;
            this.MouseMove += Form1_MouseMove;
            this.MouseUp += Form1_MouseUp;
            SetupPolygon();
        }

        private void SetupPolygon()
        {
            int h = ClientSize.Height, w = ClientSize.Width;
            var positions = new[]
            {
                new Point(w/3, h/3),
                new Point(2*w/3, h/3),
                new Point(w/2, 2*h/3)
            };

            foreach (var p in positions)
            {
                var v = new Vertex(p.X, p.Y);
                polygon.AddVertex(v);
            }
            polygon.RedrawAll();
            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            canvas.DrawTo(e.Graphics);
        }

        private void Form1_Resize(object? sender, EventArgs e)
        {
            if (ClientSize.Width <= 0 || ClientSize.Height <= 0)
                return;

            canvas.Resize(ClientSize.Width, ClientSize.Height);
            Invalidate();
        }

        private void Form1_MouseUp(object? sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left && isMovingPolygon)
                isMovingPolygon = false;
            else if (e.Button == MouseButtons.Left && isDragging)
            {
                isDragging = false;
                selectedVertex = null;
            }
        }

        private void Form1_MouseMove(object? sender, MouseEventArgs e)
        {
            if (isMovingPolygon)
            {
                int dx = e.X - lastMousePos.X;
                int dy = e.Y - lastMousePos.Y;
                lastMousePos = e.Location;

                polygon.MoveAllVertex(dx, dy);
            }
            else if (isDragging && selectedVertex != null)
                polygon.MoveVertex(selectedVertex, this.PointToClient(Cursor.Position));

            Invalidate();
        }

        private void Form1_MouseDown(object? sender, MouseEventArgs e)
        {
            int maxDistance = 10;
            if (e.Button == MouseButtons.Right)
            {

                Vertex? v = polygon.FindNearestVertex(e.Location, maxDistance);
                if (v != null && v.Type == TypeOfVertex.Normal) VertexMenu(e.Location, v);
                else
                {
                    Edge? nearest = polygon.FindNearestLine(e.Location, maxDistance);
                    if(nearest != null) EdgeMenu(e.Location, nearest);
                }
            }
            else if (e.Button == MouseButtons.Left)
            {
                Vertex? v = polygon.FindNearestVertex(e.Location, maxDistance);
                if (v != null)
                {
                    selectedVertex = v;
                    isDragging = true;
                }
                else
                {
                    isMovingPolygon = true;
                    lastMousePos = e.Location;
                }
            }
        }

        private void EdgeMenu(Point location, Edge nearest)
        {
            Point snapPoint = MathHelper.ProjectPointOntoLine(location, nearest.A.Position, nearest.B.Position);

            ContextMenuStrip menu = new ContextMenuStrip();
            menu.Items.Add("Dodaj punkt", null, (s, ev) =>
            {
                Vertex v = new Vertex(nearest.MidPoint.X, nearest.MidPoint.Y);
                polygon.AddVertexOnEdge(v, nearest);
                Invalidate();
            });

            var curveItem = new ToolStripMenuItem("Krzywa (Bézier)");
            curveItem.Checked = nearest.Type == TypeOfEdge.Bezier;
            curveItem.Click += (s, ev) =>
            {
                if (nearest.Type == TypeOfEdge.Line)
                    polygon.MakeEdgeBezier(nearest);
                else
                    polygon.MakeEdgeLine(nearest);

                Invalidate();
            };

            menu.Items.Add(curveItem);

            var advancedItem = new ToolStripMenuItem("Ograniczenia");

            advancedItem.DropDownItems.Add(
                MakeConstraintItem("Brak", new NoneConstraint(), () =>
                {
                    polygon.SetConstraintOnEdge(nearest, new NoneConstraint());
                    Invalidate();
                }, nearest)
            );

            // Pionowa
            advancedItem.DropDownItems.Add(
                MakeConstraintItem("Pionowa", new VerticalConstraint(), () =>
                {
                    int index = polygon.Edges.IndexOf(nearest);
                    if (polygon.Edges[(index + 1) % polygon.Edges.Count].ConstraintStrategy.GetType() == new VerticalConstraint().GetType() || 
                            polygon.Edges[(index - 1 + polygon.Edges.Count) % polygon.Edges.Count].ConstraintStrategy.GetType() == new VerticalConstraint().GetType())
                    {
                        MessageBox.Show("Nie można ustawić dwóch sąsiednich odcinków jako pionowe.", "Błąd ograniczenia", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }
                    polygon.SetConstraintOnEdge(nearest, new VerticalConstraint());
                    Invalidate();
                }, nearest)
            );

            // Skośna
            advancedItem.DropDownItems.Add(
                MakeConstraintItem("Skośna 45°", new DiagonalConstraint(), () =>
                {
                    polygon.SetConstraintOnEdge(nearest, new DiagonalConstraint());
                    Invalidate();
                }, nearest)
            );

            // Stała długość – popup z możliwością edycji
            var constItem = MakeConstraintItem("Długość...", new ConstConstraint(), () =>
            {
                double currentLength = nearest.LengthConstraint;
                string input = Microsoft.VisualBasic.Interaction.InputBox(
                    "Podaj długość odcinka:",
                    "Ograniczenie długości",
                    currentLength.ToString("0")
                );

                if (double.TryParse(input, out double newLength) && newLength > 0)
                {
                    nearest.LengthConstraint = newLength;
                    polygon.SetConstraintOnEdge(nearest, new ConstConstraint());
                    Invalidate();
                }
            }, nearest);
            advancedItem.DropDownItems.Add(constItem);

            menu.Items.Add(advancedItem);
            menu.Show(this, snapPoint);
        }

        private void VertexMenu(Point location, Vertex v)
        {
            Point snapPoint = new Point(v.Position.X, v.Position.Y);
            ContextMenuStrip menu = new ContextMenuStrip();
            menu.Items.Add("Usuń wierzchoek", null, (s, ev) => { polygon.RemoveVertex(v); Invalidate(); });

            var advancedItem = new ToolStripMenuItem("Ciągłości");

            advancedItem.DropDownItems.Add(
                MakeConstraintItem("G0", new ContinuityG0(), () =>
                {
                    polygon.SetContinuity(v, new ContinuityG0());
                    Invalidate();
                }, v)
            );

            advancedItem.DropDownItems.Add(
                MakeConstraintItem("G1", new ContinuityG1(), () =>
                {
                    polygon.SetContinuity(v, new ContinuityG1());
                    Invalidate();
                }, v)
            );

            advancedItem.DropDownItems.Add(
                MakeConstraintItem("C1", new ContinuityC1(), () =>
                {
                    polygon.SetContinuity(v, new ContinuityC1());
                    Invalidate();
                }, v)
            );
            menu.Items.Add(advancedItem);
            menu.Show(this, snapPoint);
        }

        private ToolStripMenuItem MakeConstraintItem(string text, IVisitable c, Action onClick, object nearest)
        {
            var item = new ToolStripMenuItem(text);
                
            if (nearest is Edge edge) item.Checked = edge.ConstraintStrategy.GetType() == c.GetType();
            else if (nearest is Vertex vertex) item.Checked = vertex.ContinuityStrategy.GetType() == c.GetType();
            item.Click += (s, ev) => onClick();
            return item;
        }

        private void restartToolStripMenuItem_Click_1(object sender, EventArgs e)
        {
            polygon.Dispose();
            canvas.Clear();
            SetupPolygon();
        }

        private void lib_CheckedChanged(object sender, EventArgs e)
        {
            if (lib.Checked)
            {
                canvas.useLibDrawing = true;
                polygon.RedrawAll();
                Invalidate();
            }
        }

        private void self_CheckedChanged(object sender, EventArgs e)
        {
            if (self.Checked)
            {
                canvas.useLibDrawing = false;
                polygon.RedrawAll();
                Invalidate();
            }
        }
    }

}
