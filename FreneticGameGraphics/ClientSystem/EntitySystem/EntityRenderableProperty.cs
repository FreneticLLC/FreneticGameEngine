using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FreneticGameCore;

namespace FreneticGameGraphics.ClientSystem.EntitySystem
{
    /// <summary>
    /// Abstract helper to render an entity.
    /// </summary>
    public abstract class EntityRenderableProperty : Property
    {
        /// <summary>
        /// Whether this Renderable entity should cast shadows.
        /// </summary>
        [PropertyDebuggable]
        public readonly bool CastShadows;

        /// <summary>
        /// Constructs the entity renderable property.
        /// </summary>
        /// <param name="_castShadows">Whether the entity should cast shadows.</param>
        public EntityRenderableProperty(bool _castShadows)
        {
            CastShadows = _castShadows;
        }

        /// <summary>
        /// Render the entity as seen by a top-down map.
        /// </summary>
        public abstract void RenderForTopMap(RenderContext context);
        
        /// <summary>
        /// Render the entity as seen normally.
        /// </summary>
        public abstract void RenderStandard(RenderContext context);
    }
}
