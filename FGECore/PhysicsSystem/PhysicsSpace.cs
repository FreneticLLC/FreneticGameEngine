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
        public void Despawn(BodyHandle bepuent)
        {
            Internal.CoreSimulation.Bodies.Remove(bepuent);
            Internal.EntitiesByPhysicsID[bepuent.Value] = null;
        }

        /// <summary>Performs a single tick update of all physics world data.</summary>
        public void Tick(float delta)
        {
            Internal.CoreSimulation.Timestep(delta, Internal.ThreadDispatcher);
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

        private class EntitiesInBoxHelper : IBreakableForEach<CollidableReference>
        {
            public List<T> Entities;

            public PhysicsSpace Space;

            public bool LoopBody(CollidableReference collidable) // true = continue, false = stop
            {
                if (collidable.Mobility == CollidableMobility.Dynamic)
                {
                    Entities.Add(Space.Internal.EntitiesByPhysicsID[collidable.BodyHandle.Value].Entity as T);
                }
                return true;
            }
        }

        /// <summary>
        /// Gets all (physics enabled) entities whose boundaries touch the specified bounding box. This includes entities fully within the box.
        /// TODO: Filter options!
        /// </summary>
        /// <param name="box">The bounding box.</param>
        /// <returns>The list of entities found.</returns>
        public IEnumerable<T> GetEntitiesInBox(AABB box)
        {
            EntitiesInBoxHelper helper = new EntitiesInBoxHelper() { Entities = new List<T>(), Space = this };
            Internal.CoreSimulation.BroadPhase.GetOverlaps(new BoundingBox(box.Min.ToNumerics(), box.Max.ToNumerics()), ref helper);
            return helper.Entities;
        }

        private struct RayTraceHelper : IRayHitHandler
        {
            public Func<T, bool> Filter;

            public T Hit;

            public PhysicsSpace Space;

            public bool AllowTest(CollidableReference collidable)
            {
                if (collidable.Mobility == CollidableMobility.Dynamic)
                {
                    T entity = Space.Internal.EntitiesByPhysicsID[collidable.BodyHandle.Value].Entity as T;
                    return Filter == null || Filter(entity);
                }
                return false;
            }

            public bool AllowTest(CollidableReference collidable, int childIndex)
            {
                return AllowTest(collidable);
            }

            public void OnRayHit(in RayData ray, ref float maximumT, float t, in Vector3 normal, CollidableReference collidable, int childIndex)
            {
                Hit = Space.Internal.EntitiesByPhysicsID[collidable.BodyHandle.Value].Entity as T;
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
            RayTraceHelper helper = new RayTraceHelper() { Space = this, Filter = filter };
            Internal.CoreSimulation.RayCast(start.ToNumerics(), dir.ToNumerics(), (float)dist, ref helper);
            return helper.Hit;
        }
    }
}
