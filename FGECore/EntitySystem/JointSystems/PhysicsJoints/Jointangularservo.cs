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
using BepuPhysics.Constraints;

namespace FGECore.EntitySystem.JointSystems.PhysicsJoints;

/// <summary>Constrains two bodies to have a target relative rotation around a specified axis using servo (position-based) control.</summary>
public class JointAngularServo(EntityPhysicsProperty e1, EntityPhysicsProperty e2, Location axis) : PhysicsJointBase<AngularServo>(e1, e2)
{
    /// <summary>The local axis on body A around which rotation is controlled.</summary>
    public Location Axis = axis;

    /// <summary>Target angle in radians around the axis.</summary>
    public float TargetAngle = 0;

    /// <summary>Maximum angular speed the servo can rotate at (radians per second).</summary>
    public float MaximumSpeed = float.MaxValue;

    /// <summary>Maximum torque the servo can apply.</summary>
    public float MaximumForce = float.MaxValue;

    /// <summary>Base speed for the servo controller.</summary>
    public float BaseSpeed = 0;

    /// <summary>Target number of undamped oscillations per second.</summary>
    public float SpringFrequency = 20;

    /// <summary>Ratio of the spring's actual damping to its critical damping. 0 is undamped, 1 is critically damped, and higher values are overdamped.</summary>
    public float SpringDamping = 1;

    /// <summary>Sets the target angle and immediately reapplies.</summary>
    public void SetTargetAngle(float angleInRadians)
    {
        TargetAngle = angleInRadians;
        Reapply();
    }

    /// <inheritdoc/>
    public override AngularServo CreateJointDescription() => new()
    {
        TargetRelativeRotationLocalA = System.Numerics.Quaternion.CreateFromAxisAngle(Axis.ToNumerics(), TargetAngle),
        SpringSettings = new SpringSettings(SpringFrequency, SpringDamping),
        ServoSettings = new ServoSettings(MaximumSpeed, BaseSpeed, MaximumForce)
    };
}
