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

/// <summary>A joint that works like a ball in a socket (like how human arms are attached to shoulders).</summary>
public class JointBallSocket(EntityPhysicsProperty e1, EntityPhysicsProperty e2, Location pos1, Location pos2) : PhysicsJointBase<BallSocket>(e1, e2)
{
    /// <summary>The world space position that the ball is centered on.</summary>
    public Location PositionOne = pos1;

    /// <summary>The world space position that the ball is centered on.</summary>
    public Location PositionTwo = pos2;

    /// <inheritdoc/>
    public override BallSocket CreateJointDescription() => new()
    {
        LocalOffsetA = PositionOne.ToNumerics(),
        LocalOffsetB = PositionTwo.ToNumerics(),
        SpringSettings = new SpringSettings(20, 1)
    };
}
