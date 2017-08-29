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

namespace FreneticGameCore.Collision
{
    /// <summary>
    /// Represents an Axis-Aligned Bounding Box.
    /// </summary>
    public class AABB
    {
        /// <summary>
        /// The minimum coordinates.
        /// </summary>
        public Location Min;

        /// <summary>
        /// The maximum coordinates.
        /// </summary>
        public Location Max;

        /// <summary>
        /// Returns whether the box intersects another box.
        /// </summary>
        /// <param name="box2">The second box.</param>
        /// <returns>Whether they intersect.</returns>
        public bool Intersects(AABB box2)
        {
            Location min2 = box2.Min;
            Location max2 = box2.Max;
            return !(min2.X > Max.X || max2.X < Min.X || min2.Y > Max.Y || max2.Y < Min.Y || min2.Z > Max.Z || max2.Z < Min.Z);
        }

        /// <summary>
        /// Converts the AABB to a string.
        /// </summary>
        /// <returns>The string.</returns>
        public override string ToString()
        {
            return Min + "/" + Max;
        }

        /// <summary>
        /// Includes a Location into the box's space, expanding as needed (but not shrinking).
        /// </summary>
        /// <param name="pos">The position to include.</param>
        public void Include(Location pos)
        {
            if (pos.X < Min.X)
            {
                Min.X = pos.X;
            }
            if (pos.Y < Min.Y)
            {
                Min.Y = pos.Y;
            }
            if (pos.Z < Min.Z)
            {
                Min.Z = pos.Z;
            }
            if (pos.X > Max.X)
            {
                Max.X = pos.X;
            }
            if (pos.Y > Max.Y)
            {
                Max.Y = pos.Y;
            }
            if (pos.Z > Max.Z)
            {
                Max.Z = pos.Z;
            }
        }
    }
}
