using System;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace GK1_25Z_01189143_Zadanie1
{
    public partial class Form1 : Form
    {
        private enum LineType
        {
            None,
            Vertical,
            Diagonal,
            Const
        }
        private List<LineType> lineTypes = new List<LineType>();
        private List<Button> buttons = new List<Button>();
        private List<(Point, Point)> edges = new List<(Point, Point)>();
        private Bitmap canvas;
        private Bitmap offscreen;
        private Dictionary<(int, int), int> pixelDictionary = new Dictionary<(int, int), int>();

        private Button? selectedButton = null;
        private bool isDragging = false;

        private bool isMovingPolygon = false;
        private Point lastMousePos;

        public Form1()
        {
            InitializeComponent();
            this.DoubleBuffered = true;
            this.SetStyle(ControlStyles.AllPaintingInWmPaint |
                          ControlStyles.UserPaint |
                          ControlStyles.OptimizedDoubleBuffer, true);
            this.UpdateStyles();

            canvas = new Bitmap(this.ClientSize.Width, this.ClientSize.Height);
            offscreen = new Bitmap(this.ClientSize.Width, this.ClientSize.Height);
            this.Resize += Form1_Resize;
            this.MouseDown += Form1_MouseDown;
            this.MouseMove += Form1_MouseMove;
            this.MouseUp += Form1_MouseUp;

            MakeSetup();

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

          
                using (Graphics g = Graphics.FromImage(offscreen))
                {
                    g.Clear(this.BackColor);
                }
                pixelDictionary.Clear();

                foreach (var btn in buttons)
                {
                    btn.Left += dx;
                    btn.Top += dy;
                }

                for (int i = 0; i < buttons.Count; i++)
                {
                    var a = buttons[i];
                    var b = buttons[(i + 1) % buttons.Count];
                    drawPath(a, b, Color.Red);
                }

                Invalidate();
            }
        }

        private void Form1_MouseDown(object? sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                // Szukamy najbliższej linii
                Point click = e.Location;
                (Button a, Button b)? nearest = FindNearestLine(click, 10);

                if (nearest != null)
                {
                    Point snapPoint = ProjectPointOntoLine(click, nearest.Value.a, nearest.Value.b);

                    // Można tu np. pokazać marker lub tymczasowo przesunąć punkt
                    // Otwórz menu kontekstowe w tym miejscu
                    ContextMenuStrip menu = new ContextMenuStrip();
                    menu.Items.Add("Dodaj punkt", null, (s, ev) => AddVertexBetween(nearest.Value.a, nearest.Value.b));

                    var advancedItem = new ToolStripMenuItem("Ograniczenia");

                  
                   
                    advancedItem.DropDownItems.Add("Pionowa", null, (s, ev) => SetConstraint(nearest.Value.a, nearest.Value.b, LineType.Vertical));
                    advancedItem.DropDownItems.Add("Skośna 45°", null, (s, ev) => SetConstraint(nearest.Value.a, nearest.Value.b, LineType.Diagonal));
                    advancedItem.DropDownItems.Add("Długość...", null, (s, ev) => SetConstraint(nearest.Value.a, nearest.Value.b, LineType.Const));

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

        private void SetConstraint(Button a, Button b, LineType linetype)
        {
           
          
            if(linetype == LineType.Const)
            {
                return;
            }
            else
            {
                Point snapPoint = new Point(0, 0);
                if (linetype == LineType.Vertical)
                {
                    snapPoint.X = a.Left + a.Width / 2;
                    snapPoint.Y = b.Top + b.Height / 2;
                }
                else if(linetype == LineType.Diagonal)
                {
                    int midX = (a.Left + a.Width / 2 + b.Left + b.Width / 2) / 2;
                    int midY = (a.Top + a.Height / 2 + b.Top + b.Height / 2) / 2;
                    int dx = (b.Left + b.Width / 2) - (a.Left + a.Width / 2);
                    int dy = (b.Top + b.Height / 2) - (a.Top + a.Height / 2);
                    if(Math.Abs(dx) > Math.Abs(dy))
                    {
                        if(dx > 0)  midY = a.Top + a.Height / 2 + Math.Abs(dx);
                        else        midY = a.Top + a.Height / 2 - Math.Abs(dx);
                    }
                    else
                    {
                        if(dy > 0)  midX = a.Left + a.Width / 2 + Math.Abs(dy);
                        else        midX = a.Left + a.Width / 2 - Math.Abs(dy);
                    }
                    snapPoint.X = midX;
                    snapPoint.Y = midY;
                }

                MoveButton(b, snapPoint, a);
            }
            lineTypes[buttons.IndexOf(a)] = linetype;
        }

        private void MoveButton(Button b, Point snapPoint, Button? sender)
        {
            Button prev = buttons[(buttons.IndexOf(b) - 1 + buttons.Count) % buttons.Count];
            Button next = buttons[(buttons.IndexOf(b) + 1) % buttons.Count];
    
            if(sender == null)
            {
                clearPath(prev, b);
                clearPath(b, next);

                if (lineTypes[buttons.IndexOf(b)] == LineType.Vertical)
                {
                    MoveButton(next, new Point(snapPoint.X, next.Top + next.Height/2), b);
                }
                else if (lineTypes[buttons.IndexOf(b)] == LineType.Diagonal || lineTypes[buttons.IndexOf(b)] == LineType.Const)
                {
                    MoveButton(next, new Point(snapPoint.X + (next.Left + next.Width/2 - b.Left - b.Width/2), snapPoint.Y + (next.Top + next.Height/2 - b.Top - b.Height/2)), b);
                }
            }
            

            
    
               
        }

        private void AddVertexBetween(Button a, Button b)
        {
            clearPath(a, b);

            int midX = (a.Left + a.Width / 2 + b.Left + b.Width / 2) / 2;
            int midY = (a.Top + a.Height / 2 + b.Top + b.Height / 2) / 2;

            // Stwórz nowy guzik ze środkiem w tym punkcie
            Button newBtn = MakeButton(midY, midX);

            this.Controls.Add(newBtn);
            buttons.Insert(buttons.IndexOf(b), newBtn);
            drawPath(a, newBtn, Color.Red);
            drawPath(newBtn, b, Color.Red);
            Invalidate();
        }

        private void Form1_Resize(object? sender, EventArgs e)
        {
            if (ClientSize.Width <= 0 || ClientSize.Height <= 0)
                return;

            Bitmap oldCanvas = canvas;
            offscreen.Dispose();

            canvas = new Bitmap(ClientSize.Width, ClientSize.Height);
            offscreen = new Bitmap(ClientSize.Width, ClientSize.Height);

            if (oldCanvas != null)
            {
                using (Graphics g = Graphics.FromImage(canvas))
                {
                    g.DrawImage(oldCanvas, 0, 0);
                }
                using (Graphics og = Graphics.FromImage(offscreen))
                {
                    og.DrawImage(oldCanvas, 0, 0);
                }
                oldCanvas.Dispose();
            }

            Invalidate();
        }


        private void MakeSetup()
        {
            int height = this.ClientSize.Height;
            int width = this.ClientSize.Width;

            var positions = new List<(int y, int x)>
            {
                (height/3, width/3),
                (height/3, 2*width/3),
                (2*height/3, width/2)
            };


            foreach (var (y, x) in positions)
                MakeButton(y, x);


            buttons = this.Controls.OfType<Button>().ToList();



            for (int i = 0; i < buttons.Count; i++)
            {
                var start = buttons[i];
                var end = buttons[(i + 1) % buttons.Count];
                drawPath(start, end, Color.Red);

                lineTypes.Add(LineType.None);
            }

            using (Graphics g = Graphics.FromImage(canvas))
                g.DrawImage(offscreen, 0, 0);
            Invalidate();

        }

        private Button MakeButton(int top, int left)
        {
            Button btn = new Button();
            GraphicsPath path = new GraphicsPath();
            btn.Size = new Size(15, 15);
            path.AddEllipse(0, 0, btn.Width, btn.Height);
            btn.Text = "";

            btn.Left = left - btn.Width / 2; // aby środek w punkcie (left, top)
            btn.Top = top - btn.Height / 2;

            btn.Anchor = AnchorStyles.Left | AnchorStyles.Top;
            btn.BackColor = Color.Black;
            btn.ForeColor = Color.Black;
            btn.FlatStyle = FlatStyle.Flat;
            btn.FlatAppearance.BorderSize = 0; // grubość obramowania
            btn.Region = new Region(path);

            btn.MouseDown += MouseDownBtn;
            btn.MouseMove += MouseMoveBtn;
            btn.MouseUp += MouseUpBtn;

            this.Controls.Add(btn);
            return btn;
        }

        private void MouseUpBtn(object? sender, MouseEventArgs e)
        {
            if (isDragging)
            {
                isDragging = false;
                selectedButton = null;
            }
        }

        private void MouseMoveBtn(object? sender, MouseEventArgs e)
        {
            if (isDragging && selectedButton != null)
            {


                int index = buttons.IndexOf(selectedButton);

                if (index == -1) throw new Exception("Invalid button");

                Button prev = buttons[(index - 1 + buttons.Count) % buttons.Count];
                Button next = buttons[(index + 1) % buttons.Count];

                clearPath(prev, selectedButton);
                clearPath(selectedButton, next);

                Point cursor = this.PointToClient(Cursor.Position);
                var point = new Point(
                    cursor.X - (selectedButton.Width / 2),
                    cursor.Y - (selectedButton.Height / 2));
                selectedButton.Location = point;



                drawPath(prev, selectedButton, Color.Red);
                drawPath(selectedButton, next, Color.Red);

                //using (Graphics g = Graphics.FromImage(canvas))
                //    g.DrawImage(offscreen, 0, 0);
                Invalidate();

            }
        }



        private void MouseDownBtn(object? sender, MouseEventArgs e)
        {
            if (sender is Button btn)
            {
                if (e.Button == MouseButtons.Left)
                {
                    selectedButton = btn;

                    isDragging = true;
                    btn.Capture = true;
                }
                else if (e.Button == MouseButtons.Right)
                {
                    Point snapPoint = new Point(btn.Left + btn.Width / 2, btn.Top + btn.Height / 2);
                    ContextMenuStrip menu = new ContextMenuStrip();
                    menu.Items.Add("Usuń wierzchoek", null, (s, ev) => RemoveVertex(btn));
                    menu.Show(this, snapPoint); 
                }
            }
            
        }

        private void RemoveVertex(Button btn)
        {
            clearPath(btn, buttons[(buttons.IndexOf(btn) + 1) % buttons.Count]);
            clearPath(buttons[(buttons.IndexOf(btn) - 1 + buttons.Count) % buttons.Count], btn);
            drawPath(buttons[(buttons.IndexOf(btn) - 1 + buttons.Count) % buttons.Count], buttons[(buttons.IndexOf(btn) + 1) % buttons.Count], Color.Red);
            this.Controls.Remove(btn);
            buttons.Remove(btn);
            btn.Dispose();
            Invalidate();
        }

        private (Button, Button)? FindNearestLine(Point p, int maxDistance)
        {
            (Button, Button)? nearest = null;
            double minDist = maxDistance;

            for (int i = 0; i < buttons.Count; i++)
            {
                Button a = buttons[i];
                Button b = buttons[(i + 1) % buttons.Count];

                Point p1 = new Point(a.Left + a.Width / 2, a.Top + a.Height / 2);
                Point p2 = new Point(b.Left + b.Width / 2, b.Top + b.Height / 2);

                double dist = DistancePointToSegment(p, p1, p2);
                if (dist <= minDist)
                {
                    minDist = dist;
                    nearest = (a, b);
                }
            }

            return nearest;
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

        private double DistancePointToSegment(Point p, Point a, Point b)
        {
            double dx = b.X - a.X;
            double dy = b.Y - a.Y;

            if (dx == 0 && dy == 0)
                return Math.Sqrt((p.X - a.X) * (p.X - a.X) + (p.Y - a.Y) * (p.Y - a.Y));

            double t = ((p.X - a.X) * dx + (p.Y - a.Y) * dy) / (dx * dx + dy * dy);
            t = Math.Max(0, Math.Min(1, t)); 

            double projX = a.X + t * dx;
            double projY = a.Y + t * dy;

            return Math.Sqrt((p.X - projX) * (p.X - projX) + (p.Y - projY) * (p.Y - projY));
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            using (Graphics g = Graphics.FromImage(canvas))
            {
                g.DrawImage(offscreen, 0, 0);
            }
            if (canvas != null)
                e.Graphics.DrawImage(canvas, 0, 0);
        }
        private void drawPath(Button a, Button b, Color color)
        {

            int x0 = a.Left + a.Width / 2;
            int y0 = a.Top + a.Height / 2;
            int x1 = b.Left + b.Width / 2;
            int y1 = b.Top + b.Height / 2;

            int dx = Math.Abs(x1 - x0);
            int dy = Math.Abs(y1 - y0);

            int sx = x0 < x1 ? 1 : -1;
            int sy = y0 < y1 ? 1 : -1;

            int x = x0;
            int y = y0;

            if (x >= 0 && x < offscreen.Width && y >= 0 && y < offscreen.Height)
                ModifyPixel(x, y, color, offscreen);

            int e;

            if (dx >= dy)
            {
                e = dx / 2;
                for (int i = 0; i < dx; i++)
                {
                    x += sx;
                    e -= dy;
                    if (e < 0)
                    {
                        y += sy;
                        e += dx;
                    }

                    if (x >= 0 && x < offscreen.Width && y >= 0 && y < offscreen.Height)
                        ModifyPixel(x, y, color, offscreen);

                }
            }
            else
            {
                e = dy / 2;
                for (int i = 0; i < dy; i++)
                {
                    y += sy;
                    e -= dx;
                    if (e < 0)
                    {
                        x += sx;
                        e += dy;
                    }

                    if (x >= 0 && x < offscreen.Width && y >= 0 && y < offscreen.Height)
                        ModifyPixel(x, y, color, offscreen);

                }
            }


        }

        private void clearPath(Button a, Button b)
        {
            drawPath(a, b, this.BackColor);
        }

        private void ModifyPixel(int x, int y, Color color, Bitmap offscreen)
        {
            if (color == this.BackColor && pixelDictionary.ContainsKey((x, y)))
            {
                pixelDictionary[(x, y)]--;
                if (pixelDictionary[(x, y)] == 0)
                {
                    pixelDictionary.Remove((x, y));
                    offscreen.SetPixel(x, y, this.BackColor);
                }
            }
            else
            {
                if (pixelDictionary.ContainsKey((x, y)))
                {
                    pixelDictionary[(x, y)]++;
                }
                else
                {
                    pixelDictionary.Add((x, y), 1);
                    offscreen.SetPixel(x, y, color);
                }
            }
        }

        private void restartToolStripMenuItem_Click(object sender, EventArgs e)
        {

            foreach (var btn in buttons)
            {
                this.Controls.Remove(btn);
                btn.Dispose();
            }
            buttons.Clear();
            pixelDictionary.Clear();
            using (Graphics g = Graphics.FromImage(offscreen))
            {
                g.Clear(this.BackColor);
            }

            using (Graphics g = Graphics.FromImage(canvas))
            {
                g.Clear(this.BackColor);
            }
            MakeSetup();
        }
    }

}
