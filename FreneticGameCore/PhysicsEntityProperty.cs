using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BEPUphysics;
using BEPUphysics.Entities;
using BEPUphysics.CollisionShapes;

namespace FreneticGameCore
{
    /// <summary>
    /// Identifies and controls the factors of an entity relating to standard-implemented physics.
    /// Add this BEFORE you spawn an entity!
    /// </summary>
    public class PhysicsEntityProperty : Property
    {
        /// <summary>
        /// The owning physics world.
        /// </summary>
        public PhysicsSpace PhysicsWorld;

        /// <summary>
        /// The spawned physics body.
        /// </summary>
        public Entity SpawnedBody;

        /// <summary>
        /// The shape of the physics body.
        /// </summary>
        public EntityShape Shape;

        /// <summary>
        /// The starting mass of the physics body. Does not automatically update the internal body.
        /// </summary>
        private double InternalMass;

        /// <summary>
        /// Gets or sets the entity's mass.
        /// </summary>
        public double Mass
        {
            get
            {
                return SpawnedBody == null ? Mass : SpawnedBody.Mass;
            }
            set
            {
                Mass = value;
                if (SpawnedBody != null)
                {
                    SpawnedBody.Mass = Mass;
                }
            }
        }

        // TODO: Other sub-properties!
        
        /// <summary>
        /// Construct the physics entity property.
        /// </summary>
        /// <param name="space">The space it will be spawned into.</param>
        public PhysicsEntityProperty(PhysicsSpace space)
        {
            PhysicsWorld = space;
        }

        /// <summary>
        /// Fired when the property is added to an entity.
        /// </summary>
        public override void OnAdded()
        {
            BasicEntity be = Holder as BasicEntity;
            be.OnSpawn += SpawnHandle;
            be.OnDeSpawn += DeSpawnHandle;
        }

        /// <summary>
        /// Fired when the property is removed from an entity.
        /// </summary>
        public override void OnRemoved()
        {
            BasicEntity be = Holder as BasicEntity;
            be.OnSpawn -= SpawnHandle;
            be.OnDeSpawn -= DeSpawnHandle;
        }

        /// <summary>
        /// Handles the physics entity being spawned into a world.
        /// </summary>
        public void SpawnHandle(int prio, EntitySpawnEventArgs context)
        {
            SpawnedBody = new Entity(Shape, InternalMass);
            // TODO: Other settings
            PhysicsWorld.Internal.Add(SpawnedBody);
        }
        
        /// <summary>
        /// Handles the physics entity being de-spawned from a world.
        /// </summary>
        public void DeSpawnHandle(int prio, EntityDeSpawnEventArgs context)
        {
            PhysicsWorld.Internal.Remove(SpawnedBody);
        }
    }
}
