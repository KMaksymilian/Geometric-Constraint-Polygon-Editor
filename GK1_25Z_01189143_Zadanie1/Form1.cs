using System;
using System.Drawing.Drawing2D;

namespace GK1_25Z_01189143_Zadanie1
{
    public partial class Form1 : Form
    {
        private List<Button> buttons = new List<Button>();
        private Bitmap canvas;
        private Bitmap offscreen;

        private Button? selectedButton = null;
        private bool isDragging = false;

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
            MakeSetup();
           
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
                using( Graphics og = Graphics.FromImage(offscreen))
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
            }

            using (Graphics g = Graphics.FromImage(canvas))
                g.DrawImage(offscreen, 0, 0);
            Invalidate();

        }

        private void MakeButton(int top, int left)
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
            if(isDragging && selectedButton != null)
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

                using (Graphics g = Graphics.FromImage(canvas))
                    g.DrawImage(offscreen, 0, 0);
                Invalidate();

            }
        }

        private void MouseDownBtn(object? sender, MouseEventArgs e)
        {
            if(sender is Button btn && e.Button == MouseButtons.Left)
            {
                selectedButton = btn;
              
                isDragging = true;
                btn.Capture = true;
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
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
                offscreen.SetPixel(x, y, color);

            int e;

            if (dx >= dy)
            {
                e = dx / 2;
                for (int i = 0; i < dx; i++)
                {
                    x += sx;
                    e -= dy;
                    if(e < 0)
                    {
                        y += sy;
                        e += dx;
                    }

                    if (x >= 0 && x < offscreen.Width && y >= 0 && y < offscreen.Height)
                        offscreen.SetPixel(x, y, color);

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
                        offscreen.SetPixel(x, y, color);

                }
            }

            
        }

        private void clearPath(Button a, Button b)
        {
            drawPath(a, b, this.BackColor);
        }

    }

}
