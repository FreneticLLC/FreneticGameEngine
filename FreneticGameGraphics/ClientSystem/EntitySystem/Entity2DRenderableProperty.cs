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
using OpenTK;
using FreneticGameCore;

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
        /// Setting this will broadcast an orientation update.
        /// </summary>
        public float RenderAngle
        {
            get
            {
                return (float)RenderOrientation.ToBEPU().AxisAngleFor(BEPUutilities.Vector3.UnitZ);
            }
            set
            {
                BEPUutilities.Quaternion quat = BEPUutilities.Quaternion.CreateFromAxisAngle(BEPUutilities.Vector3.UnitZ, value);
                RenderOrientation = quat.ToOpenTK();
                Entity?.OnOrientationChanged?.Invoke(quat);
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
