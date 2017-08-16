using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FreneticGameGraphics.GraphicsHelpers;
using FreneticGameCore;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;

namespace FreneticGameGraphics.ClientSystem
{
    /// <summary>
    /// Represents a Three-Dimensional game engine.
    /// </summary>
    public class GameEngine3D : GameEngineBase
    {
        /// <summary>
        /// Sets up the game engine 3D.
        /// Considering also attaching to available events such as <see cref="GameEngineBase.OnWindowSetUp"/>.
        /// Then call <see cref="GameEngineBase.Start"/>.
        /// </summary>
        /// <param name="_windowTitle">The title, if different from game program descriptor.</param>
        public GameEngine3D(string _sWindowTitle) : base(_sWindowTitle)
        {
        }

        /// <summary>
        /// The list of common shaders for this engine.
        /// </summary>
        public GE3DShaders Shaders3D = new GE3DShaders();

        /// <summary>
        /// Whether to allow LL light helpers.
        /// </summary>
        public bool AllowLL = false;

        /// <summary>
        /// Whether to enable forward lights.
        /// </summary>
        public bool Forward_Lights = true;

        /// <summary>
        /// Whether to enable forward normal effects.
        /// </summary>
        public bool Forward_Normals = true;

        /// <summary>
        /// Whether to enable forward shadows.
        /// </summary>
        public bool Forward_Shadows = false;

        /// <summary>
        /// Loads all shaders for the standard Game Engine 3D.
        /// </summary>
        public override void GetShaders()
        {
            Shaders3D.LoadAll(Shaders, AllowLL, Forward_Normals, Forward_Lights, Forward_Shadows);
        }

        /// <summary>
        /// The render helper system.
        /// </summary>
        public Renderer Rendering;

        /// <summary>
        /// System to help with models.
        /// </summary>
        public ModelEngine Models;

        /// <summary>
        /// Loads any additional final data.
        /// </summary>
        public override void PostLoad()
        {
            GraphicsUtil.CheckError("PostLoad - Pre");
            SysConsole.Output(OutputType.INIT, "GameEngine configuring graphics...");
            GL.Enable(EnableCap.CullFace);
            GL.CullFace(CullFaceMode.Front);
            SysConsole.Output(OutputType.INIT, "GameEngine loading model engine...");
            Models = new ModelEngine();
            Models.Init(null, this);
            SysConsole.Output(OutputType.INIT, "GameEngine loading render helper...");
            Rendering = new Renderer(Textures, Shaders, Models);
            Rendering.Init();
            GraphicsUtil.CheckError("PostLoad - Post");
        }

        /// <summary>
        /// Renders a single frame of the 3D game engine.
        /// </summary>
        public override void RenderSingleFrame()
        {
            Models.Update(GlobalTickTime);
        }
    }
}
