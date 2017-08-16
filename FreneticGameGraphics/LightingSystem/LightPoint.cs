using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FreneticGameCore;
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
