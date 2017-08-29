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

namespace FreneticGameGraphics.ClientSystem
{
    /// <summary>
    /// Represents a camera in 3D space.
    /// </summary>
    public class Camera3D
    {
        /// <summary>
        /// The position in 3D space of the camera.
        /// </summary>
        public Location Position = Location.Zero;

        /// <summary>
        /// The direction the camera is facing... keep normalized!
        /// </summary>
        public Location Direction = Location.UnitX;

        /// <summary>
        /// The up vector of the camera... keep normalized!
        /// </summary>
        public Location Up = Location.UnitZ;

        /// <summary>
        /// Gets the sideways direction for this camera.
        /// </summary>
        public Location Side
        {
            get
            {
                return Direction.CrossProduct(Up);
            }
        }

        /// <summary>
        /// The Z-Near value of the camera.
        /// </summary>
        public float ZNear = 0.1f;

        /// <summary>
        /// The Z-Far value of the camera.
        /// </summary>
        public float ZFar = 1000f;
    }
}
