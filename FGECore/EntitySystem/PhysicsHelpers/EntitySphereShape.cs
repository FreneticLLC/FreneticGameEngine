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
using FGECore.PhysicsSystem;
using FGECore.PropertySystem;
using BepuPhysics;
using BepuPhysics.Collidables;

namespace FGECore.EntitySystem.PhysicsHelpers;

/// <summary>A sphere shape for an entity.</summary>
public class EntitySphereShape : EntityShapeHelper
{
    /// <summary>Constructs a new <see cref="EntitySphereShape"/> of the specified size.</summary>
    public EntitySphereShape(float size, PhysicsSpace space) : base(space)
    {
        BepuShape = new Sphere(size);
    }

    /// <summary>Implements <see cref="EntityShapeHelper.Register"/>.</summary>
    public override EntitySphereShape Register()
    {
        EntitySphereShape dup = MemberwiseClone() as EntitySphereShape;
        dup.ShapeIndex = Space.Internal.CoreSimulation.Shapes.Add((Sphere)BepuShape);
        return dup;
    }

    /// <summary>Implements <see cref="Object.ToString"/>.</summary>
    public override string ToString()
    {
        Sphere sphere = (Sphere)BepuShape;
        return $"{nameof(EntitySphereShape)}({sphere.Radius})";
    }
}
