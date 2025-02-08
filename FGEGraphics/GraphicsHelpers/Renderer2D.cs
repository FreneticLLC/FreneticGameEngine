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
using FGECore.CoreSystems;
using FGECore.MathHelpers;
using FGEGraphics.ClientSystem;
using FGEGraphics.GraphicsHelpers.Shaders;
using FGEGraphics.GraphicsHelpers.Textures;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace FGEGraphics.GraphicsHelpers;

/// <summary>2D render helper.</summary>
/// <param name="client">Relevant game client window.</param>
public class Renderer2D(GameClientWindow client)
{
    /// <summary>Prepare the renderer.</summary>
    public void Init()
    {
        GenerateSquareVBO();
        GenerateSquareOfLinesVBO();
        GenerateLineVBO();
    }

    /// <summary>Square mesh.</summary>
    public Renderable Square;

    /// <summary>Square-of-lines mesh.</summary>
    public Renderable SquareOfLines;

    /// <summary>Line mesh.</summary>
    public Renderable Line;

    void GenerateSquareVBO()
    {
        Renderable.ArrayBuilder builder = new();
        builder.Prepare2D(4, 4);
        for (uint n = 0; n < 4; n++)
        {
            builder.Indices[n] = n;
            builder.Colors[n] = new(1, 1, 1, 1);
            builder.Normals[n] = new(0, 0, 1);
        }
        builder.Vertices[0] = new(1, 0, 0);
        builder.TexCoords[0] = new(1, 0, 0);
        builder.Vertices[1] = new(1, 1, 0);
        builder.TexCoords[1] = new(1, 1, 0);
        builder.Vertices[2] = new(0, 0, 0);
        builder.TexCoords[2] = new(0, 0, 0);
        builder.Vertices[3] = new(0, 1, 0);
        builder.TexCoords[3] = new(0, 1, 0);
        Square = builder.Generate();
    }

    void GenerateSquareOfLinesVBO()
    {
        Renderable.ArrayBuilder builder = new();
        builder.Prepare2D(5, 5);
        for (uint n = 0; n < 5; n++)
        {
            builder.Indices[n] = n;
            builder.Colors[n] = new(1, 1, 1, 1);
            builder.Normals[n] = new(0, 0, 1);
        }
        builder.Vertices[0] = new(1, 0, 0);
        builder.TexCoords[0] = new(1, 0, 0);
        builder.Vertices[1] = new(1, 1, 0);
        builder.TexCoords[1] = new(1, 1, 0);
        builder.Vertices[2] = new(0, 1, 0);
        builder.TexCoords[2] = new(0, 1, 0);
        builder.Vertices[3] = new(0, 0, 0);
        builder.TexCoords[3] = new(0, 0, 0);
        builder.Vertices[4] = new(1, 0, 0);
        builder.TexCoords[4] = new(1, 0, 0);
        SquareOfLines = builder.Generate();
    }

    void GenerateLineVBO()
    {
        Renderable.ArrayBuilder builder = new();
        builder.Prepare2D(2, 2);
        for (uint n = 0; n < 2; n++)
        {
            builder.Indices[n] = n;
            builder.Colors[n] = new(1, 1, 1, 1);
            builder.Normals[n] = new(0, 0, 1);
        }
        builder.Vertices[0] = new(0, 0, 0);
        builder.TexCoords[0] = new(0, 0, 0);
        builder.Vertices[1] = new(1, 0, 0);
        builder.TexCoords[1] = new(1, 0, 0);
        Line = builder.Generate();
    }

    /// <summary>Relevant game client.</summary>
    public GameClientWindow Client = client;

    /// <summary>Render a line between two points.</summary>
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

    /// <summary>Sets the color of the next rendered objects.</summary>
    /// <param name="c">The color.</param>
    public static void SetColor(Color4F c)
    {
        ManagedShader2D.CurrentColor.SetColor(c);
    }

    /// <summary>Sets the color of the next rendered objects.</summary>
    /// <param name="col">The color.</param>
    public static void SetColor(Vector4 col)
    {
        ManagedShader2D.CurrentColor.Set(col);
    }

    /// <summary>Sets the color of the next rendered objects.</summary>
    /// <param name="c">The color.</param>
    public static void SetColor(Color4 c)
    {
        ManagedShader2D.CurrentColor.Set(c.R, c.G, c.B, c.A);
    }

    /// <summary>Renders a 2D texture fitted to a given rectangle.</summary>
    /// <param name="rc">The render context.</param>
    /// <param name="texture">The texture to render. Will be automatically bound, and used to source the fitting reference size.</param>
    /// <param name="xmin">The lower bounds of the the rectangle: X coordinate.</param>
    /// <param name="ymin">The lower bounds of the the rectangle: Y coordinate.</param>
    /// <param name="xmax">The upper bounds of the the rectangle: X coordinate.</param>
    /// <param name="ymax">The upper bounds of the the rectangle: Y coordinate.</param>
    /// <param name="fit">How to adjust the rectangle to fit the texture's aspect ratio.</param>
    /// <param name="rot">The rotation, if any applies.</param>
    public void RenderFittedTextureRectangle(RenderContext2D rc, Texture texture, float xmin, float ymin, float xmax, float ymax, TextureFit fit, Vector3? rot = null)
    {
        texture.Bind();
        float aspect = texture.Width / (float)texture.Height;
        float xsize = xmax - xmin;
        float ysize = ymax - ymin;
        float rectAspect = xsize / ysize;
        switch (fit)
        {
            case TextureFit.STRETCH:
                break;
            case TextureFit.CONTAIN:
                if (rectAspect > aspect)
                {
                    float shift = (xsize - (ysize * aspect)) * 0.5f;
                    xmin += shift;
                    xmax -= shift;
                }
                else
                {
                    float shift = (ysize - (xsize / aspect)) * 0.5f;
                    ymin += shift;
                    ymax -= shift;
                }
                break;
            case TextureFit.OVEREXTEND:
                if (rectAspect > aspect)
                {
                    float shift = (xsize - (ysize * aspect)) * 0.5f;
                    ymin -= shift;
                    ymax += shift;
                }
                else
                {
                    float shift = (ysize - (xsize / aspect)) * 0.5f;
                    xmin -= shift;
                    xmax += shift;
                }
                break;
            default:
                throw new InvalidOperationException($"Unrecognized {nameof(TextureFit)} value: {fit}");
        }
        RenderRectangle(rc, xmin, ymin, xmax, ymax, rot);
    }

    /// <summary>Renders a 2D rectangle.</summary>
    /// <param name="rc">The render context.</param>
    /// <param name="xmin">The lower bounds of the the rectangle: X coordinate.</param>
    /// <param name="ymin">The lower bounds of the the rectangle: Y coordinate.</param>
    /// <param name="xmax">The upper bounds of the the rectangle: X coordinate.</param>
    /// <param name="ymax">The upper bounds of the the rectangle: Y coordinate.</param>
    /// <param name="hollow">Whether to render the edges of the rectangle only.</param>
    /// <param name="rot">The rotation, if any applies.</param>
    public void RenderRectangle(RenderContext2D rc, float xmin, float ymin, float xmax, float ymax, Vector3? rot = null, bool hollow = false)
    {
        GraphicsUtil.CheckError($"Renderer2D - RenderRectangle - Pre");
        Vector2 scaler = new(xmax - xmin, ymax - ymin);
        Vector2 adder = new(xmin, ymin);
        Vector2 tscaler = rc.Scaler * scaler;
        ManagedShader2D.CurrentScaler.Set(tscaler.X, tscaler.Y, rc.AspectHelper);
        ManagedShader2D.CurrentAdder.Set((rc.Adder + adder) * rc.Scaler);
        if (rot.HasValue)
        {
            ManagedShader2D.CurrentRotation.Set(rot.Value);
        }
        GraphicsUtil.CheckError($"Renderer2D - RenderRectangle - Setup");
        if (hollow || (rc.CalcShadows && rc.Engine.OneDLights))
        {
            GL.BindVertexArray(SquareOfLines.Internal.VAO);
            GL.DrawElements(PrimitiveType.LineStrip, 5, DrawElementsType.UnsignedInt, IntPtr.Zero);
        }
        else
        {
            GL.BindVertexArray(Square.Internal.VAO);
            GL.DrawElements(PrimitiveType.TriangleStrip, 4, DrawElementsType.UnsignedInt, IntPtr.Zero);
        }
        if (rot is not null)
        {
            ManagedShader2D.CurrentRotation.Set(Vector3.Zero);
        }
        GraphicsUtil.CheckError($"Renderer2D - RenderRectangle - Post");
    }

    /// <summary>Scissors the rendering view, ie adds a bounding box restriction to what's visible.</summary>
    public void Scissor(RenderContext2D rc, int xmin, int ymin, int xmax, int ymax)
    {
        if (xmax < xmin || ymax < ymin)
        {
            throw new InvalidOperationException($"Tried to scissor with invalid size, {xmax - xmin}, {ymax - ymin}");
        }
        GL.Scissor(xmin, rc.Height - ymax, xmax - xmin, ymax - ymin);
    }

    /// <summary>Pushes a scissor onto the stack, ie a stackable version of <see cref="Scissor(RenderContext2D, int, int, int, int)"/>.
    /// After rendering, call <see cref="PopScissor(RenderContext2D)"/>.</summary>
    public void PushScissor(RenderContext2D rc, int xmin, int ymin, int xmax, int ymax)
    {
        GraphicsUtil.CheckError("Renderer2D - PushScissor - Pre");
        if (rc.ScissorStack.Count == 0)
        {
            GL.Enable(EnableCap.ScissorTest);
        }
        if (rc.ScissorStack.Count > 0)
        {
            (int prevXmin, int prevYmin, int prevXmax, int prevYmax) = rc.ScissorStack.Peek();
            xmin = Math.Max(xmin, prevXmin);
            ymin = Math.Max(ymin, prevYmin);
            xmax = Math.Min(xmax, prevXmax);
            ymax = Math.Min(ymax, prevYmax);
            xmax = Math.Max(xmin, xmax);
            ymax = Math.Max(ymin, ymax);
        }
        Scissor(rc, xmin, ymin, xmax, ymax);
        rc.ScissorStack.Push((xmin, ymin, xmax, ymax));
        GraphicsUtil.CheckError("Renderer2D - PushScissor - Post", rc.ScissorStack.Peek());
    }

    /// <summary>Pops a scissor from the stack, ie a stackable version of <see cref="Scissor(RenderContext2D, int, int, int, int)"/>.
    /// Prior to this, call <see cref="PushScissor(RenderContext2D, int, int, int, int)"/>.</summary>
    public void PopScissor(RenderContext2D rc)
    {
        GraphicsUtil.CheckError("Renderer2D - PopScissor - Pre");
        if (rc.ScissorStack.Count == 0)
        {
            throw new Exception("Scissor stack empty");
        }
        rc.ScissorStack.Pop();
        if (rc.ScissorStack.Count > 0)
        {
            (int xmin, int ymin, int xmax, int ymax) = rc.ScissorStack.Peek();
            Scissor(rc, xmin, ymin, xmax, ymax);
        }
        else
        {
            GL.Scissor(0, 0, rc.Width, rc.Height);
            GL.Disable(EnableCap.ScissorTest);
        }
        GraphicsUtil.CheckError("Renderer2D - PopScissor - Post");
    }
}
