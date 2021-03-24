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
using BepuPhysics;
using BepuPhysics.Collidables;
using FGECore.PhysicsSystem;
using FGECore.PropertySystem;

namespace FGECore.EntitySystem.PhysicsHelpers
{
    /// <summary>A convex hull shape for an entity.</summary>
    public class EntityMeshShape : EntityShapeHelper
    {
        /// <summary>Constructs a new <see cref="EntityMeshShape"/> from the specified mesh object.</summary>
        public EntityMeshShape(Mesh mesh, PhysicsSpace space)
        {
            BepuShape = mesh;
            ShapeIndex = space.Internal.CoreSimulation.Shapes.Add(mesh);
        }

        /// <summary>Implements <see cref="EntityShapeHelper.ComputeInertia(float, out BodyInertia)"/>.</summary>
        public override void ComputeInertia(float mass, out BodyInertia inertia)
        {
            ((Mesh) BepuShape).ComputeClosedInertia(mass, out inertia);
        }
    }
}
