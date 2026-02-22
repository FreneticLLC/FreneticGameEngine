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

namespace FGECore.EntitySystem.JointSystems;

/// <summary>The base class for all non-physics-based joints.</summary>
public abstract class NonPhysicalJointBase(BasicEntity _one, BasicEntity _two) : GenericBaseJoint
{

    /// <summary>The applicable entities for this joint.</summary>
    public BasicEntity One = _one, Two = _two;

    /// <summary>Implements <see cref="GenericBaseJoint.EntityOne"/>.</summary>
    public override BasicEntity EntityOne => One;

    /// <summary>Implements <see cref="GenericBaseJoint.EntityTwo"/>.</summary>
    public override BasicEntity EntityTwo => Two;

    /// <summary>Calculate the joint's logic and apply it.</summary>
    public abstract void Solve();

    /// <inheritdoc/>
    public override void Enable()
    {
    }

    /// <inheritdoc/>
    public override void Disable()
    {
    }
}
