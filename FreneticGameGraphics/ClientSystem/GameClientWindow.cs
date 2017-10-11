//
// This file is created by Frenetic LLC.
// This code is Copyright (C) 2017 Frenetic LLC under the terms of a strict license.
// See README.md or LICENSE.txt in the source root for the contents of the license.
// If neither of these are available, assume that neither you nor anyone other than the copyright holder
// hold any right or permission to use this software until such time as the official license is identified.
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;
using OpenTK.Input;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;
using FreneticGameCore;
using FreneticGameCore.Files;
using FreneticGameGraphics.GraphicsHelpers;
using FreneticGameCore.Collision;

namespace FreneticGameGraphics.ClientSystem
{
    /// <summary>
    /// Represents a game client window for a game.
    /// </summary>
    public class GameClientWindow : IDisposable
    {
        /// <summary>
        /// A scheduler for the general game client.
        /// </summary>
        public Scheduler Schedule = new Scheduler();

        /// <summary>
        /// The primary window for the game.
        /// </summary>
        public GameWindow Window;

        /// <summary>
        /// The current primary engine, dominating the view.
        /// </summary>
        public GameEngineBase CurrentEngine;

        /// <summary>
        /// Gets a 2D form of the current engine, if valid.
        /// </summary>
        public GameEngine2D Engine2D
        {
            get
            {
                return CurrentEngine as GameEngine2D;
            }
        }

        /// <summary>
        /// Gets a 3D form of the current engine, if valid.
        /// </summary>
        public GameEngine3D Engine3D
        {
            get
            {
                return CurrentEngine as GameEngine3D;
            }
        }

        /// <summary>
        /// The VR support system, if any.
        /// </summary>
        public VRSupport VR = null;

        /// <summary>
        /// The title of the window.
        /// </summary>
        public readonly string StartingWindowTitle;

        /// <summary>
        /// Constructs the game client window.
        /// </summary>
        /// <param name="_sWindowTitle">The starting window title.</param>
        /// <param name="threed">Whether the game is 3D.</param>
        public GameClientWindow(string _sWindowTitle = null, bool threed = true)
        {
            StartingWindowTitle = _sWindowTitle ?? Program.GameName + " v" + Program.GameVersion + " " + Program.GameVersionDescription;
            if (threed)
            {
                CurrentEngine = new GameEngine3D()
                {
                    Client = this
                };
            }
            else
            {
                CurrentEngine = new GameEngine2D()
                {
                    Client = this
                };
            }
        }

        /// <summary>
        /// Whether to process the default engine.
        /// </summary>
        public bool ProcessMainEngine = true;

        /// <summary>
        /// The X-coordinate of the mouse in screen coordinates.
        /// </summary>
        public int MouseX;

        /// <summary>
        /// The Y-coordinate of the mouse in screen coordinates.
        /// </summary>
        public int MouseY;

        /// <summary>
        /// Current frame delta (seconds).
        /// </summary>
        public double Delta;

        /// <summary>
        /// How long the game has run (seconds).
        /// </summary>
        public double GlobalTickTime = 1.0;

        /// <summary>
        /// The current mouse state for this tick.
        /// </summary>
        public MouseState CurrentMouse;

        /// <summary>
        /// The mouse state during the previous tick.
        /// </summary>
        public MouseState PreviousMouse;

        /// <summary>
        /// System to help with models.
        /// </summary>
        public ModelEngine Models;

        /// <summary>
        /// The render helper system, for 3D rendering.
        /// </summary>
        public Renderer Rendering3D;

        /// <summary>
        /// The 2D rendering helper, for any UI or general 2D logic.
        /// </summary>
        public Renderer2D Rendering2D;

        /// <summary>
        /// The 3D animation helper.
        /// </summary>
        public AnimationEngine Animations;

        /// <summary>
        /// Monitors on-window mouse movement.
        /// </summary>
        /// <param name="sender">The sender object.</param>
        /// <param name="e">The event data.</param>
        private void Mouse_Move(object sender, MouseMoveEventArgs e)
        {
            MouseX = e.X;
            MouseY = e.Y;
        }

        private int WindWid = 800;
        private int WindHei = 600;

        /// <summary>
        /// Window width.
        /// </summary>
        public int WindowWidth
        {
            get
            {
                return Window == null ? WindWid : Window.Width;
            }
            set
            {
                WindWid = value;
                if (Window != null)
                {
                    Window.Width = WindWid;
                }
            }
        }

        /// <summary>
        /// Window height.
        /// </summary>
        public int WindowHeight
        {
            get
            {
                return Window == null ? WindHei : Window.Height;
            }
            set
            {
                WindHei = value;
                if (Window != null)
                {
                    Window.Height = WindHei;
                }
            }
        }

        /// <summary>
        /// Size of the window.
        /// </summary>
        public Vector2i WindowSize
        {
            get
            {
                return Window == null ? new Vector2i(WindWid, WindHei) : new Vector2i(Window.Width, Window.Height);
            }
            set
            {
                WindWid = value.X;
                WindHei = value.Y;
                if (Window != null)
                {
                    Window.ClientSize = new System.Drawing.Size(value.X, value.Y);
                }
            }
        }

        /// <summary>
        /// Starts the game engine, and begins the primary loop.
        /// </summary>
        public void Start()
        {
            SysConsole.Output(OutputType.INIT, "GameEngine loading...");
            Window = new GameWindow(WindWid, WindHei, GraphicsMode.Default, StartingWindowTitle, GameWindowFlags.FixedWindow, DisplayDevice.Default, 4, 3, GraphicsContextFlags.ForwardCompatible);
            Window.Load += Window_Load;
            Window.RenderFrame += Window_RenderFrame;
            Window.Mouse.Move += Mouse_Move;
            Window.Closed += Window_Closed;
            SysConsole.Output(OutputType.INIT, "GameEngine calling SetUp event...");
            OnWindowSetUp?.Invoke();
            SysConsole.Output(OutputType.INIT, "GameEngine running...");
            Window.Run();
        }

        /// <summary>
        /// Loads all content for the game, and starts the systems.
        /// </summary>
        /// <param name="sender">Irrelevant.</param>
        /// <param name="e">Irrelevant.</param>
        private void Window_Load(object sender, EventArgs e)
        {
            SysConsole.Output(OutputType.INIT, "GameClient starting load sequence...");
            GL.Viewport(0, 0, Window.Width, Window.Height);
            GL.Enable(EnableCap.Texture2D);
            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);
            GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Fill);
            GL.Disable(EnableCap.CullFace);
            GraphicsUtil.CheckError("GEB - Initial");
            SysConsole.Output(OutputType.INIT, "GameClient loading file helpers...");
            Files = new FileHandler();
            Files.Init();
            SysConsole.Output(OutputType.INIT, "GameClient loading shader helpers...");
            Shaders = new ShaderEngine();
            Shaders.InitShaderSystem();
            SysConsole.Output(OutputType.INIT, "GameClient loading texture helpers...");
            Textures = new TextureEngine();
            Textures.InitTextureSystem(Files);
            GraphicsUtil.CheckError("GEB - Textures");
            SysConsole.Output(OutputType.INIT, "GameClient loading font helpers...");
            GLFonts = new GLFontEngine(Shaders);
            GLFonts.Init(Files);
            FontSets = new FontSetEngine(GLFonts)
            {
                FixTo = Shaders.ColorMult2DShader
            };
            // TODO: FGE/Core->Languages engine!
            FontSets.Init((subdata) => null, () => Ortho, () => GlobalTickTime);
            GraphicsUtil.CheckError("GEB - Fonts");
            SysConsole.Output(OutputType.INIT, "GameClient loading 2D/UI render helper...");
            MainUI = new ViewUI2D(this);
            SysConsole.Output(OutputType.INIT, "GameEngine loading model engine...");
            Animations = new AnimationEngine();
            Models = new ModelEngine();
            Models.Init(Animations, this);
            SysConsole.Output(OutputType.INIT, "GameEngine loading render helper...");
            Rendering3D = new Renderer(Textures, Shaders, Models);
            Rendering3D.Init();
            Rendering2D = new Renderer2D(Textures, Shaders);
            Rendering2D.Init();
            SysConsole.Output(OutputType.INIT, "GameClient calling engine load...");
            CurrentEngine.Load();
            SysConsole.Output(OutputType.INIT, "GameClient calling external load event...");
            OnWindowLoad?.Invoke();
            SysConsole.Output(OutputType.INIT, "GameClient is ready and loaded! Starting main game loop...");
            GraphicsUtil.CheckError("GEB - Loaded");
        }

        /// <summary>
        /// The Ortho matrix, for Font rendering simplicity.
        /// </summary>
        public Matrix4 Ortho;

        /// <summary>
        /// Called when the window is closed.
        /// </summary>
        /// <param name="sender">Irrelevant sender.</param>
        /// <param name="e">Empty event args.</param>
        private void Window_Closed(object sender, EventArgs e)
        {
            OnWindowClosed?.Invoke();
            if (ExitOnClose)
            {
                Environment.Exit(0);
            }
        }

        /// <summary>
        /// The color to blank the screen to every frame.
        /// Defaults to cyan (0:1:1:1).
        /// </summary>
        public float[] ScreenClearColor = new float[] { 0, 1, 1, 1 };

        private float[] DepthClear = new float[] { 1 };

        /// <summary>
        /// Renders a single frame of the game, and also ticks.
        /// </summary>
        /// <param name="sender">Irrelevant.</param>
        /// <param name="e">Holds the frame time (delta).</param>
        private void Window_RenderFrame(object sender, FrameEventArgs e)
        {
            // First step: check delta
            Delta = e.Time;
            CurrentEngine.Delta = Delta;
            if (e.Time <= 0.0)
            {
                return;
            }
            GlobalTickTime += Delta;
            ErrorCode ec = GL.GetError();
            while (ec != ErrorCode.NoError)
            {
                SysConsole.Output(OutputType.WARNING, "Uncaught GL Error: " + ec);
                ec = GL.GetError();
            }
            // Second step: clear the screen
            GL.ClearBuffer(ClearBuffer.Color, 0, ScreenClearColor);
            GL.ClearBuffer(ClearBuffer.Depth, 0, DepthClear);
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
            GL.DrawBuffer(DrawBufferMode.Back);
            GraphicsUtil.CheckError("GameClient - Pre");
            // Tick helpers
            Models.Update(GlobalTickTime);
            GraphicsUtil.CheckError("GameClient - PostModelUpdate");
            // Third step: general game rendering
            if (ProcessMainEngine)
            {
                CurrentEngine.RenderSingleFrame();
                GraphicsUtil.CheckError("GameClient - PostMainEngine");
            }
            // Add the UI Layer too
            MainUI.Draw();
            GraphicsUtil.CheckError("GameClient - PostUI");
            // Fourth step: clean up!
            GL.BindTexture(TextureTarget.Texture2D, 0);
            GL.BindVertexArray(0);
            GL.UseProgram(0);
            // Semi-final step: Tick logic!
            GraphicsUtil.CheckError("GameClient - PreTick");
            // Pre-tick.
            ClientEngineTick();
            // Primary entity tick
            if (ProcessMainEngine)
            {
                CurrentEngine.Tick();
            }
            // Primary UI tick
            MainUI.Tick();
            GraphicsUtil.CheckError("GameClient - PostTick");
            // Final step: Swap the render buffer onto the screen!
            Window.SwapBuffers();
            GraphicsUtil.CheckError("GameClient - Post");
        }

        /// <summary>
        /// Ticks the client engine.
        /// </summary>
        public void ClientEngineTick()
        {
            // Scheduler
            Schedule.RunAllSyncTasks(Delta);
            // Mouse handling
            PreviousMouse = CurrentMouse;
            CurrentMouse = Window.Mouse.GetState();
        }

        /// <summary>
        /// The shader system.
        /// </summary>
        public ShaderEngine Shaders;

        /// <summary>
        /// Helper for files.
        /// </summary>
        public FileHandler Files;

        /// <summary>
        /// Helper for internal GL font data in the system.
        /// </summary>
        public GLFontEngine GLFonts;

        /// <summary>
        /// Helper for all font sets in the system.
        /// </summary>
        public FontSetEngine FontSets;

        /// <summary>
        /// Helper for all textures in the system.
        /// </summary>
        public TextureEngine Textures;

        /// <summary>
        /// Fired when the window is set up.
        /// </summary>
        public Action OnWindowSetUp;

        /// <summary>
        /// Fired when the window is loading. Use this to load any data you need.
        /// </summary>
        public Action OnWindowLoad;

        /// <summary>
        /// Fired when the window is closed.
        /// </summary>
        public Action OnWindowClosed;

        /// <summary>
        /// Whether the program should shut down when the window is closed.
        /// </summary>
        public bool ExitOnClose = true;

        /// <summary>
        /// The currently rendering UI for this engine.
        /// </summary>
        public ViewUI2D MainUI;

        /// <summary>
        /// Dumb MS logic dispoe method.
        /// </summary>
        /// <param name="disposing">Whether to dispose managed resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                CurrentEngine.Dispose();
                Textures.Dispose();
                GLFonts.Dispose();
                Window.Dispose();
            }
        }
        
        /// <summary>
        /// Disposes the window client.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
        }
    }
}
