using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BEPUutilities;
using BEPUphysics.CollisionShapes;
using BEPUphysics.CollisionShapes.ConvexShapes;
using FreneticGameCore.Files;

namespace FreneticGameCore
{
    /// <summary>
    /// Handles abstract 3D models. Can be purposed for both collision systems and rendering.
    /// </summary>
    public class ModelHandler
    {
        /// <summary>
        /// Loads a model from .FMD (Frenetic Model Data) input.
        /// </summary>
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
            dat_filt = FileHandler.UnGZip(dat_filt);
            DataStream ds = new DataStream(dat_filt);
            DataReader dr = new DataReader(ds);
            Model3D mod = new Model3D();
            Matrix matA = ReadMat(dr);
            mod.MatrixA = matA;
            int meshCount = dr.ReadInt();
            mod.Meshes = new List<Model3DMesh>(meshCount);
            for (int m = 0; m < meshCount; m++)
            {
                Model3DMesh mesh = new Model3DMesh();
                mod.Meshes.Add(mesh);
                mesh.Name = dr.ReadFullString();
                int vertexCount = dr.ReadInt();
                mesh.Vertices = new List<Vector3>(vertexCount);
                for (int v = 0; v < vertexCount; v++)
                {
                    double f1 = dr.ReadFloat();
                    double f2 = dr.ReadFloat();
                    double f3 = dr.ReadFloat();
                    mesh.Vertices.Add(new Vector3(f1, f2, f3));
                }
                int indiceCount = dr.ReadInt() * 3;
                mesh.Indices = new List<int>(indiceCount);
                for (int i = 0; i < indiceCount; i++)
                {
                    mesh.Indices.Add(dr.ReadInt());
                }
                int tcCount = dr.ReadInt();
                mesh.TexCoords = new List<Vector2>(tcCount);
                for (int t = 0; t < tcCount; t++)
                {
                    double f1 = dr.ReadFloat();
                    double f2 = dr.ReadFloat();
                    mesh.TexCoords.Add(new Vector2(f1, f2));
                }
                int normCount = dr.ReadInt();
                mesh.Normals = new List<Vector3>(normCount);
                for (int n = 0; n < normCount; n++)
                {
                    double f1 = dr.ReadFloat();
                    double f2 = dr.ReadFloat();
                    double f3 = dr.ReadFloat();
                    mesh.Normals.Add(new Vector3(f1, f2, f3));
                }
                int boneCount = dr.ReadInt();
                mesh.Bones = new List<Model3DBone>(boneCount);
                for (int b = 0; b < boneCount; b++)
                {
                    Model3DBone bone = new Model3DBone();
                    mesh.Bones.Add(bone);
                    bone.Name = dr.ReadFullString();
                    int weights = dr.ReadInt();
                    bone.IDs = new List<int>(weights);
                    bone.Weights = new List<double>(weights);
                    for (int w = 0; w < weights; w++)
                    {
                        bone.IDs.Add(dr.ReadInt());
                        bone.Weights.Add(dr.ReadFloat());
                    }
                    bone.MatrixA = ReadMat(dr);
                }
            }
            mod.RootNode = ReadSingleNode(null, dr);
            return mod;
        }

        /// <summary>
        /// Reads a single node from a model.
        /// </summary>
        /// <param name="root">The root node.</param>
        /// <param name="dr">The data reader.</param>
        /// <returns>The node.</returns>
        public Model3DNode ReadSingleNode(Model3DNode root, DataReader dr)
        {
            Model3DNode n = new Model3DNode() { Parent = root };
            string nname = dr.ReadFullString();
            n.Name = nname;
            n.MatrixA = ReadMat(dr);
            int cCount = dr.ReadInt();
            n.Children = new List<Model3DNode>(cCount);
            for (int i = 0; i < cCount; i++)
            {
                n.Children.Add(ReadSingleNode(n, dr));
            }
            return n;
        }

        /// <summary>
        /// Reads a matrix.
        /// </summary>
        /// <param name="reader">The data reader.</param>
        /// <returns>The matrix.</returns>
        public Matrix ReadMat(DataReader reader)
        {
            double a1 = reader.ReadFloat();
            double a2 = reader.ReadFloat();
            double a3 = reader.ReadFloat();
            double a4 = reader.ReadFloat();
            double b1 = reader.ReadFloat();
            double b2 = reader.ReadFloat();
            double b3 = reader.ReadFloat();
            double b4 = reader.ReadFloat();
            double c1 = reader.ReadFloat();
            double c2 = reader.ReadFloat();
            double c3 = reader.ReadFloat();
            double c4 = reader.ReadFloat();
            double d1 = reader.ReadFloat();
            double d2 = reader.ReadFloat();
            double d3 = reader.ReadFloat();
            double d4 = reader.ReadFloat();
            return new Matrix(a1, a2, a3, a4, b1, b2, b3, b4, c1, c2, c3, c4, d1, d2, d3, d4);
            //return new Matrix(a1, b1, c1, d1, a2, b2, c2, d2, a3, b3, c3, d3, a4, b4, c4, d4);
        }

        /// <summary>
        /// Gets all vertices in a model.
        /// </summary>
        /// <param name="input">The model.</param>
        /// <returns>The vertices.</returns>
        public List<Vector3> GetVertices(Model3D input)
        {
            List<Vector3> vertices = new List<Vector3>(input.Meshes.Count * 100);
            foreach (Model3DMesh mesh in input.Meshes)
            {
                for (int i = 0; i < mesh.Vertices.Count; i++)
                {
                    vertices.Add(mesh.Vertices[i]);
                }
            }
            return vertices;
        }

        /// <summary>
        /// Gets all COLLISION ENABLED vertices of a model.
        /// </summary>
        /// <param name="input">The model.</param>
        /// <returns>The collision vertices.</returns>
        public List<Vector3> GetCollisionVertices(Model3D input)
        {
            List<Vector3> vertices = new List<Vector3>(input.Meshes.Count * 100);
            bool colOnly = false;
            foreach (Model3DMesh mesh in input.Meshes)
            {
                if (mesh.Name.ToLowerFast().Contains("collision"))
                {
                    colOnly = true;
                    break;
                }
            }
            foreach (Model3DMesh mesh in input.Meshes)
            {
                if ((!colOnly || mesh.Name.ToLowerFast().Contains("collision")) && !mesh.Name.ToLowerFast().Contains("nocollide"))
                {
                    for (int i = 0; i < mesh.Indices.Count; i++)
                    {
                        vertices.Add(mesh.Vertices[mesh.Indices[i]]);
                    }
                }
            }
            return vertices;
        }

        /// <summary>
        /// Converts a mesh to a BEPU perfect mesh.
        /// </summary>
        /// <param name="input">The model.</param>
        /// <param name="verts">The vertice count if needed.</param>
        /// <returns>The BEPU mesh.</returns>
        public MobileMeshShape MeshToBepu(Model3D input, out int verts)
        {
            List<Vector3> vertices = GetCollisionVertices(input);
            List<int> indices = new List<int>(vertices.Count);
            for (int i = 0; i < vertices.Count; i++)
            {
                indices.Add(indices.Count);
            }
            verts = vertices.Count;
            return new MobileMeshShape(vertices.ToArray(), indices.ToArray(), AffineTransform.Identity, MobileMeshSolidity.DoubleSided);
        }

        /// <summary>
        /// Converts a mesh to a BEPU convex mesh.
        /// </summary>
        /// <param name="input">The model.</param>
        /// <param name="verts">The vertice count if needed.</param>
        /// <param name="center">The center output.</param>
        /// <returns>The BEPU mesh.</returns>
        public ConvexHullShape MeshToBepuConvex(Model3D input, out int verts, out Vector3 center)
        {
            List<Vector3> vertices = GetCollisionVertices(input);
            ConvexHullHelper.RemoveRedundantPoints(vertices);
            verts = vertices.Count;
            return new ConvexHullShape(vertices, out center);
        }
    }
}
