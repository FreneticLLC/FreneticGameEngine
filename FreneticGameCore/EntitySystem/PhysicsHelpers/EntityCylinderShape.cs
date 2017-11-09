//
// This file is created by Frenetic LLC.
// This code is Copyright (C) 2017 Frenetic LLC under the terms of a strict license.
// See README.md or LICENSE.txt in the source root for the contents of the license.
// If neither of these are available, assume that neither you nor anyone other than the copyright holder
// hold any right or permission to use this software until such time as the official license is identified.
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BEPUphysics.CollisionShapes;
using BEPUphysics.CollisionShapes.ConvexShapes;

namespace FreneticGameCore.EntitySystem.PhysicsHelpers
{
    /// <summary>
    /// A cylinder shape for an entity.
    /// <para>This will be Y-Up by default, might be beneficial to use <see cref="BasicEntity{T, T2}.RotateYToZ"/>.</para>
    /// </summary>
    public class EntityCylinderShape : EntityShapeHelper
    {
        /// <summary>
        /// The height of the cylinder.
        /// </summary>
        [PropertyDebuggable]
        [PropertyAutoSavable]
        public double Height;

        /// <summary>
        /// The radius of the cylinder.
        /// </summary>
        [PropertyDebuggable]
        [PropertyAutoSavable]
        public double Radius;

        /// <summary>
        /// Gets the BEPU shape object.
        /// </summary>
        /// <returns>The BEPU shape.</returns>
        public override EntityShape GetBEPUShape()
        {
            return new CylinderShape(Height, Radius);
        }

        /// <summary>
        /// The string form of this shape helper.
        /// </summary>
        /// <returns>String form.</returns>
        public override string ToString()
        {
            return "CylinderShape, Radius=" + Radius + ", Height=" + Height;
        }
    }
}
