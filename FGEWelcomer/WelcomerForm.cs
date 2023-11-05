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
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.Versioning;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FGEWelcomer;

/// <summary>
/// The main Welcomer form.
/// TODO: This system should have its own simple UI engine, as opposed to hardcoding everything.
/// </summary>
[SupportedOSPlatform("windows")]
public partial class WelcomerForm : Form
{
    /// <summary>possible things for the mouse to be over on this form.</summary>
    public enum MouseOver
    {
        /// <summary>No item is under the mouse.</summary>
        NONE = 0,
        /// <summary>The "Exit" button is under the mouse.</summary>
        EXIT = 1,
        /// <summary>The general top bar is under the mouse.</summary>
        TOPBAR = 2,
        /// <summary>The 'new project 3d static' button is under the mouse.</summary>
        NEW_BUTTON_3D_STATIC = 3,
        /// <summary>The 'new project (2D)' button is under the mouse.</summary>
        NEW_BUTTON_2D = 4,
        /// <summary>The 'new project 3d git' button is under the mouse.</summary>
        NEW_BUTTON_3D_GIT = 5
    }

    /// <summary>Enable double buffering.</summary>
    protected override CreateParams CreateParams
    {
        get
        {
            CreateParams handleParam = base.CreateParams;
            handleParam.ExStyle |= 0x02000000; // WS_EX_COMPOSITED
            return handleParam;
        }
    }

    /// <summary>What the mouse is currently over.</summary>
    public MouseOver Hovered = MouseOver.NONE;

    /// <summary>What the mouse has clicked (NONE if mouse button is not pressed down).</summary>
    public MouseOver Clicked = MouseOver.NONE;

    /// <summary>The main icon for the Welcomer form.</summary>
    public Icon WelcomerIcon;

    /// <summary>The "Exit" button icon.</summary>
    public Icon WelcomerExitIcon;

    /// <summary>The background color for <see cref="WelcomerIcon"/>.</summary>
    public Color WelcomerIconBackColor = Color.FromArgb(170, 190, 190);

    /// <summary>The outline color for <see cref="WelcomerIcon"/>.</summary>
    public Color WelcomerIconOutlineColor = Color.FromArgb(100, 230, 230);

    /// <summary>The main form background color.</summary>
    public Color WelcomerBackColor = Color.FromArgb(245, 255, 255);

    /// <summary>The color the <see cref="WelcomerExitIcon"/> shines behind when it is hovered over.</summary>
    public Color WelcomerExitButtonOver = Color.FromArgb(255, 20, 20);

    /// <summary>The color new button shines normally.</summary>
    public Color WelcomerNewButton = Color.FromArgb(100, 200, 200);

    /// <summary>The color new button is outlined in.</summary>
    public Color WelcomerNewButtonOutline = Color.FromArgb(100, 230, 230);

    /// <summary>The color new button shines behind when it is hovered over.</summary>
    public Color WelcomerNewButtonOver = Color.FromArgb(20, 200, 100);

    /// <summary>The picture box for rendering.</summary>
    public PictureBox PicBox;

    /// <summary>Whether the form is currently being dragged.</summary>
    public bool Dragging = false;

    /// <summary>The last position of the cursor when dragging.</summary>
    public Point DragLast;

    /// <summary>Timer for general ticking.</summary>
    public Timer TickTimer;

    /// <summary>The font for the title bar;</summary>
    public Font TitleBarFont;

    /// <summary>The font for the new button.</summary>
    public Font NewButtonFont;

    /// <summary>Initialize the form.</summary>
    public WelcomerForm()
    {
        SuspendLayout();
        ComponentResourceManager resources = new(typeof(Common));
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

    /// <summary>Handles mouse leaving the form.</summary>
    /// <param name="sender">Sender.</param>
    /// <param name="e">Event data</param>
    private void WelcomerForm_MouseLeave(object sender, EventArgs e)
    {
        Hovered = MouseOver.NONE;
        PicBox.Invalidate();
    }

    /// <summary>Handles general ticking needs.</summary>
    /// <param name="sender">The event sender.</param>
    /// <param name="e">The event arguments.</param>
    private void TickTimer_Tick(object sender, EventArgs e)
    {
        if (Dragging)
        {
            Point pos = Cursor.Position;
            Point rel = new(pos.X - DragLast.X, pos.Y - DragLast.Y);
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

    /// <summary>Handles form resizing.</summary>
    /// <param name="sender">The event sender.</param>
    /// <param name="e">The event arguments.</param>
    private void Form1_Resize(object sender, EventArgs e)
    {
        PicBox.Size = new Size(Width, Height);
    }

    /// <summary>Copies over a text file to a new project.</summary>
    /// <param name="f_in">Input file name.</param>
    /// <param name="f_out">Output file name.</param>
    /// <param name="projectData">Any custom settings.</param>
    public static void CopyOverText(string f_in, string f_out, List<KeyValuePair<string, string>> projectData)
    {
        string inp = File.ReadAllText("./generator/" + f_in + ".txt");
        for (int i = 0; i < projectData.Count; i++)
        {
            inp = inp.Replace("{{{" + projectData[i].Key + "}}}", projectData[i].Value);
        }
        Directory.GetParent(f_out).Create();
        File.WriteAllText(f_out, inp);
    }

    /// <summary>Runs a git command automatically.</summary>
    /// <param name="folder">The folder to run it in.</param>
    /// <param name="gitExe">The git executable.</param>
    /// <param name="args">The git command arguments.</param>
    public static void RunGitCommand(string folder, string gitExe, string args)
    {
        ProcessStartInfo psi = new()
        {
            FileName = gitExe,
            WorkingDirectory = folder,
            Arguments = args
        };
        Process.Start(psi).WaitForExit();
    }

    /// <summary>Creates a game project.</summary>
    /// <param name="threed">Whether it should be 3D.</param>
    /// <param name="submodule">Whether it should be backed by a submodule (if not, use static dll files).</param>
    public void CreateGame(bool threed, bool submodule)
    {
        FolderBrowserDialog fbd = new()
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
        string gitExe = "C:/Program Files/Git/cmd/git.exe";
        if (submodule && !File.Exists(gitExe))
        {
            using OpenFileDialog ofd = new()
            {
                InitialDirectory = "C:/",
                Filter = "Git Executable File (*.exe)|*.exe",
                FilterIndex = 1,
                RestoreDirectory = true,
                Multiselect = false,
                Title = "Please select your git executable"
            };
            DialogResult result = ofd.ShowDialog(this);
            if (result != DialogResult.OK)
            {
                MessageBox.Show(this, "Need a git executable for submodule backing.", "Generation failed.");
                return;
            }
            gitExe = ofd.FileName;
        }
        string pfname = folder.TrimEnd('/', '\\');
        int ind = pfname.LastIndexOfAny(new char[] { '/', '\\' });
        string proj_name = pfname[(ind + 1)..];
        string baseFolder = folder + "/" + proj_name + "/";
        List<KeyValuePair<string, string>> strs = new() { };
        strs.Add(new KeyValuePair<string, string>("name", proj_name));
        strs.Add(new KeyValuePair<string, string>("guid_project", Guid.NewGuid().ToString()));
        strs.Add(new KeyValuePair<string, string>("guid_sln", Guid.NewGuid().ToString()));
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
        if (submodule)
        {
            CopyOverText("project_cs_submodule", baseFolder + proj_name + ".csproj", strs);
            CopyOverText("project_solution_submodule", folder + "/" + proj_name + ".sln", strs);
            RunGitCommand(folder, gitExe, "init");
            RunGitCommand(folder, gitExe, "submodule add https://github.com/FreneticLLC/FreneticGameEngine");
            RunGitCommand(folder, gitExe, "submodule add https://github.com/FreneticLLC/FreneticUtilities");
            RunGitCommand(folder, gitExe, "submodule update --init --recursive");
        }
        else
        {
            CopyOverText("project_cs", baseFolder + proj_name + ".csproj", strs);
            CopyOverText("project_solution", folder + "/" + proj_name + ".sln", strs);
            foreach (string f in FILES)
            {
                File.Copy("./" + f, baseFolder + f);
            }
            CopyDirectoryAndChildren("shaders", baseFolder + "bin/Debug/shaders/");
            CopyDirectoryAndChildren("data", baseFolder + "bin/Debug/data/");
        }
        new Process
        {
            StartInfo = new ProcessStartInfo(folder + "/" + proj_name + ".sln")
            {
                UseShellExecute = true
            }
        }.Start();
        MessageBox.Show(this, "Created! Launching your editor... if it doesn't open, navigate to the folder and open the SLN file!", "Success");
    }

    /// <summary>Copies a directory and all child directories and files to a new location.</summary>
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
                string fn = f.TrimEnd('/', '\\')[(find + 1)..];
                CopyDirectoryAndChildren(dir + "/" + fn, newBase + fn + "/");
                continue;
            }
            File.Copy(f, newBase + f[(find + 1)..]);
        }
    }

    readonly string[] FILES = new string[] { "BepuPhysics.dll", "BepuPhysics.pdb", "BepuPhysics.xml",
        "BepuUtilities.dll", "BepuUtilities.pdb", "BepuUtilities.xml",
        "FreneticUtilities.dll", "FreneticUtilities.pdb", "FreneticUtilities.xml",
        "FGECore.dll", "FGECore.pdb", "FGECore.xml",
        "FGEGraphics.dll", "FGEGraphics.pdb", "FGEGraphics.xml",
        "LZ4.dll",
        "NVorbis.dll",
        "OpenTK.dll", "OpenTK.dll.config", "OpenTK.pdb", "OpenTK.xml",
        "openvr_api.dll", "openvr_api.pdb"
    };

    /// <summary>Handles a mouse button being released.</summary>
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
        else if (Clicked == MouseOver.NEW_BUTTON_3D_STATIC && Hovered == MouseOver.NEW_BUTTON_3D_STATIC)
        {
            CreateGame(true, false);
        }
        else if (Clicked == MouseOver.NEW_BUTTON_2D && Hovered == MouseOver.NEW_BUTTON_2D)
        {
            CreateGame(false, false);
        }
        else if (Clicked == MouseOver.NEW_BUTTON_3D_GIT && Hovered == MouseOver.NEW_BUTTON_3D_GIT)
        {
            CreateGame(true, true);
        }
        Clicked = MouseOver.NONE;
        Dragging = false;
        PicBox.Invalidate();
    }

    /// <summary>Handles a mouse button being pressed.</summary>
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
        else if (Hovered == MouseOver.NEW_BUTTON_3D_STATIC)
        {
            Clicked = MouseOver.NEW_BUTTON_3D_STATIC;
        }
        else if (Hovered == MouseOver.NEW_BUTTON_2D)
        {
            Clicked = MouseOver.NEW_BUTTON_2D;
        }
        else if (Hovered == MouseOver.NEW_BUTTON_3D_GIT)
        {
            Clicked = MouseOver.NEW_BUTTON_3D_GIT;
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

    /// <summary>Handles mouse movements over the form.</summary>
    /// <param name="sender">The event sender.</param>
    /// <param name="e">The event arguments.</param>
    private void Form1_MouseMove(object sender, MouseEventArgs e)
    {
        if (e.X > Width - 34 && e.X < Width && e.Y > 0 && e.Y < 34)
        {
            Hovered = MouseOver.EXIT;
        }
        else if (NEWBUTTON_RECTANGLE_3D_STATIC.Contains(e.X, e.Y))
        {
            Hovered = MouseOver.NEW_BUTTON_3D_STATIC;
        }
        else if (NEWBUTTON_RECTANGLE_2D.Contains(e.X, e.Y))
        {
            Hovered = MouseOver.NEW_BUTTON_2D;
        }
        else if (NEWBUTTON_RECTANGLE_3D_GIT.Contains(e.X, e.Y))
        {
            Hovered = MouseOver.NEW_BUTTON_3D_GIT;
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

    /// <summary>The 'welcome' message topbar text.</summary>
    public const string TOPBAR_TEXT = "Welcome | Frenetic Game Engine";

    /// <summary>The 'create new game project (3D - Static)' message text.</summary>
    public const string NEWBUTTON_TEXT_3D_STATIC = "Create New Game Project (3D - Static Backed)";

    /// <summary>The 'create new game project (3D - Static)' message rectangle.</summary>
    public static readonly Rectangle NEWBUTTON_RECTANGLE_3D_STATIC = new(2, 38, 400, 25);

    /// <summary>The 'create new game project (2D)' message text.</summary>
    public const string NEWBUTTON_TEXT_2D = "Create New Game Project (2D)";

    /// <summary>The 'create new game project (2D)' message rectangle.</summary>
    public static readonly Rectangle NEWBUTTON_RECTANGLE_2D = new(2 + 400 + 5, 38, 234, 25);

    /// <summary>The 'create new game project (3D - Submodule Backed)' message text.</summary>
    public const string NEWBUTTON_TEXT_3D_GIT = "Create New Game Project (3D - Submodule Backed)";

    /// <summary>The 'create new game project (3D - Submodule Backed)' message rectangle.</summary>
    public static readonly Rectangle NEWBUTTON_RECTANGLE_3D_GIT = new(2, 38 + 25 + 5, 400, 25);

    /// <summary>Handles redrawing the form.</summary>
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
        // Draw the new button 3D Static
        if (Hovered == MouseOver.NEW_BUTTON_3D_STATIC)
        {
            e.Graphics.FillRectangle(new SolidBrush(WelcomerNewButtonOver), NEWBUTTON_RECTANGLE_3D_STATIC);
        }
        else
        {
            e.Graphics.FillRectangle(new SolidBrush(WelcomerNewButton), NEWBUTTON_RECTANGLE_3D_STATIC);
        }
        e.Graphics.DrawRectangle(new Pen(WelcomerNewButtonOutline), NEWBUTTON_RECTANGLE_3D_STATIC);
        e.Graphics.DrawString(NEWBUTTON_TEXT_3D_STATIC, NewButtonFont, Brushes.Black, new PointF(5, 40));
        // Draw the new button 2D
        if (Hovered == MouseOver.NEW_BUTTON_2D)
        {
            e.Graphics.FillRectangle(new SolidBrush(WelcomerNewButtonOver), NEWBUTTON_RECTANGLE_2D);
        }
        else
        {
            e.Graphics.FillRectangle(new SolidBrush(WelcomerNewButton), NEWBUTTON_RECTANGLE_2D);
        }
        e.Graphics.DrawRectangle(new Pen(WelcomerNewButtonOutline), NEWBUTTON_RECTANGLE_2D);
        e.Graphics.DrawString(NEWBUTTON_TEXT_2D, NewButtonFont, Brushes.Black, new PointF(5 + 5 + 400, 40));
        // Draw the new button 3D Git
        if (Hovered == MouseOver.NEW_BUTTON_3D_GIT)
        {
            e.Graphics.FillRectangle(new SolidBrush(WelcomerNewButtonOver), NEWBUTTON_RECTANGLE_3D_GIT);
        }
        else
        {
            e.Graphics.FillRectangle(new SolidBrush(WelcomerNewButton), NEWBUTTON_RECTANGLE_3D_GIT);
        }
        e.Graphics.DrawRectangle(new Pen(WelcomerNewButtonOutline), NEWBUTTON_RECTANGLE_3D_GIT);
        e.Graphics.DrawString(NEWBUTTON_TEXT_3D_GIT, NewButtonFont, Brushes.Black, new PointF(5, 40 + 25 + 5));
        // Drop the exit icon and backdrop
        if (Hovered == MouseOver.EXIT)
        {
            e.Graphics.FillRectangle(new SolidBrush(WelcomerExitButtonOver), new Rectangle(e.ClipRectangle.Width - 33, 1, 32, 32));
        }
        e.Graphics.DrawIcon(WelcomerExitIcon, new Rectangle(e.ClipRectangle.Width - 33, 1, 32, 32));
    }
}
