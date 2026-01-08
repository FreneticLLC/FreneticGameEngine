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
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FreneticUtilities.FreneticExtensions;
using FGECore.CoreSystems;
using FGECore.MathHelpers;
using FGECore.ModelSystems;
using FGECore.StackNoteSystem;
using FGEGraphics.ClientSystem.EntitySystem;
using FGEGraphics.ClientSystem.ViewRenderSystem;
using FGEGraphics.GraphicsHelpers;
using FGEGraphics.GraphicsHelpers.FontSets;
using FGEGraphics.GraphicsHelpers.Models;
using FGEGraphics.GraphicsHelpers.Shaders;
using FGEGraphics.GraphicsHelpers.Textures;
using FGEGraphics.UISystem.InputSystems;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Input;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;

using ErrorCode = OpenTK.Graphics.OpenGL4.ErrorCode;

namespace FGEGraphics.ClientSystem;

/// <summary>Represents a game client window for a game.</summary>
public class GameClientWindow : GameInstance<ClientEntity, GameEngineBase>, IDisposable
{
    /// <summary>The primary window for the game.</summary>
    public GameWindow Window;

    /// <summary>The current primary engine, dominating the view.</summary>
    public GameEngineBase CurrentEngine;

    /// <summary>Gets a 2D form of the current engine, if valid.</summary>
    public GameEngine2D Engine2D => CurrentEngine as GameEngine2D;

    /// <summary>Gets a 3D form of the current engine, if valid.</summary>
    public GameEngine3D Engine3D => CurrentEngine as GameEngine3D;

    /// <summary>The <see cref="SysConsole"/> output type for "init" messages: <see cref="OutputType.CLIENTINIT"/>.</summary>
    public override OutputType InitOutputType => OutputType.CLIENTINIT;

    /// <summary>The <see cref="SysConsole"/> output type for "info" messages: <see cref="OutputType.CLIENTINFO"/>.</summary>
    public override OutputType InfoOutputType => OutputType.CLIENTINFO;

    /// <summary>The VR support system, if any.</summary>
    public VRSupport VR = null;

    /// <summary>The title of the window.</summary>
    public readonly string StartingWindowTitle;

    /// <summary>The X-coordinate of the mouse in screen coordinates.</summary>
    public float MouseX;

    /// <summary>The Y-coordinate of the mouse in screen coordinates.</summary>
    public float MouseY;

    /// <summary>The current mouse state for this tick.</summary>
    public MouseState CurrentMouse;

    /// <summary>The mouse state during the previous tick.</summary>
    public MouseState PreviousMouse;

    /// <summary>System to help with models.</summary>
    public ModelEngine Models;

    /// <summary>The render helper system, for 3D rendering.</summary>
    public Renderer Rendering3D;

    /// <summary>The 2D rendering helper, for any UI or general 2D logic.</summary>
    public Renderer2D Rendering2D;

    /// <summary>The keyboard input helper.</summary>
    public KeyHandler Keyboard;

    /// <summary>The current Orthographic matrix, for Font rendering simplicity.</summary>
    public Matrix4 Ortho;

    /// <summary>The shader system.</summary>
    public ShaderEngine Shaders;

    /// <summary>Helper for internal GL font data in the system.</summary>
    public GLFontEngine GLFonts;

    /// <summary>Helper for all font sets in the system.</summary>
    public FontSetEngine FontSets;

    /// <summary>Helper for all textures in the system.</summary>
    public TextureEngine Textures;

    /// <summary>Fired when the window is set up.</summary>
    public Action OnWindowSetUp;

    /// <summary>Fired when the window is loading. Use this to load any data you need.</summary>
    public Action OnWindowLoad;

    /// <summary>Fired when the window is closed.</summary>
    public Action OnWindowClosed;

    /// <summary>Whether the program should forcibly shut down when the window is closed.</summary>
    public bool ExitOnClose = false;

    /// <summary>The currently rendering UI for this engine.</summary>
    public ViewUI2D MainUI;

    /// <summary>Standard framerate (FPS) limit when playing. See also <see cref="MaxFpsWhenUnselected"/>.</summary>
    public int MaxFps = 1000;

    /// <summary>Optional framerate limit when the window is unselected/unfocused. <c>0</c> means no limit, <c>30</c> means a 30 FPS cap when the window is unselected.
    /// Does nothing when the window is selected/focused. See also <see cref="MaxFps"/>.</summary>
    public int MaxFpsWhenUnselected = 0;

    /// <summary>Internal data for the <see cref="GameClientWindow"/>.</summary>
    public struct InternalData
    {
        /// <summary>The expected window width.</summary>
        public int WindowWidth;

        /// <summary>The expected window height.</summary>
        public int WindowHeight;

        /// <summary>
        /// The color to blank the screen to every frame.
        /// Defaults to cyan (0:1:1:1).
        /// </summary>
        public float[] ScreenClearColor;

        /// <summary>The depth value to blank the screen to every frame. Defaults to 1 (full-distance).</summary>
        public float[] DepthClear;

        /// <summary>Whether the window is already loaded.</summary>
        public bool Loaded;
    }

    /// <summary>Internal data for the <see cref="GameClientWindow"/>.</summary>
    public InternalData Internal = new() { WindowWidth = 800, WindowHeight = 600, ScreenClearColor = [0f, 1f, 1f, 1f], DepthClear = [1f], Loaded = false };

    /// <summary>Constructs the game client window.</summary>
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
        Engines.Add(CurrentEngine);
    }

    /// <summary>Monitors on-window mouse movement.</summary>
    /// <param name="e">The event data.</param>
    private void Mouse_Move(MouseMoveEventArgs e)
    {
        MouseX = e.X;
        MouseY = e.Y;
    }

    /// <summary>Window width.</summary>
    public int WindowWidth
    {
        get
        {
            return Window == null ? Internal.WindowWidth : Window.ClientSize.X;
        }
        set
        {
            Internal.WindowWidth = value;
            if (Window != null)
            {
                Window.ClientSize = new OpenTK.Mathematics.Vector2i(Internal.WindowWidth, Internal.WindowHeight);
            }
        }
    }

    /// <summary>Window height.</summary>
    public int WindowHeight
    {
        get
        {
            return Window == null ? Internal.WindowHeight : Window.ClientSize.Y;
        }
        set
        {
            Internal.WindowHeight = value;
            if (Window != null)
            {
                Window.ClientSize = new OpenTK.Mathematics.Vector2i(Internal.WindowWidth, Internal.WindowHeight);
            }
        }
    }

    /// <summary>Size of the window.</summary>
    public FGECore.MathHelpers.Vector2i WindowSize
    {
        get
        {
            return Window == null ? new FGECore.MathHelpers.Vector2i(Internal.WindowWidth, Internal.WindowHeight) : new FGECore.MathHelpers.Vector2i(Window.ClientSize.X, Window.ClientSize.Y);
        }
        set
        {
            Internal.WindowWidth = value.X;
            Internal.WindowHeight = value.Y;
            if (Window != null)
            {
                Window.ClientSize = new OpenTK.Mathematics.Vector2i(value.X, value.Y);
            }
        }
    }

    /// <summary>Starts the game engine, and begins the primary loop.</summary>
    /// <param name="initialFlags">The initial window flag.</param>
    public void Start(WindowBorder initialFlags = WindowBorder.Fixed)
    {
        using var _push = StackNoteHelper.UsePush("GameClientWindow - Start, run", this);
        Logs.ClientInit("GameEngine loading...");
        GraphicsUtil.Init();
        Window = new GameWindow(new GameWindowSettings() { UpdateFrequency = MaxFps }, new NativeWindowSettings()
        {
            ClientSize = new OpenTK.Mathematics.Vector2i(Internal.WindowWidth, Internal.WindowHeight),
            Title = StartingWindowTitle,
            Flags = ContextFlags.ForwardCompatible,
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
        Window.Closing += Window_Closed;
        Window.Resize += Window_Resize;
        Window.FocusedChanged += Window_FocusedChanged;
        Logs.ClientInit("GameEngine calling SetUp event...");
        OnWindowSetUp?.Invoke();
        Logs.ClientInit("GameEngine running...");
        Window.Run();
    }

    /// <summary>Fired when the window is resized.</summary>
    private void Window_Resize(ResizeEventArgs e)
    {
        if (Internal.Loaded)
        {
            CurrentEngine.ReloadScreenBuffers();
        }
    }

    /// <summary>Loads all content for the game, and starts the systems.</summary>
    private void Window_Load()
    {
        Window.Focus();
        Logs.ClientInit("GameClient starting load sequence...");
        string GLVendor = GL.GetString(StringName.Vendor);
        string GLVersion = GL.GetString(StringName.Version);
        string GLRenderer = GL.GetString(StringName.Renderer);
        int maxVertComp = GL.GetInteger(GetPName.MaxVertexUniformComponents);
        int maxFragComp = GL.GetInteger(GetPName.MaxFragmentUniformComponents);
        Logs.ClientInit($"Vendor: {GLVendor}, GLVersion: {GLVersion}, Renderer: {GLRenderer}");
        Logs.Debug($"Max uniform components: {maxVertComp} vertex, {maxFragComp} fragment");
        GL.Viewport(0, 0, Window.ClientSize.X, Window.ClientSize.Y);
        GL.Enable(EnableCap.Blend);
        GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
        GL.PolygonMode(TriangleFace.FrontAndBack, PolygonMode.Fill);
        GL.Disable(EnableCap.CullFace);
        GraphicsUtil.CheckError("GEB - Initial");
        InstanceInit();
        Logs.ClientInit("GameClient loading shader helpers...");
        Shaders = new ShaderEngine();
        if (GLVendor.ToLowerFast().Contains("intel"))
        {
            Logs.ClientInit($"Disabling good graphics (Appears to be Intel: '{GLVendor}')");
            Shaders.MCM_GOOD_GRAPHICS = false;
            Shaders.ShutUpIntel = true;
        }
        if (GLVendor.ToLowerFast().Contains("ati technologies"))
        {
            Logs.ClientInit("Some AMD GPUs have been seen messing up shader compiles, enabling workaround.");
            Shaders.ShutUpIntel = true;
        }
        Shaders.InitShaderSystem(Files);
        Logs.ClientInit("GameClient loading texture helpers...");
        Textures = new TextureEngine();
        Textures.InitTextureSystem(Files, AssetStreaming, Schedule);
        GraphicsUtil.CheckError("GEB - Textures");
        Logs.ClientInit("GameClient loading font helpers...");
        GLFonts = new GLFontEngine(Textures, Shaders);
        GLFonts.Init(Files);
        FontSets = new FontSetEngine(GLFonts)
        {
            FixToShader = Shaders.ColorMult2D.UnderlyingShader
        };
        // TODO: FGE/Core->Languages engine!
        FontSets.Init((subdata) => null, () => Ortho, () => GlobalTickTime);
        GraphicsUtil.CheckError("GEB - Fonts");
        Logs.ClientInit("GameClient loading 2D/UI render helper...");
        Keyboard = new KeyHandler(this);
        MainUI = new ViewUI2D(this);
        Logs.ClientInit("GameClient loading model engine...");
        Models = new ModelEngine();
        Models.Init(Animations, this);
        Logs.ClientInit("GameClient loading render helper...");
        Rendering3D = new Renderer(Textures, Shaders, Models);
        Rendering3D.Init();
        Rendering2D = new Renderer2D(this);
        Rendering2D.Init();
        Logs.ClientInit("GameClient calling engine load...");
        CurrentEngine.Load();
        Logs.ClientInit("GameClient calling external load event...");
        OnWindowLoad?.Invoke();
        Logs.ClientInit("GameClient is ready and loaded! Starting main game loop...");
        GraphicsUtil.CheckError("GEB - Loaded");
        Internal.Loaded = true;
    }

    /// <summary>Called when the window is focused or unfocused.</summary>
    public void Window_FocusedChanged(FocusedChangedEventArgs e)
    {
        if (e.IsFocused)
        {
            Window.UpdateFrequency = MaxFps;
        }
        else
        {
            Window.UpdateFrequency = MaxFpsWhenUnselected <= 0 ? MaxFps : MaxFpsWhenUnselected;
        }
    }

    /// <summary>Called when the window is closed.</summary>
    public void Window_Closed(CancelEventArgs args)
    {
        if (!InstanceShutdownToken.IsCancellationRequested)
        {
            InstanceShutdown();
            return;
        }
        OnWindowClosed?.Invoke();
        if (ExitOnClose)
        {
            Logs.ClientInfo("Window closed, full exiting now.");
            SysConsole.ShutDown();
            Environment.Exit(0);
        }
    }

    /// <summary>Renders a single frame of the game, and also ticks.</summary>
    public void Window_RenderFrame(FrameEventArgs e)
    {
        using var _push = StackNoteHelper.UsePush("GameClientWindow - Render and tick frame", this);
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
            Logs.Warning($"Uncaught GL Error: {ec}");
            ec = GL.GetError();
        }
        // Second step: clear the screen
        GL.ClearBuffer(ClearBuffer.Color, 0, Internal.ScreenClearColor);
        GL.ClearBuffer(ClearBuffer.Depth, 0, Internal.DepthClear);
        GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
        GL.DrawBuffer(DrawBufferMode.Back);
        GraphicsUtil.CheckError("GameClient - Pre");
        // Tick helpers
        Models.Update(GlobalTickTime);
        GraphicsUtil.CheckError("GameClient - PostModelUpdate");
        // Third step: general game rendering
        CurrentEngine.RenderSingleFrame();
        GraphicsUtil.CheckError("GameClient - PostMainEngine");
        if (VR != null) // VR Push-To-Screen
        {
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
            GL.DrawBuffer(DrawBufferMode.Back);
            Shaders.ColorMultShader.Bind();
            Rendering3D.SetColor(Vector4.One, Engine3D.MainView);
            GL.Disable(EnableCap.DepthTest);
            GL.Disable(EnableCap.CullFace);
            GL.UniformMatrix4(ShaderLocations.Common.PROJECTION, false, ref View3DInternalData.SimpleOrthoMatrix);
            GL.UniformMatrix4(ShaderLocations.Common.WORLD, false, ref View3DInternalData.IdentityMatrix);
            Engine3D.MainView.Internal.CurrentFBOTexture.Bind();
            Rendering3D.RenderRectangle(-1, -1, 1, 1);
            GL.Enable(EnableCap.DepthTest);
            GL.Enable(EnableCap.CullFace);
            GraphicsUtil.CheckError("GameClient - PostVRPush");
        }
        // Add the UI Layer too
        MainUI.Draw();
        GraphicsUtil.CheckError("GameClient - PostUI");
        // Fourth step: clean up!
        GraphicsUtil.BindTexture(TextureTarget.Texture2D, 0);
        GL.BindVertexArray(0);
        GraphicsUtil.UseProgram("GameClientWindow - PostRender/PreTick", 0);
        ShaderEngine.BoundNow = null;
        // Semi-final step: Tick logic!
        GraphicsUtil.CheckError("GameClient - PreTick");
        // Main instance tick.
        Tick();
        // Primary UI tick
        MainUI.Tick();
        Keyboard.ResetState();
        GraphicsUtil.CheckError("GameClient - PostTick");
        // Final non-VR step: Swap the render buffer onto the screen!
        Window.SwapBuffers();
        VR?.Submit(); // VR Push-To-HMD
        GraphicsUtil.CheckError("GameClient - Post");
    }

    /// <summary>Converts the window into VR mode, if possible.</summary>
    /// <returns>True if VR is enabled, false if not (there was an error, or there is no VR support on the client system).</returns>
    public bool ActivateVR()
    {
        if (VR is not null)
        {
            return true;
        }
        VR = VRSupport.TryInit(this);
        return VR is not null;
    }

    /// <inheritdoc/>
    public override void InstanceShutdown()
    {
        if (InstanceShutdownToken.IsCancellationRequested)
        {
            return;
        }
        base.InstanceShutdown();
        if (VR is not null)
        {
            Logs.Debug("[Shutdown] Closing VR...");
            VR.Stop();
            VR = null;
        }
        try
        {
            Window.Close();
        }
        catch (Exception) { } // Ignore
    }

    /// <summary>Dumb MS logic dispose method.</summary>
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

    /// <summary>Disposes the window client.</summary>
    public void Dispose()
    {
        GC.SuppressFinalize(this);
        Dispose(true);
    }

    /// <summary>Returns a string of this object.</summary>
    public override string ToString()
    {
        return "GameClientWindow";
    }
}
