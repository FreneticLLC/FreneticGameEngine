using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BEPUphysics.CollisionShapes;

namespace FreneticGameCore.EntitySystem.PhysicsHelpers
{
    /// <summary>
    /// Implementations of this class are helpers for the various possible entity physics shapes.
    /// </summary>
    public abstract class EntityShapeHelper
    {
        /// <summary>
        /// Gets the BEPU shape object.
        /// </summary>
        /// <returns>The BEPU shape.</returns>
        public abstract EntityShape GetBEPUShape();

        /// <summary>
        /// The string form of this shape helper.
        /// </summary>
        /// <returns>String form.</returns>
        public override string ToString()
        {
            return "EntityShapeHelper / Undefined";
        }
    }
}
