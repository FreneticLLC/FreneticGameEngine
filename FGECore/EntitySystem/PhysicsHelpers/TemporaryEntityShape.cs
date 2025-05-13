//
// This file is part of the Frenetic Game Engine, created by Frenetic LLC.
// This code is Copyright (C) Frenetic LLC under the terms of a strict license.
// See README.md or LICENSE.txt in the FreneticGameEngine source root for the contents of the license.
// If neither of these are available, assume that neither you nor anyone other than the copyright holder
// hold any right or permission to use this software until such time as the official license is identified.
//

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using BepuPhysics;
using BepuPhysics.Collidables;
using BepuUtilities.Memory;

namespace FGECore.EntitySystem.PhysicsHelpers;

/// <summary>Special helper to hold a temporary entity shape value, for convenience.</summary>
public class TemporaryEntityShape : EntityShapeHelper
{
    /// <summary>Construct the temporary holder.</summary>
    public TemporaryEntityShape(IShape shape, TypedIndex shapeIndex) : base(null)
    {
        BepuShape = shape;
        ShapeIndex = shapeIndex;
    }

    /// <summary>Not valid for this type.</summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public override EntityShapeHelper Register() => throw new NotImplementedException();

    /// <summary>Not valid for this type.</summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public override void Sweep<TSweepHitHandler>(in Simulation simulation, in Vector3 pos, in BodyVelocity velocity, float maximumT, BufferPool pool, ref TSweepHitHandler hitHandler) => throw new NotImplementedException();
}
