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
    public abstract class BasicEngine<T, T2> where T : BasicEntity<T2> where T2: BasicEngine<T, T2>
    {
        /// <summary>
        /// The general-purpose physics world.
        /// </summary>
        public PhysicsSpace<T, T2> PhysicsWorld;

        /// <summary>
        /// Loads the basic engine.
        /// </summary>
        public void LoadBasic()
        {
            SysConsole.Output(OutputType.INIT, "BasicEngine prepping physics helper...");
            PhysicsWorld = new PhysicsSpace<T, T2>();
        }

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
        /// All entities currently on this server, if EIDs are used.
        /// </summary>
        public Dictionary<long, T> Entities = new Dictionary<long, T>(8192);

        /// <summary>
        /// All entities currently on the server.
        /// </summary>
        public List<T> EntityList = new List<T>(8192);

        /// <summary>
        /// Returns a duplicate of the entity list, for when you expect the master list to change.
        /// </summary>
        /// <returns>The duplicate.</returns>
        public IReadOnlyList<T> EntityListDuplicate()
        {
            return new List<T>(EntityList);
        }

        /// <summary>
        /// Adds an entity to the server, quick and deadly. Prefer spawn over this.
        /// </summary>
        /// <param name="be">The entity.</param>
        public void AddEntity(T be)
        {
            EntityList.Add(be);
            Entities.Add(be.EID, be);
        }

        /// <summary>
        /// Removes an entity from the list, quick and deadly. Prefer despawn over this.
        /// </summary>
        /// <param name="be">The entity to remove.</param>
        public void RemoveEntity(T be)
        {
            EntityList.Remove(be);
            Entities.Remove(be.EID);
        }

        /// <summary>
        /// The current highest EID value.
        /// </summary>
        public long CurrentEID = 1;

        /// <summary>
        /// Gets all entities with a specific property.
        /// </summary>
        /// <typeparam name="TP">The property type.</typeparam>
        /// <returns>All entities that match.</returns>
        public List<TP> GetAllByType<TP>() where TP: Property
        {
            List<TP> result = new List<TP>();
            foreach (T ent in EntityList)
            {
                if (ent.TryGetProperty(out TP resAdd))
                {
                    result.Add(resAdd);
                }
            }
            return result;
        }

        /// <summary>
        /// Gets any one entity with a specific property.
        /// <para>This does not care for any order if multiple entities contain the property.</para>
        /// <para>This works best when only one entity will ever have a certain property in an engine.
        /// For example, the main player, or a game controller.</para>
        /// <para>Returns null if none found.</para>
        /// </summary>
        /// <typeparam name="TP">The property type.</typeparam>
        /// <returns>One entity, or null.</returns>
        public TP GetAnyByType<TP>() where TP : Property
        {
            foreach (T ent in EntityList)
            {
                if (ent.TryGetProperty(out TP retme))
                {
                    return retme;
                }
            }
            return null;
        }

        /// <summary>
        /// Creates an entity.
        /// </summary>
        /// <param name="ticks">Whether it should tick.</param>
        /// <returns>The entity.</returns>
        public abstract T CreateEntity(bool ticks);

        /// <summary>
        /// Spawns an entity into the world.
        /// </summary>
        /// <param name="ticks">Whether it should tick.</param>
        /// <param name="configure">A method to configure the entity prior to spawn, if one applies.</param>
        /// <param name="props">Any properties to apply.</param>
        /// <returns>The spawned entity.</returns>
        public T SpawnEntity(bool ticks, Action<T> configure, params Property[] props)
        {
            T ce = CreateEntity(ticks);
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
                if (prop is BasicEntityProperty<T, T2> bep)
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
        public T SpawnEntity(bool ticks, params Property[] props)
        {
            return SpawnEntity(ticks, null, props);
        }

        /// <summary>
        /// Spawns an entity into the world.
        /// </summary>
        /// <param name="configure">A method to configure the entity prior to spawn, if one applies.</param>
        /// <param name="props">Any properties to apply.</param>
        /// <returns>The spawned entity.</returns>
        public T SpawnEntity(Action<T> configure, params Property[] props)
        {
            return SpawnEntity(true, configure, props);
        }

        /// <summary>
        /// Spawns an entity into the world.
        /// </summary>
        /// <param name="props">Any properties to apply.</param>
        /// <returns>The spawned entity.</returns>
        public T SpawnEntity(params Property[] props)
        {
            return SpawnEntity(true, null, props);
        }

        /// <summary>
        /// Removes an entity from the world.
        /// </summary>
        /// <param name="ent">The entity to remove.</param>
        public void DespawnEntity(T ent)
        {
            if (!ent.IsSpawned)
            {
                SysConsole.Output(OutputType.WARNING, "Despawing non-spawned entity.");
                return;
            }
            foreach (Property prop in ent.GetAllProperties())
            {
                if (prop is BasicEntityProperty<T, T2> bep)
                {
                    bep.OnDespawn();
                }
            }
            ent.OnDespawnEvent?.Fire(Schedule, new EntityDespawnEventArgs());
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
            IReadOnlyList<T> ents = EntityListDuplicate();
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
