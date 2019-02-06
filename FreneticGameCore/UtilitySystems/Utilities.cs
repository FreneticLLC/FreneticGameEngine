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
using System.Threading;
using FreneticUtilities.FreneticExtensions;
using FreneticGameCore.MathHelpers;
using FreneticUtilities.FreneticToolkit;

namespace FreneticGameCore.UtilitySystems
{
    /// <summary>
    /// Helpful utilities for general usage.
    /// </summary>
    public static class Utilities
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
            int mod = 1;
            for (int i = 1; i < 31; i++)
            {
                if ((1 << mod) <= x)
                {
                    return 1 << mod;
                }
            }
            // Number too massive!
            return x;
        }
        /// <summary>
        /// A thread-static random object for all non-deterministic objects to use.
        /// When possible, this should be avoided in favor of contextually available random objects.
        /// </summary>
        public static MTRandom UtilRandom
        {
            get
            {
                if (intRandom == null)
                {
                    intRandom = new MTRandom();
                }
                return intRandom;
            }
        }

        /// <summary>
        /// A thread-static random provider.
        /// </summary>
        [ThreadStatic]
        private static MTRandom intRandom;
        
        /// <summary>
        /// Grabs a sub section of a byte array.
        /// </summary>
        /// <param name="full">The original byte array.</param>
        /// <param name="start">The start index.</param>
        /// <param name="length">The length.</param>
        /// <returns>The subset.</returns>
        public static byte[] BytesPartial(byte[] full, int start, int length)
        {
            byte[] data = new byte[length];
            for (int i = 0; i < length; i++)
            {
                data[i] = full[i + start];
            }
            return data;
        }

        /// <summary>
        /// Checks an exception for rethrow necessity.
        /// </summary>
        /// <param name="ex">The exception to check.</param>
        public static void CheckException(Exception ex)
        {
            if (ex is ThreadAbortException)
            {
                throw ex;
            }
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
            return one > target ? one - amount < target : one + amount > target;
        }

        /// <summary>
        /// Returns a peice of text copied a specified number of times.
        /// </summary>
        /// <param name="text">What text to copy.</param>
        /// <param name="times">How many times to copy it.</param>
        /// <returns>The repeated text.</returns>
        public static string CopyText(string text, int times)
        {
            StringBuilder toret = new StringBuilder(text.Length * times);
            for (int i = 0; i < times; i++)
            {
                toret.Append(text);
            }
            return toret.ToString();
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

        /// <summary>
        /// Valid ASCII symbols for a plaintext alphanumeric username.
        /// </summary>
        public static AsciiMatcher UsernameValidationMatcher = new AsciiMatcher(
            "abcdefghijklmnopqrstuvwxyz" + "ABCDEFGHIJKLMNOPQRSTUVWXYZ" + "0123456789" + "_");

        /// <summary>
        /// Validates a username as correctly formatted, as plaintext alphanumeric ASCII.
        /// Also enforces length between 4 and 15 symbols, inclusive.
        /// </summary>
        /// <param name="str">The username to validate.</param>
        /// <returns>Whether the username is valid.</returns>
        public static bool ValidateUsername(string str)
        {
            if (str == null)
            {
                return false;
            }
            // Length = 4-15
            if (str.Length < 4 || str.Length > 15)
            {
                return false;
            }
            // Starts A-Z
            if (!(str[0] >= 'a' && str[0] <= 'z') && !(str[0] >= 'A' && str[0] <= 'Z'))
            {
                return false;
            }
            // All symbols are A-Z, 0-9, _
            return UsernameValidationMatcher.IsOnlyMatches(str);
        }

        /// <summary>
        /// Calculates a Halton Sequence result.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <param name="basen">The base number, should be prime.</param>
        static double HaltonSequence(int index, int basen)
        {
            if (basen <= 1)
            {
                return 0;
            }
            double res = 0;
            double f = 1;
            int i = index;
            while (i > 0)
            {
                f = f / basen;
                res = res + f * (i % basen);
                i = (int)Math.Floor((double)i / basen);
            }
            return res;
        }

        /// <summary>
        /// Formats a long with "123,456" style notation.
        /// </summary>
        /// <param name="input">The number.</param>
        /// <returns>The formatted string.</returns>
        public static string FormatNumber(long input)
        {
            // TODO: Better method here.
            string basinp = input.ToString();
            string creation = "";
            int c = 0;
            for (int i = basinp.Length - 1; i >= 0; i--)
            {
                if ((c % 3) == 0 && c != 0)
                {
                    creation = basinp[i] + "," + creation;
                }
                else
                {
                    creation = basinp[i] + creation;
                }
                c++;
            }
            return creation;
        }
    }
}
