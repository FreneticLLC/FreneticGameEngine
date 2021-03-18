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
using BepuPhysics;
using BepuPhysics.Collidables;
using BepuUtilities.Memory;
using FGECore.PhysicsSystem;
using FGECore.PropertySystem;

namespace FGECore.EntitySystem.PhysicsHelpers
{
    /// <summary>A capsule shape for an entity.</summary>
    public class EntityCapsuleShape : EntityShapeHelper
    {
        /// <summary>Constructs a new <see cref="EntityCapsuleShape"/> of the specified size.</summary>
        public EntityCapsuleShape(float radius, float height, PhysicsSpace space)
        {
            Capsule capsule = new Capsule(radius, height);
            TypedIndex capsuleIndex = space.Internal.CoreSimulation.Shapes.Add(capsule);
            space.Internal.CoreSimulation.BufferPool.Take(1, out Buffer<CompoundChild> buffer);
            buffer[0].LocalPose = new RigidPose(Vector3.Zero, Quaternion_Y2Z);
            buffer[0].ShapeIndex = capsuleIndex;
            Compound compound = new Compound(buffer);
            BepuShape = capsule;
            ShapeIndex = space.Internal.CoreSimulation.Shapes.Add(compound);
        }
    }
}
