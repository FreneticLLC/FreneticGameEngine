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
using FreneticGameCore;
using FreneticGameCore.CoreSystems;
using FreneticGameCore.MathHelpers;
using FreneticGameGraphics.ClientSystem;

namespace FreneticGameGraphics.GraphicsHelpers
{
    /// <summary>
    /// 2D render helper.
    /// </summary>
    public class Renderer2D
    {
        /// <summary>
        /// Prepare the renderer.
        /// </summary>
        public void Init()
        {
            GenerateSquareVBO();
            GenerateSquareOfLinesVBO();
            GenerateLineVBO();
        }

        /// <summary>
        /// Square mesh.
        /// </summary>
        public VBO Square;

        /// <summary>
        /// Square-of-lines mesh.
        /// </summary>
        public VBO SquareOfLines;

        /// <summary>
        /// Line mesh.
        /// </summary>
        public VBO Line;

        void GenerateSquareVBO()
        {
            Vector3[] vecs = new Vector3[4];
            uint[] inds = new uint[4];
            Vector3[] texs = new Vector3[4];
            Vector4[] cols = new Vector4[4];
            Vector3[] nrms = new Vector3[4];
            for (uint u = 0; u < 4; u++)
            {
                inds[u] = u;
            }
            for (int c = 0; c < 4; c++)
            {
                cols[c] = new Vector4(1, 1, 1, 1);
                nrms[c] = new Vector3(0, 0, 1);
            }
            vecs[0] = new Vector3(1, 0, 0);
            texs[0] = new Vector3(1, 0, 0);
            vecs[1] = new Vector3(1, 1, 0);
            texs[1] = new Vector3(1, 1, 0);
            vecs[2] = new Vector3(0, 0, 0);
            texs[2] = new Vector3(0, 0, 0);
            vecs[3] = new Vector3(0, 1, 0);
            texs[3] = new Vector3(0, 1, 0);
            Square = new VBO()
            {
                Vertices = vecs.ToList(),
                Indices = inds.ToList(),
                TexCoords = texs.ToList(),
                Colors = cols.ToList(),
                Normals = nrms.ToList()
            };
            Square.GenerateVBO();
        }

        void GenerateSquareOfLinesVBO()
        {
            Vector3[] vecs = new Vector3[5];
            uint[] inds = new uint[5];
            Vector3[] texs = new Vector3[5];
            Vector4[] cols = new Vector4[5];
            Vector3[] nrms = new Vector3[5];
            for (uint u = 0; u < 5; u++)
            {
                inds[u] = u;
            }
            for (int c = 0; c < 5; c++)
            {
                cols[c] = new Vector4(1, 1, 1, 1);
                nrms[c] = new Vector3(0, 0, 1);
            }
            vecs[0] = new Vector3(1, 0, 0);
            texs[0] = new Vector3(1, 0, 0);
            vecs[1] = new Vector3(1, 1, 0);
            texs[1] = new Vector3(1, 1, 0);
            vecs[2] = new Vector3(0, 1, 0);
            texs[2] = new Vector3(0, 1, 0);
            vecs[3] = new Vector3(0, 0, 0);
            texs[3] = new Vector3(0, 0, 0);
            vecs[4] = new Vector3(1, 0, 0);
            texs[4] = new Vector3(1, 0, 0);
            SquareOfLines = new VBO()
            {
                Vertices = vecs.ToList(),
                Indices = inds.ToList(),
                TexCoords = texs.ToList(),
                Colors = cols.ToList(),
                Normals = nrms.ToList()
            };
            SquareOfLines.GenerateVBO();
        }

        void GenerateLineVBO()
        {
            Vector3[] vecs = new Vector3[2];
            uint[] inds = new uint[2];
            Vector3[] texs = new Vector3[2];
            Vector4[] cols = new Vector4[2];
            Vector3[] nrms = new Vector3[2];
            for (uint u = 0; u < 2; u++)
            {
                inds[u] = u;
            }
            for (int c = 0; c < 2; c++)
            {
                cols[c] = new Vector4(1, 1, 1, 1);
                nrms[c] = new Vector3(0, 0, 1);
            }
            Vector4[] BoneIDs = new Vector4[2];
            Vector4[] BoneWeights = new Vector4[2];
            Vector4[] BoneIDs2 = new Vector4[2];
            Vector4[] BoneWeights2 = new Vector4[2];
            for (int n = 0; n < 2; n++)
            {
                BoneIDs[n] = new Vector4(0, 0, 0, 0);
                BoneWeights[n] = new Vector4(0, 0, 0, 0);
                BoneIDs2[n] = new Vector4(0, 0, 0, 0);
                BoneWeights2[n] = new Vector4(0, 0, 0, 0);
            }
            vecs[0] = new Vector3(0, 0, 0);
            texs[0] = new Vector3(0, 0, 0);
            vecs[1] = new Vector3(1, 0, 0);
            texs[1] = new Vector3(1, 0, 0);
            Line = new VBO()
            {
                Vertices = vecs.ToList(),
                Indices = inds.ToList(),
                TexCoords = texs.ToList(),
                Colors = cols.ToList(),
                Normals = nrms.ToList()
            };
            Line.GenerateVBO();
        }

        /// <summary>
        /// Constructs the renderer - Init() it after!
        /// </summary>
        /// <param name="tengine">Texture engine.</param>
        /// <param name="shaderdet">Shader engine.</param>
        public Renderer2D(TextureEngine tengine, ShaderEngine shaderdet)
        {
            Engine = tengine;
            Shaders = shaderdet;
        }

        /// <summary>
        /// Texture system.
        /// </summary>
        public TextureEngine Engine;

        /// <summary>
        /// Shader system.
        /// </summary>
        public ShaderEngine Shaders;

        /// <summary>
        /// Render a line between two points.
        /// </summary>
        /// <param name="start">The initial point.</param>
        /// <param name="end">The ending point.</param>
        public void RenderLine(Vector2 start, Vector2 end)
        {
            throw new NotImplementedException(); // TODO: IMPL!
            /*
            // TODO: Efficiency!
            float len = (end - start).Length;
            float yawRel = (float)Math.Atan2(end.Y - start.Y, end.X - start.X);
            Matrix4 mat = Matrix4.CreateScale(len, 1, 1)
                * Matrix4.CreateRotationZ(yawRel)
                * Matrix4.CreateTranslation(start.X, start.Y, 0);
            GL.UniformMatrix4(2, false, ref mat);
            GL.BindVertexArray(Line.VAO);
            GL.DrawElements(PrimitiveType.Lines, 2, DrawElementsType.UnsignedInt, IntPtr.Zero);
            */
        }

        /// <summary>
        /// Sets the color of the next rendered objects.
        /// </summary>
        /// <param name="c">The color.</param>
        public void SetColor(Color4F c)
        {
            SetColor(new Vector4(c.R, c.G, c.B, c.A));
        }

        /// <summary>
        /// Sets the color of the next rendered objects.
        /// </summary>
        /// <param name="col">The color.</param>
        public void SetColor(Vector4 col)
        {
            GL.Uniform4(ShaderLocations.Common2D.COLOR, ref col);
        }

        /// <summary>
        /// Sets the color of the next rendered objects.
        /// </summary>
        /// <param name="c">The color.</param>
        public void SetColor(Color4 c)
        {
            SetColor(new Vector4(c.R, c.G, c.B, c.A));
        }

        /// <summary>
        /// Renders a 2D rectangle.
        /// </summary>
        /// <param name="rc">The render context.</param>
        /// <param name="xmin">The lower bounds of the the rectangle: X coordinate.</param>
        /// <param name="ymin">The lower bounds of the the rectangle: Y coordinate.</param>
        /// <param name="xmax">The upper bounds of the the rectangle: X coordinate.</param>
        /// <param name="ymax">The upper bounds of the the rectangle: Y coordinate.</param>
        /// <param name="rot">The rotation, if any applies.</param>
        public void RenderRectangle(RenderContext2D rc, float xmin, float ymin, float xmax, float ymax, Vector3? rot = null)
        {
            Vector2 scaler = new Vector2(xmax - xmin, ymax - ymin);
            Vector2 invScaler = new Vector2(1.0f / scaler.X, 1.0f / scaler.Y);
            Vector2 adder = new Vector2(xmin, ymin);
            Vector2 tscaler = rc.Scaler * scaler;
            GL.Uniform3(ShaderLocations.Common2D.SCALER, new Vector3(tscaler.X, tscaler.Y, rc.AspectHelper));
            Vector2 tadder = (rc.Adder + adder) * rc.Scaler;
            GL.Uniform2(ShaderLocations.Common2D.ADDER, tadder);
            if (rot != null)
            {
                GL.Uniform3(ShaderLocations.Common2D.ROTATION, rot.Value);
            }
            if (rc.CalcShadows && rc.Engine.OneDLights)
            {
                GL.BindVertexArray(SquareOfLines._VAO);
                GL.DrawElements(PrimitiveType.LineStrip, 5, DrawElementsType.UnsignedInt, IntPtr.Zero);
            }
            else
            {
                GL.BindVertexArray(Square._VAO);
                GL.DrawElements(PrimitiveType.TriangleStrip, 4, DrawElementsType.UnsignedInt, IntPtr.Zero);
            }
            if (rot != null)
            {
                GL.Uniform3(ShaderLocations.Common2D.ROTATION, Vector3.Zero);
            }
        }
    }
}
