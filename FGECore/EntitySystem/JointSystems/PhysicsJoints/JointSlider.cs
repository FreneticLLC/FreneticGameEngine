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
public class JointSlider(EntityPhysicsProperty e1, EntityPhysicsProperty e2, Location dir) : PhysicsJointBase<PointOnLineServo>(e1, e2)
{
    /// <summary>The direction of the slider axis.</summary>
    public Location Direction = dir.Normalize();

    /// <summary>Offset from <see cref="PhysicsJointBase.One"/> to its anchor.</summary>
    public Location OffsetOne = Location.Zero;

    /// <summary>Offset from <see cref="PhysicsJointBase.Two"/> to its anchor.</summary>
    public Location OffsetTwo = Location.Zero;

    /// <summary>Maximum speed this servo can try to move at.</summary>
    public float MaxSpeed = float.MaxValue;

    /// <summary>Maximum amount of force this servo can output.</summary>
    public float MaxForce = float.MaxValue;

    /// <summary>(?) TODO: Explain what this done.</summary>
    public float BaseSpeed = 0;

    /// <summary>Target number of undamped oscillations per second.</summary>
    public float SpringFrequency = 20;

    /// <summary>Ratio of the spring's actual damping to its critical damping. 0 is undamped, 1 is critically damped, and higher values are overdamped.</summary>
    public float SpringDamping = 1;

    /// <inheritdoc/>
    public override PointOnLineServo CreateJointDescription() => new()
    {
        LocalDirection = Direction.ToNumerics(),
        LocalOffsetA = OffsetOne.ToNumerics(),
        LocalOffsetB = OffsetTwo.ToNumerics(),
        SpringSettings = new(SpringFrequency, SpringDamping),
        ServoSettings = new(MaxSpeed, BaseSpeed, MaxForce)
    };
}
