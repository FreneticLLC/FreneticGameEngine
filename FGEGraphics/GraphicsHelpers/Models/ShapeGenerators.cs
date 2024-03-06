//
// This file is part of the Frenetic Game Engine, created by Frenetic LLC.
// This code is Copyright (C) Frenetic LLC under the terms of a strict license.
// See README.md or LICENSE.txt in the FreneticGameEngine source root for the contents of the license.
// If neither of these are available, assume that neither you nor anyone other than the copyright holder
// hold any right or permission to use this software until such time as the official license is identified.
//

using FGECore.MathHelpers;
using FGECore.ModelSystems;
using System;
using System.Collections.Generic;
using OpenTK.Mathematics;
using FGECore.CoreSystems;

namespace FGEGraphics.GraphicsHelpers.Models;

/// <summary>Generates shapes and returns them as a model.</summary>
public static class ShapeGenerators
{
    /// <summary>
    /// Generates a 3D sphere model.
    /// Note that the stacks should be lower than the slices to avoid incorrect normals a good example is 10 stacks and 60 slices.
    /// </summary>
    public static Model GenerateSphere(float radius, uint stacks, uint slices, ModelEngine modelEngine, bool reverseOrder = false)
    {
        if (stacks > slices)
        {
            Logs.Warning("Sphere has more stacks than slices, this may result in incorrect normals.");
        }

        // Calculate the number of vertices and indices
        uint vertexCount = stacks * slices;
        uint numIndices = stacks * slices * 6;  // 2 triangles per stack/slice, 3 indices per triangle

        List<Vector3> vertices = new((int)vertexCount);
        List<Vector3> normals = new((int)vertexCount);
        List<Vector2> texCoords = new((int)vertexCount);

        uint[] indices = new uint[numIndices];
        int index = 0;

        for (int i = 0; i <= stacks; i++)
        {
            float theta = i * MathHelper.Pi / stacks;
            float sinTheta = (float)Math.Sin(theta);
            float cosTheta = (float)Math.Cos(theta);

            for (int j = 0; j <= slices; j++)
            {
                float phi = j * 2 * MathHelper.Pi / slices;
                float sinPhi = (float)Math.Sin(phi);
                float cosPhi = (float)Math.Cos(phi);

                float x = radius * cosPhi * sinTheta;
                float y = radius * cosTheta;
                float z = radius * sinPhi * sinTheta;

                vertices.Add(new Vector3(x, y, z));
                normals.Add(new Vector3(x, y, z).Normalized());
                texCoords.Add(new Vector2((float)j / slices, (float)i / stacks));

                if (i < stacks && j < slices)
                {
                    uint currentRow = (uint)(i * (slices + 1));
                    uint nextRow = (uint)((i + 1) * (slices + 1));

                    indices[index++] = (uint)(currentRow + j);
                    indices[index++] = (uint)(nextRow + j);
                    indices[index++] = (uint)(currentRow + j + 1);

                    indices[index++] = (uint)(currentRow + j + 1);
                    indices[index++] = (uint)(nextRow + j);
                    indices[index++] = (uint)(nextRow + j + 1);
                }
            }
        }

        if (reverseOrder)
        {
            Array.Reverse(indices);
        }

        return GetModelAfterGenerating(modelEngine, "sphere", vertices, normals, texCoords, indices);
    }

    /// <summary>Generates a 2D circle model.</summary>
    public static Model Generated2DCircle(float radius, uint slices, ModelEngine modelEngine, bool reverseOrder = false)
    {
        // Calculate the number of vertices and indices
        uint vertexCount = slices;
        uint numIndices = slices * 3;  // 1 triangle per slice, 3 indices per triangle

        List<Vector3> vertices = new((int)vertexCount);
        List<Vector3> normals = new((int)vertexCount);
        List<Vector2> texCoords = new((int)vertexCount);

        uint[] indices = new uint[numIndices];
        int index = 0;

        for (int i = 0; i <= slices; i++)
        {
            float phi = i * 2 * MathHelper.Pi / slices;
            float sinPhi = (float)Math.Sin(phi);
            float cosPhi = (float)Math.Cos(phi);

            float x = radius * cosPhi;
            float y = radius * sinPhi;
            float z = 0;

            vertices.Add(new Vector3(x, y, z));
            normals.Add(new Vector3(x, y, z).Normalized());
            texCoords.Add(new Vector2((float)cosPhi, (float)sinPhi));

            if (i < slices)
            {
                indices[index++] = 0;
                indices[index++] = (uint)(i + 1);
                indices[index++] = (uint)(i + 2);
            }
        }

        if (reverseOrder)
        {
            Array.Reverse(indices);
        }

        return GetModelAfterGenerating(modelEngine, "circle", vertices, normals, texCoords, indices);
    }

    /// <summary>Generates a 3D cylinder model.</summary>
    public static Model GenerateCylinder(float radius, float height, uint slices, uint stacks, ModelEngine modelEngine, bool reverseOrder = true)
    {
        // Calculate the number of vertices and indices
        uint vertexCount = stacks * slices;
        // We need to account for top and bottom faces, so we multiply by 2
        uint numIndices = (stacks * slices * 6) * 2;  // 2 triangles per stack/slice, 3 indices per triangle

        List<Vector3> vertices = new((int)vertexCount);
        List<Vector3> normals = new((int)vertexCount);
        List<Vector2> texCoords = new((int)vertexCount);

        uint[] indices = new uint[numIndices];
        int index = 0;

        // Top
        for (int i = 0; i <= slices; i++)
        {
            float phi = i * 2 * MathHelper.Pi / slices;
            float sinPhi = (float)Math.Sin(phi);
            float cosPhi = (float)Math.Cos(phi);

            float x = radius * cosPhi;
            float y = radius * sinPhi;
            float z = height;

            vertices.Add(new Vector3(x, y, z));
            normals.Add(new Vector3(x, y, z));
            texCoords.Add(new Vector2((float)cosPhi, (float)sinPhi));

            if (i < slices)
            {
                indices[index++] = (vertexCount - 1);
                indices[index++] = (uint)(vertexCount - 2 - i);
                indices[index++] = (uint)(vertexCount - 2 - i - 1);
            }
        }

        // Middle
        for (int i = 0; i <= stacks; i++)
        {
            float theta = i * MathHelper.Pi / stacks;
            float cosTheta = (float)Math.Cos(theta);

            for (int j = 0; j <= slices; j++)
            {
                float phi = j * 2 * MathHelper.Pi / slices;
                float sinPhi = (float)Math.Sin(phi);
                float cosPhi = (float)Math.Cos(phi);

                float x = radius * cosPhi;
                float y = radius * sinPhi;
                float z = height * cosTheta;

                vertices.Add(new Vector3(x, y, z));
                normals.Add(new Vector3(x, y, 0).Normalized());
                texCoords.Add(new Vector2((float)cosPhi, (float)sinPhi));

                if (i < stacks && j < slices)
                {
                    uint currentRow = (uint)(i * (slices + 1));
                    uint nextRow = (uint)((i + 1) * (slices + 1));

                    indices[index++] = (uint)(currentRow + j);
                    indices[index++] = (uint)(nextRow + j);
                    indices[index++] = (uint)(currentRow + j + 1);

                    indices[index++] = (uint)(currentRow + j + 1);
                    indices[index++] = (uint)(nextRow + j);
                    indices[index++] = (uint)(nextRow + j + 1);
                }
            }
        }

        // Bottom
        for (int i = 0; i <= slices; i++)
        {
            float phi = i * 2 * MathHelper.Pi / slices;
            float sinPhi = (float)Math.Sin(phi);
            float cosPhi = (float)Math.Cos(phi);

            float x = radius * cosPhi;
            float y = radius * sinPhi;
            float z = -height;

            vertices.Add(new Vector3(x, y, z));
            normals.Add(new Vector3(x, y, z).Normalized());
            texCoords.Add(new Vector2((float)cosPhi, (float)sinPhi));

            if (i < slices)
            {
                indices[index++] = 0;
                indices[index++] = (uint)(i + 1);
                indices[index++] = (uint)(i + 2);
            }
        }

        // Reverse order to account for winding order
        if (reverseOrder)
        {
            Array.Reverse(indices);
        }

        return GetModelAfterGenerating(modelEngine, "cylinder", vertices, normals, texCoords, indices);
    }

    /// <summary>
    /// Generates a 3D torus (donut) model.
    /// Note that the number of sides should be less than or equal to the number of rings to avoid incorrect normals.
    /// </summary>
    public static Model GenerateTorus(float radius, float tubeRadius, uint sides, uint rings, ModelEngine modelEngine, bool reverseOrder = true)
    {
        if (sides > rings)
        {
            Logs.Warning("Torus has more sides than rings, this may result in incorrect normals.");
        }

        // Calculate the number of vertices and indices
        uint vertexCount = rings * sides + 1;
        uint numIndices = rings * sides * 6;  // 2 triangles per ring/side, 3 indices per triangle

        List<Vector3> vertices = new((int)vertexCount);
        List<Vector3> normals = new((int)vertexCount);
        List<Vector2> textureCoords = new((int)vertexCount);

        uint[] indices = new uint[numIndices];
        int indexIndex = 0;

        for (int i = 0; i <= rings; i++)
        {
            float theta = i * MathHelper.TwoPi / rings;
            float sinTheta = (float)Math.Sin(theta);
            float cosTheta = (float)Math.Cos(theta);

            for (int j = 0; j <= sides; j++)
            {
                float phi = j * MathHelper.TwoPi / sides;
                float sinPhi = (float)Math.Sin(phi);
                float cosPhi = (float)Math.Cos(phi);

                float centerX = radius * cosPhi;
                float centerY = 0;
                float centerZ = radius * sinPhi;

                float x = (radius + tubeRadius * cosTheta) * cosPhi;
                float y = tubeRadius * sinTheta;
                float z = (radius + tubeRadius * cosTheta) * sinPhi;

                vertices.Add(new Vector3(x, y, z));
                Vector3 pointOnSurface = new(x, y, z);
                Vector3 centerToSurface = pointOnSurface - new Vector3(centerX, centerY, centerZ);
                normals.Add(centerToSurface.Normalized());
                textureCoords.Add(new Vector2((float)j / sides, (float)i / rings));

                if (i < rings && j < sides)
                {
                    uint currentRow = (uint)(i * (sides + 1));
                    uint nextRow = (uint)((i + 1) * (sides + 1));

                    indices[indexIndex++] = (uint)(currentRow + j);
                    indices[indexIndex++] = (uint)(nextRow + j);
                    indices[indexIndex++] = (uint)(nextRow + j + 1);

                    indices[indexIndex++] = (uint)(currentRow + j);
                    indices[indexIndex++] = (uint)(nextRow + j + 1);
                    indices[indexIndex++] = (uint)(currentRow + j + 1);
                }
            }
        }

        if (reverseOrder)
        {
            Array.Reverse(indices);
        }

        return GetModelAfterGenerating(modelEngine, "torus", vertices, normals, textureCoords, indices);
    }

    private static Model GetModelAfterGenerating(ModelEngine engine, string name, List<Vector3> vertices, List<Vector3> normals, List<Vector2> texCoords, uint[] indices)
    {
        Renderable.ListBuilder builder = new();
        builder.Prepare();
        builder.Vertices = vertices;
        builder.Normals = normals;
        builder.TexCoords = texCoords.ConvertAll(t => new Vector3(t.X, t.Y, 0));
        builder.Indices = new List<uint>(indices);
        for (int i = 0; i < vertices.Count; i++)
        {
            builder.AddEmptyBoneInfo();
            builder.Colors.Add(new Vector4(1, 1, 1, 1));
        }
        Model3DMesh mesh = new()
        {
            Name = name,
            Vertices = [.. vertices.ConvertAll(v => v.ToLocation().ToNumerics())],
            Normals = [.. normals.ConvertAll(n => n.ToLocation().ToNumerics())],
            TexCoords = [.. texCoords.ConvertAll(t => new System.Numerics.Vector2(t.X, t.Y))],
            Indices = indices,
            Bones = []
        };

        Model3D model3D = new()
        {
            RootNode = new Model3DNode()
            {
                Name = name,
                Children = [],
            },            
            Meshes = [mesh]
        };

        return engine.FromScene(model3D, name);
    }
}
