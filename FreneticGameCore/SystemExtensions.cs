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
using BEPUphysics.CollisionShapes.ConvexShapes;
using BEPUutilities;
using System.Globalization;
using BEPUutilities.ResourceManagement;
using BEPUutilities.DataStructures;
using BEPUphysics.BroadPhaseEntries.MobileCollidables;

namespace FreneticGameCore
{
    /// <summary>
    /// Helpers for various system classes.
    /// </summary>
    public static class SystemExtensions
    {
        /// <summary>
        /// Rapidly converts a string to a lowercase representation.
        /// </summary>
        /// <param name="input">The original string.</param>
        /// <returns>A lowercase version.</returns>
        public static string ToLowerFast(this string input)
        {
            char[] dt = input.ToCharArray();
            for (int i = 0; i < dt.Length; i++)
            {
                if (dt[i] >= 'A' && dt[i] <= 'Z')
                {
                    dt[i] = (char)(dt[i] - ('A' - 'a'));
                }
            }
            return new string(dt);
        }

        /// <summary>
        /// Returns whether the string starts with a null character.
        /// </summary>
        /// <param name="input">The input string.</param>
        /// <returns>A boolean.</returns>
        public static bool StartsWithNull(this string input)
        {
            return input.Length > 0 && input[0] == '\0';
        }

        /// <summary>
        /// Quickly split a string.
        /// </summary>
        /// <param name="input">The original string.</param>
        /// <param name="splitter">What to split it by.</param>
        /// <param name="count">The maximum number of times to split it.</param>
        /// <returns>The split string pieces.</returns>
        public static string[] SplitFast(this string input, char splitter, int count = int.MaxValue)
        {
            int len = input.Length;
            int c = 0;
            for (int i = 0; i < len; i++)
            {
                if (input[i] == splitter)
                {
                    c++;
                }
            }
            c = ((c > count) ? count : c);
            string[] res = new string[c + 1];
            int start = 0;
            int x = 0;
            for (int i = 0; i < len && x < c; i++)
            {
                if (input[i] == splitter)
                {
                    res[x++] = input.Substring(start, i - start);
                    start = i + 1;
                }
            }
            res[x] = input.Substring(start);
            return res;
        }

        /// <summary>
        /// Get the angle around an axis for a specific quaternion.
        /// </summary>
        /// <param name="rotation">The quaternion.</param>
        /// <param name="axis">The relative axis.</param>
        /// <returns>The angle.</returns>
        public static double AxisAngleFor(this BEPUutilities.Quaternion rotation, Vector3 axis)
        {
            Vector3 ra = new Vector3(rotation.X, rotation.Y, rotation.Z);
            Vector3 p = Utilities.Project(ra, axis);
            BEPUutilities.Quaternion twist = new BEPUutilities.Quaternion(p.X, p.Y, p.Z, rotation.W);
            twist.Normalize();
            Vector3 new_forward = BEPUutilities.Quaternion.Transform(Vector3.UnitX, twist);
            return Utilities.VectorToAngles(new Location(new_forward)).Yaw * Math.PI / 180.0;
        }

        /// <summary>
        /// Converts a Core quaternion to a BEPU quaternion.
        /// </summary>
        /// <param name="q">The OpenTK quaternion.</param>
        /// <returns>The BEPU quaternion.</returns>
        public static BEPUutilities.Quaternion ToBEPU(this FreneticGameCore.Quaternion q)
        {
            return new BEPUutilities.Quaternion(q.X, q.Y, q.Z, q.W);
        }

        /// <summary>
        /// Converts a BEPU quaternion to a Core quaternion.
        /// </summary>
        /// <param name="q">The BEPU quaternion.</param>
        /// <returns>The Core quaternion.</returns>
        public static Quaternion ToCore(this BEPUutilities.Quaternion q)
        {
            return new Quaternion(q.X, q.Y, q.Z, q.W);
        }

        /// <summary>
        /// Converts a <see cref="TextElementEnumerator"/> to an Enumerable.
        /// </summary>
        /// <typeparam name="T">The expected Enumerable type.</typeparam>
        /// <param name="enumerator">The original Enumerator.</param>
        /// <returns>The enumerable.</returns>
        public static IEnumerable<T> AsEnumerable<T>(this TextElementEnumerator enumerator)
        {
            while (enumerator.MoveNext())
            {
                yield return (T)enumerator.Current;
            }
        }

        /// <summary>
        /// Gets the part of a string before a specified portion.
        /// </summary>
        /// <param name="input">The original string.</param>
        /// <param name="match">The end marker.</param>
        /// <returns>The prior portion.</returns>
        public static string Before(this string input, string match)
        {
            int ind = input.IndexOf(match);
            if (ind < 0)
            {
                return input;
            }

            return input.Substring(0, ind);
        }

        /// <summary>
        /// Gets the parts of a string before and after a specified portion.
        /// </summary>
        /// <param name="input">The original string.</param>
        /// <param name="match">The end marker.</param>
        /// <param name="after">The output of the latter portion.</param>
        /// <returns>The prior portion.</returns>
        public static string BeforeAndAfter(this string input, string match, out string after)
        {
            int ind = input.IndexOf(match);
            if (ind < 0)
            {
                after = "";
                return input;
            }
            after = input.Substring(ind + match.Length);
            return input.Substring(0, ind);
        }

        /// <summary>
        /// Gets the part of a string after a specified portion.
        /// </summary>
        /// <param name="input">The original string.</param>
        /// <param name="match">The end marker.</param>
        /// <returns>The latter portion.</returns>
        public static string After(this string input, string match)
        {
            int ind = input.IndexOf(match);
            if (ind < 0)
            {
                return input;
            }
            return input.Substring(ind + match.Length);
        }

        /// <summary>
        /// Gets a Gaussian random value from a Random object.
        /// </summary>
        /// <param name="input">The random object.</param>
        /// <returns>The Gaussian value.</returns>
        public static double NextGaussian(this Random input)
        {
            double u1 = input.NextDouble();
            double u2 = input.NextDouble();
            return Math.Sqrt(-2.0 * Math.Log(u1)) * Math.Sin(2.0 * Math.PI * u2);
        }

        /// <summary>
        /// Rescales a convex hull shape.
        /// </summary>
        /// <param name="shape">The shape.</param>
        /// <param name="scaleFactor">The scaling factor.</param>
        /// <returns>The new hull.</returns>
        public static ConvexHullShape Rescale(this ConvexHullShape shape, double scaleFactor)
        {
            ReadOnlyList<Vector3> verts = shape.Vertices;
            List<Vector3> newlist = new List<Vector3>(verts.Count);
            foreach (Vector3 vert in verts)
            {
                newlist.Add(vert * scaleFactor);
            }
            RawList<int> triangles = CommonResources.GetIntList();
            ConvexHullHelper.GetConvexHull(newlist, triangles);
            InertiaHelper.ComputeShapeDistribution(newlist, triangles, out double volume, out Matrix3x3 volumeDistribution);
            ConvexShapeDescription csd = new ConvexShapeDescription()
            {
                CollisionMargin = shape.CollisionMargin,
                EntityShapeVolume = new BEPUphysics.CollisionShapes.EntityShapeVolumeDescription()
                {
                    Volume = volume,
                    VolumeDistribution = volumeDistribution
                },
                MaximumRadius = shape.MaximumRadius * scaleFactor,
                MinimumRadius = shape.MinimumRadius * scaleFactor
            };
            CommonResources.GiveBack(triangles);
            return new ConvexHullShape(newlist, csd);
        }

        /// <summary>
        /// Rescales a convex hull shape.
        /// </summary>
        /// <param name="shape">The shape.</param>
        /// <param name="scaleFactor">The scaling factor.</param>
        /// <returns>The new hull.</returns>
        public static ConvexHullShape Rescale(this ConvexHullShape shape, Vector3 scaleFactor)
        {
            ReadOnlyList<Vector3> verts = shape.Vertices;
            List<Vector3> newlist = new List<Vector3>(verts.Count);
            foreach (Vector3 vert in verts)
            {
                newlist.Add(vert * scaleFactor);
            }
            double len = scaleFactor.Length();
            RawList<int> triangles = CommonResources.GetIntList();
            ConvexHullHelper.GetConvexHull(newlist, triangles);
            InertiaHelper.ComputeShapeDistribution(newlist, triangles, out double volume, out Matrix3x3 volumeDistribution);
            ConvexShapeDescription csd = new ConvexShapeDescription()
            {
                CollisionMargin = shape.CollisionMargin,
                EntityShapeVolume = new BEPUphysics.CollisionShapes.EntityShapeVolumeDescription()
                {
                    Volume = volume,
                    VolumeDistribution = volumeDistribution
                },
                MaximumRadius = shape.MaximumRadius * len,
                MinimumRadius = shape.MinimumRadius * len
            };
            CommonResources.GiveBack(triangles);
            return new ConvexHullShape(newlist, csd);
        }
    }
}
