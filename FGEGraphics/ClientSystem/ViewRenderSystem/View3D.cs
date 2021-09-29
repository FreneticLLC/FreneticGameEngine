//
// This file is part of the Frenetic Game Engine, created by Frenetic LLC.
// This code is Copyright (C) Frenetic LLC under the terms of a strict license.
// See README.md or LICENSE.txt in the FreneticGameEngine source root for the contents of the license.
// If neither of these are available, assume that neither you nor anyone other than the copyright holder
// hold any right or permission to use this software until such time as the official license is identified.
//

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using FGECore;
using FGECore.CoreSystems;
using FGECore.MathHelpers;
using FGECore.UtilitySystems;
using FGEGraphics.GraphicsHelpers;
using FGEGraphics.LightingSystem;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace FGEGraphics.ClientSystem.ViewRenderSystem
{
    /// <summary>
    /// Represents a 3D view port.
    /// <para>Used mainly by <see cref="GameEngine3D"/>.</para>
    /// <para>To use outside of the game engine: Call <see cref="View3DGenerationHelper.Generate(int, int)"/>,
    /// then set <see cref="View3DConfiguration.Render3D"/>, <see cref="View3DConfiguration.PostFirstRender"/>, ...
    /// and call <see cref="Render"/> every frame.</para>
    /// </summary>
    public class View3D : View3DCoreDataSet
    {
        /// <summary>
        /// Constructs this <see cref="View3D"/> instance.
        /// </summary>
        /// <param name="_engine">The relevant game engine instance.</param>
        public View3D(GameEngine3D _engine)
        {
            View = this;
            Engine = _engine;
            Shaders = _engine.Shaders3D;
            GenerationHelper = new View3DGenerationHelper();
            Statistics = new View3DStats();
            State = new View3DState();
            Config = new View3DConfiguration();
            Internal = new View3DInternalData();
            GenerationHelper.CopyDataFrom(this);
            ForwardRenderer.CopyDataFrom(this);
            DeferredRenderer.CopyDataFrom(this);
        }

        /// <summary>
        /// Internal forward renderer.
        /// </summary>
        public View3DForwardRenderer ForwardRenderer = new View3DForwardRenderer();

        /// <summary>
        /// Internal deferred renderer.
        /// </summary>
        public View3DDeferredRenderer DeferredRenderer = new View3DDeferredRenderer();

        /// <summary>
        /// Sets the viewport to match the render target, but does not track the change.
        /// </summary>
        public void SetViewPortUntracked()
        {
            GL.Viewport(0, 0, Config.Width, Config.Height);
        }

        /// <summary>
        /// Sets the view port to the correct values, and tracks them.
        /// </summary>
        public void SetViewportTracked()
        {
            Viewport(0, 0, Config.Width, Config.Height);
        }

        /// <summary>
        /// Sets and tracks the draw buffer mode.
        /// </summary>
        /// <param name="dbm">The mode to set.</param>
        public void DrawBuffer(DrawBufferMode dbm)
        {
            State.BufferMode = dbm;
            GL.DrawBuffer(dbm);
        }

        /// <summary>
        /// Causes a simple 'flash bang' effect.
        /// </summary>
        /// <param name="duration_add">The duration to add to the effect.</param>
        public void Flashbang(double duration_add)
        {
            if (State.Flash_DurationLeft == 0.0)
            {
                Internal.Flash_Texture = GenerationHelper.NextFrameToTexture();
            }
            State.Flash_DurationLeft += duration_add;
        }

        /// <summary>
        /// Renders the entire 3D viewport.
        /// </summary>
        public void Render()
        {
            int pfbo = Internal.CurrentFBO;
            try
            {
                if (Internal.FBO_NextFrame != 0)
                {
                    if (pfbo == 0)
                    {
                        Internal.CurrentFBO = Internal.FBO_NextFrame;
                    }
                }
                if (!Config.ForwardMode)
                {
                    DeferredRenderer.ReadDynamicExposure();
                    GraphicsUtil.CheckError("Render - HDR");
                }
                RenderPass_Setup();
                GraphicsUtil.CheckError("Render - Setup");
                if (Config.ForwardMode)
                {
                    if (Engine.Forward_Shadows)
                    {
                        DeferredRenderer.RenderPass_Shadows();
                        GraphicsUtil.CheckError("Render - Shadow (Fast)");
                    }
                    ForwardRenderer.RenderPass_FAST();
                    GraphicsUtil.CheckError("Render - Fast");
                    GenerationHelper.EndNF(pfbo);
                    return;
                }
                Statistics.Total.Start();
                if (Engine.Deferred_Shadows)
                {
                    DeferredRenderer.RenderPass_Shadows();
                    GraphicsUtil.CheckError("Render - Shadow");
                }
                DeferredRenderer.RenderPass_GBuffer();
                GraphicsUtil.CheckError("Render - Buffer");
                DeferredRenderer.RenderPass_Lights();
                GraphicsUtil.CheckError("Render - Lights");
                State.PreviousForward = Config.CameraPos + State.CameraForward;
                Statistics.Total.Stop();
                GenerationHelper.EndNF(pfbo);
            }
            catch (Exception ex)
            {
                CommonUtilities.CheckException(ex);
                SysConsole.Output("Rendering (3D)", ex);
                Internal.CurrentFBO = pfbo;
            }
        }

        /// <summary>
        /// Gets a Matrix4(f) from a Matrix4d. For use with SetMatrix.
        /// WARNING: This is NOT a flat converter, it CHANGES data for SetMatrix.
        /// </summary>
        /// <param name="mat">The double matrix.</param>
        /// <returns>The float matrix.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Matrix4 GetMat4f(Matrix4d mat)
        {
            Matrix4d temp = mat * State.OffsetWorld;
            Matrix4 mat4f = new Matrix4((float)temp.M11, (float)temp.M12, (float)temp.M13, (float)temp.M14, (float)temp.M21, (float)temp.M22, (float)temp.M23, (float)temp.M24,
                (float)temp.M31, (float)temp.M32, (float)temp.M33, (float)temp.M34, (float)temp.M41, (float)temp.M42, (float)temp.M43, (float)temp.M44);
            return mat4f;
        }

        /// <summary>
        /// Sets the view matrix. Will yield a modified matrix, according to world offset. Generally used for the WORLD matrix and not others.
        /// </summary>
        /// <param name="mat_loc">The matrix target location.</param>
        /// <param name="mat">The matrix itself.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetMatrix(int mat_loc, Matrix4d mat)
        {
            Matrix4 mat4f = GetMat4f(mat);
            GL.UniformMatrix4(mat_loc, false, ref mat4f);
        }

        /// <summary>
        /// Sets and tracks the viewport for trackable changes.
        /// </summary>
        /// <param name="x">X coordinate.</param>
        /// <param name="y">Y coordinate.</param>
        /// <param name="w">Width.</param>
        /// <param name="h">Height.</param>
        public void Viewport(int x, int y, int w, int h)
        {
            State.ViewportX = x;
            State.ViewportY = y;
            State.ViewportWidth = w;
            State.ViewportHeight = h;
            GL.Viewport(x, y, w, h);
        }

        /// <summary>
        /// Calculates the tranformation of the forward vector.
        /// </summary>
        /// <returns>Transformed forward vector.</returns>
        public Location CalcForward()
        {
            return Config.CameraModifier().Transform(Config.ForwardVec);
        }

        /// <summary>
        /// Fixes the viewport back to what was last set by <see cref="Viewport(int, int, int, int)"/>.
        /// </summary>
        public void FixVP()
        {
            GL.Viewport(State.ViewportX, State.ViewportY, State.ViewportWidth, State.ViewportHeight);
        }

        /// <summary>
        /// Binds a framebuffer and tracks it. Generally used with DrawFB's.
        /// </summary>
        /// <param name="fbt">The target (usually draw).</param>
        /// <param name="fbo">The object.</param>
        public void BindFramebuffer(FramebufferTarget fbt, int fbo)
        {
            GL.BindFramebuffer(fbt, fbo);
            Internal.LastBoundFramebuffer = fbo;
        }

        /// <summary>
        /// Sets the blend to a standard ordered blend.
        /// </summary>
        public static void StandardBlend()
        {
            GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
        }

        /// <summary>
        /// Sets the blend to additive (transparent) mode.
        /// </summary>
        public static void TranspBlend()
        {
            GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.One);
        }

        /// <summary>
        /// Set up the rendering engine.
        /// </summary>
        public void RenderPass_Setup()
        {
            FGECore.MathHelpers.Quaternion cameraRotationModifier = Config.CameraModifier();
            Location cameraUpVector = cameraRotationModifier.Transform(Config.CameraUp());
            Location cameraForwardVector = cameraRotationModifier.Transform(Config.ForwardVec);
            State.CameraForward = cameraForwardVector;
            BindFramebuffer(FramebufferTarget.Framebuffer, Internal.CurrentFBO);
            DrawBuffer(Internal.CurrentFBO == 0 ? DrawBufferMode.Back : DrawBufferMode.ColorAttachment0);
            StandardBlend();
            GL.Enable(EnableCap.DepthTest);
            State.RenderTextures = true;
            GL.ClearBuffer(ClearBuffer.Color, 0, Config.ClearColor);
            GL.ClearBuffer(ClearBuffer.Depth, 0, View3DInternalData.DEPTH_CLEAR);
            State.CameraBasePos = Config.CameraPos;
            State.CameraAdjust = -cameraForwardVector.CrossProduct(cameraUpVector) * 0.25;
            if (Engine.Client.VR != null)
            {
                //cameraAdjust = -cameraAdjust;
                State.CameraAdjust = Location.Zero;
            }
            State.RenderRelative = (Config.CameraPos / View3DInternalData.CAMERA_POSITION_JUMP_SCALE).Round() * View3DInternalData.CAMERA_POSITION_JUMP_SCALE;
            SetViewportTracked();
            Config.CameraTarget = Config.CameraPos + cameraForwardVector;
            State.OffsetWorld = Matrix4d.CreateTranslation((-State.RenderRelative).ToOpenTK3D());
            Matrix4d outviewD;
            State.CameraRelativePosition = (State.CameraBasePos - State.RenderRelative).ToOpenTK();
            if (Engine.Client.VR != null)
            {
                Matrix4 proj = Engine.Client.VR.GetProjection(true, Engine.ZNear, Engine.ZFar());
                Matrix4 view = Engine.Client.VR.Eye(true, State.CameraRelativePosition); // TODO: account for player height?
                State.PrimaryMatrix = view * proj;
                Matrix4 proj2 = Engine.Client.VR.GetProjection(false, Engine.ZNear, Engine.ZFar());
                Matrix4 view2 = Engine.Client.VR.Eye(false, State.CameraRelativePosition); // TODO: account for player height?
                State.PrimaryMatrix_OffsetFor3D = view2 * proj2;
                State.PrimaryMatrixd = Matrix4d.CreateTranslation((-Config.CameraPos).ToOpenTK3D()) * view.ConvertToD() * proj.ConvertToD();
                State.PrimaryMatrix_OffsetFor3Dd = Matrix4d.CreateTranslation((-Config.CameraPos).ToOpenTK3D()) * view2.ConvertToD() * proj2.ConvertToD();
                Matrix4 projo = Engine.Client.VR.GetProjection(true, 60f, Engine.ZFarOut());
                State.OutViewMatrix = view * projo;
                outviewD = Matrix4d.CreateTranslation((-Config.CameraPos).ToOpenTK3D()) * view.ConvertToD() * projo.ConvertToD();
                Matrix4 projo2 = Engine.Client.VR.GetProjection(false, 60f, Engine.ZFarOut());
                State.OutView_OffsetVR = view2 * projo2;
                State.OutView_OffsetVRd = Matrix4d.CreateTranslation((-Config.CameraPos).ToOpenTK3D()) * view2.ConvertToD() * projo2.ConvertToD();
                // TODO: Transform VR by cameraRotationModifier?
            }
            else
            {
                Matrix4 proj = Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(Engine.FOV), (float)Config.Width / (float)Config.Height, Engine.ZNear, Engine.ZFar()); // TODO: View3D-level vars?
                Vector3 bx = State.CameraRelativePosition + (Engine.Render3DView ? State.CameraAdjust.ToOpenTK() : Vector3.Zero);
                Matrix4 view = Matrix4.LookAt(bx, bx + cameraForwardVector.ToOpenTK(), cameraUpVector.ToOpenTK());
                State.PrimaryMatrix = view * proj;
                if (Engine.Render3DView)
                {
                    Matrix4 view2 = Matrix4.LookAt((-State.CameraAdjust).ToOpenTK(), (-State.CameraAdjust + cameraForwardVector).ToOpenTK(), cameraUpVector.ToOpenTK());
                    State.PrimaryMatrix_OffsetFor3D = view2 * proj;
                }
                Matrix4 proj_out = Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(Engine.FOV), (float)Config.Width / (float)Config.Height, 60f, Engine.ZFarOut()); // TODO: View3D-level vars?
                State.OutViewMatrix = view * proj_out;
                Matrix4d projd = Matrix4d.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(Engine.FOV),
                    (float)Config.Width / (float)Config.Height, Engine.ZNear, Engine.ZFar()); // TODO: View3D-level vars?
                Location bxd = Engine.Render3DView ? (Config.CameraPos + State.CameraAdjust) : Config.CameraPos;
                Matrix4d viewd = Matrix4d.LookAt(bxd.ToOpenTK3D(), (bxd + cameraForwardVector).ToOpenTK3D(), cameraUpVector.ToOpenTK3D());
                State.PrimaryMatrixd = viewd * projd;
                Matrix4d proj_outd = Matrix4d.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(Engine.FOV), (float)Config.Width / (float)Config.Height, 60f, Engine.ZFarOut()); // TODO: View3D-level vars?
                outviewD = viewd * proj_outd;
                State.PrimaryMatrix_OffsetFor3Dd = Matrix4d.Identity;
                if (Engine.Render3DView)
                {
                    Matrix4d view2d = Matrix4d.LookAt((Config.CameraPos - State.CameraAdjust).ToOpenTK3D(), (Config.CameraPos - State.CameraAdjust + cameraForwardVector).ToOpenTK3D(), cameraUpVector.ToOpenTK3D());
                    State.PrimaryMatrix_OffsetFor3Dd = view2d * projd;
                }
            }
            State.LongFrustum = new Frustum(outviewD.ConvertD());
            State.CameraFrustum = new Frustum(State.PrimaryMatrixd.ConvertD());
            State.SecondEyeFrustum = new Frustum(State.PrimaryMatrix_OffsetFor3Dd.ConvertD());
            State.CurrentFrustum = State.CameraFrustum;
            GraphicsUtil.CheckError("AfterSetup");
        }
    }
}
