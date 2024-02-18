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

/// <summary>A joint that restricts one axis of rotation between two entities, like a hinge that can also swivel.</summary>
public class JointSwivelHingeAngular(EntityPhysicsProperty e1, EntityPhysicsProperty e2, Location hinge, Location twist) : PhysicsJointBase<AngularSwivelHinge>(e1, e2)
{
    /// <summary>The hinge axis. For a door, this with be vertical.</summary>
    public Location WorldHinge = hinge;

    /// <summary>The twist axis. This should be perpendicular to the hinge axis.</summary>
    public Location WorldTwist = twist;

    /// <inheritdoc/>
    public override AngularSwivelHinge CreateJointDescription()
    {
        return new AngularSwivelHinge() { LocalHingeAxisB = WorldHinge.ToNumerics(), LocalSwivelAxisA = WorldTwist.ToNumerics(), SpringSettings = new SpringSettings(20, 1) };
    }
}
