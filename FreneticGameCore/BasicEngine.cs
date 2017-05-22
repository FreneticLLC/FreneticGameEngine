using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FreneticGameCore.EntitySystem;

namespace FreneticGameCore
{
    /// <summary>
    /// Represents the common code shared by a server and client engine.
    /// </summary>
    public abstract class BasicEngine
    {

        /// <summary>
        /// The general-purpose physics world.
        /// </summary>
        public PhysicsSpace PhysicsWorld;

        /// <summary>
        /// Current frame delta (seconds).
        /// </summary>
        public double Delta;

        /// <summary>
        /// How long the game has run (seconds).
        /// </summary>
        public double GlobalTickTime = 1.0;

        /// <summary>
        /// All entities on the server, in a list.
        /// A list was chosen over a lookup table, as quick-resorting and running through is more important to be fast than EID lookups.
        /// </summary>
        public List<BasicEntity> Entities = new List<BasicEntity>();

        /// <summary>
        /// Returns a duplicate of the entity list, for when you expect the master list to change.
        /// </summary>
        /// <returns>The duplicate.</returns>
        public IReadOnlyList<BasicEntity> EntityListDuplicate()
        {
            return new List<BasicEntity>(Entities);
        }

        /// <summary>
        /// The current highest EID value.
        /// </summary>
        public long CurrentEID = 1;

        /// <summary>
        /// Creates an entity.
        /// </summary>
        /// <param name="ticks">Whether it should tick.</param>
        /// <returns>The entity.</returns>
        public abstract BasicEntity CreateEntity(bool ticks);

        /// <summary>
        /// Spawns an entity into the world.
        /// </summary>
        /// <param name="ticks">Whether it should tick.</param>
        /// <param name="configure">A method to configure the entity prior to spawn, if one applies.</param>
        /// <param name="props">Any properties to apply.</param>
        /// <returns>The spawned entity.</returns>
        public BasicEntity SpawnEntity(bool ticks, Action<BasicEntity> configure, params Property[] props)
        {
            BasicEntity ce = CreateEntity(ticks);
            ce.EID = CurrentEID++;
            configure?.Invoke(ce);
            for (int i = 0; i < props.Length; i++)
            {
                ce.AddProperty(props[i]);
            }
            Entities.Add(ce);
            // TODO: it may be useful here to sort the entity list by rendering order? Would increase spawn lag but decrease frame time!
            ce.IsSpawned = true;
            ce.OnSpawn?.Fire(new EntitySpawnEventArgs());
            return ce;
        }

        /// <summary>
        /// Spawns an entity into the world.
        /// </summary>
        /// <param name="ticks">Whether it should tick.</param>
        /// <param name="props">Any properties to apply.</param>
        /// <returns>The spawned entity.</returns>
        public BasicEntity SpawnEntity(bool ticks, params Property[] props)
        {
            return SpawnEntity(ticks, null, props);
        }

        /// <summary>
        /// Spawns an entity into the world.
        /// </summary>
        /// <param name="configure">A method to configure the entity prior to spawn, if one applies.</param>
        /// <param name="props">Any properties to apply.</param>
        /// <returns>The spawned entity.</returns>
        public BasicEntity SpawnEntity(Action<BasicEntity> configure, params Property[] props)
        {
            return SpawnEntity(true, configure, props);
        }

        /// <summary>
        /// Spawns an entity into the world.
        /// </summary>
        /// <param name="props">Any properties to apply.</param>
        /// <returns>The spawned entity.</returns>
        public BasicEntity SpawnEntity(params Property[] props)
        {
            return SpawnEntity(true, null, props);
        }

        /// <summary>
        /// Removes an entity from the world.
        /// </summary>
        /// <param name="ent">The entity to remove.</param>
        public void DeSpawnEntity(BasicEntity ent)
        {
            int ind = Entities.IndexOf(ent);
            if (ind < 0 || !ent.IsSpawned)
            {
                SysConsole.Output(OutputType.WARNING, "Despawing non-spawned entity.");
                return;
            }
            ent.OnDeSpawn?.Fire(new EntityDeSpawnEventArgs());
            Entities.RemoveAt(ind);
            ent.IsSpawned = false;
        }

        /// <summary>
        /// The internal engine tick sequence.
        /// </summary>
        public void Tick()
        {
            // Dup list, to ensure ents can despawn themselves in the tick method!
            IReadOnlyList<BasicEntity> ents = EntityListDuplicate();
            for (int i = 0; i < ents.Count; i++)
            {
                if (ents[i].Ticks)
                {
                    ents[i].TickThis();
                }
            }
        }
    }
}
