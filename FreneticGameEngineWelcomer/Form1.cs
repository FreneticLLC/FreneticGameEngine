using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FreneticGameEngineWelcomer
{
    public partial class Form1 : Form
    {
        public enum MouseOver
        {
            NONE = 0,
            EXIT = 1
        }

        public MouseOver Hovered = MouseOver.NONE;

        public MouseOver Clicked = MouseOver.NONE;

        public Icon WelcomerIcon;

        public Icon WelcomerExitIcon;

        public Color WelcomerIconBackColor = Color.FromArgb(170, 190, 190);

        public Color WelcomerIconOutlineColor = Color.FromArgb(100, 230, 230);

        public Color WelcomerBackColor = Color.FromArgb(235, 235, 235);

        public Color WelcomerExitButtonOver = Color.FromArgb(255, 20, 20);

        public PictureBox PicBox;

        public Form1()
        {
            SuspendLayout();
            ComponentResourceManager resources = new ComponentResourceManager(typeof(Common));
            WelcomerIcon = resources.GetObject("FGE_Icon") as Icon;
            WelcomerExitIcon = resources.GetObject("Exit_Icon") as Icon;
            PicBox = new PictureBox()
            {
                Location = new Point(0, 0),
                Size = new Size(Width, Height)
            };
            PicBox.Paint += Form1_Paint;
            PicBox.MouseMove += Form1_MouseMove;
            PicBox.MouseClick += Form1_MouseClick;
            PicBox.MouseUp += Form1_MouseUp;
            Resize += Form1_Resize;
            Controls.Add(PicBox);
            ResumeLayout();
            InitializeComponent();
            PicBox.Size = new Size(Width, Height);
        }

        private void Form1_Resize(object sender, EventArgs e)
        {
            PicBox.Size = new Size(Width, Height);
        }

        private void Form1_MouseUp(object sender, MouseEventArgs e)
        {
            if (Clicked == MouseOver.EXIT && Hovered == MouseOver.EXIT)
            {
                Close();
            }
            Clicked = MouseOver.NONE;
            PicBox.Invalidate();
        }

        private void Form1_MouseClick(object sender, MouseEventArgs e)
        {
            if (Hovered == MouseOver.EXIT)
            {
                Clicked = MouseOver.EXIT;
            }
            else
            {
                Clicked = MouseOver.NONE;
            }
            PicBox.Invalidate();
        }

        private void Form1_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.X > Width - 34 && e.X < Width && e.Y > 0 && e.Y < 34)
            {
                Hovered = MouseOver.EXIT;
            }
            else
            {
                Hovered = MouseOver.NONE;
            }
            PicBox.Invalidate();
        }

        private void Form1_Paint(object sender, PaintEventArgs e)
        {
            e.Graphics.Clear(WelcomerBackColor);
            e.Graphics.FillRectangle(new SolidBrush(WelcomerIconBackColor), new Rectangle(1, 1, 32, 32));
            e.Graphics.DrawRectangle(new Pen(WelcomerIconOutlineColor), new Rectangle(0, 0, 33, 33));
            e.Graphics.DrawLine(new Pen(WelcomerIconOutlineColor), new Point(0, 33), new Point(e.ClipRectangle.Width, 33));
            e.Graphics.DrawIcon(WelcomerIcon, new Rectangle(1, 1, 32, 32));
            if (Hovered == MouseOver.EXIT)
            {
                e.Graphics.FillRectangle(new SolidBrush(WelcomerExitButtonOver), new Rectangle(e.ClipRectangle.Width - 33, 1, 32, 32));
            }
            e.Graphics.DrawIcon(WelcomerExitIcon, new Rectangle(e.ClipRectangle.Width - 33, 1, 32, 32));
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }
    }
}
