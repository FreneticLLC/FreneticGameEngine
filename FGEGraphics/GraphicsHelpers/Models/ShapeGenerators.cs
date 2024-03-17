//
// This file is part of the Frenetic Game Engine, created by Frenetic LLC.
// This code is Copyright (C) Frenetic LLC under the terms of a strict license.
// See README.md or LICENSE.txt in the FreneticGameEngine source root for the contents of the license.
// If neither of these are available, assume that neither you nor anyone other than the copyright holder
// hold any right or permission to use this software until such time as the official license is identified.
//

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
    public static Model GenerateSphere(float radius, uint stacks, uint slices, ModelEngine modelEngine)
    {
        if (stacks > slices)
        {
            Logs.Warning("Sphere has more stacks than slices, this may result in incorrect normals.");
        }
        uint vertexCount = (stacks + 1) * (slices + 1);
        uint numIndices = stacks * slices * 6;
        List<Vector3> vertices = new((int)vertexCount);
        List<Vector3> normals = new((int)vertexCount);
        List<Vector2> texCoords = new((int)vertexCount);
        uint[] indices = new uint[numIndices];
        int index = 0;
        for (uint i = 0; i <= stacks; i++)
        {
            float theta = i * MathHelper.Pi / stacks;
            float sinTheta = (float)Math.Sin(theta);
            float cosTheta = (float)Math.Cos(theta);
            for (uint j = 0; j <= slices; j++)
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
                    uint currentRow = i * (slices + 1);
                    uint nextRow = (i + 1) * (slices + 1);
                    indices[index++] = currentRow + j;
                    indices[index++] = nextRow + j;
                    indices[index++] = currentRow + j + 1;
                    indices[index++] = currentRow + j + 1;
                    indices[index++] = nextRow + j;
                    indices[index++] = nextRow + j + 1;
                }
            }
        }
        return GetModelAfterGenerating(modelEngine, "sphere", vertices, normals, texCoords, indices);
    }

    /// <summary>Generates a 2D circle model.</summary>
    public static Model Generated2DCircle(float radius, uint slices, ModelEngine modelEngine)
    {
        uint vertexCount = slices;
        uint numIndices = slices * 3;
        List<Vector3> vertices = new((int)vertexCount);
        List<Vector3> normals = new((int)vertexCount);
        List<Vector2> texCoords = new((int)vertexCount);
        uint[] indices = new uint[numIndices];
        int index = 0;
        for (uint i = 0; i <= slices; i++)
        {
            float phi = i * 2 * MathHelper.Pi / slices;
            float sinPhi = (float)Math.Sin(phi);
            float cosPhi = (float)Math.Cos(phi);
            float x = radius * cosPhi;
            float y = radius * sinPhi;
            float z = 0;
            vertices.Add(new Vector3(x, y, z));
            normals.Add(new Vector3(x, y, z).Normalized());
            texCoords.Add(new Vector2(cosPhi, sinPhi));
            if (i < slices)
            {
                indices[index++] = 0;
                indices[index++] = i + 1;
                indices[index++] = i + 2;
            }
        }
        return GetModelAfterGenerating(modelEngine, "circle", vertices, normals, texCoords, indices);
    }

    /// <summary>Generates a 3D cylinder model.</summary>
    public static Model GenerateCylinder(float radius, float height, uint slices, uint stacks, ModelEngine modelEngine)
    {
        uint vertexCount = (stacks + 1) * (slices + 1);
        uint numIndices = (stacks * slices * 6) * 2;
        List<Vector3> vertices = new((int)vertexCount);
        List<Vector3> normals = new((int)vertexCount);
        List<Vector2> texCoords = new((int)vertexCount);
        uint[] indices = new uint[numIndices];
        int index = 0;
        for (uint i = 0; i <= slices; i++)
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
                indices[index++] = vertexCount - 1;
                indices[index++] = vertexCount - 2 - i;
                indices[index++] = vertexCount - 3 - i;
            }
        }
        for (uint i = 0; i <= stacks; i++)
        {
            float theta = i * MathHelper.Pi / stacks;
            float cosTheta = (float)Math.Cos(theta);
            for (uint j = 0; j <= slices; j++)
            {
                float phi = j * 2 * MathHelper.Pi / slices;
                float sinPhi = (float)Math.Sin(phi);
                float cosPhi = (float)Math.Cos(phi);
                float x = radius * cosPhi;
                float y = radius * sinPhi;
                float z = height * cosTheta;
                vertices.Add(new Vector3(x, y, z));
                normals.Add(new Vector3(x, y, 0).Normalized());
                texCoords.Add(new Vector2(cosPhi, sinPhi));
                if (i < stacks && j < slices)
                {
                    uint currentRow = i * (slices + 1);
                    uint nextRow = (i + 1) * (slices + 1);
                    indices[index++] = currentRow + j;
                    indices[index++] = nextRow + j;
                    indices[index++] = currentRow + j + 1;
                    indices[index++] = currentRow + j + 1;
                    indices[index++] = nextRow + j;
                    indices[index++] = nextRow + j + 1;
                }
            }
        }
        for (uint i = 0; i <= slices; i++)
        {
            float phi = i * 2 * MathHelper.Pi / slices;
            float sinPhi = (float)Math.Sin(phi);
            float cosPhi = (float)Math.Cos(phi);
            float x = radius * cosPhi;
            float y = radius * sinPhi;
            float z = -height;
            vertices.Add(new Vector3(x, y, z));
            normals.Add(new Vector3(x, y, z).Normalized());
            texCoords.Add(new Vector2(cosPhi, sinPhi));
            if (i < slices)
            {
                indices[index++] = 0;
                indices[index++] = i + 1;
                indices[index++] = i + 2;
            }
        }
        Array.Reverse(indices);
        return GetModelAfterGenerating(modelEngine, "cylinder", vertices, normals, texCoords, indices);
    }

    /// <summary>
    /// Generates a 3D torus (donut) model.
    /// Note that the number of sides should be less than or equal to the number of rings to avoid incorrect normals.
    /// </summary>
    public static Model GenerateTorus(float radius, float tubeRadius, uint sides, uint rings, ModelEngine modelEngine)
    {
        if (sides > rings)
        {
            Logs.Warning("Torus has more sides than rings, this may result in incorrect normals.");
        }
        uint vertexCount = rings * sides + 1;
        uint numIndices = rings * sides * 6;
        List<Vector3> vertices = new((int)vertexCount);
        List<Vector3> normals = new((int)vertexCount);
        List<Vector2> textureCoords = new((int)vertexCount);
        uint[] indices = new uint[numIndices];
        int indexIndex = 0;
        for (uint i = 0; i <= rings; i++)
        {
            float theta = i * MathHelper.TwoPi / rings;
            float sinTheta = (float)Math.Sin(theta);
            float cosTheta = (float)Math.Cos(theta);
            for (uint j = 0; j <= sides; j++)
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
                    uint currentRow = i * (sides + 1);
                    uint nextRow = (i + 1) * (sides + 1);
                    indices[indexIndex++] = currentRow + j + 1;
                    indices[indexIndex++] = nextRow + j + 1;
                    indices[indexIndex++] = currentRow + j;
                    indices[indexIndex++] = nextRow + j + 1;
                    indices[indexIndex++] = nextRow + j;
                    indices[indexIndex++] = currentRow + j;
                }
            }
        }
        return GetModelAfterGenerating(modelEngine, "torus", vertices, normals, textureCoords, indices);
    }

    /// <summary>Returns a shape model after generating it.</summary>
    public static Model GetModelAfterGenerating(ModelEngine engine, string name, List<Vector3> vertices, List<Vector3> normals, List<Vector2> texCoords, uint[] indices)
    {
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
