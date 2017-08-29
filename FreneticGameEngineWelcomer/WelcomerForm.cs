//
// This file is created by Frenetic LLC.
// This code is Copyright (C) 2017 Frenetic LLC under the terms of a strict license.
// See README.md or LICENSE.txt in the source root for the contents of the license.
// If neither of these are available, assume that neither you nor anyone other than the copyright holder
// hold any right or permission to use this software until such time as the official license is identified.
//

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
    /// <summary>
    /// The main Welcomer form.
    /// </summary>
    public partial class WelcomerForm : Form
    {
        /// <summary>
        /// possible things for the mouse to be over on this form.
        /// </summary>
        public enum MouseOver
        {
            /// <summary>
            /// No item is under the mouse.
            /// </summary>
            NONE = 0,
            /// <summary>
            /// The "Exit" button is under the mouse.
            /// </summary>
            EXIT = 1,
            /// <summary>
            /// The general top bar is under the mouse.
            /// </summary>
            TOPBAR = 2
        }

        /// <summary>
        /// Enable double buffering.
        /// </summary>
        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams handleParam = base.CreateParams;
                handleParam.ExStyle |= 0x02000000; // WS_EX_COMPOSITED       
                return handleParam;
            }
        }

        /// <summary>
        /// What the mouse is currently over.
        /// </summary>
        public MouseOver Hovered = MouseOver.NONE;

        /// <summary>
        /// What the mouse has clicked (NONE if mouse button is not pressed down).
        /// </summary>
        public MouseOver Clicked = MouseOver.NONE;

        /// <summary>
        /// The main icon for the Welcomer form.
        /// </summary>
        public Icon WelcomerIcon;

        /// <summary>
        /// The "Exit" button icon.
        /// </summary>
        public Icon WelcomerExitIcon;

        /// <summary>
        /// The background color for <see cref="WelcomerIcon"/>.
        /// </summary>
        public Color WelcomerIconBackColor = Color.FromArgb(170, 190, 190);

        /// <summary>
        /// The outline color for <see cref="WelcomerIcon"/>.
        /// </summary>
        public Color WelcomerIconOutlineColor = Color.FromArgb(100, 230, 230);

        /// <summary>
        /// The main form background color.
        /// </summary>
        public Color WelcomerBackColor = Color.FromArgb(245, 255, 255);

        /// <summary>
        /// The color the <see cref="WelcomerExitIcon"/> shines behind when it is hovered over.
        /// </summary>
        public Color WelcomerExitButtonOver = Color.FromArgb(255, 20, 20);

        /// <summary>
        /// The picture box for rendering.
        /// </summary>
        public PictureBox PicBox;

        /// <summary>
        /// Whether the form is currently being dragged.
        /// </summary>
        public bool Dragging = false;

        /// <summary>
        /// The last position of the cursor when dragging.
        /// </summary>
        public Point DragLast;

        /// <summary>
        /// Timer for general ticking.
        /// </summary>
        public Timer TickTimer;

        /// <summary>
        /// The font for the title bar;
        /// </summary>
        public Font TitleBarFont;

        /// <summary>
        /// Initialize the form.
        /// </summary>
        public WelcomerForm()
        {
            SuspendLayout();
            ComponentResourceManager resources = new ComponentResourceManager(typeof(Common));
            WelcomerIcon = Common.FGE_Icon;
            WelcomerExitIcon = Common.Exit_Icon;
            TitleBarFont = new Font(FontFamily.GenericSansSerif, 16f); // TODO: Screen scaling oddities may occur here?
            PicBox = new PictureBox()
            {
                Location = new Point(0, 0),
                Size = new Size(Width, Height)
            };
            PicBox.Paint += Form1_Paint;
            PicBox.MouseMove += Form1_MouseMove;
            PicBox.MouseDown += Form1_MouseClick;
            PicBox.MouseUp += Form1_MouseUp;
            Resize += Form1_Resize;
            Controls.Add(PicBox);
            ResumeLayout();
            InitializeComponent();
            PicBox.Size = new Size(Width, Height);
            TickTimer = new Timer()
            {
                Interval = 50
            };
            TickTimer.Tick += TickTimer_Tick;
            TickTimer.Start();
        }

        /// <summary>
        /// Handles general ticking needs.
        /// </summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void TickTimer_Tick(object sender, EventArgs e)
        {
            if (Dragging)
            {
                Point pos = Cursor.Position;
                Point rel = new Point(pos.X - DragLast.X, pos.Y - DragLast.Y);
                DragLast = pos;
                Location = new Point(Location.X + rel.X, Location.Y + rel.Y);
            }
        }

        /// <summary>
        /// Handles form resizing.
        /// </summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void Form1_Resize(object sender, EventArgs e)
        {
            PicBox.Size = new Size(Width, Height);
        }

        /// <summary>
        /// Handles a mouse button being released.
        /// </summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void Form1_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left)
            {
                return;
            }
            if (Clicked == MouseOver.EXIT && Hovered == MouseOver.EXIT)
            {
                Close();
            }
            Clicked = MouseOver.NONE;
            Dragging = false;
            PicBox.Invalidate();
        }

        /// <summary>
        /// Handles a mouse button being pressed.
        /// </summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void Form1_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left)
            {
                return;
            }
            if (Hovered == MouseOver.EXIT)
            {
                Clicked = MouseOver.EXIT;
            }
            else if (Hovered == MouseOver.TOPBAR)
            {
                Clicked = MouseOver.TOPBAR;
                Dragging = true;
                DragLast = Cursor.Position;
            }
            else
            {
                Clicked = MouseOver.NONE;
            }
            PicBox.Invalidate();
        }

        /// <summary>
        /// Handles mouse movements over the form.
        /// </summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void Form1_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.X > Width - 34 && e.X < Width && e.Y > 0 && e.Y < 34)
            {
                Hovered = MouseOver.EXIT;
            }
            else if (e.Y < 34)
            {
                Hovered = MouseOver.TOPBAR;
            }
            else
            {
                Hovered = MouseOver.NONE;
            }
            PicBox.Invalidate();
        }

        /// <summary>
        /// Handles redrawing the form.
        /// </summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void Form1_Paint(object sender, PaintEventArgs e)
        {
            // Clear the screen
            e.Graphics.Clear(WelcomerBackColor);
            // Fill it to the correct color
            e.Graphics.FillRectangle(new SolidBrush(WelcomerIconBackColor), new Rectangle(1, 1, 32, 32));
            // Draw the outer edge of the screen
            e.Graphics.DrawRectangle(new Pen(WelcomerIconOutlineColor), new Rectangle(0, 0, Width - 1, Height - 1));
            // Drop the topbar text
            e.Graphics.DrawString("Welcome | Frenetic Game Engine", TitleBarFont, Brushes.Black, new PointF(40, 0));
            // Draw the welcomer icon, the icon's backdrop, and topbar underline
            e.Graphics.DrawRectangle(new Pen(WelcomerIconOutlineColor), new Rectangle(0, 0, 33, 33));
            e.Graphics.DrawLine(new Pen(WelcomerIconOutlineColor), new Point(0, 33), new Point(e.ClipRectangle.Width, 33));
            e.Graphics.DrawIcon(WelcomerIcon, new Rectangle(1, 1, 32, 32));
            // Drop the exit icon and backdrop
            if (Hovered == MouseOver.EXIT)
            {
                e.Graphics.FillRectangle(new SolidBrush(WelcomerExitButtonOver), new Rectangle(e.ClipRectangle.Width - 33, 1, 32, 32));
            }
            e.Graphics.DrawIcon(WelcomerExitIcon, new Rectangle(e.ClipRectangle.Width - 33, 1, 32, 32));
        }
    }
}
