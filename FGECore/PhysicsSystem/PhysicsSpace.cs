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
using FGECore.EntitySystem;
using FGECore.MathHelpers;
using FGECore.CoreSystems;
using BepuPhysics;
using BepuUtilities.Memory;
using BepuUtilities;
using System.Numerics;
using BepuPhysics.CollisionDetection;
using BepuPhysics.Collidables;
using BepuPhysics.Constraints;
using BepuPhysics.Trees;

namespace FGECore.PhysicsSystem
{
    /// <summary>
    /// Represents a physical world (space).
    /// </summary>
    public class PhysicsSpace
    {
        /// <summary>Internal data for the physics space.</summary>
        public struct InternalData
        {
            /// <summary>The actual internal physics simulation core.</summary>
            public Simulation CoreSimulation;

            /// <summary>The <see cref="IThreadDispatcher"/> used by this simulation.</summary>
            public BepuThreadDispatcher ThreadDispatcher;

            /// <summary>The pose handler, with gravity and all.</summary>
            public BepuPoseIntegratorCallbacks PoseHandler;

            /// <summary>The standard <see cref="INarrowPhaseCallbacks"/> instance.</summary>
            public BepuNarrowPhaseCallbacks NarrowPhaseHandler;

            /// <summary>Initialize internal space data.</summary>
            public void Init()
            {
                // TODO: Add user configurability to the thread count.
                int targetThreadCount = Math.Max(1, Environment.ProcessorCount > 4 ? Environment.ProcessorCount - 2 : Environment.ProcessorCount - 1);
                ThreadDispatcher = new BepuThreadDispatcher(targetThreadCount);
                PoseHandler = new BepuPoseIntegratorCallbacks() { Gravity = new Vector3(0, 0, -9.8f) };
                NarrowPhaseHandler = new BepuNarrowPhaseCallbacks();
                CoreSimulation = Simulation.Create(new BufferPool(), NarrowPhaseHandler, PoseHandler, new PositionFirstTimestepper());
            }
        }

        /// <summary>Internal data for the physics space.</summary>
        public InternalData Internal;

        /// <summary>
        /// The scale of all physics vs. rendered objects. Phys * Scale = Render.
        /// Use this for setting.
        /// To get values, use <see cref="RelativeScaleForward"/> and <see cref="RelativeScaleInverse"/>.
        /// </summary>
        public double RelativeScale
        {
            get
            {
                return RelativeScaleForward;
            }
            set
            {
                RelativeScaleForward = value;
                RelativeScaleInverse = 1.0 / value;
            }
        }

        /// <summary>
        /// The scale of all physics vs. rendered objects. Phys * Scale = Render.
        /// Valid only for getting.
        /// </summary>
        public double RelativeScaleForward = 1.0;

        /// <summary>
        /// The scale of all rendered objects vs physics. Phys = Render * Scale.
        /// Valid only for getting.
        /// </summary>
        public double RelativeScaleInverse = 1.0;

        /// <summary>
        /// Gets or sets the internal default gravity value.
        /// </summary>
        public Location Gravity
        {
            get
            {
                return Internal.PoseHandler.Gravity.ToLocation();
            }
            set
            {
                Internal.PoseHandler.Gravity = value.ToNumerics();
            }
        }

        /// <summary>
        /// Spawns a physical object into the physics world.
        /// One entity per physics object only!
        /// </summary>
        /// <param name="bepuent">The BEPU object.</param>
        public void Spawn(BodyDescription bepuent)
        {
            Internal.CoreSimulation.Bodies.Add(bepuent);
        }

        /// <summary>
        /// De-Spawns a physical object from the physics world.
        /// One entity per physics object only!
        /// </summary>
        /// <param name="bepuent">The BEPU object.</param>
        public void Despawn(BodyHandle bepuent)
        {
            Internal.CoreSimulation.Bodies.Remove(bepuent);
        }

        /// <summary>
        /// Returns a simple string form of this physics world.
        /// </summary>
        /// <returns>The simple string form.</returns>
        public override string ToString()
        {
            return "Physics World";
        }
    }

    /// <summary>
    /// Represents a physical world (space), with generic types refering the implementation type.
    /// </summary>
    public class PhysicsSpace<T, T2> : PhysicsSpace where T: BasicEntity<T, T2> where T2: BasicEngine<T, T2>
    {
        /// <summary>
        /// Construct the physics space.
        /// </summary>
        /// <param name="construct">Set false to disable constructing the internal space.</param>
        public PhysicsSpace(bool construct = true)
        {
            if (!construct)
            {
                return;
            }
            Internal.Init();
        }

        /// <summary>
        /// Gets all (physics enabled) entities whose boundaries touch the specified bounding box. This includes entities fully within the box.
        /// <para>Note that this method is designed for best acceleration with LINQ.</para>
        /// TODO: Filter options!
        /// </summary>
        /// <param name="box">The bounding box.</param>
        /// <returns>The list of entities found.</returns>
        public IEnumerable<T> GetEntitiesInBox(AABB box)
        {
            List<BroadPhaseEntry> bpes = new List<BroadPhaseEntry>();
            Internal.BroadPhase.QueryAccelerator.GetEntries(new BoundingBox(box.Min.ToNumerics(), box.Max.ToNumerics()), bpes);
            foreach (BroadPhaseEntry bpe in bpes)
            {
                if (bpe is EntityCollidable ec && ec.Entity.Tag is T res)
                {
                    yield return res;
                }
                else if (bpe.Tag is T bres)
                {
                    yield return bres;
                }
                else if (bpe.Tag is EntityPhysicsProperty pres)
                {
                    yield return pres.Entity as T;
                }
            }
        }

        public struct RayTraceHelper : IRayHitHandler
        {
            public Func<T, bool> Filter;

            public bool AllowTest(CollidableReference collidable)
            {
                // TODO
                throw new NotImplementedException();
            }

            public bool AllowTest(CollidableReference collidable, int childIndex)
            {
                throw new NotImplementedException();
            }

            public void OnRayHit(in RayData ray, ref float maximumT, float t, in Vector3 normal, CollidableReference collidable, int childIndex)
            {
                throw new NotImplementedException();
            }
        }

        /// <summary>
        /// Sends a world ray trace, giving back the single found object, or null if none.
        /// Uses a standard filter.
        /// </summary>
        /// <param name="start">The start position.</param>
        /// <param name="dir">The direction.</param>
        /// <param name="dist">The distance.</param>
        /// <param name="filter">The filter, if any.</param>
        public T RayTraceSingle(Location start, Location dir, double dist, Func<T, bool> filter = null)
        {
            RayCastResult rcr;
            if (filter != null)
            {
                Internal.CoreSimulation.RayCast(start.ToNumerics(), dir.ToNumerics(), (float)dist, (b) => (b.Tag is T t1) ? filter(t1) : ((b.Tag is EntityPhysicsProperty t2) && filter(t2.Entity as T))
                if (!Internal.CoreSimulation.RayCast(start.ToNumerics(), dir.ToNumerics(), (float) dist, (b) => (b.Tag is T t1) ? filter(t1) : ((b.Tag is EntityPhysicsProperty t2) && filter(t2.Entity as T)), out rcr))
                {
                    return null;
                }
            }
            else
            {
                if (!Internal.CoreSimulation.RayCast(start.ToNumerics(), dir.ToNumerics(), (float)dist, out rcr))
                {
                    return null;
                }
            }
            if (rcr.HitObject != null && rcr.HitObject.Tag != null)
            {
                if (rcr.HitObject.Tag is T res)
                {
                    return res;
                }
                else if (rcr.HitObject.Tag is EntityPhysicsProperty pres)
                {
                    return pres.Entity as T;
                }
            }
            return null;
        }

        /// <summary>
        /// Sends a world ray trace, giving back the single found object, or null if none.
        /// Uses a raw filter.
        /// </summary>
        /// <param name="start">The start position.</param>
        /// <param name="dir">The direction.</param>
        /// <param name="dist">The distance.</param>
        /// <param name="filter">The filter, if any.</param>
        public T RayTraceSingle_RawFilter(Location start, Location dir, double dist, Func<BroadPhaseEntry, bool> filter = null)
        {
            RayCastResult rcr;
            if (filter != null)
            {
                if (!Internal.CoreSimulation.RayCast(start.ToNumerics(), dir.ToNumerics(), (float)dist, filter, out rcr))
                {
                    return null;
                }
            }
            else
            {
                if (!Internal.CoreSimulation.RayCast(start.ToNumerics(), dir.ToNumerics(), (float)dist, out rcr))
                {
                    return null;
                }
            }
            if (rcr.HitObject != null && rcr.HitObject.Tag != null)
            {
                if (rcr.HitObject.Tag is T res)
                {
                    return res;
                }
                else if (rcr.HitObject.Tag is EntityPhysicsProperty pres)
                {
                    return pres.Entity as T;
                }
            }
            return null;
        }
    }
}
