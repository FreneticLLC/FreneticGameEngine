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
using FGECore.MathHelpers;

namespace FGECore.EntitySystem.JointSystems.NonPhysicsJoints;

/// <summary>Special pseudo-constraint to indicate two physics entities shouldn't collide with each other.</summary>
public class JointNoCollide(EntityPhysicsProperty _one, EntityPhysicsProperty _two) : NonPhysicalJointBase(_one.Entity, _two.Entity)
{
    /// <summary>Reference to the first entity's physics property.</summary>
    public EntityPhysicsProperty PhysicsOne = _one;

    /// <summary>Reference to the second entity's physics property.</summary>
    public EntityPhysicsProperty PhysicsTwo = _two;

    /// <summary>Implements <see cref="GenericBaseJoint.Enable"/>.</summary>
    public override void Enable()
    {
        PhysicsOne.Internal.NoCollideIDs ??= new HashSet<long>(16);
        PhysicsOne.Internal.NoCollideIDs.Add(PhysicsTwo.Entity.EID);
        PhysicsTwo.Internal.NoCollideIDs ??= new HashSet<long>(16);
        PhysicsTwo.Internal.NoCollideIDs.Add(PhysicsOne.Entity.EID);
    }

    /// <summary>Implements <see cref="GenericBaseJoint.Disable"/>.</summary>
    public override void Disable()
    {
        PhysicsOne.Internal.NoCollideIDs.Remove(PhysicsTwo.Entity.EID);
        PhysicsTwo.Internal.NoCollideIDs.Remove(PhysicsOne.Entity.EID);
    }

    /// <summary>Implementation of <see cref="NonPhysicalJointBase.Solve"/> that does nothing.</summary>
    public override void Solve()
    {
        // Do nothing
    }
}
