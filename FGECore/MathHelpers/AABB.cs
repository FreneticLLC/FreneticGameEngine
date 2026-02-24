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
using System.Text;
using System.Threading.Tasks;

namespace FGECore.MathHelpers;

/// <summary>Represents an Axis-Aligned Bounding Box.</summary>
public struct AABB(Location _min, Location _max)
{
    /// <summary>An AABB of (NaN, NaN).</summary>
    public static readonly AABB NaN = new(Location.NaN, Location.NaN);

    /// <summary>The minimum coordinates.</summary>
    public Location Min = _min;

    /// <summary>The maximum coordinates.</summary>
    public Location Max = _max;

    /// <summary>Returns the center of the box - that is, (Min+Max)/2.</summary>
    public readonly Location Center => (Min + Max) * 0.5;

    /// <summary>Returns the full size of the box - that is, Max-Min.</summary>
    public readonly Location Size => Max - Min;

    /// <summary>Returns whether the box intersects another box.</summary>
    /// <param name="box2">The second box.</param>
    public readonly bool Intersects(in AABB box2)
    {
        Location min2 = box2.Min;
        Location max2 = box2.Max;
        return !(min2.X > Max.X || max2.X < Min.X || min2.Y > Max.Y || max2.Y < Min.Y || min2.Z > Max.Z || max2.Z < Min.Z);
    }

    /// <summary>Converts the AABB to a string, in the form (X, Y, Z)/(X, Y, Z)</summary>
    public override readonly string ToString() => $"{Min}/{Max}";

    /// <summary>Converts the AABB to a string, in the form (Min=(X, Y, Z), Max=(X, Y, Z), Center=(X, Y, Z), Size=(X, Y, Z)), limited to 2 decimal digits.</summary>
    public readonly string ToDebugString() => $"(Min={Min.ToBasicString()}, Max={Max.ToBasicString()}, Center={Center.ToBasicString()}, Size={Size.ToBasicString()})";

    /// <summary>Includes a Location into the box's space, expanding as needed (but not shrinking).</summary>
    /// <param name="pos">The position to include.</param>
    public void Include(in Location pos)
    {
        Min = Min.Min(pos);
        Max = Max.Max(pos);
    }

    /// <summary>Includes a Location into the box's space, expanding as needed (but not shrinking), and returns the resultant AABB.</summary>
    /// <param name="pos">The position to include.</param>
    public readonly AABB Including(in Location pos) => new(Min.Min(pos), Max.Max(pos));

    #region operators
    /// <summary>Returns whether two AABBs are equal.</summary>
    /// <param name="v1">The first AABB.</param>
    /// <param name="v2">The second AABB.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator ==(in AABB v1, in AABB v2) => v1.Min == v2.Min && v1.Max == v2.Max;

    /// <summary>Returns whether two AABBs are not equal.</summary>
    /// <param name="v1">The first AABB.</param>
    /// <param name="v2">The second AABB.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator !=(in AABB v1, in AABB v2) => v1.Min != v2.Min || v1.Max != v2.Max;

    /// <summary>Returns an AABB offset by a Location vector.</summary>
    /// <param name="box">The AABB.</param>
    /// <param name="vec">The location vector.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static AABB operator +(in AABB box, in Location vec) => new(box.Min + vec, box.Max + vec);

    /// <summary>Negates an AABB.</summary>
    /// <param name="v">The AABB.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static AABB operator -(in AABB v) => new(-v.Min, -v.Max);

    /// <summary>Returns an AABB offset backwards by Location vector.</summary>
    /// <param name="box">The AABB.</param>
    /// <param name="vec">The location vector.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static AABB operator -(in AABB box, in Location vec) => new(box.Min - vec, box.Max - vec);

    /// <inheritdoc/>
    public readonly override bool Equals(object obj) => obj is AABB box && box == this;

    /// <inheritdoc/>
    public readonly override int GetHashCode() => HashCode.Combine(Min, Max);
    #endregion
}

/// <summary>Helper extensions for <see cref="AABB"/>.</summary>
public static class ExtensionsForAABB
{
    /// <summary>Converts a floating point <see cref="BepuUtilities.BoundingBox"/> to an <see cref="AABB"/>.</summary>
    public static AABB ToCore(this BepuUtilities.BoundingBox box)
    {
        return new AABB(box.Min.ToLocation(), box.Max.ToLocation());
    }
}
