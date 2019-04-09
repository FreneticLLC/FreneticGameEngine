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
using FGECore;
using FGECore.CoreSystems;
using FGECore.MathHelpers;

namespace FGEGraphics.LightingSystem
{
    /// <summary>
    /// Represents any 3D light source.
    /// </summary>
    public abstract class LightObject
    {
        /// <summary>
        /// The actual lights making up this light.
        /// </summary>
        public List<Light> InternalLights = new List<Light>();

        /// <summary>
        /// The position of the light source.
        /// </summary>
        public Location EyePos;

        /// <summary>
        /// The maximum radius this light can affect.
        /// </summary>
        public float MaxDistance;

        /// <summary>
        /// Repositions the light.
        /// </summary>
        /// <param name="pos">Position.</param>
        public abstract void Reposition(Location pos);
    }
}
