//
// This file is part of the Frenetic Game Engine, created by Frenetic LLC.
// This code is Copyright (C) Frenetic LLC under the terms of a strict license.
// See README.md or LICENSE.txt in the FreneticGameEngine source root for the contents of the license.
// If neither of these are available, assume that neither you nor anyone other than the copyright holder
// hold any right or permission to use this software until such time as the official license is identified.
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FGECore.EntitySystem;
using FGECore.MathHelpers;
using FGECore.PropertySystem;

namespace FGEGraphics.ClientSystem.EntitySystem
{
    /// <summary>Abstract helper to render an entity.</summary>
    public abstract class EntityRenderableProperty : ClientEntityProperty
    {
        /// <summary>
        /// Whether this Renderable entity should cast shadows.
        /// <para>Note: Setting this after it's added is not required to validly modify its value.</para>
        /// </summary>
        [PropertyDebuggable]
        [PropertyAutoSavable]
        public bool CastShadows = true;

        /// <summary>Whether to ONLY RENDER as a shadow.</summary>
        [PropertyDebuggable]
        [PropertyAutoSavable]
        public bool ShadowsOnly = false;

        /// <summary>Whether this Renderable entity is currently visible.</summary>
        [PropertyDebuggable]
        [PropertyAutoSavable]
        public bool IsVisible = true;

        /// <summary>
        /// priority order of rendering: lower means sooner in the rendering order. This means higher numbers appear "on top" of lower numbers.
        /// <para>For 2D entities, this is by default automatically set to match the Z coordinate if <see cref="Entity2DRenderableProperty.AutoUpdatePriorityZ"/> is set.</para>
        /// <para>For 3D entities, this is by default ignored in favor of sorting by distance from camera (to ensure transparents render logically).</para>
        /// </summary>
        [PropertyDebuggable]
        [PropertyAutoSavable]
        public double RenderingPriorityOrder = 0;

        /// <summary>
        /// Where the entity should render at.
        /// <para>Use <see cref="BasicEntity.SetPosition(Location)"/> to update this.</para>
        /// </summary>
        [PropertyDebuggable]
        public Location RenderAt
        {
            get
            {
                return Entity.LastKnownPosition;
            }
        }

        /// <summary>
        /// What orientation to render the entity at.
        /// <para>Use <see cref="BasicEntity.SetOrientation(Quaternion)"/> to update this.</para>
        /// </summary>
        [PropertyDebuggable]
        public Quaternion RenderOrientation
        {
            get
            {
                return Entity.LastKnownOrientation;
            }
        }

        /// <summary>Fired when the entity is spawned.</summary>
        public override void OnSpawn()
        {
            Entity.OnPositionChanged += FixLocation;
            Entity.OnOrientationChanged += FixOrientation;
            HandledRemove = false;
        }

        /// <summary>Fired when the entity is de-spawned.</summary>
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

        /// <summary>Whether the removal was already handled.</summary>
        private bool HandledRemove = false;

        /// <summary>Handles removal event.</summary>
        public override void OnRemoved()
        {
            OnDespawn();
        }

        /// <summary>Fixes the location of the renderable.</summary>
        /// <param name="location">The new location.</param>
        public void FixLocation(Location location)
        {
            OtherLocationPatch();
        }

        /// <summary>Fired when the location is fixed.</summary>
        public virtual void OtherLocationPatch()
        {
        }

        /// <summary>Fixes the orientation of the renderable.</summary>
        /// <param name="orientation">The new orientation.</param>
        public void FixOrientation(Quaternion orientation)
        {
            OtherOrientationPatch();
        }

        /// <summary>Fired when the orientation is fixed.</summary>
        public virtual void OtherOrientationPatch()
        {
        }

        /// <summary>Render the entity as seen by a top-down map.</summary>
        /// <param name="context">The render context.</param>
        public abstract void RenderForTopMap(RenderContext context);

        /// <summary>Render the entity as seen normally, in 3D.</summary>
        /// <param name="context">The render context.</param>
        public abstract void RenderStandard(RenderContext context);

        /// <summary>Render the entity as seen normally, in 2D.</summary>
        /// <param name="context">The render context.</param>
        public abstract void RenderStandard2D(RenderContext2D context);
    }
}
