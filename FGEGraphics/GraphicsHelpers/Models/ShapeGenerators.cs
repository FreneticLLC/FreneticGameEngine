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

namespace FGEGraphics.GraphicsHelpers.Models;

/// <summary>
/// Generates shapes and returns them as a model.
/// </summary>
public static class ShapeGenerators
{
    /// <summary>Generates a 3D sphere model.</summary>
    /// <param name="radius"></param>
    /// <param name="stacks"></param>
    /// <param name="slices"></param>
    /// <param name="modelEngine"></param>
    /// <returns>The generated model.</returns>
    public static Model GenerateSphere(float radius, uint stacks, uint slices, ModelEngine modelEngine)
    {
        // Calculate the number of vertices and indices
        uint vertexCount = (stacks + 1) * (slices + 1);
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

                Vector3 normal = new(x, y, z);
                normals.Add(normal.Normalized());

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

        if (vertices.Count != normals.Count || vertices.Count != texCoords.Count)
            throw new ArgumentException("Position, normal, and texture coordinate lists must have the same length.");

        Model sphereModel = new("sphere")
        {
            Engine = modelEngine,
            Skinned = true,
            ModelMin = new Location(-0.5),
            ModelMax = new Location(0.5),
            Original = new Model3D()
        };
        ModelMesh sphereMesh = new("sphere");
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
            Name = "sphere",
            Vertices = [..vertices.ConvertAll(v => v.ToLocation().ToNumerics())],
            Normals = [..normals.ConvertAll(n => n.ToLocation().ToNumerics())],
            TexCoords = [..texCoords.ConvertAll(t => new System.Numerics.Vector2(t.X, t.Y))],
            Indices = indices
        };
        sphereModel.Original.Meshes = [mesh];
        sphereMesh.BaseRenderable.GenerateVBO(builder);
        sphereModel.AddMesh(sphereMesh);

        return sphereModel;
    }
}
