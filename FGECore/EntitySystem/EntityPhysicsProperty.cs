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

            /// <summary>The starting friction value of the physics body.</summary>
            public float Friction;

            /// <summary>The starting bounciness (restitution coefficient) of the physics body.</summary>
            public float Bounciness;

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
        public InternalData Internal = new InternalData() { Mass = 1f, Orientation = Quaternion.Identity, Friction = 0.5f, Bounciness = 0.25f };

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
                return SpawnedBody == null ? Internal.Gravity : SpawnedBody.Gravity.Value.ToLocation();
            }
            set
            {
                Internal.Gravity = value;
                GravityIsSet = true;
                if (IsSpawned)
                {
                    SpawnedBody.Gravity = Internal.Gravity.ToBEPU();
                }
            }
        }

        /// <summary>Gets or sets the entity's friction.</summary>
        [PropertyDebuggable]
        [PropertyAutoSavable]
        public double Friction
        {
            get
            {
                // TODO: Separate kinetic and static friction?
                return SpawnedBody == null ? Internal.Friction : SpawnedBody.Material.KineticFriction;
            }
            set
            {
                Internal.Friction = value;
                if (IsSpawned)
                {
                    SpawnedBody.Material.KineticFriction = Internal.Friction;
                    SpawnedBody.Material.StaticFriction = Internal.Friction;
                }
            }
        }

        /// <summary>Gets or sets the entity's bounciness (Restitution coefficient).</summary>
        [PropertyDebuggable]
        [PropertyAutoSavable]
        public double Bounciness
        {
            get
            {
                return SpawnedBody == null ? Internal.Bounciness : SpawnedBody.Material.Bounciness;
            }
            set
            {
                Internal.Bounciness = value;
                if (IsSpawned)
                {
                    SpawnedBody.Material.Bounciness = Internal.Bounciness;
                }
            }
        }

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
        /// This value is scaled to the physics scaling factor defined by <see cref="PhysicsSpace.RelativeScale"/>.
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

        /// <summary>
        /// Gets relevant helper systems for the entity (if it is a character, otherwise: null).
        /// </summary>
        [PropertyPriority(1000)]
        [PropertyDebuggable]
        public EntityPhysicsCharacterHelper Character
        {
            get
            {
                return (OriginalObject is CharacterController cc) ? new EntityPhysicsCharacterHelper() { Internal = cc } : null;
            }
        }

        /// <summary>
        /// Character instance ID.
        /// </summary>
        public long InstanceId
        {
            get
            {
                return SpawnedBody.InstanceId;
            }
        }

        /// <summary>
        /// Construct the physics entity property.
        /// </summary>
        public EntityPhysicsProperty()
        {
        }

        /// <summary>
        /// Fired when the entity is added to the world.
        /// </summary>
        public override void OnSpawn()
        {
            if (PhysicsWorld == null)
            {
                PhysicsWorld = Engine.PhysicsWorldGeneric;
            }
            SpawnHandle();
            Entity.OnPositionChanged += PosCheck;
            Entity.OnOrientationChanged += OriCheck;
        }

        /// <summary>
        /// Fired when the entity is removed from the world.
        /// </summary>
        public override void OnDespawn()
        {
            if (HandledRemove)
            {
                return;
            }
            HandledRemove = true;
            DespawnHandle();
            Entity.OnPositionChanged -= PosCheck;
            Entity.OnOrientationChanged -= OriCheck;
        }

        /// <summary>
        /// Whether <see cref="NoCheck"/> should be automatically set (to true) when the <see cref="EntityPhysicsProperty"/> is pushing its own updates.
        /// </summary>
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
        /// <param name="p">The new position.</param>
        public void PosCheck(Location p)
        {
            if (NoCheck)
            {
                return;
            }
            Location coff = BEPUutilities.Quaternion.Transform(Shape.GetCenterOffset(), SpawnedBody.Pose.Orientation).ToLocation();
            Location p2 = (p * PhysicsWorld.RelativeScaleInverse) + coff;
            if (p2.DistanceSquared(Internal.Position) > 0.01) // TODO: Is this validation needed?
            {
                Position = p2;
            }
        }

        /// <summary>
        /// Checks and handles an orientation update.
        /// </summary>
        /// <param name="q">The new orientation.</param>
        public void OriCheck(Quaternion q)
        {
            if (NoCheck)
            {
                return;
            }
            Quaternion relative = Quaternion.GetQuaternionBetween(q, Internal.Orientation);
            if (relative.RepresentedAngle() > 0.01) // TODO: Is this validation needed? This is very expensive to run.
            {
                Orientation = q;
            }
        }

        // TODO: Damping values!

        /// <summary>
        /// Handles the physics entity being spawned into a world.
        /// </summary>
        public void SpawnHandle()
        {
            if (!GravityIsSet)
            {
                Internal.Gravity = PhysicsWorld.Gravity;
                GravityIsSet = true;
            }
            if (Shape is EntityCharacterShape chr)
            {
                CharacterController cc = chr.GetBEPUCharacter();
                cc.Tag = Entity;
                OriginalObject = cc;
                SpawnedBody = cc.Body;
                SpawnedBody.Mass = Internal.Mass;
            }
            else
            {
                SpawnedBody = new Entity(Shape.GetBEPUShape(), Internal.Mass);
                OriginalObject = SpawnedBody;
                SpawnedBody.Pose.Orientation = Internal.Orientation.ToNumerics();
            }
            SpawnedBody.Velocity.Linear = Internal.LinearVelocity.ToNumerics();
            SpawnedBody.Velocity.Angular = Internal.AngularVelocity.ToNumerics();
            SpawnedBody.Material.KineticFriction = Internal.Friction;
            SpawnedBody.Material.StaticFriction = Internal.Friction;
            SpawnedBody.Material.Bounciness = Internal.Bounciness;
            SpawnedBody.Position = Internal.Position.ToNumerics();
            SpawnedBody.Gravity = Internal.Gravity.ToNumerics();
            SpawnedBody.Tag = Entity;
            SpawnedBody.CollisionInformation.Tag = this;
            // TODO: Other settings
            PhysicsWorld.Spawn(SpawnedBody);
            Entity.OnTick += Tick;
            Internal.Position = Location.Zero;
            Internal.Orientation = Quaternion.Identity;
            IsSpawned = true;
            TickUpdates();
        }
        
        /// <summary>
        /// Ticks the physics entity.
        /// </summary>
        public void Tick()
        {
            TickUpdates();
        }

        /// <summary>
        /// Ticks external positioning updates.
        /// </summary>
        public void TickUpdates()
        {
            NoCheck = CheckDisableAllowed;
            Location bpos = SpawnedBody.Pose.Position.ToLocation();
            if (Internal.Position.DistanceSquared(bpos) > 0.0001)
            {
                Internal.Position = bpos;
                Location coff = BEPUutilities.Quaternion.Transform(Shape.GetCenterOffset(), SpawnedBody.Pose.Orientation).ToLocation();
                Entity.OnPositionChanged?.Invoke((bpos - coff) * PhysicsWorld.RelativeScaleForward);
            }
            BEPUutilities.Quaternion cur = SpawnedBody.Pose.Orientation;
            BEPUutilities.Quaternion qio = Internal.Orientation.ToNumerics();
            BEPUutilities.Quaternion.GetRelativeRotation(ref cur, ref qio, out BEPUutilities.Quaternion rel);
            if (BEPUutilities.Quaternion.GetAngleFromQuaternion(ref rel) > 0.0001)
            {
                Internal.Orientation = cur.ToCore();
                Entity.OnOrientationChanged?.Invoke(cur.ToCore());
            }
            NoCheck = false;
        }

        /// <summary>
        /// Updates the entity's local fields from spawned variant.
        /// </summary>
        public void UpdateFields()
        {
            float invMass = SpawnedBody.LocalInertia.InverseMass;
            Internal.Mass = invMass == 0 ? 0 : 1f / invMass;
            Internal.Gravity = SpawnedBody.Gravity.Value.ToLocation();
            Internal.Friction = SpawnedBody.Material.KineticFriction;
            Internal.Bounciness = SpawnedBody.Material.Bounciness;
            Internal.LinearVelocity = SpawnedBody.Velocity.Linear.ToLocation();
            Internal.AngularVelocity = SpawnedBody.Velocity.Angular.ToLocation();
            Internal.Position = SpawnedBody.Pose.Position.ToLocation();
            Internal.Orientation = SpawnedBody.Pose.Orientation.ToCore();
        }

        /// <summary>
        /// Fired before the physics entity is despawned from the world.
        /// </summary>
        public Action DespawnEvent;
        
        /// <summary>
        /// Handles the physics entity being de-spawned from a world.
        /// </summary>
        public void DespawnHandle()
        {
            UpdateFields();
            Entity.OnTick -= Tick;
            DespawnEvent?.Invoke();
            PhysicsWorld.Despawn(SpawnedBody.Handle);
            IsSpawned = false;
        }

        private bool HandledRemove = false;

        /// <summary>
        /// Handles removal event.
        /// </summary>
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
