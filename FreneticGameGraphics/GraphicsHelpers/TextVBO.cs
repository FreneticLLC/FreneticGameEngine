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
using FreneticGameGraphics.ClientSystem;

namespace FreneticGameGraphics.GraphicsHelpers
{
    /// <summary>
    /// Handles Text rendering.
    /// </summary>
    public class TextVBO
    {
        /// <summary>
        /// The base Font engine.
        /// </summary>
        public GLFontEngine Engine;

        /// <summary>
        /// Constructs an empty Text VBO.
        /// </summary>
        /// <param name="fengine">The base font engine.</param>
        public TextVBO(GLFontEngine fengine)
        {
            Engine = fengine;
        }

        /// <summary>
        /// The position VBO (Vertex Buffer Object).
        /// </summary>
        uint VBO;

        /// <summary>
        /// The texture coordinate VBO (Vertex Buffer Object).
        /// </summary>
        uint VBOTexCoords;

        /// <summary>
        /// The colors VBO (Vertex Buffer Object).
        /// </summary>
        uint VBOColors;

        /// <summary>
        /// The indices VBO (Vertex Buffer Object).
        /// </summary>
        uint VBOIndices;

        /// <summary>
        /// The VAO (VertexArrayObject).
        /// </summary>
        uint VAO;

        /// <summary>
        /// All vertices on this VBO.
        /// </summary>
        public List<Vector4> Vecs = new List<Vector4>();

        /// <summary>
        /// All texture coordinate sets on this VBO.
        /// </summary>
        public List<Vector4> Texs = new List<Vector4>();

        /// <summary>
        /// All color indicators on this VBO.
        /// </summary>
        public List<Vector4> Cols = new List<Vector4>();

        /// <summary>
        /// Adds a quadrilateral (rectangle) to the VBO.
        /// </summary>
        /// <param name="minX">The minimum X.</param>
        /// <param name="minY">The minimum Y.</param>
        /// <param name="maxX">The maximum X.</param>
        /// <param name="maxY">The maximum Y.</param>
        /// <param name="tminX">The minimum texture X.</param>
        /// <param name="tminY">The minimum texture Y.</param>
        /// <param name="tmaxX">The maximum texture X.</param>
        /// <param name="tmaxY">The maximum texture Y.</param>
        /// <param name="color">The color.</param>
        public void AddQuad(float minX, float minY, float maxX, float maxY, float tminX, float tminY, float tmaxX, float tmaxY, Vector4 color)
        {
            Vecs.Add(new Vector4(minX, minY, maxX, maxY));
            Texs.Add(new Vector4(tminX, tminY, tmaxX, tmaxY));
            Cols.Add(color);
        }

        /// <summary>
        /// Destroys the internal VBO, so this can be safely deleted.
        /// </summary>
        public void Destroy()
        {
            GL.DeleteBuffer(VBO);
            GL.DeleteBuffer(VBOTexCoords);
            GL.DeleteBuffer(VBOColors);
            GL.DeleteBuffer(VBOIndices);
            GL.DeleteVertexArray(VAO);
            hasBuffers = false;
        }

        /// <summary>
        /// Builds the buffers pre-emptively.
        /// </summary>
        public void BuildBuffers()
        {
            GL.GenBuffers(1, out VBO);
            GL.GenBuffers(1, out VBOTexCoords);
            GL.GenBuffers(1, out VBOColors);
            GL.GenBuffers(1, out VBOIndices);
            GL.GenVertexArrays(1, out VAO);
            hasBuffers = true;
        }

        /// <summary>
        /// The number of indices in the VBO.
        /// </summary>
        public int Length = 0;

        /// <summary>
        /// Whether this VBO has buffers already.
        /// </summary>
        bool hasBuffers = false;

        /// <summary>
        /// A helper position array.
        /// </summary>
        public Vector4[] Positions = null;

        /// <summary>
        /// A helper texture-coordinate array.
        /// </summary>
        public Vector4[] TexCoords = null;

        /// <summary>
        /// A helper color array.
        /// </summary>
        public Vector4[] Colors = null;

        /// <summary>
        /// Turns the local VBO build information into an actual internal GPU-side VBO.
        /// </summary>
        public void Build()
        {
            if (!hasBuffers)
            {
                BuildBuffers();
            }
            if (Positions == null)
            {
                Positions = Vecs.ToArray();
                TexCoords = Texs.ToArray();
                Colors = Cols.ToArray();
            }
            Length = Positions.Length;
            uint[] Indices = new uint[Length];
            for (uint i = 0; i < Length; i++)
            {
                Indices[i] = i;
            }
            GL.BindVertexArray(0);
            // Vertex buffer
            GL.BindBuffer(BufferTarget.ArrayBuffer, VBO);
            GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(Positions.Length * Vector4.SizeInBytes), Positions, BufferUsageHint.StaticDraw);
            // TexCoord buffer
            GL.BindBuffer(BufferTarget.ArrayBuffer, VBOTexCoords);
            GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(TexCoords.Length * Vector4.SizeInBytes), TexCoords, BufferUsageHint.StaticDraw);
            // Color buffer
            GL.BindBuffer(BufferTarget.ArrayBuffer, VBOColors);
            GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(Colors.Length * Vector4.SizeInBytes), Colors, BufferUsageHint.StaticDraw);
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            // Index buffer
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, VBOIndices);
            GL.BufferData(BufferTarget.ElementArrayBuffer, (IntPtr)(Indices.Length * sizeof(uint)), Indices, BufferUsageHint.StaticDraw);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);
            // VAO
            GL.BindVertexArray(VAO);
            GL.BindBuffer(BufferTarget.ArrayBuffer, VBO);
            GL.VertexAttribPointer(0, 4, VertexAttribPointerType.Float, false, 0, 0);
            GL.BindBuffer(BufferTarget.ArrayBuffer, VBOTexCoords);
            GL.VertexAttribPointer(1, 4, VertexAttribPointerType.Float, false, 0, 0);
            GL.BindBuffer(BufferTarget.ArrayBuffer, VBOColors);
            GL.VertexAttribPointer(2, 4, VertexAttribPointerType.Float, false, 0, 0);
            GL.EnableVertexAttribArray(0);
            GL.EnableVertexAttribArray(1);
            GL.EnableVertexAttribArray(2);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, VBOIndices);
            // Clean up
            GL.BindVertexArray(0);
            Vecs.Clear();
            Texs.Clear();
            Cols.Clear();
            Positions = null;
            TexCoords = null;
            Colors = null;
        }

        /// <summary>
        /// Renders the internal VBO to screen.
        /// </summary>
        public void Render()
        {
            if (Length == 0)
            {
                return;
            }
            GL.BindTexture(TextureTarget.Texture2D, Engine.TextureMain);
            GL.BindVertexArray(VAO);
            GL.DrawElements(PrimitiveType.Points, Length, DrawElementsType.UnsignedInt, IntPtr.Zero);
            GL.BindVertexArray(0);
            GL.BindTexture(TextureTarget.Texture2D, 0);
        }
    }
}
