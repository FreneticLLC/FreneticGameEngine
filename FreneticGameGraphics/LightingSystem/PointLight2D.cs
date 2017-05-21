using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;
using FreneticGameGraphics.ClientSystem;
using FreneticGameGraphics.GraphicsHelpers;

namespace FreneticGameGraphics.LightingSystem
{
    /// <summary>
    /// Represents a point-light in 2D spaces.
    /// </summary>
    public class PointLight2D
    {
        /// <summary>
        /// Constructs the point light 2D.
        /// </summary>
        /// <param name="pos">Its starting position.</param>
        /// <param name="str">Its strength.</param>
        public PointLight2D(Vector2 pos, float str)
        {
            if (str < 1)
            {
                throw new Exception("Invalid strength!");
            }
            Position = pos;
            Strength = str;
            FBO = GL.GenFramebuffer();
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, FBO);
            FBO_Tex = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, FBO_Tex);
            Width = (int)(Strength * 2f);
            // TODO: Alpha texture!
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, Width, Width, 0, PixelFormat.Rgba, PixelType.UnsignedByte, IntPtr.Zero);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (uint)TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (uint)TextureMagFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (uint)TextureWrapMode.ClampToEdge);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (uint)TextureWrapMode.ClampToEdge);
            GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, TextureTarget.Texture2D, FBO_Tex, 0);
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
            GL.BindTexture(TextureTarget.Texture2D, 0);
            SubDivider = Math.Max((float)Math.Sqrt(Strength) * 0.1f, 1f);
            GraphicsUtil.CheckError("PointLight2D init");
        }
        
        /// <summary>
        /// Destroys the light object.
        /// </summary>
        public void Destroy()
        {
            GL.DeleteTexture(FBO_Tex);
            GL.DeleteFramebuffer(FBO);
        }

        /// <summary>
        /// The position of the point light 2D, in 2D world space.
        /// </summary>
        public Vector2 Position;

        /// <summary>
        /// The strength (how far it goes) of the point light 2D.
        /// </summary>
        public float Strength;

        /// <summary>
        /// The FrameBufferObject used by this Point Light 2D.
        /// </summary>
        public int FBO;

        /// <summary>
        /// The FrameBufferObject texture used by this Point Light 2D.
        /// </summary>
        public int FBO_Tex;

        /// <summary>
        /// The maximum width of this point light 2D's effects.
        /// </summary>
        public int Width;

        /// <summary>
        /// The subdivider to affect the render detail of this point light 2D.
        /// </summary>
        public float SubDivider = 4;

        /// <summary>
        /// The color of this point light 2D.
        /// </summary>
        public Color4 Color = Color4.White;
        
        /// <summary>
        /// Prepares the point light 2D for rendering.
        /// </summary>
        public void PrepareLightmap()
        {
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, FBO);
            GL.ClearBuffer(ClearBuffer.Color, 0, new float[] { 0, 0, 0, 0 });
            GL.Viewport(0, 0, Width, Width);
        }

        /// <summary>
        /// Gets the scaler value of this Point Light 2D.
        /// </summary>
        /// <returns>The scaler.</returns>
        public Vector2 GetScaler()
        {
            float sc = 1.0f / (Strength);
            return new Vector2(sc, sc);
        }

        /// <summary>
        /// Gets the adder value of this Point Light 2D.
        /// </summary>
        /// <returns>The adder.</returns>
        public Vector2 GetAdder()
        {
            float sc = 1.0f / (Strength);
            return new Vector2(-Position.X, -Position.Y);
        }

        /// <summary>
        /// Gets the secondary scaler value of this Point Light 2D.
        /// </summary>
        /// <returns>The second scaler.</returns>
        public Vector2 GetSecondScaler(RenderContext2D rc)
        {
            float sc = (rc.Zoom * rc.ZoomMultiplier);
            float sc_over_str = sc / Strength;
            return new Vector2(sc_over_str, sc_over_str);
        }

        /// <summary>
        /// Gets the secondary adder value of this Point Light 2D.
        /// </summary>
        /// <returns>The second adder.</returns>
        public Vector2 GetSecondAdder(RenderContext2D rc)
        {
            float aspect = rc.Width / (float)rc.Height;
            float one_over_str = 1f / Strength;
            float adx = (-rc.ViewCenter.X - Position.X) * one_over_str;
            float ady = (-rc.ViewCenter.Y - Position.Y) * one_over_str * aspect;
            return new Vector2(adx, ady);
        }
    }
}
