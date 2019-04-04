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

namespace FreneticGameCore.MathHelpers
{
    /// <summary>
    /// Represents a 3D vector of integers.
    /// Occupies 12 bytes, calculated as 4 * 3, as it has 3 fields (X, Y, Z) each occupying 4 bytes (an integer).
    /// </summary>
    [StructLayout(LayoutKind.Explicit)]
    public struct Vector3i : IEquatable<Vector3i>
    {
        /// <summary>
        /// Construct the vec3i.
        /// </summary>
        /// <param name="x">X coordinaate.</param>
        /// <param name="y">Y coordinate.</param>
        /// <param name="z">Z coordinate.</param>
        public Vector3i(int x, int y, int z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        /// <summary>
        /// Construct the vec3i.
        /// </summary>
        /// <param name="xy">X and Y coordinaate.</param>
        /// <param name="z">Z coordinate.</param>
        public Vector3i(Vector2i xy, int z)
        {
            X = xy.X;
            Y = xy.Y;
            Z = z;
        }

        /// <summary>
        /// The zero vector.
        /// </summary>
        public static readonly Vector3i Zero = new Vector3i(0, 0, 0);

        /// <summary>
        /// The X coordinate.
        /// </summary>
        [FieldOffset(0)]
        public int X;

        /// <summary>
        /// The Y coordinate.
        /// </summary>
        [FieldOffset(4)]
        public int Y;

        /// <summary>
        /// The Z coordinate.
        /// </summary>
        [FieldOffset(8)]
        public int Z;

        /// <summary>
        /// Gets a cheap hash code.
        /// </summary>
        /// <returns>The hash code.</returns>
        public override int GetHashCode()
        {
            return X * 23 + Y * 77 + Z;
        }

        /// <summary>
        /// Compares equality between this and another vector.
        /// </summary>
        /// <param name="other">The other vector.</param>
        /// <returns>Whether they are equal.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override bool Equals(object other)
        {
            return Equals((Vector3i)other);
        }

        /// <summary>
        /// Compares equality between this and another vector.
        /// </summary>
        /// <param name="other">The other vector.</param>
        /// <returns>Whether they are equal.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Equals(Vector3i other)
        {
            return other.X == X && other.Y == Y && other.Z == Z;
        }

        /// <summary>
        /// Converts this vector a BEPU floating point vector.
        /// </summary>
        /// <returns>The float vector.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Vector3 ToVector3()
        {
            return new Vector3(X, Y, Z);
        }

        /// <summary>
        /// Converts this vector a floating point Location.
        /// </summary>
        /// <returns>The Location.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Location ToLocation()
        {
            return new Location(X, Y, Z);
        }

        /// <summary>
        /// Gets a simple string of the vector.
        /// </summary>
        /// <returns>The string.</returns>
        public override string ToString()
        {
            return "(" + X + ", " + Y + ", " + Z + ")";
        }

        /// <summary>
        /// Logical comparison.
        /// </summary>
        /// <param name="one">First vec.</param>
        /// <param name="two">Second vec.</param>
        /// <returns>Result.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(Vector3i one, Vector3i two)
        {
            return !one.Equals(two);
        }

        /// <summary>
        /// Logical comparison.
        /// </summary>
        /// <param name="one">First vec.</param>
        /// <param name="two">Second vec.</param>
        /// <returns>Result.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(Vector3i one, Vector3i two)
        {
            return one.Equals(two);
        }

        /// <summary>
        /// Mathematical operation.
        /// </summary>
        /// <param name="one">First vec.</param>
        /// <param name="two">Second vec.</param>
        /// <returns>Result.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3i operator +(Vector3i one, Vector3i two)
        {
            return new Vector3i(one.X + two.X, one.Y + two.Y, one.Z + two.Z);
        }

        /// <summary>
        /// Mathematical operation.
        /// </summary>
        /// <param name="one">First vec.</param>
        /// <param name="two">Second vec.</param>
        /// <returns>Result.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3i operator -(Vector3i one, Vector3i two)
        {
            return new Vector3i(one.X - two.X, one.Y - two.Y, one.Z - two.Z);
        }

        /// <summary>
        /// Mathematical operation.
        /// </summary>
        /// <param name="one">First vec.</param>
        /// <param name="two">Int scalar.</param>
        /// <returns>Result.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3i operator *(Vector3i one, int two)
        {
            return new Vector3i(one.X * two, one.Y * two, one.Z * two);
        }

        /// <summary>
        /// Gets or set the XY 2D component of this <see cref="Vector3i"/>.
        /// </summary>
        public Vector2i XY
        {
            get
            {
                return new Vector2i(X, Y);
            }
            set
            {
                X = value.X;
                Y = value.Y;
            }
        }
    }
}
