using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;

namespace FreneticGameGraphics.ClientSystem.EntitySystem
{
    /// <summary>
    /// Represents a 2D renderable.
    /// </summary>
    public abstract class Entity2DRenderableProperty : EntityRenderableProperty
    {
        /// <summary>
        /// Gets the 2D angle to render around.
        /// Assumes the renderable is rotated only around the Z axis.
        /// </summary>
        public float RenderAngle
        {
            get
            {
                RenderOrientation.ToAxisAngle(out Vector3 axis, out float ang);
                if (axis.Z < 0)
                {
                    return -ang;
                }
                return ang;
            }
        }

        /// <summary>
        /// Render the entity as seen by a top-down map.
        /// </summary>
        /// <param name="context">The render context.</param>
        public override void RenderForTopMap(RenderContext context)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Render the entity as seen normally.
        /// </summary>
        /// <param name="context">The render context.</param>
        public override void RenderStandard(RenderContext context)
        {
            throw new NotImplementedException();
        }
    }
}
