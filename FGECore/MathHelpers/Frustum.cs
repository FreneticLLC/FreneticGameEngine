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
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using BepuUtilities;

namespace FGECore.MathHelpers;

/// <summary>
/// Represents a 3D Frustum.
/// <para>Can be used to represent the area a camera can see.</para>
/// <para>Can be used for high-speed culling of visible objects.</para>
/// </summary>
/// <param name="matrix">The matrix.</param>
public class Frustum(Matrix4x4 matrix)
{
    /// <summary>Near plane.</summary>
    public Plane Near = new(new(-matrix.M13, -matrix.M23, -matrix.M33), -matrix.M43);

    /// <summary>Far plane.</summary>
    public Plane Far = new(new(matrix.M13 - matrix.M14, matrix.M23 - matrix.M24, matrix.M33 - matrix.M34), matrix.M43 - matrix.M44);

    /// <summary>Left plane.</summary>
    public Plane Left = new(new(-matrix.M14 - matrix.M11, -matrix.M24 - matrix.M21, -matrix.M34 - matrix.M31), -matrix.M44 - matrix.M41);

    /// <summary>Right plane.</summary>
    public Plane Right = new(new(matrix.M11 - matrix.M14, matrix.M21 - matrix.M24, matrix.M31 - matrix.M34), matrix.M41 - matrix.M44);

    /// <summary>Top plane.</summary>
    public Plane Top = new(new(matrix.M12 - matrix.M14, matrix.M22 - matrix.M24, matrix.M32 - matrix.M34), matrix.M42 - matrix.M44);

    /// <summary>Bottom plane.</summary>
    public Plane Bottom = new(new(-matrix.M14 - matrix.M12, -matrix.M24 - matrix.M22, -matrix.M34 - matrix.M32), -matrix.M44 - matrix.M42);

    /// <summary>Returns a boolean indicating whether an AABB is contained by the Frustum.</summary>
    /// <param name="min">The lower coord of the AABB.</param>
    /// <param name="max">The higher coord of the AABB.</param>
    /// <returns>Whether it is contained.</returns>
    public bool ContainsBox(Location min, Location max)
    {
        // TODO: Improve accuracy
        if (min == max)
        {
            return Contains(min);
        }
        Location[] locs = [
            min, max, new(min.X, min.Y, max.Z),
            new(min.X, max.Y, max.Z),
            new(max.X, min.Y, max.Z),
            new(max.X, min.Y, min.Z),
            new(max.X, max.Y, min.Z),
            new(min.X, max.Y, min.Z)
        ];
        for (int p = 0; p < 6; p++)
        {
            Plane pl = GetFor(p);
            int inC = 8;
            for (int i = 0; i < 8; i++)
            {
                if (Math.Sign(pl.Distance(locs[i])) == 1)
                {
                    inC--;
                }
            }

            if (inC == 0)
            {
                /*
                // Backup
                if (Contains(min)) { return true; }
                else if (Contains(max)) { return true; }
                else if (Contains(new Location(min.X, min.Y, max.Z))) { return true; }
                else if (Contains(new Location(min.X, max.Y, max.Z))) { return true; }
                else if (Contains(new Location(max.X, min.Y, max.Z))) { return true; }
                else if (Contains(new Location(max.X, min.Y, min.Z))) { return true; }
                else if (Contains(new Location(max.X, max.Y, min.Z))) { return true; }
                else if (Contains(new Location(min.X, max.Y, min.Z))) { return true; }*/
                return false;
            }
        }
        return true;
    }

    /// <summary>Returns whether the frustum contains a sphere.</summary>
    /// <param name="point">The center of the sphere.</param>
    /// <param name="radius">The radius of the sphere.</param>
    /// <returns>Whether it intersects.</returns>
    public bool ContainsSphere(Location point, double radius)
    {
        // TODO: Fix
        /*
        double dist;
        for (int i = 0; i < 6; i++)
        {
            Plane pl = GetFor(i);
            dist = pl.Normal.Dot(point) + pl.D;
            if (dist < -radius)
            {
                return false;
            }
            if (Math.Abs(dist) < radius)
            {
                return true;
            }
        }
        return true;*/
        return ContainsBox(point - new Location(radius), point + new Location(radius));
    }

    /// <summary>Gets the plane associated with an index.</summary>
    public Plane GetFor(int i)
    {
        return i switch
        {
            0 => Far,
            1 => Near,
            2 => Top,
            3 => Bottom,
            4 => Left,
            5 => Right,
            _ => throw new InvalidOperationException($"GetFor({i}) is invalid: input must be between 0 and 5, inclusive."),
        };
    }

    /// <summary>Returns whether the Frustum contains a point.</summary>
    /// <param name="point">The point.</param>
    /// <returns>Whether it's contained.</returns>
    public bool Contains(Location point)
    {
        double TryPoint(Plane plane)
        {
            return point.X * plane.Normal.X + point.Y * plane.Normal.Y + point.Z * plane.Normal.Z + plane.NormalDistance;
        }
        if (TryPoint(Far) > 0 || TryPoint(Near) > 0 || TryPoint(Top) > 0 || TryPoint(Bottom) > 0 || TryPoint(Left) > 0 || TryPoint(Right) > 0)
        {
            return false;
        }
        return true;
    }
}
