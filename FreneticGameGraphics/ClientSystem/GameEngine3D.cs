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
using FreneticGameGraphics.GraphicsHelpers;
using FreneticGameCore;
using FreneticGameCore.CoreSystems;
using FreneticGameCore.MathHelpers;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;
using FreneticGameGraphics.ClientSystem.EntitySystem;
using FreneticGameCore.StackNoteSystem;

namespace FreneticGameGraphics.ClientSystem
{
    /// <summary>
    /// Represents a Three-Dimensional game engine.
    /// </summary>
    public class GameEngine3D : GameEngineBase
    {
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
        /// Whether to enable deferred mode lights.
        /// </summary>
        public bool Deferred_Lights = true;

        /// <summary>
        /// Whether to enable deferred mode transparent-object-affecting lights.
        /// </summary>
        public bool Deferred_TransparentLights = true;

        /// <summary>
        /// Whether to enable deferred mode shadows.
        /// </summary>
        public bool Deferred_Shadows = true;

        /// <summary>
        /// Whether to enable deferred mode HDR.
        /// </summary>
        public bool Deferred_HDR = true;

        /// <summary>
        /// Whether deferred mode should use SSAO effects.
        /// </summary>
        public bool Deferred_SSAO = true;

        /// <summary>
        /// Whether deferred mode should have a motion blur effect.
        /// TODO: This can probably be simplified to just a buffer cheat instead of a complex shader with dumb logic.
        /// </summary>
        public bool Deferred_MotionBlur = false;

        /// <summary>
        /// Whether deferred mode should use toonify effects.
        /// </summary>
        public bool Deferred_Toonify = false;

        /// <summary>
        /// Whether deferred mode should use grayscaling effects.
        /// </summary>
        public bool Deferred_Grayscale = false;

        /// <summary>
        /// Whether to have 'bright' transparent objects in deferred mode.
        /// </summary>
        public bool Deferred_BrightTransp = true;

        /// <summary>
        /// The (deferred mode) exposure value.
        /// </summary>
        public float Exposure = 1;

        /// <summary>
        /// Loads all shaders for the standard Game Engine 3D.
        /// </summary>
        public override void GetShaders()
        {
            Shaders3D.LoadAll(Shaders, AllowLL, Forward_Normals, Forward_Lights, Forward_Shadows);
        }

        /// <summary>
        /// Whether forward mode should calculate reflection helpers.
        /// </summary>
        public bool ForwardReflections = false;
        
        /// <summary>
        /// Whether to display decal effects.
        /// </summary>
        public bool DisplayDecals = true;

        /// <summary>
        /// Whether to render the view as a 3D side-by-side view.
        /// </summary>
        public bool Render3DView = false;

        /// <summary>
        /// The current Field Of View, in degrees (Defaults to 70).
        /// TODO: Camera attachment?
        /// </summary>
        public float FOV = 70;

        /// <summary>
        /// The current Z-Near value, defaults to '0.1'.
        /// TODO: Camera attachment?
        /// </summary>
        public float ZNear = 0.1f;

        /// <summary>
        /// Get the Z-Far value (defaults to 1000 autoget).
        /// TODO: Camera attachment?
        /// </summary>
        public Func<float> ZFar = () => 1000;
        
        /// <summary>
        /// Get the Z-Far (OUT-View) value (defaults to 10000 autoget).
        /// TODO: Camera attachment?
        /// </summary>
        public Func<float> ZFarOut = () => 10000;

        /// <summary>
        /// Get the maximum distance of fog. Defaults to match ZFar.
        /// </summary>
        public Func<float> FogMaxDist = null;

        /// <summary>
        /// The "Sun adjustment" backup light color and value.
        /// </summary>
        public Vector4 SunAdjustBackupLight = Vector4.One;

        /// <summary>
        /// The direction of the sun for backup light.
        /// </summary>
        public Location SunAdjustDirection = -Location.UnitZ;

        /// <summary>
        /// Whether dynamic shadows should be handled at all.
        /// </summary>
        public bool EnableDynamicShadows = true;

        /// <summary>
        /// The main 3D view.
        /// </summary>
        public View3D MainView = new View3D();

        /// <summary>
        /// Reloads relevant 3D screen buffers.
        /// </summary>
        public override void ReloadScreenBuffers()
        {
            MainView.Generate(this, Window.Width, Window.Height);
        }

        /// <summary>
        /// The main rendering context.
        /// </summary>
        public RenderContext MainContext = new RenderContext();

        /// <summary>
        /// Whether this engine is a 'sub engine' (otherwise, it's the main engine).
        /// </summary>
        public bool IsSubEngine = false;

        /// <summary>
        /// The sub-view size, if a sub engine.
        /// </summary>
        public Vector2i SubSize = new Vector2i(1024, 768);

        /// <summary>
        /// Loads any additional final data.
        /// </summary>
        public override void PostLoad()
        {
            MainContext.Engine = this;
            FogMaxDist = () => ZFar();
            GraphicsUtil.CheckError("PostLoad - Pre");
            SysConsole.Output(OutputType.INIT, "GameEngine configuring graphics...");
            GL.Enable(EnableCap.CullFace);
            GL.CullFace(CullFaceMode.Front);
            SysConsole.Output(OutputType.INIT, "GameEngine loading main 3D view...");
            if (!IsSubEngine)
            {
                SubSize = new Vector2i(Window.Width, Window.Height);
            }
            MainView.Width = SubSize.X;
            MainView.Height = SubSize.Y;
            if (IsSubEngine)
            {
                MainView.GenerateFBO();
            }
            MainView.Generate(this, SubSize.X, SubSize.Y);
            MainView.Render3D = Render3D;
            MainView.PostFirstRender = ReverseEntities;
            MainView.CameraUp = () => MainCamera.Up;
            AudioCamera = MainCamera;
            ZFar = () => MainCamera.ZFar;
            GraphicsUtil.CheckError("PostLoad - Post");
        }

        /// <summary>
        /// Sorts the entities according to distance from camera view.
        /// </summary>
        public void SortEntities()
        {
            Location pos = MainView.RenderRelative;
            EntityList = EntityList.OrderBy((e) => e.LastKnownPosition.DistanceSquared(pos)).ToList();
        }

        /// <summary>
        /// Reverses the entity order for transparent rendering.
        /// </summary>
        public void ReverseEntities()
        {
            EntityList.Reverse();
        }

        /// <summary>
        /// Renders the standard view's 3D data.
        /// </summary>
        /// <param name="view">The view object.</param>
        public void Render3D(View3D view)
        {
            try
            {
                StackNoteHelper.Push("GameEngine3D - Render All Entities", this);
                // TODO: Out View Rendering!
                GL.ActiveTexture(TextureUnit.Texture1);
                Textures.NormalDef.Bind();
                GL.ActiveTexture(TextureUnit.Texture0);
                foreach (ClientEntity ce in EntityList)
                {
                    // TODO: layering logic of some form instead of this overly basic stuff.
                    if (ShouldRender(ce.Renderer, view.RenderingShadows))
                    {
                        try
                        {
                            StackNoteHelper.Push("GameEngine3D - Render Specific Entity", ce);
                            ce.Renderer?.RenderStandard(MainContext);
                        }
                        finally
                        {
                            StackNoteHelper.Pop();
                        }
                    }
                }
            }
            finally
            {
                StackNoteHelper.Pop();
            }
        }

        /// <summary>
        /// The main default camera for the main view.
        /// </summary>
        public Camera3D MainCamera = new Camera3D();


        /// <summary>
        /// Gets the 3D renderer.
        /// </summary>
        public Renderer Rendering
        {
            get
            {
                return Client.Rendering3D;
            }
        }
        /// <summary>
        /// Renders a single frame of the 3D game engine.
        /// </summary>
        public override void RenderSingleFrame()
        {
            // Audio handling
            Sounds.Update(AudioCamera.Position, AudioCamera.Direction, AudioCamera.Up, Location.Zero, Window.Focused);
            // Setup requirements
            GL.Enable(EnableCap.DepthTest);
            GL.Enable(EnableCap.CullFace);
            // Set camera to view
            MainView.CameraPos = MainCamera.Position;
            MainView.ForwardVec = MainCamera.Direction;
            ZNear = MainCamera.ZNear;
            // Sort entities to prep render
            SortEntities();
            // Render
            MainView.Render();
            // Fix entities back to sorted value (They will be flipped by the render process)
            ReverseEntities();
        }
    }
}
