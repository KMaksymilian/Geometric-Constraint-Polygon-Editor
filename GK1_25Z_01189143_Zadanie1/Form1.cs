using System.Drawing.Drawing2D;

namespace GK1_25Z_01189143_Zadanie1
{
    public partial class Form1 : Form
    {
        private List<Button> buttons = new List<Button>();
        private Bitmap canvas;
        public Form1()
        {
            InitializeComponent();
            canvas = new Bitmap(this.ClientSize.Width, this.ClientSize.Height);
            MakeSetup();

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

            this.Controls.Add(btn);
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

            int dx = x1 - x0;
            int dy = y1 - y0;

            int d = 2 * dy - dx;

            int incrE = 2 * dy; 
            int incrNE = 2 * (dy - dx); 
            int x = x0;
            int y = y0;

            if (x >= 0 && x < canvas.Width && y >= 0 && y < canvas.Height)
                canvas.SetPixel(x, y, color);

           

            while (x<x1)
            {

                if (d < 0) //choose E
                {
                    d += incrE;
                    x++;
                }
                else //choose NE
                {
                    d += incrNE;
                    x++;
                    y++;
                }
                if (x >= 0 && x < canvas.Width && y >= 0 && y < canvas.Height)
                    canvas.SetPixel(x, y, color);

              
            }


            this.Invalidate();
        }

    }

}
