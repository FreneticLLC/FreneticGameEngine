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
using FGEGraphics.GraphicsHelpers;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace FGEGraphics.ClientSystem.ViewRenderSystem;

/// <summary>Class that helps with basic data generation for <see cref="View3D"/>.</summary>
public class View3DGenerationHelper : View3DCoreDataSet
{
    /// <summary>Generates the <see cref="View3D"/>.</summary>
    /// <param name="w">Width.</param>
    /// <param name="h">Height.</param>
    public void Generate(int w, int h)
    {
        Config.Width = w;
        Config.Height = h;
        GenerateLightHelpers();
        GraphicsUtil.CheckError("Load - View3D - Light");
        GenerateTranspHelpers();
        GraphicsUtil.CheckError("Load - View3D - Transp");
        GenerateOutDepthHelpers();
        GraphicsUtil.CheckError("Load - View3D - OV");
    }

    /// <summary>Generate helpers to deal with out-view depth logic.</summary>
    public void GenerateOutDepthHelpers()
    {
        if (Internal.FBO_OutView_Main != 0)
        {
            GL.DeleteFramebuffer(Internal.FBO_OutView_Main);
            Internal.FBO_OutView_DepthTexture.Dispose();
        }
        Internal.FBO_OutView_DepthTexture = new("View3DGenHelper_FBO_OutView_Depth", TextureTarget.Texture2D);
        GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.DepthComponent32, Config.Width, Config.Height, 0, PixelFormat.DepthComponent, PixelType.Float, IntPtr.Zero);
        GraphicsUtil.TexParamLinearClamp();
        Internal.FBO_OutView_Main = GL.GenFramebuffer();
        View.BindFramebuffer(FramebufferTarget.Framebuffer, Internal.FBO_OutView_Main);
        GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.DepthAttachment, TextureTarget.Texture2D, Internal.FBO_OutView_DepthTexture.ID, 0);
        GL.ClearBuffer(ClearBuffer.Depth, 0, View3DInternalData.ARR_FLOAT_1F_1);
        View.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
        GraphicsUtil.BindTexture(TextureTarget.Texture2D, 0);
    }

    /// <summary>(Re-)Generate transparent helpers, internal call.</summary>
    public void GenerateTranspHelpers()
    {
        if (Internal.FBO_Transparents_Main != 0)
        {
            GL.DeleteFramebuffer(Internal.FBO_Transparents_Main);
            Internal.FBO_Transparents_Texture.Dispose();
            Internal.FBO_Transparents_DepthTexture.Dispose();
        }
        // TODO: Helper class!
        Internal.FBO_Transparents_Texture = new("View3DGenHelper_FBO_Transp_Text", TextureTarget.Texture2D);
        GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba32f, Config.Width, Config.Height, 0, PixelFormat.Rgba, PixelType.Float, IntPtr.Zero);
        GraphicsUtil.TexParamLinearClamp();
        Internal.FBO_Transparents_DepthTexture = new("View3DGenHelper_FBO_Transp_Depth", TextureTarget.Texture2D);
        GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.DepthComponent32, Config.Width, Config.Height, 0, PixelFormat.DepthComponent, PixelType.Float, IntPtr.Zero);
        GraphicsUtil.TexParamLinearClamp();
        Internal.FBO_Transparents_Main = GL.GenFramebuffer();
        View.BindFramebuffer(FramebufferTarget.Framebuffer, Internal.FBO_Transparents_Main);
        GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, TextureTarget.Texture2D, Internal.FBO_Transparents_Texture.ID, 0);
        GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.DepthAttachment, TextureTarget.Texture2D, Internal.FBO_Transparents_DepthTexture.ID, 0);
        View.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
        GraphicsUtil.BindTexture(TextureTarget.Texture2D, 0);
        // Linked list stuff
        // TODO: Regeneratable, on window resize in particular.
        if (Config.LLActive)
        {
            // TODO: If was active, delete old data
            GL.ActiveTexture(TextureUnit.Texture4);
            GraphicsUtil.TrackedTexture temp = new("View3DGenHelper_LL_TranspText0", TextureTarget.Texture2DArray);
            GL.TexImage3D(TextureTarget.Texture2DArray, 0, PixelInternalFormat.R32f, Config.Width, Config.Height, 3, 0, PixelFormat.Red, PixelType.Float, IntPtr.Zero);
            GL.TexParameter(TextureTarget.Texture2DArray, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2DArray, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
            GL.BindImageTexture(4, temp.ID, 0, false, 0, TextureAccess.ReadWrite, SizedInternalFormat.R32ui);
            Internal.LL_TransparentTextures[0] = temp;
            GenBuffer(1, false);
            GenBuffer(2, true);
            GL.ActiveTexture(TextureUnit.Texture7);
            // TODO: Coherent names
            uint cspb = GraphicsUtil.GenBuffer("View3DGenerationHelper_cspb", BufferTarget.ArrayBuffer);
            GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)sizeof(uint), IntPtr.Zero, BufferUsageHint.StaticDraw);
            GraphicsUtil.TrackedTexture csp = new("View3DGenHelper_LL_csp", TextureTarget.TextureBuffer);
            GL.TexBuffer(TextureBufferTarget.TextureBuffer, SizedInternalFormat.R32f, cspb);
            GL.BindImageTexture(5, csp.ID, 0, false, 0, TextureAccess.ReadWrite, SizedInternalFormat.R32ui);
            Internal.LL_TransparentTextures[3] = csp;
            GraphicsUtil.BindTexture(TextureTarget.TextureBuffer, 0);
            GL.ActiveTexture(TextureUnit.Texture0);
        }
    }

    /// <summary>Internal call to generate a buffer, for the Linked-List Transparency trick.</summary>
    /// <param name="c">The texture target ID.</param>
    /// <param name="flip">Whether it needs to be flipped.</param>
    /// <returns>The buffer.</returns>
    public uint GenBuffer(uint c, bool flip)
    {
        GL.ActiveTexture(TextureUnit.Texture4 + (int)c);
        uint temp = GraphicsUtil.GenBuffer("View3DGenerationHelper_LL_GenBuffer", BufferTarget.TextureBuffer);
        GL.BufferData(BufferTarget.TextureBuffer, (IntPtr)(flip ? View3DInternalData.LL_AB_SIZE / View3DInternalData.LL_P_SIZE * sizeof(uint) : View3DInternalData.LL_AB_SIZE * sizeof(float) * 4), IntPtr.Zero, BufferUsageHint.StaticDraw);
        GraphicsUtil.TrackedTexture ttex = new("View3DGenHelper_LL_TTex", TextureTarget.TextureBuffer);
        GL.TexBuffer(TextureBufferTarget.TextureBuffer, flip ? SizedInternalFormat.R32f : SizedInternalFormat.Rgba32f, temp);
        GL.BindImageTexture(4 + c, ttex.ID, 0, false, 0, TextureAccess.ReadWrite, flip ? SizedInternalFormat.R32ui : SizedInternalFormat.Rgba32f);
        Internal.LL_TransparentTextures[c] = ttex;
        //GraphicsUtil.BindTexture(TextureTarget.TextureBuffer, 0);
        return temp;
    }

    /// <summary>Internal call to generate light helpers.</summary>
    public void GenerateLightHelpers()
    {
        GraphicsUtil.CheckError("Load - View3D - Pre");
        if (State.DeferredTarget != null)
        {
            State.DeferredTarget.Destroy();
            GL.DeleteFramebuffer(Internal.FBO_Screen_Main);
            Internal.FBO_Screen_Texture.Dispose();
            State.DeferredTarget = null;
            Internal.FBO_Screen_Main = 0;
            Internal.FBO_Screen_Main = 0;
            GraphicsUtil.CheckError("Load - View3D - Light - Deletes - 1");
            GL.DeleteFramebuffer(Internal.FBO_GodRay_Main);
            Internal.FBO_GodRay_Texture.Dispose();
            Internal.FBO_GodRay_Texture_2.Dispose();
            GL.DeleteFramebuffer(Internal.FBO_DynamicExposure);
            Internal.FBO_DynamicExposure_Texture.Dispose();
            GraphicsUtil.CheckError("Load - View3D - Light - Deletes - 2");
            GL.DeleteFramebuffers(View3DInternalData.SHADOW_BITS_MAX + 1, Internal.FBO_Shadow);
            GraphicsUtil.CheckError("Load - View3D - Light - Deletes - 3");
            Internal.FBO_Shadow_ColorTexture.Dispose();
            Internal.FBO_Shadow_DepthTexture.Dispose();
            GL.DeleteFramebuffer(Internal.FBO_Decal);
            Internal.FBO_Decal_Texture.Dispose();
            Internal.FBO_Decal_Depth.Dispose();
            GraphicsUtil.CheckError("Load - View3D - Light - Deletes - 4");
        }
        GraphicsUtil.CheckError("Load - View3D - Light - Deletes");
        State.DeferredTarget = new DeferredRenderTarget(Config.Width, Config.Height, Engine.Rendering, View);
        // FBO
        Internal.FBO_Screen_Main = GL.GenFramebuffer();
        Internal.FBO_Screen_Texture = new("View3DGenHelper_FBO_Screen_Texture", TextureTarget.Texture2D);
        GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba32f, Config.Width, Config.Height, 0, PixelFormat.Bgra, PixelType.UnsignedByte, IntPtr.Zero);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);
        View.BindFramebuffer(FramebufferTarget.Framebuffer, Internal.FBO_Screen_Main);
        GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, TextureTarget.Texture2D, Internal.FBO_Screen_Texture.ID, 0);
        View.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
        GraphicsUtil.CheckError("Load - View3D - Light - FBO");
        // Godray FBO
        Internal.FBO_GodRay_Main = GL.GenFramebuffer();
        Internal.FBO_GodRay_Texture = new("View3DGenHelper_FBO_GodRay_Texture", TextureTarget.Texture2D);
        GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, Config.Width, Config.Height, 0, PixelFormat.Bgra, PixelType.UnsignedByte, IntPtr.Zero);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);
        Internal.FBO_GodRay_Texture_2 = new("View3DGenHelper_FBO_GodRay_Texture2", TextureTarget.Texture2D);
        GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, Config.Width, Config.Height, 0, PixelFormat.Bgra, PixelType.UnsignedByte, IntPtr.Zero);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);
        View.BindFramebuffer(FramebufferTarget.Framebuffer, Internal.FBO_GodRay_Main);
        GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, TextureTarget.Texture2D, Internal.FBO_GodRay_Texture.ID, 0);
        GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment1, TextureTarget.Texture2D, Internal.FBO_GodRay_Texture_2.ID, 0);
        View.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
        GraphicsUtil.BindTexture(TextureTarget.Texture2D, 0);
        GraphicsUtil.CheckError("Load - View3D - Light - Godray");
        // Dynamic Exposure FBO
        Internal.FBO_DynamicExposure = GL.GenFramebuffer();
        Internal.FBO_DynamicExposure_Texture = new("View3DGenHelper_FBO_DynamicExposure_Texture", TextureTarget.Texture2D);
        GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.R32f, View3DInternalData.DYNAMIC_EXPOSURE_SPREAD, View3DInternalData.DYNAMIC_EXPOSURE_SPREAD, 0, PixelFormat.Red, PixelType.Float, IntPtr.Zero);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);
        View.BindFramebuffer(FramebufferTarget.Framebuffer, Internal.FBO_DynamicExposure);
        GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, TextureTarget.Texture2D, Internal.FBO_DynamicExposure_Texture.ID, 0);
        View.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
        GraphicsUtil.CheckError("Load - View3D - Light - HDR");
        // Shadow FBO
        int sq = Config.ShadowTexSize();
        Internal.FBO_Shadow_DepthTexture = new("View3DGenHelper_FBO_Shadow_DepthArray", TextureTarget.Texture2DArray);
        GL.TexImage3D(TextureTarget.Texture2DArray, 0, PixelInternalFormat.DepthComponent, sq, sq, View3DInternalData.SHADOW_BITS_MAX + 1, 0, PixelFormat.DepthComponent, PixelType.Float, IntPtr.Zero);
        GL.TexParameter(TextureTarget.Texture2DArray, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
        GL.TexParameter(TextureTarget.Texture2DArray, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
        GL.TexParameter(TextureTarget.Texture2DArray, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
        GL.TexParameter(TextureTarget.Texture2DArray, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);
        GraphicsUtil.CheckError("Load - View3D - Light - Shadows");
        Internal.FBO_Shadow_ColorTexture = new("View3DGenHelper_FBO_Shadow_Color", TextureTarget.Texture2D);
        GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.R32f, sq, sq, 0, PixelFormat.Red, PixelType.Float, IntPtr.Zero);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);
        GraphicsUtil.BindTexture(TextureTarget.Texture2D, 0);
        GraphicsUtil.CheckError("Load - View3D - Light - ShadowColor");
        GL.GenFramebuffers(View3DInternalData.SHADOW_BITS_MAX + 1, Internal.FBO_Shadow);
        for (int i = 0; i < View3DInternalData.SHADOW_BITS_MAX + 1; i++)
        {
            View.BindFramebuffer(FramebufferTarget.Framebuffer, Internal.FBO_Shadow[i]);
            GL.FramebufferTextureLayer(FramebufferTarget.Framebuffer, FramebufferAttachment.DepthAttachment, Internal.FBO_Shadow_DepthTexture.ID, 0, i);
            GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, TextureTarget.Texture2D, Internal.FBO_Shadow_ColorTexture.ID, 0);
            GraphicsUtil.CheckError("Load - View3D - Light - LMAP:" + i);
        }
        View.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
        GraphicsUtil.CheckError("Load - View3D - Light - Final");
        Internal.FBO_Decal = GL.GenFramebuffer();
        Internal.FBO_Decal_Texture = new("View3DGenHelper_FBO_Decal_Texture", TextureTarget.Texture2D);
        GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba8, Config.Width, Config.Height, 0, PixelFormat.Bgra, PixelType.UnsignedByte, IntPtr.Zero);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);
        Internal.FBO_Decal_Depth = new("View3DGenHelper_FBO_Decal_Depth", TextureTarget.Texture2D);
        GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.DepthComponent32, Config.Width, Config.Height, 0, PixelFormat.DepthComponent, PixelType.Float, IntPtr.Zero);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);
        View.BindFramebuffer(FramebufferTarget.Framebuffer, Internal.FBO_Decal);
        GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.DepthAttachment, TextureTarget.Texture2D, Internal.FBO_Decal_Depth.ID, 0);
        GraphicsUtil.BindTexture(TextureTarget.Texture2D, 0);
        GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, TextureTarget.Texture2D, Internal.FBO_Decal_Texture.ID, 0);
        View.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
        GraphicsUtil.BindTexture(TextureTarget.Texture2D, 0);
        GraphicsUtil.CheckError("Load - View3D - Decal");
    }

    /// <summary>Helper to generate an FBO backend for the view, if not a main-screen view.</summary>
    public void GenerateFBO()
    {
        if (Internal.CurrentFBO != 0)
        {
            GL.DeleteFramebuffer(Internal.CurrentFBO);
            Internal.CurrentFBOTexture.Dispose();
            Internal.CurrentFBODepth.Dispose();
        }
        GraphicsUtil.CheckError("Load - View3D - GenFBO - Deletes");
        GL.ActiveTexture(TextureUnit.Texture0);
        Internal.CurrentFBOTexture = new("View3DGenHelper_CurrentFBOTexture", TextureTarget.Texture2D);
        GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, Config.Width, Config.Height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, IntPtr.Zero);
        GraphicsUtil.TexParamLinearClamp();
        Internal.CurrentFBODepth = new("View3DGenHelper_CurrentFBODepth", TextureTarget.Texture2D);
        GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.DepthComponent32, Config.Width, Config.Height, 0, PixelFormat.DepthComponent, PixelType.Float, IntPtr.Zero);
        GraphicsUtil.TexParamLinearClamp();
        Internal.CurrentFBO = GL.GenFramebuffer();
        View.BindFramebuffer(FramebufferTarget.Framebuffer, Internal.CurrentFBO);
        GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, TextureTarget.Texture2D, Internal.CurrentFBOTexture.ID, 0);
        GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.DepthAttachment, TextureTarget.Texture2D, Internal.CurrentFBODepth.ID, 0);
        View.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
        GraphicsUtil.BindTexture(TextureTarget.Texture2D, 0);
        GraphicsUtil.CheckError("Load - View3D - GenFBO");
    }

    /// <summary>Converts the next frame to a texture.</summary>
    /// <returns>The texture.</returns>
    public GraphicsUtil.TrackedTexture NextFrameToTexture()
    {
        if (Internal.FBO_NextFrame_Texture is not null)
        {
            return Internal.FBO_NextFrame_Texture;
        }
        if (Internal.Flash_Texture is not null)
        {
            return Internal.Flash_Texture;
        }
        GraphicsUtil.CheckError("View3D - NFTex - Pre");
        GL.ActiveTexture(TextureUnit.Texture0);
        Internal.FBO_NextFrame_Texture = new("View3DGenHelper_FBO_NextFrame_Texture", TextureTarget.Texture2D);
        GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, Config.Width, Config.Height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, IntPtr.Zero);
        GraphicsUtil.TexParamLinearClamp();
        Internal.FBO_NextFrame_DepthTexture = new("View3DGenHelper_FBO_NextFrame_Depth", TextureTarget.Texture2D);
        GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.DepthComponent32, Config.Width, Config.Height, 0, PixelFormat.DepthComponent, PixelType.Float, IntPtr.Zero);
        GraphicsUtil.TexParamLinearClamp();
        Internal.FBO_NextFrame = GL.GenFramebuffer();
        View.BindFramebuffer(FramebufferTarget.Framebuffer, Internal.FBO_NextFrame);
        GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, TextureTarget.Texture2D, Internal.FBO_NextFrame_Texture.ID, 0);
        GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.DepthAttachment, TextureTarget.Texture2D, Internal.FBO_NextFrame_DepthTexture.ID, 0);
        View.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
        GraphicsUtil.BindTexture(TextureTarget.Texture2D, 0);
        GraphicsUtil.CheckError("View3D - NFTex");
        return Internal.FBO_NextFrame_Texture;
    }

    /// <summary>Internal call to end a 'next frame' effect on any given render tick.</summary>
    /// <param name="pfbo">The previous FBO.</param>
    public void EndNF(int pfbo)
    {
        if (Internal.Flash_Texture is null)
        {
            return;
        }
        State.Flash_DurationLeft -= Engine.Delta;
        if (State.Flash_DurationLeft > 0)
        {
            Engine.Shaders.ColorMultShader.Bind();
            GL.ActiveTexture(TextureUnit.Texture0);
            Internal.Flash_Texture.Bind();
            float power = State.Flash_DurationLeft > 2.0 ? 1f : ((float)State.Flash_DurationLeft * 0.5f);
            Engine.Rendering.SetColor(new Vector4(1f, 1f, 1f, power), View);
            GL.Disable(EnableCap.DepthTest);
            GL.Disable(EnableCap.CullFace);
            GL.UniformMatrix4(1, false, ref View3DInternalData.SimpleOrthoMatrix);
            GL.UniformMatrix4(2, false, ref View3DInternalData.IdentityMatrix);
            Engine.Rendering.RenderRectangle(-1, -1, 1, 1);
            Engine.Textures.White.Bind();
            if (power < 1f)
            {
                Engine.Rendering.SetColor(new Vector4(1f, 1f, 1f, (1f - power) * power), View);
                Engine.Rendering.RenderRectangle(-1, -1, 1, 1);
            }
        }
        else
        {
            State.Flash_DurationLeft = 0;
            Internal.Flash_Texture.Dispose();
            Internal.Flash_Texture = null;
        }
        if (pfbo != 0)
        {
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
            GL.BindFramebuffer(FramebufferTarget.ReadFramebuffer, pfbo);
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, Internal.FBO_NextFrame);
            GL.BlitFramebuffer(0, 0, Config.Width, Config.Height, 0, 0, Config.Width, Config.Height, ClearBufferMask.ColorBufferBit, BlitFramebufferFilter.Nearest);
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
            GL.BindFramebuffer(FramebufferTarget.ReadFramebuffer, 0);
        }
        GL.DeleteFramebuffer(Internal.FBO_NextFrame);
        Internal.FBO_NextFrame_Texture.Dispose();
        Internal.FBO_NextFrame_DepthTexture.Dispose();
        Internal.CurrentFBO = pfbo;
        Internal.FBO_NextFrame = 0;
        Internal.FBO_NextFrame_DepthTexture = null;
        Internal.FBO_NextFrame_Texture = null;
    }

    /// <summary>
    /// Destroys the view.
    /// // TODO: Ensure that this fully destroys all data!
    /// </summary>
    public void Destroy()
    {
        if (Internal.CurrentFBO != 0)
        {
            GL.DeleteFramebuffer(Internal.CurrentFBO);
            Internal.CurrentFBOTexture.Dispose();
            Internal.CurrentFBODepth.Dispose();
        }
        if (Internal.FBO_Transparents_Main != 0)
        {
            GL.DeleteFramebuffer(Internal.FBO_Transparents_Main);
            Internal.FBO_Transparents_Texture.Dispose();
            Internal.FBO_Transparents_DepthTexture.Dispose();
        }
        if (State.DeferredTarget is not null)
        {
            State.DeferredTarget.Destroy();
            GL.DeleteFramebuffer(Internal.FBO_Screen_Main);
            Internal.FBO_Screen_Texture.Dispose();
            State.DeferredTarget = null;
            Internal.FBO_Screen_Main = 0;
            Internal.FBO_Screen_Texture = null;
            GraphicsUtil.CheckError("Load - View3D - Light - Deletes - 1");
            GL.DeleteFramebuffer(Internal.FBO_GodRay_Main);
            Internal.FBO_GodRay_Texture.Dispose();
            Internal.FBO_GodRay_Texture_2.Dispose();
            GL.DeleteFramebuffer(Internal.FBO_DynamicExposure);
            Internal.FBO_DynamicExposure_Texture.Dispose();
            GraphicsUtil.CheckError("Load - View3D - Light - Deletes - 2");
            GL.DeleteFramebuffers(View3DInternalData.SHADOW_BITS_MAX + 1, Internal.FBO_Shadow);
            GraphicsUtil.CheckError("Load - View3D - Light - Deletes - 3");
            Internal.FBO_Shadow_ColorTexture.Dispose();
            Internal.FBO_Shadow_DepthTexture.Dispose();
            GL.DeleteFramebuffer(Internal.FBO_Decal);
            Internal.FBO_Decal_Texture.Dispose();
            Internal.FBO_Decal_Depth.Dispose();
            GraphicsUtil.CheckError("Load - View3D - Light - Deletes - 4");
        }
    }
}
