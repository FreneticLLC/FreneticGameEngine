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
using FreneticGameCore.EntitySystem;
using FreneticGameGraphics.ClientSystem.EntitySystem;

namespace FreneticGameGraphics.ClientSystem
{
    /// <summary>
    /// Represents the common functionality of a client Game Engine.
    /// </summary>
    public abstract class GameEngineBase : BasicEngine
    {
        /// <summary>
        /// The primary window for the game.
        /// </summary>
        public GameWindow Window;

        /// <summary>
        /// The title of the window.
        /// </summary>
        public readonly string StartingWindowTitle;

        /// <summary>
        /// Constructs the base game engine.
        /// </summary>
        /// <param name="_sWindowTitle">The starting window title.</param>
        public GameEngineBase(string _sWindowTitle)
        {
            StartingWindowTitle = _sWindowTitle ?? Program.GameName + " v" + Program.GameVersion + " " + Program.GameVersionDescription;
        }

        /// <summary>
        /// Creates an entity.
        /// </summary>
        /// <param name="ticks">Whether it should tick.</param>
        /// <returns>The entity.</returns>
        public override BasicEntity CreateEntity(bool ticks)
        {
            return new ClientEntity(this, ticks);
        }

        /// <summary>
        /// Starts the game engine, and begins the primary loop.
        /// </summary>
        public void Start()
        {
            SysConsole.Output(OutputType.INIT, "GameEngine loading...");
            Window = new GameWindow(800, 600, GraphicsMode.Default, StartingWindowTitle, GameWindowFlags.FixedWindow, DisplayDevice.Default, 4, 3, GraphicsContextFlags.ForwardCompatible);
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
        /// The color to blank the screen to every frame.
        /// Defaults to cyan (0:1:1:1).
        /// </summary>
        public float[] ScreenClearColor = new float[] { 0, 1, 1, 1 };

        /// <summary>
        /// Renders a single frame of the game, and also ticks.
        /// </summary>
        /// <param name="sender">Irrelevant.</param>
        /// <param name="e">Holds the frame time (delta).</param>
        private void Window_RenderFrame(object sender, FrameEventArgs e)
        {
            // First step: check delta
            Delta = e.Time;
            if (e.Time <= 0.0)
            {
                return;
            }
            // Second step: clear the screen
            GL.ClearBuffer(ClearBuffer.Color, 0, ScreenClearColor);
            // Third step: general rendering
            RenderSingleFrame();
            // Fourth step: clean up!
            GL.BindTexture(TextureTarget.Texture2D, 0);
            GL.BindVertexArray(0);
            GL.UseProgram(0);
            // Semi-final step: Tick logic!
            Tick();
            // Final step: Swap the render buffer onto the screen!
            Window.SwapBuffers();
        }

        /// <summary>
        /// Run through a full single-frame render sequence.
        /// </summary>
        public abstract void RenderSingleFrame();

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
        /// The X-coordinate of the mouse in screen coordinates.
        /// </summary>
        public int MouseX;

        /// <summary>
        /// The Y-coordinate of the mouse in screen coordinates.
        /// </summary>
        public int MouseY;

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

        /// <summary>
        /// Loads all content for the game, and starts the systems.
        /// </summary>
        /// <param name="sender">Irrelevant.</param>
        /// <param name="e">Irrelevant.</param>
        private void Window_Load(object sender, EventArgs e)
        {
            SysConsole.Output(OutputType.INIT, "GameEngine starting load sequence...");
            GL.Viewport(0, 0, Window.Width, Window.Height);
            GL.Enable(EnableCap.Texture2D);
            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);
            GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Fill);
            GL.Disable(EnableCap.CullFace);
            SysConsole.Output(OutputType.INIT, "GameEngine loading file helpers...");
            Files = new FileHandler();
            Files.Init();
            SysConsole.Output(OutputType.INIT, "GameEngine loading shader helpers...");
            Shaders = new ShaderEngine();
            Shaders.InitShaderSystem();
            GetShaders();
            SysConsole.Output(OutputType.INIT, "GameEngine loading texture helpers...");
            Textures = new TextureEngine();
            Textures.InitTextureSystem(Files);
            SysConsole.Output(OutputType.INIT, "GameEngine loading font helpers...");
            GLFonts = new GLFontEngine(Shaders);
            GLFonts.Init(Files);
            FontSets = new FontSetEngine(GLFonts);
            // TODO: FGE/Core->Languages engine!
            FontSets.Init((subdata) => null, () => Ortho, () => GlobalTickTime);
            SysConsole.Output(OutputType.INIT, "GameEngine prepping physics helper...");
            PhysicsWorld = new PhysicsSpace();
            SysConsole.Output(OutputType.INIT, "GameEngine core load complete, calling additional load...");
            PostLoad();
            SysConsole.Output(OutputType.INIT, "GameEngine calling external load event...");
            OnWindowLoad?.Invoke();
            SysConsole.Output(OutputType.INIT, "GameEngine is ready and loaded! Starting main game loop...");
        }

        /// <summary>
        /// Get any relevant shaders.
        /// </summary>
        public abstract void GetShaders();

        /// <summary>
        /// Any post-load actions.
        /// </summary>
        public abstract void PostLoad();

        /// <summary>
        /// The Ortho matrix, for Font rendering simplicity.
        /// </summary>
        public Matrix4 Ortho;

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
    }
}
