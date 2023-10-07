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
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace FGEGraphics.GraphicsHelpers
{
    /// <summary>
    /// Rendering utility.
    /// Construct and call <see cref="Init"/>.
    /// </summary>
    public class Renderer
    {
        /// <summary>Prepare the renderer.</summary>
        public void Init()
        {
            GenerateSquareVBO();
            GenerateLineVBO();
            GenerateBoxVBO();
        }

        /// <summary>A square.</summary>
        public Renderable Square;

        /// <summary>A line.</summary>
        Renderable Line;
        /// <summary>A box.</summary>
        Renderable Box;

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

        /// <summary>Constructs the renderer.</summary>
        /// <param name="tengine">The relevant texture engine.</param>
        /// <param name="shaderdet">The relevant shader engine.</param>
        /// <param name="modelsdet">The relevant model engine.</param>
        public Renderer(TextureEngine tengine, ShaderEngine shaderdet, ModelEngine modelsdet)
        {
            TEngine = tengine;
            Shaders = shaderdet;
            Models = modelsdet;
        }

        /// <summary>Texture engine.</summary>
        public TextureEngine TEngine;

        /// <summary>Shader engine.</summary>
        public ShaderEngine Shaders;

        /// <summary>Model engine.</summary>
        public ModelEngine Models;

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
                OutputType.WARNING.Output("Invalid line box from " + min + " to " + max);
                OutputType.DEBUG.Output(Environment.StackTrace);
                return;
            }
            GL.ActiveTexture(TextureUnit.Texture0);
            TEngine.White.Bind();
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
            // TODO: Efficiency!
            float len = (float)(end - start).Length();
            Location vecang = MathUtilities.VectorToAngles(start - end);
            vecang.Yaw += 180;
            Matrix4d mat = Matrix4d.Scale(len, 1, 1)
                * Matrix4d.CreateRotationY((float)(vecang.Y * MathUtilities.PI180))
                * Matrix4d.CreateRotationZ((float)(vecang.Z * MathUtilities.PI180))
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
            float len = (float)(end - start).Length();
            Location vecang = MathUtilities.VectorToAngles(start - end);
            vecang.Yaw += 180;
            Matrix4d mat = Matrix4d.CreateRotationY((float)(90 * MathUtilities.PI180))
                * Matrix4d.Scale(len, width, width)
                * Matrix4d.CreateRotationY((float)(vecang.Y * MathUtilities.PI180))
                * Matrix4d.CreateRotationZ((float)(vecang.Z * MathUtilities.PI180))
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

        /// <summary>Renders a 3D rectangle.</summary>
        /// <param name="mat">The matrix.</param>
        public void RenderRectangle3D(Matrix4 mat)
        {
            GL.UniformMatrix4(2, false, ref mat);
            GL.BindVertexArray(Square.Internal.VAO);
            GL.DrawElements(PrimitiveType.Triangles, 6, DrawElementsType.UnsignedInt, IntPtr.Zero);
            GL.BindVertexArray(0);
        }

        /// <summary>Renders a 2D rectangle.</summary>
        /// <param name="xmin">The lower bounds of the the rectangle: X coordinate.</param>
        /// <param name="ymin">The lower bounds of the the rectangle: Y coordinate.</param>
        /// <param name="xmax">The upper bounds of the the rectangle: X coordinate.</param>
        /// <param name="ymax">The upper bounds of the the rectangle: Y coordinate.</param>
        /// <param name="rot">The rotation matrix, if any.</param>
        public void RenderRectangle(float xmin, float ymin, float xmax, float ymax, Matrix4? rot = null)
        {
            Matrix4 mat = Matrix4.CreateScale(xmax - xmin, ymax - ymin, 1) * (rot != null && rot.HasValue ? rot.Value : Matrix4.Identity) * Matrix4.CreateTranslation(xmin, ymin, 0);
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
        /// <param name="pos">Start position.</param>
        /// <param name="p2">End position.</param>
        /// <param name="width">Width of the line.</param>
        /// <param name="facing">Facing target.</param>
        /// <param name="view">Relevant view.</param>
        public void RenderBilboardLine(Location pos, Location p2, float width, Location facing, View3D view)
        {
            Location center = (pos + p2) * 0.5;
            double len = (center - facing).Length();
            Location lookdir = (center - facing) / len;
            double len2 = (p2 - pos).Length();
            if (len < 0.001 || len2 < 0.001)
            {
                return;
            }
            Location updir = (p2 - pos) / len2;
            Location right = updir.CrossProduct(lookdir);
            Matrix4d mat = Matrix4d.CreateTranslation(-0.5f, -0.5f, 0f) * Matrix4d.Scale((float)len2 * 0.5f, width, 1f);
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
        }
    }
}
