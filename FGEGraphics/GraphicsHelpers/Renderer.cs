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
using FGEGraphics.ClientSystem.ViewRenderSystem;
using FGEGraphics.GraphicsHelpers.Models;
using FGEGraphics.GraphicsHelpers.Shaders;
using FGEGraphics.GraphicsHelpers.Textures;
using OpenTK;
using OpenTK.Compute.OpenCL;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace FGEGraphics.GraphicsHelpers;

/// <summary>
/// Rendering utility.
/// Construct and call <see cref="Init"/>.
/// </summary>
/// <param name="_textures">The relevant texture engine.</param>
/// <param name="_shaders">The relevant shader engine.</param>
/// <param name="_models">The relevant model engine.</param>
public class Renderer(TextureEngine _textures, ShaderEngine _shaders, ModelEngine _models)
{
    /// <summary>Prepare the renderer.</summary>
    public void Init()
    {
        GenerateSquareVBO();
        GenerateLineVBO();
        GenerateBoxVBO();
        GenerateCircleVBO();
    }

    /// <summary>A 2D square from (0,0,0) to (1,1,0) with normals all equal to (0,0,1).</summary>
    public Renderable Square;

    /// <summary>A 2D line from (0,0,0) to (1,0,0).</summary>
    public Renderable Line;

    /// <summary>A 3D box from (-1,-1,-1) to (1,1,1), ie size is (2,2,2) but the box is centered at (0,0,0).</summary>
    public Renderable Box;

    /// <summary>A 2D circle <summary>
    public Renderable Circle;

    /// <summary>Texture engine.</summary>
    public TextureEngine Textures = _textures;

    /// <summary>Shader engine.</summary>
    public ShaderEngine Shaders = _shaders;

    /// <summary>Model engine.</summary>
    public ModelEngine Models = _models;

    /// <summary>Generates a square.</summary>
    void GenerateSquareVBO()
    {
        Renderable.ArrayBuilder builder = new();
        builder.Prepare(6, 6);
        builder.Tangents = new Vector3[6];
        for (uint n = 0; n < 6; n++)
        {
            builder.Indices[n] = n;
            builder.Tangents[n] = new Vector3(1, 0, 0);
            builder.Normals[n] = new Vector3(0, 0, 1);
            builder.Colors[n] = new Vector4(1, 1, 1, 1);
            builder.ZeroBoneInfo(n);
        }
        builder.Vertices[0] = new Vector3(1, 0, 0);
        builder.TexCoords[0] = new Vector3(1, 0, 0);
        builder.Vertices[1] = new Vector3(1, 1, 0);
        builder.TexCoords[1] = new Vector3(1, 1, 0);
        builder.Vertices[2] = new Vector3(0, 1, 0);
        builder.TexCoords[2] = new Vector3(0, 1, 0);
        builder.Vertices[3] = new Vector3(1, 0, 0);
        builder.TexCoords[3] = new Vector3(1, 0, 0);
        builder.Vertices[4] = new Vector3(0, 1, 0);
        builder.TexCoords[4] = new Vector3(0, 1, 0);
        builder.Vertices[5] = new Vector3(0, 0, 0);
        builder.TexCoords[5] = new Vector3(0, 0, 0);
        Square = builder.Generate();
    }

    /// <summary>Generates a line.</summary>
    void GenerateLineVBO()
    {
        Renderable.ArrayBuilder builder = new();
        builder.Prepare(2, 2);
        builder.Tangents = new Vector3[2];
        for (uint n = 0; n < 2; n++)
        {
            builder.Indices[n] = n;
            builder.Tangents[n] = new Vector3(1f, 0f, 0f);
            builder.Normals[n] = new Vector3(0, 0, 1);
            builder.Colors[n] = new Vector4(1, 1, 1, 1);
            builder.ZeroBoneInfo(n);
        }
        builder.Vertices[0] = new Vector3(0, 0, 0);
        builder.TexCoords[0] = new Vector3(0, 0, 0);
        builder.Vertices[1] = new Vector3(1, 0, 0);
        builder.TexCoords[1] = new Vector3(1, 0, 0);
        Line = builder.Generate();
    }

    /// <summary>Generates a box.</summary>
    void GenerateBoxVBO()
    {
        Renderable.ArrayBuilder builder = new();
        builder.Prepare(24, 24);
        builder.Tangents = new Vector3[24];
        // TODO: Optimize?
        for (uint n = 0; n < 24; n++)
        {
            builder.Indices[n] = n;
            builder.Tangents[n] = new Vector3(1f, 0f, 0f);
            builder.TexCoords[n] = new Vector3(0, 0, 0);
            builder.Normals[n] = new Vector3(0, 0, 1); // TODO: Accurate normals somehow? Do lines even have normals?
            builder.Colors[n] = new Vector4(1, 1, 1, 1);
            builder.ZeroBoneInfo(n);
        }
        int i = 0;
        const int lowValue = -1;
        builder.Vertices[i] = new Vector3(lowValue, lowValue, lowValue); i++;
        builder.Vertices[i] = new Vector3(1, lowValue, lowValue); i++;
        builder.Vertices[i] = new Vector3(1, lowValue, lowValue); i++;
        builder.Vertices[i] = new Vector3(1, 1, lowValue); i++;
        builder.Vertices[i] = new Vector3(1, 1, lowValue); i++;
        builder.Vertices[i] = new Vector3(lowValue, 1, lowValue); i++;
        builder.Vertices[i] = new Vector3(lowValue, 1, lowValue); i++;
        builder.Vertices[i] = new Vector3(lowValue, lowValue, lowValue); i++;
        builder.Vertices[i] = new Vector3(lowValue, lowValue, 1); i++;
        builder.Vertices[i] = new Vector3(1, lowValue, 1); i++;
        builder.Vertices[i] = new Vector3(1, lowValue, 1); i++;
        builder.Vertices[i] = new Vector3(1, 1, 1); i++;
        builder.Vertices[i] = new Vector3(1, 1, 1); i++;
        builder.Vertices[i] = new Vector3(lowValue, 1, 1); i++;
        builder.Vertices[i] = new Vector3(lowValue, 1, 1); i++;
        builder.Vertices[i] = new Vector3(lowValue, lowValue, 1); i++;
        builder.Vertices[i] = new Vector3(lowValue, lowValue, lowValue); i++;
        builder.Vertices[i] = new Vector3(lowValue, lowValue, 1); i++;
        builder.Vertices[i] = new Vector3(1, lowValue, lowValue); i++;
        builder.Vertices[i] = new Vector3(1, lowValue, 1); i++;
        builder.Vertices[i] = new Vector3(1, 1, lowValue); i++;
        builder.Vertices[i] = new Vector3(1, 1, 1); i++;
        builder.Vertices[i] = new Vector3(lowValue, 1, lowValue); i++;
        builder.Vertices[i] = new Vector3(lowValue, 1, 1);
        Box = builder.Generate();
    }


    void GenerateCircleVBO()
    {
        const int segments = 32;
        const int vertexCount = segments + 2;
        const int indexCount = segments + 2;

        Renderable.ArrayBuilder builder = new();
        builder.Prepare2D(vertexCount, indexCount);
        builder.Tangents = new Vector3[vertexCount];
        builder.Vertices[0] = new Vector3(0, 0, 0);
        builder.TexCoords[0] = new Vector3(0.5f, 0.5f, 0);
        builder.Normals[0] = new Vector3(0, 0, -1);
        builder.Colors[0] = new Vector4(1, 1, 1, 1);
        builder.Tangents[0] = new Vector3(1, 0, 0);
        builder.Indices[0] = 0;
        // Generate vertices around circle
        for (int i = 0; i <= segments; i++)
        {
            float angle = (float)(i * 2.0 * Math.PI / segments);
            float x = (float)Math.Cos(angle);
            float y = (float)Math.Sin(angle);

            int vertIndex = i + 1;
            builder.Vertices[vertIndex] = new Vector3(x, y, 0);
            builder.TexCoords[vertIndex] = new Vector3((x + 1) * 0.5f, (y + 1) * 0.5f, 0);
            builder.Normals[vertIndex] = new Vector3(0, 0, -1);
            builder.Colors[vertIndex] = new Vector4(1, 1, 1, 1);
            builder.Tangents[vertIndex] = new Vector3(1, 0, 0);
            builder.Indices[vertIndex] = (uint)vertIndex;
        }
        Circle = builder.Generate();
    }

    /// <summary>Renders a line box.</summary>
    /// <param name="min">The minimum coordinate.</param>
    /// <param name="max">The maximmum coordinate.</param>
    /// <param name="view">The relevant view.</param>
    /// <param name="rot">Any rotation.</param>
    public void RenderLineBox(Location min, Location max, View3D view, Matrix4d? rot = null)
    {
        GraphicsUtil.CheckError("RenderLineBox: Pre");
        if (min.IsNaN() || min.IsInfinite() || max.IsNaN() || max.IsInfinite())
        {
            Logs.Warning($"Invalid line box from {min} to {max}");
            Logs.Debug(Environment.StackTrace);
            return;
        }
        GL.ActiveTexture(TextureUnit.Texture0);
        Textures.White.Bind();
        GraphicsUtil.CheckError("RenderLineBox: BindTexture");
        Location halfsize = (max - min) * 0.5;
        Matrix4d mat = Matrix4d.Scale(halfsize.ToOpenTK3D())
            * (rot != null && rot.HasValue ? rot.Value : Matrix4d.Identity)
            * Matrix4d.CreateTranslation((min + halfsize).ToOpenTK3D());
        view.SetMatrix(2, mat); // TODO: Client reference!
        GraphicsUtil.CheckError("RenderLineBox: SetMatrix");
        GL.BindVertexArray(Box.Internal.VAO);
        GraphicsUtil.CheckError("RenderLineBox: Bind VAO");
        GL.DrawElements(PrimitiveType.Lines, 24, DrawElementsType.UnsignedInt, IntPtr.Zero);
        GraphicsUtil.CheckError("RenderLineBox: Pass");
    }

    /// <summary>Render a line between two points.</summary>
    /// <param name="start">The initial point.</param>
    /// <param name="end">The ending point.</param>
    /// <param name="view">The relevant view.</param>
    public void RenderLine(Location start, Location end, View3D view)
    {
        double len = start.Distance(end);
        Location vecang = MathUtilities.VectorToAngles(start - end);
        vecang.Yaw += 180; // TODO: Why are we getting a backwards vector then flipping the yaw? Just get the vector `end - start`? (Is pitch inverted when you do that? If so, why?)
        Matrix4d mat = Matrix4d.Scale(len, 1, 1)
            * Matrix4d.CreateRotationY(vecang.Pitch * MathUtilities.PI180)
            * Matrix4d.CreateRotationZ(vecang.Yaw * MathUtilities.PI180)
            * Matrix4d.CreateTranslation(start.ToOpenTK3D());
        view.SetMatrix(2, mat);
        GL.BindVertexArray(Line.Internal.VAO);
        GL.DrawElements(PrimitiveType.Lines, 2, DrawElementsType.UnsignedInt, IntPtr.Zero);
    }

    /// <summary>Render a cylinder between two points.</summary>
    /// <param name="context">The sourcing render context.</param>
    /// <param name="start">The initial point.</param>
    /// <param name="end">The ending point.</param>
    /// <param name="width">The width of the cylinder.</param>
    /// <param name="view">The relevant view.</param>
    public void RenderCylinder(RenderContext context, Location start, Location end, float width, View3D view)
    {
        double len = start.Distance(end);
        Location vecang = MathUtilities.VectorToAngles(start - end);
        vecang.Yaw += 180;
        Matrix4d mat = Matrix4d.CreateRotationY(90 * MathUtilities.PI180)
            * Matrix4d.Scale(len, width, width)
            * Matrix4d.CreateRotationY(vecang.Pitch * MathUtilities.PI180)
            * Matrix4d.CreateRotationZ(vecang.Yaw * MathUtilities.PI180)
             * Matrix4d.CreateTranslation(start.ToOpenTK3D());
        view.SetMatrix(2, mat);
        Models.Cylinder.Draw(context);
    }

    /// <summary>Set the color of rendered objects.</summary>
    /// <param name="col">The color.</param>
    /// <param name="view">The relevant view.</param>
    public void SetColor(Vector4 col, View3D view)
    {
        if (!view.State.RenderingShadows)
        {
            GL.Uniform4(3, ref col);
        }
    }

    /// <summary>Set the color of rendered objects.</summary>
    /// <param name="c">The color.</param>
    /// <param name="view">The relevant view.</param>
    public void SetColor(Color4 c, View3D view)
    {
        SetColor(new Vector4(c.R, c.G, c.B, c.A), view);
    }

    /// <summary>Set the color of rendered objects.</summary>
    /// <param name="c">The color.</param>
    /// <param name="view">The relevant view.</param>
    public void SetColor(Color3F c, View3D view)
    {
        SetColor(new Vector4(c.R, c.G, c.B, 1), view);
    }

    /// <summary>Set the color of rendered objects.</summary>
    /// <param name="c">The color.</param>
    /// <param name="view">The relevant view.</param>
    public void SetColor(Color4F c, View3D view)
    {
        SetColor(new Vector4(c.R, c.G, c.B, c.A), view);
    }

    /// <summary>Set the minimum light to 0.0, indicating that fog is disabled but minimum light is also disabled.</summary>
    /// <param name="view">Relevant view.</param>
    public void SetSpecialFoglessLight(View3D view)
    {
        if (view.State.RenderLights && !view.State.RenderingShadows)
        {
            GL.Uniform1(16, 0.0f);
        }
    }

    /// <summary>Set the minimum light.</summary>
    /// <param name="min">Minimum light.</param>
    /// <param name="view">Relevant view.</param>
    public void SetMinimumLight(float min, View3D view)
    {
        if (view.State.RenderLights && !view.State.RenderingShadows)
        {
            GL.Uniform1(16, Math.Max(min, 0.01f));
        }
    }

    /// <summary>Enables shine effects.</summary>
    /// <param name="view">The relevant view.</param>
    /// <param name="defaultColor">Whether to default the color.</param>
    public void EnableShine(View3D view, bool defaultColor = true)
    {
        if (defaultColor)
        {
            SetColor(new Vector4(1.0f, 0.0f, 0.0f, 1.0f), view);
        }
        GL.Disable(EnableCap.DepthTest);
        GL.DepthMask(false);
        GL.Uniform1(8, 1.0f);
    }

    /// <summary>Disables shine effects.</summary>
    /// <param name="view">The relevant view.</param>
    public void DisableShine(View3D view)
    {
        SetColor(Vector4.One, view);
        GL.Enable(EnableCap.DepthTest);
        GL.DepthMask(true);
        GL.Uniform1(8, 0.0f);
    }

    /// <summary>Renders a 2D texture fitted to a given rectangle.</summary>
    /// <param name="texture">The texture to render. Will be automatically bound, and used to source the fitting reference size.</param>
    /// <param name="xmin">The lower bounds of the the rectangle: X coordinate.</param>
    /// <param name="ymin">The lower bounds of the the rectangle: Y coordinate.</param>
    /// <param name="xmax">The upper bounds of the the rectangle: X coordinate.</param>
    /// <param name="ymax">The upper bounds of the the rectangle: Y coordinate.</param>
    /// <param name="fit">How to adjust the rectangle to fit the texture's aspect ratio.</param>
    /// <param name="rot">The rotation matrix, if any.</param>
    public void RenderFittedTextureRectangle(Texture texture, float xmin, float ymin, float xmax, float ymax, TextureFit fit, Matrix4? rot = null)
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
        RenderRectangle(xmin, ymin, xmax, ymax, rot);
    }

    /// <summary>Renders a 3D rectangle.</summary>
    /// <param name="mat">The matrix.</param>
    public void RenderRectangle3D(Matrix4 mat)
    {
        GL.UniformMatrix4(2, false, ref mat);
        GL.BindVertexArray(Square.Internal.VAO);
        GL.DrawElements(PrimitiveType.Triangles, 6, DrawElementsType.UnsignedInt, IntPtr.Zero);
        GL.BindVertexArray(0);
    }



    /// <summary>
    /// Renders a Circle based on Location (Vectors Normalized Down)
    /// </summary>
    /// <param name="center">The Location for the circle to be rendered</param>
    /// <param name="radius">Radius of the circle</param>
    /// <param name="view">View to render Circle in</param>
    public void RenderGroundCircle(Location center, float radius, View3D view)
    {
        // Save current states prior to render
        bool cullFaceEnabled = GL.IsEnabled(EnableCap.CullFace);
        bool depthTestEnabled = GL.IsEnabled(EnableCap.DepthTest);
        bool blendEnabled = GL.IsEnabled(EnableCap.Blend);
        bool prevDepthMask = GL.GetBoolean(GetPName.DepthWritemask);

        // Question should this be outside of this function and based on user call like color?
        GL.Disable(EnableCap.CullFace);
        GL.Enable(EnableCap.DepthTest); 
        
        GL.Enable(EnableCap.Blend);
        GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
        
        

        GL.ActiveTexture(TextureUnit.Texture0);
        Textures.White.Bind();

        
       
        
        Matrix4d mat = Matrix4d.Scale(radius, radius, 1.0)
            * Matrix4d.CreateTranslation(center.ToOpenTK3D());


        GL.DepthMask(true);
        GL.DepthFunc(DepthFunction.Lequal);


        view.SetMatrix(2, mat);
        GL.BindVertexArray(Circle.Internal.VAO);
        GL.DrawElements(PrimitiveType.TriangleFan, 34, DrawElementsType.UnsignedInt, IntPtr.Zero);

        // Restore previous states 
        if (cullFaceEnabled) GL.Enable(EnableCap.CullFace); else GL.Disable(EnableCap.CullFace);
        if (depthTestEnabled) GL.Enable(EnableCap.DepthTest); else GL.Disable(EnableCap.DepthTest);
        GL.DepthMask(prevDepthMask);
        if (blendEnabled) GL.Enable(EnableCap.Blend); else GL.Disable(EnableCap.Blend);

    }


    /// <summary>Renders a 2D rectangle.</summary>
    /// <param name="xmin">The lower bounds of the the rectangle: X coordinate.</param>
    /// <param name="ymin">The lower bounds of the the rectangle: Y coordinate.</param>
    /// <param name="xmax">The upper bounds of the the rectangle: X coordinate.</param>
    /// <param name="ymax">The upper bounds of the the rectangle: Y coordinate.</param>
    /// <param name="rot">The rotation matrix, if any.</param>
    public void RenderRectangle(float xmin, float ymin, float xmax, float ymax, Matrix4? rot = null)
    {
        Matrix4 mat = Matrix4.CreateScale(xmax - xmin, ymax - ymin, 1) * (rot ?? Matrix4.Identity) * Matrix4.CreateTranslation(xmin, ymin, 0);
        GL.UniformMatrix4(2, false, ref mat);
        GL.BindVertexArray(Square.Internal.VAO);
        GL.DrawElements(PrimitiveType.Triangles, 6, DrawElementsType.UnsignedInt, IntPtr.Zero);
        GL.BindVertexArray(0);
    }

    /// <summary>Renders a 2D rectangle, with centered rotation.</summary>
    /// <param name="xmin">The lower bounds of the the rectangle: X coordinate.</param>
    /// <param name="ymin">The lower bounds of the the rectangle: Y coordinate.</param>
    /// <param name="xmax">The upper bounds of the the rectangle: X coordinate.</param>
    /// <param name="ymax">The upper bounds of the the rectangle: Y coordinate.</param>
    /// <param name="cx">The rotation offset X.</param>
    /// <param name="cy">The rotation offset Y.</param>
    /// <param name="rot">The rotation matrix.</param>
    public void RenderRectangleCentered(float xmin, float ymin, float xmax, float ymax, float cx, float cy, Matrix4 rot)
    {
        Matrix4 mat = Matrix4.CreateScale(xmax - xmin, ymax - ymin, 1) * Matrix4.CreateTranslation(-cx, -cy, 0) * rot * Matrix4.CreateTranslation(xmin + cx, ymin + cy, 0);
        GL.UniformMatrix4(2, false, ref mat);
        GL.BindVertexArray(Square.Internal.VAO);
        GL.DrawElements(PrimitiveType.Triangles, 6, DrawElementsType.UnsignedInt, IntPtr.Zero);
        GL.BindVertexArray(0);
    }

    /// <summary>Renders a flat billboard (a sprite).</summary>
    /// <param name="center">The center of it.</param>
    /// <param name="scale">The scale of it.</param>
    /// <param name="facing">Where it's facing.</param>
    /// <param name="view">The relevant view.</param>
    /// <param name="pzr">Z rotation if any.</param>
    public void RenderBillboard(Location center, Location scale, Location facing, View3D view, float pzr = 0f)
    {
        Location lookdir = (facing - center).Normalize();
        Location right = lookdir.CrossProduct(Location.UnitZ); // TODO: Camera up vector!
        Location updir = right.CrossProduct(lookdir);
        Matrix4d mat = Matrix4d.CreateTranslation(-0.5f, -0.5f, 0f) * Matrix4d.Scale((float)scale.X, (float)scale.Y, (float)scale.Z);
        Matrix4d m2 = new(right.X, updir.X, lookdir.X, center.X,
            right.Y, updir.Y, lookdir.Y, center.Y,
            right.Z, updir.Z, lookdir.Z, center.Z,
            0, 0, 0, 1);
        m2.Transpose();
        mat *= m2;
        view.SetMatrix(2, mat);
        GL.BindVertexArray(Square.Internal.VAO);
        GL.DrawElements(PrimitiveType.Triangles, 6, DrawElementsType.UnsignedInt, IntPtr.Zero);
        GL.BindVertexArray(0);
        /*
        // TODO: Quaternion magic?
        Location relang = Utilities.VectorToAngles(pos - facing);
        if (relang.IsInfinite() || relang.IsNaN())
        {
            throw new Exception("Unable to handle billboard: relang=" + relang);
        }
        Matrix4d mat =
            Matrix4d.Scale(ClientUtilities.ConvertD(scale))
            * Matrix4d.CreateTranslation(-0.5f, -0.5f, 0f)
            * Matrix4d.CreateRotationY((float)((relang.Y - 90) * Utilities.PI180))
            * Matrix4d.CreateRotationZ((float)(relang.Z * Utilities.PI180))
            * Matrix4d.CreateTranslation(ClientUtilities.ConvertD(pos + new Location(scale.X * 0.5, scale.Y * 0.5, 0.0)));
        Client.Central.MainWorldView.SetMatrix(2, mat); // TODO: Client reference!
        GL.BindVertexArray(Square._VAO);
        GL.DrawElements(PrimitiveType.Triangles, 6, DrawElementsType.UnsignedInt, IntPtr.Zero);
        GL.BindVertexArray(0);
        */
    }

    /// <summary>Renders a billboard along a line.</summary>
    /// <param name="start">Start position.</param>
    /// <param name="end">End position.</param>
    /// <param name="width">Width of the line.</param>
    /// <param name="facing">Facing target, normally the camera position.</param>
    /// <param name="view">Relevant view.</param>
    public void RenderBilboardLine(Location start, Location end, float width, Location facing, View3D view)
    {
        Location center = (start + end) * 0.5;
        double viewLength = center.Distance(facing);
        double lineLength = start.Distance(end);
        if (viewLength < 0.001 || lineLength < 0.001)
        {
            return;
        }
        Location lookDir = (center - facing) / viewLength;
        Location forwardDir = (end - start) / lineLength;
        Location right = forwardDir.CrossProduct(lookDir);
        Matrix4d mat = Matrix4d.CreateTranslation(-0.5, -0.5, 0) * Matrix4d.Scale(width, lineLength, 1);
        Matrix4d m2 = new(
            right.X, forwardDir.X, lookDir.X, center.X,
            right.Y, forwardDir.Y, lookDir.Y, center.Y,
            right.Z, forwardDir.Z, lookDir.Z, center.Z,
            0, 0, 0, 1);
        m2.Transpose();
        mat *= m2;
        view.SetMatrix(2, mat);
        GL.BindVertexArray(Square.Internal.VAO);
        GL.DrawElements(PrimitiveType.Triangles, 6, DrawElementsType.UnsignedInt, IntPtr.Zero);
        GL.BindVertexArray(0);
    }
}
