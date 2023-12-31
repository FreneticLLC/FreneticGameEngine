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
using BepuPhysics.Collidables;
using System.Numerics;

namespace FGECore.EntitySystem.PhysicsHelpers;

/// <summary>A convex hull shape for an entity.</summary>
public class EntityConvexHullShape : EntityShapeHelper
{
    /// <summary>Constructs a new <see cref="EntityConvexHullShape"/> from the specified hull object.</summary>
    public EntityConvexHullShape(ConvexHull hull, PhysicsSpace space) : base(space)
    {
        BepuShape = hull;
        Volume = 0;
        ConvexHull.ConvexHullTriangleSource tris = new(hull);
        while (tris.GetNextTriangle(out Vector3 a, out Vector3 b, out Vector3 c))
        {
            Volume += MeshInertiaHelper.ComputeTetrahedronVolume(a, b, c);
        }
    }

    /// <summary>Implements <see cref="EntityShapeHelper.Register"/>.</summary>
    public override EntityConvexHullShape Register()
    {
        EntityConvexHullShape dup = MemberwiseClone() as EntityConvexHullShape;
        dup.ShapeIndex = Space.Internal.CoreSimulation.Shapes.Add((ConvexHull) BepuShape);
        return dup;
    }

    /// <inheritdoc/>
    public override string ToString()
    {
        ConvexHull hull = (ConvexHull)BepuShape;
        return $"{nameof(EntityConvexHullShape)}({hull.Points.Length} points)";
    }
}
