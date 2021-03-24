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
    /// <summary>Constrains two bodies to be perfectly glued together.</summary>
    public class JointWeld : PhysicsJointBase<Weld>
    {
        /// <summary>Constructs the <see cref="JointWeld"/>.</summary>
        public JointWeld(EntityPhysicsProperty e1, EntityPhysicsProperty e2) : base(e1, e2)
        {
        }

        /// <summary>Implements <see cref="PhysicsJointBase{T}.CreateJointDescription"/>.</summary>
        public override Weld CreateJointDescription()
        {
            return new Weld() { LocalOffset = (One.Position - Two.Position).ToNumerics(), LocalOrientation = Quaternion.GetQuaternionBetween(One.Orientation, Two.Orientation).ToNumerics(), SpringSettings = new SpringSettings(20, 1) };
        }
    }
}
