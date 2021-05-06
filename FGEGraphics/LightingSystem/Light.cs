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
using FGEGraphics.ClientSystem.ViewRenderSystem;
using OpenTK;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace FGEGraphics.LightingSystem
{
    /// <summary>
    /// Represents a 3D light source.
    /// </summary>
    public class Light
    {
        /// <summary>
        /// The light's position.
        /// </summary>
        public Vector3d EyePosition;

        /// <summary>
        /// The light's target.
        /// </summary>
        public Vector3d TargetPosition;

        /// <summary>
        /// The light's up vector.
        /// </summary>
        public Vector3 UpVector = Vector3.UnitZ;

        /// <summary>
        /// The light's field-of-view.
        /// </summary>
        public float FOV;

        /// <summary>
        /// The maximum range (Effective distance) of the light.
        /// </summary>
        public float MaxRange;

        /// <summary>
        /// The color of the light.
        /// </summary>
        public Vector3 Color;

        /// <summary>
        /// Whether this light needs an update.
        /// </summary>
        public bool NeedsUpdate = true;

        /// <summary>
        /// Whether transparents cast shadows.
        /// </summary>
        public bool TransparentShadows = false;

        /// <summary>
        /// Whether anything casts shadows.
        /// </summary>
        public bool CastShadows = true;

        /// <summary>
        /// Creates the light object.
        /// </summary>
        /// <param name="pos">Position.</param>
        /// <param name="targ">Target.</param>
        /// <param name="fov">Field of view.</param>
        /// <param name="max_range">Range.</param>
        /// <param name="col">Color.</param>
        public void Create(Vector3d pos, Vector3d targ, float fov, float max_range, Vector3 col)
        {
            EyePosition = pos;
            TargetPosition = targ;
            FOV = fov;
            MaxRange = max_range;
            Color = col;
        }

        /// <summary>
        /// Destroys the light object.
        /// </summary>
        public void Destroy()
        {
        }

        /// <summary>
        /// Sets the projection for rendering light shadows.
        /// </summary>
        /// <param name="view">The relevant view system.</param>
        public void SetProj(View3D view)
        {
            Matrix4 mat = GetMatrix(view);
            GL.UniformMatrix4(1, false, ref mat);
        }

        /// <summary>
        /// Gets the matrix of the light.
        /// </summary>
        /// <param name="view">The relevant view system.</param>
        /// <returns>The relevant matrix.</returns>
        public virtual Matrix4 GetMatrix(View3D view)
        {
            Vector3d c = view.State.RenderRelative.ToOpenTK3D();
            Vector3d e = EyePosition - c;
            Vector3d d = TargetPosition - c;
            return Matrix4.LookAt(new Vector3((float)e.X, (float)e.Y, (float)e.Z), new Vector3((float)d.X, (float)d.Y, (float)d.Z), UpVector) *
                Matrix4.CreatePerspectiveFieldOfView(FOV * (float)Math.PI / 180f, 1, 0.1f, MaxRange);
        }
    }
}
