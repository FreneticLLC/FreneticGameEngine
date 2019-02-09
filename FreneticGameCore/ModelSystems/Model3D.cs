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
using BEPUutilities;

namespace FreneticGameCore.ModelSystems
{
    /// <summary>
    /// Represents an abstract 3D model.
    /// </summary>
    public class Model3D
    {
        /// <summary>
        /// The meshes that compose this model.
        /// </summary>
        public Model3DMesh[] Meshes;

        /// <summary>
        /// The root bone node of the model.
        /// </summary>
        public Model3DNode RootNode;

        /// <summary>
        /// The default matrix of the model.
        /// </summary>
        public Matrix MatrixA;
    }

    /// <summary>
    /// Represents a single mesh of an abstract 3D model.
    /// </summary>
    public class Model3DMesh
    {
        // TODO: Remove dependence on BEPU, use FGE types for Vector3 and Matrix.

        /// <summary>
        /// The vertices on this mesh.
        /// </summary>
        public Vector3[] Vertices;

        /// <summary>
        /// The indices on this mesh.
        /// </summary>
        public uint[] Indices;

        /// <summary>
        /// The normal vectors on this mesh.
        /// </summary>
        public Vector3[] Normals;

        /// <summary>
        /// The texture coordinates on this mesh.
        /// </summary>
        public Vector2[] TexCoords;

        /// <summary>
        /// The bones on this mesh.
        /// </summary>
        public Model3DBone[] Bones;

        /// <summary>
        /// The name of this mesh.
        /// </summary>
        public string Name;
    }

    /// <summary>
    /// Represents a single bone in an abstract 3D model mesh.
    /// </summary>
    public class Model3DBone
    {
        /// <summary>
        /// The name of this bone.
        /// </summary>
        public string Name;

        /// <summary>
        /// The vertex IDs of this bone.
        /// </summary>
        public int[] IDs;

        /// <summary>
        /// The vertex weights on this bone.
        /// </summary>
        public double[] Weights;

        /// <summary>
        /// The default matrix of this bone.
        /// </summary>
        public Matrix MatrixA;
    }

    /// <summary>
    /// Represents a single node in an abstract 3D model mesh.
    /// </summary>
    public class Model3DNode
    {
        /// <summary>
        /// The name of this node.
        /// </summary>
        public string Name;

        /// <summary>
        /// The default matrix of this node.
        /// </summary>
        public Matrix MatrixA;

        /// <summary>
        /// The parent of this node.
        /// </summary>
        public Model3DNode Parent;

        /// <summary>
        /// All children of this node.
        /// </summary>
        public Model3DNode[] Children;
    }
}
