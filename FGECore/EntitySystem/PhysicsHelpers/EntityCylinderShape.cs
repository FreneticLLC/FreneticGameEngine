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
using BepuPhysics;
using BepuUtilities.Memory;
using System.Numerics;
using FGECore.MathHelpers;

namespace FGECore.EntitySystem.PhysicsHelpers
{
    /// <summary>A cylinder shape for an entity.</summary>
    public class EntityCylinderShape : EntityShapeHelper
    {
        /// <summary>Constructs a new <see cref="EntityCylinderShape"/> of the specified size.</summary>
        public EntityCylinderShape(float radius, float height, PhysicsSpace space)
        {
            Cylinder cylinder = new Cylinder(radius, height);
            TypedIndex cylinderIndex = space.Internal.CoreSimulation.Shapes.Add(cylinder);
            space.Internal.CoreSimulation.BufferPool.Take(1, out Buffer<CompoundChild> buffer);
            buffer[0].LocalPose = new RigidPose(Vector3.Zero, Y2Z);
            buffer[0].ShapeIndex = cylinderIndex;
            Compound compound = new Compound(buffer);
            BepuShape = cylinder;
            ShapeIndex = space.Internal.CoreSimulation.Shapes.Add(compound);
        }

        private static System.Numerics.Quaternion Y2Z = MathHelpers.Quaternion.GetQuaternionBetween(Location.UnitY, Location.UnitZ).ToNumerics();
    }
}
