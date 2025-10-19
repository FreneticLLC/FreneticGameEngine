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
using FreneticUtilities.FreneticExtensions;
using FreneticUtilities.FreneticToolkit;
using FGECore;
using FGECore.CoreSystems;
using FGECore.MathHelpers;
using FGEGraphics.ClientSystem;
using FGEGraphics.GraphicsHelpers.Textures;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace FGEGraphics.GraphicsHelpers;

/// <summary>Represents a Vertex Buffer/Array Object set.</summary>
public class Renderable
{
    /// <summary>A structure of the internal GPU-side data for this <see cref="Renderable"/>.</summary>
    public struct InternalData
    {
        /// <summary>The vertices buffer.</summary>
        public GraphicsUtil.TrackedBuffer VertexVBO;

        /// <summary>The indices buffer.</summary>
        public GraphicsUtil.TrackedBuffer IndexVBO;

        /// <summary>The normals buffer.</summary>
        public GraphicsUtil.TrackedBuffer NormalVBO;

        /// <summary>The texture coordinates buffer.</summary>
        public GraphicsUtil.TrackedBuffer TexCoordVBO;

        /// <summary>The colors buffer.</summary>
        public GraphicsUtil.TrackedBuffer ColorVBO;

        /// <summary>The tangents buffer.</summary>
        public GraphicsUtil.TrackedBuffer TangentVBO;

        /// <summary>The bone IDs buffer.</summary>
        public GraphicsUtil.TrackedBuffer BoneIDVBO;

        /// <summary>The bone weights buffer.</summary>
        public GraphicsUtil.TrackedBuffer BoneWeightVBO;

        /// <summary>The bone IDs (set 2) buffer.</summary>
        public GraphicsUtil.TrackedBuffer BoneID2VBO;

        /// <summary>The bone weights (set 2) buffer.</summary>
        public GraphicsUtil.TrackedBuffer BoneWeight2VBO;

        /// <summary>The internal main Vertex Array Object.</summary>
        public uint VAO;

        /// <summary>How much VRAM this <see cref="Renderable"/> would consume at last generation.</summary>
        public long LastVRAM;

        /// <summary>Whether this <see cref="Renderable"/> has colors.</summary>
        public bool HasColors;

        /// <summary>Whether this <see cref="Renderable"/> has bones.</summary>
        public bool HasBones;

        /// <summary>The number of indices in this <see cref="Renderable"/>.</summary>
        public int IndexCount;

        /// <summary>What buffer mode to use.</summary>
        public BufferUsageHint BufferMode;
    }

    /// <summary>The internal (GPU) data for this <see cref="Renderable"/>.</summary>
    public InternalData Internal = new() { BufferMode = BufferUsageHint.StaticDraw };

    /// <summary>The primary texture.</summary>
    public Texture ColorTexture;

    /// <summary>The specular texture.</summary>
    public Texture SpecularTexture;

    /// <summary>The reflectivity texture.</summary>
    public Texture ReflectivityTexture;

    /// <summary>The normal texture.</summary>
    public Texture NormalTexture;

    /// <summary>Name for this renderable, if any. Useful for debugging mainly.</summary>
    public string Name;

    /// <inheritdoc/>
    public override string ToString()
    {
        return $"Renderable(Name={Name}, ColorTexture={ColorTexture}, SpecularTexture={SpecularTexture}, ReflectivityTexture={ReflectivityTexture}, NormalTexture={NormalTexture})";
    }

    /// <summary>Represents a <see cref="Renderable"/> builder type object.</summary>
    public abstract class Builder
    {
        /// <summary>Generates a VBO from this builder.</summary>
        /// <returns>The generated VBO.</returns>
        public Renderable Generate()
        {
            Renderable vbo = new();
            vbo.GenerateVBO(this);
            return vbo;
        }
    }

    /// <summary>A way to build a <see cref="Renderable"/> from lists.</summary>
    public class ListBuilder : Builder
    {
        /// <summary>The position vertices.</summary>
        public List<Vector3> Vertices;

        /// <summary>The indices.</summary>
        public List<uint> Indices;

        /// <summary>The normals.</summary>
        public List<Vector3> Normals;

        /// <summary>The normal tangents.</summary>
        public List<Vector3> Tangents;

        /// <summary>The texture coordinates.</summary>
        public List<Vector3> TexCoords;

        /// <summary>The colors.</summary>
        public List<Vector4> Colors;

        /// <summary>The bone weight IDs.</summary>
        public List<Vector4> BoneIDs;

        /// <summary>The bone weight levels.</summary>
        public List<Vector4> BoneWeights;

        /// <summary>The second bone weight IDs.</summary>
        public List<Vector4> BoneIDs2;

        /// <summary>The second bone weight levels.</summary>
        public List<Vector4> BoneWeights2;

        /// <summary>Adds an empty bone info (1 vertex worth of pure zeroes).</summary>
        public void AddEmptyBoneInfo()
        {
            BoneIDs.Add(Vector4.Zero);
            BoneWeights.Add(Vector4.Zero);
            BoneIDs2.Add(Vector4.Zero);
            BoneWeights2.Add(Vector4.Zero);
        }

        /// <summary>Sets the capacity of all lists.</summary>
        /// <param name="capacity">The capacity to set.</param>
        public void SetCapacity(int capacity)
        {
            Vertices.Capacity = capacity;
            Indices.Capacity = capacity;
            Normals.Capacity = capacity;
            Tangents.Capacity = capacity;
            TexCoords.Capacity = capacity;
            Colors.Capacity = capacity;
            BoneIDs.Capacity = capacity;
            BoneWeights.Capacity = capacity;
            BoneIDs2.Capacity = capacity;
            BoneWeights2.Capacity = capacity;
        }

        /// <summary>Helper to add an axis-aligned side to this <see cref="Renderable"/> builder.</summary>
        /// <param name="normal">The normal.</param>
        /// <param name="tc">The texture coordinates.</param>
        /// <param name="offs">Whether to offset.</param>
        /// <param name="texf">The texture ID.</param>
        /// <param name="scale">The distance from center for each side.</param>
        public void AddSide(Location normal, TextureCoordinates tc, bool offs = false, float texf = 0, float scale = 1)
        {
            if (Vertices.Capacity < 6)
            {
                SetCapacity(12);
            }
            // TODO: IMPROVE! Or discard?!
            for (int i = 0; i < 6; i++)
            {
                Normals.Add(normal.ToOpenTK());
                Colors.Add(new Vector4(1f, 1f, 1f, 1f));
                Indices.Add((uint)Indices.Count);
                AddEmptyBoneInfo();
            }
            float aX = (tc.XFlip ? 1 : 0) + tc.XShift;
            float aY = (tc.YFlip ? 1 : 0) + tc.YShift;
            float bX = (tc.XFlip ? 0 : 1) * tc.XScale + tc.XShift;
            float bY = (tc.YFlip ? 1 : 0) + tc.YShift;
            float cX = (tc.XFlip ? 0 : 1) * tc.XScale + tc.XShift;
            float cY = (tc.YFlip ? 0 : 1) * tc.YScale + tc.YShift;
            float dX = (tc.XFlip ? 1 : 0) + tc.XShift;
            float dY = (tc.YFlip ? 0 : 1) * tc.YScale + tc.YShift;
            float zero = offs ? scale * -0.5f : -scale; // Sssh
            float one = offs ? scale * 0.5f : scale;
            if (normal.Z == 1)
            {
                // T1
                TexCoords.Add(new Vector3(bX, bY, texf));
                Vertices.Add(new Vector3(zero, zero, one));
                TexCoords.Add(new Vector3(cX, cY, texf));
                Vertices.Add(new Vector3(zero, one, one));
                TexCoords.Add(new Vector3(dX, dY, texf));
                Vertices.Add(new Vector3(one, one, one));
                // T2
                TexCoords.Add(new Vector3(bX, bY, texf));
                Vertices.Add(new Vector3(zero, zero, one));
                TexCoords.Add(new Vector3(dX, dY, texf));
                Vertices.Add(new Vector3(one, one, one));
                TexCoords.Add(new Vector3(aX, aY, texf));
                Vertices.Add(new Vector3(one, zero, one));
            }
            else if (normal.Z == -1)
            {
                // T1
                TexCoords.Add(new Vector3(cX, cY, texf));
                Vertices.Add(new Vector3(one, one, zero));
                TexCoords.Add(new Vector3(dX, dY, texf));
                Vertices.Add(new Vector3(zero, one, zero));
                TexCoords.Add(new Vector3(aX, aY, texf));
                Vertices.Add(new Vector3(zero, zero, zero));
                // T2
                TexCoords.Add(new Vector3(bX, bY, texf));
                Vertices.Add(new Vector3(one, zero, zero));
                TexCoords.Add(new Vector3(cX, cY, texf));
                Vertices.Add(new Vector3(one, one, zero));
                TexCoords.Add(new Vector3(aX, aY, texf));
                Vertices.Add(new Vector3(zero, zero, zero));
            }
            else if (normal.X == 1)
            {
                // T1
                TexCoords.Add(new Vector3(bX, bY, texf));
                Vertices.Add(new Vector3(one, one, one));
                TexCoords.Add(new Vector3(cX, cY, texf));
                Vertices.Add(new Vector3(one, one, zero));
                TexCoords.Add(new Vector3(dX, dY, texf));
                Vertices.Add(new Vector3(one, zero, zero));
                // T2
                TexCoords.Add(new Vector3(aX, aY, texf));
                Vertices.Add(new Vector3(one, zero, one));
                TexCoords.Add(new Vector3(bX, bY, texf));
                Vertices.Add(new Vector3(one, one, one));
                TexCoords.Add(new Vector3(dX, dY, texf));
                Vertices.Add(new Vector3(one, zero, zero));
            }
            else if (normal.X == -1)
            {
                // T1
                TexCoords.Add(new Vector3(cX, cY, texf));
                Vertices.Add(new Vector3(zero, zero, zero));
                TexCoords.Add(new Vector3(dX, dY, texf));
                Vertices.Add(new Vector3(zero, one, zero));
                TexCoords.Add(new Vector3(aX, aY, texf));
                Vertices.Add(new Vector3(zero, one, one));
                // T2
                TexCoords.Add(new Vector3(cX, cY, texf));
                Vertices.Add(new Vector3(zero, zero, zero));
                TexCoords.Add(new Vector3(aX, aY, texf));
                Vertices.Add(new Vector3(zero, one, one));
                TexCoords.Add(new Vector3(bX, bY, texf));
                Vertices.Add(new Vector3(zero, zero, one));
            }
            else if (normal.Y == 1)
            {
                // T1
                TexCoords.Add(new Vector3(aX, aY, texf));
                Vertices.Add(new Vector3(one, one, one));
                TexCoords.Add(new Vector3(bX, bY, texf));
                Vertices.Add(new Vector3(zero, one, one));
                TexCoords.Add(new Vector3(cX, cY, texf));
                Vertices.Add(new Vector3(zero, one, zero));
                // T2
                TexCoords.Add(new Vector3(dX, dY, texf));
                Vertices.Add(new Vector3(one, one, zero));
                TexCoords.Add(new Vector3(aX, aY, texf));
                Vertices.Add(new Vector3(one, one, one));
                TexCoords.Add(new Vector3(cX, cY, texf));
                Vertices.Add(new Vector3(zero, one, zero));
            }
            else if (normal.Y == -1)
            {
                // T1
                TexCoords.Add(new Vector3(dX, dY, texf));
                Vertices.Add(new Vector3(zero, zero, zero));
                TexCoords.Add(new Vector3(aX, aY, texf));
                Vertices.Add(new Vector3(zero, zero, one));
                TexCoords.Add(new Vector3(bX, bY, texf));
                Vertices.Add(new Vector3(one, zero, one));
                // T2
                TexCoords.Add(new Vector3(dX, dY, texf));
                Vertices.Add(new Vector3(zero, zero, zero));
                TexCoords.Add(new Vector3(bX, bY, texf));
                Vertices.Add(new Vector3(one, zero, one));
                TexCoords.Add(new Vector3(cX, cY, texf));
                Vertices.Add(new Vector3(one, zero, zero));
            }
            else
            {
                throw new Exception("Lazy code can't handle unique normals! Only axis-aligned, normalized normals!");
            }
        }

        /// <summary>
        /// Prepare the <see cref="Renderable"/> builder's lists to be added to, for 2D usage.
        /// Does not prepare bones or tangents.
        /// </summary>
        /// <param name="capacity">The initial capacity.</param>
        public void Prepare2D(int capacity = 10)
        {
            Vertices = new List<Vector3>(capacity);
            Indices = new List<uint>(capacity);
            Normals = new List<Vector3>(capacity);
            TexCoords = new List<Vector3>(capacity);
            Colors = new List<Vector4>(capacity);
        }

        /// <summary>
        /// Prepare the <see cref="Renderable"/> builder's lists to be added to.
        /// Does not prepare tangents.
        /// </summary>
        /// <param name="capacity">The initial capacity.</param>
        public void Prepare(int capacity = 10)
        {
            Vertices = new List<Vector3>(capacity);
            Indices = new List<uint>(capacity);
            Normals = new List<Vector3>(capacity);
            TexCoords = new List<Vector3>(capacity);
            Colors = new List<Vector4>(capacity);
            BoneIDs = new List<Vector4>(capacity);
            BoneWeights = new List<Vector4>(capacity);
            BoneIDs2 = new List<Vector4>(capacity);
            BoneWeights2 = new List<Vector4>(capacity);
        }
    }

    /// <summary>A way to build a <see cref="Renderable"/> from arrays.</summary>
    public class ArrayBuilder : Builder
    {
        /// <summary>The position vertices.</summary>
        public Vector3[] Vertices;

        /// <summary>The indices.</summary>
        public uint[] Indices;

        /// <summary>The normals.</summary>
        public Vector3[] Normals;

        /// <summary>The normal tangents.</summary>
        public Vector3[] Tangents;

        /// <summary>The texture coordinates.</summary>
        public Vector3[] TexCoords;

        /// <summary>The colors.</summary>
        public Vector4[] Colors;

        /// <summary>The bone weight IDs.</summary>
        public Vector4[] BoneIDs;

        /// <summary>The bone weight levels.</summary>
        public Vector4[] BoneWeights;

        /// <summary>The second bone weight IDs.</summary>
        public Vector4[] BoneIDs2;

        /// <summary>The second bone weight levels.</summary>
        public Vector4[] BoneWeights2;

        /// <summary>Sets the bone info for a given vertex index to zero.</summary>
        /// <param name="index">The index in the arrays of the vertex to zero the bones for.</param>
        public void ZeroBoneInfo(uint index)
        {
            BoneIDs[index] = Vector4.Zero;
            BoneWeights[index] = Vector4.Zero;
            BoneIDs2[index] = Vector4.Zero;
            BoneWeights2[index] = Vector4.Zero;
        }

        /// <summary>
        /// Prepare the <see cref="Renderable"/> builder's arrays to be filled with values.
        /// Does not generate tangents.
        /// </summary>
        /// <param name="vertexCount">How many vertices.</param>
        /// <param name="indexCount">How many indices.</param>
        public void Prepare(int vertexCount, int indexCount)
        {
            Vertices = new Vector3[vertexCount];
            Indices = new uint[indexCount];
            Normals = new Vector3[vertexCount];
            TexCoords = new Vector3[vertexCount];
            Colors = new Vector4[vertexCount];
            BoneIDs = new Vector4[vertexCount];
            BoneWeights = new Vector4[vertexCount];
            BoneIDs2 = new Vector4[vertexCount];
            BoneWeights2 = new Vector4[vertexCount];
        }

        /// <summary>
        /// Prepare the <see cref="Renderable"/> builder's arrays to be filled with values for 2D usage.
        /// Does not generate bones or tangents.
        /// </summary>
        /// <param name="vertexCount">How many vertices.</param>
        /// <param name="indexCount">How many indices.</param>
        public void Prepare2D(int vertexCount, int indexCount)
        {
            Vertices = new Vector3[vertexCount];
            Indices = new uint[indexCount];
            Normals = new Vector3[vertexCount];
            TexCoords = new Vector3[vertexCount];
            Colors = new Vector4[vertexCount];
        }
    }

    /// <summary>Gets a somewhat accurate amount of vRAM usage, if any.</summary>
    /// <returns>The vRAM usage.</returns>
    public long GetVRAMUsage()
    {
        if (Generated)
        {
            return Internal.LastVRAM;
        }
        return 0;
    }

    /// <summary>Whether the VBO has been generated on the GPU.</summary>
    public bool Generated = false;

    /// <summary>Destroys GPU-side data.</summary>
    public void Destroy()
    {
        if (Generated)
        {
            GraphicsUtil.DeleteVertexArray(Internal.VAO);
            Internal.VertexVBO.Dispose();
            Internal.IndexVBO.Dispose();
            Internal.NormalVBO.Dispose();
            Internal.TexCoordVBO.Dispose();
            Internal.TangentVBO.Dispose();
            if (Internal.HasColors)
            {
                Internal.ColorVBO.Dispose();
                Internal.HasColors = false;
            }
            if (Internal.HasBones)
            {
                Internal.BoneIDVBO.Dispose();
                Internal.BoneWeightVBO.Dispose();
                Internal.BoneID2VBO.Dispose();
                Internal.BoneWeight2VBO.Dispose();
                Internal.HasBones = false;
            }
            Generated = false;
        }
    }

    /// <summary>Gets the normal tangents for a VBO setup.</summary>
    /// <param name="vecs">The vertices.</param>
    /// <param name="norms">The normals.</param>
    /// <param name="texs">The texture coordinates.</param>
    /// <returns>The tangent set.</returns>
    public static Vector3[] TangentsFor(Vector3[] vecs, Vector3[] norms, Vector3[] texs)
    {
        Vector3[] tangs = new Vector3[vecs.Length];
        if (vecs.Length != norms.Length || texs.Length != vecs.Length || (vecs.Length % 3) != 0)
        {
            for (int i = 0; i < tangs.Length; i++)
            {
                tangs[i] = new Vector3(0, 0, 0);
            }
            return tangs;
        }
        for (int i = 0; i < vecs.Length; i += 3)
        {
            Vector3 v1 = vecs[i];
            Vector3 dv1 = vecs[i + 1] - v1;
            Vector3 dv2 = vecs[i + 2] - v1;
            Vector3 t1 = texs[i];
            Vector3 dt1 = texs[i + 1] - t1;
            Vector3 dt2 = texs[i + 2] - t1;
            Vector3 tangent = (dv1 * dt2.Y - dv2 * dt1.Y) / (dt1.X * dt2.Y - dt1.Y * dt2.X);
            Vector3 normal = norms[i];
            tangent = (tangent - normal * Vector3.Dot(normal, tangent)).Normalized(); // TODO: Necessity of this correction?
            tangs[i] = tangent;
            tangs[i + 1] = tangent;
            tangs[i + 2] = tangent;
        }
        return tangs;
    }

    /// <summary>Gets the normal tangents for a VBO setup.</summary>
    /// <param name="vecs">The vertices.</param>
    /// <param name="norms">The normals.</param>
    /// <param name="texs">The texture coordinates.</param>
    /// <returns>The tangent set.</returns>
    public static Vector3[] TangentsFor(Vector3[] vecs, Vector3[] norms, Vector2[] texs)
    {
        Vector3[] tangs = new Vector3[vecs.Length];
        if (vecs.Length != norms.Length || texs.Length != vecs.Length || (vecs.Length % 3) != 0)
        {
            for (int i = 0; i < tangs.Length; i++)
            {
                tangs[i] = new Vector3(0, 0, 0);
            }
            return tangs;
        }
        for (int i = 0; i < vecs.Length; i += 3)
        {
            Vector3 v1 = vecs[i];
            Vector3 dv1 = vecs[i + 1] - v1;
            Vector3 dv2 = vecs[i + 2] - v1;
            Vector2 t1 = texs[i];
            Vector2 dt1 = texs[i + 1] - t1;
            Vector2 dt2 = texs[i + 2] - t1;
            Vector3 tangent = (dv1 * dt2.Y - dv2 * dt1.Y) / (dt1.X * dt2.Y - dt1.Y * dt2.X);
            Vector3 normal = norms[i];
            tangent = (tangent - normal * Vector3.Dot(normal, tangent)).Normalized(); // TODO: Necessity of this correction?
            tangs[i] = tangent;
            tangs[i + 1] = tangent;
            tangs[i + 2] = tangent;
        }
        return tangs;
    }

    /// <summary>Generate this <see cref="Renderable"/> to the GPU.</summary>
    /// <param name="builder">The builder to use.</param>
    public void GenerateVBO(Builder builder)
    {
        if (Generated)
        {
            Destroy();
        }
        GL.BindVertexArray(0);
        uint[] inds;
        Vector3[] vecs;
        Vector3[] norms;
        Vector3[] texs;
        Vector3[] tangs;
        Vector4[] cols;
        Vector4[] ids = null;
        Vector4[] weights = null;
        Vector4[] ids2 = null;
        Vector4[] weights2 = null;
        if (builder is ListBuilder buildList)
        {
            if (buildList.Vertices is null)
            {
                Logs.CriticalError("Failed to generate VBO, null vertices!");
                return;
            }
            vecs = [.. buildList.Vertices];
            inds = [.. buildList.Indices];
            norms = [.. buildList.Normals];
            texs = [.. buildList.TexCoords];
            tangs = buildList.Tangents != null ? [.. buildList.Tangents] : TangentsFor(vecs, norms, texs);
            cols = buildList.Colors?.ToArray();
            if (buildList.BoneIDs != null)
            {
                Internal.HasBones = true;
                ids = [.. buildList.BoneIDs];
                weights = [.. buildList.BoneWeights];
                ids2 = [.. buildList.BoneIDs2];
                weights2 = [.. buildList.BoneWeights2];
            }
        }
        else if (builder is ArrayBuilder buildArray)
        {
            if (buildArray.Vertices is null)
            {
                Logs.CriticalError("Failed to generate VBO, null vertices!");
                return;
            }
            vecs = buildArray.Vertices;
            inds = buildArray.Indices;
            norms = buildArray.Normals;
            texs = buildArray.TexCoords;
            tangs = buildArray.Tangents ?? TangentsFor(vecs, norms, texs);
            cols = buildArray.Colors;
            if (buildArray.BoneIDs is not null)
            {
                Internal.HasBones = true;
                ids = buildArray.BoneIDs;
                weights = buildArray.BoneWeights;
                ids2 = buildArray.BoneIDs2;
                weights2 = buildArray.BoneWeights2;
            }
        }
        else
        {
            Logs.CriticalError($"Failed to generate VBO, unknown builder type '{builder}'!");
            return;
        }
        if (vecs.Length == 0)
        {
            Logs.CriticalError("Failed to generate VBO, empty vertices!");
            return;
        }
#if DEBUG
        if (vecs.Length != norms.Length || vecs.Length != texs.Length || vecs.Length != tangs.Length)
        {
            Logs.CriticalError("Failed to generate VBO, main vertex attribute counts not aligned!");
            return;
        }
        if (cols != null && vecs.Length != cols.Length)
        {
            Logs.CriticalError("Failed to generate VBO, vertex color attribute count not aligned!");
            return;
        }
        if (Internal.HasBones && (vecs.Length != ids.Length || vecs.Length != weights.Length || vecs.Length != ids2.Length || vecs.Length != weights2.Length))
        {
            Logs.CriticalError("Failed to generate VBO, vertex bone attribute counts not aligned!");
            return;
        }
#endif
        Internal.IndexCount = inds.Length;
        Internal.LastVRAM = 0;
        // Vertex buffer
        Internal.VertexVBO = new("Renderable_VertexVBO", BufferTarget.ArrayBuffer);
        GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(vecs.Length * Vector3.SizeInBytes), vecs, Internal.BufferMode);
        Internal.LastVRAM += vecs.Length * Vector3.SizeInBytes;
        // Normal buffer
        Internal.NormalVBO = new("Renderable_NormalVBO", BufferTarget.ArrayBuffer);
        GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(norms.Length * Vector3.SizeInBytes), norms, Internal.BufferMode);
        Internal.LastVRAM += norms.Length * Vector3.SizeInBytes;
        // TexCoord buffer
        Internal.TexCoordVBO = new("Renderable_TexCoordVBO", BufferTarget.ArrayBuffer);
        GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(texs.Length * Vector3.SizeInBytes), texs, Internal.BufferMode);
        Internal.LastVRAM += texs.Length * Vector3.SizeInBytes;
        Internal.TangentVBO = new("Renderable_TangentVBO", BufferTarget.ArrayBuffer);
        GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(tangs.Length * Vector3.SizeInBytes), tangs, Internal.BufferMode);
        Internal.LastVRAM += tangs.Length * Vector3.SizeInBytes;
        // Color buffer
        if (cols != null)
        {
            Internal.HasColors = true;
            Internal.ColorVBO = new("Renderable_ColorVBO", BufferTarget.ArrayBuffer);
            GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(cols.Length * Vector4.SizeInBytes), cols, Internal.BufferMode);
            Internal.LastVRAM += cols.Length * Vector4.SizeInBytes;
        }
        if (Internal.HasBones)
        {
            // Weight buffer
            Internal.BoneWeightVBO = new("Renderable_BoneWeightVBO", BufferTarget.ArrayBuffer);
            GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(weights.Length * Vector4.SizeInBytes), weights, Internal.BufferMode);
            Internal.LastVRAM += weights.Length * Vector4.SizeInBytes;
            // ID buffer
            Internal.BoneIDVBO = new("Renderable_BoneIDVBO", BufferTarget.ArrayBuffer);
            GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(ids.Length * Vector4.SizeInBytes), ids, Internal.BufferMode);
            Internal.LastVRAM += ids.Length * Vector4.SizeInBytes;
            // Weight2 buffer
            Internal.BoneWeight2VBO = new("Renderable_BoneWeight2VBO", BufferTarget.ArrayBuffer);
            GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(weights2.Length * Vector4.SizeInBytes), weights2, Internal.BufferMode);
            Internal.LastVRAM += weights2.Length * Vector4.SizeInBytes;
            // ID2 buffer
            Internal.BoneID2VBO = new("Renderable_BoneID2VBO", BufferTarget.ArrayBuffer);
            GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(ids2.Length * Vector4.SizeInBytes), ids2, Internal.BufferMode);
            Internal.LastVRAM += ids2.Length * Vector4.SizeInBytes;
        }
        GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
        // Index buffer
        Internal.IndexVBO = new("Renderable_IndexVBO", BufferTarget.ElementArrayBuffer);
        GL.BufferData(BufferTarget.ElementArrayBuffer, (IntPtr)(inds.Length * sizeof(uint)), inds, Internal.BufferMode);
        Internal.LastVRAM += inds.Length * sizeof(uint);
        GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);
        // VAO
        Internal.VAO = GraphicsUtil.GenVertexArray("Renderable_VAO");
        Internal.VertexVBO.Bind();
        GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 0, 0);
        Internal.NormalVBO.Bind();
        GL.VertexAttribPointer(1, 3, VertexAttribPointerType.Float, false, 0, 0);
        Internal.TexCoordVBO.Bind();
        GL.VertexAttribPointer(2, 3, VertexAttribPointerType.Float, false, 0, 0);
        Internal.TangentVBO.Bind();
        GL.VertexAttribPointer(3, 3, VertexAttribPointerType.Float, false, 0, 0);
        if (Internal.HasColors)
        {
            Internal.ColorVBO.Bind();
            GL.VertexAttribPointer(4, 4, VertexAttribPointerType.Float, false, 0, 0);
        }
        if (Internal.HasBones)
        {
            Internal.BoneWeightVBO.Bind();
            GL.VertexAttribPointer(5, 4, VertexAttribPointerType.Float, false, 0, 0);
            Internal.BoneIDVBO.Bind();
            GL.VertexAttribPointer(6, 4, VertexAttribPointerType.Float, false, 0, 0);
            Internal.BoneWeight2VBO.Bind();
            GL.VertexAttribPointer(7, 4, VertexAttribPointerType.Float, false, 0, 0);
            Internal.BoneID2VBO.Bind();
            GL.VertexAttribPointer(8, 4, VertexAttribPointerType.Float, false, 0, 0);
        }
        GL.EnableVertexAttribArray(0);
        GL.EnableVertexAttribArray(1);
        GL.EnableVertexAttribArray(2);
        GL.EnableVertexAttribArray(3);
        if (cols != null)
        {
            GL.EnableVertexAttribArray(4);
        }
        if (Internal.HasBones)
        {
            GL.EnableVertexAttribArray(5);
            GL.EnableVertexAttribArray(6);
            GL.EnableVertexAttribArray(7);
            GL.EnableVertexAttribArray(8);
        }
        Internal.IndexVBO.Bind();
        // Clean up
        GL.BindVertexArray(0);
        Generated = true;

    }

    /// <summary>Renders the VBO fully, without handling textures at all.</summary>
    /// <param name="context">The sourcing render context.</param>
    public void RenderWithoutTextures(RenderContext context)
    {
        if (!Generated)
        {
            return;
        }
        if (context is not null)
        {
            context.ObjectsRendered++;
            context.VerticesRendered += Internal.IndexCount;
        }
        GL.BindVertexArray(Internal.VAO);
        GL.DrawElements(PrimitiveType.Triangles, Internal.IndexCount, DrawElementsType.UnsignedInt, IntPtr.Zero);
        GL.BindVertexArray(0);
        GraphicsUtil.CheckError("Renderable - Render - VAO", this);
    }

    /// <summary>Render the VBO fully.</summary>
    /// <param name="context">The sourcing render context.</param>
    /// <param name="fixafter">Whether to fix textures after rendering (if textures are enabled).</param>
    public void Render(RenderContext context, bool fixafter)
    {
        GraphicsUtil.CheckError("Renderable - Render - Pre", this);
        if (ColorTexture is not null)
        {
            GL.ActiveTexture(TextureUnit.Texture3);
            if (ReflectivityTexture is not null)
            {
                ReflectivityTexture.Bind();
            }
            else
            {
                ColorTexture.Engine.Black.Bind();
            }
            GL.ActiveTexture(TextureUnit.Texture2);
            if (SpecularTexture is not null)
            {
                SpecularTexture.Bind();
            }
            else
            {
                ColorTexture.Engine.Black.Bind();
            }
            GL.ActiveTexture(TextureUnit.Texture1);
            if (NormalTexture is not null)
            {
                NormalTexture.Bind();
            }
            else
            {
                ColorTexture.Engine.NormalDef.Bind();
            }
            GL.ActiveTexture(TextureUnit.Texture0);
            ColorTexture.Bind();
            GraphicsUtil.CheckError("Renderable - Render - Textures", this);
        }
        RenderWithoutTextures(context);
        if (fixafter && ColorTexture is not null)
        {
            GL.ActiveTexture(TextureUnit.Texture3);
            ColorTexture.Engine.Black.Bind();
            GL.ActiveTexture(TextureUnit.Texture2);
            ColorTexture.Engine.Black.Bind();
            GL.ActiveTexture(TextureUnit.Texture1);
            ColorTexture.Engine.NormalDef.Bind();
            GL.ActiveTexture(TextureUnit.Texture0);
            ColorTexture.Engine.White.Bind();
        }
        GraphicsUtil.CheckError("Renderable - Render - Post", this);
    }
}

/// <summary>Represents a set of texture coordinates.</summary>
public class TextureCoordinates
{
    /// <summary>Construct a basic set of texture coordinates.</summary>
    public TextureCoordinates()
    {
        XScale = 1;
        YScale = 1;
        XShift = 0;
        YShift = 0;
        XFlip = false;
        YFlip = false;
    }

    /// <summary>The X-Scale.</summary>
    public float XScale;

    /// <summary>The Y-Scale.</summary>
    public float YScale;

    /// <summary>The X-Shift.</summary>
    public float XShift;

    /// <summary>The Y-Shift.</summary>
    public float YShift;

    /// <summary>The X-flip option.</summary>
    public bool XFlip;

    /// <summary>The Y-flip option.</summary>
    public bool YFlip;

    /// <summary>Gets a quick string form of the Texture Coordinates.</summary>
    /// <returns>The string form.</returns>
    public override string ToString()
    {
        return XScale + "/" + YScale + "/" + XShift + "/" + YShift + "/" + (XFlip ? "t" : "f") + "/" + (YFlip ? "t" : "f");
    }

    /// <summary>Converts a quick string of a Texture Coordinate set to an actual TextureCoordinates.</summary>
    /// <param name="str">The string.</param>
    /// <returns>The texture coordinates.</returns>
    public static TextureCoordinates FromString(string str)
    {
        TextureCoordinates tc = new();
        string[] data = str.SplitFast('/');
        tc.XScale = StringConversionHelper.StringToFloat(data[0]);
        tc.YScale = StringConversionHelper.StringToFloat(data[1]);
        tc.XShift = StringConversionHelper.StringToFloat(data[2]);
        tc.YShift = StringConversionHelper.StringToFloat(data[3]);
        tc.XFlip = data[4] == "t";
        tc.YFlip = data[5] == "t";
        return tc;
    }
}
