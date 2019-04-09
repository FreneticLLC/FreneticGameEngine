//
// This file is part of the Frenetic Game Engine, created by Frenetic LLC.
// This code is Copyright (C) Frenetic LLC under the terms of a strict license.
// See README.md or LICENSE.txt in the FreneticGameEngine source root for the contents of the license.
// If neither of these are available, assume that neither you nor anyone other than the copyright holder
// hold any right or permission to use this software until such time as the official license is identified.
//

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;

namespace FGEWelcomer
{
    /// <summary>
    /// The main Welcomer form.
    /// TODO: This system should have its own simple UI engine, as opposed to hardcoding everything.
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
            TOPBAR = 2,
            /// <summary>
            /// The 'new project' button is under the mouse.
            /// </summary>
            NEW_BUTTON = 3,
            /// <summary>
            /// The 'new project (2D)' button is under the mouse.
            /// </summary>
            NEW_BUTTON_2D = 4
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
        /// The color new button shines normally.
        /// </summary>
        public Color WelcomerNewButton = Color.FromArgb(100, 200, 200);

        /// <summary>
        /// The color new button is outlined in.
        /// </summary>
        public Color WelcomerNewButtonOutline = Color.FromArgb(100, 230, 230);

        /// <summary>
        /// The color new button shines behind when it is hovered over.
        /// </summary>
        public Color WelcomerNewButtonOver = Color.FromArgb(20, 200, 100);

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
        /// The font for the new button.
        /// </summary>
        public Font NewButtonFont;

        /// <summary>
        /// Initialize the form.
        /// </summary>
        public WelcomerForm()
        {
            SuspendLayout();
            ComponentResourceManager resources = new ComponentResourceManager(typeof(Common));
            WelcomerIcon = Common.FGE_Icon;
            WelcomerExitIcon = Common.Exit_Icon;
            // TODO: Screen scaling oddities may occur here? (Fonts)
            TitleBarFont = new Font(FontFamily.GenericSansSerif, 16f);
            NewButtonFont = new Font(FontFamily.GenericSansSerif, 12f);
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
            MouseLeave += WelcomerForm_MouseLeave;
            Controls.Add(PicBox);
            ResumeLayout();
            InitializeComponent();
            PicBox.Size = new Size(Width, Height);
            TickTimer = new Timer()
            {
                Interval = 25
            };
            TickTimer.Tick += TickTimer_Tick;
            TickTimer.Start();
        }

        /// <summary>
        /// Handles mouse leaving the form.
        /// </summary>
        /// <param name="sender">Sender.</param>
        /// <param name="e">Event data</param>
        private void WelcomerForm_MouseLeave(object sender, EventArgs e)
        {
            Hovered = MouseOver.NONE;
            PicBox.Invalidate();
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
            else if (Hovered != MouseOver.NONE)
            {
                Point pos = Cursor.Position;
                if (pos.X < Location.X || pos.Y < Location.Y || pos.X > Location.X + Size.Width || pos.Y > Location.Y + Size.Height)
                {
                    Hovered = MouseOver.NONE;
                    Clicked = MouseOver.NONE;
                    PicBox.Invalidate();
                }
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
        /// Copies over a text file to a new project.
        /// </summary>
        /// <param name="f_in">Input file name.</param>
        /// <param name="f_out">Output file name.</param>
        /// <param name="projectData">Any custom settings.</param>
        public void CopyOverText(string f_in, string f_out, List<KeyValuePair<string, string>> projectData)
        {
            string inp = File.ReadAllText("./generator/" + f_in + ".txt");
            for (int i = 0; i < projectData.Count; i++)
            {
                inp = inp.Replace("{{{" + projectData[i].Key + "}}}", projectData[i].Value);
            }
            Directory.GetParent(f_out).Create();
            File.WriteAllText(f_out, inp);
        }
        
        /// <summary>
        /// Creates a game project.
        /// </summary>
        /// <param name="threed">Whether it should be 3D.</param>
        public void CreateGame(bool threed)
        {
            FolderBrowserDialog fbd = new FolderBrowserDialog()
            {
                ShowNewFolderButton = true
            };
            DialogResult dr = fbd.ShowDialog(this);
            if (dr != DialogResult.OK && dr != DialogResult.Yes)
            {
                return;
            }
            string folder = fbd.SelectedPath;
            if (!Directory.Exists(folder))
            {
                MessageBox.Show(this, "Invalid directory (does not exist).", "Error");
                return;
            }
            if (Directory.GetFiles(folder).Length > 0)
            {
                MessageBox.Show(this, "Invalid directory (not empty).", "Error");
                return;
            }
            string pfname = folder.TrimEnd('/', '\\');
            int ind = pfname.LastIndexOfAny(new char[] { '/', '\\' });
            string proj_name = pfname.Substring(ind + 1);
            string baseFolder = folder + "/" + proj_name + "/";
            List<KeyValuePair<string, string>> strs = new List<KeyValuePair<string, string>>() { };
            strs.Add(new KeyValuePair<string, string>("name", proj_name));
            strs.Add(new KeyValuePair<string, string>("guid_project", Guid.NewGuid().ToString()));
            strs.Add(new KeyValuePair<string, string>("guid_sln", Guid.NewGuid().ToString()));
            CopyOverText("project_solution", folder + "/" + proj_name + ".sln", strs);
            CopyOverText("app_conf", baseFolder + "App.config", strs);
            CopyOverText("project_cs", baseFolder + proj_name + ".csproj", strs);
            CopyOverText("gprogram_cs", baseFolder + "GameProgram.cs", strs);
            CopyOverText("gitignore", folder + "/.gitignore", strs);
            CopyOverText("fge_legal", folder + "/FGE-LEGAL.md", strs);
            CopyOverText("gprogram_cs", baseFolder + "GameProgram.cs", strs);
            CopyOverText("asminf_cs", baseFolder + "Properties/AssemblyInfo.cs", strs);
            if (threed)
            {
                CopyOverText("game_cs", baseFolder + "MainGame/Game.cs", strs);
            }
            else
            {
                CopyOverText("game_cs_2d", baseFolder + "MainGame/Game.cs", strs);
            }
            foreach (string f in FILES)
            {
                File.Copy("./" + f, baseFolder + f);
            }
            CopyDirectoryAndChildren("shaders", baseFolder + "bin/Debug/shaders/");
            CopyDirectoryAndChildren("data", baseFolder + "bin/Debug/data/");
            Process.Start(folder + "/" + proj_name + ".sln");
            MessageBox.Show(this, "Created! Launching your editor... if it doesn't open, navigate to the folder and open the SLN file!", "Success");
        }

        /// <summary>
        /// Copies a directory and all child directories and files to a new location.
        /// </summary>
        /// <param name="dir">The directory.</param>
        /// <param name="newBase">The new base folder.</param>
        public void CopyDirectoryAndChildren(string dir, string newBase)
        {
            Directory.CreateDirectory(newBase);
            foreach (string f in Directory.GetFileSystemEntries("./" + dir + "/"))
            {
                int find = f.LastIndexOfAny(new char[] { '/', '\\' });
                if (f.EndsWith("/") || File.GetAttributes(f).HasFlag(FileAttributes.Directory))
                {
                    string fn = f.TrimEnd('/', '\\').Substring(find + 1);
                    CopyDirectoryAndChildren(dir + "/" + fn, newBase + fn + "/");
                    continue;
                }
                File.Copy(f, newBase + f.Substring(find + 1));
            }
        }

        string[] FILES = new string[] { "BEPUphysics.dll", "BEPUphysics.pdb", "BEPUphysics.xml",
            "BEPUutilities.dll", "BEPUutilities.pdb", "BEPUutilities.xml",
            "csogg.dll", "csvorbis.dll",
            "FreneticUtilities.dll", "FreneticUtilities.pdb", "FreneticUtilities.xml",
            "FGECore.dll", "FGECore.pdb", "FGECore.xml",
            "FGEGraphics.dll", "FGEGraphics.pdb", "FGEGraphics.xml",
            //"FreneticScript.dll", "FreneticScript.pdb", "FreneticScript.xml",
            "LZ4.dll",
            "OpenTK.dll", "OpenTK.dll.config", "OpenTK.pdb", "OpenTK.xml",
            "openvr_api.dll", "openvr_api.pdb"
        };

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
            else if (Clicked == MouseOver.NEW_BUTTON && Hovered == MouseOver.NEW_BUTTON)
            {
                CreateGame(true);
            }
            else if (Clicked == MouseOver.NEW_BUTTON_2D && Hovered == MouseOver.NEW_BUTTON_2D)
            {
                CreateGame(false);
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
            else if (Hovered == MouseOver.NEW_BUTTON)
            {
                Clicked = MouseOver.NEW_BUTTON;
            }
            else if (Hovered == MouseOver.NEW_BUTTON_2D)
            {
                Clicked = MouseOver.NEW_BUTTON_2D;
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
            else if (e.X >= 2 && e.X <= 234 + 2 && e.Y >= 38 && e.Y <= 38 + 25)
            {
                Hovered = MouseOver.NEW_BUTTON;
            }
            else if (e.X >= 2 + 234 + 5 && e.X <= 234 + 5 + 234 + 2 && e.Y >= 38 && e.Y <= 38 + 25)
            {
                Hovered = MouseOver.NEW_BUTTON_2D;
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
        /// The 'welcome' message topbar text.
        /// </summary>
        public const string TOPBAR_TEXT = "Welcome | Frenetic Game Engine";

        /// <summary>
        /// The 'create new game project (3D)' message text.
        /// </summary>
        public const string NEWBUTTON_TEXT = "Create New Game Project (3D)";

        /// <summary>
        /// The 'create new game project (2D)' message text.
        /// </summary>
        public const string NEWBUTTON_TEXT_2D = "Create New Game Project (2D)";

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
            e.Graphics.DrawString(TOPBAR_TEXT, TitleBarFont, Brushes.Black, new PointF(40, 0));
            // Draw the welcomer icon, the icon's backdrop, and topbar underline
            e.Graphics.DrawRectangle(new Pen(WelcomerIconOutlineColor), new Rectangle(0, 0, 33, 33));
            e.Graphics.DrawLine(new Pen(WelcomerIconOutlineColor), new Point(0, 33), new Point(e.ClipRectangle.Width, 33));
            e.Graphics.DrawIcon(WelcomerIcon, new Rectangle(1, 1, 32, 32));
            // Draw the new button
            if (Hovered == MouseOver.NEW_BUTTON)
            {
                e.Graphics.FillRectangle(new SolidBrush(WelcomerNewButtonOver), new RectangleF(2, 38, 234, 25));
            }
            else
            {
                e.Graphics.FillRectangle(new SolidBrush(WelcomerNewButton), new RectangleF(2, 38, 234, 25));
            }
            e.Graphics.DrawRectangle(new Pen(WelcomerNewButtonOutline), new Rectangle(2, 38, 234, 25));
            e.Graphics.DrawString(NEWBUTTON_TEXT, NewButtonFont, Brushes.Black, new PointF(5, 40));
            // Draw the new button 2D
            if (Hovered == MouseOver.NEW_BUTTON_2D)
            {
                e.Graphics.FillRectangle(new SolidBrush(WelcomerNewButtonOver), new RectangleF(2 + 234 + 5, 38, 234, 25));
            }
            else
            {
                e.Graphics.FillRectangle(new SolidBrush(WelcomerNewButton), new RectangleF(2 + 234 + 5, 38, 234, 25));
            }
            e.Graphics.DrawRectangle(new Pen(WelcomerNewButtonOutline), new Rectangle(2 + 234 + 5, 38, 234, 25));
            e.Graphics.DrawString(NEWBUTTON_TEXT_2D, NewButtonFont, Brushes.Black, new PointF(5 + 5 + 234, 40));
            // Drop the exit icon and backdrop
            if (Hovered == MouseOver.EXIT)
            {
                e.Graphics.FillRectangle(new SolidBrush(WelcomerExitButtonOver), new Rectangle(e.ClipRectangle.Width - 33, 1, 32, 32));
            }
            e.Graphics.DrawIcon(WelcomerExitIcon, new Rectangle(e.ClipRectangle.Width - 33, 1, 32, 32));
        }
    }
}
