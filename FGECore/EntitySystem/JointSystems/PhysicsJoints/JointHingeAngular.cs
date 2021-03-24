//
// This file is part of the Frenetic Game Engine, created by Frenetic LLC.
// This code is Copyright (C) Frenetic LLC under the terms of a strict license.
// See README.md or LICENSE.txt in the FreneticGameEngine source root for the contents of the license.
// If neither of these are available, assume that neither you nor anyone other than the copyright holder
// hold any right or permission to use this software until such time as the official license is identified.
//

using BepuPhysics.Constraints;
using FGECore.MathHelpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FGECore.EntitySystem.JointSystems.PhysicsJoints
{
    /// <summary>A joint that works like the angular portion of a hinge. The two entities can only rotate relative to each other around the hinge axis.</summary>
    public class JointHingeAngular : PhysicsJointBase<AngularHinge>
    {
        /// <summary>Constructs the <see cref="JointHingeAngular"/>.</summary>
        public JointHingeAngular(EntityPhysicsProperty e1, EntityPhysicsProperty e2, Location hingeAxis) : base(e1, e2)
        {
            Axis = hingeAxis;
        }

        /// <summary>The hinge axis. For a door, this with be vertical.</summary>
        public Location Axis;

        /// <summary>Implements <see cref="PhysicsJointBase{T}.CreateJointDescription"/>.</summary>
        public override AngularHinge CreateJointDescription()
        {
            return new AngularHinge() { LocalHingeAxisA = Axis.ToNumerics(), LocalHingeAxisB = Axis.ToNumerics(), SpringSettings = new SpringSettings(20, 1) };
        }
    }
}
