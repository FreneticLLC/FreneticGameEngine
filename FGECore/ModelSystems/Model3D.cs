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
using System.Numerics;
using BepuPhysics;
using BepuPhysics.Collidables;
using FGECore.EntitySystem.PhysicsHelpers;
using FGECore.MathHelpers;
using FGECore.PhysicsSystem;

namespace FGECore.ModelSystems;

/// <summary>Represents an abstract 3D model.</summary>
public class Model3D
{
    /// <summary>Name of this model. Often a partial filename matched to <see cref="CoreModelEngine"/>'s inputs.</summary>
    public string Name;

    /// <summary>The meshes that compose this model.</summary>
    public Model3DMesh[] Meshes = [];

    /// <summary>The root bone node of the model.</summary>
    public Model3DNode RootNode;

    /// <summary>The default matrix of the model.</summary>
    public Matrix4x4 MatrixA = Matrix4x4.Identity;

    /// <summary>User-tag on this model. Often this is an FGEGraphics Model instance.</summary>
    public object Tag;

    /// <summary>(Potentially null) info data lines that sourced this model, often containing texture data.</summary>
    public string[] InfoDataLines;

    /// <summary>The type of collision data this model provides.</summary>
    public Model3DCollisionType CollisionType = Model3DCollisionType.NONE;

    /// <summary>The physics shape for this model, if calculated. Do not read directly, use <see cref="GetShape(PhysicsSpace)"/>.</summary>
    public EntityShapeHelper Shape;

    /// <summary>If true, this model is populated with valid data. If false, it has in some way failed.</summary>
    public bool IsValid = false;

    /// <summary>Creates an appropriate entity shape for this model.</summary>
    /// <param name="space">The physics space that this shape will be registered into.</param>
    public EntityShapeHelper GetShape(PhysicsSpace space)
    {
        if (Shape is null)
        {
            switch (CollisionType)
            {
                case Model3DCollisionType.CONVEX:
                    Shape = ModelHandler.MeshToBepuConvexSingle(space, this, out _);
                    break;
                case Model3DCollisionType.COMPLEX:
                    Mesh bepuMesh = ModelHandler.MeshToBepu(space, this, out _);
                    Shape = new EntityMeshShape(bepuMesh, space);
                    break;
                case Model3DCollisionType.COMPOUND_CONVEX:
                    Shape = ModelHandler.MeshToBepuConvexCompound(space, this, out _);
                    break;
                case Model3DCollisionType.NONE:
                default:
                    if (Meshes.Length == 0)
                    {
                        Shape = new EntityBoxShape(new Location(1, 1, 1), space);
                        break;
                    }
                    Location start = Meshes[0].Vertices[0].ToLocation();
                    AABB box = new(start, start);
                    foreach (Model3DMesh mesh in Meshes)
                    {
                        foreach (Vector3 vert in mesh.Vertices)
                        {
                            box.Include(vert.ToLocation());
                        }
                    }
                    EntityBoxShape boxShape = new(box.Max - box.Min, space);
                    Location center = (box.Min + box.Max) / 2;
                    Shape = new EntityCompoundShape([new(boxShape, new RigidPose(-center.ToNumerics()))], boxShape.Volume, space);
                    break;
            }
        }
        return Shape.Duplicate(space);
    }

    /// <summary>Returns a simple shallow copy of this <see cref="Model3D"/>.</summary>
    public Model3D Duplicate()
    {
        return new()
        {
            Meshes = Meshes,
            RootNode = RootNode,
            MatrixA = MatrixA,
            Tag = Tag,
            InfoDataLines = InfoDataLines
        };
    }
}

/// <summary>Represents a single mesh of an abstract 3D model.</summary>
public class Model3DMesh
{
    /// <summary>The vertices on this mesh.</summary>
    public Vector3[] Vertices;

    /// <summary>The indices on this mesh.</summary>
    public uint[] Indices;

    /// <summary>The normal vectors on this mesh.</summary>
    public Vector3[] Normals;

    /// <summary>The texture coordinates on this mesh.</summary>
    public Vector2[] TexCoords;

    /// <summary>The color data on this mesh.</summary>
    public Vector4[] Colors;

    /// <summary>The bones on this mesh.</summary>
    public Model3DBone[] Bones;

    /// <summary>The name of this mesh.</summary>
    public string Name;

    /// <summary>If true, this is a pseudo-mesh used as a special marker.</summary>
    public bool IsMarker = false;

    /// <summary>If true, this is a pseudo-mesh used as a complex collision mesh.</summary>
    public bool IsCollisionComplexMesh = false;

    /// <summary>If true, this is a pseudo-mesh used a a convex collision mesh.</summary>
    public bool IsCollisionConvexMesh = false;

    /// <summary>If true, this mesh renders as normal but is excluded from collision calculation.</summary>
    public bool NoCollide = false;

    /// <summary>If true, this mesh should render visibly (excludes collision, marker, etc.).</summary>
    public bool IsVisible = true;
}

/// <summary>Represents a single bone in an abstract 3D model mesh.</summary>
public class Model3DBone
{
    /// <summary>The name of this bone.</summary>
    public string Name;

    /// <summary>The vertex IDs of this bone.</summary>
    public int[] IDs;

    /// <summary>The vertex weights on this bone.</summary>
    public double[] Weights;

    /// <summary>The default matrix of this bone.</summary>
    public Matrix4x4 MatrixA;
}

/// <summary>Represents a single node in an abstract 3D model mesh.</summary>
public class Model3DNode
{
    /// <summary>The name of this node.</summary>
    public string Name;

    /// <summary>The default matrix of this node.</summary>
    public Matrix4x4 MatrixA;

    /// <summary>The parent of this node.</summary>
    public Model3DNode Parent;

    /// <summary>All children of this node.</summary>
    public Model3DNode[] Children;
}

/// <summary>Enumeration of possible collision types found in a 3D model.</summary>
public enum Model3DCollisionType
{
    /// <summary>No collision data provided. Will be handled like a simple AABB.</summary>
    NONE,
    /// <summary>Has complex mesh component.</summary>
    COMPLEX,
    /// <summary>A single convex component.</summary>
    CONVEX,
    /// <summary>Multiple convex components.</summary>
    COMPOUND_CONVEX
}
