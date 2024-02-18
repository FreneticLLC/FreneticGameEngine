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
using BepuPhysics.Constraints;
using BepuPhysics;

namespace FGECore.EntitySystem.JointSystems.PhysicsJoints;

/// <summary>Constrains two bodies to be perfectly glued together.</summary>
public class JointWeld(EntityPhysicsProperty e1, EntityPhysicsProperty e2) : PhysicsJointBase<Weld>(e1, e2)
{
    /// <summary>The tracked offset between the entities.</summary>
    public RigidPose Offset;

    /// <inheritdoc/>
    public override Weld CreateJointDescription()
    {
        RigidPose rt1 = new(One.Position.ToNumerics(), One.Orientation.ToNumerics());
        RigidPose rt2 = new(Two.Position.ToNumerics(), Two.Orientation.ToNumerics());
        RigidPose.Invert(rt2, out RigidPose rt2inv);
        RigidPose.MultiplyWithoutOverlap(rt1, rt2inv, out Offset);
        return new Weld() { LocalOffset = Offset.Position, LocalOrientation = Offset.Orientation, SpringSettings = new SpringSettings(20, 1) };
    }
}
