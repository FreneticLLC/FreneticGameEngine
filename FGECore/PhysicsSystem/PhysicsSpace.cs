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

            /// <summary>An array of FGE entities, where the index in the array is a physics space handle ID.</summary>
            public EntityPhysicsProperty[] EntitiesByPhysicsID;

            /// <summary>Initialize internal space data.</summary>
            public void Init(PhysicsSpace space)
            {
                EntitiesByPhysicsID = new EntityPhysicsProperty[128];
                // TODO: Add user configurability to the thread count.
                int targetThreadCount = Math.Max(1, Environment.ProcessorCount > 4 ? Environment.ProcessorCount - 2 : Environment.ProcessorCount - 1);
                ThreadDispatcher = new BepuThreadDispatcher(targetThreadCount);
                PoseHandler = new BepuPoseIntegratorCallbacks() { Space = space };
                NarrowPhaseHandler = new BepuNarrowPhaseCallbacks() { Space = space };
                CoreSimulation = Simulation.Create(new BufferPool(), NarrowPhaseHandler, PoseHandler, new PositionFirstTimestepper());
            }
        }

        /// <summary>Internal data for the physics space.</summary>
        public InternalData Internal;

        /// <summary>
        /// The default gravity value.
        /// </summary>
        public Location Gravity = new Location(0, 0, -9.8);

        /// <summary>
        /// Spawns a physical object into the physics world.
        /// One entity per physics object only!
        /// </summary>
        /// <param name="entity">The FGE entity.</param>
        /// <param name="bepuent">The BEPU object.</param>
        public BodyReference Spawn(EntityPhysicsProperty entity, BodyDescription bepuent)
        {
            BodyHandle handle = Internal.CoreSimulation.Bodies.Add(bepuent);
            if (Internal.EntitiesByPhysicsID.Length < handle.Value)
            {
                EntityPhysicsProperty[] newArray = new EntityPhysicsProperty[Internal.EntitiesByPhysicsID.Length * 2];
                Array.Copy(Internal.EntitiesByPhysicsID, newArray, Internal.EntitiesByPhysicsID.Length);
                Internal.EntitiesByPhysicsID = newArray;
            }
            Internal.EntitiesByPhysicsID[handle.Value] = entity;
            return new BodyReference(handle, Internal.CoreSimulation.Bodies);
        }

        /// <summary>
        /// De-Spawns a physical object from the physics world.
        /// One entity per physics object only!
        /// </summary>
        /// <param name="bepuent">The BEPU object.</param>
        public void Despawn(BodyReference bepuent)
        {
            bepuent.Bodies.Remove(bepuent.Handle);
            Internal.EntitiesByPhysicsID[bepuent.Handle.Value] = null;
        }

        /// <summary>Performs a single tick update of all physics world data.</summary>
        public void Tick(float delta)
        {
            Internal.CoreSimulation.Timestep(delta, Internal.ThreadDispatcher);
        }

        private class EntitiesInBoxHelper : IBreakableForEach<CollidableReference>
        {
            public List<EntityPhysicsProperty> Entities;

            public PhysicsSpace Space;

            public bool LoopBody(CollidableReference collidable) // true = continue, false = stop
            {
                Entities.Add(Space.Internal.EntitiesByPhysicsID[collidable.BodyHandle.Value]);
                return true;
            }
        }

        /// <summary>
        /// Gets all (physics enabled) entities whose boundaries touch the specified bounding box. This includes entities fully within the box.
        /// TODO: Filter options!
        /// </summary>
        /// <param name="box">The bounding box.</param>
        /// <returns>The list of entities found.</returns>
        public IEnumerable<EntityPhysicsProperty> GetEntitiesInBox(AABB box)
        {
            EntitiesInBoxHelper helper = new EntitiesInBoxHelper() { Entities = new List<EntityPhysicsProperty>(), Space = this };
            Internal.CoreSimulation.BroadPhase.GetOverlaps(new BoundingBox(box.Min.ToNumerics(), box.Max.ToNumerics()), ref helper);
            return helper.Entities;
        }

        private struct RayTraceHelper : IRayHitHandler
        {
            public Func<EntityPhysicsProperty, bool> Filter;

            public CollisionResult Hit;

            public PhysicsSpace Space;

            public bool AllowTest(CollidableReference collidable)
            {
                EntityPhysicsProperty entity = Space.Internal.EntitiesByPhysicsID[collidable.BodyHandle.Value];
                return Filter == null || Filter(entity);
            }

            public bool AllowTest(CollidableReference collidable, int childIndex)
            {
                return AllowTest(collidable);
            }

            public void OnRayHit(in RayData ray, ref float maximumT, float t, in Vector3 normal, CollidableReference collidable, int childIndex)
            {
                Hit = new CollisionResult() { Hit = true, HitEnt = Space.Internal.EntitiesByPhysicsID[collidable.BodyHandle.Value], Normal = normal.ToLocation(), Position = (ray.Origin + ray.Direction * t).ToLocation() };
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
        public CollisionResult RayTraceSingle(Location start, Location dir, double dist, Func<EntityPhysicsProperty, bool> filter = null)
        {
            RayTraceHelper helper = new RayTraceHelper() { Space = this, Filter = filter };
            Internal.CoreSimulation.RayCast(start.ToNumerics(), dir.ToNumerics(), (float)dist, ref helper);
            return helper.Hit ?? new CollisionResult() { Position = start + dir * dist };
        }

        private struct ConvexTraceHelper : ISweepHitHandler
        {
            public Func<EntityPhysicsProperty, bool> Filter;

            public CollisionResult Hit;

            public PhysicsSpace Space;

            public Location Start;

            public bool AllowTest(CollidableReference collidable)
            {
                EntityPhysicsProperty entity = Space.Internal.EntitiesByPhysicsID[collidable.BodyHandle.Value];
                return Filter == null || Filter(entity);
            }

            public bool AllowTest(CollidableReference collidable, int childIndex)
            {
                return AllowTest(collidable);
            }

            public void OnHit(ref float maximumT, float t, in Vector3 hitLocation, in Vector3 hitNormal, CollidableReference collidable)
            {
                Hit = new CollisionResult() { Hit = true, HitEnt = Space.Internal.EntitiesByPhysicsID[collidable.BodyHandle.Value], Normal = hitNormal.ToLocation(), Position = hitLocation.ToLocation() };
            }

            public void OnHitAtZeroT(ref float maximumT, CollidableReference collidable)
            {
                Hit = new CollisionResult() { Hit = true, HitEnt = Space.Internal.EntitiesByPhysicsID[collidable.BodyHandle.Value], Normal = Location.Zero, Position = Start };
            }
        }

        /// <summary>
        /// Sends a world convex-shape trace, giving back the single found object, or null if none.
        /// Uses a standard filter.
        /// </summary>
        /// <param name="shape">The shape to trace with.</param>
        /// <param name="start">The start position.</param>
        /// <param name="dir">The direction.</param>
        /// <param name="dist">The distance.</param>
        /// <param name="filter">The filter, if any.</param>
        public CollisionResult ConvexTraceSingle<TShape>(TShape shape, Location start, Location dir, double dist, Func<EntityPhysicsProperty, bool> filter = null) where TShape : unmanaged, IConvexShape
        {
            ConvexTraceHelper helper = new ConvexTraceHelper() { Space = this, Filter = filter, Start = start };
            Internal.CoreSimulation.Sweep(shape, new RigidPose(start.ToNumerics(), System.Numerics.Quaternion.Identity), new BodyVelocity(dir.ToNumerics(), Vector3.Zero), (float)dist, Internal.CoreSimulation.BufferPool, ref helper);
            return helper.Hit ?? new CollisionResult() { Position = start + dir * dist };
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
            Internal.Init(this);
        }
    }
}
