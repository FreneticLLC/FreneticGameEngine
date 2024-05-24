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
using FGECore.CoreSystems;
using System.Numerics;

namespace FGEGraphics.GraphicsHelpers.Models;

/// <summary>Generates shapes and returns them as a model.</summary>
public static class ShapeGenerators
{
    /// <summary>Generates a 3D UV-Sphere model, centered at 0,0,0. A UV-sphere is characterized by quad sidings, with triangles on top/bottom.
    /// This is type of sphere most people visualize, sometimes also known as a globe model.</summary>
    /// <param name="radius">Distance from center to an edge, standard round-object radius.</param>
    /// <param name="vstacks">Total number of vertical stacks of vertices (lines of latitude). Minimum 3.</param>
    /// <param name="hslices">Total number of horizontal slices of vertices (lines of longitude). Minimum 3.</param>
    public static Model3D GenerateUVSphere(float radius, uint vstacks, uint hslices)
    {
        vstacks -= 2; // Account for top/bottom.
        uint vstacksPlus1 = vstacks + 1;
        uint vstacksMin1 = vstacks - 1;
        uint vertexCount = 2 + vstacks * hslices;
        uint numIndices = hslices * vstacks * 6;
        Vector3[] vertices = new Vector3[vertexCount];
        Vector3[] normals = new Vector3[vertexCount];
        Vector2[] texCoords = new Vector2[vertexCount];
        uint[] indices = new uint[numIndices];
        vertices[0] = new(0, 0, radius);
        normals[0] = new(0, 0, 1);
        texCoords[0] = new(0.5f, 0.5f);
        for (uint vi = 0; vi < vstacks; vi++)
        {
            float radianStack = MathF.PI * (vi + 1) / vstacksPlus1;
            float sinStack = MathF.Sin(radianStack);
            float cosStack = MathF.Cos(radianStack);
            for (uint hi = 0; hi < hslices; hi++)
            {
                float radianSlice = MathF.Tau * hi / hslices;
                float sinSlice = MathF.Sin(radianSlice);
                float cosSlice = MathF.Cos(radianSlice);
                float x = radius * cosSlice * sinStack;
                float y = radius * sinSlice * sinStack;
                float z = radius * cosStack;
                Vector3 vec = new(x, y, z);
                uint vInd = vi * hslices + hi + 1;
                vertices[vInd] = vec;
                normals[vInd] = vec / radius;
                texCoords[vInd] = new((float)hi / hslices, (float)vi / vstacksPlus1);
            }
        }
        uint bottomInd = vertexCount - 1;
        vertices[bottomInd] = new(0, 0, -radius);
        normals[bottomInd] = new(0, 0, -1);
        texCoords[bottomInd] = new(0.5f, 0.5f);
        // Triangles for top/bottom
        for (uint hi = 0; hi < hslices; hi++)
        {
            indices[hi * 6] = 0;
            indices[hi * 6 + 1] = (hi + 1) % hslices + 1;
            indices[hi * 6 + 2] = hi + 1;
            indices[hi * 6 + 3] = bottomInd;
            indices[hi * 6 + 4] = hi + hslices * vstacksMin1 + 1;
            indices[hi * 6 + 5] = (hi + 1) % hslices + hslices * vstacksMin1 + 1;
        }
        // The rest of the sides (quads)
        for (uint vi = 0; vi < vstacksMin1; vi++)
        {
            uint row = vi * hslices + 1;
            uint nextRow = (vi + 1) * hslices + 1;
            for (uint hi = 0; hi < hslices; hi++)
            {
                uint index = (vi * hslices + hi + hslices) * 6;
                uint column = (hi + 1) % hslices;
                indices[index] = row + hi;
                indices[index + 1] = row + column;
                indices[index + 2] = nextRow + column;
                indices[index + 3] = row + hi;
                indices[index + 4] = nextRow + column;
                indices[index + 5] = nextRow + hi;
            }
        }
        return Internal.GetModelAfterGenerating("sphere", vertices, normals, texCoords, indices);
    }

    /// <summary>Generates a 2D circle model, centered at 0,0,0.</summary>
    public static Model3D Generate2DCircle(float radius, uint corners, bool flip = false)
    {
        uint vertexCount = corners + 1;
        uint numIndices = corners * 3;
        Vector3[] vertices = new Vector3[vertexCount];
        Vector3[] normals = new Vector3[vertexCount];
        Vector2[] texCoords = new Vector2[vertexCount];
        uint[] indices = new uint[numIndices];
        Internal.GenerateCircle(vertices, normals, texCoords, indices, radius, corners, 0, 0, 0, flip);
        return Internal.GetModelAfterGenerating("circle", vertices, normals, texCoords, indices);
    }

    /// <summary>Generates a 3D cylinder model, centered at 0,0,0.</summary>
    public static Model3D GenerateCylinder(float radius, float height, uint corners)
    {
        height /= 2;
        uint cornersPlus1 = corners + 1;
        uint sideVertices = cornersPlus1 * 2;
        uint sideIndices = cornersPlus1 * 6;
        uint circleVertices = cornersPlus1;
        uint circleIndices = corners * 3;
        uint vertexCount = sideVertices + (circleVertices * 2);
        uint numIndices = sideIndices + (circleIndices * 2);
        Vector3[] vertices = new Vector3[vertexCount];
        Vector3[] normals = new Vector3[vertexCount];
        Vector2[] texCoords = new Vector2[vertexCount];
        uint[] indices = new uint[numIndices];
        for (uint i = 0; i < cornersPlus1; i++)
        {
            float phi = i * 2 * MathF.PI / corners;
            float sinPhi = (float)Math.Sin(phi);
            float cosPhi = (float)Math.Cos(phi);
            float x = radius * cosPhi;
            float y = radius * sinPhi;
            Vector3 norm = new(x, y, 0);
            vertices[i] = new(x, y, height);
            vertices[i + cornersPlus1] = new(x, y, -height);
            normals[i] = norm / norm.Length();
            normals[i + cornersPlus1] = norm / norm.Length();
            texCoords[i] = new(cosPhi, sinPhi);
            texCoords[i + cornersPlus1] = new(cosPhi, sinPhi);
            uint index = i * 6;
            indices[index] = i + cornersPlus1 + 1;
            indices[index + 1] = i + cornersPlus1;
            indices[index + 2] = i + 1;
            indices[index + 3] = i + 1;
            indices[index + 4] = i + cornersPlus1;
            indices[index + 5] = i;
        }
        Internal.GenerateCircle(vertices, normals, texCoords, indices, radius, corners, sideIndices, sideVertices, height, false);
        Internal.GenerateCircle(vertices, normals, texCoords, indices, radius, corners, sideIndices + circleIndices, sideVertices + circleVertices, -height, true);
        return Internal.GetModelAfterGenerating("cylinder", vertices, normals, texCoords, indices);
    }

    /// <summary>Generates a 3D torus (donut) model, centered at 0,0,0.</summary>
    public static Model3D GenerateTorus(float radius, float tubeRadius, uint sides, uint rings)
    {
        uint vertexCount = rings * sides;
        uint numIndices = rings * sides * 6;
        Vector3[] vertices = new Vector3[vertexCount];
        Vector3[] normals = new Vector3[vertexCount];
        Vector2[] texCoords = new Vector2[vertexCount];
        uint[] indices = new uint[numIndices];
        for (uint ri = 0; ri < rings; ri++)
        {
            float radRing = ri * MathF.Tau / rings;
            float sinRing = (float)Math.Sin(radRing);
            float cosRing = (float)Math.Cos(radRing);
            for (uint si = 0; si < sides; si++)
            {
                uint vInd = ri * sides + si;
                float radSide = si * MathF.Tau / sides;
                float sinSide = (float)Math.Sin(radSide);
                float cosSide = (float)Math.Cos(radSide);
                float centerX = radius * cosSide;
                float centerY = radius * sinSide;
                float centerZ = 0;
                float x = (radius + tubeRadius * cosRing) * cosSide;
                float y = (radius + tubeRadius * cosRing) * sinSide;
                float z = tubeRadius * sinRing;
                Vector3 pointOnSurface = new(x, y, z);
                vertices[vInd] = pointOnSurface;
                Vector3 centerToSurface = pointOnSurface - new Vector3(centerX, centerY, centerZ);
                normals[vInd] = centerToSurface / centerToSurface.Length();
                texCoords[vInd] = new((float)si / sides, (float)ri / rings);
                uint index = (ri * sides + si) * 6;
                uint currentRow = ri * sides;
                uint nextRow = ((ri + 1) % rings) * sides;
                uint nextSide = (si + 1) % sides;
                indices[index] = currentRow + si;
                indices[index + 1] = nextRow + nextSide;
                indices[index + 2] = currentRow + nextSide;
                indices[index + 3] = currentRow + si;
                indices[index + 4] = nextRow + si;
                indices[index + 5] = nextRow + nextSide;
            }
        }
        return Internal.GetModelAfterGenerating("torus", vertices, normals, texCoords, indices);
    }

    /// <summary>Internal methods for shape generation.</summary>
    public static class Internal
    {
        /// <summary>Builds an actual <see cref="Model3D"/> for a given shape's raw data.</summary>
        public static Model3D GetModelAfterGenerating(string name, Vector3[] vertices, Vector3[] normals, Vector2[] texCoords, uint[] indices)
        {
            Model3DMesh mesh = new()
            {
                Name = name,
                Vertices = vertices,
                Normals = normals,
                TexCoords = texCoords,
                Indices = indices,
                Bones = []
            };
            Model3D model3D = new()
            {
                RootNode = new()
                {
                    Name = name,
                    Children = [],
                },
                Meshes = [mesh]
            };
            return model3D;
        }

        /// <summary>Generates a a circle into an existing shape generation.</summary>
        public static void GenerateCircle(Vector3[] vertices, Vector3[] normals, Vector2[] texCoords, uint[] indices, float radius, uint corners, uint startIndex, uint startVertex, float z, bool flip)
        {
            Vector3 normal = new(0, 0, flip ? -1 : 1);
            vertices[startVertex] = new(0, 0, z);
            normals[startVertex] = normal;
            texCoords[startVertex] = new(0.5f, 0.5f);
            uint firstRealVert = startVertex + 1;
            for (uint i = 0; i < corners; i++)
            {
                float radian = i * MathF.Tau / corners;
                float sin = MathF.Sin(radian);
                float cos = MathF.Cos(radian);
                float x = radius * cos;
                float y = radius * sin;
                vertices[firstRealVert + i] = new(x, y, z);
                normals[firstRealVert + i] = normal;
                texCoords[firstRealVert + i] = new(cos, sin);
                uint index = startIndex + i * 3;
                uint next = (i + 1) % corners;
                indices[index] = startVertex;
                indices[index + 1] = firstRealVert + (flip ? i : next);
                indices[index + 2] = firstRealVert + (flip ? next : i);
            }
        }
    }
}
