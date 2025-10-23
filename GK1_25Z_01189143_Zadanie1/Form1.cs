using System;
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
        VertexButton? selectedVertexButton = null;
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
                var v = new VertexButton(p.X, p.Y);
                polygon.AddVertex(v);
                Controls.Add(v);
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
            {
                isMovingPolygon = false;
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
                Invalidate();
            }
        }

        private void Form1_MouseDown(object? sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {

                Point click = e.Location;
                Edge? nearest = polygon.FindNearestLine(click, 10);

                if (nearest != null)
                {
                    Point snapPoint = ProjectPointOntoLine(click, nearest.A, nearest.B);

                    // Można tu np. pokazać marker lub tymczasowo przesunąć punkt
                    // Otwórz menu kontekstowe w tym miejscu
                    ContextMenuStrip menu = new ContextMenuStrip();
                    menu.Items.Add("Dodaj punkt", null, (s, ev) =>
                    {
                        VertexButton newVertex = new VertexButton(nearest.MidPoint.X, nearest.MidPoint.Y);
                        this.Controls.Add(newVertex);
                        polygon.AddVertexOnEdge(newVertex, nearest);
                        Invalidate();
                    });

                    var curveItem = new ToolStripMenuItem("Krzywa (Bézier)");
                    curveItem.Checked = nearest.Kind == EdgeKind.Bezier;
                    curveItem.Click += (s, ev) =>
                    {
                        if (nearest.Kind == EdgeKind.Line)
                        {
                            nearest.Kind = EdgeKind.Bezier;
                            nearest.MakeBezier(); // twoja metoda ustawiająca kontrolne punkty

                            Controls.Add(nearest.Ctrl1);
                            Controls.Add(nearest.Ctrl2);
                        }
                        else
                        {
                            nearest.Kind = EdgeKind.Line;

                            if (nearest.Ctrl1 != null) Controls.Remove(nearest.Ctrl1);
                            if (nearest.Ctrl2 != null) Controls.Remove(nearest.Ctrl2);
                        }

                        polygon.RedrawAll();
                        Invalidate();
                    };

                    menu.Items.Add(curveItem);

                    // Podmenu „Ograniczenia”
                    var advancedItem = new ToolStripMenuItem("Ograniczenia");

                    // Helper: tworzy pozycję menu z automatycznym zaznaczeniem
                    ToolStripMenuItem MakeConstraintItem(string text, LineConstraint constraint, Action onClick)
                    {
                        var item = new ToolStripMenuItem(text)
                        {
                            Checked = nearest.Constraint == constraint
                        };
                        item.Click += (s, ev) => onClick();
                        return item;
                    }

                    // None
                    advancedItem.DropDownItems.Add(
                        MakeConstraintItem("Brak", LineConstraint.None, () =>
                        {
                            polygon.SetConstraintOnEdge(nearest, LineConstraint.None);
                            Invalidate();
                        })
                    );

                    // Pionowa
                    advancedItem.DropDownItems.Add(
                        MakeConstraintItem("Pionowa", LineConstraint.Vertical, () =>
                        {
                            int index = polygon.Edges.IndexOf(nearest);
                            if (polygon.Edges[(index + 1) % polygon.Edges.Count].Constraint == LineConstraint.Vertical || polygon.Edges[(index - 1 + polygon.Edges.Count) % polygon.Edges.Count].Constraint == LineConstraint.Vertical)
                            {
                                MessageBox.Show("Nie można ustawić dwóch sąsiednich odcinków jako pionowe.", "Błąd ograniczenia", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                return;
                            }
                            polygon.SetConstraintOnEdge(nearest, LineConstraint.Vertical);
                            Invalidate();
                        })
                    );

                    // Skośna
                    advancedItem.DropDownItems.Add(
                        MakeConstraintItem("Skośna 45°", LineConstraint.Diagonal, () =>
                        {
                            polygon.SetConstraintOnEdge(nearest, LineConstraint.Diagonal);
                            Invalidate();
                        })
                    );

                    // Stała długość – popup z możliwością edycji
                    var constItem = MakeConstraintItem("Długość...", LineConstraint.Const, () =>
                    {
                        double currentLength = nearest.Length;
                        string input = Microsoft.VisualBasic.Interaction.InputBox(
                            "Podaj długość odcinka:",
                            "Ograniczenie długości",
                            currentLength.ToString("0")
                        );

                        if (double.TryParse(input, out double newLength) && newLength > 0)
                        {
                            nearest.Length = newLength;
                            polygon.SetConstraintOnEdge(nearest, LineConstraint.Const);
                            Invalidate();
                        }
                    });
                    advancedItem.DropDownItems.Add(constItem);

                    menu.Items.Add(advancedItem);
                    menu.Show(this, snapPoint);
                }
            }
            else if (e.Button == MouseButtons.Left && sender is not Button)
            {
                isMovingPolygon = true;
                lastMousePos = e.Location;
            }
        }

        private Point ProjectPointOntoLine(Point p, Button a, Button b)
        {
            Point p1 = new Point(a.Left + a.Width / 2, a.Top + a.Height / 2);
            Point p2 = new Point(b.Left + b.Width / 2, b.Top + b.Height / 2);

            double dx = p2.X - p1.X;
            double dy = p2.Y - p1.Y;

            double t = ((p.X - p1.X) * dx + (p.Y - p1.Y) * dy) / (dx * dx + dy * dy);
            t = Math.Max(0, Math.Min(1, t));

            return new Point((int)(p1.X + t * dx), (int)(p1.Y + t * dy));
        }


        internal void MouseUpBtn(object? sender, MouseEventArgs e)
        {
            if (isDragging)
            {
                isDragging = false;
                selectedVertexButton = null;
            }
        }

        internal void MouseMoveBtn(object? sender, MouseEventArgs e)
        {
            if (isDragging && selectedVertexButton != null)
            {
                polygon.MoveVertex(selectedVertexButton, this.PointToClient(Cursor.Position));
                Invalidate();

            }
        }



        internal void MouseDownBtn(object? sender, MouseEventArgs e)
        {
            if (sender is VertexButton btn)
            {
                if (e.Button == MouseButtons.Left)
                {
                    selectedVertexButton = btn;
                    isDragging = true;
                    btn.Capture = true;
                }
                else if (e.Button == MouseButtons.Right && btn.type != typeOfVertex.BCtrl)
                {
                    Point snapPoint = new Point(btn.Left + btn.Width / 2, btn.Top + btn.Height / 2);
                    ContextMenuStrip menu = new ContextMenuStrip();
                    menu.Items.Add("Usuń wierzchoek", null, (s, ev) => { polygon.RemoveVertex(btn); btn.Dispose(); Invalidate(); });
                   
                    var advancedItem = new ToolStripMenuItem("Ciągłości");

                    ToolStripMenuItem MakeContinuityItem(string text, typeOfContinuity continuity, Action onClick)
                    {
                        var item = new ToolStripMenuItem(text)
                        {
                            Checked = btn.continuity == continuity
                        };
                        item.Click += (s, ev) => onClick();
                        return item;
                    }

                    advancedItem.DropDownItems.Add(
                        MakeContinuityItem("G0", typeOfContinuity.G0, () =>
                        {
                            polygon.SetContinuity(btn, typeOfContinuity.G0);
                            Invalidate();
                        })
                    );

                    advancedItem.DropDownItems.Add(
                        MakeContinuityItem("G1", typeOfContinuity.G1, () =>
                        {
                            polygon.SetContinuity(btn, typeOfContinuity.G1);
                            Invalidate();
                        })
                    );

                    advancedItem.DropDownItems.Add(
                        MakeContinuityItem("C1", typeOfContinuity.C1, () =>
                        {
                            polygon.SetContinuity(btn, typeOfContinuity.C1);
                            Invalidate();
                        })
                    );
                    menu.Items.Add(advancedItem);
                    menu.Show(this, snapPoint);
                }
            }

        }


        private void restartToolStripMenuItem_Click_1(object sender, EventArgs e)
        {
            foreach (var btn in polygon.Vertices)
            {
                this.Controls.Remove(btn);
                btn.Dispose();
            }
            polygon.Dispose();
            canvas.Clear();
            SetupPolygon();
        }

        //private void MakeSetup()
        //{
        //    int height = this.ClientSize.Height;
        //    int width = this.ClientSize.Width;

        //    var positions = new List<(int y, int x)>
        //    {
        //        (height/3, width/3),
        //        (height/3, 2*width/3),
        //        (2*height/3, width/2)
        //    };


        //    foreach (var (y, x) in positions)
        //        MakeButton(y, x);


        //    buttons = this.Controls.OfType<Button>().ToList();



        //    for (int i = 0; i < buttons.Count; i++)
        //    {
        //        var start = buttons[i];
        //        var end = buttons[(i + 1) % buttons.Count];
        //        lineTypes.Add(LineType.None);
        //        drawPath(start, end, Color.Red);
        //    }

        //    using (Graphics g = Graphics.FromImage(canvas))
        //        g.DrawImage(offscreen, 0, 0);
        //    Invalidate();

        //}

        //private void MoveButton(Button b, Point snapPoint, Button? sender)
        //{
        //    Button prev = buttons[(buttons.IndexOf(b) - 1 + buttons.Count) % buttons.Count];
        //    Button next = buttons[(buttons.IndexOf(b) + 1) % buttons.Count];

        //    if (sender == null)
        //    {
        //        clearPath(prev, b);
        //        clearPath(b, next);

        //        if (lineTypes[buttons.IndexOf(b)] == LineType.Vertical)
        //        {
        //            MoveButton(next, new Point(snapPoint.X, next.Top + next.Height / 2), b);
        //        }
        //        else if (lineTypes[buttons.IndexOf(b)] == LineType.Diagonal || lineTypes[buttons.IndexOf(b)] == LineType.Const)
        //        {
        //            MoveButton(next, new Point(snapPoint.X + (next.Left + next.Width / 2 - b.Left - b.Width / 2), snapPoint.Y + (next.Top + next.Height / 2 - b.Top - b.Height / 2)), b);
        //        }
        //    }
        //}
    }

}
