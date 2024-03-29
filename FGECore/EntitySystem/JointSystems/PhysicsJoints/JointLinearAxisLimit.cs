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

/// <summary>Limits the relative motion of two entities along a linear axis.</summary>
public class JointLinearAxisLimit(EntityPhysicsProperty e1, EntityPhysicsProperty e2, float min, float max, Location relPos1, Location relPos2, Location axis) : PhysicsJointBase<LinearAxisLimit>(e1, e2)
{
    /// <summary>The minimum distance between the two entities.</summary>
    public float Min = min;

    /// <summary>The maximum distance between the two entities.</summary>
    public float Max = max;

    /// <summary>Offset from <see cref="PhysicsJointBase.One"/> to its anchor.</summary>
    public Location OffsetOne = relPos1;

    /// <summary>Offset from <see cref="PhysicsJointBase.Two"/> to its anchor.</summary>
    public Location OffsetTwo = relPos2;

    /// <summary>The constrained axis.</summary>
    public Location Axis = axis;

    /// <inheritdoc/>
    public override LinearAxisLimit CreateJointDescription() => new()
    {
        LocalAxis = Axis.ToNumerics(),
        MinimumOffset = Min,
        MaximumOffset = Max,
        LocalOffsetA = OffsetOne.ToNumerics(),
        LocalOffsetB = OffsetTwo.ToNumerics(),
        SpringSettings = new SpringSettings(20, 1)
    };
}
