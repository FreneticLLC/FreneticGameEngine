//
// This file is part of the Frenetic Game Engine, created by Frenetic LLC.
// This code is Copyright (C) Frenetic LLC under the terms of a strict license.
// See README.md or LICENSE.txt in the FreneticGameEngine source root for the contents of the license.
// If neither of these are available, assume that neither you nor anyone other than the copyright holder
// hold any right or permission to use this software until such time as the official license is identified.
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;
using OpenTK.Mathematics;
using OpenTK.Input;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;
using FGECore.CoreSystems;
using FGECore.MathHelpers;
using FGECore.StackNoteSystem;
using FGECore.ModelSystems;
using FGEGraphics.GraphicsHelpers;
using FGEGraphics.GraphicsHelpers.Shaders;
using FGEGraphics.GraphicsHelpers.Textures;
using FGEGraphics.GraphicsHelpers.FontSets;
using FGEGraphics.GraphicsHelpers.Models;
using FGEGraphics.ClientSystem.EntitySystem;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.GraphicsLibraryFramework;
using ErrorCode = OpenTK.Graphics.OpenGL4.ErrorCode;

namespace FGEGraphics.ClientSystem
{
    /// <summary>
    /// Represents a game client window for a game.
    /// </summary>
    public class GameClientWindow : GameInstance<ClientEntity, GameEngineBase>, IDisposable
    {
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
                    OwningInstance = this
                };
            }
            else
            {
                CurrentEngine = new GameEngine2D()
                {
                    OwningInstance = this
                };
            }
        }

        /// <summary>
        /// The X-coordinate of the mouse in screen coordinates.
        /// </summary>
        public float MouseX;

        /// <summary>
        /// The Y-coordinate of the mouse in screen coordinates.
        /// </summary>
        public float MouseY;

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
        /// <param name="e">The event data.</param>
        private void Mouse_Move(MouseMoveEventArgs e)
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
                return Window == null ? WindWid : Window.Size.X;
            }
            set
            {
                WindWid = value;
                if (Window != null)
                {
                    Window.Size = new OpenTK.Mathematics.Vector2i(WindWid, WindHei);
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
                return Window == null ? WindHei : Window.Size.Y;
            }
            set
            {
                WindHei = value;
                if (Window != null)
                {
                    Window.Size = new OpenTK.Mathematics.Vector2i(WindWid, WindHei);
                }
            }
        }

        /// <summary>
        /// Size of the window.
        /// </summary>
        public FGECore.MathHelpers.Vector2i WindowSize
        {
            get
            {
                return Window == null ? new FGECore.MathHelpers.Vector2i(WindWid, WindHei) : new FGECore.MathHelpers.Vector2i(Window.Size.X, Window.Size.Y);
            }
            set
            {
                WindWid = value.X;
                WindHei = value.Y;
                if (Window != null)
                {
                    Window.Size = new OpenTK.Mathematics.Vector2i(value.X, value.Y);
                }
            }
        }

        private bool Loaded = false;

        /// <summary>
        /// Starts the game engine, and begins the primary loop.
        /// </summary>
        /// <param name="initialFlags">The initial window flag.</param>
        public void Start(WindowBorder initialFlags = WindowBorder.Fixed)
        {
            try
            {
                StackNoteHelper.Push("GameClientWindow - Start, run", this);
                SysConsole.Output(OutputType.INIT, "GameEngine loading...");
                Window = new GameWindow(new GameWindowSettings() { IsMultiThreaded = false }, new NativeWindowSettings()
                {
                    Size = new OpenTK.Mathematics.Vector2i(WindWid, WindHei),
                    Title = StartingWindowTitle,
                    Flags = ContextFlags.ForwardCompatible,
                    IsFullscreen = false,
                    WindowState = WindowState.Normal,
                    API = ContextAPI.OpenGL,
                    APIVersion = new Version(4, 3),
                    Profile = ContextProfile.Core,
                    StartFocused = true,
                    StartVisible = true,
                    WindowBorder = initialFlags
                });
                Window.Load += Window_Load;
                Window.RenderFrame += Window_RenderFrame;
                Window.MouseMove += Mouse_Move;
                Window.Closed += Window_Closed;
                Window.Resize += Window_Resize;
                SysConsole.Output(OutputType.INIT, "GameEngine calling SetUp event...");
                OnWindowSetUp?.Invoke();
                SysConsole.Output(OutputType.INIT, "GameEngine running...");
                Window.Run();
            }
            finally
            {
                StackNoteHelper.Pop();
            }
        }

        /// <summary>
        /// Fired when the window is resized.
        /// </summary>
        private void Window_Resize(ResizeEventArgs e)
        {
            if (Loaded)
            {
                CurrentEngine.ReloadScreenBuffers();
            }
        }

        /// <summary>
        /// Loads all content for the game, and starts the systems.
        /// </summary>
        private void Window_Load()
        {
            SysConsole.Output(OutputType.INIT, "GameClient starting load sequence...");
            GL.Viewport(0, 0, Window.Size.X, Window.Size.Y);
            GL.Enable(EnableCap.Texture2D);
            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
            GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Fill);
            GL.Disable(EnableCap.CullFace);
            GraphicsUtil.CheckError("GEB - Initial");
            InstanceInit();
            SysConsole.Output(OutputType.INIT, "GameClient loading shader helpers...");
            Shaders = new ShaderEngine();
            Shaders.InitShaderSystem(Files);
            SysConsole.Output(OutputType.INIT, "GameClient loading texture helpers...");
            Textures = new TextureEngine();
            Textures.InitTextureSystem(Files, AssetStreaming, Schedule);
            GraphicsUtil.CheckError("GEB - Textures");
            SysConsole.Output(OutputType.INIT, "GameClient loading font helpers...");
            GLFonts = new GLFontEngine(Textures, Shaders);
            GLFonts.Init(Files);
            FontSets = new FontSetEngine(GLFonts)
            {
                FixToShader = Shaders.ColorMult2DShader
            };
            // TODO: FGE/Core->Languages engine!
            FontSets.Init((subdata) => null, () => Ortho, () => GlobalTickTime);
            GraphicsUtil.CheckError("GEB - Fonts");
            SysConsole.Output(OutputType.INIT, "GameClient loading 2D/UI render helper...");
            MainUI = new ViewUI2D(this);
            SysConsole.Output(OutputType.INIT, "GameClient loading model engine...");
            Animations = new AnimationEngine();
            Models = new ModelEngine();
            Models.Init(Animations, this);
            SysConsole.Output(OutputType.INIT, "GameClient loading render helper...");
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
            Loaded = true;
        }

        /// <summary>
        /// The Ortho matrix, for Font rendering simplicity.
        /// </summary>
        public Matrix4 Ortho;

        /// <summary>
        /// Called when the window is closed.
        /// </summary>
        private void Window_Closed()
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
        public readonly float[] ScreenClearColor = new float[] { 0, 1, 1, 1 };

        private readonly float[] DepthClear = new float[] { 1 };

        /// <summary>
        /// Renders a single frame of the game, and also ticks.
        /// </summary>
        private void Window_RenderFrame(FrameEventArgs e)
        {
            try
            {
                StackNoteHelper.Push("GameClientWindow - Render and tick frame", this);
                // First step: check delta
                if (e.Time <= 0.0)
                {
                    return;
                }
                // Mouse handling
                PreviousMouse = CurrentMouse;
                CurrentMouse = Window.MouseState;
                // Standard pre-tick
                PreTick(e.Time);
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
                CurrentEngine.RenderSingleFrame();
                GraphicsUtil.CheckError("GameClient - PostMainEngine");
                // Add the UI Layer too
                MainUI.Draw();
                GraphicsUtil.CheckError("GameClient - PostUI");
                // Fourth step: clean up!
                GL.BindTexture(TextureTarget.Texture2D, 0);
                GL.BindVertexArray(0);
                GL.UseProgram(0);
                // Semi-final step: Tick logic!
                GraphicsUtil.CheckError("GameClient - PreTick");
                // Main instance tick.
                Tick();
                // Primary UI tick
                MainUI.Tick();
                GraphicsUtil.CheckError("GameClient - PostTick");
                // Final step: Swap the render buffer onto the screen!
                Window.SwapBuffers();
                GraphicsUtil.CheckError("GameClient - Post");
            }
            finally
            {
                StackNoteHelper.Pop();
            }
        }

        /// <summary>
        /// The shader system.
        /// </summary>
        public ShaderEngine Shaders;

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
        /// Dumb MS logic dispose method.
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
            GC.SuppressFinalize(this);
            Dispose(true);
        }

        /// <summary>
        /// Returns a string of this object.
        /// </summary>
        /// <returns>The string.</returns>
        public override string ToString()
        {
            return "GameClientWindow";
        }
    }
}
