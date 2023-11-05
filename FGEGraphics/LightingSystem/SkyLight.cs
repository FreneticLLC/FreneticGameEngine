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
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace FGEGraphics.LightingSystem;

/// <summary>Represents a light from the sky.</summary>
public class SkyLight : LightObject
{
    /// <summary>The radius of the effect of the light (vertical).</summary>
    public float Radius;

    /// <summary>The color of the light.</summary>
    public Location Color;

    /// <summary>The direction of the light.</summary>
    public Location Direction;

    /// <summary>The width of effect of the light (horizontal).</summary>
    public float Width;

    /// <summary>The FrameBufferObject.</summary>
    public int FBO = -1;

    /// <summary>The FBO Texture.</summary>
    public int FBO_Tex = -1;

    /// <summary>The FBO Depth Texture.</summary>
    public int FBO_DepthTex = -1;

    /// <summary>The width of the shadow texture.</summary>
    public int TexWidth = 0;

    /// <summary>Constructs the sky light.</summary>
    /// <param name="pos">The position.</param>
    /// <param name="radius">The radius (vertical).</param>
    /// <param name="col">The color.</param>
    /// <param name="dir">The direction.</param>
    /// <param name="size">Effective size (horizontal).</param>
    /// <param name="transp">Whether to include transparents for shadow effects.</param>
    /// <param name="twidth">The shadow texture width.</param>
    public SkyLight(Location pos, float radius, Location col, Location dir, float size, bool transp, int twidth)
    {
        EyePos = pos;
        Radius = radius;
        Color = col;
        Width = size;
        InternalLights.Add(new LightOrtho());
        if (dir.Z >= 0.99 || dir.Z <= -0.99)
        {
            InternalLights[0].UpVector = new Vector3(0, 1, 0);
        }
        else
        {
            InternalLights[0].UpVector = new Vector3(0, 0, 1);
        }
        InternalLights[0].TransparentShadows = transp;
        Direction = dir;
        InternalLights[0].Create(pos.ToOpenTK3D(), (pos + dir).ToOpenTK3D(), Width, Radius, Color.ToOpenTK());
        MaxDistance = radius;
        TexWidth = twidth;
        FBO = GL.GenFramebuffer();
        FBO_Tex = GL.GenTexture();
        GL.BindTexture(TextureTarget.Texture2D, FBO_Tex);
        GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.R32f, TexWidth, TexWidth, 0, PixelFormat.Bgra, PixelType.UnsignedByte, IntPtr.Zero);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);
        FBO_DepthTex = GL.GenTexture();
        GL.BindTexture(TextureTarget.Texture2D, FBO_DepthTex);
        GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.DepthComponent32, TexWidth, TexWidth, 0, PixelFormat.DepthComponent, PixelType.Float, IntPtr.Zero);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);
        GL.BindFramebuffer(FramebufferTarget.Framebuffer, FBO);
        GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, TextureTarget.Texture2D, FBO_Tex, 0);
        GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.DepthAttachment, TextureTarget.Texture2D, FBO_DepthTex, 0);
        GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
    }

    /// <summary>Destroys the sky light.</summary>
    public void Destroy()
    {
        InternalLights[0].Destroy();
        GL.DeleteFramebuffer(FBO);
        GL.DeleteTexture(FBO_Tex);
        GL.DeleteTexture(FBO_DepthTex);
    }

    /// <summary>Repositions the sky light.</summary>
    /// <param name="pos">New position.</param>
    public override void Reposition(Location pos)
    {
        EyePos = pos;
        InternalLights[0].NeedsUpdate = true;
        InternalLights[0].EyePosition = EyePos.ToOpenTK3D();
        InternalLights[0].TargetPosition = (EyePos + Direction).ToOpenTK3D();
    }
}
