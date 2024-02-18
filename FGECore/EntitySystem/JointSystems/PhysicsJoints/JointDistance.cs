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

/// <summary>Constrains two entities to have a specific position on each body maintain a specific distance from each other.</summary>
public class JointDistance(EntityPhysicsProperty e1, EntityPhysicsProperty e2, float _min, float _max, Location e1pos, Location e2pos) : PhysicsJointBase<DistanceLimit>(e1, e2)
{
    /// <summary>The minimum allowed distance.</summary>
    public float Min = _min;

    /// <summary>The maximum allowed distance.</summary>
    public float Max = _max;

    /// <summary>The position relative to entity <see cref="PhysicsJointBase.One"/>'s body.</summary>
    public Location Ent1Pos = e1pos;

    /// <summary>The position relative to entity <see cref="PhysicsJointBase.Two"/>'s body.</summary>
    public Location Ent2Pos = e2pos;

    /// <inheritdoc/>
    public override DistanceLimit CreateJointDescription() => new(Ent1Pos.ToNumerics(), Ent2Pos.ToNumerics(), Min, Max, new SpringSettings(20, 1));
}
