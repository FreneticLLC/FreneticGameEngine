using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FreneticGameCore;

namespace FreneticGameGraphics.LightingSystem
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
