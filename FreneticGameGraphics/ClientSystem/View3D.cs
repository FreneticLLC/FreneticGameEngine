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
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;
using System.Runtime.CompilerServices;
using FreneticGameGraphics.GraphicsHelpers;
using System.Diagnostics;
using FreneticGameCore;
using FreneticGameCore.CoreSystems;
using FreneticGameCore.MathHelpers;
using FreneticGameGraphics.LightingSystem;
using FreneticGameCore.UtilitySystems;

namespace FreneticGameGraphics.ClientSystem
{
    /// <summary>
    /// Represents a 3D view port.
    /// <para>Call <see cref="Generate(GameEngine3D, int, int)"/></para>
    /// <para>Set <see cref="Render3D"/>, <see cref="PostFirstRender"/>, ...</para>
    /// <para>Call <see cref="Render"/> every frame.</para>
    /// </summary>
    public class View3D
    {
        /// <summary>
        /// The backing 3D engine.
        /// </summary>
        public GameEngine3D Engine;

        /// <summary>
        /// Generates the View3D.
        /// </summary>
        /// <param name="tengine">Backing 3D engine.</param>
        /// <param name="w">Width.</param>
        /// <param name="h">Height.</param>
        public void Generate(GameEngine3D tengine, int w, int h)
        {
            Engine = tengine;
            Width = w;
            Height = h;
            GenerateLightHelpers();
            GraphicsUtil.CheckError("Load - View3D - Light");
            GenerateTranspHelpers();
            GraphicsUtil.CheckError("Load - View3D - Transp");
            GenerateOutDepthHelpers();
            GraphicsUtil.CheckError("Load - View3D - OV");
        }

        /// <summary>
        /// Whether the system is rendering things where specular value is relevant.
        /// </summary>
        public bool RenderSpecular = false;

        /// <summary>
        /// Set this to whatever method call renders all 3D objects in this view.
        /// </summary>
        public Action<View3D> Render3D = null;

        /// <summary>
        /// Whether shadows are the only thing to render.
        /// </summary>
        public bool ShadowsOnly = false;

        /// <summary>
        /// Whether shadows are allowed to be rendered.
        /// </summary>
        public bool ShadowingAllowed = true;

        /// <summary>
        /// Whether transparent objects can cast shadows ever (defaults true).
        /// </summary>
        public bool TranspShadows = true;

        /// <summary>
        /// Current lights count.
        /// </summary>
        public int LightsC = 0;

        /// <summary>
        /// Set this to whatever method call is needed after the solid rendering and we're switching to transparent rendering.
        /// </summary>
        public Action PostFirstRender = null;

        /// <summary>
        /// Set this to whatever method call renders all 3D decals in this view.
        /// </summary>
        public Action<View3D> DecalRender = null;

        /// <summary>
        /// Render target width.
        /// </summary>
        public int Width;

        /// <summary>
        /// Render target height.
        /// </summary>
        public int Height;

        /// <summary>
        /// Whether the system is rendering lighting effects.
        /// </summary>
        public bool RenderLights = false;

        /// <summary>
        /// Current fog color.
        /// </summary>
        public Location FogCol = new Location(0.7);

        /// <summary>
        /// All lights known to this view.
        /// </summary>
        public List<LightObject> Lights = new List<LightObject>();

        int transp_fbo_main = 0;
        int transp_fbo_texture = 0;
        int transp_fbo_depthtex = 0;

        /// <summary>
        /// Time statistic: Shadows.
        /// </summary>
        public double ShadowTime;

        /// <summary>
        /// Whether to not touch the buffer mode.
        /// </summary>
        public bool BufferDontTouch = false;

        /// <summary>
        /// Time statistic: FBO.
        /// </summary>
        public double FBOTime;

        /// <summary>
        /// Time statistic: Lights.
        /// </summary>
        public double LightsTime;

        /// <summary>
        /// Time statistic: Total.
        /// </summary>
        public double TotalTime;

        /// <summary>
        /// Time statistic (spike): Shadows.
        /// </summary>
        public double ShadowSpikeTime;

        /// <summary>
        /// Time statistic (spike): FBO.
        /// </summary>
        public double FBOSpikeTime;

        /// <summary>
        /// Time statistic (spike): Lights.
        /// </summary>
        public double LightsSpikeTime;

        /// <summary>
        /// Time statistic (spike): Total.
        /// </summary>
        public double TotalSpikeTime;

        /// <summary>
        /// The current FBO ID.
        /// </summary>
        public FBOID FBOid;

        /// <summary>
        /// Minimum sun light value.
        /// </summary>
        public float SunLight_Minimum = 0;

        /// <summary>
        /// Maximum sun light value.
        /// </summary>
        public float SunLight_Maximum = 1;

        /// <summary>
        /// Current HDR exposure value.
        /// </summary>
        public float MainEXP = 1.0f;

        /// <summary>
        /// The alpha value to clear to.
        /// </summary>
        public float RenderClearAlpha = 1f;

        /// <summary>
        /// The shadow blur factor for deferred mode.
        /// </summary>
        public float ShadowBlur = 0.25f;

        /// <summary>
        /// The strength factor for depth-of-field if enabled.
        /// </summary>
        public float DOF_Factor = 4;

        /// <summary>
        /// The target location, if DOF is used.
        /// </summary>
        public Location DOF_Target = Location.Zero;

        /// <summary>
        /// The location of the sun when available.
        /// </summary>
        public Location SunLocation = Location.NaN;

        /// <summary>
        /// Maximum view distance of small lights.
        /// </summary>
        public double LightMaxDistance = 200f;

        /// <summary>
        /// Out-View FBO.
        /// </summary>
        public int OV_FBO;

        /// <summary>
        /// Out-View depth texture.
        /// </summary>
        public int OV_DT;

        /// <summary>
        /// Generate helpers to deal with out-view depth logic.
        /// </summary>
        public void GenerateOutDepthHelpers()
        {
            if (OV_FBO != 0)
            {
                GL.DeleteFramebuffer(OV_FBO);
                GL.DeleteTexture(OV_DT);
            }
            OV_DT = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, OV_DT);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.DepthComponent32, Width, Height, 0, PixelFormat.DepthComponent, PixelType.Float, IntPtr.Zero);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);
            OV_FBO = GL.GenFramebuffer();
            BindFramebuffer(FramebufferTarget.Framebuffer, OV_FBO);
            GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.DepthAttachment, TextureTarget.Texture2D, OV_DT, 0);
            GL.ClearBuffer(ClearBuffer.Depth, 0, new float[] { 1f });
            BindFramebuffer(FramebufferTarget.Framebuffer, 0);
            GL.BindTexture(TextureTarget.Texture2D, 0);
        }

        /// <summary>
        /// (Re-)Generate transparent helpers, internal call.
        /// </summary>
        public void GenerateTranspHelpers()
        {
            if (transp_fbo_main != 0)
            {
                GL.DeleteFramebuffer(transp_fbo_main);
                GL.DeleteTexture(transp_fbo_texture);
                GL.DeleteTexture(transp_fbo_depthtex);
            }
            // TODO: Helper class!
            transp_fbo_texture = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, transp_fbo_texture);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba32f, Width, Height, 0, PixelFormat.Rgba, PixelType.Float, IntPtr.Zero);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);
            transp_fbo_depthtex = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, transp_fbo_depthtex);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.DepthComponent32, Width, Height, 0, PixelFormat.DepthComponent, PixelType.Float, IntPtr.Zero);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);
            transp_fbo_main = GL.GenFramebuffer();
            BindFramebuffer(FramebufferTarget.Framebuffer, transp_fbo_main);
            GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, TextureTarget.Texture2D, transp_fbo_texture, 0);
            GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.DepthAttachment, TextureTarget.Texture2D, transp_fbo_depthtex, 0);
            BindFramebuffer(FramebufferTarget.Framebuffer, 0);
            GL.BindTexture(TextureTarget.Texture2D, 0);
            // Linked list stuff
            // TODO: Regeneratable, on window resize in particular.
            if (LLActive)
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
                TransTexs[3] = csp;
                GL.BindTexture(TextureTarget.TextureBuffer, 0);
                GL.ActiveTexture(TextureUnit.Texture0);
            }
        }

        /// <summary>
        /// Whether LL tricks are enabled.
        /// </summary>
        public bool LLActive = false;

        /// <summary>
        /// Currently know transparente textures.
        /// </summary>
        int[] TransTexs = new int[4];

        /// <summary>
        /// Internal call to generate a texture.
        /// </summary>
        /// <returns>The texture.</returns>
        public int GenTexture()
        {
            GL.ActiveTexture(TextureUnit.Texture4);
            int temp = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2DArray, temp);
            GL.TexParameter(TextureTarget.Texture2DArray, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2DArray, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
            GL.TexImage3D(TextureTarget.Texture2DArray, 0, PixelInternalFormat.R32f, Width, Height, 3, 0, PixelFormat.Red, PixelType.Float, IntPtr.Zero);
            GL.BindImageTexture(4, temp, 0, false, 0, TextureAccess.ReadWrite, SizedInternalFormat.R32ui);
            TransTexs[0] = temp;
            //GL.BindTexture(TextureTarget.Texture2DArray, 0);
            return temp;
        }

        /// <summary>
        /// LL needed info.
        /// </summary>
        public int AB_SIZE = 8 * 1024 * 1024; // TODO: Tweak me!

        /// <summary>
        /// LL needed info.
        /// </summary>
        public const int P_SIZE = 4;

        /// <summary>
        /// Internal call to generate a buffer.
        /// </summary>
        /// <param name="c">The ID.</param>
        /// <param name="flip">Whether it needs to be flipped.</param>
        /// <returns>The buffer.</returns>
        public int GenBuffer(int c, bool flip)
        {
            GL.ActiveTexture(TextureUnit.Texture4 + c);
            int temp = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.TextureBuffer, temp);
            GL.BufferData(BufferTarget.TextureBuffer, (IntPtr)(flip ? AB_SIZE / P_SIZE * sizeof(uint) : AB_SIZE * sizeof(float) * 4), IntPtr.Zero, BufferUsageHint.StaticDraw);
            int ttex = GL.GenTexture();
            GL.BindTexture(TextureTarget.TextureBuffer, ttex);
            GL.TexBuffer(TextureBufferTarget.TextureBuffer, flip ? SizedInternalFormat.R32f : SizedInternalFormat.Rgba32f, temp);
            GL.BindImageTexture(4 + c, ttex, 0, false, 0, TextureAccess.ReadWrite, flip ? SizedInternalFormat.R32ui : SizedInternalFormat.Rgba32f);
            TransTexs[c] = ttex;
            //GL.BindTexture(TextureTarget.TextureBuffer, 0);
            return temp;
        }

        /// <summary>
        /// What range to spread HDR calculations by. Gets squared to estimate graphics cores involved in process.
        /// Have to edit shader hdrpass.fs to adjust this.
        /// </summary>
        const int HDR_SPREAD = 32;

        int hdrfbo;
        int hdrtex;

        /// <summary>
        /// Internal call to generate light helpers.
        /// </summary>
        public void GenerateLightHelpers()
        {
            GraphicsUtil.CheckError("Load - View3D - Pre");
            if (RS4P != null)
            {
                RS4P.Destroy();
                GL.DeleteFramebuffer(fbo_main);
                GL.DeleteTexture(fbo_texture);
                RS4P = null;
                fbo_main = 0;
                fbo_texture = 0;
                GraphicsUtil.CheckError("Load - View3D - Light - Deletes - 1");
                GL.DeleteFramebuffer(fbo_godray_main);
                GL.DeleteTexture(fbo_godray_texture);
                GL.DeleteTexture(fbo_godray_texture2);
                GL.DeleteFramebuffer(hdrfbo);
                GL.DeleteTexture(hdrtex);
                GraphicsUtil.CheckError("Load - View3D - Light - Deletes - 2");
                GL.DeleteFramebuffers(SHADOW_BITS_MAX + 1, fbo_shadow);
                GraphicsUtil.CheckError("Load - View3D - Light - Deletes - 3");
                GL.DeleteTexture(fbo_shadow_color);
                GL.DeleteTexture(fbo_shadow_tex);
                GL.DeleteFramebuffer(fbo_decal);
                GL.DeleteTexture(fbo_decal_tex);
                GL.DeleteTexture(fbo_decal_depth);
                GraphicsUtil.CheckError("Load - View3D - Light - Deletes - 4");
            }
            GraphicsUtil.CheckError("Load - View3D - Light - Deletes");
            RS4P = new RenderSurface4Part(Width, Height, Engine.Rendering, this);
            // FBO
            fbo_texture = GL.GenTexture();
            fbo_main = GL.GenFramebuffer();
            GL.BindTexture(TextureTarget.Texture2D, fbo_texture);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba32f, Width, Height, 0, PixelFormat.Bgra, PixelType.UnsignedByte, IntPtr.Zero);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);
            BindFramebuffer(FramebufferTarget.Framebuffer, fbo_main);
            GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, TextureTarget.Texture2D, fbo_texture, 0);
            BindFramebuffer(FramebufferTarget.Framebuffer, 0);
            GraphicsUtil.CheckError("Load - View3D - Light - FBO");
            // Godray FBO
            fbo_godray_texture = GL.GenTexture();
            fbo_godray_texture2 = GL.GenTexture();
            fbo_godray_main = GL.GenFramebuffer();
            GL.BindTexture(TextureTarget.Texture2D, fbo_godray_texture);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, Width, Height, 0, PixelFormat.Bgra, PixelType.UnsignedByte, IntPtr.Zero);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);
            GL.BindTexture(TextureTarget.Texture2D, fbo_godray_texture2);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, Width, Height, 0, PixelFormat.Bgra, PixelType.UnsignedByte, IntPtr.Zero);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);
            BindFramebuffer(FramebufferTarget.Framebuffer, fbo_godray_main);
            GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, TextureTarget.Texture2D, fbo_godray_texture, 0);
            GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment1, TextureTarget.Texture2D, fbo_godray_texture2, 0);
            BindFramebuffer(FramebufferTarget.Framebuffer, 0);
            GL.BindTexture(TextureTarget.Texture2D, 0);
            GraphicsUtil.CheckError("Load - View3D - Light - Godray");
            // HDR FBO
            hdrtex = GL.GenTexture();
            hdrfbo = GL.GenFramebuffer();
            GL.BindTexture(TextureTarget.Texture2D, hdrtex);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.R32f, HDR_SPREAD, HDR_SPREAD, 0, PixelFormat.Red, PixelType.Float, IntPtr.Zero);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);
            BindFramebuffer(FramebufferTarget.Framebuffer, hdrfbo);
            GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, TextureTarget.Texture2D, hdrtex, 0);
            BindFramebuffer(FramebufferTarget.Framebuffer, 0);
            GraphicsUtil.CheckError("Load - View3D - Light - HDR");
            // Shadow FBO
            int sq = ShadowTexSize();
            fbo_shadow_tex = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2DArray, fbo_shadow_tex);
            GL.TexImage3D(TextureTarget.Texture2DArray, 0, PixelInternalFormat.DepthComponent, sq, sq, SHADOW_BITS_MAX + 1, 0, PixelFormat.DepthComponent, PixelType.Float, IntPtr.Zero);
            GL.TexParameter(TextureTarget.Texture2DArray, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2DArray, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2DArray, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
            GL.TexParameter(TextureTarget.Texture2DArray, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);
            GraphicsUtil.CheckError("Load - View3D - Light - Shadows");
            fbo_shadow_color = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, fbo_shadow_color);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.R32f, sq, sq, 0, PixelFormat.Red, PixelType.Float, IntPtr.Zero);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);
            GL.BindTexture(TextureTarget.Texture2D, 0);
            GraphicsUtil.CheckError("Load - View3D - Light - ShadowColor");
            GL.GenFramebuffers(SHADOW_BITS_MAX + 1, fbo_shadow);
            for (int i = 0; i < SHADOW_BITS_MAX + 1; i++)
            {
                BindFramebuffer(FramebufferTarget.Framebuffer, fbo_shadow[i]);
                GL.FramebufferTextureLayer(FramebufferTarget.Framebuffer, FramebufferAttachment.DepthAttachment, fbo_shadow_tex, 0, i);
                GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, TextureTarget.Texture2D, fbo_shadow_color, 0);
                GraphicsUtil.CheckError("Load - View3D - Light - LMAP:" + i);
            }
            BindFramebuffer(FramebufferTarget.Framebuffer, 0);
            GraphicsUtil.CheckError("Load - View3D - Light - Final");
            fbo_decal = GL.GenFramebuffer();
            fbo_decal_tex = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, fbo_decal_tex);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba8, Width, Height, 0, PixelFormat.Bgra, PixelType.UnsignedByte, IntPtr.Zero);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);
            fbo_decal_depth = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, fbo_decal_depth);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.DepthComponent32, Width, Height, 0, PixelFormat.DepthComponent, PixelType.Float, IntPtr.Zero);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);
            BindFramebuffer(FramebufferTarget.Framebuffer, fbo_decal);
            GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.DepthAttachment, TextureTarget.Texture2D, fbo_decal_depth, 0);
            GL.BindTexture(TextureTarget.Texture2D, 0);
            GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, TextureTarget.Texture2D, fbo_decal_tex, 0);
            BindFramebuffer(FramebufferTarget.Framebuffer, 0);
            GL.BindTexture(TextureTarget.Texture2D, 0);
            GraphicsUtil.CheckError("Load - View3D - Decal");
        }

        /// <summary>
        /// Helper to generate an FBO backend for the view, if not a main-screen view.
        /// </summary>
        public void GenerateFBO()
        {
            if (CurrentFBO != 0)
            {
                GL.DeleteFramebuffer(CurrentFBO);
                GL.DeleteTexture(CurrentFBOTexture);
                GL.DeleteTexture(CurrentFBODepth);
            }
            GraphicsUtil.CheckError("Load - View3D - GenFBO - Deletes");
            GL.ActiveTexture(TextureUnit.Texture0);
            CurrentFBOTexture = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, CurrentFBOTexture);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, Width, Height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, IntPtr.Zero);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);
            CurrentFBODepth = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, CurrentFBODepth);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.DepthComponent32, Width, Height, 0, PixelFormat.DepthComponent, PixelType.Float, IntPtr.Zero);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);
            CurrentFBO = GL.GenFramebuffer();
            BindFramebuffer(FramebufferTarget.Framebuffer, CurrentFBO);
            GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, TextureTarget.Texture2D, CurrentFBOTexture, 0);
            GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.DepthAttachment, TextureTarget.Texture2D, CurrentFBODepth, 0);
            BindFramebuffer(FramebufferTarget.Framebuffer, 0);
            GL.BindTexture(TextureTarget.Texture2D, 0);
            GraphicsUtil.CheckError("Load - View3D - GenFBO");
        }

        /// <summary>
        /// An RS4P used in some locations.
        /// </summary>
        public RenderSurface4Part RS4P;

        /// <summary>
        /// The camera frustum.
        /// </summary>
        public Frustum camFrust;

        /// <summary>
        /// Maximum shadow textures at once (Some may be reused as sub-texture based shadows).
        /// </summary>
        public const int SHADOW_BITS_MAX = 17;

        /// <summary>
        /// Maximum lights at once.
        /// </summary>
        public const int LIGHTS_MAX = 38;

        /// <summary>
        /// The shadow texture size. Defaults to 64.
        /// </summary>
        public Func<int> ShadowTexSize = () => 64;

        int fbo_texture;
        int fbo_main;

        int fbo_godray_main;
        int fbo_godray_texture;
        int fbo_godray_texture2;

        int fbo_decal = -1;
        int fbo_decal_depth = -1;
        int fbo_decal_tex = -1;

        int[] fbo_shadow = new int[SHADOW_BITS_MAX + 1];
        int fbo_shadow_tex = -1;
        int fbo_shadow_color = -1;

        /// <summary>
        /// Current fog alpha.
        /// </summary>
        public float FogAlpha = 0.0f;

        /// <summary>
        /// Sets the viewport to match the render target.
        /// </summary>
        public void SetViewPort()
        {
            GL.Viewport(0, 0, Width, Height);
        }

        /// <summary>
        /// Sets the view port to the correct values, and tracks them.
        /// </summary>
        public void OSetViewport()
        {
            Viewport(0, 0, Width, Height);
        }

        /// <summary>
        /// Whether to render in fast mode. Defaults to on.
        /// </summary>
        public bool FastOnly = true;

        /// <summary>
        /// Duration left on a simple 'flash bang' effect.
        /// </summary>
        public double FB_DurLeft = 0.0;

        /// <summary>
        /// The texture the system is using for a simple 'flash bang' effect.
        /// </summary>
        public int FB_Tex = -1;

        /// <summary>
        /// The current view FBO.
        /// </summary>
        public int CurrentFBO = 0;

        /// <summary>
        /// The tracked draw buffer mode.
        /// </summary>
        public DrawBufferMode BufferMode = DrawBufferMode.Back;

        /// <summary>
        /// Sets and tracks the draw buffer mode.
        /// </summary>
        /// <param name="dbm">The mode to set.</param>
        public void DrawBuffer(DrawBufferMode dbm)
        {
            BufferMode = dbm;
            GL.DrawBuffer(dbm);
        }

        /// <summary>
        /// The current view FBO texture.
        /// </summary>
        public int CurrentFBOTexture = 0;

        /// <summary>
        /// The current view FBO depth texture.
        /// </summary>
        public int CurrentFBODepth = 0;

        /// <summary>
        /// Whether textures are currently rendering.
        /// </summary>
        public bool RenderTextures = false;

        /// <summary>
        /// Whether shadows are currently rendering.
        /// </summary>
        public bool RenderingShadows = false;

        /// <summary>
        /// The current forward vector.
        /// </summary>
        public Location ForwardVec = Location.Zero;

        /// <summary>
        /// The current view matrix.
        /// </summary>
        public Matrix4 PrimaryMatrix;

        /// <summary>
        /// The second-eye view frustum.
        /// </summary>
        public Frustum cf2;

        /// <summary>
        /// The primary frustum.
        /// </summary>
        public Frustum CFrust;

        /// <summary>
        /// The long-view ("out view") frustum.
        /// </summary>
        public Frustum LongFrustum;
        
        /// <summary>
        /// What color to clear the viewport to.
        /// </summary>
        public float[] ClearColor = new float[] { 0.2f, 1f, 1f, 1f };

        /// <summary>
        /// Ambient light.
        /// </summary>
        public Location ambient;

        /// <summary>
        /// Desaturation amount to apply to the view.
        /// </summary>
        public float DesaturationAmount = 0f;

        /// <summary>
        /// Desaturation color to apply to the screen.
        /// </summary>
        public Vector3 DesaturationColor = new Vector3(0.95f, 0.77f, 0.55f);

        /// <summary>
        /// The next frame texture.
        /// </summary>
        int NF_Tex = -1;

        /// <summary>
        /// The next frame FBO.
        /// </summary>
        int NF_FBO = -1;

        /// <summary>
        /// The next frame depth texture.
        /// </summary>
        int NF_DTx = -1;

        /// <summary>
        /// The camera's current position.
        /// </summary>
        public Location CameraPos;

        /// <summary>
        /// The camera's current 'target' location.
        /// </summary>
        public Location CameraTarget;

        /// <summary>
        /// Maximum strength of a light. Used for ortho light radius.
        /// </summary>
        public const float LightMaximum = 1E10f;

        /// <summary>
        /// Whether to enable godrays.
        /// </summary>
        public bool GodRays = true;

        /// <summary>
        /// Converts the next frame to a texture.
        /// </summary>
        /// <returns>The texture.</returns>
        public int NextFrameToTexture()
        {
            if (NF_Tex != -1)
            {
                return NF_Tex;
            }
            if (FB_Tex != -1)
            {
                return FB_Tex;
            }
            GraphicsUtil.CheckError("View3D - NFTex - Pre");
            GL.ActiveTexture(TextureUnit.Texture0);
            NF_Tex = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, NF_Tex);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, Width, Height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, IntPtr.Zero);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);
            NF_DTx = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, NF_DTx);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.DepthComponent32, Width, Height, 0, PixelFormat.DepthComponent, PixelType.Float, IntPtr.Zero);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);
            NF_FBO = GL.GenFramebuffer();
            BindFramebuffer(FramebufferTarget.Framebuffer, NF_FBO);
            GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, TextureTarget.Texture2D, NF_Tex, 0);
            GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.DepthAttachment, TextureTarget.Texture2D, NF_DTx, 0);
            BindFramebuffer(FramebufferTarget.Framebuffer, 0);
            GL.BindTexture(TextureTarget.Texture2D, 0);
            GraphicsUtil.CheckError("View3D - NFTex");
            return NF_Tex;
        }

        /// <summary>
        /// Causes a simple 'flash bang' effect.
        /// </summary>
        /// <param name="duration_add">The duration to add to the effect.</param>
        public void Flashbang(double duration_add)
        {
            if (FB_DurLeft == 0.0)
            {
                FB_Tex = NextFrameToTexture();
            }
            FB_DurLeft += duration_add;
        }

        /// <summary>
        /// Represents a simple orthographic matrix of all 1's and -1's.
        /// </summary>
        public Matrix4 SimpleOrthoMatrix = Matrix4.CreateOrthographicOffCenter(-1, 1, -1, 1, -1, 1);

        /// <summary>
        /// Internal call to end a 'next frame' effect on any given render tick.
        /// </summary>
        /// <param name="pfbo">The previous FBO.</param>
        public void EndNF(int pfbo)
        {
            if (FB_Tex != -1)
            {
                FB_DurLeft -= Engine.Delta;
                if (FB_DurLeft > 0)
                {
                    Engine.Shaders.ColorMultShader.Bind();
                    GL.ActiveTexture(TextureUnit.Texture0);
                    GL.BindTexture(TextureTarget.Texture2D, FB_Tex);
                    float power = FB_DurLeft > 2.0 ? 1f : ((float)FB_DurLeft * 0.5f);
                    Engine.Rendering.SetColor(new Vector4(1f, 1f, 1f, power), this);
                    GL.Disable(EnableCap.DepthTest);
                    GL.Disable(EnableCap.CullFace);
                    GL.UniformMatrix4(1, false, ref SimpleOrthoMatrix);
                    GL.UniformMatrix4(2, false, ref View3D.IdentityMatrix);
                    Engine.Rendering.RenderRectangle(-1, -1, 1, 1);
                    Engine.Textures.White.Bind();
                    if (power < 1f)
                    {
                        Engine.Rendering.SetColor(new Vector4(1f, 1f, 1f, (1f - power) * power), this);
                        Engine.Rendering.RenderRectangle(-1, -1, 1, 1);
                    }
                }
                else
                {
                    FB_DurLeft = 0;
                    GL.DeleteTexture(FB_Tex);
                    FB_Tex = -1;
                }
                if (pfbo != 0)
                {
                    GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
                    GL.BindFramebuffer(FramebufferTarget.ReadFramebuffer, pfbo);
                    GL.BindFramebuffer(FramebufferTarget.Framebuffer, NF_FBO);
                    GL.BlitFramebuffer(0, 0, Width, Height, 0, 0, Width, Height, ClearBufferMask.ColorBufferBit, BlitFramebufferFilter.Nearest);
                    GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
                    GL.BindFramebuffer(FramebufferTarget.ReadFramebuffer, 0);
                }
                GL.DeleteFramebuffer(NF_FBO);
                GL.DeleteTexture(NF_DTx);
                CurrentFBO = pfbo;
                NF_FBO = -1;
                NF_DTx = -1;
                NF_Tex = -1;
            }
        }

        /// <summary>
        /// Renders the entire 3D viewport.
        /// </summary>
        public void Render()
        {
            int pfbo = CurrentFBO;
            try
            {
                if (NF_FBO != -1)
                {
                    if (pfbo == 0)
                    {
                        CurrentFBO = NF_FBO;
                    }
                }
                RenderPass_Setup();
                GraphicsUtil.CheckError("Render - Setup");
                if (FastOnly)
                {
                    if (Engine.Forward_Shadows)
                    {
                        RenderPass_Shadows();
                        GraphicsUtil.CheckError("Render - Shadow (Fast)");
                    }
                    RenderPass_FAST();
                    GraphicsUtil.CheckError("Render - Fast");
                    EndNF(pfbo);
                    return;
                }
                Stopwatch timer = new Stopwatch();
                timer.Start();
                if (Engine.Deferred_Shadows)
                {
                    RenderPass_Shadows();
                    GraphicsUtil.CheckError("Render - Shadow");
                }
                RenderPass_GBuffer();
                GraphicsUtil.CheckError("Render - Buffer");
                RenderPass_Lights();
                GraphicsUtil.CheckError("Render - Lights");
                FinalHDRGrab();
                GraphicsUtil.CheckError("Render - HDR");
                PForward = CameraForward + CameraPos;
                timer.Stop();
                TotalTime = (double)timer.ElapsedMilliseconds / 1000f;
                if (TotalTime > TotalSpikeTime)
                {
                    TotalSpikeTime = TotalTime;
                }
                EndNF(pfbo);
            }
            catch (Exception ex)
            {
                CommonUtilities.CheckException(ex);
                SysConsole.Output("Rendering (3D)", ex);
                CurrentFBO = pfbo;
            }
        }

        /// <summary>
        /// The offset of the world matrix.
        /// </summary>
        public Matrix4d OffsetWorld = Matrix4d.Identity;

        /// <summary>
        /// Gets a Matrix4(f) from a Matrix4d. For use with SetMatrix.
        /// WARNING: This is NOT a flat converter, it CHANGES data for SetMatrix.
        /// </summary>
        /// <param name="mat">The double matrix.</param>
        /// <returns>The float matrix.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Matrix4 GetMat4f(Matrix4d mat)
        {
            Matrix4d temp = mat * OffsetWorld;
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
        /// Viewport X reference.
        /// </summary>
        public int vx;

        /// <summary>
        /// Viewport Y reference.
        /// </summary>
        public int vy;

        /// <summary>
        /// Viewport width reference.
        /// </summary>
        public int vw;

        /// <summary>
        /// Viewport height reference.
        /// </summary>
        public int vh;

        /// <summary>
        /// The primary matrix, as doubles.
        /// </summary>
        public Matrix4d PrimaryMatrixd;

        /// <summary>
        /// The primary matrix in 3D view mode's second eye if needed.
        /// </summary>
        public Matrix4d PrimaryMatrix_OffsetFor3Dd;

        /// <summary>
        /// The matrix for very far distance rendering.
        /// </summary>
        public Matrix4 OutViewMatrix = Matrix4.Identity;

        /// <summary>
        /// The location all rendering is relative to.
        /// </summary>
        public Location RenderRelative;

        /// <summary>
        /// Sets and tracks the viewport for trackable changes.
        /// </summary>
        /// <param name="x">X coordinate.</param>
        /// <param name="y">Y coordinate.</param>
        /// <param name="w">Width.</param>
        /// <param name="h">Height.</param>
        public void Viewport(int x, int y, int w, int h)
        {
            vx = x;
            vy = y;
            vw = w;
            vh = h;
            GL.Viewport(x, y, w, h);
        }
        
        /// <summary>
        /// The camera forward vector.
        /// </summary>
        public Location CameraForward = Location.UnitX;

        /// <summary>
        /// An identity matrix - do not edit!
        /// </summary>
        public static Matrix4 IdentityMatrix = Matrix4.Identity;

        /// <summary>
        /// The camera's base position.
        /// </summary>
        public Location cameraBasePos;

        /// <summary>
        /// The camera's adjustment value for 3D.
        /// </summary>
        public Location cameraAdjust;

        /// <summary>
        /// The primary matrix, as offset for 3D logic.
        /// </summary>
        public Matrix4 PrimaryMatrix_OffsetFor3D;

        /// <summary>
        /// The previous forward vector.
        /// </summary>
        public Location PForward = Location.Zero;

        /// <summary>
        /// Gets any camera rotation effects needed. Defaults to Identity.
        /// </summary>
        public Func<BEPUutilities.Quaternion> CameraModifier = () => BEPUutilities.Quaternion.Identity;

        /// <summary>
        /// Calculates the tranformation of the forward vector.
        /// </summary>
        /// <returns>Transformed forward vector.</returns>
        public Location CalcForward()
        {
            BEPUutilities.Quaternion cammod = CameraModifier();
            Location camforward = ForwardVec;
            camforward = new Location(BEPUutilities.Quaternion.Transform(camforward.ToBVector(), cammod));
            return camforward;
        }

        /// <summary>
        /// Fixes the viewport back to what was last set by <see cref="Viewport(int, int, int, int)"/>.
        /// </summary>
        public void FixVP()
        {
            GL.Viewport(vx, vy, vw, vh);
        }

        /// <summary>
        /// The current FBO, as tracked by <see cref="BindFramebuffer(FramebufferTarget, int)"/>.
        /// </summary>
        public int cFBO = 0;

        /// <summary>
        /// Binds a framebuffer and tracks it. Generally used with DrawFB's.
        /// </summary>
        /// <param name="fbt">The target (usually draw).</param>
        /// <param name="fbo">The object.</param>
        public void BindFramebuffer(FramebufferTarget fbt, int fbo)
        {
            GL.BindFramebuffer(fbt, fbo);
            cFBO = fbo;
        }

        /// <summary>
        /// Sets the blend to a standard ordered blend.
        /// </summary>
        public void StandardBlend()
        {
            GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
        }

        /// <summary>
        /// Sets the blend to additive (transparent) mode.
        /// </summary>
        public void TranspBlend()
        {
            GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.One);
        }

        /// <summary>
        /// The camera's up vector. Defaults to Z-Up (0,0,1).
        /// </summary>
        public Func<Location> CameraUp = () => Location.UnitZ;

        /// <summary>
        /// Outview matrix, offset for VR.
        /// </summary>
        public Matrix4 OutView_OffsetVR;

        /// <summary>
        /// Outview matrix, offset for VR.
        /// </summary>
        public Matrix4d OutView_OffsetVRd;

        /// <summary>
        /// Set up the rendering engine.
        /// </summary>
        public void RenderPass_Setup()
        {
            BEPUutilities.Quaternion cammod = CameraModifier();
            Location camup = new Location(BEPUutilities.Quaternion.Transform(CameraUp().ToBVector(), cammod));
            Location camforward = new Location(BEPUutilities.Quaternion.Transform(ForwardVec.ToBVector(), cammod));
            CameraForward = camforward;
            BindFramebuffer(FramebufferTarget.Framebuffer, CurrentFBO);
            DrawBuffer(CurrentFBO == 0 ? DrawBufferMode.Back : DrawBufferMode.ColorAttachment0);
            StandardBlend();
            GL.Enable(EnableCap.DepthTest);
            RenderTextures = true;
            GL.ClearBuffer(ClearBuffer.Color, 0, ClearColor);
            GL.ClearBuffer(ClearBuffer.Depth, 0, new float[] { 1.0f });
            cameraBasePos = CameraPos;
            cameraAdjust = -camforward.CrossProduct(camup) * 0.25;
            if (Engine.Client.VR != null)
            {
                //cameraAdjust = -cameraAdjust;
                cameraAdjust = Location.Zero;
            }
            RenderRelative = CameraPos;
            OSetViewport();
            CameraTarget = CameraPos + camforward;
            OffsetWorld = Matrix4d.CreateTranslation((-CameraPos).ToOpenTK3D());
            Matrix4d outviewD;
            if (Engine.Client.VR != null)
            {
                Matrix4 proj = Engine.Client.VR.GetProjection(true, Engine.ZNear, Engine.ZFar());
                Matrix4 view = Engine.Client.VR.Eye(true);
                PrimaryMatrix = view * proj;
                Matrix4 proj2 = Engine.Client.VR.GetProjection(false, Engine.ZNear, Engine.ZFar());
                Matrix4 view2 = Engine.Client.VR.Eye(false);
                PrimaryMatrix_OffsetFor3D = view2 * proj2;
                PrimaryMatrixd = Matrix4d.CreateTranslation((-CameraPos).ToOpenTK3D()) * view.ConvertToD() * proj.ConvertToD();
                PrimaryMatrix_OffsetFor3Dd = Matrix4d.CreateTranslation((-CameraPos).ToOpenTK3D()) * view2.ConvertToD() * proj2.ConvertToD();
                Matrix4 projo = Engine.Client.VR.GetProjection(true, 60f, Engine.ZFarOut());
                OutViewMatrix = view * projo;
                outviewD = Matrix4d.CreateTranslation((-CameraPos).ToOpenTK3D()) * view.ConvertToD() * projo.ConvertToD();
                Matrix4 projo2 = Engine.Client.VR.GetProjection(false, 60f, Engine.ZFarOut());
                OutView_OffsetVR = view2 * projo2;
                OutView_OffsetVRd = Matrix4d.CreateTranslation((-CameraPos).ToOpenTK3D()) * view2.ConvertToD() * projo2.ConvertToD();
                // TODO: Transform VR by cammod?
            }
            else
            {
                Matrix4 proj = Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(Engine.FOV), (float)Width / (float)Height, Engine.ZNear, Engine.ZFar()); // TODO: View3D-level vars?
                Location bx = Engine.Render3DView ? (cameraAdjust) : Location.Zero;
                Matrix4 view = Matrix4.LookAt(bx.ToOpenTK(), (bx + camforward).ToOpenTK(), camup.ToOpenTK());
                PrimaryMatrix = view * proj;
                if (Engine.Render3DView)
                {
                    Matrix4 view2 = Matrix4.LookAt((-cameraAdjust).ToOpenTK(), (-cameraAdjust + camforward).ToOpenTK(), camup.ToOpenTK());
                    PrimaryMatrix_OffsetFor3D = view2 * proj;
                }
                Matrix4 proj_out = Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(Engine.FOV), (float)Width / (float)Height, 60f, Engine.ZFarOut()); // TODO: View3D-level vars?
                OutViewMatrix = view * proj_out;
                Matrix4d projd = Matrix4d.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(Engine.FOV),
                    (float)Width / (float)Height, Engine.ZNear, Engine.ZFar()); // TODO: View3D-level vars?
                Location bxd = Engine.Render3DView ? (CameraPos + cameraAdjust) : CameraPos;
                Matrix4d viewd = Matrix4d.LookAt(bxd.ToOpenTK3D(), (bxd + camforward).ToOpenTK3D(), camup.ToOpenTK3D());
                PrimaryMatrixd = viewd * projd;
                Matrix4d proj_outd = Matrix4d.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(Engine.FOV), (float)Width / (float)Height, 60f, Engine.ZFarOut()); // TODO: View3D-level vars?
                outviewD = viewd * proj_outd;
                PrimaryMatrix_OffsetFor3Dd = Matrix4d.Identity;
                if (Engine.Render3DView)
                {
                    Matrix4d view2d = Matrix4d.LookAt((CameraPos - cameraAdjust).ToOpenTK3D(), (CameraPos - cameraAdjust + camforward).ToOpenTK3D(), camup.ToOpenTK3D());
                    PrimaryMatrix_OffsetFor3Dd = view2d * projd;
                }
            }
            LongFrustum = new Frustum(outviewD.ConvertD());
            camFrust = new Frustum(PrimaryMatrixd.ConvertD());
            cf2 = new Frustum(PrimaryMatrix_OffsetFor3Dd.ConvertD());
            CFrust = camFrust;
            GraphicsUtil.CheckError("AfterSetup");
        }

        /// <summary>
        /// The maximum distance of lights.
        /// </summary>
        public double LightsMaxDistance = 1000;

        /// <summary>
        /// Executable view patch.
        /// </summary>
        public Action<float[], float[], float, Vector3, int> ViewPatchOne;

        /// <summary>
        /// Executable view patch.
        /// </summary>
        public Action ViewPatchTwo;

        /// <summary>
        /// Executable view patch.
        /// </summary>
        public Action<float, float[], float[], int> ViewPatchThree;

        /// <summary>
        /// Executable view patch.
        /// </summary>
        public Action ViewPatchFour;

        /// <summary>
        /// Executable view patch.
        /// </summary>
        public Action ViewPatchFive;

        /// <summary>
        /// Executable view patch.
        /// </summary>
        public Action<int, int> ViewPatchSix;

        /// <summary>
        /// Executable view patch.
        /// </summary>
        public Action ViewPatchSeven;

        /// <summary>
        /// Executable view patch.
        /// </summary>
        public Action ViewPatchEight;

        /// <summary>
        /// Executable view patch.
        /// </summary>
        public Action ViewPatchNine;

        /// <summary>
        /// Executable view patch.
        /// </summary>
        public Action ViewPatchTen;

        /// <summary>
        /// Executable view patch.
        /// </summary>
        public Action ViewPatchEleven;

        /// <summary>
        /// Executable view patch.
        /// </summary>
        public Action ViewPatchTwelve;

        /// <summary>
        /// Executable view patch.
        /// </summary>
        public Action<Matrix4, float[], float[]> ViewPatchThirteen;

        /// <summary>
        /// Executable view patch.
        /// </summary>
        public Action<Matrix4> ViewPatchFourteen;

        /// <summary>
        /// Executable view patch.
        /// </summary>
        public Action ViewPatchFifteen;
        
        /// <summary>
        /// Executable view patch.
        /// </summary>
        public Action ViewPatchSixteen;
        
        /// <summary>
        /// Whether the system is currently rendering the second eye (When VR or 3D enabled).
        /// </summary>
        public bool IsSecondEye = false;

        /// <summary>
        /// Render everything as quickly as possible: a simple forward renderer.
        /// </summary>
        public void RenderPass_FAST()
        {
            GraphicsUtil.CheckError("Render/Fast - Prep");
            if (Engine.DisplayDecals || Engine.ForwardReflections)
            {
                RS4P.Bind(this);
                RS4P.Clear();
                GL.ClearBuffer(ClearBuffer.Color, 0, ClearColor);
            }
            float[] light_dat = new float[LIGHTS_MAX * 16];
            float[] shadowmat_dat = new float[LIGHTS_MAX * 16];
            int c = 0;
            if (Engine.Forward_Lights)
            {
                // TODO: An ambient light source?
                for (int i = 0; i < Lights.Count; i++)
                {
                    if (Lights[i] is SkyLight || camFrust == null || camFrust.ContainsSphere(Lights[i].EyePos, Lights[i].MaxDistance))
                    {
                        double d1 = (Lights[i].EyePos - CameraPos).LengthSquared();
                        double d2 = LightsMaxDistance * LightsMaxDistance + Lights[i].MaxDistance * Lights[i].MaxDistance;
                        double maxrangemult = 0;
                        if (d1 < d2 * 4 || Lights[i] is SkyLight)
                        {
                            maxrangemult = 1;
                        }
                        else if (d1 < d2 * 6)
                        {
                            maxrangemult = 1 - ((d1 - (d2 * 4)) / ((d2 * 6) - (d2 * 4)));
                        }
                        if (maxrangemult > 0)
                        {
                            if (Lights[i] is PointLight pl && !pl.CastShadows)
                            {
                                Matrix4 smat = Matrix4.Identity;
                                Vector3d eyep = Lights[i].InternalLights[0].eye - CameraPos.ToOpenTK3D();
                                Vector3 col = Lights[i].InternalLights[0].color * (float)maxrangemult;
                                Matrix4 light_data = new Matrix4(
                                    (float)eyep.X, (float)eyep.Y, (float)eyep.Z, // light_pos
                                    0.7f, // diffuse_albedo
                                    0.7f, // specular_albedo
                                    0.0f, // should_sqrt
                                    col.X, col.Y, col.Z, // light_color
                                    (Lights[i].InternalLights[0].maxrange <= 0 ? LightMaximum : Lights[i].InternalLights[0].maxrange), // light_radius
                                    0f, 0f, 0f, // eye_pos
                                    2.0f, // light_type
                                    1f / ShadowTexSize(), // tex_size
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
                                if (c >= LIGHTS_MAX)
                                {
                                    goto lights_apply;
                                }
                            }
                            else
                            {
                                for (int x = 0; x < Lights[i].InternalLights.Count; x++)
                                {
                                    if (Lights[i].InternalLights[x].color.LengthSquared <= 0.01)
                                    {
                                        continue;
                                    }
                                    int sp = ShadowTexSize();
                                    if (c >= 10)
                                    {
                                        sp /= 2;
                                    }
                                    Matrix4 smat = Lights[i].InternalLights[x].GetMatrix(this);
                                    Vector3d eyep = Lights[i] is SkyLight se ? -se.Direction.ToOpenTK3D() : Lights[i].InternalLights[x].eye - CameraPos.ToOpenTK3D();
                                    Vector3 col = Lights[i].InternalLights[x].color * (float)maxrangemult;
                                    Matrix4 light_data = new Matrix4(
                                        (float)eyep.X, (float)eyep.Y, (float)eyep.Z, // light_pos
                                        0.7f, // diffuse_albedo
                                        0.7f, // specular_albedo
                                        Lights[i].InternalLights[x] is LightOrtho ? 1.0f : 0.0f, // should_sqrt
                                        col.X, col.Y, col.Z, // light_color
                                        Lights[i].InternalLights[x] is LightOrtho ? LightMaximum : (Lights[i].InternalLights[0].maxrange <= 0 ? LightMaximum : Lights[i].InternalLights[0].maxrange), // light_radius
                                        0f, 0f, 0f, // eye_pos
                                        Lights[i] is SpotLight ? 1.0f : 0.0f, // light_type
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
                                    if (c >= LIGHTS_MAX)
                                    {
                                        goto lights_apply;
                                    }
                                }
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
                GL.BindTexture(TextureTarget.Texture2D, 0);
                GL.BindTexture(TextureTarget.Texture2DArray, fbo_shadow_tex);
                GL.ActiveTexture(TextureUnit.Texture0);
            }
            GraphicsUtil.CheckError("Render/Fast - Uniforms 1");
            RenderingShadows = false;
            RenderLights = Engine.Forward_Lights;
            GL.ActiveTexture(TextureUnit.Texture0);
            FBOid = FBOID.FORWARD_SOLID;
            Vector3 maxLit = Engine.SunAdjustBackupLight.Xyz;
            Engine.Shaders3D.Forward.s_forw_particles.Bind();
            GraphicsUtil.CheckError("Render/Fast - Uniforms 1.3");
            GL.Uniform4(4, new Vector4(Width, Height, Engine.ZNear, Engine.ZFar()));
            GraphicsUtil.CheckError("Render/Fast - Uniforms 1.4");
            //GL.Uniform1(6, (float)Engine.GlobalTickTimeLocal);
            //CheckError("Render/Fast - Uniforms 1.43");
            GL.Uniform4(12, new Vector4(FogCol.ToOpenTK(), FogAlpha));
            GraphicsUtil.CheckError("Render/Fast - Uniforms 1.46");
            //GL.Uniform2(14, new Vector2(Engine.ZNear, Engine.ZFar()));
            GraphicsUtil.CheckError("Render/Fast - Uniforms 1.5");
            /*if (Engine.CVars.r_forward_lights.ValueB)
            {
                GL.Uniform1(15, (float)c);
                CheckError("Render/Fast - Uniforms 1.7");
                GL.UniformMatrix4(20, LIGHTS_MAX, false, shadowmat_dat);
                CheckError("Render/Fast - Uniforms 1.8");
                GL.UniformMatrix4(20 + LIGHTS_MAX, LIGHTS_MAX, false, light_dat);
                CheckError("Render/Fast - Uniforms 2");
            }*/
            Engine.Shaders3D.Forward.s_forw_grass.Bind();
            GraphicsUtil.CheckError("Render/Fast - Uniforms 2.2");
            if (Engine.Forward_Lights)
            {
                GL.Uniform1(15, (float)c);
                GL.UniformMatrix4(20, LIGHTS_MAX, false, shadowmat_dat);
                GL.UniformMatrix4(20 + LIGHTS_MAX, LIGHTS_MAX, false, light_dat);
            }
            GraphicsUtil.CheckError("Render/Fast - Uniforms 2.5");
            Engine.Shaders3D.Forward.s_forwdecal.Bind();
            GraphicsUtil.CheckError("Render/Fast - Uniforms 2.6");
            if (Engine.Forward_Lights)
            {
                GL.Uniform1(15, (float)c);
                GraphicsUtil.CheckError("Render/Fast - Uniforms 2.7");
                GL.UniformMatrix4(20, LIGHTS_MAX, false, shadowmat_dat);
                GraphicsUtil.CheckError("Render/Fast - Uniforms 2.8");
                GL.UniformMatrix4(20 + LIGHTS_MAX, LIGHTS_MAX, false, light_dat);
                GraphicsUtil.CheckError("Render/Fast - Uniforms 2.9");
            }
            GraphicsUtil.CheckError("Render/Fast - Uniforms 3");
            GL.UniformMatrix4(1, false, ref PrimaryMatrix);
            GL.UniformMatrix4(2, false, ref IdentityMatrix);
            GraphicsUtil.CheckError("Render/Fast - Uniforms 3.2");
            GL.Uniform4(4, new Vector4(Width, Height, Engine.ZNear, Engine.ZFar()));
            //GL.Uniform1(6, (float)Engine.GlobalTickTimeLocal);
            GraphicsUtil.CheckError("Render/Fast - Uniforms 3.3");
            GL.Uniform4(12, new Vector4(FogCol.ToOpenTK(), FogAlpha));
            GraphicsUtil.CheckError("Render/Fast - Uniforms 3.5");
            float fogDist = 1.0f / Engine.FogMaxDist();
            fogDist *= fogDist;
            Vector2 zfar_rel = new Vector2(Engine.ZNear, Engine.ZFar());
            GL.Uniform1(13, fogDist);
            GraphicsUtil.CheckError("Render/Fast - Uniforms 3.9");
            //GL.Uniform2(14, zfar_rel);
            Engine.Rendering.SetColor(Color4.White, this);
            Engine.Shaders3D.Forward.s_forwt.Bind();
            GraphicsUtil.CheckError("Render/Fast - Uniforms 4");
            GL.UniformMatrix4(1, false, ref PrimaryMatrix);
            GraphicsUtil.CheckError("Render/Fast - Uniforms 4.1");
            GL.UniformMatrix4(2, false, ref IdentityMatrix);
            GraphicsUtil.CheckError("Render/Fast - Uniforms 4.2");
            GL.Uniform1(6, (float)Engine.GlobalTickTime);
            GraphicsUtil.CheckError("Render/Fast - Uniforms 4.3");
            GL.Uniform4(12, new Vector4(FogCol.ToOpenTK(), FogAlpha));
            GraphicsUtil.CheckError("Render/Fast - Uniforms 4.4");
            GL.Uniform1(13, fogDist);
            GraphicsUtil.CheckError("Render/Fast - Uniforms 4.5");
            //GL.Uniform2(14, zfar_rel);
            GraphicsUtil.CheckError("Render/Fast - Uniforms 4.6");
            Engine.Rendering.SetColor(Color4.White, this);
            GraphicsUtil.CheckError("Render/Fast - Uniforms 4.7");
            Engine.Shaders3D.Forward.s_forwt_nofog.Bind();
            GraphicsUtil.CheckError("Render/Fast - Uniforms 4.8");
            GL.UniformMatrix4(1, false, ref PrimaryMatrix);
            GL.UniformMatrix4(2, false, ref IdentityMatrix);
            GL.Uniform1(6, (float)Engine.GlobalTickTime);
            GL.Uniform4(12, new Vector4(FogCol.ToOpenTK(), FogAlpha));
            GL.Uniform1(13, fogDist);
            //GL.Uniform2(14, zfar_rel);
            Engine.Rendering.SetColor(Color4.White, this);
            Engine.Shaders3D.Forward.s_forwt_obj.Bind();
            GraphicsUtil.CheckError("Render/Fast - Uniforms 4");
            GL.UniformMatrix4(1, false, ref PrimaryMatrix);
            GL.UniformMatrix4(2, false, ref IdentityMatrix);
            GL.Uniform1(6, (float)Engine.GlobalTickTime);
            GL.Uniform4(12, new Vector4(FogCol.ToOpenTK(), FogAlpha));
            GL.Uniform1(13, fogDist);
            //GL.Uniform2(14, zfar_rel);
            Engine.Rendering.SetColor(Color4.White, this);
            ViewPatchOne?.Invoke(shadowmat_dat, light_dat, fogDist, maxLit, c);
            Engine.Shaders3D.Forward.s_forw_nobones.Bind();
            if (Engine.Forward_Lights)
            {
                GL.Uniform1(15, (float)c);
                GL.UniformMatrix4(20, LIGHTS_MAX, false, shadowmat_dat);
                GL.UniformMatrix4(20 + LIGHTS_MAX, LIGHTS_MAX, false, light_dat);
            }
            GraphicsUtil.CheckError("Render/Fast - Uniforms 5.5");
            GL.UniformMatrix4(1, false, ref PrimaryMatrix);
            GraphicsUtil.CheckError("Render/Fast - Uniforms 5.51");
            GL.UniformMatrix4(2, false, ref IdentityMatrix);
            GraphicsUtil.CheckError("Render/Fast - Uniforms 5.52");
            GL.Uniform1(6, (float)Engine.GlobalTickTime);
            GraphicsUtil.CheckError("Render/Fast - Uniforms 5.53");
            GL.Uniform4(12, new Vector4(FogCol.ToOpenTK(), FogAlpha));
            GraphicsUtil.CheckError("Render/Fast - Uniforms 5.54");
            GL.Uniform1(13, fogDist);
            GraphicsUtil.CheckError("Render/Fast - Uniforms 5.55");
            //GL.Uniform2(14, zfar_rel);
            Engine.Rendering.SetColor(Color4.White, this);
            GraphicsUtil.CheckError("Render/Fast - Uniforms 5.56");
            if (!Engine.Forward_Lights)
            {
                GL.Uniform3(10, -Engine.SunAdjustDirection.ToOpenTK());
                GraphicsUtil.CheckError("Render/Fast - Uniforms 5.57");
                GL.Uniform3(11, maxLit);
            }
            GraphicsUtil.CheckError("Render/Fast - Uniforms 5.58");
            Engine.Shaders3D.Forward.s_forw.Bind();
            GraphicsUtil.CheckError("Render/Fast - Uniforms 5.59");
            if (Engine.Forward_Lights)
            {
                GL.Uniform1(15, (float)c);
                GL.UniformMatrix4(20, LIGHTS_MAX, false, shadowmat_dat);
                GL.UniformMatrix4(20 + LIGHTS_MAX, LIGHTS_MAX, false, light_dat);
            }
            GraphicsUtil.CheckError("Render/Fast - Uniforms 6");
            GL.UniformMatrix4(1, false, ref PrimaryMatrix);
            GL.UniformMatrix4(2, false, ref IdentityMatrix);
            GL.Uniform1(6, (float)Engine.GlobalTickTime);
            GL.Uniform4(12, new Vector4(FogCol.ToOpenTK(), FogAlpha));
            GL.Uniform1(13, fogDist);
            //GL.Uniform2(14, zfar_rel);
            Engine.Rendering.SetColor(Color4.White, this);
            if (!Engine.Forward_Lights)
            {
                GL.Uniform3(10, -Engine.SunAdjustDirection.ToOpenTK());
                GL.Uniform3(11, maxLit);
            }
            GraphicsUtil.CheckError("Render/Fast - Uniforms");
            if (Engine.Render3DView || Engine.Client.VR != null)
            {
                Viewport(Width / 2, 0, Width / 2, Height);
                Render3D(this);
                FBOid = FBOID.FORWARD_SOLID;
                CFrust = cf2;
                Viewport(0, 0, Width / 2, Height);
                CameraPos = cameraBasePos - cameraAdjust;
                ViewPatchTwo?.Invoke();
                Engine.Shaders3D.Forward.s_forw_particles.Bind();
                GL.UniformMatrix4(1, false, ref PrimaryMatrix_OffsetFor3D);
                Engine.Shaders3D.Forward.s_forw_grass.Bind();
                GL.UniformMatrix4(1, false, ref PrimaryMatrix_OffsetFor3D);
                Engine.Shaders3D.Forward.s_forwdecal.Bind();
                GL.UniformMatrix4(1, false, ref PrimaryMatrix_OffsetFor3D);
                Engine.Shaders3D.Forward.s_forwt_nofog.Bind();
                GL.UniformMatrix4(1, false, ref PrimaryMatrix_OffsetFor3D);
                Engine.Shaders3D.Forward.s_forwt_obj.Bind();
                GL.UniformMatrix4(1, false, ref PrimaryMatrix_OffsetFor3D);
                Engine.Shaders3D.Forward.s_forwt.Bind();
                GL.UniformMatrix4(1, false, ref PrimaryMatrix_OffsetFor3D);
                Engine.Shaders3D.Forward.s_forw_nobones.Bind();
                GL.UniformMatrix4(1, false, ref PrimaryMatrix_OffsetFor3D);
                Engine.Shaders3D.Forward.s_forw = Engine.Shaders3D.Forward.s_forw.Bind();
                GL.UniformMatrix4(1, false, ref PrimaryMatrix_OffsetFor3D);
                Matrix4 orig = PrimaryMatrix;
                PrimaryMatrix = PrimaryMatrix_OffsetFor3D;
                Matrix4 orig_out = OutViewMatrix;
                OutViewMatrix = OutView_OffsetVR;
                IsSecondEye = true;
                Render3D(this);
                IsSecondEye = false;
                OutViewMatrix = orig_out;
                PrimaryMatrix = orig;
                Viewport(0, 0, Width, Height);
                CameraPos = cameraBasePos + cameraAdjust;
                CFrust = camFrust;
                GraphicsUtil.CheckError("Render/Fast - 3D Solid");
            }
            else
            {
                Render3D(this);
                GraphicsUtil.CheckError("Render/Fast - Solid");
            }
            if (Engine.DisplayDecals || Engine.ForwardReflections)
            {
                RS4P.Unbind(this);
                BindFramebuffer(FramebufferTarget.Framebuffer, CurrentFBO);
                DrawBuffer(CurrentFBO == 0 ? DrawBufferMode.Back : DrawBufferMode.ColorAttachment0);
                GL.BindFramebuffer(FramebufferTarget.ReadFramebuffer, RS4P.fbo);
                GL.BlitFramebuffer(0, 0, Width, Height, 0, 0, Width, Height, ClearBufferMask.DepthBufferBit | ClearBufferMask.ColorBufferBit, BlitFramebufferFilter.Nearest);
                GL.BindFramebuffer(FramebufferTarget.ReadFramebuffer, 0);
                if (Engine.ForwardReflections)
                {
                    Engine.Shaders3D.Forward.s_post_fast = Engine.Shaders3D.Forward.s_post_fast.Bind();
                    GL.UniformMatrix4(1, false, ref SimpleOrthoMatrix);
                    GL.UniformMatrix4(2, false, ref IdentityMatrix);
                    GL.UniformMatrix4(6, false, ref PrimaryMatrix);
                    GL.Uniform2(5, zfar_rel);
                    GL.Uniform2(7, new Vector2(60f, Engine.ZFarOut())); // TODO: View3D-level Vars!
                    GL.ActiveTexture(TextureUnit.Texture5);
                    GL.BindTexture(TextureTarget.Texture2D, OV_DT);
                    GL.ActiveTexture(TextureUnit.Texture4);
                    GL.BindTexture(TextureTarget.Texture2D, RS4P.PositionTexture);
                    GL.ActiveTexture(TextureUnit.Texture3);
                    GL.BindTexture(TextureTarget.Texture2D, RS4P.DepthTexture);
                    GL.ActiveTexture(TextureUnit.Texture2);
                    GL.BindTexture(TextureTarget.Texture2D, RS4P.NormalsTexture);
                    GL.ActiveTexture(TextureUnit.Texture1);
                    GL.BindTexture(TextureTarget.Texture2D, RS4P.DiffuseTexture);
                    GL.ActiveTexture(TextureUnit.Texture0);
                    GL.BindTexture(TextureTarget.Texture2D, RS4P.Rh2Texture);
                    GL.Disable(EnableCap.DepthTest);
                    GL.Disable(EnableCap.CullFace);
                    Engine.Rendering.RenderRectangle(-1, -1, 1, 1);
                    GL.Enable(EnableCap.DepthTest);
                    GL.Enable(EnableCap.CullFace);
                    GL.ActiveTexture(TextureUnit.Texture5);
                    GL.BindTexture(TextureTarget.Texture2D, 0);
                    GL.ActiveTexture(TextureUnit.Texture4);
                    GL.BindTexture(TextureTarget.Texture2D, 0);
                    GL.ActiveTexture(TextureUnit.Texture3);
                    GL.BindTexture(TextureTarget.Texture2D, 0);
                    GL.ActiveTexture(TextureUnit.Texture2);
                    GL.BindTexture(TextureTarget.Texture2D, 0);
                    GL.ActiveTexture(TextureUnit.Texture1);
                    GL.BindTexture(TextureTarget.Texture2D, 0);
                    GL.ActiveTexture(TextureUnit.Texture0);
                    GL.BindTexture(TextureTarget.Texture2D, 0);
                }
            }
            if (Engine.DisplayDecals)
            {
                Engine.Shaders3D.Forward.s_forwdecal = Engine.Shaders3D.Forward.s_forwdecal.Bind();
                GL.ActiveTexture(TextureUnit.Texture4);
                GL.BindTexture(TextureTarget.Texture2D, RS4P.DepthTexture);
                GL.ActiveTexture(TextureUnit.Texture0);
                FBOid = FBOID.FORWARD_EXTRAS;
                GL.DepthMask(false);
                GraphicsUtil.CheckError("Render/Fast - Decal Prep");
                if (Engine.Render3DView || Engine.Client.VR != null)
                {
                    Viewport(Width / 2, 0, Width / 2, Height);
                    DecalRender?.Invoke(this);
                    CFrust = cf2;
                    Viewport(0, 0, Width / 2, Height);
                    CameraPos = cameraBasePos - cameraAdjust;
                    GL.UniformMatrix4(1, false, ref PrimaryMatrix_OffsetFor3D);
                    IsSecondEye = true;
                    DecalRender?.Invoke(this);
                    IsSecondEye = false;
                    Viewport(0, 0, Width, Height);
                    CameraPos = cameraBasePos + cameraAdjust;
                    CFrust = camFrust;
                    GraphicsUtil.CheckError("Render/Fast - Decals 3D");
                }
                else
                {
                    FBOid = FBOID.FORWARD_EXTRAS;
                    Engine.Shaders3D.Forward.s_forwdecal = Engine.Shaders3D.Forward.s_forwdecal.Bind();
                    DecalRender?.Invoke(this);
                    GraphicsUtil.CheckError("Render/Fast - Decals");
                }
            }
            GL.ActiveTexture(TextureUnit.Texture0);
            FBOid = FBOID.FORWARD_TRANSP;
            ViewPatchThree?.Invoke(fogDist, shadowmat_dat, light_dat, c);
            Engine.Shaders3D.Forward.s_forw_trans_nobones.Bind();
            GL.UniformMatrix4(1, false, ref PrimaryMatrix);
            GL.UniformMatrix4(2, false, ref IdentityMatrix);
            GL.Uniform1(6, (float)Engine.GlobalTickTime);
            GL.Uniform4(12, new Vector4(FogCol.ToOpenTK(), FogAlpha));
            GL.Uniform1(13, fogDist);
            if (Engine.Forward_Lights)
            {
                GL.Uniform1(15, (float)c);
                GL.UniformMatrix4(20, LIGHTS_MAX, false, shadowmat_dat);
                GL.UniformMatrix4(20 + LIGHTS_MAX, LIGHTS_MAX, false, light_dat);
            }
            //GL.Uniform2(14, zfar_rel);
            Engine.Rendering.SetColor(Color4.White, this);
            Engine.Shaders3D.Forward.s_forw_trans.Bind();
            GL.UniformMatrix4(1, false, ref PrimaryMatrix);
            GL.UniformMatrix4(2, false, ref IdentityMatrix);
            GL.Uniform1(6, (float)Engine.GlobalTickTime);
            GL.Uniform4(12, new Vector4(FogCol.ToOpenTK(), FogAlpha));
            GL.Uniform1(13, fogDist);
            if (Engine.Forward_Lights)
            {
                GL.Uniform1(15, (float)c);
                GL.UniformMatrix4(20, LIGHTS_MAX, false, shadowmat_dat);
                GL.UniformMatrix4(20 + LIGHTS_MAX, LIGHTS_MAX, false, light_dat);
            }
            //GL.Uniform2(14, zfar_rel);
            Engine.Rendering.SetColor(Color4.White, this);
            PostFirstRender?.Invoke();
            GraphicsUtil.CheckError("Render/Fast - Transp Unifs");
            if (Engine.Render3DView || Engine.Client.VR != null)
            {
                Viewport(Width / 2, 0, Width / 2, Height);
                Render3D(this);
                CFrust = cf2;
                Viewport(0, 0, Width / 2, Height);
                CameraPos = cameraBasePos - cameraAdjust;
                ViewPatchFour?.Invoke();
                Engine.Shaders3D.Forward.s_forw_trans_nobones.Bind();
                GL.UniformMatrix4(1, false, ref PrimaryMatrix_OffsetFor3D);
                Engine.Shaders3D.Forward.s_forw_trans = Engine.Shaders3D.Forward.s_forw_trans.Bind();
                GL.UniformMatrix4(1, false, ref PrimaryMatrix_OffsetFor3D);
                Matrix4 orig = PrimaryMatrix;
                PrimaryMatrix = PrimaryMatrix_OffsetFor3D;
                Matrix4 orig_out = OutViewMatrix;
                OutViewMatrix = OutView_OffsetVR;
                IsSecondEye = true;
                Render3D(this);
                IsSecondEye = false;
                OutViewMatrix = orig_out;
                PrimaryMatrix = orig;
                Viewport(0, 0, Width, Height);
                CameraPos = cameraBasePos + cameraAdjust;
                CFrust = camFrust;
                GraphicsUtil.CheckError("Render/Fast - Transp 3D");
            }
            else
            {
                Render3D(this);
                GraphicsUtil.CheckError("Render/Fast - Transp");
            }
            ViewPatchSixteen?.Invoke();
            if (Engine.Forward_Shadows)
            {
                GL.ActiveTexture(TextureUnit.Texture5);
                GL.BindTexture(TextureTarget.Texture2D, 0);
                GL.BindTexture(TextureTarget.Texture2DArray, fbo_shadow_tex);
                GL.ActiveTexture(TextureUnit.Texture0);
            }
            GL.ActiveTexture(TextureUnit.Texture4);
            GL.BindTexture(TextureTarget.Texture2D, 0);
            GL.ActiveTexture(TextureUnit.Texture0);
            GL.DepthMask(true);
            BindFramebuffer(FramebufferTarget.Framebuffer, 0);
            DrawBuffer(DrawBufferMode.Back);
            GraphicsUtil.CheckError("AfterFast");
        }

        /// <summary>
        /// Change to set whether the system redraws shadows... defaults to always true.
        /// </summary>
        public Func<bool> ShouldRedrawShadows = () => true;

        /// <summary>
        /// Get and reset an indication of major updates that needs redrawing. Defaults to always give a true and reset nothing.
        /// </summary>
        public Func<bool> GetAndResetShouldMajorUpdates = () => true;

        /// <summary>
        /// Calculate shadow maps for the later (lighting) render passes.
        /// </summary>
        public void RenderPass_Shadows()
        {
            if (ShouldRedrawShadows() && ShadowingAllowed)
            {
                bool redraw = GetAndResetShouldMajorUpdates();
                Stopwatch timer = new Stopwatch();
                timer.Start();
                Engine.Shaders3D.Deferred.s_shadow = Engine.Shaders3D.Deferred.s_shadow.Bind();
                ViewPatchFive?.Invoke();
                RenderingShadows = true;
                ShadowsOnly = true;
                LightsC = 0;
                Location campos = CameraPos;
                int n = 0;
                Frustum tcf = CFrust;
                int sp = ShadowTexSize();
                int ssp = sp / 2;
                for (int i = 0; i < Lights.Count; i++)
                {
                    if (Lights[i] is SkyLight || camFrust == null || camFrust.ContainsSphere(Lights[i].EyePos, Lights[i].MaxDistance))
                    {
                        if (Lights[i] is SkyLight || Lights[i].EyePos.DistanceSquared(campos) <
                            LightsMaxDistance * LightsMaxDistance + Lights[i].MaxDistance * Lights[i].MaxDistance * 6)
                        {
                            LightsC++;
                            if (Lights[i] is PointLight pl && !pl.CastShadows)
                            {
                                n++;
                                if (n >= LIGHTS_MAX)
                                {
                                    goto complete;
                                }
                            }
                            else
                            {
                                for (int x = 0; x < Lights[i].InternalLights.Count; x++)
                                {
                                    if (Lights[i].InternalLights[x].color.LengthSquared <= 0.01)
                                    {
                                        continue;
                                    }
                                    if (Lights[i].InternalLights[x] is LightOrtho)
                                    {
                                        CFrust = null;
                                    }
                                    else
                                    {
                                        CFrust = new Frustum(Lights[i].InternalLights[x].GetMatrix(this).ConvertToD().ConvertD()); // TODO: One-step conversion!
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
                                        BindFramebuffer(FramebufferTarget.Framebuffer, fbo_shadow[lTID]);
                                        GL.Viewport(ssp * ltX, ssp * ltY, ssp, ssp);
                                        widX = ssp;
                                        widY = ssp;
                                    }
                                    else
                                    {
                                        BindFramebuffer(FramebufferTarget.Framebuffer, fbo_shadow[lTID]);
                                        GL.Viewport(0, 0, sp, sp);
                                    }
                                    GraphicsUtil.CheckError("Pre-Prerender - Shadows - " + i);
                                    CameraPos = Lights[i].InternalLights[x].eye.ToLocation() - campos;
                                    ViewPatchSix?.Invoke(i, x);
                                    Engine.Shaders3D.Deferred.s_shadow_grass = Engine.Shaders3D.Deferred.s_shadow_grass.Bind();
                                    SetMatrix(2, Matrix4d.Identity);
                                    GL.Uniform1(5, (Lights[i].InternalLights[x] is LightOrtho) ? 1.0f : 0.0f);
                                    GL.Uniform1(4, Lights[i].InternalLights[x].transp ? 1.0f : 0.0f);
                                    Lights[i].InternalLights[x].SetProj(this);
                                    GraphicsUtil.CheckError("Pre-Prerender2 - Shadows - " + i);
                                    Engine.Shaders3D.Deferred.s_shadow_parts = Engine.Shaders3D.Deferred.s_shadow_parts.Bind();
                                    SetMatrix(2, Matrix4d.Identity);
                                    GL.Uniform1(5, (Lights[i].InternalLights[x] is LightOrtho) ? 1.0f : 0.0f);
                                    GL.Uniform1(4, Lights[i].InternalLights[x].transp ? 1.0f : 0.0f);
                                    GL.Uniform3(7, CameraPos.ToOpenTK());
                                    Lights[i].InternalLights[x].SetProj(this);
                                    Engine.Shaders3D.Deferred.s_shadow_nobones = Engine.Shaders3D.Deferred.s_shadow_nobones.Bind();
                                    SetMatrix(2, Matrix4d.Identity);
                                    GraphicsUtil.CheckError("Pre-Prerender2.5 - Shadows - " + i);
                                    GL.Uniform1(5, (Lights[i].InternalLights[x] is LightOrtho) ? 1.0f : 0.0f);
                                    GL.Uniform1(4, Lights[i].InternalLights[x].transp ? 1.0f : 0.0f);
                                    TranspShadows = Lights[i].InternalLights[x].transp;
                                    Lights[i].InternalLights[x].SetProj(this);
                                    Engine.Shaders3D.Deferred.s_shadow = Engine.Shaders3D.Deferred.s_shadow.Bind();
                                    SetMatrix(2, Matrix4d.Identity);
                                    GraphicsUtil.CheckError("Pre-Prerender3 - Shadows - " + i);
                                    GL.Uniform1(5, (Lights[i].InternalLights[x] is LightOrtho) ? 1.0f : 0.0f);
                                    GL.Uniform1(4, Lights[i].InternalLights[x].transp ? 1.0f : 0.0f);
                                    TranspShadows = Lights[i].InternalLights[x].transp;
                                    Lights[i].InternalLights[x].SetProj(this);
                                    GraphicsUtil.CheckError("Pre-Prerender4 - Shadows - " + i);
                                    DrawBuffer(DrawBufferMode.ColorAttachment0);
                                    GL.BlendFunc(BlendingFactor.One, BlendingFactor.Zero);
                                    if (Lights[i] is SkyLight sky)
                                    {
                                        if (redraw || sky.InternalLights[x].NeedsUpdate)
                                        {
                                            sky.InternalLights[x].NeedsUpdate = false;
                                            BindFramebuffer(FramebufferTarget.Framebuffer, sky.FBO);
                                            DrawBuffer(DrawBufferMode.ColorAttachment0);
                                            GL.ClearBuffer(ClearBuffer.Color, 0, new float[] { 1f });
                                            GL.ClearBuffer(ClearBuffer.Depth, 0, new float[] { 1f });
                                            FBOid = FBOID.STATIC_SHADOWS;
                                            GraphicsUtil.CheckError("Prerender - Shadows - " + i);
                                            Render3D(this);
                                            BindFramebuffer(FramebufferTarget.Framebuffer, 0);
                                        }
                                        BindFramebuffer(FramebufferTarget.Framebuffer, fbo_shadow[lTID]);
                                        GL.BindFramebuffer(FramebufferTarget.ReadFramebuffer, sky.FBO);
                                        GL.BlitFramebuffer(0, 0, sky.TexWidth, sky.TexWidth, ltX, ltY, widX, widY, ClearBufferMask.DepthBufferBit, BlitFramebufferFilter.Nearest);
                                        GL.BindFramebuffer(FramebufferTarget.ReadFramebuffer, 0);
                                        if (Engine.EnableDynamicShadows)
                                        {
                                            //GL.ClearBuffer(ClearBuffer.Color, 0, new float[] { 1f });
                                            FBOid = FBOID.DYNAMIC_SHADOWS;
                                            Render3D(this);
                                        }
                                    }
                                    else if (!Lights[i].InternalLights[x].CastShadows)
                                    {
                                        BindFramebuffer(FramebufferTarget.Framebuffer, fbo_shadow[lTID]);
                                        GL.ClearBuffer(ClearBuffer.Depth, 0, new float[] { 1f });
                                        GL.ClearBuffer(ClearBuffer.Color, 0, new float[] { 1f });
                                    }
                                    else
                                    {
                                        BindFramebuffer(FramebufferTarget.Framebuffer, fbo_shadow[lTID]);
                                        GL.ClearBuffer(ClearBuffer.Depth, 0, new float[] { 1f });
                                        GL.ClearBuffer(ClearBuffer.Color, 0, new float[] { 1f });
                                        FBOid = FBOID.SHADOWS;
                                        Render3D(this);
                                    }
                                    FBOid = FBOID.NONE;
                                    n++;
                                    GraphicsUtil.CheckError("Postrender - Shadows - " + i);
                                    if (n >= LIGHTS_MAX)
                                    {
                                        goto complete;
                                    }
                                }
                            }
                        }
                    }
                }
                complete:
                OSetViewport();
                CFrust = tcf;
                BindFramebuffer(FramebufferTarget.Framebuffer, CurrentFBO);
                DrawBuffer(CurrentFBO == 0 ? DrawBufferMode.Back : DrawBufferMode.ColorAttachment0);
                CameraPos = campos;
                RenderingShadows = false;
                ShadowsOnly = false;
                timer.Stop();
                ShadowTime = (double)timer.ElapsedMilliseconds / 1000f;
                if (ShadowTime > ShadowSpikeTime)
                {
                    ShadowSpikeTime = ShadowTime;
                }
                StandardBlend();
                GraphicsUtil.CheckError("AfterShadows");
            }
        }
        /// <summary>
        /// Generate the G-Buffer ("FBO") for lighting and final passes.
        /// </summary>
        public void RenderPass_GBuffer()
        {
            Stopwatch timer = new Stopwatch();
            timer.Start();
            OSetViewport();
            Engine.Shaders3D.Deferred.s_fbodecal = Engine.Shaders3D.Deferred.s_fbodecal.Bind();
            GL.UniformMatrix4(1, false, ref PrimaryMatrix);
            GL.UniformMatrix4(2, false, ref IdentityMatrix);
            GL.Uniform4(4, new Vector4(Width, Height, Engine.ZNear, Engine.ZFar()));
            ViewPatchSeven?.Invoke();
            Engine.Shaders3D.Deferred.s_fbot = Engine.Shaders3D.Deferred.s_fbot.Bind();
            GL.UniformMatrix4(1, false, ref PrimaryMatrix);
            GL.UniformMatrix4(2, false, ref IdentityMatrix);
            GL.Uniform1(6, (float)Engine.GlobalTickTime);
            GraphicsUtil.CheckError("Render - GBuffer - Uniforms - 2");
            Engine.Shaders3D.Deferred.s_fbo = Engine.Shaders3D.Deferred.s_fbo.Bind();
            GL.UniformMatrix4(1, false, ref PrimaryMatrix);
            GL.UniformMatrix4(2, false, ref IdentityMatrix);
            GL.Uniform1(6, (float)Engine.GlobalTickTime);
            GraphicsUtil.CheckError("Render - GBuffer - 0");
            FBOid = FBOID.MAIN;
            RenderingShadows = false;
            CFrust = camFrust;
            GL.ActiveTexture(TextureUnit.Texture0);
            RS4P.Bind(this);
            RS4P.Clear();
            RenderLights = true;
            RenderSpecular = true;
            Engine.Rendering.SetColor(Color4.White, this);
            StandardBlend();
            GraphicsUtil.CheckError("Render - GBuffer - 1");
            if (Engine.Render3DView || Engine.Client.VR != null)
            {
                Viewport(Width / 2, 0, Width / 2, Height);
                Render3D(this);
                CFrust = cf2;
                Viewport(0, 0, Width / 2, Height);
                CameraPos = cameraBasePos - cameraAdjust;
                ViewPatchEight?.Invoke();
                Engine.Shaders3D.Deferred.s_fbot = Engine.Shaders3D.Deferred.s_fbot.Bind();
                GL.UniformMatrix4(1, false, ref PrimaryMatrix_OffsetFor3D);
                Engine.Shaders3D.Deferred.s_fbo = Engine.Shaders3D.Deferred.s_fbo.Bind();
                GL.UniformMatrix4(1, false, ref PrimaryMatrix_OffsetFor3D);
                Matrix4 orig = PrimaryMatrix;
                PrimaryMatrix = PrimaryMatrix_OffsetFor3D;
                Matrix4 orig_out = OutViewMatrix;
                OutViewMatrix = OutView_OffsetVR;
                IsSecondEye = true;
                Render3D(this);
                IsSecondEye = false;
                OutViewMatrix = orig_out;
                PrimaryMatrix = orig;
                Viewport(0, 0, Width, Height);
                CameraPos = cameraBasePos + cameraAdjust;
                CFrust = camFrust;
            }
            else
            {
                Render3D(this);
            }
            GraphicsUtil.CheckError("AfterFBO");
            RenderPass_Decals();
            RenderPass_RefractionBuffer();
            timer.Stop();
            FBOTime = (double)timer.ElapsedMilliseconds / 1000f;
            if (FBOTime > FBOSpikeTime)
            {
                FBOSpikeTime = FBOTime;
            }
            GraphicsUtil.CheckError("Render - GBuffer - Final");
        }

        /// <summary>
        /// Adds decal data to the G-Buffer ("FBO").
        /// </summary>
        public void RenderPass_Decals()
        {
            Engine.Shaders3D.Deferred.s_fbodecal = Engine.Shaders3D.Deferred.s_fbodecal.Bind();
            RS4P.Unbind(this);
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, fbo_decal);
            GL.DrawBuffer(DrawBufferMode.ColorAttachment0);
            GL.BindFramebuffer(FramebufferTarget.ReadFramebuffer, RS4P.fbo);
            GL.BlitFramebuffer(0, 0, Width, Height, 0, 0, Width, Height, ClearBufferMask.DepthBufferBit, BlitFramebufferFilter.Nearest);
            GL.BindFramebuffer(FramebufferTarget.ReadFramebuffer, 0);
            RS4P.Bind(this);
            GL.ActiveTexture(TextureUnit.Texture4);
            GL.BindTexture(TextureTarget.Texture2D, fbo_decal_depth);
            GL.ActiveTexture(TextureUnit.Texture0);
            GL.DepthMask(false);
            GraphicsUtil.CheckError("Render - Decals - 0");
            if (Engine.Render3DView  || Engine.Client.VR != null)
            {
                Viewport(Width / 2, 0, Width / 2, Height);
                DecalRender?.Invoke(this);
                CFrust = cf2;
                Viewport(0, 0, Width / 2, Height);
                CameraPos = cameraBasePos - cameraAdjust;
                GL.UniformMatrix4(1, false, ref PrimaryMatrix_OffsetFor3D);
                Matrix4 orig = PrimaryMatrix;
                PrimaryMatrix = PrimaryMatrix_OffsetFor3D;
                Matrix4 orig_out = OutViewMatrix;
                OutViewMatrix = OutView_OffsetVR;
                IsSecondEye = true;
                DecalRender?.Invoke(this);
                IsSecondEye = false;
                OutViewMatrix = orig_out;
                PrimaryMatrix = orig;
                Viewport(0, 0, Width, Height);
                CameraPos = cameraBasePos + cameraAdjust;
                CFrust = camFrust;
            }
            else
            {
                DecalRender?.Invoke(this);
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
            FBOid = FBOID.REFRACT;
            ViewPatchNine?.Invoke();
            Engine.Shaders3D.Deferred.s_fbo_refract = Engine.Shaders3D.Deferred.s_fbo_refract.Bind();
            GL.Uniform1(6, (float)Engine.GlobalTickTime);
            GL.UniformMatrix4(1, false, ref PrimaryMatrix);
            GL.UniformMatrix4(2, false, ref IdentityMatrix);
            GL.DepthMask(false);
            GraphicsUtil.CheckError("Render - Refract - 0");
            if (Engine.Render3DView || Engine.Client.VR != null)
            {
                Viewport(Width / 2, 0, Width / 2, Height);
                Render3D(this);
                CFrust = cf2;
                Viewport(0, 0, Width / 2, Height);
                CameraPos = cameraBasePos - cameraAdjust;
                ViewPatchTen?.Invoke();
                Engine.Shaders3D.Deferred.s_fbo_refract = Engine.Shaders3D.Deferred.s_fbo_refract.Bind();
                GL.UniformMatrix4(1, false, ref PrimaryMatrix_OffsetFor3D);
                Matrix4 orig = PrimaryMatrix;
                PrimaryMatrix = PrimaryMatrix_OffsetFor3D;
                Matrix4 orig_out = OutViewMatrix;
                OutViewMatrix = OutView_OffsetVR;
                IsSecondEye = true;
                Render3D(this);
                IsSecondEye = false;
                OutViewMatrix = orig_out;
                PrimaryMatrix = orig;
                Viewport(0, 0, Width, Height);
                CameraPos = cameraBasePos + cameraAdjust;
                CFrust = camFrust;
            }
            else
            {
                Render3D(this);
            }
            GraphicsUtil.CheckError("AfterRefract");
            GL.DepthMask(true);
            RenderLights = false;
            RenderSpecular = false;
            RS4P.Unbind(this);
            FBOid = FBOID.NONE;
        }

        /// <summary>
        /// Light source addition render passes.
        /// </summary>
        public void RenderPass_Lights()
        {
            Stopwatch timer = new Stopwatch();
            timer.Start();
            BindFramebuffer(FramebufferTarget.Framebuffer, fbo_main);
            DrawBuffer(DrawBufferMode.ColorAttachment0);
            GL.ClearBuffer(ClearBuffer.Color, 0, new float[] { 0.0f, 0.0f, 0.0f, RenderClearAlpha });
            if (Engine.Deferred_Shadows)
            {
                if (Engine.Deferred_SSAO)
                {
                    Engine.Shaders3D.Deferred.s_shadowadder_ssao = Engine.Shaders3D.Deferred.s_shadowadder_ssao.Bind();
                }
                else
                {
                    Engine.Shaders3D.Deferred.s_shadowadder = Engine.Shaders3D.Deferred.s_shadowadder.Bind();
                }
                GL.Uniform1(3, ShadowBlur);
            }
            else
            {
                if (Engine.Deferred_SSAO)
                {
                    Engine.Shaders3D.Deferred.s_lightadder_ssao = Engine.Shaders3D.Deferred.s_lightadder_ssao.Bind();
                }
                else
                {
                    Engine.Shaders3D.Deferred.s_lightadder = Engine.Shaders3D.Deferred.s_lightadder.Bind();
                }
            }
            GL.ActiveTexture(TextureUnit.Texture1);
            GL.BindTexture(TextureTarget.Texture2D, RS4P.PositionTexture);
            GL.ActiveTexture(TextureUnit.Texture2);
            GL.BindTexture(TextureTarget.Texture2D, RS4P.NormalsTexture);
            GL.ActiveTexture(TextureUnit.Texture3);
            GL.BindTexture(TextureTarget.Texture2D, RS4P.DepthTexture);
            GL.ActiveTexture(TextureUnit.Texture4);
            GL.BindTexture(TextureTarget.Texture2D, 0);
            GL.BindTexture(TextureTarget.Texture2DArray, fbo_shadow_tex);
            GL.ActiveTexture(TextureUnit.Texture5);
            GL.BindTexture(TextureTarget.Texture2D, RS4P.RenderhintTexture);
            GL.ActiveTexture(TextureUnit.Texture6);
            GL.BindTexture(TextureTarget.Texture2D, RS4P.DiffuseTexture);
            GL.Uniform3(4, ambient.ToOpenTK());
            GL.UniformMatrix4(1, false, ref SimpleOrthoMatrix);
            GL.UniformMatrix4(2, false, ref IdentityMatrix);
            GL.Disable(EnableCap.CullFace);
            GL.Disable(EnableCap.DepthTest);
            TranspBlend();
            if (Engine.Deferred_Lights)
            {
                float[] light_dat = new float[LIGHTS_MAX * 16];
                float[] shadowmat_dat = new float[LIGHTS_MAX * 16];
                int c = 0;
                // TODO: An ambient light source?
                for (int i = 0; i < Lights.Count; i++)
                {
                    if (Lights[i] is SkyLight || camFrust == null || camFrust.ContainsSphere(Lights[i].EyePos, Lights[i].MaxDistance))
                    {
                        double d1 = (Lights[i].EyePos - CameraPos).LengthSquared();
                        double d2 = LightMaxDistance * LightMaxDistance + Lights[i].MaxDistance * Lights[i].MaxDistance;
                        double maxrangemult = 0;
                        if (d1 < d2 * 4 || Lights[i] is SkyLight)
                        {
                            maxrangemult = 1;
                        }
                        else if (d1 < d2 * 6)
                        {
                            maxrangemult = 1 - ((d1 - (d2 * 4)) / ((d2 * 6) - (d2 * 4)));
                        }
                        if (maxrangemult > 0)
                        {
                            if (Lights[i] is PointLight pl && !pl.CastShadows)
                            {
                                Matrix4 smat = Matrix4.Identity;
                                Vector3d eyep = pl.EyePos.ToOpenTK3D() - CameraPos.ToOpenTK3D();
                                Vector3 col = Lights[i].InternalLights[0].color * (float)maxrangemult;
                                Matrix4 light_data = new Matrix4(
                                    (float)eyep.X, (float)eyep.Y, (float)eyep.Z, // light_pos
                                    0.7f, // diffuse_albedo
                                    0.7f, // specular_albedo
                                   0.0f, // should_sqrt
                                    col.X, col.Y, col.Z, // light_color
                                    (Lights[i].InternalLights[0].maxrange <= 0 ? LightMaximum : Lights[i].InternalLights[0].maxrange), // light_radius
                                    0f, 0f, 0f, // eye_pos
                                    2.0f, // light_type
                                    1f / ShadowTexSize(), // tex_size
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
                                if (c >= LIGHTS_MAX)
                                {
                                    goto lights_apply;
                                }
                            }
                            else
                            {
                                for (int x = 0; x < Lights[i].InternalLights.Count; x++)
                                {
                                    if (Lights[i].InternalLights[x].color.LengthSquared <= 0.01)
                                    {
                                        continue;
                                    }
                                    Matrix4 smat = Lights[i].InternalLights[x].GetMatrix(this);
                                    Vector3d eyep = Lights[i] is SkyLight se ? -se.Direction.ToOpenTK3D() : Lights[i].InternalLights[x].eye - CameraPos.ToOpenTK3D();
                                    Vector3 col = Lights[i].InternalLights[x].color * (float)maxrangemult;
                                    Matrix4 light_data = new Matrix4(
                                        (float)eyep.X, (float)eyep.Y, (float)eyep.Z, // light_pos
                                        0.7f, // diffuse_albedo
                                        0.7f, // specular_albedo
                                        Lights[i].InternalLights[x] is LightOrtho ? 1.0f : 0.0f, // should_sqrt
                                        col.X, col.Y, col.Z, // light_color
                                        Lights[i].InternalLights[x] is LightOrtho ? LightMaximum : (Lights[i].InternalLights[0].maxrange <= 0 ? LightMaximum : Lights[i].InternalLights[0].maxrange), // light_radius
                                        0f, 0f, 0f, // eye_pos
                                        Lights[i] is SpotLight ? 1.0f : 0.0f, // light_type
                                        1f / ShadowTexSize(), // tex_size
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
                                    if (c >= LIGHTS_MAX)
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
                GL.BindTexture(TextureTarget.Texture2DArray, fbo_shadow_tex);
                GL.Uniform2(7, new Vector2(Engine.ZNear, Engine.ZFar()));
                GL.UniformMatrix4(8, false, ref PrimaryMatrix); // TODO: In 3D/VR, render both eyes separately here for SSAO accuracy?
                GL.Uniform1(9, (float)c);
                GL.UniformMatrix4(10, LIGHTS_MAX, false, shadowmat_dat);
                GL.UniformMatrix4(10 + LIGHTS_MAX, LIGHTS_MAX, false, light_dat);
                Engine.Rendering.RenderRectangle(-1, -1, 1, 1);
                GL.ActiveTexture(TextureUnit.Texture0);
                GL.BindTexture(TextureTarget.Texture2D, 0);
                StandardBlend();
                GraphicsUtil.CheckError("AfterLighting");
                RenderPass_HDR();
            }
            else
            {
                GL.ActiveTexture(TextureUnit.Texture0);
                GL.BindTexture(TextureTarget.Texture2D, hdrtex);
                float[] data = new float[HDR_SPREAD * HDR_SPREAD];
                for (int i = 0; i < data.Length; i++)
                {
                    data[i] = 1f;
                }
                GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.R32f, HDR_SPREAD, HDR_SPREAD, 0, PixelFormat.Red, PixelType.Float, data);
            }
            GraphicsUtil.CheckError("AfterAllLightCode");
            RenderPass_LightsToBase();
            int lightc = RenderPass_Transparents();
            RenderPass_Bloom(lightc);
            timer.Stop();
            LightsTime = (double)timer.ElapsedMilliseconds / 1000f;
            if (LightsTime > LightsSpikeTime)
            {
                LightsSpikeTime = LightsTime;
            }
            timer.Reset();
            GraphicsUtil.CheckError("AtEnd");
        }

        /// <summary>
        /// Calculates the brightness value for High Dynamic Range rendering.
        /// </summary>
        public void RenderPass_HDR()
        {
            if (Engine.Deferred_Lights && Engine.Deferred_HDR)
            {
                Engine.Shaders3D.Deferred.s_hdrpass.Bind();
                GL.ActiveTexture(TextureUnit.Texture0);
                GL.Disable(EnableCap.CullFace);
                GL.Disable(EnableCap.DepthTest);
                GL.BindTexture(TextureTarget.Texture2D, fbo_texture);
                GL.BlendFunc(BlendingFactor.One, BlendingFactor.Zero);
                BindFramebuffer(FramebufferTarget.Framebuffer, hdrfbo);
                DrawBuffer(DrawBufferMode.ColorAttachment0);
                GL.UniformMatrix4(1, false, ref SimpleOrthoMatrix);
                GL.UniformMatrix4(2, false, ref IdentityMatrix);
                GL.Uniform2(4, new Vector2(Width, Height));
                Engine.Rendering.RenderRectangle(-1, -1, 1, 1);
                StandardBlend();
                GraphicsUtil.CheckError("AfterHDRRead");
            }
            else
            {
                GL.ActiveTexture(TextureUnit.Texture0);
                GL.BindTexture(TextureTarget.Texture2D, hdrtex);
                float[] data = new float[HDR_SPREAD * HDR_SPREAD];
                for (int i = 0; i < data.Length; i++)
                {
                    data[i] = 1f;
                }
                GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.R32f, HDR_SPREAD, HDR_SPREAD, 0, PixelFormat.Red, PixelType.Float, data);
            }
        }

        /// <summary>
        /// Applies lights to the base buffer.
        /// </summary>
        public void RenderPass_LightsToBase()
        {
            BindFramebuffer(FramebufferTarget.Framebuffer, fbo_godray_main);
            if (Engine.Deferred_Lights)
            {
                if (Engine.Deferred_Toonify)
                {
                    Engine.Shaders3D.Deferred.s_finalgodray_lights_toonify = Engine.Shaders3D.Deferred.s_finalgodray_lights_toonify.Bind();
                }
                else
                {
                    if (Engine.Deferred_MotionBlur)
                    {
                        Engine.Shaders3D.Deferred.s_finalgodray_lights_motblur = Engine.Shaders3D.Deferred.s_finalgodray_lights_motblur.Bind();
                    }
                    else
                    {
                        Engine.Shaders3D.Deferred.s_finalgodray_lights = Engine.Shaders3D.Deferred.s_finalgodray_lights.Bind();
                    }
                }
            }
            else
            {
                if (Engine.Deferred_Toonify)
                {
                    Engine.Shaders3D.Deferred.s_finalgodray_toonify = Engine.Shaders3D.Deferred.s_finalgodray_toonify.Bind();
                }
                else
                {
                    Engine.Shaders3D.Deferred.s_finalgodray = Engine.Shaders3D.Deferred.s_finalgodray.Bind();
                }
            }
            BufferDontTouch = true;
            GL.DrawBuffers(2, new DrawBuffersEnum[] { DrawBuffersEnum.ColorAttachment0, DrawBuffersEnum.ColorAttachment1 });
            GL.ClearBuffer(ClearBuffer.Color, 0, new float[] { 0f, 0f, 0f, 0f });
            GL.ClearBuffer(ClearBuffer.Color, 1, new float[] { 0f, 0f, 0f, 0f });
            GL.BlendFuncSeparate(1, BlendingFactorSrc.SrcColor, BlendingFactorDest.Zero, BlendingFactorSrc.SrcAlpha, BlendingFactorDest.Zero);
            GL.Uniform3(8, DOF_Target.ToOpenTK());
            GL.Uniform1(9, DOF_Factor);
            GL.Uniform1(10, MainEXP * Engine.Exposure);
            float fogDist = 1.0f / Engine.FogMaxDist();
            fogDist *= fogDist;
            Vector2 zfar_rel = new Vector2(Engine.ZNear, Engine.ZFar());
            GL.Uniform1(16, fogDist);
            GL.Uniform2(17, ref zfar_rel);
            GL.Uniform4(18, new Vector4(FogCol.ToOpenTK(), FogAlpha));
            // TODO: If thick fog, blur the environment? Or some similar head-in-a-block effect!
            GL.Uniform1(19, DesaturationAmount);
            GL.Uniform3(20, new Vector3(0, 0, 0));
            GL.Uniform3(21, DesaturationColor);
            GL.UniformMatrix4(22, false, ref PrimaryMatrix);
            GL.Uniform1(24, (float)Width);
            GL.Uniform1(25, (float)Height);
            GL.Uniform1(26, (float)Engine.GlobalTickTime);
            Vector4 v = Vector4.Transform(new Vector4(PForward.ToOpenTK(), 1f), PrimaryMatrix);
            Vector2 v2 = (v.Xy / v.W);
            Vector2 rel = (pfRes - v2) * 0.01f;
            if (float.IsNaN(rel.X) || float.IsInfinity(rel.X) || float.IsNaN(rel.Y) || float.IsInfinity(rel.Y))
            {
                rel = new Vector2(0f, 0f);
            }
            GL.Uniform2(27, ref rel);
            pfRes = v2;
            GL.Uniform1(28, Engine.Deferred_Grayscale ? 1f : 0f);
            GL.ActiveTexture(TextureUnit.Texture3);
            GL.BindTexture(TextureTarget.Texture2D, RS4P.DepthTexture);
            GL.ActiveTexture(TextureUnit.Texture4);
            GL.BindTexture(TextureTarget.Texture2D, fbo_texture);
            GL.ActiveTexture(TextureUnit.Texture7);
            GL.BindTexture(TextureTarget.Texture2D, hdrtex);
            GL.ActiveTexture(TextureUnit.Texture6);
            GL.BindTexture(TextureTarget.Texture2D, RS4P.Rh2Texture);
            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, RS4P.DiffuseTexture);
            GL.UniformMatrix4(1, false, ref SimpleOrthoMatrix);
            GL.UniformMatrix4(2, false, ref IdentityMatrix);
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
            GL.BindTexture(TextureTarget.Texture2D, 0);
            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, 0);
            GL.Enable(EnableCap.DepthTest);
            GraphicsUtil.CheckError("PreBlendFunc");
            //GL.BlendFunc(1, BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);
            GraphicsUtil.CheckError("PreAFRFBO");
            BindFramebuffer(FramebufferTarget.Framebuffer, CurrentFBO);
            DrawBuffer(CurrentFBO == 0 ? DrawBufferMode.Back : DrawBufferMode.ColorAttachment0);
            BufferDontTouch = false;
            GraphicsUtil.CheckError("AFRFBO_1");
            BindFramebuffer(FramebufferTarget.ReadFramebuffer, (int)RS4P.fbo); // TODO: is this line and line below needed?
            GL.BlitFramebuffer(0, 0, Width, Height, 0, 0, Width, Height, ClearBufferMask.DepthBufferBit, BlitFramebufferFilter.Nearest);
            GraphicsUtil.CheckError("AFRFBO_2");
            BindFramebuffer(FramebufferTarget.ReadFramebuffer, fbo_godray_main);
            GraphicsUtil.CheckError("AFRFBO_3");
            GL.BlitFramebuffer(0, 0, Width, Height, 0, 0, Width, Height, ClearBufferMask.ColorBufferBit, BlitFramebufferFilter.Nearest);
            GraphicsUtil.CheckError("AFRFBO_4");
            BindFramebuffer(FramebufferTarget.ReadFramebuffer, 0);
            GL.Enable(EnableCap.CullFace);
            GraphicsUtil.CheckError("AfterFirstRender");
            PostFirstRender?.Invoke();
            GraphicsUtil.CheckError("AfterPostFirstRender");
        }

        /// <summary>
        /// Used for motion blurring.
        /// </summary>
        Vector2 pfRes = Vector2.Zero;

        /// <summary>
        /// Render transparent data.
        /// </summary>
        public int RenderPass_Transparents()
        {
            ViewPatchEleven?.Invoke();
            if (Engine.Deferred_TransparentLights)
            {
                if (Engine.Deferred_Shadows)
                {
                    if (Engine.AllowLL)
                    {
                        Engine.Shaders3D.Deferred.s_transponlylitsh_ll = Engine.Shaders3D.Deferred.s_transponlylitsh_ll.Bind();
                        FBOid = FBOID.TRANSP_SHADOWS_LL;
                    }
                    else
                    {
                        Engine.Shaders3D.Deferred.s_transponlylitsh = Engine.Shaders3D.Deferred.s_transponlylitsh.Bind();
                        FBOid = FBOID.TRANSP_SHADOWS;
                    }
                }
                else
                {
                    if (Engine.AllowLL)
                    {
                        Engine.Shaders3D.Deferred.s_transponlylit_ll = Engine.Shaders3D.Deferred.s_transponlylit_ll.Bind();
                        FBOid = FBOID.TRANSP_LIT_LL;
                    }
                    else
                    {
                        Engine.Shaders3D.Deferred.s_transponlylit = Engine.Shaders3D.Deferred.s_transponlylit.Bind();
                        FBOid = FBOID.TRANSP_LIT;
                    }
                }
            }
            else
            {
                if (Engine.AllowLL)
                {
                    Engine.Shaders3D.Deferred.s_transponly_ll = Engine.Shaders3D.Deferred.s_transponly_ll.Bind();
                    FBOid = FBOID.TRANSP_LL;
                }
                else
                {
                    Engine.Shaders3D.Deferred.s_transponly = Engine.Shaders3D.Deferred.s_transponly.Bind();
                    FBOid = FBOID.TRANSP_UNLIT;
                }
            }
            GL.UniformMatrix4(1, false, ref PrimaryMatrix);
            GL.UniformMatrix4(2, false, ref IdentityMatrix);
            GL.Uniform1(4, DesaturationAmount);
            GL.DepthMask(false);
            if (Engine.AllowLL || !Engine.Deferred_BrightTransp)
            {
                StandardBlend();
            }
            else
            {
                TranspBlend();
            }
            BindFramebuffer(FramebufferTarget.Framebuffer, transp_fbo_main);
            DrawBuffer(DrawBufferMode.ColorAttachment0);
            BindFramebuffer(FramebufferTarget.ReadFramebuffer, (int)RS4P.fbo);
            GL.BlitFramebuffer(0, 0, Width, Height, 0, 0, Width, Height, ClearBufferMask.DepthBufferBit, BlitFramebufferFilter.Nearest);
            BindFramebuffer(FramebufferTarget.ReadFramebuffer, 0);
            GL.ClearBuffer(ClearBuffer.Color, 0, new float[] { 0f, 0f, 0f, 0f });
            int lightc = 0;
            GraphicsUtil.CheckError("PreTransp");
            // TODO: VR Patch!
            if (Engine.Render3DView || Engine.Client.VR != null)
            {
                Viewport(Width / 2, 0, Width / 2, Height);
                CameraPos = cameraBasePos + cameraAdjust;
                RenderTransp(ref lightc);
                CFrust = cf2;
                Viewport(0, 0, Width / 2, Height);
                CFrust = cf2;
                ViewPatchTwelve?.Invoke();
                if (Engine.Deferred_TransparentLights)
                {
                    if (Engine.Deferred_Shadows)
                    {
                        if (Engine.AllowLL)
                        {
                            Engine.Shaders3D.Deferred.s_transponlylitsh_ll = Engine.Shaders3D.Deferred.s_transponlylitsh_ll.Bind();
                            FBOid = FBOID.TRANSP_SHADOWS_LL;
                        }
                        else
                        {
                            Engine.Shaders3D.Deferred.s_transponlylitsh = Engine.Shaders3D.Deferred.s_transponlylitsh.Bind();
                            FBOid = FBOID.TRANSP_SHADOWS;
                        }
                    }
                    else
                    {
                        if (Engine.AllowLL)
                        {
                            Engine.Shaders3D.Deferred.s_transponlylit_ll = Engine.Shaders3D.Deferred.s_transponlylit_ll.Bind();
                            FBOid = FBOID.TRANSP_LIT_LL;
                        }
                        else
                        {
                            Engine.Shaders3D.Deferred.s_transponlylit = Engine.Shaders3D.Deferred.s_transponlylit.Bind();
                            FBOid = FBOID.TRANSP_LIT;
                        }
                    }
                }
                else
                {
                    if (Engine.AllowLL)
                    {
                        Engine.Shaders3D.Deferred.s_transponly_ll = Engine.Shaders3D.Deferred.s_transponly_ll.Bind();
                        FBOid = FBOID.TRANSP_LL;
                    }
                    else
                    {
                        Engine.Shaders3D.Deferred.s_transponly = Engine.Shaders3D.Deferred.s_transponly.Bind();
                        FBOid = FBOID.TRANSP_UNLIT;
                    }
                }
                GL.UniformMatrix4(1, false, ref PrimaryMatrix_OffsetFor3D);
                CameraPos = cameraBasePos - cameraAdjust;
                Matrix4 orig = PrimaryMatrix;
                PrimaryMatrix = PrimaryMatrix_OffsetFor3D;
                Matrix4 orig_out = OutViewMatrix;
                OutViewMatrix = OutView_OffsetVR;
                IsSecondEye = true;
                RenderTransp(ref lightc, cf2);
                IsSecondEye = false;
                OutViewMatrix = orig_out;
                PrimaryMatrix = orig;
                Viewport(0, 0, Width, Height);
                CameraPos = cameraBasePos + cameraAdjust;
                CFrust = camFrust;
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
            BindFramebuffer(FramebufferTarget.Framebuffer, CurrentFBO);
            DrawBuffer(CurrentFBO == 0 ? DrawBufferMode.Back : DrawBufferMode.ColorAttachment0);
            StandardBlend();
            FBOid = FBOID.NONE;
            GL.ActiveTexture(TextureUnit.Texture0);
            GL.Disable(EnableCap.CullFace);
            GL.ClearBuffer(ClearBuffer.Depth, 0, new float[] { 1f });
            GL.Disable(EnableCap.DepthTest);
            GraphicsUtil.CheckError("PreGR");
            if (GodRays)
            {
                // TODO: 3d stuff for GodRays.
                GL.ActiveTexture(TextureUnit.Texture1);
                GL.BindTexture(TextureTarget.Texture2D, RS4P.DepthTexture);
                GL.ActiveTexture(TextureUnit.Texture0);
                GL.BindTexture(TextureTarget.Texture2D, fbo_godray_texture2);
                Engine.Shaders3D.Deferred.s_godray = Engine.Shaders3D.Deferred.s_godray.Bind();
                GL.UniformMatrix4(1, false, ref SimpleOrthoMatrix);
                GL.UniformMatrix4(2, false, ref IdentityMatrix);
                GL.Uniform1(6, MainEXP * Engine.Exposure);
                GL.Uniform1(7, Width / (float)Height);
                if (SunLocation.IsNaN())
                {
                    GL.Uniform2(8, new Vector2(-10f, -10f));
                }
                else
                {
                    Vector4d v = Vector4d.Transform(new Vector4d(SunLocation.ToOpenTK3D(), 1.0), PrimaryMatrixd);
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
                TranspBlend();
                Engine.Rendering.RenderRectangle(-1, -1, 1, 1);
                StandardBlend();
            }
            GraphicsUtil.CheckError("PostGR");
            {
                // TODO: Merge transp-to-screen and GR pass?
                //GL.Enable(EnableCap.DepthTest);
                GL.BindTexture(TextureTarget.Texture2D, transp_fbo_texture);
                Engine.Shaders3D.Deferred.s_transpadder = Engine.Shaders3D.Deferred.s_transpadder.Bind();
                GL.UniformMatrix4(1, false, ref SimpleOrthoMatrix);
                GL.UniformMatrix4(2, false, ref IdentityMatrix);
                GL.Uniform1(3, (float)lightc);
                Engine.Rendering.RenderRectangle(-1, -1, 1, 1);
            }
            GL.UseProgram(0);
            GL.BindTexture(TextureTarget.Texture2D, 0);
            GL.DepthMask(true);
            GL.Enable(EnableCap.DepthTest);
            GL.Enable(EnableCap.CullFace);
            GraphicsUtil.CheckError("WrapUp");
            BindFramebuffer(FramebufferTarget.Framebuffer, 0);
            DrawBuffer(DrawBufferMode.Back);
        }

        /// <summary>
        /// Render transparent objects into a temporary buffer.
        /// </summary>
        void RenderTransp(ref int lightc, Frustum frustumToUse = null)
        {
            if (Engine.AllowLL)
            {
                GL.ActiveTexture(TextureUnit.Texture4);
                GL.BindTexture(TextureTarget.Texture2DArray, TransTexs[0]);
                GL.BindImageTexture(4, TransTexs[0], 0, false, 0, TextureAccess.ReadWrite, SizedInternalFormat.R32ui);
                GL.ActiveTexture(TextureUnit.Texture5);
                GL.BindTexture(TextureTarget.TextureBuffer, TransTexs[1]);
                GL.BindImageTexture(5, TransTexs[1], 0, false, 0, TextureAccess.ReadWrite, SizedInternalFormat.Rgba32f);
                GL.ActiveTexture(TextureUnit.Texture6);
                GL.BindTexture(TextureTarget.TextureBuffer, TransTexs[2]);
                GL.BindImageTexture(6, TransTexs[2], 0, false, 0, TextureAccess.ReadWrite, SizedInternalFormat.R32ui);
                GL.ActiveTexture(TextureUnit.Texture7);
                GL.BindTexture(TextureTarget.TextureBuffer, TransTexs[3]);
                GL.BindImageTexture(7, TransTexs[3], 0, false, 0, TextureAccess.ReadWrite, SizedInternalFormat.R32ui);
                GL.ActiveTexture(TextureUnit.Texture0);
                Engine.Shaders3D.Deferred.s_ll_clearer.Bind();
                GL.Uniform2(4, new Vector2(Width, Height));
                Matrix4 flatProj = Matrix4.CreateOrthographicOffCenter(-1, 1, 1, -1, -1, 1);
                GL.UniformMatrix4(1, false, ref flatProj);
                GL.UniformMatrix4(2, false, ref IdentityMatrix);
                GL.Uniform2(4, new Vector2(Width, Height));
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
                Engine.Shaders3D.Deferred.s_ll_fpass.Bind();
                GL.Uniform2(4, new Vector2(Width, Height));
                GL.UniformMatrix4(1, false, ref flatProj);
                GL.UniformMatrix4(2, false, ref IdentityMatrix);
                GL.Uniform2(4, new Vector2(Width, Height));
                Engine.Rendering.RenderRectangle(-1, -1, 1, 1);
            }
            else
            {
                RenderTranspInt(ref lightc, frustumToUse);
            }
        }
        
        void RenderTranspInt(ref int lightc, Frustum frustumToUse)
        {
            if (frustumToUse == null)
            {
                frustumToUse = camFrust;
            }
            if (Engine.Deferred_TransparentLights)
            {
                RenderLights = true;
                for (int i = 0; i < Lights.Count; i++)
                {
                    if (Lights[i] is SkyLight || frustumToUse == null || frustumToUse.ContainsSphere(Lights[i].EyePos, Lights[i].MaxDistance))
                    {
                        for (int x = 0; x < Lights[i].InternalLights.Count; x++)
                        {
                            lightc++;
                        }
                    }
                }
                int c = 0;
                float[] l_dats1 = new float[LIGHTS_MAX * 16];
                float[] s_mats = new float[LIGHTS_MAX * 16];
                for (int i = 0; i < Lights.Count; i++)
                {
                    if (Lights[i] is SkyLight || frustumToUse == null || frustumToUse.ContainsSphere(Lights[i].EyePos, Lights[i].MaxDistance))
                    {
                        for (int x = 0; x < Lights[i].InternalLights.Count; x++)
                        {
                            Matrix4 lmat = Lights[i].InternalLights[x].GetMatrix(this);
                            float maxrange = (Lights[i].InternalLights[x] is LightOrtho) ? LightMaximum : Lights[i].InternalLights[x].maxrange;
                            Matrix4 matxyz = new Matrix4(Vector4.Zero, Vector4.Zero, Vector4.Zero, Vector4.Zero);
                            matxyz[0, 0] = maxrange <= 0 ? LightMaximum : maxrange;
                            matxyz[0, 1] = (float)(Lights[i].EyePos.X - RenderRelative.X);
                            matxyz[0, 2] = (float)(Lights[i].EyePos.Y - RenderRelative.Y);
                            matxyz[0, 3] = (float)(Lights[i].EyePos.Z - RenderRelative.Z);
                            matxyz[1, 0] = Lights[i].InternalLights[x].color.X;
                            matxyz[1, 1] = Lights[i].InternalLights[x].color.Y;
                            matxyz[1, 2] = Lights[i].InternalLights[x].color.Z;
                            matxyz[1, 3] = (Lights[i] is SpotLight) ? 1f : 0f;
                            matxyz[2, 0] = (Lights[i].InternalLights[x] is LightOrtho) ? 1f : 0f;
                            matxyz[2, 1] = 1f / ShadowTexSize();
                            matxyz[2, 2] = MainEXP * Engine.Exposure;
                            matxyz[2, 3] = (float)lightc; // TODO: Move this to a generic
                            matxyz[3, 0] = (float)ambient.X; // TODO: Remove ambient
                            matxyz[3, 1] = (float)ambient.Y;
                            matxyz[3, 2] = (float)ambient.Z;
                            for (int mx = 0; mx < 4; mx++)
                            {
                                for (int my = 0; my < 4; my++)
                                {
                                    s_mats[c * 16 + mx * 4 + my] = lmat[mx, my];
                                    l_dats1[c * 16 + mx * 4 + my] = matxyz[mx, my];
                                }
                            }
                            c++;
                            if (c >= LIGHTS_MAX)
                            {
                                goto lights_apply;
                            }
                        }
                    }
                }
                lights_apply:
                GraphicsUtil.CheckError("PreRenderTranspLights");
                if (Engine.Deferred_Shadows)
                {
                    if (Engine.AllowLL)
                    {
                        Engine.Shaders3D.Deferred.s_transponlylitsh_ll_particles = Engine.Shaders3D.Deferred.s_transponlylitsh_ll_particles.Bind();
                    }
                    else
                    {
                        Engine.Shaders3D.Deferred.s_transponlylitsh_particles = Engine.Shaders3D.Deferred.s_transponlylitsh_particles.Bind();
                    }
                }
                else
                {
                    if (Engine.AllowLL)
                    {
                        Engine.Shaders3D.Deferred.s_transponlylit_ll_particles = Engine.Shaders3D.Deferred.s_transponlylit_ll_particles.Bind();
                    }
                    else
                    {
                        Engine.Shaders3D.Deferred.s_transponlylit_particles = Engine.Shaders3D.Deferred.s_transponlylit_particles.Bind();
                    }
                }
                GraphicsUtil.CheckError("PreRenderTranspLights - 1.5");
                Matrix4 mat_lhelp = new Matrix4(c, Engine.ZNear, Engine.ZFar(), Width, Height, 0, 0, 0, 0, 0, 0, 0, (float)FogCol.X, (float)FogCol.Y, (float)FogCol.Z, FogAlpha);
                GL.UniformMatrix4(2, false, ref IdentityMatrix);
                GL.Uniform1(4, DesaturationAmount);
                //GL.Uniform1(7, (float)TheClient.GlobalTickTimeLocal);
                GL.Uniform2(8, new Vector2(Width, Height));
                GraphicsUtil.CheckError("PreRenderTranspLights - 1.75");
                GL.UniformMatrix4(9, false, ref mat_lhelp);
                GL.UniformMatrix4(20, LIGHTS_MAX, false, s_mats);
                GL.UniformMatrix4(20 + LIGHTS_MAX, LIGHTS_MAX, false, l_dats1);
                GraphicsUtil.CheckError("PreRenderTranspLights - 2");
                ViewPatchThirteen?.Invoke(mat_lhelp, s_mats, l_dats1);
                GraphicsUtil.CheckError("PreRenderTranspLights - 3");
                if (Engine.Deferred_Shadows)
                {
                    if (Engine.AllowLL)
                    {
                        Engine.Shaders3D.Deferred.s_transponlylitsh_ll = Engine.Shaders3D.Deferred.s_transponlylitsh_ll.Bind();
                    }
                    else
                    {
                        Engine.Shaders3D.Deferred.s_transponlylitsh = Engine.Shaders3D.Deferred.s_transponlylitsh.Bind();
                    }
                }
                else
                {
                    if (Engine.AllowLL)
                    {
                        Engine.Shaders3D.Deferred.s_transponlylit_ll = Engine.Shaders3D.Deferred.s_transponlylit_ll.Bind();
                    }
                    else
                    {
                        Engine.Shaders3D.Deferred.s_transponlylit = Engine.Shaders3D.Deferred.s_transponlylit.Bind();
                    }
                }
                GL.UniformMatrix4(2, false, ref IdentityMatrix);
                GL.Uniform2(8, new Vector2(Width, Height));
                GL.UniformMatrix4(9, false, ref mat_lhelp);
                GL.UniformMatrix4(20, LIGHTS_MAX, false, s_mats);
                GL.UniformMatrix4(20 + LIGHTS_MAX, LIGHTS_MAX, false, l_dats1);
                GL.ActiveTexture(TextureUnit.Texture4);
                GL.BindTexture(TextureTarget.Texture2DArray, fbo_shadow_tex);
                GL.ActiveTexture(TextureUnit.Texture0);
                GraphicsUtil.CheckError("PreparedRenderTranspLights");
                Render3D(this);
                GraphicsUtil.CheckError("PostRenderTranspLights");
                GL.ActiveTexture(TextureUnit.Texture2);
                GL.BindTexture(TextureTarget.Texture2D, 0);
                GL.ActiveTexture(TextureUnit.Texture0);
                RenderLights = false;
            }
            else
            {
                if (Engine.AllowLL)
                {
                    Matrix4 matabc = new Matrix4(Vector4.Zero, Vector4.Zero, Vector4.Zero, Vector4.Zero);
                    matabc[0, 3] = (float)Width;
                    matabc[1, 3] = (float)Height;
                    ViewPatchFourteen?.Invoke(matabc);
                    Engine.Shaders3D.Deferred.s_transponly_ll.Bind();
                    //GL.UniformMatrix4(1, false, ref combined);
                    GL.UniformMatrix4(2, false, ref IdentityMatrix);
                    GL.UniformMatrix4(9, false, ref matabc);
                }
                else
                {
                    ViewPatchFifteen?.Invoke();
                    Engine.Shaders3D.Deferred.s_transponly.Bind();
                    //GL.UniformMatrix4(1, false, ref combined);
                    GL.UniformMatrix4(2, false, ref IdentityMatrix);
                }
                Render3D(this);
            }
        }
        
        /// <summary>
        /// Reads HDR result from the GPU.
        /// </summary>
        public void FinalHDRGrab()
        {
            if (Engine.Deferred_HDR)
            {
                float[] rd = new float[HDR_SPREAD * HDR_SPREAD];
                BindFramebuffer(FramebufferTarget.Framebuffer, 0);
                BindFramebuffer(FramebufferTarget.ReadFramebuffer, hdrfbo);
                GL.ReadBuffer(ReadBufferMode.ColorAttachment0);
                GL.ReadPixels(0, 0, HDR_SPREAD, HDR_SPREAD, PixelFormat.Red, PixelType.Float, rd);
                BindFramebuffer(FramebufferTarget.ReadFramebuffer, 0);
                GL.ReadBuffer(ReadBufferMode.None);
                float exp = FindExp(rd);
                exp = Math.Max(Math.Min(exp, 5.0f), 0.4f);
                exp = 1.0f / exp;
                float stepUp = (float)Engine.Delta * 0.05f;
                float stepDown = stepUp * 5.0f;
                float relative = Math.Abs(MainEXP - exp);
                float modder = 3f * relative;
                stepUp *= modder;
                stepDown *= modder;
                if (exp > MainEXP + stepUp)
                {
                    MainEXP += stepUp;
                }
                else if (exp < MainEXP - stepDown)
                {
                    MainEXP -= stepDown;
                }
                else
                {
                    MainEXP = exp;
                }
            }
            else
            {
                MainEXP = 0.75f;
            }
        }
        
        /// <summary>
        /// Helper to find exposure.
        /// </summary>
        /// <param name="inp">Exposure array input.</param>
        public float FindExp(float[] inp)
        {
            float total = 0f;
            for (int i = 0; i < inp.Length; i++)
            {
                total += inp[i];
            }
            return total / (float)inp.Length;
        }

        /// <summary>
        /// Destroys the view.
        /// // TODO: Ensure that this fully destroys all data!
        /// </summary>
        public void Destroy()
        {
            if (CurrentFBO != 0)
            {
                GL.DeleteFramebuffer(CurrentFBO);
                GL.DeleteTexture(CurrentFBOTexture);
                GL.DeleteTexture(CurrentFBODepth);
            }
            if (transp_fbo_main != 0)
            {
                GL.DeleteFramebuffer(transp_fbo_main);
                GL.DeleteTexture(transp_fbo_texture);
                GL.DeleteTexture(transp_fbo_depthtex);
            }
            if (RS4P != null)
            {
                RS4P.Destroy();
                GL.DeleteFramebuffer(fbo_main);
                GL.DeleteTexture(fbo_texture);
                RS4P = null;
                fbo_main = 0;
                fbo_texture = 0;
                GraphicsUtil.CheckError("Load - View3D - Light - Deletes - 1");
                GL.DeleteFramebuffer(fbo_godray_main);
                GL.DeleteTexture(fbo_godray_texture);
                GL.DeleteTexture(fbo_godray_texture2);
                GL.DeleteFramebuffer(hdrfbo);
                GL.DeleteTexture(hdrtex);
                GraphicsUtil.CheckError("Load - View3D - Light - Deletes - 2");
                GL.DeleteFramebuffers(SHADOW_BITS_MAX + 1, fbo_shadow);
                GraphicsUtil.CheckError("Load - View3D - Light - Deletes - 3");
                GL.DeleteTexture(fbo_shadow_color);
                GL.DeleteTexture(fbo_shadow_tex);
                GL.DeleteFramebuffer(fbo_decal);
                GL.DeleteTexture(fbo_decal_tex);
                GL.DeleteTexture(fbo_decal_depth);
                GraphicsUtil.CheckError("Load - View3D - Light - Deletes - 4");
            }
        }
    }
    
    /// <summary>
    /// Helper for current rendering mode ID.
    /// </summary>
    public enum FBOID : byte
    {
        /// <summary>
        /// No relevant mode.
        /// </summary>
        NONE = 0,
        /// <summary>
        /// Main mode (FBO).
        /// </summary>
        MAIN = 1,
        /// <summary>
        /// Main mode (Extras: decals).
        /// </summary>
        MAIN_EXTRAS = 2,
        /// <summary>
        /// Transparency, no lights.
        /// </summary>
        TRANSP_UNLIT = 3,
        /// <summary>
        /// Shadows.
        /// </summary>
        SHADOWS = 4,
        /// <summary>
        /// Static shadows.
        /// </summary>
        STATIC_SHADOWS = 5,
        /// <summary>
        /// Dynamic shadows.
        /// </summary>
        DYNAMIC_SHADOWS = 6,
        /// <summary>
        /// Transparency (lights).
        /// </summary>
        TRANSP_LIT = 7,
        /// <summary>
        /// Transparency (lights and shadows).
        /// </summary>
        TRANSP_SHADOWS = 8,
        /// <summary>
        /// Transparency (LL).
        /// </summary>
        TRANSP_LL = 12,
        /// <summary>
        /// Transparency (lights and LL).
        /// </summary>
        TRANSP_LIT_LL = 13,
        /// <summary>
        /// Transparency (lights and shadows and LL).
        /// </summary>
        TRANSP_SHADOWS_LL = 14,
        /// <summary>
        /// Refraction helper.
        /// </summary>
        REFRACT = 21,
        /// <summary>
        /// Forward extras (decals).
        /// </summary>
        FORWARD_EXTRAS = 97,
        /// <summary>
        /// Forward transparency.
        /// </summary>
        FORWARD_TRANSP = 98,
        /// <summary>
        /// Forward opaque.
        /// </summary>
        FORWARD_SOLID = 99,
    }

    /// <summary>
    /// Helpers for <see cref="FBOID"/>.
    /// </summary>
    public static class FBOIDExtensions
    {
        /// <summary>
        /// Checks if the ID is the 'main + transparent' modes.
        /// </summary>
        /// <param name="id">The ID.</param>
        /// <returns>Whether it is.</returns>
        public static bool IsMainTransp(this FBOID id)
        {
            return id == FBOID.TRANSP_LIT || id == FBOID.TRANSP_LIT_LL || id == FBOID.TRANSP_LL || id == FBOID.TRANSP_SHADOWS || id == FBOID.TRANSP_SHADOWS_LL || id == FBOID.TRANSP_UNLIT;
        }

        /// <summary>
        /// Checks if the ID is the 'main + opaque' modes.
        /// </summary>
        /// <param name="id">The ID.</param>
        /// <returns>Whether it is.</returns>
        public static bool IsMainSolid(this FBOID id)
        {
            return id == FBOID.FORWARD_SOLID || id == FBOID.MAIN;
        }

        /// <summary>
        /// Checks if the ID is the 'solid (opaque)' modes.
        /// </summary>
        /// <param name="id">The ID.</param>
        /// <returns>Whether it is.</returns>
        public static bool IsSolid(this FBOID id)
        {
            return id == FBOID.SHADOWS || id == FBOID.STATIC_SHADOWS || id == FBOID.DYNAMIC_SHADOWS || id == FBOID.FORWARD_SOLID || id == FBOID.REFRACT || id == FBOID.MAIN;
        }

        /// <summary>
        /// Checks if the ID is the 'forward' modes.
        /// </summary>
        /// <param name="id">The ID.</param>
        /// <returns>Whether it is.</returns>
        public static bool IsForward(this FBOID id)
        {
            return id == FBOID.FORWARD_SOLID || id == FBOID.FORWARD_TRANSP || id == FBOID.FORWARD_EXTRAS;
        }
    }
}
