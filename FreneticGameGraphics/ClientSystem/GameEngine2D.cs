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
using FreneticGameGraphics.LightingSystem;
using FreneticGameGraphics.GraphicsHelpers;
using System.Threading;

namespace FreneticGameGraphics.ClientSystem
{
    /// <summary>
    /// Represents a 2D game's coremost engine.
    /// Handles things like window management.
    /// TODO: Split the rendering system into a View2D, separate from the main game engine?
    /// </summary>
    public class GameEngine2D
    {
        /// <summary>
        /// The primary window for the game.
        /// </summary>
        public GameWindow Window;
        
        /// <summary>
        /// The window can never render coordinates past this point.
        /// This constant set for precision-safety reasons.
        /// </summary>
        public const float MAX_COORD = 1000000.0f;

        /// <summary>
        /// Current frame delta (seconds).
        /// </summary>
        public double Delta;

        /// <summary>
        /// How long the game has run (seconds).
        /// </summary>
        public double GlobalTickTime;

        /// <summary>
        /// The title of the window.
        /// </summary>
        public readonly string StartingWindowTitle;

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
        /// Helps with rendering.
        /// </summary>
        public Renderer2D RenderHelper;

        /// <summary>
        /// Fired when the window is set up.
        /// </summary>
        public event Action OnWindowSetUp;

        /// <summary>
        /// Fired when the window is loading. Use this to load any data you need.
        /// </summary>
        public event Action OnWindowLoad;

        /// <summary>
        /// Fired every frame for logic update steps.
        /// Note that the first tick fires AFTER the first render.
        /// </summary>
        public event Action OnTick;

        /// <summary>
        /// Fired when the window is closed.
        /// </summary>
        public event Action OnClosed;

        /// <summary>
        /// Renders all objects. The boolean indicates whether to render objects that don't affect lighting (Meaning, things that don't cast shadows).
        /// </summary>
        public event Action<bool> RenderAllObjects;

        /// <summary>
        /// First shader stage: computes a 'light map' for each light source.
        /// </summary>
        public Shader Shader_Lightmap;

        /// <summary>
        /// Second shader stage: combines various light map inputs into a buffer.
        /// </summary>
        public Shader Shader_Combine;

        /// <summary>
        /// Final shader stage: adds light buffer to the scene.
        /// </summary>
        public Shader Shader_Addlighttoscene;

        /// <summary>
        /// Whether to use the 2D lighting engine.
        /// </summary>
        public bool UseLightEngine = false;

        /// <summary>
        /// Current zoom. Smaller numbers = zoomed in. Bigger numbers = zoomed out. Defaults to 1.
        /// </summary>
        public float Zoom = 1f;

        /// <summary>
        /// How much the zoom value is multiplied by to get pixel size.
        /// </summary>
        public float ZoomMultiplier = 1024.0f;

        /// <summary>
        /// Minimum zoom value (Most zoomed in).
        /// </summary>
        public float MinimumZoom = 0.01f;

        /// <summary>
        /// Maximum zoom value (most zoomed out).
        /// </summary>
        public float MaximumZoom = 100.0f;

        /// <summary>
        /// Center location of the view, in world coordinates.
        /// </summary>
        public Vector2 ViewCenter = Vector2.Zero;

        /// <summary>
        /// Whether the program should shut down when the window is closed.
        /// </summary>
        public bool ExitOnClose = true;

        /// <summary>
        /// Sets up the game engine 2D.
        /// Considering also attaching to available events such as <see cref="OnWindowSetUp"/>.
        /// Then call <see cref="Start"/>.
        /// </summary>
        /// <param name="render">The method that renders all visible objects from the game.</param>
        /// <param name="_windowTitle">The title, if different from game program descriptor.</param>
        public GameEngine2D(Action<bool> render, string _windowTitle = null)
        {
            RenderAllObjects = render;
            StartingWindowTitle = _windowTitle ?? Program.GameName + " v" + Program.GameVersion + " " + Program.GameVersionDescription;
        }

        /// <summary>
        /// Starts the game engine, and begins the primary loop.
        /// </summary>
        public void Start()
        {
            SysConsole.Output(OutputType.INIT, "GameEngine2D loading...");
            Window = new GameWindow(800, 600, GraphicsMode.Default, StartingWindowTitle, GameWindowFlags.FixedWindow, DisplayDevice.Default, 4, 3, GraphicsContextFlags.ForwardCompatible);
            Window.Load += Window_Load;
            Window.RenderFrame += Window_RenderFrame;
            Window.Mouse.Move += Mouse_Move;
            Window.Closed += Window_Closed;
            SysConsole.Output(OutputType.INIT, "GameEngine2D calling SetUp event...");
            OnWindowSetUp?.Invoke();
            SysConsole.Output(OutputType.INIT, "GameEngine2D running...");
            Window.Run();
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            OnClosed?.Invoke();
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
        /// The Ortho matrix, for Font rendering simplicity.
        /// </summary>
        public Matrix4 Ortho;

        /// <summary>
        /// Loads all shaders for the standard Game Engine 2D.
        /// </summary>
        public void GetShaders()
        {
            Shader_Lightmap = Shaders.GetShader("lightmap2d");
            Shader_Addlighttoscene = Shaders.GetShader("addlighttoscene2d");
            Shader_Combine = Shaders.GetShader("combine2d");
        }

        /// <summary>
        /// Loads all content for the game, and starts the systems.
        /// </summary>
        /// <param name="sender">Irrelevant.</param>
        /// <param name="e">Irrelevant.</param>
        private void Window_Load(object sender, EventArgs e)
        {
            SysConsole.Output(OutputType.INIT, "GameEngine2D starting load sequence...");
            GL.Viewport(0, 0, Window.Width, Window.Height);
            GL.Enable(EnableCap.Texture2D);
            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);
            GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Fill);
            GL.Disable(EnableCap.CullFace);
            SysConsole.Output(OutputType.INIT, "GameEngine2D loading file helpers...");
            Files = new FileHandler();
            Files.Init();
            SysConsole.Output(OutputType.INIT, "GameEngine2D loading shader helpers...");
            Shaders = new ShaderEngine();
            Shaders.InitShaderSystem();
            GetShaders();
            SysConsole.Output(OutputType.INIT, "GameEngine2D loading texture helpers...");
            Textures = new TextureEngine();
            Textures.InitTextureSystem(Files);
            SysConsole.Output(OutputType.INIT, "GameEngine2D loading render helpers...");
            RenderHelper = new Renderer2D(Textures, Shaders);
            RenderHelper.Init();
            SysConsole.Output(OutputType.INIT, "GameEngine2D loading font helpers...");
            GLFonts = new GLFontEngine(Shaders);
            GLFonts.Init(Files);
            FontSets = new FontSetEngine(GLFonts);
            // TODO: Languages engine!
            FontSets.Init((subdata) => null, () => Ortho, () => GlobalTickTime);
            SysConsole.Output(OutputType.INIT, "GameEngine2D loading light helpers...");
            LoadLightHelpers();
            SysConsole.Output(OutputType.INIT, "GameEngine2D calling load event...");
            OnWindowLoad?.Invoke();
            SysConsole.Output(OutputType.INIT, "GameEngine2D is ready and loaded! Starting main game loop...");
        }

        /// <summary>
        /// All current lights.
        /// </summary>
        public List<PointLight2D> Lights = new List<PointLight2D>();

        int c_FBO;

        int c_FBO_Tex;

        int l_FBO;

        int l_FBO_Tex;

        /// <summary>
        /// Calculates and loads some light helper information.
        /// </summary>
        public void LoadLightHelpers()
        {
            c_FBO = GL.GenFramebuffer();
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, c_FBO);
            c_FBO_Tex = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, c_FBO_Tex);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, Window.Width, Window.Height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, IntPtr.Zero);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (uint)TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (uint)TextureMagFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (uint)TextureWrapMode.ClampToEdge);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (uint)TextureWrapMode.ClampToEdge);
            GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, TextureTarget.Texture2D, c_FBO_Tex, 0);
            l_FBO = GL.GenFramebuffer();
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, l_FBO);
            l_FBO_Tex = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, l_FBO_Tex);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba32f, Window.Width, Window.Height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, IntPtr.Zero);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (uint)TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (uint)TextureMagFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (uint)TextureWrapMode.ClampToEdge);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (uint)TextureWrapMode.ClampToEdge);
            GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, TextureTarget.Texture2D, l_FBO_Tex, 0);
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
            GL.BindTexture(TextureTarget.Texture2D, 0);
            Lights.Add(new PointLight2D(new Vector2(400, 400), 800));
            Lights.Add(new PointLight2D(new Vector2(0, 2500), 3200));
        }

        /// <summary>
        /// The primary render context for this game engine 2D.
        /// </summary>
        public RenderContext2D MainRenderContext = new RenderContext2D();

        /// <summary>
        /// The color to blank the screen to every frame.
        /// Defaults to cyan (0:1:1:1).
        /// </summary>
        public float[] ScreenClearColor = new float[] { 0, 1, 1, 1 };

        /// <summary>
        /// The current view scaler.
        /// </summary>
        public Vector2 Scaler = Vector2.One;

        /// <summary>
        /// The current view adder.
        /// </summary>
        public Vector2 Adder = Vector2.Zero;

        /// <summary>
        /// The original scaler value for any given frame.
        /// </summary>
        public Vector2 OriginalScaler = Vector2.One;

        /// <summary>
        /// The original adder value for any given frame.
        /// </summary>
        public Vector2 OriginalAdder = Vector2.Zero;

        /// <summary>
        /// The current world-space mouse coordinates.
        /// </summary>
        public Vector2 MouseCoords;

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
            MainRenderContext.Width = Window.Width;
            MainRenderContext.Height = Window.Height;
            MainRenderContext.Zoom = Zoom;
            MainRenderContext.ZoomMultiplier = ZoomMultiplier;
            MainRenderContext.ViewCenter = ViewCenter;
            GlobalTickTime += Delta;
            // Second step: clear the screen
            GL.ClearBuffer(ClearBuffer.Color, 0, ScreenClearColor);
            // Third step: Prepare positioning and blank out shaders
            if (Zoom > MaximumZoom)
            {
                Zoom = MaximumZoom;
            }
            else if (Zoom < MinimumZoom)
            {
                Zoom = MinimumZoom;
            }
            float aspect = Window.Width / (float)Window.Height;
            float sc = 1.0f / (Zoom * ZoomMultiplier);
            OriginalScaler = new Vector2(sc, sc * aspect);
            OriginalAdder = new Vector2(ViewCenter.X * Scaler.X, ViewCenter.Y * Scaler.Y);
            Ortho = Matrix4.CreateOrthographicOffCenter(OriginalAdder.X - OriginalScaler.X, OriginalAdder.X + OriginalScaler.X, OriginalAdder.Y + OriginalScaler.Y, OriginalAdder.Y - OriginalScaler.Y, -1, 1);
            Scaler = OriginalScaler;
            Adder = OriginalAdder;
            float wx = MouseX / (float)Window.Width;
            float wy = MouseY / (float)Window.Height;
            wx -= 0.5f;
            wy -= 0.5f;
            wx *= 2f;
            wy *= 2f;
            wx /= OriginalScaler.X;
            wy /= OriginalScaler.Y;
            MouseCoords = new Vector2(wx, wy);
            Shader_Combine.Bind();
            GL.Uniform2(1, ref Scaler);
            GL.Uniform2(2, ref Adder);
            GL.Uniform1(7, (float)Window.Width / (float)Window.Height);
            Shader_Lightmap.Bind();
            GL.Uniform2(1, ref Scaler);
            GL.Uniform2(2, ref Adder);
            Shaders.ColorMult2DShader.Bind();
            GL.Uniform2(1, ref Scaler);
            GL.Uniform2(2, ref Adder);
            // Fourth step: Pass to the primary rendering system
            try
            {
                Render();
                GraphicsUtil.CheckError("Post-Render");
            }
            catch (Exception ex)
            {
                if (ex is ThreadAbortException)
                {
                    throw;
                }
                Console.WriteLine("Rendering exception: " + ex);
            }
            // FIfth-final step: clean up!
            GL.BindTexture(TextureTarget.Texture2D, 0);
            GL.BindVertexArray(0);
            GL.UseProgram(0);
            // Semi-final step: Tick logic!
            Tick();
            // Final step: Swap the render buffer onto the screen!
            Window.SwapBuffers();
        }

        /// <summary>
        /// Renders the entire GameEngine2D.
        /// </summary>
        private void Render()
        {
            if (!UseLightEngine)
            {
                Shaders.ColorMult2DShader.Bind();
                RenderAllObjects(true);
                return;
            }
            GraphicsUtil.CheckError("Render - Begin");
            // TODO: Discard fully out-of-view lights!
            Shader_Lightmap.Bind();
            if (Lights.Count > 0)
            {
                GL.BindFramebuffer(FramebufferTarget.Framebuffer, Lights[0].FBO);
                GL.DrawBuffer(DrawBufferMode.ColorAttachment0);
            }
            GraphicsUtil.CheckError("Render - Before Light Precalcer");
            for (int i = 0; i < Lights.Count; i++)
            {
                Lights[i].PrepareLightmap();
                GraphicsUtil.CheckError("Render - Light Precalcer (Prep)");
                GL.Uniform2(1, Scaler = Lights[i].GetScaler());
                GL.Uniform2(2, Adder = Lights[i].GetAdder());
                MainRenderContext.Scaler = Scaler;
                MainRenderContext.Adder = Adder;
                GraphicsUtil.CheckError("Render - Light Precalcer");
                RenderAllObjects(false);
            }
            GraphicsUtil.CheckError("Render - Lights precalced");
            GL.Viewport(0, 0, Window.Width, Window.Height);
            Shaders.ColorMult2DShader.Bind();
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, c_FBO);
            GL.ClearBuffer(ClearBuffer.Color, 0, new float[] { 0, 0, 0, 1 });
            Scaler = OriginalScaler;
            Adder = OriginalAdder;
            GL.Uniform2(1, ref Scaler);
            GL.Uniform2(2, ref Adder);
            MainRenderContext.Scaler = Scaler;
            MainRenderContext.Adder = Adder;
            GraphicsUtil.CheckError("Render - Lights prepped");
            RenderAllObjects(true);
            Shader_Combine.Bind();
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, l_FBO);
            GL.ClearBuffer(ClearBuffer.Color, 0, new float[] { 0, 0, 0, 1 });
            Scaler = Vector2.One;
            Adder = Vector2.Zero;
            GL.Uniform2(1, ref Scaler);
            GL.Uniform2(2, ref Adder);
            MainRenderContext.Scaler = Scaler;
            MainRenderContext.Adder = Adder;
            GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.One);
            GraphicsUtil.CheckError("Render - Setup Lights combine");
            for (int i = 0; i < Lights.Count; i++)
            {
                GL.Uniform2(3, Lights[i].GetSecondScaler(MainRenderContext));
                GL.Uniform2(4, Lights[i].GetSecondAdder(MainRenderContext));
                GL.Uniform1(5, (float)Lights[i].Width);
                GL.Uniform4(6, new Vector4(Lights[i].Color.R, Lights[i].Color.G, Lights[i].Color.B, Lights[i].Color.A));
                GL.BindTexture(TextureTarget.Texture2D, Lights[i].FBO_Tex);
                GL.Uniform1(8, Lights[i].SubDivider);
                RenderHelper.RenderRectangle(MainRenderContext, -1, -1, 1, 1);
            }
            GraphicsUtil.CheckError("Render - Lights combined");
            GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);
            GL.ActiveTexture(TextureUnit.Texture1);
            GL.BindTexture(TextureTarget.Texture2D, l_FBO_Tex);
            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, c_FBO_Tex);
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
            GL.DrawBuffer(DrawBufferMode.Back);
            Shader_Addlighttoscene.Bind();
            Scaler = Vector2.One;
            Adder = Vector2.Zero;
            GL.Uniform2(1, ref Scaler);
            GL.Uniform2(2, ref Adder);
            MainRenderContext.Scaler = Scaler;
            MainRenderContext.Adder = Adder;
            RenderHelper.RenderRectangle(MainRenderContext, -1, -1, 1, 1);
            GraphicsUtil.CheckError("Render - Added");
            GL.BindTexture(TextureTarget.Texture2D, 0);
            GL.ActiveTexture(TextureUnit.Texture1);
            GL.BindTexture(TextureTarget.Texture2D, 0);
            GL.ActiveTexture(TextureUnit.Texture0);
            Shaders.ColorMult2DShader.Bind();
            GraphicsUtil.CheckError("Render - Complete");
        }

        /// <summary>
        /// The internal GameEngine2D tick sequence.
        /// </summary>
        private void Tick()
        {
            OnTick?.Invoke();
        }
    }
}
