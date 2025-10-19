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
using FGEGraphics.ClientSystem.ViewRenderSystem;
using OpenTK.Graphics.OpenGL4;

namespace FGEGraphics.GraphicsHelpers;

/// <summary>A rendering surface for deferred rendering logic (currently holds 5 RGBA components + depth).</summary>
public class DeferredRenderTarget
{
    /// <summary>The width.</summary>
    public int Width;

    /// <summary>The height.</summary>
    public int Height;

    /// <summary>OpenGL FBO.</summary>
    public uint FBO;

    /// <summary>OpenGL diffuse texture.</summary>
    public GraphicsUtil.TrackedTexture DiffuseTexture;

    /// <summary>OpenGL position texture.</summary>
    public GraphicsUtil.TrackedTexture PositionTexture;

    /// <summary>OpenGL normals texture.</summary>
    public GraphicsUtil.TrackedTexture NormalsTexture;

    /// <summary>OpenGL depth texture.</summary>
    public GraphicsUtil.TrackedTexture DepthTexture;

    /// <summary>OpenGL Render hint texture.</summary>
    public GraphicsUtil.TrackedTexture RenderhintTexture;

    /// <summary>OpenGL render hint (2) texture.</summary>
    public GraphicsUtil.TrackedTexture Rh2Texture;

    /// <summary>Render helper.</summary>
    public Renderer Rendering;

    /// <summary>Constructs the <see cref="DeferredRenderTarget"/>.</summary>
    /// <param name="_width">Texture width.</param>
    /// <param name="_height">Texture height.</param>
    /// <param name="rendering">Render helper.</param>
    /// <param name="view">View system.</param>
    public DeferredRenderTarget(int _width, int _height, Renderer rendering, View3D view)
    {
        Rendering = rendering;
        Width = _width;
        Height = _height;
        GL.GenFramebuffers(1, out FBO);
        GL.BindFramebuffer(FramebufferTarget.Framebuffer, FBO);
        DiffuseTexture = new("DeferredRenderTarget_Diffuse", TextureTarget.Texture2D);
        GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, Width, Height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, IntPtr.Zero);
        GraphicsUtil.TexParamLinearClamp();
        GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, TextureTarget.Texture2D, DiffuseTexture.ID, 0);
        PositionTexture = new("DeferredRenderTarget_Position", TextureTarget.Texture2D);
        GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba32f, Width, Height, 0, PixelFormat.Rgba, PixelType.Float, IntPtr.Zero);
        GraphicsUtil.TexParamLinearClamp();
        GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment1, TextureTarget.Texture2D, PositionTexture.ID, 0);
        NormalsTexture = new("DeferredRenderTarget_Normals", TextureTarget.Texture2D);
        GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba16f, Width, Height, 0, PixelFormat.Rgba, PixelType.Float, IntPtr.Zero);
        GraphicsUtil.TexParamLinearClamp();
        GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment2, TextureTarget.Texture2D, NormalsTexture.ID, 0);
        DepthTexture = new("DeferredRenderTarget_Depth", TextureTarget.Texture2D);
        GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.DepthComponent32, Width, Height, 0, PixelFormat.DepthComponent, PixelType.Float, IntPtr.Zero);
        GraphicsUtil.TexParamLinearClamp();
        GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.DepthAttachment, TextureTarget.Texture2D, DepthTexture.ID, 0);
        RenderhintTexture = new("DeferredRenderTarget_Renderhint", TextureTarget.Texture2D);
        GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba16f, Width, Height, 0, PixelFormat.Rgba, PixelType.Float, IntPtr.Zero);
        GraphicsUtil.TexParamLinearClamp();
        GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment3, TextureTarget.Texture2D, RenderhintTexture.ID, 0);
        Rh2Texture = new("DeferredRenderTarget_Renderhint2", TextureTarget.Texture2D);
        GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba16f, Width, Height, 0, PixelFormat.Rgba, PixelType.Float, IntPtr.Zero);
        GraphicsUtil.TexParamLinearClamp();
        GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment4, TextureTarget.Texture2D, Rh2Texture.ID, 0);
        view.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
    }

    /// <summary>Destroys the RS4P.</summary>
    public void Destroy()
    {
        GL.DeleteFramebuffer(FBO);
        GraphicsUtil.CheckError("RS4P - Destroy - 0.1");
        DiffuseTexture.Dispose();
        GraphicsUtil.CheckError("RS4P - Destroy - 0.2");
        PositionTexture.Dispose();
        GraphicsUtil.CheckError("RS4P - Destroy - 0.3");
        NormalsTexture.Dispose();
        GraphicsUtil.CheckError("RS4P - Destroy - 0.5");
        DepthTexture.Dispose();
        GraphicsUtil.CheckError("RS4P - Destroy - 0.6");
        RenderhintTexture.Dispose();
        GraphicsUtil.CheckError("RS4P - Destroy - 0.7");
        Rh2Texture.Dispose();
        GraphicsUtil.CheckError("RS4P - Destroy");
    }

    /// <summary>Whether this RS4P is bound.</summary>
    public bool IsBound = false;

    static readonly DrawBuffersEnum[] DRAW_BUFFERS_ARRAY = [ DrawBuffersEnum.ColorAttachment0, DrawBuffersEnum.ColorAttachment1,
            DrawBuffersEnum.ColorAttachment2, DrawBuffersEnum.ColorAttachment3, DrawBuffersEnum.ColorAttachment4, DrawBuffersEnum.ColorAttachment5 ];

    /// <summary>Sets the proper "GL.DrawBuffers(6, ..." for this object.</summary>
    public static void SetDrawBuffers()
    {
        GL.DrawBuffers(6, DRAW_BUFFERS_ARRAY);
    }

    /// <summary>Binds the RS4P to OpenGL and a view.</summary>
    /// <param name="view">The view.</param>
    public void Bind(View3D view)
    {
        IsBound = true;
        view.State.BufferDontTouch = true;
        view.BindFramebuffer(FramebufferTarget.Framebuffer, (int)FBO);
        view.Viewport(0, 0, Width, Height);
        SetDrawBuffers();
        //GL.BlendFunc(3, BlendingFactorSrc.One, BlendingFactorDest.Zero);
        GL.ActiveTexture(TextureUnit.Texture0);
        //GL.Enable(EnableCap.Texture2D);
        GraphicsUtil.CheckError("Deferred Target - Bind");
    }

    /// <summary>Internal data useful to <see cref="DeferredRenderTarget"/>.</summary>
    public static class Internal
    {
        /// <summary>Premade array of 4 zeroes, for <see cref="Clear"/>.</summary>
        public static float[] Zeroes = [0f, 0f, 0f, 0f];

        /// <summary>Premade array of 1 one, for <see cref="Clear"/>.</summary>
        public static float[] One = [1f];
    }


    /// <summary>Clears the RS4P Buffers.</summary>
    public static void Clear()
    {
        GL.ClearBuffer(ClearBuffer.Color, 0, Internal.Zeroes);
        GL.ClearBuffer(ClearBuffer.Depth, 0, Internal.One);
        GL.ClearBuffer(ClearBuffer.Color, 1, Internal.Zeroes);
        GL.ClearBuffer(ClearBuffer.Color, 2, Internal.Zeroes);
        GL.ClearBuffer(ClearBuffer.Color, 3, Internal.Zeroes);
        GL.ClearBuffer(ClearBuffer.Color, 4, Internal.Zeroes);
        GL.ClearBuffer(ClearBuffer.Color, 5, Internal.Zeroes);
        GraphicsUtil.CheckError("DeferredRenderTarget - Clear");
    }

    /// <summary>Unbinds the RS4P from OpenGL and a view.</summary>
    /// <param name="view">The view.</param>
    public void Unbind(View3D view)
    {
        IsBound = false;
        view.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
        view.Viewport(0, 0, view.Config.Width, view.Config.Height);
        GL.DrawBuffer(DrawBufferMode.Back);
        view.State.BufferDontTouch = false;
    }
}
