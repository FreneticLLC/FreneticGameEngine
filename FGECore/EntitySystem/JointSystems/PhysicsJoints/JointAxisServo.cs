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
    /// <summary>Constrains two entities to remain on a shared plane using a servo.</summary>
    class JointAxisServo : PhysicsJointBase<LinearAxisServo>
    {
        /// <summary>Constructs the <see cref="JointAxisServo"/>.</summary>
        public JointAxisServo(EntityPhysicsProperty e1, EntityPhysicsProperty e2, Location axis) : base(e1, e2)
        {
            Axis = axis;
        }

        /// <summary>The plane normal axis.</summary>
        public Location Axis;

        /// <summary>Implements <see cref="PhysicsJointBase{T}.CreateJointDescription"/>.</summary>
        public override LinearAxisServo CreateJointDescription()
        {
            return new LinearAxisServo() { LocalPlaneNormal = Axis.ToNumerics(), SpringSettings = new SpringSettings(20, 1), ServoSettings = ServoSettings.Default, TargetOffset = 1f };
        }
    }
}
