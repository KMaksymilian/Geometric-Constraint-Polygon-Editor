using System;
using System.Collections.Generic;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GK1_25Z_01189143_Zadanie1
{
    internal class VertexButton : Button
    {
        public VertexButton(int x, int y)
        {
            Size = new Size(15, 15);
            Left = x - Width / 2;
            Top = y - Height / 2;
            BackColor = Color.Black;
            FlatStyle = FlatStyle.Flat;
            FlatAppearance.BorderSize = 0;
            Text = "";

            GraphicsPath path = new GraphicsPath();
            path.AddEllipse(0, 0, Width, Height);
            Region = new Region(path);
            this.MouseMove += MouseMoveBtn;
            this.MouseDown += MouseDownBtn;
            this.MouseUp += MouseUpBtn;
        }

        public Point Center => new Point(Left + Width / 2, Top + Height / 2);


        private void MouseUpBtn(object? sender, MouseEventArgs e)
        {
            if(Parent is Form1 form)
            {
                form.MouseUpBtn(sender, e);
            }
        }

        private void MouseMoveBtn(object? sender, MouseEventArgs e)
        {
            if (Parent is Form1 form)
            {
                form.MouseMoveBtn(sender, e);
            }
        }



        private void MouseDownBtn(object? sender, MouseEventArgs e)
        {
            if (Parent is Form1 form)
            {
                form.MouseDownBtn(sender, e);
            }

        }
    }
}
