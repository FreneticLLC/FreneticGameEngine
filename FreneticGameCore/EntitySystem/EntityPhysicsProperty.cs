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
using BEPUphysics;
using BEPUutilities;
using BEPUphysics.Character;
using BEPUphysics.Entities;
using BEPUphysics.CollisionShapes;
using FreneticGameCore.EntitySystem.PhysicsHelpers;

namespace FreneticGameCore.EntitySystem
{
    /// <summary>
    /// Identifies and controls the factors of an entity relating to standard-implemented physics.
    /// </summary>
    public class EntityPhysicsProperty<T, T2> : BasicEntityProperty<T, T2> where T : BasicEntity<T2> where T2 : BasicEngine<T, T2>
    {
        /// <summary>
        /// The owning physics world.
        /// </summary> // TODO: Save the correct physics world ref?
        public PhysicsSpace<T, T2> PhysicsWorld; // Set by constructor.

        /// <summary>
        /// The spawned physics body.
        /// </summary>
        public Entity SpawnedBody = null; // Set by spawner.

        /// <summary>
        /// The original spawned object.
        /// </summary>
        public ISpaceObject OriginalObject = null; // Set by spawner.

        /// <summary>
        /// The shape of the physics body.
        /// </summary>
        [PropertyDebuggable]
        [PropertyAutoSavable]
        public EntityShapeHelper Shape; // Set by client.

        /// <summary>
        /// The starting mass of the physics body.
        /// </summary>
        private double InternalMass = 1;

        /// <summary>
        /// Whether gravity value is already set for this entity. If not set, <see cref="Gravity"/> is invalid or irrelevant.
        /// </summary>
        [PropertyDebuggable]
        [PropertyAutoSavable]
        public bool GravityIsSet = false;

        /// <summary>
        /// The starting gravity of the physics body.
        /// </summary>
        private Location InternalGravity; // Auto-set to match the region at object construct time.

        /// <summary>
        /// The starting friction value of the physics body.
        /// </summary>
        private double InternalFriction = 0.5f;

        /// <summary>
        /// The starting bounciness (restitution coefficient) of the physics body.
        /// </summary>
        private double InternalBounciness = 0.25f;

        /// <summary>
        /// The starting linear velocity of the physics body.
        /// </summary>
        private Location InternalLinearVelocity; // 0,0,0 is good.

        /// <summary>
        /// The starting angular velocity of the physics body.
        /// </summary>
        private Location InternalAngularVelocity; // 0,0,0 is good.

        /// <summary>
        /// The starting position of the physics body.
        /// </summary>
        private Location InternalPosition; // 0,0,0 is good.

        /// <summary>
        /// The starting orientation of the physics body.
        /// </summary>
        private Quaternion InternalOrientation = Quaternion.Identity;

        // TODO: Shape save/debug
        // TODO: Maybe point to the correct physics space somehow in saves/debug? Needs a space ID.

        /// <summary>
        /// Gets or sets the entity's mass.
        /// </summary>
        [PropertyDebuggable]
        [PropertyAutoSavable]
        public double Mass
        {
            get
            {
                return SpawnedBody == null ? InternalMass : SpawnedBody.Mass;
            }
            set
            {
                InternalMass = value;
                if (SpawnedBody != null)
                {
                    SpawnedBody.Mass = InternalMass;
                }
            }
        }

        /// <summary>
        /// Gets or sets the entity's gravity.
        /// </summary>
        [PropertyDebuggable]
        [PropertyAutoSavable]
        public Location Gravity
        {
            get
            {
                return SpawnedBody == null ? InternalGravity : new Location(SpawnedBody.Gravity.Value);
            }
            set
            {
                InternalGravity = value;
                GravityIsSet = true;
                if (SpawnedBody != null)
                {
                    SpawnedBody.Gravity = InternalGravity.ToBVector();
                }
            }
        }

        /// <summary>
        /// Gets or sets the entity's friction.
        /// </summary>
        [PropertyDebuggable]
        [PropertyAutoSavable]
        public double Friction
        {
            get
            {
                // TODO: Separate kinetic and static friction?
                return SpawnedBody == null ? InternalFriction : SpawnedBody.Material.KineticFriction;
            }
            set
            {
                InternalFriction = value;
                if (SpawnedBody != null)
                {
                    SpawnedBody.Material.KineticFriction = InternalFriction;
                    SpawnedBody.Material.StaticFriction = InternalFriction;
                }
            }
        }

        /// <summary>
        /// Gets or sets the entity's bounciness (Restitution coefficient).
        /// </summary>
        [PropertyDebuggable]
        [PropertyAutoSavable]
        public double Bounciness
        {
            get
            {
                return SpawnedBody == null ? InternalBounciness : SpawnedBody.Material.Bounciness;
            }
            set
            {
                InternalBounciness = value;
                if (SpawnedBody != null)
                {
                    SpawnedBody.Material.Bounciness = InternalBounciness;
                }
            }
        }

        /// <summary>
        /// Gets or sets the entity's linear velocity.
        /// </summary>
        [PropertyDebuggable]
        [PropertyAutoSavable]
        public Location LinearVelocity
        {
            get
            {
                return SpawnedBody == null ? InternalLinearVelocity : new Location(SpawnedBody.LinearVelocity);
            }
            set
            {
                InternalLinearVelocity = value;
                if (SpawnedBody != null)
                {
                    SpawnedBody.LinearVelocity = InternalLinearVelocity.ToBVector();
                }
            }
        }

        /// <summary>
        /// Gets or sets the entity's angular velocity.
        /// </summary>
        [PropertyDebuggable]
        [PropertyAutoSavable]
        public Location AngularVelocity
        {
            get
            {
                return SpawnedBody == null ? InternalAngularVelocity : new Location(SpawnedBody.AngularVelocity);
            }
            set
            {
                InternalAngularVelocity = value;
                if (SpawnedBody != null)
                {
                    SpawnedBody.AngularVelocity = InternalAngularVelocity.ToBVector();
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
                return SpawnedBody == null ? InternalPosition : new Location(SpawnedBody.Position);
            }
            set
            {
                InternalPosition = value;
                if (SpawnedBody != null)
                {
                    SpawnedBody.Position = InternalPosition.ToBVector();
                }
            }
        }

        /// <summary>
        /// Gets or sets the entity's orientation.
        /// TODO: Custom FGE quaternion type?
        /// </summary>
        [PropertyDebuggable]
        [PropertyAutoSavable]
        public Quaternion Orientation
        {
            get
            {
                return SpawnedBody == null ? InternalOrientation : SpawnedBody.Orientation;
            }
            set
            {
                InternalOrientation = value;
                if (SpawnedBody != null)
                {
                    SpawnedBody.Orientation = InternalOrientation;
                }
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
                PhysicsWorld = Entity.Engine.PhysicsWorld;
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
        /// Checks and handles a position update.
        /// </summary>
        /// <param name="p">The new position.</param>
        public void PosCheck(Location p)
        {
            Location coff = new Location(Quaternion.Transform(Shape.GetCenterOffset(), SpawnedBody.Orientation));
            Location p2 = (p / PhysicsWorld.RelativeScale) + coff;
            if (p2.DistanceSquared(InternalPosition) > 0.01)
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
            Quaternion.GetRelativeRotation(ref q, ref InternalOrientation, out Quaternion rel);
            if (Quaternion.GetAngleFromQuaternion(ref rel) > 0.01)
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
                InternalGravity = PhysicsWorld.Gravity;
                GravityIsSet = true;
            }
            if (Shape is EntityCharacterShape chr)
            {
                CharacterController cc = chr.GetBEPUCharacter();
                cc.Tag = Entity;
                OriginalObject = cc;
                SpawnedBody = cc.Body;
            }
            else
            {
                SpawnedBody = new Entity(Shape.GetBEPUShape(), InternalMass);
                OriginalObject = SpawnedBody;
                SpawnedBody.Orientation = InternalOrientation;
            }
            SpawnedBody.LinearVelocity = InternalLinearVelocity.ToBVector();
            SpawnedBody.AngularVelocity = InternalAngularVelocity.ToBVector();
            SpawnedBody.Material.KineticFriction = InternalFriction;
            SpawnedBody.Material.StaticFriction = InternalFriction;
            SpawnedBody.Material.Bounciness = InternalBounciness;
            SpawnedBody.Position = InternalPosition.ToBVector();
            SpawnedBody.Gravity = InternalGravity.ToBVector();
            SpawnedBody.Tag = Entity;
            SpawnedBody.CollisionInformation.Tag = Entity;
            // TODO: Other settings
            PhysicsWorld.Spawn(Entity, OriginalObject);
            Entity.OnTick += Tick;
            InternalPosition = Location.Zero;
            InternalOrientation = Quaternion.Identity;
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
            Location bpos = new Location(SpawnedBody.Position);
            if (InternalPosition.DistanceSquared(bpos) > 0.0001) // TODO: || Active?
            {
                InternalPosition = bpos;
                Location coff = new Location(Quaternion.Transform(Shape.GetCenterOffset(), SpawnedBody.Orientation));
                Entity.OnPositionChanged?.Invoke((bpos - coff) * PhysicsWorld.RelativeScale);
            }
            Quaternion cur = SpawnedBody.Orientation;
            Quaternion.GetRelativeRotation(ref cur, ref InternalOrientation, out Quaternion rel);
            if (Quaternion.GetAngleFromQuaternion(ref rel) > 0.0001) // TODO: || Active?
            {
                InternalOrientation = cur;
                Entity.OnOrientationChanged?.Invoke(cur);
            }
        }

        /// <summary>
        /// Updates the entity's local fields from spawned variant.
        /// </summary>
        public void UpdateFields()
        {
            InternalMass = SpawnedBody.Mass;
            InternalGravity = new Location(SpawnedBody.Gravity.Value);
            InternalFriction = SpawnedBody.Material.KineticFriction;
            InternalBounciness = SpawnedBody.Material.Bounciness;
            InternalLinearVelocity = new Location(SpawnedBody.LinearVelocity);
            InternalAngularVelocity = new Location(SpawnedBody.AngularVelocity);
            InternalPosition = new Location(SpawnedBody.Position);
            InternalOrientation = SpawnedBody.Orientation;
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
            PhysicsWorld.Despawn(Entity, OriginalObject);
            SpawnedBody = null;
            OriginalObject = null;
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
            if (SpawnedBody != null)
            {
                Vector3 vec = force.ToBVector();
                SpawnedBody.ApplyLinearImpulse(ref vec);
                SpawnedBody.ActivityInformation.Activate();
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
            if (SpawnedBody != null)
            {
                Vector3 ori = origin.ToBVector();
                Vector3 vec = force.ToBVector();
                SpawnedBody.ApplyImpulse(ref ori, ref vec);
                SpawnedBody.ActivityInformation.Activate();
            }
            else
            {
                // TODO: Account for spin?
                LinearVelocity += force / Mass;
            }
        }
    }
}
