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
using FreneticGameCore.MathHelpers;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;

namespace FreneticGameGraphics.LightingSystem
{
    /// <summary>
    /// Represents a 3D point light.
    /// </summary>
    class LightPoint : Light
    {
        /// <summary>
        /// Sets up the 3D point light.
        /// </summary>
        /// <param name="pos">Position.</param>
        /// <param name="targ">Target.</param>
        /// <param name="fov">Field of view.</param>
        /// <param name="max_range">Maximum range.</param>
        /// <param name="col">Color.</param>
        public void Setup(Vector3d pos, Vector3d targ, float fov, float max_range, Vector3 col)
        {
            eye = pos;
            target = targ;
            FOV = fov;
            maxrange = max_range;
            color = col;
        }
    }
}
