using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FreneticGameCore;
using OpenTK;

namespace FreneticGameGraphics.GraphicsHelpers
{
    /// <summary>
    /// Represents a graphical "decal" effect.
    /// </summary>
    public class DecalInfo
    {
        /// <summary>
        /// The 3D space position of the decal.
        /// </summary>
        public Location Position;

        /// <summary>
        /// The decal's normal direction vector.
        /// </summary>
        public Vector3 NormalDirection;

        /// <summary>
        /// The color of the decal.
        /// </summary>
        public Vector4 Color;

        /// <summary>
        /// The scale, in spacial units, of the decal.
        /// </summary>
        public float Scale;

        /// <summary>
        /// The decal texture ID.
        /// </summary>
        public int TextureDecalID;

        /// <summary>
        /// The time left before the decal is removed.
        /// </summary>
        public double RemainingTime;
    }
}
