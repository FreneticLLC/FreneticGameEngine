using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BEPUphysics;
using BEPUutilities;
using BEPUphysics.Entities;
using BEPUphysics.CollisionShapes;
using FreneticGameCore.EntitySystem.PhysicsHelpers;

namespace FreneticGameCore.EntitySystem
{
    /// <summary>
    /// Identifies and controls the factors of an entity relating to standard-implemented physics.
    /// </summary>
    public class EntityPhysicsProperty : BasicEntityProperty
    {
        /// <summary>
        /// The owning physics world.
        /// </summary>
        public PhysicsSpace PhysicsWorld; // Set by constructor.

        /// <summary>
        /// The spawned physics body.
        /// </summary>
        public Entity SpawnedBody = null; // Set by spawner.

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
        /// <param name="space">The space it will be spawned into.</param>
        public EntityPhysicsProperty(PhysicsSpace space)
        {
            PhysicsWorld = space;
            InternalGravity = PhysicsWorld.Gravity;
        }

        /// <summary>
        /// Fired when the entity is added to the world.
        /// </summary>
        public override void OnSpawn()
        {
            SpawnHandle();
            BEntity.OnPositionChanged += PosCheck;
        }

        /// <summary>
        /// Checks and handles a position update.
        /// </summary>
        /// <param name="p">The new position.</param>
        public void PosCheck(Location p)
        {
            if (p != InternalPosition)
            {
                Position = p;
            }
        }

        /// <summary>
        /// Fired when the entity is removed from the world.
        /// </summary>
        public override void OnDeSpawn()
        {
            DeSpawnHandle();
            BEntity.OnPositionChanged -= PosCheck;
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
            SpawnedBody = new Entity(Shape.GetBEPUShape(), InternalMass)
            {
                Gravity = InternalGravity.ToBVector(),
                LinearVelocity = InternalLinearVelocity.ToBVector(),
                AngularVelocity = InternalAngularVelocity.ToBVector(),
                Tag = BEntity
            };
            SpawnedBody.Material.KineticFriction = InternalFriction;
            SpawnedBody.Material.StaticFriction = InternalFriction;
            SpawnedBody.Material.Bounciness = InternalBounciness;
            SpawnedBody.Position = InternalPosition.ToBVector();
            SpawnedBody.Orientation = InternalOrientation;
            // TODO: Other settings
            PhysicsWorld.Spawn(BEntity, SpawnedBody);
            BEntity.OnTick += Tick;
        }

        /// <summary>
        /// Ticks the physics entity.
        /// </summary>
        public void Tick()
        {
            Location bpos = new Location(SpawnedBody.Position);
            if (InternalPosition.DistanceSquared(bpos) > 0.01)
            {
                InternalPosition = bpos;
                BEntity.OnPositionChanged?.Invoke(bpos);
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
        /// Handles the physics entity being de-spawned from a world.
        /// </summary>
        public void DeSpawnHandle()
        {
            UpdateFields();
            BEntity.OnTick -= Tick;
            PhysicsWorld.DeSpawn(BEntity, SpawnedBody);
            SpawnedBody = null;
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
