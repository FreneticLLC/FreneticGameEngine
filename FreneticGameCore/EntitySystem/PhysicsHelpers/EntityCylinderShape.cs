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
using FreneticGameCore.UtilitySystems;
using FreneticGameCore.PhysicsSystem;
using FreneticGameCore.MathHelpers;
using FreneticGameCore.CoreSystems;

namespace FreneticGameCore.EntitySystem.PhysicsHelpers
{
    /// <summary>
    /// A cylinder shape for an entity.
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
        /// Whether to auto fix the orientation of the cylinder.
        /// <para>This will be Y-Up if false, might be beneficial to use <see cref="BasicEntity{T, T2}.RotateYToZ"/>.</para>
        /// </summary>
        [PropertyDebuggable]
        [PropertyAutoSavable]
        public bool FixedOrientation = true;

        /// <summary>
        /// Used with <see cref="FixedOrientation"/>.
        /// </summary>
        private static Quaternion Y2Z = Quaternion.GetQuaternionBetween(Location.UnitY, Location.UnitZ);

        /// <summary>
        /// Gets the BEPU shape object.
        /// </summary>
        /// <returns>The BEPU shape.</returns>
        public override EntityShape GetBEPUShape()
        {
            if (FixedOrientation)
            {
                return new CompoundShape(new CompoundShapeEntry[] { new CompoundShapeEntry(new CylinderShape(Height, Radius), Y2Z.ToBEPU()) });
            }
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
