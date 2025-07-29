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
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace FGECore.MathHelpers;

/// <summary>
/// Represents a 2D vector of integers.
/// Occupies 8 bytes, calculated as 4 * 2, as it has 2 fields (X, Y) each occupying 4 bytes (an integer).
/// </summary>
/// <param name="x">X coordinate.</param>
/// <param name="y">Y coordinate.</param>
[StructLayout(LayoutKind.Explicit)]
public struct Vector2i(int x, int y) : IEquatable<Vector2i>
{

    /// <summary>The zero vector.</summary>
    public static readonly Vector2i Zero = new(0, 0);

    /// <summary>The x coordinate.</summary>
    [FieldOffset(0)]
    public int X = x;

    /// <summary>The y coordinate.</summary>
    [FieldOffset(4)]
    public int Y = y;

    /// <summary>Gets a cheap hash code.</summary>
    /// <returns>The hash code.</returns>
    public override readonly int GetHashCode() => X * 23 + Y;

    /// <summary>Compares equality between this and another vector.</summary>
    /// <param name="other">The other vector.</param>
    /// <returns>Whether they are equal.</returns>
    public override readonly bool Equals(object other)
    {
        if (other is not Vector2i vecForm)
        {
            return false;
        }
        return Equals(vecForm);
    }

    /// <summary>Compares equality between this and another vector.</summary>
    /// <param name="other">The other vector.</param>
    /// <returns>Whether they are equal.</returns>
    public readonly bool Equals(Vector2i other) => other.X == X && other.Y == Y;

    /// <summary>
    /// Converts this vector a floating point Location.
    /// Zero on Z axis.
    /// </summary>
    /// <returns>The Location.</returns>
    public readonly Location ToLocation() => new(X, Y, 0);

    /// <summary>Gets a simple string of the vector.</summary>
    /// <returns>The string.</returns>
    public override readonly string ToString() => $"({X}, {Y})";

    /// <summary>Logical comparison.</summary>
    /// <param name="one">First vec.</param>
    /// <param name="two">Second vec.</param>
    /// <returns>Result.</returns>
    public static bool operator !=(Vector2i one, Vector2i two) => !one.Equals(two);

    /// <summary>Logical comparison.</summary>
    /// <param name="one">First vec.</param>
    /// <param name="two">Second vec.</param>
    /// <returns>Result.</returns>
    public static bool operator ==(Vector2i one, Vector2i two)
    {
        return one.Equals(two);
    }

    /// <summary>Identity operator.</summary>
    /// <param name="vec">The vector.</param>
    /// <returns>Result.</returns>
    public static Vector2i operator +(Vector2i vec) => vec;

    /// <summary>Negation operator.</summary>
    /// <param name="vec">The vector.</param>
    /// <returns>Result.</returns>
    public static Vector2i operator -(Vector2i vec) => new(-vec.X, -vec.Y);

    /// <summary>Mathematical comparison.</summary>
    /// <param name="one">First vec.</param>
    /// <param name="two">Second vec.</param>
    /// <returns>Result.</returns>
    public static Vector2i operator +(Vector2i one, Vector2i two) => new(one.X + two.X, one.Y + two.Y);

    /// <summary>Mathematical comparison.</summary>
    /// <param name="one">First vec.</param>
    /// <param name="two">Second vec.</param>
    /// <returns>Result.</returns>
    public static Vector2i operator -(Vector2i one, Vector2i two) => new(one.X - two.X, one.Y - two.Y);

    /// <summary>Mathematical comparison.</summary>
    /// <param name="one">First vec.</param>
    /// <param name="two">Int scalar.</param>
    /// <returns>Result.</returns>
    public static Vector2i operator *(Vector2i one, int two) => new(one.X * two, one.Y * two);

    /// <summary>Mathematical comparison.</summary>
    /// <param name="one">First vec.</param>
    /// <param name="two">Int scalar.</param>
    /// <returns>Result.</returns>
    public static Vector2i operator /(Vector2i one, int two) => new(one.X / two, one.Y / two);
}
