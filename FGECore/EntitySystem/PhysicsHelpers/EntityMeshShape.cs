//
// This file is part of the Frenetic Game Engine, created by Frenetic LLC.
// This code is Copyright (C) Frenetic LLC under the terms of a strict license.
// See README.md or LICENSE.txt in the FreneticGameEngine source root for the contents of the license.
// If neither of these are available, assume that neither you nor anyone other than the copyright holder
// hold any right or permission to use this software until such time as the official license is identified.
//

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using FGECore.PhysicsSystem;
using FGECore.PropertySystem;
using BepuPhysics;
using BepuPhysics.Collidables;
using BepuUtilities.Memory;

namespace FGECore.EntitySystem.PhysicsHelpers;

/// <summary>A concave mesh shape for an entity.</summary>
public class EntityMeshShape : EntityShapeHelper
{
    /// <summary>Constructs a new <see cref="EntityMeshShape"/> from the specified mesh object.</summary>
    public EntityMeshShape(Mesh mesh, PhysicsSpace space) : base(space)
    {
        BepuShape = mesh;
        mesh.ComputeClosedCenterOfMass(out float volume, out _);
        Volume = volume;
    }

    /// <summary>Implements <see cref="EntityShapeHelper.Register"/>.</summary>
    public override EntityMeshShape Register()
    {
        EntityMeshShape dup = MemberwiseClone() as EntityMeshShape;
        dup.ShapeIndex = Space.Internal.CoreSimulation.Shapes.Add((Mesh)BepuShape);
        return dup;
    }

    /// <summary>Implements <see cref="Object.ToString"/>.</summary>
    public override string ToString()
    {
        Mesh mesh = (Mesh)BepuShape;
        return $"{nameof(EntityMeshShape)}({mesh.Triangles.Length} tris)";
    }

    /// <summary>Implements <see cref="EntityShapeHelper.ComputeInertia(float, out BodyInertia)"/>.</summary>
    public override void ComputeInertia(float mass, out BodyInertia inertia)
    {
        inertia = ((Mesh) BepuShape).ComputeClosedInertia(mass);
    }

    /// <summary>Invalid.</summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public override void Sweep<TSweepHitHandler>(in Simulation simulation, in Vector3 pos, in BodyVelocity velocity, float maximumT, BufferPool pool, ref TSweepHitHandler hitHandler)
    {
        throw new InvalidOperationException("Mesh cannot be used for convex sweep.");
    }
}
