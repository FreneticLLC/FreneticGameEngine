//
// This file is part of the Frenetic Game Engine, created by Frenetic LLC.
// This code is Copyright (C) Frenetic LLC under the terms of a strict license.
// See README.md or LICENSE.txt in the FreneticGameEngine source root for the contents of the license.
// If neither of these are available, assume that neither you nor anyone other than the copyright holder
// hold any right or permission to use this software until such time as the official license is identified.
//

using FGECore.MathHelpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace FGECore.EntitySystem.JointSystems.NonPhysicsJoints
{
    /// <summary>Special pseudo-constraint to indicate two entities shouldn't collide with each other.</summary>
    public class JointNoCollide : PhysicsJointBase
    {
        /// <summary>Constructs the <see cref="JointNoCollide"/>.</summary>
        public JointNoCollide(EntityPhysicsProperty _one, EntityPhysicsProperty _two)
        {
            One = _one;
            Two = _two;
        }

        /// <summary>Implements <see cref="GenericBaseJoint.Enable"/>.</summary>
        public override void Enable()
        {
            if (One.Internal.NoCollideIDs == null)
            {
                One.Internal.NoCollideIDs = new HashSet<long>(16);
            }
            One.Internal.NoCollideIDs.Add(Two.Entity.EID);
            if (Two.Internal.NoCollideIDs == null)
            {
                Two.Internal.NoCollideIDs = new HashSet<long>(16);
            }
            Two.Internal.NoCollideIDs.Add(One.Entity.EID);
        }

        /// <summary>Implements <see cref="GenericBaseJoint.Disable"/>.</summary>
        public override void Disable()
        {
            One.Internal.NoCollideIDs.Remove(Two.Entity.EID);
            Two.Internal.NoCollideIDs.Remove(One.Entity.EID);
        }
    }
}
