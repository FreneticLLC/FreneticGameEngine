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
using FreneticUtilities.FreneticToolkit;
using FGECore.MathHelpers;
using OpenTK;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace FGEGraphics.GraphicsHelpers.FontSets;

/// <summary>Handles Text rendering.</summary>
public struct TextVBOBuilder
{
    /// <summary>The position VBO (Vertex Buffer Object).</summary>
    public uint VBO;

    /// <summary>The texture coordinate VBO (Vertex Buffer Object).</summary>
    public uint VBOTexCoords;

    /// <summary>The colors VBO (Vertex Buffer Object).</summary>
    public uint VBOColors;

    /// <summary>The indices VBO (Vertex Buffer Object).</summary>
    public uint VBOIndices;

    /// <summary>The VAO (VertexArrayObject).</summary>
    public uint VAO;

    /// <summary>An array of vertices, that is reused across all <see cref="TextVBOBuilder"/> instances.</summary>
    public static ResizableArray<Vector4> ReusableVertexArray = [];

    /// <summary>An array of texture coordinates, that is reused across all <see cref="TextVBOBuilder"/> instances.</summary>
    public static ResizableArray<Vector4> ReusableTextureCoordinateArray = [];

    /// <summary>An array of color values, that is reused across all <see cref="TextVBOBuilder"/> instances.</summary>
    public static ResizableArray<Vector4> ReusableColorArray = [];

    /// <summary>An array of index values, that is reused across all <see cref="TextVBOBuilder"/> instances.</summary>
    public static ResizableArray<uint> ReusableIndexArray = [];

    /// <summary>Adds a quadrilateral (rectangle) to the VBO.</summary>
    /// <param name="minX">The minimum X.</param>
    /// <param name="minY">The minimum Y.</param>
    /// <param name="maxX">The maximum X.</param>
    /// <param name="maxY">The maximum Y.</param>
    /// <param name="tminX">The minimum texture X.</param>
    /// <param name="tminY">The minimum texture Y.</param>
    /// <param name="tmaxX">The maximum texture X.</param>
    /// <param name="tmaxY">The maximum texture Y.</param>
    /// <param name="color">The color.</param>
    public static void AddQuad(float minX, float minY, float maxX, float maxY, float tminX, float tminY, float tmaxX, float tmaxY, Color4F color)
    {
        ReusableVertexArray.Add(new Vector4(minX, minY, maxX, maxY));
        ReusableTextureCoordinateArray.Add(new Vector4(tminX, tminY, tmaxX, tmaxY));
        ReusableColorArray.Add(color.ToOpenTK());
    }

    /// <summary>Destroys the internal VBO, so this can be safely deleted.</summary>
    public void Destroy()
    {
        GraphicsUtil.DeleteBuffer(VBO);
        GraphicsUtil.DeleteBuffer(VBOTexCoords);
        GraphicsUtil.DeleteBuffer(VBOColors);
        GraphicsUtil.DeleteBuffer(VBOIndices);
        GraphicsUtil.DeleteVertexArray(VAO);
        hasBuffers = false;
    }

    /// <summary>Builds the buffers pre-emptively.</summary>
    public void BuildBuffers()
    {
        VBO = GraphicsUtil.GenBuffer("TextVBO_VBO", BufferTarget.ArrayBuffer);
        VBOTexCoords = GraphicsUtil.GenBuffer("TextVBO_VBOTexCoords", BufferTarget.ArrayBuffer);
        VBOColors = GraphicsUtil.GenBuffer("TextVBO_VBOColors", BufferTarget.ArrayBuffer);
        VBOIndices = GraphicsUtil.GenBuffer("TextVBO_VBOIndices", BufferTarget.ArrayBuffer);
        GL.BindBuffer(BufferTarget.ArrayBuffer, 0); // Unbind
        VAO = GraphicsUtil.GenVertexArray("TextVBO_VAO");
        GL.BindVertexArray(0); // Unbind
        hasBuffers = true;
    }

    /// <summary>The number of indices in the VBO.</summary>
    public int Length;

    /// <summary>Whether this VBO has buffers already.</summary>
    bool hasBuffers;

    /// <summary>Turns the local VBO build information into an actual internal GPU-side VBO.</summary>
    public void Build()
    {
        if (!hasBuffers)
        {
            BuildBuffers();
        }
        Length = ReusableVertexArray.Length;
        ReusableIndexArray.EnsureCapacity(Length);
        for (uint i = 0; i < Length; i++)
        {
            ReusableIndexArray.Add(i);
        }
        GL.BindVertexArray(0);
        // Vertex buffer
        GL.BindBuffer(BufferTarget.ArrayBuffer, VBO);
        GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(ReusableVertexArray.Length * Vector4.SizeInBytes), ReusableVertexArray.Internal, BufferUsageHint.StaticDraw);
        // TexCoord buffer
        GL.BindBuffer(BufferTarget.ArrayBuffer, VBOTexCoords);
        GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(ReusableTextureCoordinateArray.Length * Vector4.SizeInBytes), ReusableTextureCoordinateArray.Internal, BufferUsageHint.StaticDraw);
        // Color buffer
        GL.BindBuffer(BufferTarget.ArrayBuffer, VBOColors);
        GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(ReusableColorArray.Length * Vector4.SizeInBytes), ReusableColorArray.Internal, BufferUsageHint.StaticDraw);
        GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
        // Index buffer
        GL.BindBuffer(BufferTarget.ElementArrayBuffer, VBOIndices);
        GL.BufferData(BufferTarget.ElementArrayBuffer, (IntPtr)(ReusableIndexArray.Length * sizeof(uint)), ReusableIndexArray.Internal, BufferUsageHint.StaticDraw);
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
        ReusableVertexArray.Clear();
        ReusableTextureCoordinateArray.Clear();
        ReusableColorArray.Clear();
        ReusableIndexArray.Clear();
    }

    /// <summary>Renders the internal VBO to screen.</summary>
    public readonly void Render(GLFontEngine engine)
    {
        if (Length == 0)
        {
            return;
        }
        engine.TextureMain.Bind();
        GL.BindVertexArray(VAO);
        GL.DrawElements(PrimitiveType.Points, Length, DrawElementsType.UnsignedInt, IntPtr.Zero);
        GL.BindVertexArray(0);
        GraphicsUtil.BindTexture(TextureTarget.Texture2D, 0);
    }
}
