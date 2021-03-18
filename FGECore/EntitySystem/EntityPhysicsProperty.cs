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

        /// <summary>The entity's friction.</summary>
        [PropertyDebuggable]
        [PropertyAutoSavable]
        public float Friction = 0.5f;

        /// <summary>The entity's bounciness (Restitution coefficient).</summary>
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

        /// <summary>Construct the physics entity property.</summary>
        public EntityPhysicsProperty()
        {
        }

        /// <summary>Fired when the entity is added to the world.</summary>
        public override void OnSpawn()
        {
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

        private void DoPosCheckEvent(Location position)
        {
            PosCheck(position);
        }
        private void DoOrientationCheckEvent(Quaternion orientation)
        {
            OrientationCheck(orientation);
        }

        /// <summary>Whether <see cref="NoCheck"/> should be automatically set (to true) when the <see cref="EntityPhysicsProperty"/> is pushing its own updates.</summary>
        public bool CheckDisableAllowed = true;

        /// <summary>
        /// Set to indicate that Position/Orientation checks are not needed currently.
        /// This will be set (to true) automatically when the <see cref="EntityPhysicsProperty"/> is pushing out its own position updates,
        /// and must be explicitly disabled to update anyway.
        /// If disabling this explicitly may be problematic, consider disabling <see cref="CheckDisableAllowed"/> instead.
        /// </summary>
        public bool NoCheck = false;

        /// <summary>
        /// Checks and handles a position update.
        /// </summary>
        /// <param name="_position">The new position.</param>
        public bool PosCheck(Location _position)
        {
            if (_position.DistanceSquared(Internal.Position) > 0.01)
            {
                if (!NoCheck)
                {
                    Position = _position;
                }
                return true;
            }
            return false;
        }

        /// <summary>
        /// Checks and handles an orientation update.
        /// </summary>
        /// <param name="orientation">The new orientation.</param>
        public bool OrientationCheck(Quaternion orientation)
        {
            Quaternion relative = Quaternion.GetQuaternionBetween(orientation, Internal.Orientation);
            if (relative.RepresentedAngle() > 0.01) // TODO: Is this validation needed? This is very expensive to run.
            {
                if (!NoCheck)
                {
                    Orientation = orientation;
                }
                return true;
            }
            return false;
        }

        /// <summary>Handles the physics entity being spawned into a world.</summary>
        public void SpawnHandle()
        {
            if (IsSpawned)
            {
                OnDespawn();
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
            IConvexShape convexShape = Shape.BepuShape;
            convexShape.ComputeInertia(Internal.Mass, out BodyInertia inertia);
            CollidableDescription collidable = new CollidableDescription(Shape.ShapeIndex, 0.1f, ContinuousDetectionSettings.Continuous(1e-3f, 1e-2f));
            BodyDescription description = BodyDescription.CreateDynamic(pose, velocity, inertia, collidable, new BodyActivityDescription(0.01f));
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
            TickUpdates();
        }

        /// <summary>Ticks external positioning updates.</summary>
        public void TickUpdates()
        {
            NoCheck = CheckDisableAllowed;
            if (PosCheck(SpawnedBody.Pose.Position.ToLocation()))
            {
                Entity.OnPositionChanged?.Invoke(Internal.Position);
            }
            if (OrientationCheck(SpawnedBody.Pose.Orientation.ToCore()))
            {
                Entity.OnOrientationChanged?.Invoke(Internal.Orientation);
            }
            NoCheck = false;
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
