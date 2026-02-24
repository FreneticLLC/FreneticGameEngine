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
using FGECore.PhysicsSystem;
using BepuPhysics;
using BepuPhysics.Collidables;
using BepuUtilities.Memory;

namespace FGECore.EntitySystem.PhysicsHelpers;

/// <summary>A compound shape (shape made of several other shapes) for an entity.</summary>
public class EntityCompoundShape : EntityShapeHelper
{
    /// <summary>Child shape information for a compound entity shape.</summary>
    /// <param name="Shape">The child shape.</param>
    /// <param name="Pose">The pose of the child shape relative to the compound's origin.</param>
    public record struct EntityCompoundChild(EntityShapeHelper Shape, RigidPose Pose);

    /// <summary>All child-shapes of this entity.</summary>
    public List<EntityCompoundChild> Children = [];

    /// <summary>Constructs a new <see cref="EntityCompoundShape"/> from the specified compound object and volume estimate.</summary>
    public EntityCompoundShape(List<EntityCompoundChild> compound, double _volume, PhysicsSpace space) : base(space)
    {
        Children = compound;
        BepuShape = null;
        Volume = _volume;
    }

    /// <summary>Implements <see cref="EntityShapeHelper.Register"/>.</summary>
    public override EntityCompoundShape Register()
    {
        EntityCompoundShape dup = MemberwiseClone() as EntityCompoundShape;
        Space.Internal.Pool.Take(dup.Children.Count, out Buffer<CompoundChild> children);
        int childIndex = 0;
        foreach (EntityCompoundChild child in dup.Children)
        {
            TypedIndex childShapeInd = child.Shape.Register().ShapeIndex;
            children[childIndex++] = new(child.Pose, childShapeInd);
        }
        Compound created = new(children);
        dup.BepuShape = created;
        dup.ShapeIndex = Space.Internal.CoreSimulation.Shapes.Add(created);
        return dup;
    }

    /// <inheritdoc/>
    public override EntityShapeHelper Duplicate(PhysicsSpace space)
    {
        EntityCompoundShape shape = base.Duplicate(space) as EntityCompoundShape;
        shape.Children = [.. Children];
        return shape;
    }

    /// <inheritdoc/>
    public override void Unregister()
    {
        base.Unregister();
        foreach (EntityCompoundChild child in Children)
        {
            child.Shape.Unregister();
        }
    }

    /// <inheritdoc/>
    public override string ToString()
    {
        return $"{nameof(EntityCompoundShape)}({Children.Count} children)";
    }

    /// <summary>Implements <see cref="EntityShapeHelper.ComputeInertia(float, out BodyInertia)"/>.</summary>
    public override void ComputeInertia(float mass, out BodyInertia inertia)
    {
        Compound compound = (Compound)BepuShape;
        float[] childMasses = [.. Enumerable.Repeat(mass / Children.Count, Children.Count)];
        inertia = compound.ComputeInertia(childMasses, Space.Internal.CoreSimulation.Shapes);
    }

    /// <inheritdoc/>
    public override void Sweep<TSweepHitHandler>(in Simulation simulation, in Vector3 pos, in BodyVelocity velocity, float maximumT, BufferPool pool, ref TSweepHitHandler hitHandler)
    {
        throw new InvalidOperationException("Compound cannot be used for convex sweep.");
    }
}
