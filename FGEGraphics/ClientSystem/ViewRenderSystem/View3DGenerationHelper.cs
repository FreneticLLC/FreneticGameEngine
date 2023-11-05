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
            GL.DeleteTexture(Internal.FBO_OutView_DepthTexture);
        }
        Internal.FBO_OutView_DepthTexture = GL.GenTexture();
        GL.BindTexture(TextureTarget.Texture2D, Internal.FBO_OutView_DepthTexture);
        GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.DepthComponent32, Config.Width, Config.Height, 0, PixelFormat.DepthComponent, PixelType.Float, IntPtr.Zero);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);
        Internal.FBO_OutView_Main = GL.GenFramebuffer();
        View.BindFramebuffer(FramebufferTarget.Framebuffer, Internal.FBO_OutView_Main);
        GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.DepthAttachment, TextureTarget.Texture2D, Internal.FBO_OutView_DepthTexture, 0);
        GL.ClearBuffer(ClearBuffer.Depth, 0, new float[] { 1f });
        View.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
        GL.BindTexture(TextureTarget.Texture2D, 0);
    }

    /// <summary>(Re-)Generate transparent helpers, internal call.</summary>
    public void GenerateTranspHelpers()
    {
        if (Internal.FBO_Transparents_Main != 0)
        {
            GL.DeleteFramebuffer(Internal.FBO_Transparents_Main);
            GL.DeleteTexture(Internal.FBO_Transparents_Texture);
            GL.DeleteTexture(Internal.FBO_Transparents_DepthTexture);
        }
        // TODO: Helper class!
        Internal.FBO_Transparents_Texture = GL.GenTexture();
        GL.BindTexture(TextureTarget.Texture2D, Internal.FBO_Transparents_Texture);
        GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba32f, Config.Width, Config.Height, 0, PixelFormat.Rgba, PixelType.Float, IntPtr.Zero);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);
        Internal.FBO_Transparents_DepthTexture = GL.GenTexture();
        GL.BindTexture(TextureTarget.Texture2D, Internal.FBO_Transparents_DepthTexture);
        GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.DepthComponent32, Config.Width, Config.Height, 0, PixelFormat.DepthComponent, PixelType.Float, IntPtr.Zero);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);
        Internal.FBO_Transparents_Main = GL.GenFramebuffer();
        View.BindFramebuffer(FramebufferTarget.Framebuffer, Internal.FBO_Transparents_Main);
        GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, TextureTarget.Texture2D, Internal.FBO_Transparents_Texture, 0);
        GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.DepthAttachment, TextureTarget.Texture2D, Internal.FBO_Transparents_DepthTexture, 0);
        View.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
        GL.BindTexture(TextureTarget.Texture2D, 0);
        // Linked list stuff
        // TODO: Regeneratable, on window resize in particular.
        if (Config.LLActive)
        {
            // TODO: If was active, delete old data
            GenTexture();
            GenBuffer(1, false);
            GenBuffer(2, true);
            GL.ActiveTexture(TextureUnit.Texture7);
            int cspb = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, cspb);
            GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)sizeof(uint), IntPtr.Zero, BufferUsageHint.StaticDraw);
            int csp = GL.GenTexture();
            GL.BindTexture(TextureTarget.TextureBuffer, csp);
            GL.TexBuffer(TextureBufferTarget.TextureBuffer, SizedInternalFormat.R32f, cspb);
            GL.BindImageTexture(5, csp, 0, false, 0, TextureAccess.ReadWrite, SizedInternalFormat.R32ui);
            Internal.LL_TransparentTextures[3] = csp;
            GL.BindTexture(TextureTarget.TextureBuffer, 0);
            GL.ActiveTexture(TextureUnit.Texture0);
        }
    }

    /// <summary>Internal call to generate a texture.</summary>
    /// <returns>The texture.</returns>
    public int GenTexture()
    {
        GL.ActiveTexture(TextureUnit.Texture4);
        int temp = GL.GenTexture();
        GL.BindTexture(TextureTarget.Texture2DArray, temp);
        GL.TexParameter(TextureTarget.Texture2DArray, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
        GL.TexParameter(TextureTarget.Texture2DArray, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
        GL.TexImage3D(TextureTarget.Texture2DArray, 0, PixelInternalFormat.R32f, Config.Width, Config.Height, 3, 0, PixelFormat.Red, PixelType.Float, IntPtr.Zero);
        GL.BindImageTexture(4, temp, 0, false, 0, TextureAccess.ReadWrite, SizedInternalFormat.R32ui);
        Internal.LL_TransparentTextures[0] = temp;
        //GL.BindTexture(TextureTarget.Texture2DArray, 0);
        return temp;
    }

    /// <summary>Internal call to generate a buffer, for the Linked-List Transparency trick.</summary>
    /// <param name="c">The ID.</param>
    /// <param name="flip">Whether it needs to be flipped.</param>
    /// <returns>The buffer.</returns>
    public int GenBuffer(int c, bool flip)
    {
        GL.ActiveTexture(TextureUnit.Texture4 + c);
        int temp = GL.GenBuffer();
        GL.BindBuffer(BufferTarget.TextureBuffer, temp);
        GL.BufferData(BufferTarget.TextureBuffer, (IntPtr)(flip ? View3DInternalData.LL_AB_SIZE / View3DInternalData.LL_P_SIZE * sizeof(uint) : View3DInternalData.LL_AB_SIZE * sizeof(float) * 4), IntPtr.Zero, BufferUsageHint.StaticDraw);
        int ttex = GL.GenTexture();
        GL.BindTexture(TextureTarget.TextureBuffer, ttex);
        GL.TexBuffer(TextureBufferTarget.TextureBuffer, flip ? SizedInternalFormat.R32f : SizedInternalFormat.Rgba32f, temp);
        GL.BindImageTexture(4 + c, ttex, 0, false, 0, TextureAccess.ReadWrite, flip ? SizedInternalFormat.R32ui : SizedInternalFormat.Rgba32f);
        Internal.LL_TransparentTextures[c] = ttex;
        //GL.BindTexture(TextureTarget.TextureBuffer, 0);
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
            GL.DeleteTexture(Internal.FBO_Screen_Texture);
            State.DeferredTarget = null;
            Internal.FBO_Screen_Main = 0;
            Internal.FBO_Screen_Main = 0;
            GraphicsUtil.CheckError("Load - View3D - Light - Deletes - 1");
            GL.DeleteFramebuffer(Internal.FBO_GodRay_Main);
            GL.DeleteTexture(Internal.FBO_GodRay_Texture);
            GL.DeleteTexture(Internal.FBO_GodRay_Texture_2);
            GL.DeleteFramebuffer(Internal.FBO_DynamicExposure);
            GL.DeleteTexture(Internal.FBO_DynamicExposure_Texture);
            GraphicsUtil.CheckError("Load - View3D - Light - Deletes - 2");
            GL.DeleteFramebuffers(View3DInternalData.SHADOW_BITS_MAX + 1, Internal.FBO_Shadow);
            GraphicsUtil.CheckError("Load - View3D - Light - Deletes - 3");
            GL.DeleteTexture(Internal.FBO_Shadow_ColorTexture);
            GL.DeleteTexture(Internal.FBO_Shadow_DepthTexture);
            GL.DeleteFramebuffer(Internal.FBO_Decal);
            GL.DeleteTexture(Internal.FBO_Decal_Texture);
            GL.DeleteTexture(Internal.FBO_Decal_Depth);
            GraphicsUtil.CheckError("Load - View3D - Light - Deletes - 4");
        }
        GraphicsUtil.CheckError("Load - View3D - Light - Deletes");
        State.DeferredTarget = new DeferredRenderTarget(Config.Width, Config.Height, Engine.Rendering, View);
        // FBO
        Internal.FBO_Screen_Texture = GL.GenTexture();
        Internal.FBO_Screen_Main = GL.GenFramebuffer();
        GL.BindTexture(TextureTarget.Texture2D, Internal.FBO_Screen_Texture);
        GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba32f, Config.Width, Config.Height, 0, PixelFormat.Bgra, PixelType.UnsignedByte, IntPtr.Zero);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);
        View.BindFramebuffer(FramebufferTarget.Framebuffer, Internal.FBO_Screen_Main);
        GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, TextureTarget.Texture2D, Internal.FBO_Screen_Texture, 0);
        View.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
        GraphicsUtil.CheckError("Load - View3D - Light - FBO");
        // Godray FBO
        Internal.FBO_GodRay_Texture = GL.GenTexture();
        Internal.FBO_GodRay_Texture_2 = GL.GenTexture();
        Internal.FBO_GodRay_Main = GL.GenFramebuffer();
        GL.BindTexture(TextureTarget.Texture2D, Internal.FBO_GodRay_Texture);
        GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, Config.Width, Config.Height, 0, PixelFormat.Bgra, PixelType.UnsignedByte, IntPtr.Zero);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);
        GL.BindTexture(TextureTarget.Texture2D, Internal.FBO_GodRay_Texture_2);
        GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, Config.Width, Config.Height, 0, PixelFormat.Bgra, PixelType.UnsignedByte, IntPtr.Zero);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);
        View.BindFramebuffer(FramebufferTarget.Framebuffer, Internal.FBO_GodRay_Main);
        GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, TextureTarget.Texture2D, Internal.FBO_GodRay_Texture, 0);
        GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment1, TextureTarget.Texture2D, Internal.FBO_GodRay_Texture_2, 0);
        View.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
        GL.BindTexture(TextureTarget.Texture2D, 0);
        GraphicsUtil.CheckError("Load - View3D - Light - Godray");
        // HDR FBO
        Internal.FBO_DynamicExposure_Texture = GL.GenTexture();
        Internal.FBO_DynamicExposure = GL.GenFramebuffer();
        GL.BindTexture(TextureTarget.Texture2D, Internal.FBO_DynamicExposure_Texture);
        GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.R32f, View3DInternalData.DYNAMIC_EXPOSURE_SPREAD, View3DInternalData.DYNAMIC_EXPOSURE_SPREAD, 0, PixelFormat.Red, PixelType.Float, IntPtr.Zero);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);
        View.BindFramebuffer(FramebufferTarget.Framebuffer, Internal.FBO_DynamicExposure);
        GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, TextureTarget.Texture2D, Internal.FBO_DynamicExposure_Texture, 0);
        View.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
        GraphicsUtil.CheckError("Load - View3D - Light - HDR");
        // Shadow FBO
        int sq = Config.ShadowTexSize();
        Internal.FBO_Shadow_DepthTexture = GL.GenTexture();
        GL.BindTexture(TextureTarget.Texture2DArray, Internal.FBO_Shadow_DepthTexture);
        GL.TexImage3D(TextureTarget.Texture2DArray, 0, PixelInternalFormat.DepthComponent, sq, sq, View3DInternalData.SHADOW_BITS_MAX + 1, 0, PixelFormat.DepthComponent, PixelType.Float, IntPtr.Zero);
        GL.TexParameter(TextureTarget.Texture2DArray, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
        GL.TexParameter(TextureTarget.Texture2DArray, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
        GL.TexParameter(TextureTarget.Texture2DArray, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
        GL.TexParameter(TextureTarget.Texture2DArray, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);
        GraphicsUtil.CheckError("Load - View3D - Light - Shadows");
        Internal.FBO_Shadow_ColorTexture = GL.GenTexture();
        GL.BindTexture(TextureTarget.Texture2D, Internal.FBO_Shadow_ColorTexture);
        GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.R32f, sq, sq, 0, PixelFormat.Red, PixelType.Float, IntPtr.Zero);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);
        GL.BindTexture(TextureTarget.Texture2D, 0);
        GraphicsUtil.CheckError("Load - View3D - Light - ShadowColor");
        GL.GenFramebuffers(View3DInternalData.SHADOW_BITS_MAX + 1, Internal.FBO_Shadow);
        for (int i = 0; i < View3DInternalData.SHADOW_BITS_MAX + 1; i++)
        {
            View.BindFramebuffer(FramebufferTarget.Framebuffer, Internal.FBO_Shadow[i]);
            GL.FramebufferTextureLayer(FramebufferTarget.Framebuffer, FramebufferAttachment.DepthAttachment, Internal.FBO_Shadow_DepthTexture, 0, i);
            GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, TextureTarget.Texture2D, Internal.FBO_Shadow_ColorTexture, 0);
            GraphicsUtil.CheckError("Load - View3D - Light - LMAP:" + i);
        }
        View.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
        GraphicsUtil.CheckError("Load - View3D - Light - Final");
        Internal.FBO_Decal = GL.GenFramebuffer();
        Internal.FBO_Decal_Texture = GL.GenTexture();
        GL.BindTexture(TextureTarget.Texture2D, Internal.FBO_Decal_Texture);
        GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba8, Config.Width, Config.Height, 0, PixelFormat.Bgra, PixelType.UnsignedByte, IntPtr.Zero);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);
        Internal.FBO_Decal_Depth = GL.GenTexture();
        GL.BindTexture(TextureTarget.Texture2D, Internal.FBO_Decal_Depth);
        GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.DepthComponent32, Config.Width, Config.Height, 0, PixelFormat.DepthComponent, PixelType.Float, IntPtr.Zero);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);
        View.BindFramebuffer(FramebufferTarget.Framebuffer, Internal.FBO_Decal);
        GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.DepthAttachment, TextureTarget.Texture2D, Internal.FBO_Decal_Depth, 0);
        GL.BindTexture(TextureTarget.Texture2D, 0);
        GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, TextureTarget.Texture2D, Internal.FBO_Decal_Texture, 0);
        View.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
        GL.BindTexture(TextureTarget.Texture2D, 0);
        GraphicsUtil.CheckError("Load - View3D - Decal");
    }

    /// <summary>Helper to generate an FBO backend for the view, if not a main-screen view.</summary>
    public void GenerateFBO()
    {
        if (Internal.CurrentFBO != 0)
        {
            GL.DeleteFramebuffer(Internal.CurrentFBO);
            GL.DeleteTexture(Internal.CurrentFBOTexture);
            GL.DeleteTexture(Internal.CurrentFBODepth);
        }
        GraphicsUtil.CheckError("Load - View3D - GenFBO - Deletes");
        GL.ActiveTexture(TextureUnit.Texture0);
        Internal.CurrentFBOTexture = GL.GenTexture();
        GL.BindTexture(TextureTarget.Texture2D, Internal.CurrentFBOTexture);
        GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, Config.Width, Config.Height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, IntPtr.Zero);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);
        Internal.CurrentFBODepth = GL.GenTexture();
        GL.BindTexture(TextureTarget.Texture2D, Internal.CurrentFBODepth);
        GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.DepthComponent32, Config.Width, Config.Height, 0, PixelFormat.DepthComponent, PixelType.Float, IntPtr.Zero);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);
        Internal.CurrentFBO = GL.GenFramebuffer();
        View.BindFramebuffer(FramebufferTarget.Framebuffer, Internal.CurrentFBO);
        GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, TextureTarget.Texture2D, Internal.CurrentFBOTexture, 0);
        GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.DepthAttachment, TextureTarget.Texture2D, Internal.CurrentFBODepth, 0);
        View.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
        GL.BindTexture(TextureTarget.Texture2D, 0);
        GraphicsUtil.CheckError("Load - View3D - GenFBO");
    }

    /// <summary>Converts the next frame to a texture.</summary>
    /// <returns>The texture.</returns>
    public int NextFrameToTexture()
    {
        if (Internal.FBO_NextFrame_Texture != 0)
        {
            return Internal.FBO_NextFrame_Texture;
        }
        if (Internal.Flash_Texture != 0)
        {
            return Internal.Flash_Texture;
        }
        GraphicsUtil.CheckError("View3D - NFTex - Pre");
        GL.ActiveTexture(TextureUnit.Texture0);
        Internal.FBO_NextFrame_Texture = GL.GenTexture();
        GL.BindTexture(TextureTarget.Texture2D, Internal.FBO_NextFrame_Texture);
        GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, Config.Width, Config.Height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, IntPtr.Zero);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);
        Internal.FBO_NextFrame_DepthTexture = GL.GenTexture();
        GL.BindTexture(TextureTarget.Texture2D, Internal.FBO_NextFrame_DepthTexture);
        GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.DepthComponent32, Config.Width, Config.Height, 0, PixelFormat.DepthComponent, PixelType.Float, IntPtr.Zero);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);
        Internal.FBO_NextFrame = GL.GenFramebuffer();
        View.BindFramebuffer(FramebufferTarget.Framebuffer, Internal.FBO_NextFrame);
        GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, TextureTarget.Texture2D, Internal.FBO_NextFrame_Texture, 0);
        GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.DepthAttachment, TextureTarget.Texture2D, Internal.FBO_NextFrame_DepthTexture, 0);
        View.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
        GL.BindTexture(TextureTarget.Texture2D, 0);
        GraphicsUtil.CheckError("View3D - NFTex");
        return Internal.FBO_NextFrame_Texture;
    }

    /// <summary>Internal call to end a 'next frame' effect on any given render tick.</summary>
    /// <param name="pfbo">The previous FBO.</param>
    public void EndNF(int pfbo)
    {
        if (Internal.Flash_Texture != 0)
        {
            State.Flash_DurationLeft -= Engine.Delta;
            if (State.Flash_DurationLeft > 0)
            {
                Engine.Shaders.ColorMultShader.Bind();
                GL.ActiveTexture(TextureUnit.Texture0);
                GL.BindTexture(TextureTarget.Texture2D, Internal.Flash_Texture);
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
                GL.DeleteTexture(Internal.Flash_Texture);
                Internal.Flash_Texture = 0;
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
            GL.DeleteTexture(Internal.FBO_NextFrame_DepthTexture);
            Internal.CurrentFBO = pfbo;
            Internal.FBO_NextFrame = 0;
            Internal.FBO_NextFrame_DepthTexture = 0;
            Internal.FBO_NextFrame_Texture = 0;
        }
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
            GL.DeleteTexture(Internal.CurrentFBOTexture);
            GL.DeleteTexture(Internal.CurrentFBODepth);
        }
        if (Internal.FBO_Transparents_Main != 0)
        {
            GL.DeleteFramebuffer(Internal.FBO_Transparents_Main);
            GL.DeleteTexture(Internal.FBO_Transparents_Texture);
            GL.DeleteTexture(Internal.FBO_Transparents_DepthTexture);
        }
        if (State.DeferredTarget != null)
        {
            State.DeferredTarget.Destroy();
            GL.DeleteFramebuffer(Internal.FBO_Screen_Main);
            GL.DeleteTexture(Internal.FBO_Screen_Texture);
            State.DeferredTarget = null;
            Internal.FBO_Screen_Main = 0;
            Internal.FBO_Screen_Texture = 0;
            GraphicsUtil.CheckError("Load - View3D - Light - Deletes - 1");
            GL.DeleteFramebuffer(Internal.FBO_GodRay_Main);
            GL.DeleteTexture(Internal.FBO_GodRay_Texture);
            GL.DeleteTexture(Internal.FBO_GodRay_Texture_2);
            GL.DeleteFramebuffer(Internal.FBO_DynamicExposure);
            GL.DeleteTexture(Internal.FBO_DynamicExposure_Texture);
            GraphicsUtil.CheckError("Load - View3D - Light - Deletes - 2");
            GL.DeleteFramebuffers(View3DInternalData.SHADOW_BITS_MAX + 1, Internal.FBO_Shadow);
            GraphicsUtil.CheckError("Load - View3D - Light - Deletes - 3");
            GL.DeleteTexture(Internal.FBO_Shadow_ColorTexture);
            GL.DeleteTexture(Internal.FBO_Shadow_DepthTexture);
            GL.DeleteFramebuffer(Internal.FBO_Decal);
            GL.DeleteTexture(Internal.FBO_Decal_Texture);
            GL.DeleteTexture(Internal.FBO_Decal_Depth);
            GraphicsUtil.CheckError("Load - View3D - Light - Deletes - 4");
        }
    }
}
