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
using System.Diagnostics;

namespace FGECore.MathHelpers
{
    /// <summary>
    /// Common utilities relating to mathematics.
    /// </summary>
    public static class MathUtilities
    {
        /// <summary>
        /// Represents the constant PI / 180. (Which is the conversion from Degrees to Radians).
        /// </summary>
        public const double PI180 = Math.PI / 180.0;

        /// <summary>
        /// Returns a one-length vector of the Yaw/Pitch angle input (in radians).
        /// </summary>
        /// <param name="yaw">The yaw angle, in radians.</param>
        /// <param name="pitch">The pitch angle, in radians.</param>
        /// <returns>.</returns>
        public static Location ForwardVector(double yaw, double pitch)
        {
            double cp = Math.Cos(pitch);
            return new Location(-(cp * Math.Cos(yaw)), -(cp * Math.Sin(yaw)), (Math.Sin(pitch)));
        }

        /// <summary>
        /// Returns a one-length vector of the Yaw/Pitch angle input in degrees.
        /// </summary>
        /// <param name="yaw">The yaw angle, in radians.</param>
        /// <param name="pitch">The pitch angle, in radians.</param>
        /// <returns>.</returns>
        public static Location ForwardVectorDegrees(double yaw, double pitch)
        {
            return ForwardVector(yaw * PI180, pitch * PI180);
        }

        /// <summary>
        /// Converts a forward vector to a yaw angle, in radians.
        /// </summary>
        /// <param name="input">The forward vector.</param>
        /// <returns>The yaw angle.</returns>
        public static double VectorToAnglesYawRad(Location input)
        {
            if (input.X == 0 && input.Y == 0)
            {
                return 0;
            }
            if (input.X != 0)
            {
                return Math.Atan2(input.Y, input.X);
            }
            if (input.Y > 0)
            {
                return 0;
            }
            return Math.PI;
        }

        /// <summary>
        /// Converts a forward vector to yaw/pitch angles, in degrees.
        /// </summary>
        /// <param name="input">The forward vector.</param>
        /// <returns>The yaw/pitch angle vector (in degrees).</returns>
        public static Location VectorToAngles(Location input)
        {
            if (input.X == 0 && input.Y == 0)
            {
                if (input.Z > 0)
                {
                    return new Location(0, 0, 0);
                }
                else
                {
                    return new Location(0, 180, 0);
                }
            }
            else
            {
                double yaw;
                double pitch;
                if (input.X != 0)
                {
                    yaw = (Math.Atan2(input.Y, input.X) * (180.0 / Math.PI));
                }
                else if (input.Y > 0)
                {
                    yaw = 0;
                }
                else
                {
                    yaw = 180;
                }
                pitch = (Math.Atan2(input.Z, Math.Sqrt(input.X * input.X + input.Y * input.Y)) * (180.0 / Math.PI));
                while (pitch < -180)
                {
                    pitch += 360;
                }
                while (pitch > 180)
                {
                    pitch -= 360;
                }
                while (yaw < 0)
                {
                    yaw += 360;
                }
                while (yaw > 360)
                {
                    yaw -= 360;
                }
                Location loc = new Location()
                {
                    Yaw = yaw,
                    Pitch = pitch
                };
                return loc;
            }
        }
    }
}
