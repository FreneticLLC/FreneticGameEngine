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
using FGECore.EntitySystem.PhysicsHelpers;
using FGECore.PhysicsSystem;
using FGECore.MathHelpers;
using FGECore.CoreSystems;
using FGECore.PropertySystem;
using BepuPhysics;
using BepuPhysics.Collidables;

namespace FGECore.EntitySystem
{
    /// <summary>Identifies and controls the factors of an entity relating to standard-implemented physics.</summary>
    public class EntityPhysicsProperty : BasicEntityProperty
    {
        // TODO: Save the correct physics world ref?
        /// <summary>The owning physics world.</summary>
        public PhysicsSpace PhysicsWorld; // Set by constructor.

        /// <summary>Whether the entity is currently spawned into the physics world.</summary>
        public bool IsSpawned = false; // Set by spawner.

        /// <summary>The spawned physics body handle.</summary>
        public BodyReference SpawnedBody; // Set by spawner.

        /// <summary>The shape of the physics body.</summary>
        [PropertyDebuggable]
        [PropertyAutoSavable]
        [PropertyPriority(-1000)]
        public EntityShapeHelper Shape; // Set by client.

        /// <summary>Whether gravity value is already set for this entity. If not set, <see cref="Gravity"/> is invalid or irrelevant.</summary>
        [PropertyDebuggable]
        [PropertyAutoSavable]
        public bool GravityIsSet = false;

        /// <summary>Internal data for this physics property.</summary>
        public struct InternalData
        {
            /// <summary>The starting mass of the physics body.</summary>
            public float Mass;

            /// <summary>The starting gravity of the physics body.</summary>
            public Location Gravity; // Auto-set to match the region at object construct time.

            /// <summary>The starting linear velocity of the physics body.</summary>
            public Location LinearVelocity; // 0,0,0 is good.

            /// <summary>The starting angular velocity of the physics body.</summary>
            public Location AngularVelocity; // 0,0,0 is good.

            /// <summary>The starting position of the physics body.</summary>
            public Location Position; // 0,0,0 is good.

            /// <summary>The starting orientation of the physics body.</summary>
            public Quaternion Orientation;
        }

        /// <summary>Internal data for this physics property.</summary>
        public InternalData Internal = new InternalData() { Mass = 1f, Orientation = Quaternion.Identity };

        // TODO: Shape save/debug
        // TODO: Maybe point to the correct physics space somehow in saves/debug? Needs a space ID.
        
        /// <summary>Gets or sets the entity's mass.</summary>
        [PropertyDebuggable]
        [PropertyAutoSavable]
        public float Mass
        {
            get
            {
                if (IsSpawned)
                {
                    float invmass = SpawnedBody.LocalInertia.InverseMass;
                    return invmass == 0 ? 0 : 1f / invmass;
                }
                else
                {
                    return Internal.Mass;
                }
            }
            set
            {
                Internal.Mass = value;
                if (IsSpawned)
                {
                    SpawnedBody.LocalInertia.InverseMass = value == 0 ? 0 : 1f / value;
                }
            }
        }

        /// <summary>Gets or sets the entity's gravity.</summary>
        [PropertyDebuggable]
        [PropertyAutoSavable]
        public Location Gravity
        {
            get
            {
                return Internal.Gravity;
            }
            set
            {
                Internal.Gravity = value;
                GravityIsSet = true;
            }
        }

        /// <summary>The entity's friction coefficient.</summary>
        [PropertyDebuggable]
        [PropertyAutoSavable]
        public float Friction = 1f;

        /// <summary>The entity's bounciness.</summary>
        [PropertyDebuggable]
        [PropertyAutoSavable]
        public float Bounciness = 0.25f;

        /// <summary>The entity's linear damping (per second) rate.</summary>
        [PropertyDebuggable]
        [PropertyAutoSavable]
        public float LinearDamping = 0.03f;

        /// <summary>The entity's angular damping (per second) rate.</summary>
        [PropertyDebuggable]
        [PropertyAutoSavable]
        public float AngularDamping = 0.03f;

        /// <summary>Gets or sets the entity's linear velocity.</summary>
        [PropertyDebuggable]
        [PropertyAutoSavable]
        public Location LinearVelocity
        {
            get
            {
                return IsSpawned ? SpawnedBody.Velocity.Linear.ToLocation() : Internal.LinearVelocity;
            }
            set
            {
                Internal.LinearVelocity = value;
                if (IsSpawned)
                {
                    SpawnedBody.Velocity.Linear = Internal.LinearVelocity.ToNumerics();
                }
            }
        }

        /// <summary>Gets or sets the entity's angular velocity.</summary>
        [PropertyDebuggable]
        [PropertyAutoSavable]
        public Location AngularVelocity
        {
            get
            {
                return IsSpawned ? SpawnedBody.Velocity.Angular.ToLocation() : Internal.AngularVelocity;
            }
            set
            {
                Internal.AngularVelocity = value;
                if (IsSpawned)
                {
                    SpawnedBody.Velocity.Angular = Internal.AngularVelocity.ToNumerics();
                }
            }
        }

        /// <summary>
        /// Gets or sets the entity's position.
        /// </summary>
        [PropertyDebuggable]
        [PropertyAutoSavable]
        public Location Position
        {
            get
            {
                return IsSpawned ? SpawnedBody.Pose.Position.ToLocation() : Internal.Position;
            }
            set
            {
                Internal.Position = value;
                if (IsSpawned)
                {
                    SpawnedBody.Pose.Position = Internal.Position.ToNumerics();
                }
            }
        }

        /// <summary>Gets or sets the entity's orientation.</summary>
        [PropertyDebuggable]
        [PropertyAutoSavable]
        public Quaternion Orientation
        {
            get
            {
                return IsSpawned ? SpawnedBody.Pose.Orientation.ToCore() : Internal.Orientation;
            }
            set
            {
                Internal.Orientation = value;
                if (IsSpawned)
                {
                    SpawnedBody.Pose.Orientation = Internal.Orientation.ToNumerics();
                }
            }
        }

        /// <summary>This entity's collision group.</summary>
        public CollisionGroup CGroup;

        /// <summary>Fired when the entity is added to the world.</summary>
        public override void OnSpawn()
        {
            if (IsSpawned)
            {
                return;
            }
            if (PhysicsWorld == null)
            {
                PhysicsWorld = Engine.PhysicsWorldGeneric;
            }
            HandledRemove = false;
            SpawnHandle();
            Entity.OnPositionChanged += DoPosCheckEvent;
            Entity.OnOrientationChanged += DoOrientationCheckEvent;
        }

        /// <summary>Fired when the entity is removed from the world.</summary>
        public override void OnDespawn()
        {
            if (HandledRemove)
            {
                return;
            }
            HandledRemove = true;
            DespawnHandle();
            Entity.OnPositionChanged -= DoPosCheckEvent;
            Entity.OnOrientationChanged -= DoOrientationCheckEvent;
        }

        /// <summary>Whether the position and orientation changed events should be propogated to the physics body. Usually leave true.</summary>
        public bool DoTrackPositionChange = true;

        private void DoPosCheckEvent(Location position)
        {
            if (DoTrackPositionChange)
            {
                Position = position;
            }
        }

        private void DoOrientationCheckEvent(Quaternion orientation)
        {
            if (DoTrackPositionChange)
            {
                Orientation = orientation;
            }
        }

        /// <summary>Handles the physics entity being spawned into a world.</summary>
        public void SpawnHandle()
        {
            if (IsSpawned)
            {
                return;
            }
            if (!GravityIsSet)
            {
                Internal.Gravity = PhysicsWorld.Gravity;
                GravityIsSet = true;
            }
            if (CGroup == null)
            {
                CGroup = CollisionUtil.Solid;
            }
            RigidPose pose = new RigidPose(Internal.Position.ToNumerics(), Internal.Orientation.ToNumerics());
            BodyVelocity velocity = new BodyVelocity(Internal.LinearVelocity.ToNumerics(), Internal.AngularVelocity.ToNumerics());
            CollidableDescription collidable = new CollidableDescription(Shape.ShapeIndex, 0.1f, ContinuousDetectionSettings.Continuous(1e-3f, 1e-2f));
            BodyDescription description;
            if (Mass == 0)
            {
                description = BodyDescription.CreateKinematic(pose, velocity, collidable, new BodyActivityDescription(0.01f));
            }
            else
            {
                IConvexShape convexShape = Shape.BepuShape;
                convexShape.ComputeInertia(Internal.Mass, out BodyInertia inertia);
                description = BodyDescription.CreateDynamic(pose, velocity, inertia, collidable, new BodyActivityDescription(0.01f));
            }
            // TODO: Other settings
            SpawnedBody = PhysicsWorld.Spawn(this, description);
            Entity.OnTick += Tick;
            Internal.Position = Location.Zero;
            Internal.Orientation = Quaternion.Identity;
            IsSpawned = true;
            TickUpdates();
        }
        
        /// <summary>Ticks the physics entity.</summary>
        public void Tick()
        {
            if (IsSpawned)
            {
                TickUpdates();
            }
        }

        /// <summary>Whether this entity was awake at last check.</summary>
        public bool WasAwake = true;

        /// <summary>Ticks external positioning updates.</summary>
        public void TickUpdates()
        {
            bool isAwake = SpawnedBody.Awake;
            if (!isAwake && !WasAwake)
            {
                return;
            }
            WasAwake = isAwake;
            bool shouldTrack = DoTrackPositionChange;
            DoTrackPositionChange = false;
            Internal.Position = SpawnedBody.Pose.Position.ToLocation();
            Entity.OnPositionChanged?.Invoke(Internal.Position);
            Internal.Orientation = SpawnedBody.Pose.Orientation.ToCore();
            Entity.OnOrientationChanged?.Invoke(Internal.Orientation);
            DoTrackPositionChange = shouldTrack;
        }

        /// <summary>Updates the entity's local fields from spawned variant.</summary>
        public void UpdateFields()
        {
            float invMass = SpawnedBody.LocalInertia.InverseMass;
            Internal.Mass = invMass == 0 ? 0 : 1f / invMass;
            Internal.LinearVelocity = SpawnedBody.Velocity.Linear.ToLocation();
            Internal.AngularVelocity = SpawnedBody.Velocity.Angular.ToLocation();
            Internal.Position = SpawnedBody.Pose.Position.ToLocation();
            Internal.Orientation = SpawnedBody.Pose.Orientation.ToCore();
        }

        /// <summary>Fired before the physics entity is despawned from the world.</summary>
        public Action DespawnEvent;
        
        /// <summary>Handles the physics entity being de-spawned from a world.</summary>
        public void DespawnHandle()
        {
            if (!IsSpawned)
            {
                return;
            }
            UpdateFields();
            Entity.OnTick -= Tick;
            DespawnEvent?.Invoke();
            PhysicsWorld.Despawn(SpawnedBody);
            IsSpawned = false;
        }

        private bool HandledRemove = false;

        /// <summary>Handles removal event.</summary>
        public override void OnRemoved()
        {
            OnDespawn();
        }

        /// <summary>
        /// Applies a force directly to the physics entity's body.
        /// The force is assumed to be perfectly central to the entity.
        /// Note: this is a force, not a velocity. Mass is relevant.
        /// This will activate the entity.
        /// </summary>
        /// <param name="force">The force to apply.</param>
        public void ApplyForce(Location force)
        {
            if (IsSpawned)
            {
                SpawnedBody.ApplyLinearImpulse(force.ToNumerics());
                SpawnedBody.Awake = true;
            }
            else
            {
                LinearVelocity += force / Mass;
            }
        }

        /// <summary>
        /// Applies a force directly to the physics entity's body, at a specified relative origin point.
        /// The origin is relevant to the body's centerpoint.
        /// The further you get from the centerpoint, the more spin and less linear motion will be applied.
        /// Note: this is a force, not a velocity. Mass is relevant.
        /// This will activate the entity.
        /// </summary>
        /// <param name="origin">Where to apply the force at.</param>
        /// <param name="force">The force to apply.</param>
        public void ApplyForce(Location origin, Location force)
        {
            if (IsSpawned)
            {
                SpawnedBody.ApplyImpulse(force.ToNumerics(), origin.ToNumerics());
                SpawnedBody.Awake = true;
            }
            else
            {
                // TODO: Account for spin?
                LinearVelocity += force / Mass;
            }
        }
    }
}
