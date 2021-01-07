//
// This file is part of the Frenetic Game Engine, created by Frenetic LLC.
// This code is Copyright (C) Frenetic LLC under the terms of a strict license.
// See README.md or LICENSE.txt in the FreneticGameEngine source root for the contents of the license.
// If neither of these are available, assume that neither you nor anyone other than the copyright holder
// hold any right or permission to use view software until such time as the official license is identified.
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;
using OpenTK.Mathematics;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;
using FGECore.MathHelpers;
using FGEGraphics.GraphicsHelpers;
using FGEGraphics.LightingSystem;

namespace FGEGraphics.ClientSystem.ViewRenderSystem
{
    /// <summary>
    /// Handles deferred rendering logic for <see cref="View3D"/>.
    /// </summary>
    public class View3DDeferredRenderer : View3DCoreDataSet
    {
        /// <summary>
        /// Patches to apply (if any).
        /// </summary>
        public View3DPatchesDeferred Patches = new View3DPatchesDeferred();

        /// <summary>
        /// Calculate shadow maps for the later (lighting) render passes.
        /// </summary>
        public void RenderPass_Shadows()
        {
            if (Config.ShouldRedrawShadows() && Config.ShadowingAllowed)
            {
                bool redraw = Config.GetAndResetShouldMajorUpdates();
                Statistics.Shadows_Timer.Reset();
                Statistics.Shadows_Timer.Start();
                Shaders.Deferred.ShadowPass_Basic = Shaders.Deferred.ShadowPass_Basic.Bind();
                Patches.PreShadowsPatch?.Invoke();
                State.RenderingShadows = true;
                State.ShadowsOnly = true;
                State.LightCount = 0;
                Location campos = Config.CameraPos;
                int n = 0;
                Frustum tcf = State.CurrentFrustum;
                int sp = Config.ShadowTexSize();
                int ssp = sp / 2;
                foreach (LightObject light in Config.Lights)
                {
                    if (light is SkyLight || State.CameraFrustum == null || State.CameraFrustum.ContainsSphere(light.EyePos, light.MaxDistance))
                    {
                        if (light is SkyLight || light.EyePos.DistanceSquared(campos) <
                            Config.LightsMaxDistance * Config.LightsMaxDistance + light.MaxDistance * light.MaxDistance * 6)
                        {
                            State.LightCount++;
                            if (light is PointLight pl && !pl.CastShadows)
                            {
                                n++;
                                if (n >= View3DInternalData.LIGHTS_MAX)
                                {
                                    goto complete;
                                }
                            }
                            else
                            {
                                foreach (Light subLight in light.InternalLights)
                                {
                                    if (subLight.Color.LengthSquared <= 0.01)
                                    {
                                        continue;
                                    }
                                    if (subLight is LightOrtho)
                                    {
                                        State.CurrentFrustum = null;
                                    }
                                    else
                                    {
                                        State.CurrentFrustum = new Frustum(subLight.GetMatrix(View).ConvertD());
                                    }
                                    int lTID = n;
                                    int widX = sp;
                                    int widY = sp;
                                    int ltX = 0;
                                    int ltY = 0;
                                    if (n >= 10)
                                    {
                                        lTID = (n - 10) / 4;
                                        int ltCO = (n - 10) % 4;
                                        ltY = ltCO / 2;
                                        ltX = ltCO % 2;
                                        View.BindFramebuffer(FramebufferTarget.Framebuffer, Internal.FBO_Shadow[lTID]);
                                        GL.Viewport(ssp * ltX, ssp * ltY, ssp, ssp);
                                        widX = ssp;
                                        widY = ssp;
                                    }
                                    else
                                    {
                                        View.BindFramebuffer(FramebufferTarget.Framebuffer, Internal.FBO_Shadow[lTID]);
                                        GL.Viewport(0, 0, sp, sp);
                                    }
                                    GraphicsUtil.CheckError("Pre-Prerender - Shadows");
                                    Config.CameraPos = subLight.EyePosition.ToLocation() - campos;
                                    Patches.ShadowLightPatch?.Invoke(light, subLight);
                                    Shaders.Deferred.ShadowPass_Particles = Shaders.Deferred.ShadowPass_Particles.Bind();
                                    View.SetMatrix(2, Matrix4d.Identity);
                                    GL.Uniform1(5, (subLight is LightOrtho) ? 1.0f : 0.0f);
                                    GL.Uniform1(4, subLight.TransparentShadows ? 1.0f : 0.0f);
                                    GL.Uniform3(14, State.CameraRelativePosition);
                                    subLight.SetProj(View);
                                    Shaders.Deferred.ShadowPass_NoBones = Shaders.Deferred.ShadowPass_NoBones.Bind();
                                    View.SetMatrix(2, Matrix4d.Identity);
                                    GraphicsUtil.CheckError("Pre-Prerender2.5 - Shadows");
                                    GL.Uniform1(5, (subLight is LightOrtho) ? 1.0f : 0.0f);
                                    GL.Uniform1(4, subLight.TransparentShadows ? 1.0f : 0.0f);
                                    State.TranspShadows = subLight.TransparentShadows;
                                    subLight.SetProj(View);
                                    Shaders.Deferred.ShadowPass_Basic = Shaders.Deferred.ShadowPass_Basic.Bind();
                                    View.SetMatrix(2, Matrix4d.Identity);
                                    GraphicsUtil.CheckError("Pre-Prerender3 - Shadows");
                                    GL.Uniform1(5, (subLight is LightOrtho) ? 1.0f : 0.0f);
                                    GL.Uniform1(4, subLight.TransparentShadows ? 1.0f : 0.0f);
                                    State.TranspShadows = subLight.TransparentShadows;
                                    subLight.SetProj(View);
                                    GraphicsUtil.CheckError("Pre-Prerender4 - Shadows");
                                    View.DrawBuffer(DrawBufferMode.ColorAttachment0);
                                    GL.BlendFunc(BlendingFactor.One, BlendingFactor.Zero);
                                    if (light is SkyLight sky)
                                    {
                                        if (redraw || subLight.NeedsUpdate)
                                        {
                                            subLight.NeedsUpdate = false;
                                            View.BindFramebuffer(FramebufferTarget.Framebuffer, sky.FBO);
                                            View.DrawBuffer(DrawBufferMode.ColorAttachment0);
                                            GL.ClearBuffer(ClearBuffer.Color, 0, new float[] { 1f });
                                            GL.ClearBuffer(ClearBuffer.Depth, 0, new float[] { 1f });
                                            State.FBOid = FBOID.STATIC_SHADOWS;
                                            GraphicsUtil.CheckError("Prerender - Shadows");
                                            Config.Render3D(View);
                                            View.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
                                        }
                                        View.BindFramebuffer(FramebufferTarget.Framebuffer, Internal.FBO_Shadow[lTID]);
                                        GL.BindFramebuffer(FramebufferTarget.ReadFramebuffer, sky.FBO);
                                        GL.BlitFramebuffer(0, 0, sky.TexWidth, sky.TexWidth, ltX, ltY, widX, widY, ClearBufferMask.DepthBufferBit, BlitFramebufferFilter.Nearest);
                                        GL.BindFramebuffer(FramebufferTarget.ReadFramebuffer, 0);
                                        if (Engine.EnableDynamicShadows)
                                        {
                                            //GL.ClearBuffer(ClearBuffer.Color, 0, new float[] { 1f });
                                            State.FBOid = FBOID.DYNAMIC_SHADOWS;
                                            Config.Render3D(View);
                                        }
                                    }
                                    else if (!subLight.CastShadows)
                                    {
                                        View.BindFramebuffer(FramebufferTarget.Framebuffer, Internal.FBO_Shadow[lTID]);
                                        GL.ClearBuffer(ClearBuffer.Depth, 0, new float[] { 1f });
                                        GL.ClearBuffer(ClearBuffer.Color, 0, new float[] { 1f });
                                    }
                                    else
                                    {
                                        View.BindFramebuffer(FramebufferTarget.Framebuffer, Internal.FBO_Shadow[lTID]);
                                        GL.ClearBuffer(ClearBuffer.Depth, 0, new float[] { 1f });
                                        GL.ClearBuffer(ClearBuffer.Color, 0, new float[] { 1f });
                                        State.FBOid = FBOID.SHADOWS;
                                        Config.Render3D(View);
                                    }
                                    State.FBOid = FBOID.NONE;
                                    n++;
                                    GraphicsUtil.CheckError("Postrender - Shadows");
                                    if (n >= View3DInternalData.LIGHTS_MAX)
                                    {
                                        goto complete;
                                    }
                                }
                            }
                        }
                    }
                }
            complete:
                View.SetViewportTracked();
                State.CurrentFrustum = tcf;
                View.BindFramebuffer(FramebufferTarget.Framebuffer, Internal.CurrentFBO);
                View.DrawBuffer(Internal.CurrentFBO == 0 ? DrawBufferMode.Back : DrawBufferMode.ColorAttachment0);
                Config.CameraPos = campos;
                State.RenderingShadows = false;
                State.ShadowsOnly = false;
                Statistics.Shadows_Timer.Stop();
                Statistics.ShadowTime = (double)Statistics.Shadows_Timer.ElapsedMilliseconds / 1000f;
                if (Statistics.ShadowTime > Statistics.ShadowSpikeTime)
                {
                    Statistics.ShadowSpikeTime = Statistics.ShadowTime;
                }
                View3D.StandardBlend();
                GraphicsUtil.CheckError("AfterShadows");
            }
        }

        /// <summary>
        /// Generate the G-Buffer ("FBO") for lighting and final passes.
        /// </summary>
        public void RenderPass_GBuffer()
        {
            Statistics.GBuffer_Timer.Reset();
            Statistics.GBuffer_Timer.Start();
            View.SetViewportTracked();
            Shaders.Deferred.GBuffer_Decals = Shaders.Deferred.GBuffer_Decals.Bind();
            GL.UniformMatrix4(1, false, ref State.PrimaryMatrix);
            GL.UniformMatrix4(2, false, ref View3DInternalData.IdentityMatrix);
            GL.Uniform4(4, new Vector4(Config.Width, Config.Height, Engine.ZNear, Engine.ZFar()));
            Patches.PreFBOPatch?.Invoke();
            Shaders.Deferred.GBuffer_SkyBox = Shaders.Deferred.GBuffer_SkyBox.Bind();
            GL.UniformMatrix4(1, false, ref State.PrimaryMatrix);
            GL.UniformMatrix4(2, false, ref View3DInternalData.IdentityMatrix);
            GL.Uniform1(6, (float)Engine.GlobalTickTime);
            GL.Uniform4(18, new Vector4(Config.FogCol.ToOpenTK(), Config.FogAlpha));
            GraphicsUtil.CheckError("Render - GBuffer - Uniforms - 2");
            Shaders.Deferred.GBufferSolid = Shaders.Deferred.GBufferSolid.Bind();
            GL.UniformMatrix4(1, false, ref State.PrimaryMatrix);
            GL.UniformMatrix4(2, false, ref View3DInternalData.IdentityMatrix);
            GL.Uniform1(6, (float)Engine.GlobalTickTime);
            GraphicsUtil.CheckError("Render - GBuffer - 0");
            State.FBOid = FBOID.MAIN;
            State.RenderingShadows = false;
            State.CurrentFrustum = State.CameraFrustum;
            GL.ActiveTexture(TextureUnit.Texture0);
            State.DeferredTarget.Bind(View);
            DeferredRenderTarget.Clear();
            State.RenderLights = true;
            State.RenderSpecular = true;
            Engine.Rendering.SetColor(Color4.White, View);
            View3D.StandardBlend();
            GraphicsUtil.CheckError("Render - GBuffer - 1");
            if (Engine.Render3DView || Engine.Client.VR != null)
            {
                View.Viewport(Config.Width / 2, 0, Config.Width / 2, Config.Height);
                Config.Render3D(View);
                State.CurrentFrustum = State.SecondEyeFrustum;
                View.Viewport(0, 0, Config.Width / 2, Config.Height);
                Config.CameraPos = State.CameraBasePos - State.CameraAdjust;
                Patches.VRFBOPatch?.Invoke();
                Shaders.Deferred.GBuffer_SkyBox = Shaders.Deferred.GBuffer_SkyBox.Bind();
                GL.UniformMatrix4(1, false, ref State.PrimaryMatrix_OffsetFor3D);
                Shaders.Deferred.GBufferSolid = Shaders.Deferred.GBufferSolid.Bind();
                GL.UniformMatrix4(1, false, ref State.PrimaryMatrix_OffsetFor3D);
                Matrix4 orig = State.PrimaryMatrix;
                State.PrimaryMatrix = State.PrimaryMatrix_OffsetFor3D;
                Matrix4 orig_out = State.OutViewMatrix;
                State.OutViewMatrix = State.OutView_OffsetVR;
                State.IsSecondEye = true;
                Config.Render3D(View);
                State.IsSecondEye = false;
                State.OutViewMatrix = orig_out;
                State.PrimaryMatrix = orig;
                View.Viewport(0, 0, Config.Width, Config.Height);
                Config.CameraPos = State.CameraBasePos + State.CameraAdjust;
                State.CurrentFrustum = State.CameraFrustum;
            }
            else
            {
                Config.Render3D(View);
            }
            GraphicsUtil.CheckError("AfterFBO");
            RenderPass_Decals();
            RenderPass_RefractionBuffer();
            Statistics.GBuffer_Timer.Stop();
            Statistics.FBOTime = (double)Statistics.GBuffer_Timer.ElapsedMilliseconds / 1000f;
            if (Statistics.FBOTime > Statistics.FBOSpikeTime)
            {
                Statistics.FBOSpikeTime = Statistics.FBOTime;
            }
            GraphicsUtil.CheckError("Render - GBuffer - Final");
        }

        /// <summary>
        /// Adds decal data to the G-Buffer ("FBO").
        /// </summary>
        public void RenderPass_Decals()
        {
            Shaders.Deferred.GBuffer_Decals = Shaders.Deferred.GBuffer_Decals.Bind();
            State.DeferredTarget.Unbind(View);
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, Internal.FBO_Decal);
            GL.DrawBuffer(DrawBufferMode.ColorAttachment0);
            GL.BindFramebuffer(FramebufferTarget.ReadFramebuffer, State.DeferredTarget.FBO);
            GL.BlitFramebuffer(0, 0, Config.Width, Config.Height, 0, 0, Config.Width, Config.Height, ClearBufferMask.DepthBufferBit, BlitFramebufferFilter.Nearest);
            GL.BindFramebuffer(FramebufferTarget.ReadFramebuffer, 0);
            State.DeferredTarget.Bind(View);
            GL.ActiveTexture(TextureUnit.Texture4);
            GL.BindTexture(TextureTarget.Texture2D, Internal.FBO_Decal_Depth);
            GL.ActiveTexture(TextureUnit.Texture0);
            GL.DepthMask(false);
            GraphicsUtil.CheckError("Render - Decals - 0");
            if (Engine.Render3DView || Engine.Client.VR != null)
            {
                View.Viewport(Config.Width / 2, 0, Config.Width / 2, Config.Height);
                Config.DecalRender?.Invoke(View);
                State.CurrentFrustum = State.SecondEyeFrustum;
                View.Viewport(0, 0, Config.Width / 2, Config.Height);
                Config.CameraPos = State.CameraBasePos - State.CameraAdjust;
                GL.UniformMatrix4(1, false, ref State.PrimaryMatrix_OffsetFor3D);
                Matrix4 orig = State.PrimaryMatrix;
                State.PrimaryMatrix = State.PrimaryMatrix_OffsetFor3D;
                Matrix4 orig_out = State.OutViewMatrix;
                State.OutViewMatrix = State.OutView_OffsetVR;
                State.IsSecondEye = true;
                Config.DecalRender?.Invoke(View);
                State.IsSecondEye = false;
                State.OutViewMatrix = orig_out;
                State.PrimaryMatrix = orig;
                View.Viewport(0, 0, Config.Width, Config.Height);
                Config.CameraPos = State.CameraBasePos + State.CameraAdjust;
                State.CurrentFrustum = State.CameraFrustum;
            }
            else
            {
                Config.DecalRender?.Invoke(View);
            }
            GraphicsUtil.CheckError("Render - Decals - Final");
            GL.DepthMask(true);
            GL.ActiveTexture(TextureUnit.Texture4);
            GL.BindTexture(TextureTarget.Texture2D, 0);
            GL.ActiveTexture(TextureUnit.Texture0);
        }

        /// <summary>
        /// Adds refraction data to the G-Buffer ("FBO").
        /// </summary>
        public void RenderPass_RefractionBuffer()
        {
            State.FBOid = FBOID.REFRACT;
            Patches.RefractionPatch?.Invoke();
            Shaders.Deferred.GBuffer_Refraction = Shaders.Deferred.GBuffer_Refraction.Bind();
            GL.Uniform1(6, (float)Engine.GlobalTickTime);
            GL.UniformMatrix4(1, false, ref State.PrimaryMatrix);
            GL.UniformMatrix4(2, false, ref View3DInternalData.IdentityMatrix);
            GL.DepthMask(false);
            GraphicsUtil.CheckError("Render - Refract - 0");
            if (Engine.Render3DView || Engine.Client.VR != null)
            {
                View.Viewport(Config.Width / 2, 0, Config.Width / 2, Config.Height);
                Config.Render3D(View);
                State.CurrentFrustum = State.SecondEyeFrustum;
                View.Viewport(0, 0, Config.Width / 2, Config.Height);
                Config.CameraPos = State.CameraBasePos - State.CameraAdjust;
                Patches.VRRefractionPatch?.Invoke();
                Shaders.Deferred.GBuffer_Refraction = Shaders.Deferred.GBuffer_Refraction.Bind();
                GL.UniformMatrix4(1, false, ref State.PrimaryMatrix_OffsetFor3D);
                Matrix4 orig = State.PrimaryMatrix;
                State.PrimaryMatrix = State.PrimaryMatrix_OffsetFor3D;
                Matrix4 orig_out = State.OutViewMatrix;
                State.OutViewMatrix = State.OutView_OffsetVR;
                State.IsSecondEye = true;
                Config.Render3D(View);
                State.IsSecondEye = false;
                State.OutViewMatrix = orig_out;
                State.PrimaryMatrix = orig;
                View.Viewport(0, 0, Config.Width, Config.Height);
                Config.CameraPos = State.CameraBasePos + State.CameraAdjust;
                State.CurrentFrustum = State.CameraFrustum;
            }
            else
            {
                Config.Render3D(View);
            }
            GraphicsUtil.CheckError("AfterRefract");
            GL.DepthMask(true);
            State.RenderLights = false;
            State.RenderSpecular = false;
            State.DeferredTarget.Unbind(View);
            State.FBOid = FBOID.NONE;
        }

        /// <summary>
        /// Light source addition render passes.
        /// </summary>
        public void RenderPass_Lights()
        {
            Statistics.Lights_Timer.Restart();
            View.BindFramebuffer(FramebufferTarget.Framebuffer, Internal.FBO_Screen_Main);
            View.DrawBuffer(DrawBufferMode.ColorAttachment0);
            GL.ClearBuffer(ClearBuffer.Color, 0, new float[] { 0.0f, 0.0f, 0.0f, Config.RenderClearAlpha });
            if (Engine.Deferred_Shadows)
            {
                if (Engine.Deferred_SSAO)
                {
                    Shaders.Deferred.ShadowAdderPass_SSAO = Shaders.Deferred.ShadowAdderPass_SSAO.Bind();
                }
                else
                {
                    Shaders.Deferred.ShadowAdderPass = Shaders.Deferred.ShadowAdderPass.Bind();
                }
                GL.Uniform1(3, Config.ShadowBlur);
            }
            else
            {
                if (Engine.Deferred_SSAO)
                {
                    Shaders.Deferred.LightAdderPass_SSAO = Shaders.Deferred.LightAdderPass_SSAO.Bind();
                }
                else
                {
                    Shaders.Deferred.LightAdderPass = Shaders.Deferred.LightAdderPass.Bind();
                }
            }
            GL.ActiveTexture(TextureUnit.Texture1);
            GL.BindTexture(TextureTarget.Texture2D, State.DeferredTarget.PositionTexture);
            GL.ActiveTexture(TextureUnit.Texture2);
            GL.BindTexture(TextureTarget.Texture2D, State.DeferredTarget.NormalsTexture);
            GL.ActiveTexture(TextureUnit.Texture3);
            GL.BindTexture(TextureTarget.Texture2D, State.DeferredTarget.DepthTexture);
            GL.ActiveTexture(TextureUnit.Texture4);
            GL.BindTexture(TextureTarget.Texture2D, 0);
            GL.BindTexture(TextureTarget.Texture2DArray, Internal.FBO_Shadow_DepthTexture);
            GL.ActiveTexture(TextureUnit.Texture5);
            GL.BindTexture(TextureTarget.Texture2D, State.DeferredTarget.RenderhintTexture);
            GL.ActiveTexture(TextureUnit.Texture6);
            GL.BindTexture(TextureTarget.Texture2D, State.DeferredTarget.DiffuseTexture);
            GL.Uniform3(4, Config.Ambient.ToOpenTK());
            GL.UniformMatrix4(1, false, ref View3DInternalData.SimpleOrthoMatrix);
            GL.UniformMatrix4(2, false, ref View3DInternalData.IdentityMatrix);
            GL.Disable(EnableCap.CullFace);
            GL.Disable(EnableCap.DepthTest);
            View3D.TranspBlend();
            if (Engine.Deferred_Lights)
            {
                float[] light_dat = new float[View3DInternalData.LIGHTS_MAX * 16];
                float[] shadowmat_dat = new float[View3DInternalData.LIGHTS_MAX * 16];
                int c = 0;
                // TODO: An ambient light source?
                foreach (LightObject light in Config.Lights)
                {
                    if (light is SkyLight || State.CameraFrustum == null || State.CameraFrustum.ContainsSphere(light.EyePos, light.MaxDistance))
                    {
                        double d1 = (light.EyePos - Config.CameraPos).LengthSquared();
                        double d2 = Config.LightMaxDistance * Config.LightMaxDistance + light.MaxDistance * light.MaxDistance;
                        double maxrangemult = 0;
                        if (d1 < d2 * 4 || light is SkyLight)
                        {
                            maxrangemult = 1;
                        }
                        else if (d1 < d2 * 6)
                        {
                            maxrangemult = 1 - ((d1 - (d2 * 4)) / ((d2 * 6) - (d2 * 4)));
                        }
                        if (maxrangemult > 0)
                        {
                            if (light is PointLight pl && !pl.CastShadows)
                            {
                                Matrix4 smat = Matrix4.Identity;
                                Vector3d eyep = pl.EyePos.ToOpenTK3D() - Config.CameraPos.ToOpenTK3D();
                                Vector3 col = light.InternalLights[0].Color * (float)maxrangemult;
                                Matrix4 light_data = new Matrix4(
                                    (float)eyep.X, (float)eyep.Y, (float)eyep.Z, // light_pos
                                    0.7f, // diffuse_albedo
                                    0.7f, // specular_albedo
                                   0.0f, // should_sqrt
                                    col.X, col.Y, col.Z, // light_color
                                    (light.InternalLights[0].MaxRange <= 0 ? View3DInternalData.LIGHT_MAXIUM_RADIUS : light.InternalLights[0].MaxRange), // light_radius
                                    0f, 0f, 0f, // eye_pos
                                    2.0f, // light_type
                                    1f / Config.ShadowTexSize(), // tex_size
                                    0.0f // Unused.
                                    );
                                for (int mx = 0; mx < 4; mx++)
                                {
                                    for (int my = 0; my < 4; my++)
                                    {
                                        shadowmat_dat[c * 16 + mx * 4 + my] = smat[mx, my];
                                        light_dat[c * 16 + mx * 4 + my] = light_data[mx, my];
                                    }
                                }
                                c++;
                                if (c >= View3DInternalData.LIGHTS_MAX)
                                {
                                    goto lights_apply;
                                }
                            }
                            else
                            {
                                foreach (Light subLight in light.InternalLights)
                                {
                                    if (subLight.Color.LengthSquared <= 0.01)
                                    {
                                        continue;
                                    }
                                    Matrix4 smat = subLight.GetMatrix(View);
                                    Vector3d eyep = light is SkyLight se ? -se.Direction.ToOpenTK3D() : subLight.EyePosition - Config.CameraPos.ToOpenTK3D();
                                    Vector3 col = subLight.Color * (float)maxrangemult;
                                    Matrix4 light_data = new Matrix4(
                                        (float)eyep.X, (float)eyep.Y, (float)eyep.Z, // light_pos
                                        0.7f, // diffuse_albedo
                                        0.7f, // specular_albedo
                                        subLight is LightOrtho ? 1.0f : 0.0f, // should_sqrt
                                        col.X, col.Y, col.Z, // light_color
                                        subLight is LightOrtho ? View3DInternalData.LIGHT_MAXIUM_RADIUS : (light.InternalLights[0].MaxRange <= 0 ? View3DInternalData.LIGHT_MAXIUM_RADIUS : light.InternalLights[0].MaxRange), // light_radius
                                        0f, 0f, 0f, // eye_pos
                                        light is SpotLight ? 1.0f : 0.0f, // light_type
                                        1f / Config.ShadowTexSize(), // tex_size
                                        0.0f // Unused.
                                        );
                                    for (int mx = 0; mx < 4; mx++)
                                    {
                                        for (int my = 0; my < 4; my++)
                                        {
                                            shadowmat_dat[c * 16 + mx * 4 + my] = smat[mx, my];
                                            light_dat[c * 16 + mx * 4 + my] = light_data[mx, my];
                                        }
                                    }
                                    c++;
                                    if (c >= View3DInternalData.LIGHTS_MAX)
                                    {
                                        goto lights_apply;
                                    }
                                }
                            }
                        }
                    }
                }
            lights_apply:
                GL.ActiveTexture(TextureUnit.Texture4);
                GL.BindTexture(TextureTarget.Texture2DArray, Internal.FBO_Shadow_DepthTexture);
                GL.Uniform2(7, new Vector2(Engine.ZNear, Engine.ZFar()));
                GL.UniformMatrix4(8, false, ref State.PrimaryMatrix); // TODO: In 3D/VR, render both eyes separately here for SSAO accuracy?
                GL.Uniform1(9, (float)c);
                GL.UniformMatrix4(10, View3DInternalData.LIGHTS_MAX, false, shadowmat_dat);
                GL.UniformMatrix4(10 + View3DInternalData.LIGHTS_MAX, View3DInternalData.LIGHTS_MAX, false, light_dat);
                Engine.Rendering.RenderRectangle(-1, -1, 1, 1);
                GL.ActiveTexture(TextureUnit.Texture0);
                GL.BindTexture(TextureTarget.Texture2D, 0);
                View3D.StandardBlend();
                GraphicsUtil.CheckError("AfterLighting");
                RenderPass_HDR();
            }
            GraphicsUtil.CheckError("AfterAllLightCode");
            RenderPass_LightsToBase();
            int lightc = RenderPass_Transparents();
            RenderPass_Bloom(lightc);
            Statistics.Lights_Timer.Stop();
            Statistics.LightsTime = (double)Statistics.Lights_Timer.ElapsedMilliseconds / 1000f;
            if (Statistics.LightsTime > Statistics.LightsSpikeTime)
            {
                Statistics.LightsSpikeTime = Statistics.LightsTime;
            }
            GraphicsUtil.CheckError("AtEnd");
        }

        /// <summary>
        /// Calculates the brightness value for High Dynamic Range rendering.
        /// </summary>
        public void RenderPass_HDR()
        {
            if (Engine.Deferred_Lights && Engine.Deferred_DynamicExposure)
            {
                Shaders.Deferred.HDRPass.Bind();
                GL.ActiveTexture(TextureUnit.Texture0);
                GL.Disable(EnableCap.CullFace);
                GL.Disable(EnableCap.DepthTest);
                GL.BindTexture(TextureTarget.Texture2D, Internal.FBO_Screen_Texture);
                GL.BlendFunc(BlendingFactor.One, BlendingFactor.Zero);
                View.BindFramebuffer(FramebufferTarget.Framebuffer, Internal.FBO_DynamicExposure);
                View.DrawBuffer(DrawBufferMode.ColorAttachment0);
                GL.UniformMatrix4(1, false, ref View3DInternalData.SimpleOrthoMatrix);
                GL.UniformMatrix4(2, false, ref View3DInternalData.IdentityMatrix);
                GL.Uniform2(4, new Vector2(Config.Width, Config.Height));
                Engine.Rendering.RenderRectangle(-1, -1, 1, 1);
                View3D.StandardBlend();
                GraphicsUtil.CheckError("AfterHDRPass");
            }
        }

        /// <summary>
        /// Applies lights to the base buffer.
        /// </summary>
        public void RenderPass_LightsToBase()
        {
            View.BindFramebuffer(FramebufferTarget.Framebuffer, Internal.FBO_GodRay_Main);
            if (Engine.Deferred_Lights)
            {
                if (Engine.Deferred_Toonify)
                {
                    Shaders.Deferred.FinalPass_Lights_Toonify = Shaders.Deferred.FinalPass_Lights_Toonify.Bind();
                }
                else
                {
                    if (Engine.Deferred_MotionBlur)
                    {
                        Shaders.Deferred.FinalPass_Lights_MotionBlur = Shaders.Deferred.FinalPass_Lights_MotionBlur.Bind();
                    }
                    else
                    {
                        Shaders.Deferred.FinalPass_Lights = Shaders.Deferred.FinalPass_Lights.Bind();
                    }
                }
            }
            else
            {
                if (Engine.Deferred_Toonify)
                {
                    Shaders.Deferred.FinalPass_Toonify = Shaders.Deferred.FinalPass_Toonify.Bind();
                }
                else
                {
                    Shaders.Deferred.FinalPass_Basic = Shaders.Deferred.FinalPass_Basic.Bind();
                }
            }
            State.BufferDontTouch = true;
            GL.DrawBuffers(2, new DrawBuffersEnum[] { DrawBuffersEnum.ColorAttachment0, DrawBuffersEnum.ColorAttachment1 });
            GL.ClearBuffer(ClearBuffer.Color, 0, new float[] { 0f, 0f, 0f, 0f });
            GL.ClearBuffer(ClearBuffer.Color, 1, new float[] { 0f, 0f, 0f, 0f });
            GL.BlendFuncSeparate(1, BlendingFactorSrc.SrcColor, BlendingFactorDest.Zero, BlendingFactorSrc.SrcAlpha, BlendingFactorDest.Zero);
            GL.Uniform3(8, Config.DOF_Target.ToOpenTK());
            GL.Uniform1(9, Config.DOF_Factor);
            GL.Uniform1(10, State.CurrentExposure * Engine.Exposure);
            float fogDist = 1.0f / Engine.FogMaxDist();
            fogDist *= fogDist;
            Vector2 zfar_rel = new Vector2(Engine.ZNear, Engine.ZFar());
            GL.Uniform3(14, State.CameraRelativePosition);
            GL.Uniform1(16, fogDist);
            GL.Uniform2(17, ref zfar_rel);
            GL.Uniform4(18, new Vector4(Config.FogCol.ToOpenTK(), Config.FogAlpha));
            // TODO: If thick fog, blur the environment? Or some similar head-in-a-block effect!
            GL.Uniform1(19, Config.DesaturationAmount);
            GL.Uniform3(20, new Vector3(0, 0, 0));
            GL.Uniform3(21, Config.DesaturationColor);
            GL.UniformMatrix4(22, false, ref State.PrimaryMatrix);
            GL.Uniform1(24, (float)Config.Width);
            GL.Uniform1(25, (float)Config.Height);
            GL.Uniform1(26, (float)Engine.GlobalTickTime);
            Vector4 v = Vector4.TransformRow(new Vector4(State.PreviousForward.ToOpenTK(), 1f), State.PrimaryMatrix);
            Vector2 v2 = (v.Xy / v.W);
            Vector2 rel = (Internal.PreviousPFResult - v2) * 0.01f;
            if (float.IsNaN(rel.X) || float.IsInfinity(rel.X) || float.IsNaN(rel.Y) || float.IsInfinity(rel.Y))
            {
                rel = new Vector2(0f, 0f);
            }
            GL.Uniform2(27, ref rel);
            Internal.PreviousPFResult = v2;
            GL.Uniform1(28, Engine.Deferred_Grayscale ? 1f : 0f);
            GL.ActiveTexture(TextureUnit.Texture3);
            GL.BindTexture(TextureTarget.Texture2D, State.DeferredTarget.DepthTexture);
            GL.ActiveTexture(TextureUnit.Texture4);
            GL.BindTexture(TextureTarget.Texture2D, Internal.FBO_Screen_Texture);
            GL.ActiveTexture(TextureUnit.Texture6);
            GL.BindTexture(TextureTarget.Texture2D, State.DeferredTarget.Rh2Texture);
            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, State.DeferredTarget.DiffuseTexture);
            GL.UniformMatrix4(1, false, ref View3DInternalData.SimpleOrthoMatrix);
            GL.UniformMatrix4(2, false, ref View3DInternalData.IdentityMatrix);
            GraphicsUtil.CheckError("FirstRenderToBasePassPre");
            Engine.Rendering.RenderRectangle(-1, -1, 1, 1);
            GraphicsUtil.CheckError("FirstRenderToBasePassComplete");
            GL.ActiveTexture(TextureUnit.Texture6);
            GL.BindTexture(TextureTarget.Texture2D, 0);
            GL.ActiveTexture(TextureUnit.Texture5);
            GL.BindTexture(TextureTarget.Texture2D, 0);
            GL.ActiveTexture(TextureUnit.Texture4);
            GL.BindTexture(TextureTarget.Texture2D, 0);
            GraphicsUtil.CheckError("AmidTextures");
            GL.ActiveTexture(TextureUnit.Texture3);
            GL.BindTexture(TextureTarget.Texture2D, 0);
            GL.ActiveTexture(TextureUnit.Texture2);
            GL.BindTexture(TextureTarget.Texture2D, 0);
            GL.ActiveTexture(TextureUnit.Texture1);
            Engine.Textures.NormalDef.Bind();
            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, 0);
            GL.Enable(EnableCap.DepthTest);
            GraphicsUtil.CheckError("PreBlendFunc");
            //GL.BlendFunc(1, BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);
            GraphicsUtil.CheckError("PreAFRFBO");
            View.BindFramebuffer(FramebufferTarget.Framebuffer, Internal.CurrentFBO);
            View.DrawBuffer(Internal.CurrentFBO == 0 ? DrawBufferMode.Back : DrawBufferMode.ColorAttachment0);
            State.BufferDontTouch = false;
            GraphicsUtil.CheckError("AFRFBO_1");
            View.BindFramebuffer(FramebufferTarget.ReadFramebuffer, (int)State.DeferredTarget.FBO); // TODO: is view line and line below needed?
            GL.BlitFramebuffer(0, 0, Config.Width, Config.Height, 0, 0, Config.Width, Config.Height, ClearBufferMask.DepthBufferBit, BlitFramebufferFilter.Nearest);
            GraphicsUtil.CheckError("AFRFBO_2");
            View.BindFramebuffer(FramebufferTarget.ReadFramebuffer, Internal.FBO_GodRay_Main);
            GraphicsUtil.CheckError("AFRFBO_3");
            GL.BlitFramebuffer(0, 0, Config.Width, Config.Height, 0, 0, Config.Width, Config.Height, ClearBufferMask.ColorBufferBit, BlitFramebufferFilter.Nearest);
            GraphicsUtil.CheckError("AFRFBO_4");
            View.BindFramebuffer(FramebufferTarget.ReadFramebuffer, 0);
            GL.Enable(EnableCap.CullFace);
            GraphicsUtil.CheckError("AfterFirstRender");
            Config.PostFirstRender?.Invoke();
            GraphicsUtil.CheckError("AfterPostFirstRender");
        }

        /// <summary>
        /// Render transparent data.
        /// </summary>
        public int RenderPass_Transparents()
        {
            float fogDist = 1.0f / Engine.FogMaxDist();
            fogDist *= fogDist;
            Patches.PreTransparentPatch?.Invoke(fogDist);
            if (Engine.Deferred_TransparentLights)
            {
                if (Engine.Deferred_TransparentShadows)
                {
                    if (Engine.AllowLL)
                    {
                        Shaders.Deferred.Transparents_Lights_Shadows_LL = Shaders.Deferred.Transparents_Lights_Shadows_LL.Bind();
                        State.FBOid = FBOID.TRANSP_SHADOWS_LL;
                    }
                    else
                    {
                        Shaders.Deferred.Transparents_Lights_Shadows = Shaders.Deferred.Transparents_Lights_Shadows.Bind();
                        State.FBOid = FBOID.TRANSP_SHADOWS;
                    }
                }
                else
                {
                    if (Engine.AllowLL)
                    {
                        Shaders.Deferred.Transparents_Lights_LL = Shaders.Deferred.Transparents_Lights_LL.Bind();
                        State.FBOid = FBOID.TRANSP_LIT_LL;
                    }
                    else
                    {
                        Shaders.Deferred.Transparents_Lights = Shaders.Deferred.Transparents_Lights.Bind();
                        State.FBOid = FBOID.TRANSP_LIT;
                    }
                }
            }
            else
            {
                if (Engine.AllowLL)
                {
                    Shaders.Deferred.Transparents_LL = Shaders.Deferred.Transparents_LL.Bind();
                    State.FBOid = FBOID.TRANSP_LL;
                }
                else
                {
                    Shaders.Deferred.Transparents = Shaders.Deferred.Transparents.Bind();
                    State.FBOid = FBOID.TRANSP_UNLIT;
                }
            }
            GraphicsUtil.CheckError("PreTransp - 0");
            GL.UniformMatrix4(1, false, ref State.PrimaryMatrix);
            GL.UniformMatrix4(2, false, ref View3DInternalData.IdentityMatrix);
            GL.Uniform1(4, Config.DesaturationAmount);
            GL.Uniform1(13, fogDist);
            GL.Uniform3(14, State.CameraRelativePosition);
            GL.DepthMask(false);
            GraphicsUtil.CheckError("PreTransp - 1");
            if (Engine.AllowLL || !Engine.Deferred_BrightTransp)
            {
                View3D.StandardBlend();
            }
            else
            {
                View3D.TranspBlend();
            }
            GraphicsUtil.CheckError("PreTransp - 2");
            View.BindFramebuffer(FramebufferTarget.Framebuffer, Internal.FBO_Transparents_Main);
            View.DrawBuffer(DrawBufferMode.ColorAttachment0);
            View.BindFramebuffer(FramebufferTarget.ReadFramebuffer, (int)State.DeferredTarget.FBO);
            GL.BlitFramebuffer(0, 0, Config.Width, Config.Height, 0, 0, Config.Width, Config.Height, ClearBufferMask.DepthBufferBit, BlitFramebufferFilter.Nearest);
            View.BindFramebuffer(FramebufferTarget.ReadFramebuffer, 0);
            GL.ClearBuffer(ClearBuffer.Color, 0, new float[] { 0f, 0f, 0f, 0f });
            int lightc = 0;
            GraphicsUtil.CheckError("PreTransp - 3");
            // TODO: VR Patch!
            if (Engine.Render3DView || Engine.Client.VR != null)
            {
                View.Viewport(Config.Width / 2, 0, Config.Width / 2, Config.Height);
                Config.CameraPos = State.CameraBasePos + State.CameraAdjust;
                RenderTransp(ref lightc);
                State.CurrentFrustum = State.SecondEyeFrustum;
                View.Viewport(0, 0, Config.Width / 2, Config.Height);
                State.CurrentFrustum = State.SecondEyeFrustum;
                Patches.VRTransparentPatch?.Invoke();
                if (Engine.Deferred_TransparentLights)
                {
                    if (Engine.Deferred_TransparentShadows)
                    {
                        if (Engine.AllowLL)
                        {
                            Shaders.Deferred.Transparents_Lights_Shadows_LL = Shaders.Deferred.Transparents_Lights_Shadows_LL.Bind();
                            State.FBOid = FBOID.TRANSP_SHADOWS_LL;
                        }
                        else
                        {
                            Shaders.Deferred.Transparents_Lights_Shadows = Shaders.Deferred.Transparents_Lights_Shadows.Bind();
                            State.FBOid = FBOID.TRANSP_SHADOWS;
                        }
                    }
                    else
                    {
                        if (Engine.AllowLL)
                        {
                            Shaders.Deferred.Transparents_Lights_LL = Shaders.Deferred.Transparents_Lights_LL.Bind();
                            State.FBOid = FBOID.TRANSP_LIT_LL;
                        }
                        else
                        {
                            Shaders.Deferred.Transparents_Lights = Shaders.Deferred.Transparents_Lights.Bind();
                            State.FBOid = FBOID.TRANSP_LIT;
                        }
                    }
                }
                else
                {
                    if (Engine.AllowLL)
                    {
                        Shaders.Deferred.Transparents_LL = Shaders.Deferred.Transparents_LL.Bind();
                        State.FBOid = FBOID.TRANSP_LL;
                    }
                    else
                    {
                        Shaders.Deferred.Transparents = Shaders.Deferred.Transparents.Bind();
                        State.FBOid = FBOID.TRANSP_UNLIT;
                    }
                }
                GL.UniformMatrix4(1, false, ref State.PrimaryMatrix_OffsetFor3D);
                Config.CameraPos = State.CameraBasePos - State.CameraAdjust;
                Matrix4 orig = State.PrimaryMatrix;
                State.PrimaryMatrix = State.PrimaryMatrix_OffsetFor3D;
                Matrix4 orig_out = State.OutViewMatrix;
                State.OutViewMatrix = State.OutView_OffsetVR;
                State.IsSecondEye = true;
                RenderTransp(ref lightc, State.SecondEyeFrustum);
                State.IsSecondEye = false;
                State.OutViewMatrix = orig_out;
                State.PrimaryMatrix = orig;
                View.Viewport(0, 0, Config.Width, Config.Height);
                Config.CameraPos = State.CameraBasePos + State.CameraAdjust;
                State.CurrentFrustum = State.CameraFrustum;
            }
            else
            {
                RenderTransp(ref lightc);
            }
            if (lightc == 0)
            {
                lightc = 1;
            }
            GraphicsUtil.CheckError("AfterTransp");
            return lightc;
        }

        /// <summary>
        /// Apply godrays, bloom, and transparent data to screen.
        /// </summary>
        public void RenderPass_Bloom(int lightc)
        {
            View.BindFramebuffer(FramebufferTarget.Framebuffer, Internal.CurrentFBO);
            View.DrawBuffer(Internal.CurrentFBO == 0 ? DrawBufferMode.Back : DrawBufferMode.ColorAttachment0);
            View3D.StandardBlend();
            State.FBOid = FBOID.NONE;
            GL.ActiveTexture(TextureUnit.Texture0);
            GL.Disable(EnableCap.CullFace);
            GL.ClearBuffer(ClearBuffer.Depth, 0, new float[] { 1f });
            GL.Disable(EnableCap.DepthTest);
            GraphicsUtil.CheckError("PreGR");
            if (Config.GodRays)
            {
                // TODO: 3d stuff for GodRays.
                GL.ActiveTexture(TextureUnit.Texture1);
                GL.BindTexture(TextureTarget.Texture2D, State.DeferredTarget.DepthTexture);
                GL.ActiveTexture(TextureUnit.Texture0);
                GL.BindTexture(TextureTarget.Texture2D, Internal.FBO_GodRay_Texture_2);
                Shaders.Deferred.Godrays = Shaders.Deferred.Godrays.Bind();
                GL.UniformMatrix4(1, false, ref View3DInternalData.SimpleOrthoMatrix);
                GL.UniformMatrix4(2, false, ref View3DInternalData.IdentityMatrix);
                GL.Uniform1(6, State.CurrentExposure * Engine.Exposure);
                GL.Uniform1(7, Config.Width / (float)Config.Height);
                if (Config.SunLocation.IsNaN())
                {
                    GL.Uniform2(8, new Vector2(-10f, -10f));
                }
                else
                {
                    Vector4d v = Vector4d.TransformRow(new Vector4d(Config.SunLocation.ToOpenTK3D(), 1.0), State.PrimaryMatrixd);
                    if (v.Z / v.W > 1.0f || v.Z / v.W < 0.0f)
                    {
                        GL.Uniform2(8, new Vector2(-10f, -10f));
                    }
                    else
                    {
                        Vector2d lp1 = (v.Xy / v.W) * 0.5f + new Vector2d(0.5f);
                        GL.Uniform2(8, new Vector2((float)lp1.X, (float)lp1.Y));
                        float lplenadj = (float)((1.0 - Math.Min(lp1.Length, 1.0)) * (0.99 - 0.6) + 0.6);
                        GL.Uniform1(12, 0.84f * lplenadj);
                    }
                }
                GL.Uniform1(14, Engine.ZNear);
                GL.Uniform1(15, Engine.ZFar());
                GL.Uniform1(16, Engine.ZFarOut() * 0.5f);
                View3D.TranspBlend();
                Engine.Rendering.RenderRectangle(-1, -1, 1, 1);
                View3D.StandardBlend();
            }
            GraphicsUtil.CheckError("PostGR");
            {
                // TODO: Merge transp-to-screen and GR pass?
                //GL.Enable(EnableCap.DepthTest);
                GL.BindTexture(TextureTarget.Texture2D, Internal.FBO_Transparents_Texture);
                Shaders.Deferred.TransparentAdderPass = Shaders.Deferred.TransparentAdderPass.Bind();
                GL.UniformMatrix4(1, false, ref View3DInternalData.SimpleOrthoMatrix);
                GL.UniformMatrix4(2, false, ref View3DInternalData.IdentityMatrix);
                GL.Uniform1(3, (float)lightc);
                Engine.Rendering.RenderRectangle(-1, -1, 1, 1);
            }
            GL.UseProgram(0);
            GL.BindTexture(TextureTarget.Texture2D, 0);
            GL.DepthMask(true);
            GL.Enable(EnableCap.DepthTest);
            GL.Enable(EnableCap.CullFace);
            GraphicsUtil.CheckError("WrapUp");
            View.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
            View.DrawBuffer(DrawBufferMode.Back);
        }

        /// <summary>
        /// Render transparent objects into a temporary buffer.
        /// </summary>
        void RenderTransp(ref int lightc, Frustum frustumToUse = null)
        {
            if (Engine.AllowLL)
            {
                GL.ActiveTexture(TextureUnit.Texture4);
                GL.BindTexture(TextureTarget.Texture2DArray, Internal.LL_TransparentTextures[0]);
                GL.BindImageTexture(4, Internal.LL_TransparentTextures[0], 0, false, 0, TextureAccess.ReadWrite, SizedInternalFormat.R32ui);
                GL.ActiveTexture(TextureUnit.Texture5);
                GL.BindTexture(TextureTarget.TextureBuffer, Internal.LL_TransparentTextures[1]);
                GL.BindImageTexture(5, Internal.LL_TransparentTextures[1], 0, false, 0, TextureAccess.ReadWrite, SizedInternalFormat.Rgba32f);
                GL.ActiveTexture(TextureUnit.Texture6);
                GL.BindTexture(TextureTarget.TextureBuffer, Internal.LL_TransparentTextures[2]);
                GL.BindImageTexture(6, Internal.LL_TransparentTextures[2], 0, false, 0, TextureAccess.ReadWrite, SizedInternalFormat.R32ui);
                GL.ActiveTexture(TextureUnit.Texture7);
                GL.BindTexture(TextureTarget.TextureBuffer, Internal.LL_TransparentTextures[3]);
                GL.BindImageTexture(7, Internal.LL_TransparentTextures[3], 0, false, 0, TextureAccess.ReadWrite, SizedInternalFormat.R32ui);
                GL.ActiveTexture(TextureUnit.Texture0);
                Shaders.Deferred.LLClearerPass.Bind();
                GL.Uniform2(4, new Vector2(Config.Width, Config.Height));
                Matrix4 flatProj = Matrix4.CreateOrthographicOffCenter(-1, 1, 1, -1, -1, 1);
                GL.UniformMatrix4(1, false, ref flatProj);
                GL.UniformMatrix4(2, false, ref View3DInternalData.IdentityMatrix);
                GL.Uniform2(4, new Vector2(Config.Width, Config.Height));
                Engine.Rendering.RenderRectangle(-1, -1, 1, 1);
                GL.MemoryBarrier(MemoryBarrierFlags.AllBarrierBits);
                //s_whatever.Bind();
                //GL.Uniform2(4, new Vector2(Window.Width, Window.Height));
                //GL.ClearBuffer(ClearBuffer.Color, 0, new float[] { 0f, 0f, 0f, 1f });
                //GL.ClearBuffer(ClearBuffer.Depth, 0, new float[] { 1f });
                //Matrix4 proj = Matrix4.CreatePerspectiveFieldOfView(FOV, Window.Width / (float)Window.Height, ZNear, ZFar);
                //Matrix4 view = Matrix4.LookAt(CamPos, CamGoal, Vector3.UnitZ);
                //Matrix4 combined = view * proj;
                //GL.UniformMatrix4(1, false, ref combined);
                RenderTranspInt(ref lightc, frustumToUse);
                GL.MemoryBarrier(MemoryBarrierFlags.AllBarrierBits);
                Shaders.Deferred.LLFinalPass.Bind();
                GL.Uniform2(4, new Vector2(Config.Width, Config.Height));
                GL.UniformMatrix4(1, false, ref flatProj);
                GL.UniformMatrix4(2, false, ref View3DInternalData.IdentityMatrix);
                GL.Uniform2(4, new Vector2(Config.Width, Config.Height));
                Engine.Rendering.RenderRectangle(-1, -1, 1, 1);
            }
            else
            {
                RenderTranspInt(ref lightc, frustumToUse);
            }
        }

        /// <summary>
        /// The actual internal transparency render method.
        /// </summary>
        public void RenderTranspInt(ref int lightc, Frustum frustumToUse)
        {
            if (frustumToUse == null)
            {
                frustumToUse = State.CameraFrustum;
            }
            if (Engine.Deferred_TransparentLights)
            {
                State.RenderLights = true;
                foreach (LightObject light in Config.Lights)
                {
                    if (light is SkyLight || frustumToUse == null || frustumToUse.ContainsSphere(light.EyePos, light.MaxDistance))
                    {
                        lightc += light.InternalLights.Count;
                    }
                }
                int lightCount = 0;
                float[] lightData = new float[View3DInternalData.LIGHTS_MAX * 16];
                float[] shadowMatrices = new float[View3DInternalData.LIGHTS_MAX * 16];
                foreach (LightObject light in Config.Lights)
                {
                    if (light is SkyLight || frustumToUse == null || frustumToUse.ContainsSphere(light.EyePos, light.MaxDistance))
                    {
                        foreach (Light subLight in light.InternalLights)
                        {
                            Matrix4 lightProjMatrix = subLight.GetMatrix(View);
                            float maxrange = (subLight is LightOrtho) ? View3DInternalData.LIGHT_MAXIUM_RADIUS : subLight.MaxRange;
                            Matrix4 lightDataMatrix = new Matrix4(Vector4.Zero, Vector4.Zero, Vector4.Zero, Vector4.Zero);
                            lightDataMatrix[0, 0] = maxrange <= 0 ? View3DInternalData.LIGHT_MAXIUM_RADIUS : maxrange;
                            lightDataMatrix[0, 1] = (float)(light.EyePos.X - State.RenderRelative.X);
                            lightDataMatrix[0, 2] = (float)(light.EyePos.Y - State.RenderRelative.Y);
                            lightDataMatrix[0, 3] = (float)(light.EyePos.Z - State.RenderRelative.Z);
                            lightDataMatrix[1, 0] = subLight.Color.X;
                            lightDataMatrix[1, 1] = subLight.Color.Y;
                            lightDataMatrix[1, 2] = subLight.Color.Z;
                            lightDataMatrix[1, 3] = (light is SpotLight) ? 1f : 0f;
                            lightDataMatrix[2, 0] = (subLight is LightOrtho) ? 1f : 0f;
                            lightDataMatrix[2, 1] = 1f / Config.ShadowTexSize();
                            lightDataMatrix[2, 2] = State.CurrentExposure * Engine.Exposure;
                            lightDataMatrix[2, 3] = (float)lightc; // TODO: Move view to a generic
                            lightDataMatrix[3, 0] = (float)Config.Ambient.X; // TODO: Remove ambient
                            lightDataMatrix[3, 1] = (float)Config.Ambient.Y;
                            lightDataMatrix[3, 2] = (float)Config.Ambient.Z;
                            for (int mx = 0; mx < 4; mx++)
                            {
                                for (int my = 0; my < 4; my++)
                                {
                                    shadowMatrices[lightCount * 16 + mx * 4 + my] = lightProjMatrix[mx, my];
                                    lightData[lightCount * 16 + mx * 4 + my] = lightDataMatrix[mx, my];
                                }
                            }
                            lightCount++;
                            if (lightCount >= View3DInternalData.LIGHTS_MAX)
                            {
                                goto lights_apply;
                            }
                        }
                    }
                }
            lights_apply:
                State.TransparentLightCount = lightCount;
                GraphicsUtil.CheckError("PreRenderTranspLights");
                Matrix4 dataMatrix = new Matrix4(lightCount, Engine.ZNear, Engine.ZFar(), Config.Width, Config.Height, 0, 0, 0, 0, 0, 0, 0, (float)Config.FogCol.X, (float)Config.FogCol.Y, (float)Config.FogCol.Z, Config.FogAlpha);
                if (Engine.Deferred_Shadows)
                {
                    if (Engine.AllowLL)
                    {
                        Shaders.Deferred.Transparents_Particles_Lights_Shadows_LL = Shaders.Deferred.Transparents_Particles_Lights_Shadows_LL.Bind();
                    }
                    else
                    {
                        Shaders.Deferred.Transparents_Particles_Lights_Shadows = Shaders.Deferred.Transparents_Particles_Lights_Shadows.Bind();
                    }
                    ConfigureParticleLightsShader(dataMatrix, lightData, shadowMatrices);
                }
                if (Engine.AllowLL)
                {
                    Shaders.Deferred.Transparents_Particles_Lights_LL = Shaders.Deferred.Transparents_Particles_Lights_LL.Bind();
                }
                else
                {
                    Shaders.Deferred.Transparents_Particles_Lights = Shaders.Deferred.Transparents_Particles_Lights.Bind();
                }
                GraphicsUtil.CheckError("PreRenderTranspLights - 1.5");
                ConfigureParticleLightsShader(dataMatrix, lightData, shadowMatrices);
                Patches.TransparentLightPatch?.Invoke(dataMatrix, shadowMatrices, lightData);
                GraphicsUtil.CheckError("PreRenderTranspLights - 3");
                if (Engine.Deferred_Shadows)
                {
                    if (Engine.AllowLL)
                    {
                        Shaders.Deferred.Transparents_Lights_Shadows_LL = Shaders.Deferred.Transparents_Lights_Shadows_LL.Bind();
                    }
                    else
                    {
                        Shaders.Deferred.Transparents_Lights_Shadows = Shaders.Deferred.Transparents_Lights_Shadows.Bind();
                    }
                }
                else
                {
                    if (Engine.AllowLL)
                    {
                        Shaders.Deferred.Transparents_Lights_LL = Shaders.Deferred.Transparents_Lights_LL.Bind();
                    }
                    else
                    {
                        Shaders.Deferred.Transparents_Lights = Shaders.Deferred.Transparents_Lights.Bind();
                    }
                }
                GL.UniformMatrix4(2, false, ref View3DInternalData.IdentityMatrix);
                GL.Uniform2(8, new Vector2(Config.Width, Config.Height));
                GL.UniformMatrix4(9, false, ref dataMatrix);
                GL.UniformMatrix4(20, View3DInternalData.LIGHTS_MAX, false, shadowMatrices);
                GL.UniformMatrix4(20 + View3DInternalData.LIGHTS_MAX, View3DInternalData.LIGHTS_MAX, false, lightData);
                GL.ActiveTexture(TextureUnit.Texture4);
                GL.BindTexture(TextureTarget.Texture2DArray, Internal.FBO_Shadow_DepthTexture);
                GL.ActiveTexture(TextureUnit.Texture0);
                GraphicsUtil.CheckError("PreparedRenderTranspLights");
                Config.Render3D(View);
                GraphicsUtil.CheckError("PostRenderTranspLights");
                GL.ActiveTexture(TextureUnit.Texture2);
                GL.BindTexture(TextureTarget.Texture2D, 0);
                GL.ActiveTexture(TextureUnit.Texture0);
                State.RenderLights = false;
            }
            else
            {
                if (Engine.AllowLL)
                {
                    Matrix4 matabc = new Matrix4(Vector4.Zero, Vector4.Zero, Vector4.Zero, Vector4.Zero);
                    matabc[0, 3] = (float)Config.Width;
                    matabc[1, 3] = (float)Config.Height;
                    Patches.LLPatch?.Invoke(matabc);
                    Shaders.Deferred.Transparents_LL.Bind();
                    //GL.UniformMatrix4(1, false, ref combined);
                    GL.UniformMatrix4(2, false, ref View3DInternalData.IdentityMatrix);
                    GL.UniformMatrix4(9, false, ref matabc);
                }
                else
                {
                    Patches.TransparentRenderPatch?.Invoke();
                    Shaders.Deferred.Transparents.Bind();
                    //GL.UniformMatrix4(1, false, ref combined);
                    GL.UniformMatrix4(2, false, ref View3DInternalData.IdentityMatrix);
                }
                Config.Render3D(View);
            }
        }

        /// <summary>
        /// Configures uniform variable data for a Particle Lights shader.
        /// </summary>
        /// <param name="dataMatrix">Matrix hacked to store data about current frame.</param>
        /// <param name="lightData">Array of data for each light.</param>
        /// <param name="shadowMatrices">Matrices of shadow perspectives.</param>
        public void ConfigureParticleLightsShader(Matrix4 dataMatrix, float[] lightData, float[] shadowMatrices)
        {
            GL.UniformMatrix4(2, false, ref View3DInternalData.IdentityMatrix);
            GL.Uniform1(4, Config.DesaturationAmount);
            //GL.Uniform1(7, (float)TheClient.GlobalTickTimeLocal);
            GL.Uniform2(8, new Vector2(Config.Width, Config.Height));
            GraphicsUtil.CheckError("PreRenderTranspLights - 1.75");
            GL.UniformMatrix4(9, false, ref dataMatrix);
            GL.Uniform3(14, State.CameraRelativePosition);
            GL.UniformMatrix4(20, View3DInternalData.LIGHTS_MAX, false, shadowMatrices);
            GL.UniformMatrix4(20 + View3DInternalData.LIGHTS_MAX, View3DInternalData.LIGHTS_MAX, false, lightData);
            GraphicsUtil.CheckError("PreRenderTranspLights - 2");
        }

        /// <summary>
        /// Reads the DynamicExposure result from the GPU.
        /// </summary>
        public void ReadDynamicExposure()
        {
            if (Engine.Deferred_DynamicExposure)
            {
                View.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
                View.BindFramebuffer(FramebufferTarget.ReadFramebuffer, Internal.FBO_DynamicExposure);
                GL.ReadBuffer(ReadBufferMode.ColorAttachment0);
                GL.ReadPixels(0, 0, View3DInternalData.DYNAMIC_EXPOSURE_SPREAD, View3DInternalData.DYNAMIC_EXPOSURE_SPREAD, PixelFormat.Red, PixelType.Float, Internal.DynamicExposureResult);
                View.BindFramebuffer(FramebufferTarget.ReadFramebuffer, 0);
                GL.ReadBuffer(ReadBufferMode.None);
                float exp = FindExp(Internal.DynamicExposureResult);
                exp = Math.Max(Math.Min(exp, 5.0f), 0.4f);
                exp = 1.0f / exp;
                float stepUp = (float)Engine.Delta * 0.05f;
                float stepDown = stepUp * 5.0f;
                float relative = Math.Abs(State.CurrentExposure - exp);
                float modder = relative * 3f;
                stepUp *= modder;
                stepDown *= modder;
                if (exp > State.CurrentExposure + stepUp)
                {
                    State.CurrentExposure += stepUp;
                }
                else if (exp < State.CurrentExposure - stepDown)
                {
                    State.CurrentExposure -= stepDown;
                }
                else
                {
                    State.CurrentExposure = exp;
                }
            }
            else
            {
                State.CurrentExposure = 0.75f;
            }
        }

        /// <summary>
        /// Helper to find exposure from a result array.
        /// </summary>
        /// <param name="inp">Exposure array input.</param>
        public static float FindExp(float[] inp)
        {
            float total = 0f;
            for (int i = 0; i < inp.Length; i++)
            {
                total += inp[i];
            }
            return total / (float)inp.Length;
        }
    }
}
