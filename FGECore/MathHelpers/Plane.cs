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
using FreneticUtilities.FreneticExtensions;

namespace FGECore.MathHelpers
{
    /// <summary>
    /// Represents a plane in 3D space, using a triangle representation.
    /// </summary>
    public class Plane
    {
        /// <summary>
        /// The normal of the plane.
        /// </summary>
        public Location Normal;

        /// <summary>
        /// The first corner.
        /// </summary>
        public Location Vertex1;

        /// <summary>
        /// The second corner.
        /// </summary>
        public Location Vertex2;

        /// <summary>
        /// The third corner.
        /// </summary>
        public Location Vertex3;

        /// <summary>
        /// The distance from the origin.
        /// </summary>
        public double NormalDistance;

        /// <summary>
        /// Constructs a plane, calculating a normal.
        /// </summary>
        /// <param name="v1">Vertex one.</param>
        /// <param name="v2">Vertex two.</param>
        /// <param name="v3">Vertex three.</param>
        public Plane(Location v1, Location v2, Location v3)
        {
            Vertex1 = v1;
            Vertex2 = v2;
            Vertex3 = v3;
            Normal = (v2 - v1).CrossProduct(v3 - v1).Normalize();
            NormalDistance = -(Normal.Dot(Vertex1));
        }

        /// <summary>
        /// Constructs a plane, with a known normal.
        /// </summary>
        /// <param name="v1">Vertex one.</param>
        /// <param name="v2">Vertex two.</param>
        /// <param name="v3">Vertex three.</param>
        /// <param name="_normal">The precalculated normal.</param>
        public Plane(Location v1, Location v2, Location v3, Location _normal)
        {
            Vertex1 = v1;
            Vertex2 = v2;
            Vertex3 = v3;
            Normal = _normal;
            NormalDistance = -(Normal.Dot(Vertex1));
        }

        /// <summary>
        /// Constructs a plane from a normal and its distance from the origin (no vertices calculated).
        /// </summary>
        /// <param name="_normal">The normal.</param>
        /// <param name="_d">The distance.</param>
        public Plane(Location _normal, double _d)
        {
            double fact = 1 / _normal.Length();
            Normal = _normal * fact;
            NormalDistance = _d * fact;
        }

        /// <summary>
        /// Finds where a line hits the plane, if anywhere.
        /// </summary>
        /// <param name="start">The start of the line.</param>
        /// <param name="end">The end of the line.</param>
        /// <returns>A location of the hit, or NaN if none.</returns>
        public Location IntersectLine(Location start, Location end)
        {
            Location ba = end - start;
            double nDotA = Normal.Dot(start);
            double nDotBA = Normal.Dot(ba);
            double t = -(nDotA + NormalDistance) / (nDotBA);
            if (t < 0) // || t > 1
            {
                return Location.NaN;
            }
            return start + t * ba;
        }

        /// <summary>
        /// Flips the normal, returned as a new object.
        /// </summary>
        /// <returns></returns>
        public Plane FlipNormal()
        {
            return new Plane(Vertex3, Vertex2, Vertex1, -Normal);
        }

        /// <summary>
        /// Returns the distance between a point and the plane.
        /// </summary>
        /// <param name="point">The point.</param>
        /// <returns>The distance.</returns>
        public double Distance(Location point)
        {
            return Normal.Dot(point) + NormalDistance;
        }

        /// <summary>
        /// Determines the signs of a box to the plane.
        /// If it returns 1, the box is above the plane.
        /// If it returns -1, the box is below the plane.
        /// If it returns 0, the box intersections with the plane.
        /// </summary>
        /// <param name="mins">The mins of the box.</param>
        /// <param name="maxes">The maxes of the box.</param>
        /// <returns>-1, 0, or 1.</returns>
        public int SignToPlane(Location mins, Location maxes)
        {
            Location[] locs = new Location[8];
            locs[0] = new Location(mins.X, mins.Y, mins.Z);
            locs[1] = new Location(mins.X, mins.Y, maxes.Z);
            locs[2] = new Location(mins.X, maxes.Y, mins.Z);
            locs[3] = new Location(mins.X, maxes.Y, maxes.Z);
            locs[4] = new Location(maxes.X, mins.Y, mins.Z);
            locs[5] = new Location(maxes.X, mins.Y, maxes.Z);
            locs[6] = new Location(maxes.X, maxes.Y, mins.Z);
            locs[7] = new Location(maxes.X, maxes.Y, maxes.Z);
            int pSign = Math.Sign(Distance(locs[0]));
            for (int i = 1; i < locs.Length; i++)
            {
                if (Math.Sign(Distance(locs[i])) != pSign)
                {
                    return 0;
                }
            }
            return pSign;
        }

        /// <summary>
        /// Converts the plane to a simple string form of [(X, Y, Z)/(X, Y, Z)/(X, Y, Z)]
        /// Inverts <see cref="FromString(string)"/>.
        /// </summary>
        /// <returns>The plane string.</returns>
        public override string ToString()
        {
            return "[" + Vertex1.ToString() + "/" + Vertex2.ToString() + "/" + Vertex3.ToString() + "]";
        }

        /// <summary>
        /// Converts a string to a plane.
        /// Inverts <see cref="ToString"/>.
        /// </summary>
        /// <param name="input">The plane string.</param>
        /// <returns>A plane.</returns>
        public static Plane FromString(string input)
        {
            string[] data = input.Replace('[', ' ').Replace(']', ' ').Replace(" ", "").SplitFast('/');
            if (data.Length < 3)
            {
                return null;
            }
            return new Plane(Location.FromString(data[0]), Location.FromString(data[1]), Location.FromString(data[2]));
        }
    }
}
