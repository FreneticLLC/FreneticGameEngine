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
using FGECore.PropertySystem;

namespace FGEGraphics.ClientSystem.EntitySystem
{
    /// <summary>
    /// Represents a 2D renderable.
    /// </summary>
    public abstract class Entity2DRenderableProperty : EntityRenderableProperty
    {
        /// <summary>
        /// Whether to automatically update the rendering priority based on Z position.
        /// </summary>
        [PropertyAutoSavable]
        [PropertyDebuggable]
        public bool AutoUpdatePriorityZ = true;

        /// <summary>
        /// Fired when the location is fixed.
        /// </summary>
        public override void OtherLocationPatch()
        {
            if (AutoUpdatePriorityZ)
            {
                RenderingPriorityOrder = RenderAt.Z;
            }
        }

        /// <summary>
        /// Gets the 2D angle to render around.
        /// Assumes the renderable is rotated only around the Z axis.
        /// <para>Setting this will broadcast an orientation update.</para>
        /// <para>MUST have a valid spawned entity for this to work!</para>
        /// </summary>
        [PropertyDebuggable]
        public float RenderAngle
        {
            get
            {
                return (float)RenderOrientation.Angle2D;
            }
            set
            {
                Entity.SetOrientation(new FGECore.MathHelpers.Quaternion() { Angle2D = value });
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
