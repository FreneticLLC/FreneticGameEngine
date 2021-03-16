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
using BEPUphysics.CollisionShapes.ConvexShapes;
using BEPUutilities;
using System.Globalization;
using BEPUutilities.ResourceManagement;
using BEPUutilities.DataStructures;
using FGECore.UtilitySystems;
using FGECore.MathHelpers;
using FGECore.PhysicsSystem;

namespace FGECore.PhysicsSystem
{
    /// <summary>Helpers for BEPU classes.</summary>
    public static class BepuExtensions
    {
        /// <summary>
        /// Converts a Core quaternion to a BEPU quaternion.
        /// </summary>
        /// <param name="q">The OpenTK quaternion.</param>
        /// <returns>The BEPU quaternion.</returns>
        public static BEPUutilities.Quaternion ToBEPU(this MathHelpers.Quaternion q)
        {
            return new BEPUutilities.Quaternion(q.X, q.Y, q.Z, q.W);
        }

        /// <summary>
        /// Converts a BEPU quaternion to a Core quaternion.
        /// </summary>
        /// <param name="q">The BEPU quaternion.</param>
        /// <returns>The Core quaternion.</returns>
        public static MathHelpers.Quaternion ToCore(this BEPUutilities.Quaternion q)
        {
            return new MathHelpers.Quaternion(q.X, q.Y, q.Z, q.W);
        }

        /// <summary>
        /// Converts a BEPUPhysics <see cref="Vector3"/> to a <see cref="Location"/>.
        /// </summary>
        /// <param name="loc">The BEPUPhysics <see cref="Vector3"/>.</param>
        /// <returns>The <see cref="Location"/>.</returns>
        public static Location ToLocation(this Vector3 loc)
        {
            return new Location(loc.X, loc.Y, loc.Z);
        }

        /// <summary>
        /// Converts a <see cref="Location"/> to a BEPUPhysics <see cref="Vector3"/>.
        /// </summary>
        /// <param name="loc">The <see cref="Location"/>.</param>
        /// <returns>The BEPUPhysics <see cref="Vector3"/>.</returns>
        public static Vector3 ToBEPU(this Location loc)
        {
            return new Vector3(loc.X, loc.Y, loc.Z);
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
        public static ConvexHullShape Rescale(this ConvexHullShape shape, in Vector3 scaleFactor)
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
