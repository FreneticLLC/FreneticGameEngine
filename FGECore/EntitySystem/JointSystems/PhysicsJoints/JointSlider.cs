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
using FGECore.MathHelpers;
using BepuPhysics.Constraints;

namespace FGECore.EntitySystem.JointSystems.PhysicsJoints;

/// <summary>A joint that constrains two entities to slide along a single line relative to each other.</summary>
public class JointSlider : PhysicsJointBase<PointOnLineServo>
{
    /// <summary>Constructs the <see cref="JointSlider"/>.</summary>
    public JointSlider(EntityPhysicsProperty e1, EntityPhysicsProperty e2, Location dir) : base(e1, e2)
    {
        Direction = dir.Normalize();
    }

    /// <summary>The direction of the slider axis.</summary>
    public Location Direction;

    /// <summary>Implements <see cref="PhysicsJointBase{T}.CreateJointDescription"/>.</summary>
    public override PointOnLineServo CreateJointDescription()
    {
        return new PointOnLineServo() { LocalDirection = Direction.ToNumerics(), SpringSettings = new SpringSettings(20, 1), ServoSettings = ServoSettings.Default };
    }
}
