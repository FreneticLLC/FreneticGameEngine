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
using System.Threading;
using System.Threading.Tasks;
using FGECore;
using FGECore.CoreSystems;
using FGECore.FileSystems;
using FGECore.MathHelpers;
using FGECore.StackNoteSystem;
using FGEGraphics.ClientSystem.EntitySystem;
using FGEGraphics.GraphicsHelpers;
using FGEGraphics.GraphicsHelpers.Shaders;
using FGEGraphics.LightingSystem;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Input;
using OpenTK.Mathematics;

namespace FGEGraphics.ClientSystem
{
    /// <summary>
    /// Represents a 2D game's coremost engine.
    /// Handles things like window management.
    /// TODO: Split the rendering system into a View2D, separate from the main game engine?
    /// </summary>
    public class GameEngine2D : GameEngineBase
    {
        /// <summary>
        /// The window can never render coordinates past this point.
        /// This constant set for precision-safety reasons.
        /// </summary>
        public const float MAX_COORD = 1000000.0f;

        /// <summary>Helps with rendering.</summary>
        public Renderer2D RenderHelper;

        /// <summary>
        /// Renders all objects. The boolean indicates whether to render objects that don't affect lighting (Meaning, things that don't cast shadows).
        /// Used when entity rendering is insufficient.
        /// Fires BEFORE rendering path.
        /// </summary>
        public Action<bool> RenderAllObjectsPre;

        /// <summary>Fires AFTER standard render path. See <see cref="RenderAllObjectsPre"/>.</summary>
        public Action<bool> RenderAllObjectsPost;

        /// <summary>First shader stage: computes a 'light map' for each light source.</summary>
        public Shader Shader_Lightmap;

        /// <summary>Second shader stage: combines various light map inputs into a buffer.</summary>
        public Shader Shader_Combine;

        /// <summary>Final shader stage: adds light buffer to the scene.</summary>
        public Shader Shader_Addlighttoscene;

        /// <summary>First shader stage: computes a 'light map' for each light source.</summary>
        public Shader Shader_Lightmap1D;

        /// <summary>Final shader stage: applies a 2D/1D light to the scene.</summary>
        public Shader Shader_ApplyLights1D;

        /// <summary>Whether to use the 2D lighting engine.</summary>
        public bool UseLightEngine = true;

        /// <summary>Whether to use the 1D lighting trick when lighting is enabled.</summary>
        public bool OneDLights = true;

        /// <summary>Current zoom. Smaller numbers = zoomed in. Bigger numbers = zoomed out. Defaults to 1.</summary>
        public float OriginalZoom = 1f;

        /// <summary>Current zoom. Smaller numbers = zoomed in. Bigger numbers = zoomed out. Defaults to 1.</summary>
        public float Zoom = 1f;

        /// <summary>How much the zoom value is multiplied by to get pixel size.</summary>
        public float ZoomMultiplier = 1024.0f;

        /// <summary>Minimum zoom value (Most zoomed in).</summary>
        public float MinimumZoom = 0.01f;

        /// <summary>Maximum zoom value (most zoomed out).</summary>
        public float MaximumZoom = 100.0f;

        /// <summary>Center location of the view, in world coordinates.</summary>
        public Vector2 ViewCenter
        {
            get
            {
                return -ViewCenterInverse;
            }
            set
            {
                ViewCenterInverse = -value;
            }
        }

        /// <summary>Center location of the view, in negative world coordinates. Primarily for internal usage.</summary>
        public Vector2 ViewCenterInverse = Vector2.Zero;

        /// <summary>How much to pixelate the view. 1 = no pixelation.</summary>
        public int Pixelation = 1;

        /// <summary>Loads all shaders for the standard Game Engine 2D.</summary>
        public override void GetShaders()
        {
            Shader_Lightmap = Shaders.GetShader("lightmap2d");
            Shader_Lightmap1D = Shaders.GetShader("lightmap1d_2d?line_densen");
            Shader_Addlighttoscene = Shaders.GetShader("addlighttoscene2d");
            Shader_Combine = Shaders.GetShader("combine2d");
            Shader_ApplyLights1D = Shaders.GetShader("applylights1d_2d");
            GraphicsUtil.CheckError("GetShaders");
        }

        /// <summary>Loads light helpers.</summary>
        public override void PostLoad()
        {
            GraphicsUtil.CheckError("PostLoad - Pre");
            OutputType.CLIENTINIT.Output("GameEngine loading render helpers...");
            RenderHelper = new Renderer2D(Textures, Shaders);
            RenderHelper.Init();
            GraphicsUtil.CheckError("PostLoad - RenderHelper Done");
            OutputType.CLIENTINIT.Output("GameEngine loading 2D light helpers...");
            LoadLightHelpers();
            GraphicsUtil.CheckError("PostLoad - Post");
        }

        /// <summary>All current lights.</summary>
        public List<PointLight2D> Lights = new List<PointLight2D>();

        int c_FBO;

        int c_FBO_Tex;

        int l_FBO;

        int l_FBO_Tex;

        /// <summary>Calculates and loads some light helper information.</summary>
        public void LoadLightHelpers()
        {
            c_FBO = GL.GenFramebuffer();
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, c_FBO);
            c_FBO_Tex = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, c_FBO_Tex);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, Window.Size.X / Pixelation, Window.Size.Y / Pixelation, 0, PixelFormat.Rgba, PixelType.UnsignedByte, IntPtr.Zero);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (uint)TextureMinFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (uint)TextureMagFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (uint)TextureWrapMode.ClampToEdge);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (uint)TextureWrapMode.ClampToEdge);
            GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, TextureTarget.Texture2D, c_FBO_Tex, 0);
            l_FBO = GL.GenFramebuffer();
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, l_FBO);
            l_FBO_Tex = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, l_FBO_Tex);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba32f, Window.Size.X / Pixelation, Window.Size.Y / Pixelation, 0, PixelFormat.Rgba, PixelType.UnsignedByte, IntPtr.Zero);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (uint)TextureMinFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (uint)TextureMagFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (uint)TextureWrapMode.ClampToEdge);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (uint)TextureWrapMode.ClampToEdge);
            GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, TextureTarget.Texture2D, l_FBO_Tex, 0);
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
            GL.BindTexture(TextureTarget.Texture2D, 0);
            GraphicsUtil.CheckError("LoadLightHelpers");
        }

        /// <summary>Reloads the 2D engine screen buffers.</summary>
        public override void ReloadScreenBuffers()
        {
            GL.DeleteFramebuffer(c_FBO);
            GL.DeleteTexture(c_FBO_Tex);
            GL.DeleteFramebuffer(l_FBO);
            GL.DeleteTexture(l_FBO_Tex);
            LoadLightHelpers();
        }

        /// <summary>The primary render context for this game engine 2D.</summary>
        public RenderContext2D MainRenderContext = new RenderContext2D();

        /// <summary>The current view scaler.</summary>
        public Vector2 Scaler = Vector2.One;

        /// <summary>The current view adder.</summary>
        public Vector2 Adder = Vector2.Zero;

        /// <summary>The original scaler value for any given frame.</summary>
        public Vector2 OriginalScaler = Vector2.One;

        /// <summary>The original adder value for any given frame.</summary>
        public Vector2 OriginalAdder = Vector2.Zero;

        /// <summary>The current world-space mouse coordinates.</summary>
        public Vector2 MouseCoords;

        /// <summary>Renders a single frame of the 2D game engine.</summary>
        public override void RenderSingleFrame()
        {
            GraphicsUtil.CheckError("RenderSingleFrame");
            // Audio handling
            Sounds.Update(AudioCamera.Position, AudioCamera.Direction, AudioCamera.Up, Location.Zero, Window.IsFocused);
            // First step: setup
            MainRenderContext.Width = Window.Size.X / Pixelation;
            MainRenderContext.Height = Window.Size.Y / Pixelation;
            MainRenderContext.Zoom = OriginalZoom;
            MainRenderContext.ZoomMultiplier = ZoomMultiplier;
            MainRenderContext.ViewCenter = ViewCenterInverse;
            MainRenderContext.Engine = this;
            MainRenderContext.AspectHelper = MainRenderContext.Width / (float)MainRenderContext.Height;
            GlobalTickTime += Delta;
            // Second step: Prepare positioning and blank out shaders
            if (Zoom > MaximumZoom)
            {
                Zoom = MaximumZoom;
            }
            else if (Zoom < MinimumZoom)
            {
                Zoom = MinimumZoom;
            }
            OriginalZoom = Zoom;
            float aspect = Window.Size.X / (float)Window.Size.Y;
            float sc = 1.0f / (OriginalZoom * ZoomMultiplier);
            OriginalScaler = new Vector2(sc, sc * aspect);
            OriginalAdder = ViewCenterInverse;
            Client.Ortho = Matrix4.CreateOrthographicOffCenter(OriginalAdder.X - OriginalScaler.X, OriginalAdder.X + OriginalScaler.X, OriginalAdder.Y + OriginalScaler.Y, OriginalAdder.Y - OriginalScaler.Y, -1, 1);
            Scaler = OriginalScaler;
            Adder = OriginalAdder;
            float wx = Client.MouseX / (float)Window.Size.X;
            float wy = 1.0f - (Client.MouseY / (float)Window.Size.Y);
            wx -= 0.5f;
            wy -= 0.5f;
            wx *= 2f;
            wy *= 2f;
            wx /= OriginalScaler.X;
            wy /= OriginalScaler.Y;
            MouseCoords = new Vector2(wx, wy) + ViewCenter;
            if (OneDLights)
            {
                Shader_ApplyLights1D.Bind();
            }
            else
            {
                Shader_Combine.Bind();
            }
            GraphicsUtil.CheckError("RenderSingleFrame - 2");
            GL.Uniform3(ShaderLocations.Common2D.SCALER, new Vector3(Scaler.X, Scaler.Y, MainRenderContext.AspectHelper));
            GL.Uniform2(2, ref Adder);
            GL.Uniform1(7, aspect);
            GraphicsUtil.CheckError("RenderSingleFrame - 2.5");
            Shader_Lightmap1D.Bind();
            Shaders.ColorMult2DShader.Bind();
            GraphicsUtil.CheckError("RenderSingleFrame - 3");
            GL.Uniform3(ShaderLocations.Common2D.SCALER, new Vector3(Scaler.X, Scaler.Y, MainRenderContext.AspectHelper));
            GL.Uniform2(2, ref Adder);
            Renderer2D.SetColor(Vector4.One);
            // Third step: Pass to the primary rendering system
            try
            {
                GraphicsUtil.CheckError("Pre-Render");
                Render();
                GraphicsUtil.CheckError("Post-Render");
            }
            catch (Exception ex)
            {
                if (ex is ThreadAbortException)
                {
                    throw;
                }
                Console.WriteLine("Rendering exception: " + ex);
            }
        }

        /// <summary>Gets the 2D renderer.</summary>
        public Renderer2D Rendering
        {
            get
            {
                return Client.Rendering2D;
            }
        }

        /// <summary>Renders all entities and render helpers.</summary>
        /// <param name="lights">Whether to include things that don't cast shadows.</param>
        /// <param name="shouldShadow">The method to determine if an object should cast a shadow.</param>
        private void RenderAll(bool lights, Func<ClientEntity, bool> shouldShadow)
        {
            try
            {
                StackNoteHelper.Push("GameEngine2D - RenderAll", this);
                Textures.White.Bind();
                Renderer2D.SetColor(Vector4.One);
                RenderAllObjectsPre?.Invoke(lights);
                // This dups the list inherently, preventing glitches from removal while rendering, helpfully!
                foreach (ClientEntity ent in Entities.Values
                    .Where((e) => ShouldRender(e.Renderer, lights) && (shouldShadow == null || shouldShadow(e)))
                    .OrderBy((e) => e.Renderer.RenderingPriorityOrder))
                {
                    try
                    {
                        StackNoteHelper.Push("GameEngine2D - Render Specific Entity", ent);
                        ent.Renderer.RenderStandard2D(MainRenderContext);
                    }
                    finally
                    {
                        StackNoteHelper.Pop();
                    }
                }
                RenderAllObjectsPost?.Invoke(lights);
                GraphicsUtil.CheckError("Render - all Entities rendered");
            }
            finally
            {
                StackNoteHelper.Pop();
            }
        }

        /// <summary>Renders the entire GameEngine2D.</summary>
        private void Render()
        {
            GraphicsUtil.CheckError("Render - Pre");
            if (!UseLightEngine)
            {
                GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
                GL.Disable(EnableCap.DepthTest);
                GL.DepthMask(false);
                GL.Viewport(0, 0, Window.Size.X / Pixelation, Window.Size.Y / Pixelation);
                Shaders.ColorMult2DShader.Bind();
                MainRenderContext.CalcShadows = false;
                Scaler = OriginalScaler;
                Adder = OriginalAdder;
                GL.Uniform3(ShaderLocations.Common2D.SCALER, new Vector3(Scaler.X, Scaler.Y, MainRenderContext.AspectHelper));
                GL.Uniform2(2, ref Adder);
                MainRenderContext.Scaler = Scaler;
                MainRenderContext.Adder = Adder;
                RenderAll(false, null);
                return;
            }
            GraphicsUtil.CheckError("Render - Begin");
            if (Lights.Count > 0)
            {
                GL.BindFramebuffer(FramebufferTarget.Framebuffer, Lights[0].FBO);
                GL.DrawBuffer(DrawBufferMode.ColorAttachment0);
            }
            // TODO: Discard fully out-of-view lights!
            if (OneDLights)
            {
                Shader_Lightmap1D.Bind();
                GL.BlendFunc(BlendingFactor.One, BlendingFactor.Zero);
                GL.Enable(EnableCap.DepthTest);
                GL.DepthMask(true);
            }
            else
            {
                Shader_Lightmap.Bind();
            }
            GraphicsUtil.CheckError("Render - Before Light Precalcer");
            MainRenderContext.CalcShadows = true;
            for (int i = 0; i < Lights.Count; i++)
            {
                Lights[i].PrepareLightmap();
                GraphicsUtil.CheckError("Render - Light Precalcer (Prep)");
                Scaler = Lights[i].GetScaler();
                GL.Uniform3(ShaderLocations.Common2D.SCALER, new Vector3(Scaler.X, Scaler.Y, MainRenderContext.AspectHelper));
                GL.Uniform2(2, Adder = Lights[i].GetAdder());
                if (OneDLights)
                {
                    GL.ClearBuffer(ClearBuffer.Depth, 0, new float[] { 1f });
                    GL.Uniform4(21, new Vector4(ViewCenterInverse.X / OriginalScaler.X, Math.Max(OriginalScaler.X, OriginalScaler.Y), ViewCenterInverse.Y / OriginalScaler.Y + 1.0f, Lights[i].IsSkyLight ? 1.0f : 0.0f));
                }
                MainRenderContext.Scaler = Scaler;
                MainRenderContext.Adder = Adder;
                GraphicsUtil.CheckError("Render - Light Precalcer");
                RenderAll(true, Lights[i].ShouldShadow);
            }
            if (OneDLights)
            {
                GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
                GL.Disable(EnableCap.DepthTest);
                GL.DepthMask(false);
            }
            MainRenderContext.CalcShadows = false;
            GraphicsUtil.CheckError("Render - Lights precalced");
            GL.Viewport(0, 0, Window.Size.X / Pixelation, Window.Size.Y / Pixelation);
            Shaders.ColorMult2DShader.Bind();
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, c_FBO);
            GL.ClearBuffer(ClearBuffer.Color, 0, new float[] { 0, 0, 0, 1 });
            Scaler = OriginalScaler;
            Adder = OriginalAdder;
            GL.Uniform3(ShaderLocations.Common2D.SCALER, new Vector3(Scaler.X, Scaler.Y, MainRenderContext.AspectHelper));
            GL.Uniform2(2, ref Adder);
            MainRenderContext.Scaler = Scaler;
            MainRenderContext.Adder = Adder;
            GraphicsUtil.CheckError("Render - Lights prepped");
            RenderAll(false, null);
            if (OneDLights)
            {
                Shader_ApplyLights1D.Bind();
            }
            else
            {
                Shader_Combine.Bind();
            }
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, l_FBO);
            GL.ClearBuffer(ClearBuffer.Color, 0, new float[] { 0, 0, 0, 1 });
            Scaler = Vector2.One;
            Adder = Vector2.Zero;
            GL.Uniform3(ShaderLocations.Common2D.SCALER, new Vector3(Scaler.X, Scaler.Y, MainRenderContext.AspectHelper));
            GL.Uniform2(2, ref Adder);
            MainRenderContext.Scaler = Scaler;
            MainRenderContext.Adder = Adder;
            MainRenderContext.Engine = this;
            GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.One);
            GraphicsUtil.CheckError("Render - Setup Lights combine");
            for (int i = 0; i < Lights.Count; i++)
            {
                GL.Uniform2(3, Lights[i].GetSecondScaler(MainRenderContext));
                GL.Uniform2(4, Lights[i].GetSecondAdder(MainRenderContext));
                GL.Uniform4(6, new Vector4(Lights[i].Color.R, Lights[i].Color.G, Lights[i].Color.B, Lights[i].Color.A));
                if (!OneDLights)
                {
                    GL.Uniform1(5, (float)Lights[i].Width);
                    GL.Uniform1(8, Lights[i].SubDivider);
                }
                else
                {
                    GL.Uniform1(8, Lights[i].ExtraLightDist);
                    GL.Uniform4(21, new Vector4(ViewCenterInverse.X / OriginalScaler.X, Math.Max(OriginalScaler.X, OriginalScaler.Y), ViewCenterInverse.Y / OriginalScaler.Y + 1.0f, Lights[i].IsSkyLight ? 1.0f : 0.0f));
                }
                GL.BindTexture(OneDLights ? TextureTarget.Texture1D : TextureTarget.Texture2D, Lights[i].FBO_Tex);
                RenderHelper.RenderRectangle(MainRenderContext, -1, -1, 1, 1);
            }
            GraphicsUtil.CheckError("Render - Lights combined");
            GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
            GL.ActiveTexture(TextureUnit.Texture1);
            GL.BindTexture(TextureTarget.Texture2D, l_FBO_Tex);
            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, c_FBO_Tex);
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
            GL.DrawBuffer(DrawBufferMode.Back);
            Shader_Addlighttoscene.Bind();
            Scaler = Vector2.One;
            Adder = Vector2.Zero;
            GL.Uniform3(ShaderLocations.Common2D.SCALER, new Vector3(Scaler.X, Scaler.Y, MainRenderContext.AspectHelper));
            GL.Uniform2(2, ref Adder);
            MainRenderContext.Scaler = Scaler;
            MainRenderContext.Adder = Adder;
            GL.Viewport(0, 0, Window.Size.X, Window.Size.Y);
            RenderHelper.RenderRectangle(MainRenderContext, -1, -1, 1, 1);
            GraphicsUtil.CheckError("Render - Added");
            GL.BindTexture(TextureTarget.Texture2D, 0);
            GL.ActiveTexture(TextureUnit.Texture1);
            GL.BindTexture(TextureTarget.Texture2D, 0);
            GL.ActiveTexture(TextureUnit.Texture0);
            Shaders.ColorMult2DShader.Bind();
            GraphicsUtil.CheckError("Render - Complete");
        }
    }
}
