//
// This file is part of the Frenetic Game Engine, created by Frenetic LLC.
// This code is Copyright (C) Frenetic LLC under the terms of a strict license.
// See README.md or LICENSE.txt in the FreneticGameEngine source root for the contents of the license.
// If neither of these are available, assume that neither you nor anyone other than the copyright holder
// hold any right or permission to use this software until such time as the official license is identified.
//

using BepuPhysics;
using BepuPhysics.Constraints;
using FGECore.PhysicsSystem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FGECore.EntitySystem.JointSystems
{
    /// <summary>The generic base class for all physics-based joints.</summary>
    public abstract class PhysicsJointBase : GenericBaseJoint
    {
        /// <summary>The applicable physics entities for this joint.</summary>
        public EntityPhysicsProperty One, Two;

        /// <summary>Implements <see cref="GenericBaseJoint.EntityOne"/>.</summary>
        public override BasicEntity EntityOne => One.Entity;

        /// <summary>Implements <see cref="GenericBaseJoint.EntityTwo"/>.</summary>
        public override BasicEntity EntityTwo => Two.Entity;

        /// <summary>Gets the relevant <see cref="PhysicsSpace"/>.</summary>
        public PhysicsSpace PhysicsWorld => One.PhysicsWorld;
    }

    /// <summary>The base class for all physics-based joints, with a type reference to the underlying constraint.</summary>
    public abstract class PhysicsJointBase<T> : PhysicsJointBase where T : unmanaged, ITwoBodyConstraintDescription<T>
    {
        /// <summary>Constructs the physics joint base.</summary>
        public PhysicsJointBase(EntityPhysicsProperty _one, EntityPhysicsProperty _two)
        {
            One = _one;
            Two = _two;
        }

        /// <summary>A reference to the underlying physics constraint.</summary>
        public ConstraintHandle CurrentJoint;

        /// <summary>Construct the applicable joint description object.</summary>
        public abstract T CreateJointDescription();

        /// <summary>Implements <see cref="GenericBaseJoint.Enable"/> by spawning the joint into the physics space.</summary>
        public override void Enable()
        {
            CurrentJoint = PhysicsWorld.Internal.CoreSimulation.Solver.Add(One.SpawnedBody.Handle, Two.SpawnedBody.Handle, CreateJointDescription());
        }

        /// <summary>Implements <see cref="GenericBaseJoint.Disable"/> by spawning the joint into the physics space.</summary>
        public override void Disable()
        {
            PhysicsWorld.Internal.CoreSimulation.Solver.Remove(CurrentJoint);
            CurrentJoint = default;
        }
    }
}
