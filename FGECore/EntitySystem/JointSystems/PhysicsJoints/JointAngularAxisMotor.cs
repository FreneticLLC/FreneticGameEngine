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

/// <summary>Constrains the relative angular velocity of two bodies around a local axis attached to body A to a target velocity.</summary>
public class JointAngularAxisMotor(EntityPhysicsProperty e1, EntityPhysicsProperty e2, Location axis) : PhysicsJointBase<AngularAxisMotor>(e1, e2)
{
    /// <summary>The relative rotational axis.</summary>
    public Location Axis = axis;

    /// <summary>Inverse damping; how soft this motor should be, 0 means super-rigid, 1 means very soft, above 1 has very little force application left.</summary>
    public float Softness = 0.03f;

    /// <summary>Maximum amount of force this motor can apply in one second.</summary>
    public float MaximumForce = 100_000;

    /// <summary>Current target speed.</summary>
    public float TargetVelocity = 0;

    /// <summary>Sets the target velocity and immediately reapplies.</summary>
    public void SetTargetVelocity(float targetVel)
    {
        TargetVelocity = targetVel;
        Reapply();
    }

    /// <inheritdoc/>
    public override AngularAxisMotor CreateJointDescription() => new()
    {
        TargetVelocity = TargetVelocity,
        LocalAxisA = Axis.ToNumerics(),
        Settings = new() { MaximumForce = MaximumForce, Softness = Softness }
    };
}
