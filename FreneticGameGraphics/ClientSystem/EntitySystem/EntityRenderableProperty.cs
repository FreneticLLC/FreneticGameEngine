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
        /// What orientation to render the entity at.
        /// </summary>
        public Quaternion RenderOrientation = Quaternion.Identity;

        /// <summary>
        /// Fired when the entity is spawned.
        /// </summary>
        public override void OnSpawn()
        {
            base.OnSpawn();
            Entity.OnPositionChanged += FixLocation;
            Entity.OnOrientationChanged += FixOrientation;
        }

        /// <summary>
        /// Fired when the entity is de-spawned.
        /// </summary>
        public override void OnDeSpawn()
        {
            if (HandledRemove)
            {
                return;
            }
            HandledRemove = true;
            base.OnDeSpawn();
            Entity.OnPositionChanged -= FixLocation;
            Entity.OnOrientationChanged -= FixOrientation;
        }

        private bool HandledRemove = false;

        /// <summary>
        /// Handles removal event.
        /// </summary>
        public override void OnRemoved()
        {
            OnDeSpawn();
        }

        /// <summary>
        /// Fixes the location of the renderable.
        /// </summary>
        /// <param name="loc">The new location.</param>
        public void FixLocation(Location loc)
        {
            RenderAt = loc.ToOpenTK();
        }

        /// <summary>
        /// Fixes the orientation of the renderable.
        /// </summary>
        /// <param name="q">The new orientation.</param>
        public void FixOrientation(BEPUutilities.Quaternion q)
        {
            RenderOrientation = q.ToOpenTK();
        }

        /// <summary>
        /// Render the entity as seen by a top-down map.
        /// </summary>
        /// <param name="context">The render context.</param>
        public abstract void RenderForTopMap(RenderContext context);

        /// <summary>
        /// Render the entity as seen normally, in 3D.
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
