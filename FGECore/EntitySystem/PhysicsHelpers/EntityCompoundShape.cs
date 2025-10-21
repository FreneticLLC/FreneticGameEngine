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
    /// <summary>Constructs a new <see cref="EntityCompoundShape"/> from the specified compound object and volume estimate.</summary>
    public EntityCompoundShape(Compound compound, double _volume, PhysicsSpace space) : base(space)
    {
        BepuShape = compound;
        Volume = _volume;
    }

    /// <summary>Implements <see cref="EntityShapeHelper.Register"/>.</summary>
    public override EntityCompoundShape Register()
    {
        EntityCompoundShape dup = MemberwiseClone() as EntityCompoundShape;
        dup.ShapeIndex = Space.Internal.CoreSimulation.Shapes.Add((Compound)BepuShape);
        return dup;
    }

    /// <inheritdoc/>
    public override string ToString()
    {
        Compound compound = (Compound)BepuShape;
        return $"{nameof(EntityCompoundShape)}({compound.ChildCount} children)";
    }

    /// <inheritdoc/>
    public override void Sweep<TSweepHitHandler>(in Simulation simulation, in Vector3 pos, in BodyVelocity velocity, float maximumT, BufferPool pool, ref TSweepHitHandler hitHandler)
    {
        throw new InvalidOperationException("Compound cannot be used for convex sweep.");
    }
}
