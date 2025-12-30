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
using FGECore.MathHelpers;
using FGEGraphics.GraphicsHelpers;
using FGEGraphics.LightingSystem;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace FGEGraphics.ClientSystem.ViewRenderSystem;

/// <summary>Handles forward rendering logic for <see cref="View3D"/>.</summary>
public class View3DForwardRenderer : View3DCoreDataSet
{
    /// <summary>Patches to apply (if any).</summary>
    public View3DPatchesForward Patches = new();

    /// <summary>Render everything as quickly as possible: a simple forward renderer.</summary>
    public void RenderPass_FAST()
    {
        GraphicsUtil.CheckError("Render/Fast - Prep");
        //if (Engine.DisplayDecals || Engine.ForwardReflections)
        {
            State.DeferredTarget.Bind(View);
            DeferredRenderTarget.Clear();
            GL.ClearBuffer(ClearBuffer.Color, 0, Config.ClearColor);
        }
        GraphicsUtil.CheckError("Render/Fast - PreLight");
        float[] light_dat = new float[View3DInternalData.LIGHTS_MAX * 16];
        float[] shadowmat_dat = new float[View3DInternalData.LIGHTS_MAX * 16];
        int c = 0;
        if (Engine.Forward_Lights)
        {
            // TODO: An ambient light source?
            foreach (LightObject light in Config.Lights)
            {
                if (light is SkyLight || State.CameraFrustum is null || State.CameraFrustum.ContainsSphere(light.EyePos, light.MaxDistance))
                {
                    GraphicsUtil.CheckError("Render/Fast - PreSingle");
                    double d1 = (light.EyePos - Config.CameraPos).LengthSquared();
                    double d2 = Config.LightsMaxDistance * Config.LightsMaxDistance + light.MaxDistance * light.MaxDistance;
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
                            Vector3d eyep = light.InternalLights[0].EyePosition - State.RenderRelative.ToOpenTK3D();
                            Color3F col = light.InternalLights[0].Color * (float)maxrangemult;
                            Matrix4 light_data = new(
                                (float)eyep.X, (float)eyep.Y, (float)eyep.Z, // light_pos
                                0.7f, // diffuse_albedo
                                0.7f, // specular_albedo
                                0.0f, // should_sqrt
                                col.R, col.G, col.B, // light_color
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
                            GraphicsUtil.CheckError("Render/Fast - PostPoint");
                        }
                        else
                        {
                            for (int x = 0; x < light.InternalLights.Count; x++)
                            {
                                if (light.InternalLights[x].Color.StrengthSquared <= 0.01)
                                {
                                    continue;
                                }
                                int sp = Config.ShadowTexSize();
                                if (c >= 10)
                                {
                                    sp /= 2;
                                }
                                Matrix4 smat = light.InternalLights[x].GetMatrix(View);
                                Vector3d eyep = light is SkyLight se ? -se.Direction.ToOpenTK3D() : light.InternalLights[x].EyePosition - State.RenderRelative.ToOpenTK3D();
                                Color3F col = light.InternalLights[x].Color * (float)maxrangemult;
                                Matrix4 light_data = new(
                                    (float)eyep.X, (float)eyep.Y, (float)eyep.Z, // light_pos
                                    0.7f, // diffuse_albedo
                                    0.7f, // specular_albedo
                                    light.InternalLights[x] is LightOrtho ? 1.0f : 0.0f, // should_sqrt
                                    col.R, col.G, col.B, // light_color
                                    light.InternalLights[x] is LightOrtho ? View3DInternalData.LIGHT_MAXIUM_RADIUS : (light.InternalLights[0].MaxRange <= 0 ? View3DInternalData.LIGHT_MAXIUM_RADIUS : light.InternalLights[0].MaxRange), // light_radius
                                    0f, 0f, 0f, // eye_pos
                                    light is SpotLight ? 1.0f : 0.0f, // light_type
                                    1f / sp, // tex_size
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
                            GraphicsUtil.CheckError("Render/Fast - PostGeneric");
                        }
                    }
                }
            }
        }
    lights_apply:
        GraphicsUtil.CheckError("Render/Fast - Lights");
        if (Engine.Forward_Shadows)
        {
            GL.ActiveTexture(TextureUnit.Texture5);
            GraphicsUtil.BindTexture(TextureTarget.Texture2D, 0);
            Internal.FBO_Shadow_DepthTexture.Bind();
            GL.ActiveTexture(TextureUnit.Texture0);
        }
        float fogDist = 1.0f / Engine.FogMaxDist();
        fogDist *= fogDist;
        void basicUniforms(bool lights)
        {
            GL.UniformMatrix4(1, false, ref State.PrimaryMatrix);
            GL.UniformMatrix4(2, false, ref View3DInternalData.IdentityMatrix);
            GraphicsUtil.CheckError("Render/Fast - Uniforms - Basic - 1");
            Engine.Rendering.SetColor(Color4.White, View);
            GL.Uniform4(4, new Vector4(Config.Width, Config.Height, Engine.ZNear, Engine.ZFar()));
            GraphicsUtil.CheckError("Render/Fast - Uniforms - Basic - 2");
            GL.Uniform1(6, (float)Engine.GlobalTickTime);
            GraphicsUtil.CheckError("Render/Fast - Uniforms - Basic - 2.5");
            GL.Uniform4(12, new Vector4(Config.FogCol.ToOpenTK(), Config.FogAlpha));
            GraphicsUtil.CheckError("Render/Fast - Uniforms - Basic - 3");
            GL.Uniform1(13, fogDist);
            GL.Uniform3(14, State.CameraRelativePosition);
            GraphicsUtil.CheckError("Render/Fast - Uniforms - Basic - 4");
            if (lights)
            {
                GL.Uniform1(15, (float)c);
                GL.UniformMatrix4(25, View3DInternalData.LIGHTS_MAX, false, shadowmat_dat);
                GL.UniformMatrix4(25 + View3DInternalData.LIGHTS_MAX, View3DInternalData.LIGHTS_MAX, false, light_dat);
                GraphicsUtil.CheckError("Render/Fast - Uniforms - Basic - 5");
            }
            GL.Uniform3(18, -Engine.SunAdjustDirection.ToOpenTK());
            GL.Uniform3(19, Engine.SunAdjustBackupLight.Xyz);
            GraphicsUtil.CheckError("Render/Fast - Uniforms - Basic - 6");
            Patches.PerShaderPatch?.Invoke();
            GraphicsUtil.CheckError("Render/Fast - Uniforms - Basic - 7");
        }
        GraphicsUtil.CheckError("Render/Fast - Uniforms 1");
        State.RenderingShadows = false;
        State.RenderLights = Engine.Forward_Lights;
        GL.ActiveTexture(TextureUnit.Texture0);
        State.FBOid = FBOID.FORWARD_SOLID;
        Shaders.Forward.Particles.Bind();
        basicUniforms(false);
        GraphicsUtil.CheckError("Render/Fast - Uniforms - ForwardParticles");
        Shaders.Forward.ParticlesNoFade.Bind();
        basicUniforms(false);
        GraphicsUtil.CheckError("Render/Fast - Uniforms - ForwardParticlesNoFade");
        Shaders.Forward.Decals.Bind();
        basicUniforms(Engine.Forward_Lights);
        GraphicsUtil.CheckError("Render/Fast - Uniforms - ForwardDecals");
        Shaders.Forward.AllTransparencies_Sky.Bind();
        basicUniforms(false);
        GraphicsUtil.CheckError("Render/Fast - Uniforms - ForwardAllTransparenciesSky");
        Shaders.Forward.AllTransparencies_NoFog.Bind();
        basicUniforms(false);
        GraphicsUtil.CheckError("Render/Fast - Uniforms - ForwardAllTransparenciesNoFog");
        Shaders.Forward.AllTransparencies_Objects.Bind();
        basicUniforms(false);
        GraphicsUtil.CheckError("Render/Fast - Uniforms - ForwardAllTransparenciesObjects");
        Patches.PreSolidPatch?.Invoke(shadowmat_dat, light_dat, fogDist, c);
        Shaders.Forward.BasicSolid_NoBones.Bind();
        basicUniforms(Engine.Forward_Lights);
        GraphicsUtil.CheckError("Render/Fast - Uniforms - Forward BasicSolidNoBones");
        Shaders.Forward.BasicSolid.Bind();
        basicUniforms(Engine.Forward_Lights);
        GraphicsUtil.CheckError("Render/Fast - Uniforms - Forward BasicSolid");
        if (Engine.Render3DView || Engine.Client.VR is not null)
        {
            View.Viewport(Config.Width / 2, 0, Config.Width / 2, Config.Height);
            Config.Render3D(View);
            State.FBOid = FBOID.FORWARD_SOLID;
            State.CurrentFrustum = State.SecondEyeFrustum;
            View.Viewport(0, 0, Config.Width / 2, Config.Height);
            Config.CameraPos = State.CameraBasePos - State.CameraAdjust;
            Patches.VRSolidPatch?.Invoke();
            Shaders.Forward.Particles.Bind();
            GL.UniformMatrix4(1, false, ref State.PrimaryMatrix_OffsetFor3D);
            Shaders.Forward.Decals.Bind();
            GL.UniformMatrix4(1, false, ref State.PrimaryMatrix_OffsetFor3D);
            Shaders.Forward.AllTransparencies_NoFog.Bind();
            GL.UniformMatrix4(1, false, ref State.PrimaryMatrix_OffsetFor3D);
            Shaders.Forward.AllTransparencies_Objects.Bind();
            GL.UniformMatrix4(1, false, ref State.PrimaryMatrix_OffsetFor3D);
            Shaders.Forward.AllTransparencies_Sky.Bind();
            GL.UniformMatrix4(1, false, ref State.PrimaryMatrix_OffsetFor3D);
            Shaders.Forward.BasicSolid_NoBones.Bind();
            GL.UniformMatrix4(1, false, ref State.PrimaryMatrix_OffsetFor3D);
            Shaders.Forward.BasicSolid = Shaders.Forward.BasicSolid.Bind();
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
            GraphicsUtil.CheckError("Render/Fast - 3D Solid");
        }
        else
        {
            Config.Render3D(View);
            GraphicsUtil.CheckError("Render/Fast - Solid");
        }
        //if (Engine.DisplayDecals || Engine.ForwardReflections)
        {
            State.DeferredTarget.Unbind(View);
            View.BindFramebuffer(FramebufferTarget.Framebuffer, Internal.CurrentFBO);
            View.DrawBuffer(Internal.CurrentFBO == 0 ? DrawBufferMode.Back : DrawBufferMode.ColorAttachment0);
            GL.BindFramebuffer(FramebufferTarget.ReadFramebuffer, State.DeferredTarget.FBO);
            GL.BlitFramebuffer(0, 0, Config.Width, Config.Height, 0, 0, Config.Width, Config.Height, ClearBufferMask.DepthBufferBit | ClearBufferMask.ColorBufferBit, BlitFramebufferFilter.Nearest);
            GL.BindFramebuffer(FramebufferTarget.ReadFramebuffer, 0);
            GraphicsUtil.CheckError("Render/Fast - PreReflect");
            if (Engine.ForwardReflections)
            {
                Shaders.Forward.PostProcess = Shaders.Forward.PostProcess.Bind();
                GL.UniformMatrix4(1, false, ref View3DInternalData.SimpleOrthoMatrix);
                GL.UniformMatrix4(2, false, ref View3DInternalData.IdentityMatrix);
                GL.UniformMatrix4(6, false, ref State.PrimaryMatrix);
                GL.Uniform2(5, new Vector2(Engine.ZNear, Engine.ZFar()));
                GL.Uniform2(7, new Vector2(60f, Engine.ZFarOut())); // TODO: View3D-level Vars!
                GL.Uniform3(8, State.CameraRelativePosition);
                GL.ActiveTexture(TextureUnit.Texture5);
                Internal.FBO_OutView_DepthTexture.Bind();
                GL.ActiveTexture(TextureUnit.Texture4);
                State.DeferredTarget.PositionTexture.Bind();
                GL.ActiveTexture(TextureUnit.Texture3);
                State.DeferredTarget.DepthTexture.Bind();
                GL.ActiveTexture(TextureUnit.Texture2);
                State.DeferredTarget.NormalsTexture.Bind();
                GL.ActiveTexture(TextureUnit.Texture1);
                State.DeferredTarget.DiffuseTexture.Bind();
                GL.ActiveTexture(TextureUnit.Texture0);
                State.DeferredTarget.Rh2Texture.Bind();
                GL.Disable(EnableCap.DepthTest);
                GL.Disable(EnableCap.CullFace);
                Engine.Rendering.RenderRectangle(-1, -1, 1, 1);
                GL.Enable(EnableCap.DepthTest);
                GL.Enable(EnableCap.CullFace);
                GL.ActiveTexture(TextureUnit.Texture5);
                GraphicsUtil.BindTexture(TextureTarget.Texture2D, 0);
                GL.ActiveTexture(TextureUnit.Texture4);
                GraphicsUtil.BindTexture(TextureTarget.Texture2D, 0);
                GL.ActiveTexture(TextureUnit.Texture3);
                GraphicsUtil.BindTexture(TextureTarget.Texture2D, 0);
                GL.ActiveTexture(TextureUnit.Texture2);
                GraphicsUtil.BindTexture(TextureTarget.Texture2D, 0);
                GL.ActiveTexture(TextureUnit.Texture1);
                Engine.Textures.NormalDef.Bind();
                GL.ActiveTexture(TextureUnit.Texture0);
                GraphicsUtil.BindTexture(TextureTarget.Texture2D, 0);
                GraphicsUtil.CheckError("Render/Fast - Reflections");
            }
        }
        if (Engine.DisplayDecals)
        {
            Shaders.Forward.Decals = Shaders.Forward.Decals.Bind();
            GL.ActiveTexture(TextureUnit.Texture4);
            State.DeferredTarget.DepthTexture.Bind();
            GL.ActiveTexture(TextureUnit.Texture0);
            State.FBOid = FBOID.FORWARD_EXTRAS;
            GL.DepthMask(false);
            GraphicsUtil.CheckError("Render/Fast - Decal Prep");
            if (Engine.Render3DView || Engine.Client.VR != null)
            {
                View.Viewport(Config.Width / 2, 0, Config.Width / 2, Config.Height);
                Config.DecalRender?.Invoke(View);
                State.CurrentFrustum = State.SecondEyeFrustum;
                View.Viewport(0, 0, Config.Width / 2, Config.Height);
                Config.CameraPos = State.CameraBasePos - State.CameraAdjust;
                GL.UniformMatrix4(1, false, ref State.PrimaryMatrix_OffsetFor3D);
                State.IsSecondEye = true;
                Config.DecalRender?.Invoke(View);
                State.IsSecondEye = false;
                View.Viewport(0, 0, Config.Width, Config.Height);
                Config.CameraPos = State.CameraBasePos + State.CameraAdjust;
                State.CurrentFrustum = State.CameraFrustum;
                GraphicsUtil.CheckError("Render/Fast - Decals 3D");
            }
            else
            {
                State.FBOid = FBOID.FORWARD_EXTRAS;
                Shaders.Forward.Decals = Shaders.Forward.Decals.Bind();
                Config.DecalRender?.Invoke(View);
                GraphicsUtil.CheckError("Render/Fast - Decals");
            }
        }
        GL.ActiveTexture(TextureUnit.Texture0);
        State.FBOid = FBOID.FORWARD_TRANSP;
        Patches.PreTransparentPatch?.Invoke(fogDist, shadowmat_dat, light_dat, c);
        GraphicsUtil.CheckError("Render/Fast - PreTranspPatch");
        Shaders.Forward.BasicTransparent_NoBones.Bind();
        GL.UniformMatrix4(1, false, ref State.PrimaryMatrix);
        GL.UniformMatrix4(2, false, ref View3DInternalData.IdentityMatrix);
        GL.Uniform1(6, (float)Engine.GlobalTickTime);
        GL.Uniform4(12, new Vector4(Config.FogCol.ToOpenTK(), Config.FogAlpha));
        GL.Uniform1(13, fogDist);
        GL.Uniform3(14, State.CameraRelativePosition);
        if (Engine.Forward_Lights)
        {
            GL.Uniform1(15, (float)c);
            GL.UniformMatrix4(25, View3DInternalData.LIGHTS_MAX, false, shadowmat_dat);
            GL.UniformMatrix4(25 + View3DInternalData.LIGHTS_MAX, View3DInternalData.LIGHTS_MAX, false, light_dat);
        }
        //GL.Uniform2(14, zfar_rel);
        Engine.Rendering.SetColor(Color4.White, View);
        GraphicsUtil.CheckError("Render/Fast - BasicTransparent_NoBones Unifs");
        Shaders.Forward.BasicTransparent.Bind();
        GL.UniformMatrix4(1, false, ref State.PrimaryMatrix);
        GL.UniformMatrix4(2, false, ref View3DInternalData.IdentityMatrix);
        GL.Uniform1(6, (float)Engine.GlobalTickTime);
        GL.Uniform4(12, new Vector4(Config.FogCol.ToOpenTK(), Config.FogAlpha));
        GL.Uniform1(13, fogDist);
        GL.Uniform3(14, State.CameraRelativePosition);
        if (Engine.Forward_Lights)
        {
            GL.Uniform1(15, (float)c);
            GL.UniformMatrix4(25, View3DInternalData.LIGHTS_MAX, false, shadowmat_dat);
            GL.UniformMatrix4(25 + View3DInternalData.LIGHTS_MAX, View3DInternalData.LIGHTS_MAX, false, light_dat);
        }
        //GL.Uniform2(14, zfar_rel);
        Engine.Rendering.SetColor(Color4.White, View);
        GraphicsUtil.CheckError("Render/Fast - Transp Unifs");
        Config.PostFirstRender?.Invoke();
        GraphicsUtil.CheckError("Render/Fast - PostFirstRender");
        if (Engine.Render3DView || Engine.Client.VR is not null)
        {
            View.Viewport(Config.Width / 2, 0, Config.Width / 2, Config.Height);
            Config.Render3D(View);
            State.CurrentFrustum = State.SecondEyeFrustum;
            View.Viewport(0, 0, Config.Width / 2, Config.Height);
            Config.CameraPos = State.CameraBasePos - State.CameraAdjust;
            Patches.VRTransparentPatch?.Invoke();
            Shaders.Forward.BasicTransparent_NoBones.Bind();
            GL.UniformMatrix4(1, false, ref State.PrimaryMatrix_OffsetFor3D);
            Shaders.Forward.BasicTransparent = Shaders.Forward.BasicTransparent.Bind();
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
            GraphicsUtil.CheckError("Render/Fast - Transp 3D");
        }
        else
        {
            Config.Render3D(View);
            GraphicsUtil.CheckError("Render/Fast - Transp");
        }
        Patches.EndPatch?.Invoke();
        GraphicsUtil.CheckError("Render/Fast - EndPatch");
        if (Engine.Forward_Shadows)
        {
            GL.ActiveTexture(TextureUnit.Texture5);
            GraphicsUtil.BindTexture(TextureTarget.Texture2D, 0);
            Internal.FBO_Shadow_DepthTexture.Bind();
            GL.ActiveTexture(TextureUnit.Texture0);
        }
        GL.ActiveTexture(TextureUnit.Texture4);
        GraphicsUtil.BindTexture(TextureTarget.Texture2D, 0);
        GL.ActiveTexture(TextureUnit.Texture0);
        GL.DepthMask(true);
        View.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
        View.DrawBuffer(DrawBufferMode.Back);
        GraphicsUtil.CheckError("AfterFast");
    }
}
