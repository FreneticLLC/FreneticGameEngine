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
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;
using FreneticGameCore;
using FreneticGameCore.CoreSystems;
using FreneticGameCore.MathHelpers;
using FreneticUtilities.FreneticExtensions;
using FreneticGameCore.UtilitySystems;
using FreneticUtilities.FreneticToolkit;

namespace FreneticGameGraphics.GraphicsHelpers
{
    /// <summary>
    /// Represents a Vertex Buffer/Array Object set.
    /// </summary>
    public class VBO
    {
        uint _VertexVBO;
        uint _IndexVBO;
        uint _NormalVBO;
        uint _TexCoordVBO;
        uint _ColorVBO;
        uint _BoneIDVBO;
        uint _BoneWeightVBO;
        uint _BoneID2VBO;
        uint _BoneWeight2VBO;
        uint _TangentVBO;

        /// <summary>
        /// The internal main Vertex Array Object.
        /// </summary>
        public int _VAO = -1;

        /// <summary>
        /// The primary texture.
        /// </summary>
        public Texture Tex;
        
        /// <summary>
        /// The specular texture.
        /// </summary>
        public Texture Tex_Specular;
        
        /// <summary>
        /// The reflectivity texture.
        /// </summary>
        public Texture Tex_Reflectivity;

        /// <summary>
        /// The normal texture.
        /// </summary>
        public Texture Tex_Normal;

        /// <summary>
        /// The position vertices.
        /// </summary>
        public List<Vector3> Vertices;

        /// <summary>
        /// The indices.
        /// </summary>
        public List<uint> Indices;

        /// <summary>
        /// The normals.
        /// </summary>
        public List<Vector3> Normals;
        
        /// <summary>
        /// The normal tangents.
        /// </summary>
        public List<Vector3> Tangents;

        /// <summary>
        /// The texture coordinates.
        /// </summary>
        public List<Vector3> TexCoords;

        /// <summary>
        /// The colors.
        /// </summary>
        public List<Vector4> Colors;

        /// <summary>
        /// The bone weight IDs.
        /// </summary>
        public List<Vector4> BoneIDs;

        /// <summary>
        /// The bone weight levels.
        /// </summary>
        public List<Vector4> BoneWeights;

        /// <summary>
        /// The second bone weight IDs.
        /// </summary>
        public List<Vector4> BoneIDs2;

        /// <summary>
        /// The second bone weight levels.
        /// </summary>
        public List<Vector4> BoneWeights2;

        /// <summary>
        /// How much VRAM this VBO would consume at last generation.
        /// </summary>
        public long LastVRAM = 0;

        /// <summary>
        /// Gets a somewhat accurate amount of VRAM used, if any.
        /// </summary>
        /// <returns>The vRAM usage.</returns>
        public long GetVRAMUsage()
        {
            if (generated)
            {
                return LastVRAM;
            }
            return 0;
        }

        /// <summary>
        /// Cleans away CPU-side data.
        /// </summary>
        public void CleanLists()
        {
            Vertices = null;
            Indices = null;
            Normals = null;
            Tangents = null;
            TexCoords = null;
            Colors = null;
            BoneIDs = null;
            BoneWeights = null;
            BoneIDs2 = null;
            BoneWeights2 = null;
            verts = null;
            indices = null;
            normals = null;
            tangents = null;
            texts = null;
            v4_colors = null;
            pre_boneids = null;
            pre_boneids2 = null;
            pre_boneweights = null;
            pre_boneweights2 = null;
        }

        /// <summary>
        /// The vertex count.
        /// </summary>
        public int vC;

        /// <summary>
        /// Helper to add an axis-aligned side to this VBO.
        /// </summary>
        /// <param name="normal">The normal.</param>
        /// <param name="tc">The texture coordinates.</param>
        /// <param name="offs">Whether to offset.</param>
        /// <param name="texf">The texture ID.</param>
        public void AddSide(Location normal, TextureCoordinates tc, bool offs = false, float texf = 0)
        {
            // TODO: IMPROVE! Or discard?!
            for (int i = 0; i < 6; i++)
            {
                Normals.Add(normal.ToOpenTK());
                Colors.Add(new Vector4(1f, 1f, 1f, 1f));
                Indices.Add((uint)Indices.Count);
                BoneIDs.Add(new Vector4(0, 0, 0, 0));
                BoneWeights.Add(new Vector4(0f, 0f, 0f, 0f));
                BoneIDs2.Add(new Vector4(0, 0, 0, 0));
                BoneWeights2.Add(new Vector4(0f, 0f, 0f, 0f));
            }
            float aX = (tc.xflip ? 1 : 0) + tc.xshift;
            float aY = (tc.yflip ? 1 : 0) + tc.yshift;
            float bX = (tc.xflip ? 0 : 1) * tc.xscale + tc.xshift;
            float bY = (tc.yflip ? 1 : 0) + tc.yshift;
            float cX = (tc.xflip ? 0 : 1) * tc.xscale + tc.xshift;
            float cY = (tc.yflip ? 0 : 1) * tc.yscale + tc.yshift;
            float dX = (tc.xflip ? 1 : 0) + tc.xshift;
            float dY = (tc.yflip ? 0 : 1) * tc.yscale + tc.yshift;
            float zero = offs ? -0.5f : -1; // Sssh
            float one = offs ? 0.5f : 1;
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
        /// Prepare the VBO's lists to be added to.
        /// </summary>
        public void Prepare()
        {
            Vertices = new List<Vector3>();
            Indices = new List<uint>();
            Normals = new List<Vector3>();
            TexCoords = new List<Vector3>();
            Colors = new List<Vector4>();
            BoneIDs = new List<Vector4>();
            BoneWeights = new List<Vector4>();
            BoneIDs2 = new List<Vector4>();
            BoneWeights2 = new List<Vector4>();
        }

        /// <summary>
        /// Whether the VBO has been generated on the GPU.
        /// </summary>
        public bool generated = false;

        /// <summary>
        /// Destroys GPU-side data.
        /// </summary>
        public void Destroy()
        {
            if (generated)
            {
                GL.DeleteVertexArray(_VAO);
                GL.DeleteBuffer(_VertexVBO);
                GL.DeleteBuffer(_IndexVBO);
                GL.DeleteBuffer(_NormalVBO);
                GL.DeleteBuffer(_TexCoordVBO);
                GL.DeleteBuffer(_TangentVBO);
                if (colors)
                {
                    GL.DeleteBuffer(_ColorVBO);
                    colors = false;
                }
                if (bones)
                {
                    GL.DeleteBuffer(_BoneIDVBO);
                    GL.DeleteBuffer(_BoneWeightVBO);
                    GL.DeleteBuffer(_BoneID2VBO);
                    GL.DeleteBuffer(_BoneWeight2VBO);
                    bones = false;
                }
                generated = false;
            }
        }

        bool colors;
        bool bones;

        /// <summary>
        /// The vertices.
        /// </summary>
        public Vector3[] verts = null;

        /// <summary>
        /// The indices.
        /// </summary>
        public uint[] indices = null;

        /// <summary>
        /// The normals.
        /// </summary>
        public Vector3[] normals = null;

        /// <summary>
        /// The texture coordinates.
        /// </summary>
        public Vector3[] texts = null;

        /// <summary>
        /// The tangents.
        /// </summary>
        public Vector3[] tangents = null;

        /// <summary>
        /// The colors.
        /// </summary>
        public Vector4[] v4_colors = null;

        /// <summary>
        /// What buffer mode to use.
        /// </summary>
        public BufferUsageHint BufferMode = BufferUsageHint.StaticDraw;

        /// <summary>
        /// Gets the normal tangents for a VBO setup.
        /// </summary>
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

        /// <summary>
        /// Gets the normal tangents for a VBO setup.
        /// </summary>
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

        /// <summary>
        /// Bone IDs.
        /// </summary>
        public Vector4[] pre_boneids = null;

        /// <summary>
        /// Bone weights.
        /// </summary>
        public Vector4[] pre_boneweights = null;


        /// <summary>
        /// Bone IDs, set 2.
        /// </summary>
        public Vector4[] pre_boneids2 = null;

        /// <summary>
        /// Bone weights, set 2.
        /// </summary>
        public Vector4[] pre_boneweights2 = null;

        /// <summary>
        /// Generate the VBO to the GPU.
        /// </summary>
        public void GenerateVBO()
        {
            if (generated)
            {
                Destroy();
            }
            GL.BindVertexArray(0);
            if (Vertices == null && verts == null)
            {
                SysConsole.Output(OutputType.ERROR, "Failed to render VBO, null vertices!");
                return;
            }
            Vector3[] vecs = verts ?? Vertices.ToArray();
            if (vecs.Length == 0)
            {
                return;
            }
            LastVRAM = 0;
            uint[] inds = indices ?? Indices.ToArray();
            Vector3[] norms = normals ?? Normals.ToArray();
            Vector3[] texs = texts ?? TexCoords.ToArray();
            Vector3[] tangs = Tangents != null ? Tangents.ToArray() : (tangents ?? TangentsFor(vecs, norms, texs));
            Vector4[] cols = Colors != null ? Colors.ToArray() : v4_colors;
            vC = inds.Length;
            Vector4[] ids = null;
            Vector4[] weights = null;
            Vector4[] ids2 = null;
            Vector4[] weights2 = null;
            if (BoneIDs != null || pre_boneids != null)
            {
                bones = true;
                ids = BoneIDs != null ? pre_boneids : BoneIDs.ToArray();
                weights = BoneWeights != null ? pre_boneweights : BoneWeights.ToArray();
                ids2 = BoneIDs2 != null ? pre_boneids2 : BoneIDs2.ToArray();
                weights2 = BoneWeights2 != null ? pre_boneweights2 : BoneWeights2.ToArray();
            }
            // Vertex buffer
            GL.GenBuffers(1, out _VertexVBO);
            GL.BindBuffer(BufferTarget.ArrayBuffer, _VertexVBO);
            GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(vecs.Length * Vector3.SizeInBytes), vecs, BufferMode);
            LastVRAM += vecs.Length * Vector3.SizeInBytes;
            // Normal buffer
            GL.GenBuffers(1, out _NormalVBO);
            GL.BindBuffer(BufferTarget.ArrayBuffer, _NormalVBO);
            GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(norms.Length * Vector3.SizeInBytes), norms, BufferMode);
            LastVRAM += norms.Length * Vector3.SizeInBytes;
            // TexCoord buffer
            GL.GenBuffers(1, out _TexCoordVBO);
            GL.BindBuffer(BufferTarget.ArrayBuffer, _TexCoordVBO);
            GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(texs.Length * Vector3.SizeInBytes), texs, BufferMode);
            LastVRAM += texs.Length * Vector3.SizeInBytes;
            GL.GenBuffers(1, out _TangentVBO);
            GL.BindBuffer(BufferTarget.ArrayBuffer, _TangentVBO);
            GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(tangs.Length * Vector3.SizeInBytes), tangs, BufferMode);
            LastVRAM += tangs.Length * Vector3.SizeInBytes;
            // Color buffer
            if (cols != null)
            {
                colors = true;
                GL.GenBuffers(1, out _ColorVBO);
                GL.BindBuffer(BufferTarget.ArrayBuffer, _ColorVBO);
                GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(cols.Length * Vector4.SizeInBytes), cols, BufferMode);
                LastVRAM += cols.Length * Vector4.SizeInBytes;
            }
            // Weight buffer
            if (weights != null)
            {
                GL.GenBuffers(1, out _BoneWeightVBO);
                GL.BindBuffer(BufferTarget.ArrayBuffer, _BoneWeightVBO);
                GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(weights.Length * Vector4.SizeInBytes), weights, BufferMode);
                LastVRAM += weights.Length * Vector4.SizeInBytes;
            }
            // ID buffer
            if (ids != null)
            {
                GL.GenBuffers(1, out _BoneIDVBO);
                GL.BindBuffer(BufferTarget.ArrayBuffer, _BoneIDVBO);
                GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(ids.Length * Vector4.SizeInBytes), ids, BufferMode);
                LastVRAM += ids.Length * Vector4.SizeInBytes;
            }
            // Weight2 buffer
            if (weights2 != null)
            {
                GL.GenBuffers(1, out _BoneWeight2VBO);
                GL.BindBuffer(BufferTarget.ArrayBuffer, _BoneWeight2VBO);
                GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(weights2.Length * Vector4.SizeInBytes), weights2, BufferMode);
                LastVRAM += weights2.Length * Vector4.SizeInBytes;
            }
            // ID2 buffer
            if (ids2 != null)
            {
                GL.GenBuffers(1, out _BoneID2VBO);
                GL.BindBuffer(BufferTarget.ArrayBuffer, _BoneID2VBO);
                GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(ids2.Length * Vector4.SizeInBytes), ids2, BufferMode);
                LastVRAM += ids2.Length * Vector4.SizeInBytes;
            }
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            // Index buffer
            GL.GenBuffers(1, out _IndexVBO);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, _IndexVBO);
            GL.BufferData(BufferTarget.ElementArrayBuffer, (IntPtr)(inds.Length * sizeof(uint)), inds, BufferMode);
            LastVRAM += inds.Length * sizeof(uint);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);
            // VAO
            GL.GenVertexArrays(1, out _VAO);
            GL.BindVertexArray(_VAO);
            GL.BindBuffer(BufferTarget.ArrayBuffer, _VertexVBO);
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 0, 0);
            GL.BindBuffer(BufferTarget.ArrayBuffer, _NormalVBO);
            GL.VertexAttribPointer(1, 3, VertexAttribPointerType.Float, false, 0, 0);
            GL.BindBuffer(BufferTarget.ArrayBuffer, _TexCoordVBO);
            GL.VertexAttribPointer(2, 3, VertexAttribPointerType.Float, false, 0, 0);
            GL.BindBuffer(BufferTarget.ArrayBuffer, _TangentVBO);
            GL.VertexAttribPointer(3, 3, VertexAttribPointerType.Float, false, 0, 0);
            if (cols != null)
            {
                GL.BindBuffer(BufferTarget.ArrayBuffer, _ColorVBO);
                GL.VertexAttribPointer(4, 4, VertexAttribPointerType.Float, false, 0, 0);
            }
            if (weights != null)
            {
                GL.BindBuffer(BufferTarget.ArrayBuffer, _BoneWeightVBO);
                GL.VertexAttribPointer(5, 4, VertexAttribPointerType.Float, false, 0, 0);
            }
            if (ids != null)
            {
                GL.BindBuffer(BufferTarget.ArrayBuffer, _BoneIDVBO);
                GL.VertexAttribPointer(6, 4, VertexAttribPointerType.Float, false, 0, 0);
            }
            if (weights2 != null)
            {
                GL.BindBuffer(BufferTarget.ArrayBuffer, _BoneWeight2VBO);
                GL.VertexAttribPointer(7, 4, VertexAttribPointerType.Float, false, 0, 0);
            }
            if (ids2 != null)
            {
                GL.BindBuffer(BufferTarget.ArrayBuffer, _BoneID2VBO);
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
            if (weights != null)
            {
                GL.EnableVertexAttribArray(5);
            }
            if (ids != null)
            {
                GL.EnableVertexAttribArray(6);
            }
            if (weights2 != null)
            {
                GL.EnableVertexAttribArray(7);
            }
            if (ids2 != null)
            {
                GL.EnableVertexAttribArray(8);
            }
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, _IndexVBO);
            // Clean up
            GL.BindVertexArray(0);
            generated = true;

        }

        /// <summary>
        /// Renders the VBO fully, without handling textures at all.
        /// </summary>
        public void RenderWithoutTextures()
        {
            GL.BindVertexArray(_VAO);
            GL.DrawElements(PrimitiveType.Triangles, vC, DrawElementsType.UnsignedInt, IntPtr.Zero);
            GL.BindVertexArray(0);
        }

        /// <summary>
        /// Render the VBO fully.
        /// </summary>
        /// <param name="fixafter">Whether to fix textures after rendering (if textures are enabled).</param>
        public void Render(bool fixafter)
        {
            if (!generated)
            {
                return;
            }
            if (Tex != null)
            {
                GL.ActiveTexture(TextureUnit.Texture3);
                if (Tex_Reflectivity != null)
                {
                    Tex_Reflectivity.Bind();
                }
                else
                {
                    Tex.Engine.Black.Bind();
                }
                GL.ActiveTexture(TextureUnit.Texture2);
                if (Tex_Specular != null)
                {
                    Tex_Specular.Bind();
                }
                else
                {
                    Tex.Engine.Black.Bind();
                }
                GL.ActiveTexture(TextureUnit.Texture1);
                if (Tex_Normal != null)
                {
                    Tex_Normal.Bind();
                }
                else
                {
                    Tex.Engine.NormalDef.Bind();
                }
                GL.ActiveTexture(TextureUnit.Texture0);
                Tex.Bind();
            }
            RenderWithoutTextures();
            if (fixafter && Tex != null)
            {
                GL.ActiveTexture(TextureUnit.Texture3);
                Tex.Engine.Black.Bind();
                GL.ActiveTexture(TextureUnit.Texture2);
                Tex.Engine.Black.Bind();
                GL.ActiveTexture(TextureUnit.Texture1);
                Tex.Engine.NormalDef.Bind();
                GL.ActiveTexture(TextureUnit.Texture0);
                Tex.Engine.White.Bind();
            }
        }
    }

    /// <summary>
    /// Represents a set of texture coordinates.
    /// </summary>
    public class TextureCoordinates
    {
        /// <summary>
        /// Construct a basic set of texture coordinates.
        /// </summary>
        public TextureCoordinates()
        {
            xscale = 1;
            yscale = 1;
            xshift = 0;
            yshift = 0;
            xflip = false;
            yflip = false;
        }

        /// <summary>
        /// The X-Scale.
        /// </summary>
        public float xscale;

        /// <summary>
        /// The Y-Scale.
        /// </summary>
        public float yscale;

        /// <summary>
        /// The X-Shift.
        /// </summary>
        public float xshift;

        /// <summary>
        /// The Y-Shift.
        /// </summary>
        public float yshift;

        /// <summary>
        /// The X-flip option.
        /// </summary>
        public bool xflip;

        /// <summary>
        /// The Y-flip option.
        /// </summary>
        public bool yflip;

        /// <summary>
        /// Gets a quick string form of the Texture Coordinates.
        /// </summary>
        /// <returns>The string form.</returns>
        public override string ToString()
        {
            return xscale + "/" + yscale + "/" + xshift + "/" + yshift + "/" + (xflip ? "t" : "f") + "/" + (yflip ? "t" : "f");
        }

        /// <summary>
        /// Converts a quick string of a Texture Coordinate set to an actual TextureCoordinates.
        /// </summary>
        /// <param name="str">The string.</param>
        /// <returns>The texture coordinates.</returns>
        public static TextureCoordinates FromString(string str)
        {
            TextureCoordinates tc = new TextureCoordinates();
            string[] data = str.SplitFast('/');
            tc.xscale = (float)StringConversionHelper.StringToFloat(data[0]);
            tc.yscale = (float)StringConversionHelper.StringToFloat(data[1]);
            tc.xshift = (float)StringConversionHelper.StringToFloat(data[2]);
            tc.yshift = (float)StringConversionHelper.StringToFloat(data[3]);
            tc.xflip = data[4] == "t";
            tc.yflip = data[5] == "t";
            return tc;
        }
    }
}
