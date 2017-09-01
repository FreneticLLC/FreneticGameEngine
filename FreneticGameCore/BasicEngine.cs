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
        /// The source object for this engine. Set to any tag style constant reference you find most helpful to keep!
        /// </summary>
        public Object Source;

        /// <summary>
        /// Current frame delta (seconds).
        /// </summary>
        public double Delta;

        /// <summary>
        /// How long the game has run (seconds).
        /// </summary>
        public double GlobalTickTime = 1.0;

        /// <summary>
        /// The general purpose scheduler.
        /// </summary>
        public Scheduler Schedule = new Scheduler();

        /// <summary>
        /// Returns a duplicate of the entity list, for when you expect the master list to change.
        /// </summary>
        /// <returns>The duplicate.</returns>
        public abstract IReadOnlyList<BasicEntity> EntityListDuplicate();

        /// <summary>
        /// Add an entity to the entity list.
        /// </summary>
        /// <param name="be">The entity.</param>
        public abstract void AddEntity(BasicEntity be);

        /// <summary>
        /// Remove an entity from the entity list.
        /// </summary>
        /// <param name="be">The entity.</param>
        public abstract void RemoveEntity(BasicEntity be);

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
            AddEntity(ce);
            ce.IsSpawned = true;
            foreach (Property prop in ce.GetAllProperties())
            {
                if (prop is BasicEntityProperty bep)
                {
                    bep.OnSpawn();
                }
            }
            ce.OnSpawnEvent?.Fire(Schedule, new EntitySpawnEventArgs());
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
            if (!ent.IsSpawned)
            {
                SysConsole.Output(OutputType.WARNING, "Despawing non-spawned entity.");
                return;
            }
            foreach (Property prop in ent.GetAllProperties())
            {
                if (prop is BasicEntityProperty bep)
                {
                    bep.OnDeSpawn();
                }
            }
            ent.OnDeSpawnEvent?.Fire(Schedule, new EntityDeSpawnEventArgs());
            RemoveEntity(ent);
            ent.IsSpawned = false;
        }

        /// <summary>
        /// The internal engine tick sequence.
        /// </summary>
        public void Tick()
        {
            Schedule.RunAllSyncTasks(Delta);
            PhysicsWorld.Internal.Update(Delta);
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
