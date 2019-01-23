using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BEPUphysics.CollisionShapes;
using BEPUphysics.CollisionShapes.ConvexShapes;
using BEPUutilities;
using FreneticGameCore.CoreSystems;

namespace FreneticGameCore.EntitySystem.PhysicsHelpers
{
    /// <summary>
    /// A convex hull shape for an entity.
    /// </summary>
    public class EntityConvexHullShape : EntityShapeHelper
    {
        /// <summary>
        /// The internal convex hull shape.
        /// </summary>
        public ConvexHullShape Internal;

        // TODO: Savable vertex/index set?

        /// <summary>
        /// The center offset for this shape.
        /// </summary>
        [PropertyDebuggable]
        [PropertyAutoSavable]
        public Vector3 Center;

        /// <summary>
        /// Gets the center offset.
        /// </summary>
        /// <returns>The center offset.</returns>
        public override Vector3 GetCenterOffset()
        {
            return Center;
        }

        /// <summary>
        /// Gets the BEPU shape object.
        /// </summary>
        /// <returns>The BEPU shape.</returns>
        public override EntityShape GetBEPUShape()
        {
            return Internal;
        }

        /// <summary>
        /// The string form of this shape helper.
        /// </summary>
        /// <returns>String form.</returns>
        public override string ToString()
        {
            return "ConvexHullShape";
        }
    }
}
