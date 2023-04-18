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
using FGECore.CoreSystems;
using FGECore.EntitySystem;
using FGECore.MathHelpers;
using BepuPhysics;
using BepuPhysics.Collidables;
using BepuPhysics.CollisionDetection;
using BepuPhysics.Constraints;
using BepuPhysics.Trees;
using BepuUtilities;
using BepuUtilities.Memory;

namespace FGECore.PhysicsSystem
{
    /// <summary>Represents a physical world (space).</summary>
    public class PhysicsSpace
    {
        /// <summary>Internal data for the physics space.</summary>
        public struct InternalData
        {
            /// <summary>The actual internal physics simulation core.</summary>
            public Simulation CoreSimulation;

            /// <summary>The <see cref="IThreadDispatcher"/> used by this simulation.</summary>
            public ThreadDispatcher BepuThreadDispatcher;

            /// <summary>The pose handler, with gravity and all.</summary>
            public BepuPoseIntegratorCallbacks PoseHandler;

            /// <summary>The standard <see cref="INarrowPhaseCallbacks"/> instance.</summary>
            public BepuNarrowPhaseCallbacks NarrowPhaseHandler;

            /// <summary>An array of FGE entities, where the index in the array is a physics space handle ID.</summary>
            public EntityPhysicsProperty[] EntitiesByPhysicsID;

            /// <summary>Bepu physics character controller system.</summary>
            public BepuCharacters.CharacterControllers Characters;

            /// <summary>Bepu physics memory buffer pool.</summary>
            public BufferPool Pool;

            /// <summary>Accumulator for delta, to avoid over-processing.</summary>
            public double DeltaAccumulator;

            /// <summary>The Position offset, used to reduce issues converting from engine double-precision space to physics single-precision space.</summary>
            public Location Offset;

            /// <summary>Initialize internal space data.</summary>
            public void Init(PhysicsSpace space)
            {
                EntitiesByPhysicsID = new EntityPhysicsProperty[128];
                // TODO: Add user configurability to the thread count.
                int targetThreadCount = Math.Max(1, Environment.ProcessorCount > 4 ? Environment.ProcessorCount - 2 : Environment.ProcessorCount - 1);
                BepuThreadDispatcher = new ThreadDispatcher(targetThreadCount);
                PoseHandler = new BepuPoseIntegratorCallbacks() { Space = space };
                NarrowPhaseHandler = new BepuNarrowPhaseCallbacks() { Space = space };
                Pool = new BufferPool();
                Characters = new BepuCharacters.CharacterControllers(Pool);
                CoreSimulation = Simulation.Create(Pool, NarrowPhaseHandler, PoseHandler, new SolveDescription(velocityIterationCount: 1, substepCount: 8));
            }
        }

        /// <summary>The backing engine.</summary>
        public BasicEngine Engine;

        /// <summary>Internal data for the physics space.</summary>
        public InternalData Internal;

        /// <summary>The default gravity value (which is later configurable on a per-entity basis).</summary>
        public Location Gravity = new(0, 0, -9.8);

        /// <summary>The delta value to use for all non-lagging physics updates.</summary>
        public double UpdateDelta = 1.0 / 60.0;

        /// <summary>Gets Position offset, used to reduce issues converting from engine double-precision space to physics single-precision space.
        /// <para>To change this value, use <see cref="Recenter(Location)"/>.</para></summary>
        public Location Offset => Internal.Offset;

        /// <summary>Recenters the <see cref="Offset"/> at a new location, and adjusts all internal position values accordingly.</summary>
        public void Recenter(Location newCenter)
        {
            Location relative = newCenter - Offset;
            ref Buffer<BodySet> sets = ref Internal.CoreSimulation.Bodies.Sets;
            for (int s = 0; s < sets.Length; s++)
            {
                ref BodySet set = ref sets[s];
                if (set.Allocated)
                {
                    ref Buffer<BodyHandle> handles = ref set.IndexToHandle;
                    if (handles.Allocated)
                    {
                        for (int h = 0; h < handles.Length; h++)
                        {
                            BodyHandle handle = handles[h];
                            BodyReference body = new(handle, Internal.CoreSimulation.Bodies);
                            if (body.Exists)
                            {
                                // convert to doubles before doing the addition, then convert back. The hope is that this will avoid numerical precision issues from the repositioning.
                                body.Pose.Position = (body.Pose.Position.ToLocation() + relative).ToNumerics();
                                body.UpdateBounds();
                            }
                        }
                    }
                }
            }
            ref Buffer<StaticHandle> staticHandles = ref Internal.CoreSimulation.Statics.IndexToHandle;
            if (staticHandles.Allocated)
            {
                for (int h = 0; h < staticHandles.Length; h++)
                {
                    StaticHandle handle = staticHandles[h];
                    StaticReference staticRef = new(handle, Internal.CoreSimulation.Statics);
                    if (staticRef.Exists)
                    {
                        staticRef.Pose.Position = (staticRef.Pose.Position.ToLocation() + relative).ToNumerics();
                        staticRef.UpdateBounds();
                    }
                }
            }
            Internal.Offset = newCenter;
        }

        /// <summary>
        /// Spawns a physical object into the physics world.
        /// One entity per physics object only!
        /// </summary>
        /// <param name="entity">The FGE entity.</param>
        /// <param name="bepuent">The BEPU object.</param>
        public BodyReference Spawn(EntityPhysicsProperty entity, BodyDescription bepuent)
        {
            BodyHandle handle = Internal.CoreSimulation.Bodies.Add(bepuent);
            if (Internal.EntitiesByPhysicsID.Length <= handle.Value)
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
        public void Tick(double delta)
        {
            Internal.DeltaAccumulator += delta;
            while (Internal.DeltaAccumulator > UpdateDelta)
            {
                double updateBy = UpdateDelta;
                while (Internal.DeltaAccumulator > updateBy * 3)
                {
                    updateBy *= 3;
                }
                Internal.DeltaAccumulator -= updateBy;
                Internal.CoreSimulation.Timestep((float)updateBy, Internal.BepuThreadDispatcher);
            }
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
            EntitiesInBoxHelper helper = new() { Entities = new List<EntityPhysicsProperty>(), Space = this };
            Internal.CoreSimulation.BroadPhase.GetOverlaps(new BoundingBox(box.Min.ToNumerics(), box.Max.ToNumerics()), ref helper);
            return helper.Entities;
        }

        /// <summary>Converts a BEPU collidable instance to an FGE entity physics property, or null.</summary>
        public EntityPhysicsProperty GetEntityFrom(CollidableReference collidable)
        {
            if (collidable.Mobility == CollidableMobility.Static)
            {
                return null;
            }
            return Internal.EntitiesByPhysicsID[collidable.BodyHandle.Value];
        }

        private struct RayTraceHelper : IRayHitHandler
        {
            public Func<EntityPhysicsProperty, bool> Filter;

            public CollisionResult Hit;

            public PhysicsSpace Space;

            public bool AllowTest(CollidableReference collidable)
            {
                EntityPhysicsProperty entity = Space.GetEntityFrom(collidable);
                return entity == null || Filter == null || Filter(entity);
            }

            public bool AllowTest(CollidableReference collidable, int childIndex)
            {
                return AllowTest(collidable);
            }

            public void OnRayHit(in RayData ray, ref float maximumT, float t, Vector3 normal, CollidableReference collidable, int childIndex)
            {
                if (Hit == null || t < Hit.Time)
                {
                    Hit = new CollisionResult() { Hit = true, Time = t, HitEnt = Space.GetEntityFrom(collidable), Normal = normal.ToLocation(), Position = (ray.Origin + ray.Direction * t).ToLocation() };
                }
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
            RayTraceHelper helper = new() { Space = this, Filter = filter };
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
                EntityPhysicsProperty entity = Space.GetEntityFrom(collidable);
                return entity == null || Filter == null || Filter(entity);
            }

            public bool AllowTest(CollidableReference collidable, int childIndex)
            {
                return AllowTest(collidable);
            }

            public void OnHit(ref float maximumT, float t, Vector3 hitLocation, Vector3 hitNormal, CollidableReference collidable)
            {
                if (Hit == null || t < Hit.Time)
                {
                    Hit = new CollisionResult() { Hit = true, Time = t, HitEnt = Space.GetEntityFrom(collidable), Normal = hitNormal.ToLocation(), Position = hitLocation.ToLocation() };
                }
            }

            public void OnHitAtZeroT(ref float maximumT, CollidableReference collidable)
            {
                Hit = new CollisionResult() { Hit = true, Time = 0, HitEnt = Space.GetEntityFrom(collidable), Normal = Location.Zero, Position = Start };
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
            ConvexTraceHelper helper = new() { Space = this, Filter = filter, Start = start };
            Internal.CoreSimulation.Sweep(shape, new RigidPose(start.ToNumerics(), System.Numerics.Quaternion.Identity), new BodyVelocity(dir.ToNumerics(), Vector3.Zero), (float)dist, Internal.Pool, ref helper);
            return helper.Hit ?? new CollisionResult() { Position = start + dir * dist };
        }

        /// <summary>Shuts down the physics world and all internal resources.</summary>
        public void Shutdown()
        {
            Internal.CoreSimulation.Dispose();
            Internal.CoreSimulation = null;
            Internal.Characters.Dispose();
            Internal.Characters = null;
            Internal.BepuThreadDispatcher.Dispose();
            Internal.BepuThreadDispatcher = null;
            Internal.EntitiesByPhysicsID = null;
            Internal.NarrowPhaseHandler.Dispose();
            Internal.Pool.Clear();
            Internal.Pool = null;
        }

        /// <summary>Returns a simple string to represent this physics world.</summary>
        public override string ToString()
        {
            return "Physics World";
        }
    }

    /// <summary>Represents a physical world (space), with generic types refering the implementation type.</summary>
    public class PhysicsSpace<T, T2> : PhysicsSpace where T: BasicEntity<T, T2> where T2: BasicEngine<T, T2>
    {
        /// <summary>Construct the physics space.</summary>
        /// <param name="_engine">The backing engine.</param>
        /// <param name="construct">Set false to disable constructing the internal space.</param>
        public PhysicsSpace(BasicEngine _engine, bool construct = true)
        {
            Engine = _engine;
            if (!construct)
            {
                return;
            }
            Internal.Init(this);
        }
    }
}
