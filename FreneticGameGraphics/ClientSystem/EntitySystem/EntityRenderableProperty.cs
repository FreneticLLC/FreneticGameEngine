using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FreneticGameCore;
using OpenTK;

namespace FreneticGameGraphics.ClientSystem.EntitySystem
{
    /// <summary>
    /// Abstract helper to render an entity.
    /// </summary>
    public abstract class EntityRenderableProperty : ClientEntityProperty
    {
        /// <summary>
        /// Whether this Renderable entity should cast shadows.
        /// <para>Note: Setting this after it's added is not required to validly modify its value.</para>
        /// </summary>
        [PropertyDebuggable]
        [PropertyAutoSavable]
        public bool CastShadows = true;

        /// <summary>
        /// Whether this Renderable entity is currently visible.
        /// </summary>
        [PropertyDebuggable]
        [PropertyAutoSavable]
        public bool IsVisible = true;

        /// <summary>
        /// priority order of rendering: lower means sooner in the rendering order.
        /// </summary>
        [PropertyDebuggable]
        [PropertyAutoSavable]
        public int RenderingPriorityOrder = 0;

        /// <summary>
        /// Where the entity should render at.
        /// </summary>
        [PropertyDebuggable]
        [PropertyAutoSavable]
        public Vector3 RenderAt;
        
        /// <summary>
        /// Render the entity as seen by a top-down map.
        /// </summary>
        /// <param name="context">The render context.</param>
        public abstract void RenderForTopMap(RenderContext context);

        /// <summary>
        /// Render the entity as seen normally.
        /// </summary>
        /// <param name="context">The render context.</param>
        public abstract void RenderStandard(RenderContext context);

        /// <summary>
        /// Render the entity as seen normally, in 2D.
        /// </summary>
        /// <param name="context">The render context.</param>
        public abstract void RenderStandard2D(RenderContext2D context);
    }
}
