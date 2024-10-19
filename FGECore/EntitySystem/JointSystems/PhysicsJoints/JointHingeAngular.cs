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
using BepuPhysics;
using BepuPhysics.Constraints;
using BepuUtilities;

namespace FGECore.EntitySystem.JointSystems.PhysicsJoints;

/// <summary>A joint that works like the angular portion of a hinge. The two entities can only rotate relative to each other around the hinge axis.</summary>
public class JointHingeAngular(EntityPhysicsProperty e1, EntityPhysicsProperty e2) : PhysicsJointBase<AngularHinge>(e1, e2)
{
    /// <summary>The hinge axis, relative to <see cref="PhysicsJointBase.One"/>.</summary>
    public Location AxisOne = Location.UnitX;

    /// <summary>The hinge axis, relative to <see cref="PhysicsJointBase.Two"/>.</summary>
    public Location AxisTwo = Location.UnitY;

    /// <summary>Assuming that <see cref="AxisTwo"/> is a car's up axis, updates <see cref="AxisOne"/> to be a wheel steering axis for the given angle around the car's up axis.</summary>
    public void SetSteerAngle(float angle)
    {
        Matrix3x3.CreateFromAxisAngle(AxisTwo.ToNumerics(), -angle, out Matrix3x3 rotation);
        Matrix3x3.Transform(AxisOne.ToNumerics(), rotation, out Vector3 newAxisA);
        AxisOne = newAxisA.ToLocation();
        Reapply();
    }

    /// <inheritdoc/>
    public override AngularHinge CreateJointDescription() => new()
    {
        LocalHingeAxisA = AxisOne.ToNumerics(),
        LocalHingeAxisB = AxisTwo.ToNumerics(),
        SpringSettings = new SpringSettings(20, 1)
    };
}
