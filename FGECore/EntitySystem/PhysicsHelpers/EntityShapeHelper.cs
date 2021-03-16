//
// This file is part of the Frenetic Game Engine, created by Frenetic LLC.
// This code is Copyright (C) Frenetic LLC under the terms of a strict license.
// See README.md or LICENSE.txt in the FreneticGameEngine source root for the contents of the license.
// If neither of these are available, assume that neither you nor anyone other than the copyright holder
// hold any right or permission to use this software until such time as the official license is identified.
//

using BepuPhysics;
using BepuPhysics.Collidables;
using FGECore.PhysicsSystem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace FGECore.EntitySystem.PhysicsHelpers
{
    /// <summary>Implementations of this class are helpers for the various possible entity physics shapes.</summary>
    public abstract class EntityShapeHelper
    {
        /// <summary>Gets the BEPU shape index.</summary>
        public TypedIndex ShapeIndex;

        /// <summary>Gets the BEPU convex shape (if possible).</summary>
        public IConvexShape BepuShape;

        /// <summary>
        /// Gets the BEPU Shape offset, if any.
        /// </summary>
        /// <returns>The shape offset, or Zero if none.</returns>
        public virtual Vector3 GetCenterOffset()
        {
            return Vector3.Zero;
        }
    }
}
