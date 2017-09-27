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
        /// priority order of rendering: lower means sooner in the rendering order. This means higher numbers appear "on top" of lower numbers.
        /// <para>For 2D entities, this is by default automatically set to match the Z coordinate if <see cref="Entity2DRenderableProperty.AutoUpdatePriorityZ"/> is set.</para>
        /// </summary>
        [PropertyDebuggable]
        [PropertyAutoSavable]
        public double RenderingPriorityOrder = 0;

        /// <summary>
        /// Where the entity should render at.
        /// </summary>
        [PropertyDebuggable]
        [PropertyAutoSavable]
        public Location RenderAt;

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
            HandledRemove = false;
        }

        /// <summary>
        /// Fired when the entity is de-spawned.
        /// </summary>
        public override void OnDespawn()
        {
            if (HandledRemove)
            {
                return;
            }
            HandledRemove = true;
            base.OnDespawn();
            Entity.OnPositionChanged -= FixLocation;
            Entity.OnOrientationChanged -= FixOrientation;
        }

        /// <summary>
        /// Whether the removal was already handled.
        /// </summary>
        private bool HandledRemove = false;

        /// <summary>
        /// Handles removal event.
        /// </summary>
        public override void OnRemoved()
        {
            OnDespawn();
        }

        /// <summary>
        /// Fixes the location of the renderable.
        /// </summary>
        /// <param name="loc">The new location.</param>
        public void FixLocation(Location loc)
        {
            RenderAt = loc;
            OtherLocationPatch();
        }

        /// <summary>
        /// Fired when the location is fixed.
        /// </summary>
        public virtual void OtherLocationPatch()
        {
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
