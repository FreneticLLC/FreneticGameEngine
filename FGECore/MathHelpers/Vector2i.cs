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
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;
using BEPUutilities;

namespace FGECore.MathHelpers
{
    /// <summary>
    /// Represents a 2D vector of integers.
    /// Occupies 8 bytes, calculated as 4 * 2, as it has 2 fields (X, Y) each occupying 4 bytes (an integer).
    /// </summary>
    [StructLayout(LayoutKind.Explicit)]
    public struct Vector2i : IEquatable<Vector2i>
    {
        /// <summary>
        /// Construct the vec2i.
        /// </summary>
        /// <param name="x">X coordinate.</param>
        /// <param name="y">Y coordinate.</param>
        public Vector2i(int x, int y)
        {
            X = x;
            Y = y;
        }

        /// <summary>
        /// The zero vector.
        /// </summary>
        public static readonly Vector2i Zero = new Vector2i(0, 0);

        /// <summary>
        /// The x coordinate.
        /// </summary>
        [FieldOffset(0)]
        public int X;

        /// <summary>
        /// The y coordinate.
        /// </summary>
        [FieldOffset(4)]
        public int Y;

        /// <summary>
        /// Gets a cheap hash code.
        /// </summary>
        /// <returns>The hash code.</returns>
        public override int GetHashCode()
        {
            return X * 23 + Y;
        }

        /// <summary>
        /// Compares equality between this and another vector.
        /// </summary>
        /// <param name="other">The other vector.</param>
        /// <returns>Whether they are equal.</returns>
        public override bool Equals(object other)
        {
            if (!(other is Vector2i vecForm))
            {
                return false;
            }
            return Equals(vecForm);
        }

        /// <summary>
        /// Compares equality between this and another vector.
        /// </summary>
        /// <param name="other">The other vector.</param>
        /// <returns>Whether they are equal.</returns>
        public bool Equals(Vector2i other)
        {
            return other.X == X && other.Y == Y;
        }

        /// <summary>
        /// Converts this vector a BEPU floating point vector.
        /// </summary>
        /// <returns>The float vector.</returns>
        public Vector2 ToVector2()
        {
            return new Vector2(X, Y);
        }

        /// <summary>
        /// Converts this vector a floating point Location.
        /// Zero on Z axis.
        /// </summary>
        /// <returns>The Location.</returns>
        public Location ToLocation()
        {
            return new Location(X, Y, 0);
        }

        /// <summary>
        /// Gets a simple string of the vector.
        /// </summary>
        /// <returns>The string.</returns>
        public override string ToString()
        {
            return "(" + X + ", " + Y + ")";
        }

        /// <summary>
        /// Logical comparison.
        /// </summary>
        /// <param name="one">First vec.</param>
        /// <param name="two">Second vec.</param>
        /// <returns>Result.</returns>
        public static bool operator !=(Vector2i one, Vector2i two)
        {
            return !one.Equals(two);
        }

        /// <summary>
        /// Logical comparison.
        /// </summary>
        /// <param name="one">First vec.</param>
        /// <param name="two">Second vec.</param>
        /// <returns>Result.</returns>
        public static bool operator ==(Vector2i one, Vector2i two)
        {
            return one.Equals(two);
        }

        /// <summary>
        /// Mathematical comparison.
        /// </summary>
        /// <param name="one">First vec.</param>
        /// <param name="two">Second vec.</param>
        /// <returns>Result.</returns>
        public static Vector2i operator +(Vector2i one, Vector2i two)
        {
            return new Vector2i(one.X + two.X, one.Y + two.Y);
        }

        /// <summary>
        /// Mathematical comparison.
        /// </summary>
        /// <param name="one">First vec.</param>
        /// <param name="two">Int scalar.</param>
        /// <returns>Result.</returns>
        public static Vector2i operator *(Vector2i one, int two)
        {
            return new Vector2i(one.X * two, one.Y * two);
        }

        /// <summary>
        /// Mathematical comparison.
        /// </summary>
        /// <param name="one">First vec.</param>
        /// <param name="two">Int scalar.</param>
        /// <returns>Result.</returns>
        public static Vector2i operator /(Vector2i one, int two)
        {
            return new Vector2i(one.X / two, one.Y / two);
        }
    }
}
