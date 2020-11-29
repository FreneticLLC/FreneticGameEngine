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
using OpenTK;
using OpenTK.Mathematics;
using FGEGraphics.ClientSystem.ViewRenderSystem;

namespace FGEGraphics.LightingSystem
{
    /// <summary>
    /// Represents an orthographic light.
    /// </summary>
    public class LightOrtho : Light
    {
        /// <summary>
        /// Gets the matrix of the light.
        /// </summary>
        /// <param name="view">The relevant view system.</param>
        /// <returns>The relevant matrix.</returns>
        public override Matrix4 GetMatrix(View3D view)
        {
            Vector3d c = view.State.RenderRelative.ToOpenTK3D();
            Vector3d e = EyePosition - c;
            Vector3d d = TargetPosition - c;
            return Matrix4.LookAt(new Vector3((float)e.X, (float)e.Y, (float)e.Z), new Vector3((float)d.X, (float)d.Y, (float)d.Z), UpVector) * Matrix4.CreateOrthographicOffCenter(-FOV * 0.5f, FOV * 0.5f, -FOV * 0.5f, FOV * 0.5f, 1, MaxRange);
        }
    }
}
