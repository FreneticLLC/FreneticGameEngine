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
        /// Returns the next power of two.
        /// Meaning, the next number in the sequence:
        /// 2, 4, 8, 16, 32, 64, 128, 256, 512, 1024, 2048, 4096, 8192, 16384, ...
        /// Result is >= input.
        /// </summary>
        /// <param name="x">The value, less than or equal to the result.</param>
        /// <returns>The result, greater than or equal to the value.</returns>
        public static int NextPowerOfTwo(int x)
        {
            Debug.Assert(x > 0, $"For NextPowerOfTwo, X must be > 0, but was {x}");
            // Spread the Most Significant Bit all the way down
            // so eg "00100100" becomes "11111100"
            int spreadMSB = x | (x >> 1);
            spreadMSB |= spreadMSB >> 2;
            spreadMSB |= spreadMSB >> 4;
            spreadMSB |= spreadMSB >> 8;
            spreadMSB |= spreadMSB >> 16;
            // Full value minus the downshift of it = *only* the MSB
            int onlyMSB = spreadMSB - (spreadMSB >> 1);
            // Exactly on MSB = return that, otherwise we're greater so grow by one.
            if (x == onlyMSB)
            {
                return onlyMSB;
            }
            return onlyMSB << 1;
        }

        /// <summary>
        /// Steps a value towards a goal by a specified amount, automatically moving the correct direction (positive or negative) and preventing going past the goal.
        /// </summary>
        /// <param name="start">The initial value.</param>
        /// <param name="target">The goal value.</param>
        /// <param name="amount">The amount to step by.</param>
        /// <returns>The result.</returns>
        public static double StepTowards(double start, double target, double amount)
        {
            if (start < target - amount)
            {
                return start + amount;
            }
            else if (start > target + amount)
            {
                return start - amount;
            }
            else
            {
                return target;
            }
        }

        /// <summary>
        /// Returns whether a number is close to another number, within a specified range.
        /// </summary>
        /// <param name="one">The first number.</param>
        /// <param name="target">The second number.</param>
        /// <param name="amount">The range.</param>
        /// <returns>Whether it's close.</returns>
        public static bool IsCloseTo(double one, double target, double amount)
        {
            return Math.Abs(one - target) < amount;
        }

        /// <summary>
        /// Represents the constant PI / 180.
        /// </summary>
        public const double PI180 = Math.PI / 180.0;

        /// <summary>
        /// Returns a one-length vector of the Yaw/Pitch angle input.
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
        /// Returns a one-length vector of the Yaw/Pitch angle input in degrees
        /// </summary>
        /// <param name="yaw">The yaw angle, in radians.</param>
        /// <param name="pitch">The pitch angle, in radians.</param>
        /// <returns>.</returns>
        public static Location ForwardVector_Deg(double yaw, double pitch)
        {
            return ForwardVector(yaw * PI180, pitch * PI180);
        }

        /// <summary>
        /// Rotates a vector by a certain yaw.
        /// </summary>
        /// <param name="vec">The original vector.</param>
        /// <param name="yaw">The yaw to rotate by.</param>
        /// <returns>The rotated vector.</returns>
        public static Location RotateVector(Location vec, double yaw)
        {
            double cos = Math.Cos(yaw);
            double sin = Math.Sin(yaw);
            return new Location((vec.X * cos) - (vec.Y * sin), (vec.X * sin) + (vec.Y * cos), vec.Z);
        }

        /// <summary>
        /// Rotates a vector by a certain yaw and pitch.
        /// </summary>
        /// <param name="vec">The original vector.</param>
        /// <param name="yaw">The yaw to rotate by.</param>
        /// <param name="pitch">The pitch to rotate by.</param>
        /// <returns>The rotated vector.</returns>
        public static Location RotateVector(Location vec, double yaw, double pitch)
        {
            double cosyaw = Math.Cos(yaw);
            double cospitch = Math.Cos(pitch);
            double sinyaw = Math.Sin(yaw);
            double sinpitch = Math.Sin(pitch);
            double bX = vec.Z * sinpitch + vec.X * cospitch;
            double bZ = vec.Z * cospitch - vec.X * sinpitch;
            return new Location(bX * cosyaw - vec.Y * sinyaw, bX * sinyaw + vec.Y * cosyaw, bZ);
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
        /// Converts a forward vector to yaw/pitch angles.
        /// </summary>
        /// <param name="input">The forward vector.</param>
        /// <returns>The yaw/pitch angle vector.</returns>
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
