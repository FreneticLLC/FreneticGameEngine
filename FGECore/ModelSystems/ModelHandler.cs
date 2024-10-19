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
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using FreneticUtilities.FreneticExtensions;
using FGECore.FileSystems;
using FGECore.MathHelpers;
using FGECore.PhysicsSystem;
using BepuPhysics.Collidables;
using BepuUtilities.Memory;

namespace FGECore.ModelSystems;

/// <summary>Handles abstract 3D models. Can be purposed for both collision systems and rendering.</summary>
public class ModelHandler
{
    /// <summary>Loads a model from .FMD (Frenetic Model Data) input.</summary>
    /// <param name="data">The input FMD data.</param>
    public Model3D LoadModel(byte[] data)
    {
        // TODO: Remove VMD option!
        if (data.Length < "FMD001".Length || (data[0] != 'F' && data[0] != 'V') || data[1] != 'M' || data[2] != 'D')
        {
            throw new Exception("Model3D: Invalid header bits.");
        }
        string vers = ((char)data[3]).ToString() + ((char)data[4]).ToString() + ((char)data[5]).ToString();
        if (!int.TryParse(vers, out int vid))
        {
            throw new Exception("Model3D: Invalid version ID.");
        }
        if (vid < 001)
        {
            throw new Exception("Model3D: Bad version.");
        }
        byte[] dat_filt = new byte[data.Length - "FMD001".Length];
        Array.ConstrainedCopy(data, "FMD001".Length, dat_filt, 0, dat_filt.Length);
        dat_filt = FileUtilities.UnGZip(dat_filt);
        DataStream ds = new(dat_filt);
        DataReader dr = new(ds);
        Model3D mod = new();
        Matrix4x4 matA = ReadMat(dr);
        mod.MatrixA = matA;
        int meshCount = dr.ReadInt();
        mod.Meshes = new Model3DMesh[meshCount];
        for (int m = 0; m < meshCount; m++)
        {
            Model3DMesh mesh = new();
            mod.Meshes[m] = mesh;
            mesh.Name = dr.ReadFullString();
            int vertexCount = dr.ReadInt();
            mesh.Vertices = new Vector3[vertexCount];
            for (int v = 0; v < vertexCount; v++)
            {
                float f1 = dr.ReadFloat();
                float f2 = dr.ReadFloat();
                float f3 = dr.ReadFloat();
                mesh.Vertices[v] = new Vector3(f1, f2, f3);
            }
            int indiceCount = dr.ReadInt() * 3;
            mesh.Indices = new uint[indiceCount];
            for (int i = 0; i < indiceCount; i++)
            {
                mesh.Indices[i] = dr.ReadUInt();
            }
            int tcCount = dr.ReadInt();
            mesh.TexCoords = new Vector2[tcCount];
            for (int t = 0; t < tcCount; t++)
            {
                float f1 = dr.ReadFloat();
                float f2 = dr.ReadFloat();
                mesh.TexCoords[t] = new Vector2(f1, f2);
            }
            int normCount = dr.ReadInt();
            mesh.Normals = new Vector3[normCount];
            for (int n = 0; n < normCount; n++)
            {
                float f1 = dr.ReadFloat();
                float f2 = dr.ReadFloat();
                float f3 = dr.ReadFloat();
                mesh.Normals[n] = new Vector3(f1, f2, f3);
            }
            int boneCount = dr.ReadInt();
            mesh.Bones = new Model3DBone[boneCount];
            for (int b = 0; b < boneCount; b++)
            {
                Model3DBone bone = new();
                mesh.Bones[b] = bone;
                bone.Name = dr.ReadFullString();
                int weights = dr.ReadInt();
                bone.IDs = new int[weights];
                bone.Weights = new double[weights];
                for (int w = 0; w < weights; w++)
                {
                    bone.IDs[w] = dr.ReadInt();
                    bone.Weights[w] = dr.ReadFloat();
                }
                bone.MatrixA = ReadMat(dr);
            }
        }
        mod.RootNode = ReadSingleNode(null, dr);
        return mod;
    }

    /// <summary>Reads a single node from a model.</summary>
    /// <param name="root">The root node.</param>
    /// <param name="dr">The data reader.</param>
    /// <returns>The node.</returns>
    public Model3DNode ReadSingleNode(Model3DNode root, DataReader dr)
    {
        Model3DNode n = new() { Parent = root };
        string nname = dr.ReadFullString();
        n.Name = nname;
        n.MatrixA = ReadMat(dr);
        int cCount = dr.ReadInt();
        n.Children = new Model3DNode[cCount];
        for (int i = 0; i < cCount; i++)
        {
            n.Children[i] = ReadSingleNode(n, dr);
        }
        return n;
    }

    /// <summary>Reads a matrix.</summary>
    /// <param name="reader">The data reader.</param>
    /// <returns>The matrix.</returns>
    public static Matrix4x4 ReadMat(DataReader reader)
    {
        float a1 = reader.ReadFloat();
        float a2 = reader.ReadFloat();
        float a3 = reader.ReadFloat();
        float a4 = reader.ReadFloat();
        float b1 = reader.ReadFloat();
        float b2 = reader.ReadFloat();
        float b3 = reader.ReadFloat();
        float b4 = reader.ReadFloat();
        float c1 = reader.ReadFloat();
        float c2 = reader.ReadFloat();
        float c3 = reader.ReadFloat();
        float c4 = reader.ReadFloat();
        float d1 = reader.ReadFloat();
        float d2 = reader.ReadFloat();
        float d3 = reader.ReadFloat();
        float d4 = reader.ReadFloat();
        return new Matrix4x4(a1, a2, a3, a4, b1, b2, b3, b4, c1, c2, c3, c4, d1, d2, d3, d4);
        //return new Matrix(a1, b1, c1, d1, a2, b2, c2, d2, a3, b3, c3, d3, a4, b4, c4, d4);
    }

    /// <summary>Iterates over all COLLISION ENABLED vertices of a model (if "collision" is used).</summary>
    /// <param name="input">The model.</param>
    /// <returns>The collision vertices.</returns>
    public static IEnumerable<Vector3[]> IterateCollisionVertices(Model3D input)
    {
        foreach (Model3DMesh mesh in input.Meshes)
        {
            if (mesh.Name.ToLowerFast().Contains("collision"))
            {
                yield return mesh.Vertices;
            }
        }
    }

    /// <summary>Iterates over all COLLISION ENABLED vertices of a model (if "collision" isn't used).</summary>
    /// <param name="input">The model.</param>
    /// <returns>The collision vertices.</returns>
    public static IEnumerable<Vector3[]> IteratePossibleCollisionVertices(Model3D input)
    {
        foreach (Model3DMesh mesh in input.Meshes)
        {
            if (!mesh.Name.ToLowerFast().Contains("nocollide"))
            {
                yield return mesh.Vertices;
            }
        }
    }

    /// <summary>Gets all COLLISION ENABLED vertices of a model.</summary>
    /// <param name="input">The model.</param>
    /// <returns>The collision vertices.</returns>
    public static Vector3[] GetCollisionVertices(Model3D input)
    {
        int count = 0;
        bool colOnly = true;
        foreach (Vector3[] vertices in IterateCollisionVertices(input))
        {
            count += vertices.Length;
        }
        if (count == 0)
        {
            colOnly = false;
            foreach (Vector3[] vertices in IteratePossibleCollisionVertices(input))
            {
                count += vertices.Length;
            }
        }
        Vector3[] resultVertices = new Vector3[count];
        count = 0;
        foreach (Vector3[] vertices in (colOnly ? IterateCollisionVertices(input) : IteratePossibleCollisionVertices(input)))
        {
            vertices.CopyTo(resultVertices, count);
            count += vertices.Length;
        }
        return resultVertices;
    }

    /// <summary>Converts a mesh to a BEPU perfect mesh.</summary>
    /// <param name="space">The relevant physics space.</param>
    /// <param name="input">The model.</param>
    /// <param name="verts">The vertice count if needed.</param>
    /// <returns>The BEPU mesh.</returns>
    public static Mesh MeshToBepu(PhysicsSpace space, Model3D input, out int verts)
    {
        Vector3[] vertices = GetCollisionVertices(input);
        verts = vertices.Length;
        int tris = vertices.Length / 3;
        space.Internal.Pool.Take(tris, out Buffer<Triangle> triangles);
        for (int i = 0; i < tris; i++)
        {
            triangles[i] = new Triangle(vertices[i * 3], vertices[i * 3 + 1], vertices[i * 3 + 2]);
        }
        return new Mesh(triangles, Vector3.One, space.Internal.Pool);
    }

    /// <summary>Converts a mesh to a BEPU convex hull.</summary>
    /// <param name="space">The relevant physics space.</param>
    /// <param name="input">The model.</param>
    /// <param name="verts">The vertex count if needed.</param>
    /// <param name="center">The center output.</param>
    public static ConvexHull MeshToBepuConvex(PhysicsSpace space, Model3D input, out int verts, out Location center)
    {
        Span<Vector3> vertices = new(GetCollisionVertices(input));
        verts = vertices.Length;
        return MeshToBepuConvex(space, vertices, out center);
    }

    /// <summary>Converts a mesh to a BEPU convex hull.</summary>
    /// <param name="space">The relevant physics space.</param>
    /// <param name="vertices">Triplets of vertices - 3 vertices per face.</param>
    /// <param name="center">The center output.</param>
    public static ConvexHull MeshToBepuConvex(PhysicsSpace space, Span<Vector3> vertices, out Location center)
    {
        if (!ConvexHullHelper.CreateShape(vertices, space.Internal.Pool, out Vector3 centerNumerics, out ConvexHull hull))
        {
            throw new InvalidOperationException("Cannot create a convex hull from the given vertices.");
        }
        center = centerNumerics.ToLocation();
        return hull;
    }
}
