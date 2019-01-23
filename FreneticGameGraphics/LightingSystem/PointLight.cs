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
using FreneticGameCore;
using FreneticGameCore.CoreSystems;
using FreneticGameCore.MathHelpers;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;

namespace FreneticGameGraphics.LightingSystem
{
    /// <summary>
    /// Represents a 3D point light.
    /// </summary>
    public class PointLight : LightObject
    {
        /// <summary>
        /// Radius of the light.
        /// </summary>
        float Radius;

        /// <summary>
        /// Color of the light.
        /// </summary>
        Color3F Color;

        /// <summary>
        /// Gets whether it should cast shadows.
        /// </summary>
        public bool CastShadows = true;

        /// <summary>
        /// Sets whether it should cast shadows properly.
        /// </summary>
        /// <param name="shad">Shadow cast mode.</param>
        public void SetCastShadows(bool shad)
        {
            CastShadows = shad;
            for (int i = 0; i < 6; i++)
            {
                InternalLights[i].CastShadows = shad;
            }
        }

        /// <summary>
        /// Constructs the point light.
        /// </summary>
        /// <param name="pos">The position.</param>
        /// <param name="radius">The radius.</param>
        /// <param name="col">The color.</param>
        public PointLight(Location pos, float radius, Color3F col)
        {
            EyePos = pos;
            Radius = radius;
            Color = col;
            for (int i = 0; i < 6; i++)
            {
                Light li = new Light();
                li.Create(pos.ToOpenTK3D(), (pos + Location.UnitX).ToOpenTK3D(), 90f, Radius, Color.ToOpenTK());
                InternalLights.Add(li);
            }
            InternalLights[4].up = new Vector3(0, 1, 0);
            InternalLights[5].up = new Vector3(0, 1, 0);
            Reposition(EyePos);
            MaxDistance = radius;
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
        }

        /// <summary>
        /// Destroys the light.
        /// </summary>
        public void Destroy()
        {
        }

        /// <summary>
        /// Repositions the light.
        /// </summary>
        /// <param name="pos">The new position.</param>
        public sealed override void Reposition(Location pos)
        {
            EyePos = pos;
            for (int i = 0; i < 6; i++)
            {
                InternalLights[i].NeedsUpdate = true;
                InternalLights[i].eye = EyePos.ToOpenTK3D();
            }
            InternalLights[0].target = (EyePos + new Location(1, 0, 0)).ToOpenTK3D();
            InternalLights[1].target = (EyePos + new Location(-1, 0, 0)).ToOpenTK3D();
            InternalLights[2].target = (EyePos + new Location(0, 1, 0)).ToOpenTK3D();
            InternalLights[3].target = (EyePos + new Location(0, -1, 0)).ToOpenTK3D();
            InternalLights[4].target = (EyePos + new Location(0, 0, 1)).ToOpenTK3D();
            InternalLights[5].target = (EyePos + new Location(0, 0, -1)).ToOpenTK3D();
        }
    }
}
