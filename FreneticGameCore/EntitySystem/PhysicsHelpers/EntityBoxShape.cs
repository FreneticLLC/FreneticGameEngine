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
    /// A box shape for an entity.
    /// </summary>
    public class EntityBoxShape : EntityShapeHelper
    {
        /// <summary>
        /// The size of the box on each axis.
        /// </summary>
        public Location Size;

        /// <summary>
        /// Gets the BEPU shape object.
        /// </summary>
        /// <returns>The BEPU shape.</returns>
        public override EntityShape GetBEPUShape()
        {
            return new BoxShape(Size.X, Size.Y, Size.Z);
        }

        /// <summary>
        /// The string form of this shape helper.
        /// </summary>
        /// <returns>String form.</returns>
        public override string ToString()
        {
            return "BoxShape, size=" + Size;
        }

        /// <summary>
        /// Converts the box shape to a byte array.
        /// </summary>
        /// <returns>The byte array.</returns>
        public byte[] ToBytes()
        {
            return Size.ToDoubleBytes();
        }

        /// <summary>
        /// Converts the byte array to box shape.
        /// </summary>
        /// <param name="b">The bytes.</param>
        /// <returns>The box.</returns>
        public static EntityBoxShape FromBytes(byte[] b)
        {
            return new EntityBoxShape()
            {
                Size = Location.FromDoubleBytes(b, 0)
            };
        }
    }
}
