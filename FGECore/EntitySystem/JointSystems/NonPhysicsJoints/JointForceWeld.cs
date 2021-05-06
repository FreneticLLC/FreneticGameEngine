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

namespace FGECore.EntitySystem.JointSystems.NonPhysicsJoints
{
    /// <summary>Constrains entity Two to be force-welded to entity One, meaning that wherever One moves, Two will follow.</summary>
    public class JointForceWeld : NonPhysicalJointBase
    {
        /// <summary>Constructs the <see cref="JointForceWeld"/>.</summary>
        public JointForceWeld(BasicEntity e1, BasicEntity e2) : base(e1, e2)
        {
            Matrix4x4 worldTrans = Matrix4x4.CreateFromQuaternion(One.LastKnownOrientation.ToNumerics()) * Matrix4x4.CreateTranslation(One.LastKnownPosition.ToNumerics());
            Matrix4x4.Invert(worldTrans, out worldTrans);
            Relative = (Matrix4x4.CreateFromQuaternion(Two.LastKnownOrientation.ToNumerics()) * Matrix4x4.CreateTranslation(Two.LastKnownPosition.ToNumerics())) * worldTrans;
        }

        /// <summary>The relative matrix from entity One to entity Two.</summary>
        public Matrix4x4 Relative;

        /// <summary>Implements <see cref="NonPhysicalJointBase.Solve"/>.</summary>
        public override void Solve()
        {
            Matrix4x4 worldTrans = Matrix4x4.CreateFromQuaternion(One.LastKnownOrientation.ToNumerics()) * Matrix4x4.CreateTranslation(One.LastKnownPosition.ToNumerics());
            Matrix4x4 tmat = Relative * worldTrans;
            Two.SetPosition(tmat.Translation.ToLocation());
            Two.SetOrientation(System.Numerics.Quaternion.CreateFromRotationMatrix(tmat).ToCore());
        }
    }
}
