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
using FGECore;
using FGECore.CoreSystems;
using FGECore.MathHelpers;
using FGEGraphics.ClientSystem;
using FGEGraphics.ClientSystem.EntitySystem;
using FGEGraphics.ClientSystem.ViewRenderSystem;
using FGEGraphics.GraphicsHelpers;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace FGEGraphics.LightingSystem;

/// <summary>Represents a point-light in 2D spaces.</summary>
public class PointLight2D
{
    /// <summary>The powering 2D game engine.</summary>
    public GameEngine2D Engine;

    /// <summary>Constructs the point light 2D.</summary>
    /// <param name="pos">Its starting position.</param>
    /// <param name="str">Its strength.</param>
    /// <param name="sdscale">The subdivision scale.</param>
    /// <param name="_engine">The powering game engine.</param>
    public PointLight2D(Vector2 pos, float str, float sdscale, GameEngine2D _engine)
    {
        if (str < 1)
        {
            throw new Exception("Invalid strength!");
        }
        Engine = _engine;
        Position = pos;
        Strength = str;
        FBO = GL.GenFramebuffer();
        GL.BindFramebuffer(FramebufferTarget.Framebuffer, FBO);
        FBO_Tex = GL.GenTexture();
        if (Engine.OneDLights)
        {
            Width = 8192;
            GL.BindTexture(TextureTarget.Texture1D, FBO_Tex);
            GL.TexImage1D(TextureTarget.Texture1D, 0, PixelInternalFormat.R32f, Width, 0, PixelFormat.Red, PixelType.Float, IntPtr.Zero);
            GL.TexParameter(TextureTarget.Texture1D, TextureParameterName.TextureMinFilter, (uint)TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.Texture1D, TextureParameterName.TextureMagFilter, (uint)TextureMagFilter.Linear);
            GL.TexParameter(TextureTarget.Texture1D, TextureParameterName.TextureWrapS, (uint)TextureWrapMode.ClampToEdge);
            GL.TexParameter(TextureTarget.Texture1D, TextureParameterName.TextureWrapT, (uint)TextureWrapMode.ClampToEdge);
            GL.FramebufferTexture1D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, TextureTarget.Texture1D, FBO_Tex, 0);
            GraphicsUtil.CheckError("PointLight2D init - 1D Tex");
            FBO_DTex = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture1D, FBO_DTex);
            GL.TexImage1D(TextureTarget.Texture1D, 0, PixelInternalFormat.DepthComponent, Width, 0, PixelFormat.DepthComponent, PixelType.Float, IntPtr.Zero);
            GraphicsUtil.CheckError("PointLight2D init - 1D DTex - Tex");
            GL.TexParameter(TextureTarget.Texture1D, TextureParameterName.TextureMinFilter, (uint)TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.Texture1D, TextureParameterName.TextureMagFilter, (uint)TextureMagFilter.Linear);
            GL.TexParameter(TextureTarget.Texture1D, TextureParameterName.TextureWrapS, (uint)TextureWrapMode.ClampToEdge);
            GL.TexParameter(TextureTarget.Texture1D, TextureParameterName.TextureWrapT, (uint)TextureWrapMode.ClampToEdge);
            GraphicsUtil.CheckError("PointLight2D init - 1D DTex");
            GL.FramebufferTexture1D(FramebufferTarget.Framebuffer, FramebufferAttachment.DepthAttachment, TextureTarget.Texture1D, FBO_DTex, 0);
            GraphicsUtil.CheckError("PointLight2D init - 1D DTex Attach");
        }
        else
        {
            GL.BindTexture(TextureTarget.Texture2D, FBO_Tex);
            // TODO: Utilities.NextPowerOfTwo? Should probably only be added if it's confirmed as need (POT-only hardware on OpenGL 4.3 is unlikely... NPOTs are common!)
            Width = (int)(Strength * 2f);
            // TODO: Alpha texture!?
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, Width, Width, 0, PixelFormat.Rgba, PixelType.UnsignedByte, IntPtr.Zero);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (uint)TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (uint)TextureMagFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (uint)TextureWrapMode.ClampToEdge);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (uint)TextureWrapMode.ClampToEdge);
            GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, TextureTarget.Texture2D, FBO_Tex, 0);
        }
        GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
        GL.BindTexture(TextureTarget.Texture2D, 0);
        SubDivider = Math.Max((float)Math.Sqrt(Strength) * sdscale, 1f);
        GraphicsUtil.CheckError("PointLight2D init");
    }

    /// <summary>Destroys the light object.</summary>
    public void Destroy()
    {
        GL.DeleteTexture(FBO_Tex);
        if (FBO_DTex > 0)
        {
            GL.DeleteTexture(FBO_DTex);
        }
        GL.DeleteFramebuffer(FBO);
        GraphicsUtil.CheckError("PointLight2D destroy");
    }

    /// <summary>The position of the point light 2D, in 2D world space.</summary>
    public Vector2 Position;

    /// <summary>The strength (how far it goes) of the point light 2D.</summary>
    public float Strength;

    /// <summary>The FrameBufferObject used by this Point Light 2D.</summary>
    public int FBO;

    /// <summary>The FrameBufferObject texture used by this Point Light 2D.</summary>
    public int FBO_Tex;

    /// <summary>The FrameBufferObject depth-texture used by this point light 1d_2d.</summary>
    public int FBO_DTex;

    /// <summary>The maximum width of this point light 2D's effects.</summary>
    public int Width;

    /// <summary>The subdivider to affect the render detail of this point light 2D.</summary>
    public float SubDivider = 4;

    /// <summary>Extra light distance (affects how deep into an object a light will still show at).</summary>
    public float ExtraLightDist = 50.0f;

    /// <summary>Whether this is a 'sky' light.</summary>
    public bool IsSkyLight = false;

    /// <summary>Set to configure whether an entity should cast a shadow from this light.</summary>
    public Func<ClientEntity, bool> ShouldShadow;

    /// <summary>The color of this point light 2D.</summary>
    public Color4F Color = Color4F.White;

    /// <summary>Prepares the point light 2D for rendering.</summary>
    public void PrepareLightmap()
    {
        GL.BindFramebuffer(FramebufferTarget.Framebuffer, FBO);
        GL.ClearBuffer(ClearBuffer.Color, 0, Engine.OneDLights ? View3DInternalData.ARR_FLOAT_1F_1 : [0, 0, 0, 0]);
        GL.Viewport(0, 0, Width, Engine.OneDLights ? 1 : Width);
    }

    /// <summary>Gets the scaler value of this Point Light 2D.</summary>
    /// <returns>The scaler.</returns>
    public Vector2 GetScaler()
    {
        if (IsSkyLight)
        {
            Strength = 1.0f / (Math.Min(Engine.OriginalScaler.X, Engine.OriginalScaler.Y) * 0.05f);
        }
        float sc = 1.0f / (Strength);
        return new Vector2(sc, sc);
    }

    /// <summary>Gets the adder value of this Point Light 2D.</summary>
    /// <returns>The adder.</returns>
    public Vector2 GetAdder()
    {
        if (IsSkyLight)
        {
            Position = -Engine.OriginalAdder + (new Vector2(1.0f / (Engine.OriginalScaler.X), 1.0f / (Engine.OriginalScaler.Y * 0.25f)));
        }
        return new Vector2(-Position.X, -Position.Y);
    }

    /// <summary>Gets the secondary scaler value of this Point Light 2D.</summary>
    /// <returns>The second scaler.</returns>
    public Vector2 GetSecondScaler(RenderContext2D rc)
    {
        float sc = (rc.Zoom * rc.ZoomMultiplier);
        float sc_over_str = sc / Strength;
        return new Vector2(sc_over_str, sc_over_str);
    }

    /// <summary>Gets the secondary adder value of this Point Light 2D.</summary>
    /// <returns>The second adder.</returns>
    public Vector2 GetSecondAdder(RenderContext2D rc)
    {
        float one_over_str = 1f / Strength;
        float adx = (-rc.ViewCenter.X - Position.X) * one_over_str;
        float ady = (-rc.ViewCenter.Y - Position.Y) * one_over_str * rc.AspectHelper;
        return new Vector2(adx, ady);
    }
}
