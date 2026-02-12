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

/// <summary>Drives rotation of a local axis toward a target angle using a servo, with spring/damping and speed/force limits.
/// The target can be changed at runtime via <see cref="SetTargetAngle"/>.</summary>
public class JointAngularServo(EntityPhysicsProperty e1, EntityPhysicsProperty e2, Location axis) : PhysicsJointBase<AngularServo>(e1, e2)
{
    /// <summary>Local axis on <see cref="PhysicsJointBase.One"/> to rotate around.</summary>
    public Location Axis = axis;

    /// <summary>Target angle (radians) of the <see cref="Axis"/> of <see cref="PhysicsJointBase.One"/>.</summary>
    public float TargetAngle = 0;

    /// <summary>Maximum speed this servo can try to move at.</summary>
    public float MaxSpeed = float.MaxValue;

    /// <summary>Maximum amount of force this servo can output.</summary>
    public float MaxForce = float.MaxValue;

    /// <summary>Minimum move speed. Clamped by <see cref="MaxSpeed"/> and by error per step to avoid overshoot.</summary>
    public float BaseSpeed = 0;

    /// <summary>Target number of undamped oscillations per second.</summary>
    public float SpringFrequency = 20;

    /// <summary>Ratio of the spring's actual damping to its critical damping. 0 is undamped, 1 is critically damped, and higher values are overdamped.</summary>
    public float SpringDamping = 1;

    /// <summary>Use to change the target angle (steering/aiming). If the target angle is changed, reapplies so the new angle is used next frame. 
    /// Otherwise it takes effect when added.</summary>
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
        ServoSettings = new ServoSettings(MaxSpeed, BaseSpeed, MaxForce)
    };
}
