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
using FGECore.StackNoteSystem;
using FGECore.UtilitySystems;
using FGECore.PhysicsSystem;
using FGECore.PropertySystem;

namespace FGECore.CoreSystems
{
    /// <summary>Represents the common code shared by a server and client engine.</summary>
    public abstract class BasicEngine
    {
        /// <summary>Random helper object.</summary>
        public MTRandom RandomHelper = new MTRandom();

        /// <summary>The source object for this engine. Set to any tag style constant reference you find most helpful to keep!</summary>
        public Object Source;

        /// <summary>Current frame delta (seconds).</summary>
        public double Delta;

        /// <summary>How long the game has run (seconds).</summary>
        public double GlobalTickTime = 1.0;

        /// <summary>The current highest EID value.</summary>
        public long CurrentEID = 1;

        /// <summary>The instance that owns this engine.</summary>
        public abstract GameInstance OwningInstanceGeneric { get; }

        /// <summary>The general-purpose physics world.</summary>
        public abstract PhysicsSpace PhysicsWorldGeneric { get; }

        /// <summary>Gets the scheduler from the backing GameInstance.</summary>
        public Scheduler Schedule => OwningInstanceGeneric.Schedule;

        /// <summary>Shuts down the <see cref="BasicEngine"/> and disposes any used resources.</summary>
        public void Shutdown()
        {
            PhysicsWorldGeneric.Shutdown();
        }
    }

    /// <summary>
    /// Represents the common code shared by a server and client engine, with generic types refering the implementation type.
    /// </summary>
    public abstract class BasicEngine<T, T2> : BasicEngine where T : BasicEntity<T, T2> where T2: BasicEngine<T, T2>
    {
        /// <summary>The general-purpose physics world.</summary>
        public PhysicsSpace<T, T2> PhysicsWorld;

        /// <summary>The general-purpose physics world.</summary>
        public sealed override PhysicsSpace PhysicsWorldGeneric => PhysicsWorld;

        /// <summary>The instance that owns this engine.</summary>
        public GameInstance<T, T2> OwningInstance;

        /// <summary>The instance that owns this engine.</summary>
        public sealed override GameInstance OwningInstanceGeneric => OwningInstance;

        /// <summary>Loads the basic engine.</summary>
        public void LoadBasic()
        {
            SysConsole.Output(OutputType.INIT, "BasicEngine prepping physics helper...");
            PhysicsWorld = new PhysicsSpace<T, T2>();
        }

        /// <summary>All entities currently on this server, if EIDs are used.</summary>
        public Dictionary<long, T> Entities = new Dictionary<long, T>(8192);

        /// <summary>All entities currently on the server.</summary>
        public List<T> EntityList = new List<T>(8192);

        /// <summary>
        /// Returns a duplicate of the entity list, for when you expect the master list to change.
        /// </summary>
        public IReadOnlyList<T> EntityListDuplicate()
        {
            return new List<T>(EntityList);
        }

        /// <summary>Adds an entity to the server, quick and deadly. Prefer <see cref="SpawnEntity(Property[])"/> over this.</summary>
        public void AddEntity(T entity)
        {
            EntityList.Add(entity);
            Entities.Add(entity.EID, entity);
        }

        /// <summary>
        /// Removes an entity from the list, quick and deadly. Prefer <see cref="DespawnEntity(T)"/> over this.
        /// </summary>
        public void RemoveEntity(T entity)
        {
            EntityList.Remove(entity);
            Entities.Remove(entity.EID);
        }

        /// <summary>Gets all properties with a specific property type from any and all entities currently spawned.</summary>
        public IEnumerable<TP> GetAllByType<TP>() where TP: Property
        {
            foreach (T ent in EntityList)
            {
                if (ent.TryGetProperty(out TP resAdd))
                {
                    yield return resAdd;
                }
            }
        }

        /// <summary>Gets all properties with a specific property type from any and all entities currently spawned.</summary>
        public IEnumerable<Property> GetAllByType(Type t)
        {
            foreach (T ent in EntityList)
            {
                if (ent.TryGetProperty(t, out Property resAdd))
                {
                    yield return resAdd;
                }
            }
        }

        /// <summary>
        /// Gets all properties that are a sub-type of the given property type from any and all entities currently spawned.
        /// <para>This can return multiple properties for any given entity.</para>
        /// </summary>
        public IEnumerable<TP> GetAllSubTypes<TP>() where TP : Property
        {
            foreach (T ent in EntityList)
            {
                foreach (TP prop in ent.GetAllSubTypes<TP>())
                {
                    yield return prop;
                }
            }
        }

        /// <summary>
        /// Gets all properties that are a sub-type of the given property type from any and all entities currently spawned.
        /// <para>This can return multiple properties for any given entity.</para>
        /// </summary>
        public IEnumerable<Property> GetAllSubTypes(Type t)
        {
            foreach (T ent in EntityList)
            {
                foreach (Property prop in ent.GetAllSubTypes(t))
                {
                    yield return prop;
                }
            }
        }

        /// <summary>
        /// Gets any one property with a specific property type from any and all entities currently spawned.
        /// <para>This does not care for any order if multiple entities contain the property.</para>
        /// <para>This works best when only one entity will ever have a certain property in an engine.
        /// For example, the main player, or a game controller.</para>
        /// <para>Returns null if none found.</para>
        /// </summary>
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
        /// Gets any one property with a specific property type from any and all entities currently spawned.
        /// <para>This does not care for any order if multiple entities contain the property.</para>
        /// <para>This works best when only one entity will ever have a certain property in an engine.
        /// For example, the main player, or a game controller.</para>
        /// <para>Returns null if none found.</para>
        /// </summary>
        public Property GetAnyByType(Type t)
        {
            foreach (T ent in EntityList)
            {
                if (ent.TryGetProperty(t, out Property retme))
                {
                    return retme;
                }
            }
            return null;
        }

        /// <summary>Creates an entity.</summary>
        /// <param name="ticks">Whether it should tick.</param>
        public abstract T CreateEntity(bool ticks);

        /// <summary>Spawns an entity into the world.</summary>
        /// <param name="ticks">Whether it should tick.</param>
        /// <param name="configure">A method to configure the entity prior to spawn or property add, if one applies.</param>
        /// <param name="props">Any properties to apply.</param>
        public T SpawnEntity(bool ticks, Action<T> configure, params Property[] props)
        {
            try
            {
                StackNoteHelper.Push("BasicEngine - Spawn Entity", this);
                T ce = CreateEntity(ticks);
                ce.EID = CurrentEID++;
                try
                {
                    StackNoteHelper.Push("BasicEngine - Configure Entity", ce);
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
                    ce.OnSpawnEvent?.Fire(new EntitySpawnEventArgs());
                }
                finally
                {
                    StackNoteHelper.Pop();
                }
                return ce;
            }
            finally
            {
                StackNoteHelper.Pop();
            }
        }

        /// <summary>Spawns an entity into the world.</summary>
        /// <param name="ticks">Whether it should tick.</param>
        /// <param name="props">Any properties to apply.</param>
        public T SpawnEntity(bool ticks, params Property[] props)
        {
            return SpawnEntity(ticks, null, props);
        }

        /// <summary>Spawns an entity into the world.</summary>
        /// <param name="configure">A method to configure the entity prior to spawn, if one applies.</param>
        /// <param name="props">Any properties to apply.</param>
        public T SpawnEntity(Action<T> configure, params Property[] props)
        {
            return SpawnEntity(true, configure, props);
        }

        /// <summary>Spawns an entity into the world.</summary>
        /// <param name="props">Any properties to apply.</param>
        public T SpawnEntity(params Property[] props)
        {
            return SpawnEntity(true, null, props);
        }

        /// <summary>Removes an entity from the world.</summary>
        public void DespawnEntity(T ent)
        {
            if (!ent.IsSpawned)
            {
                SysConsole.Output(OutputType.WARNING, "Despawing non-spawned entity.");
                return;
            }
            try
            {
                StackNoteHelper.Push("BasicEngine - Despawn Entity", ent);
                foreach (Property prop in ent.EnumerateAllProperties())
                {
                    if (prop is BasicEntityProperty bep)
                    {
                        bep.OnDespawn();
                    }
                }
                ent.OnDespawnEvent?.Fire(new EntityDespawnEventArgs());
                RemoveEntity(ent);
                ent.IsSpawned = false;
            }
            finally
            {
                StackNoteHelper.Pop();
            }
        }

        /// <summary>The internal engine tick sequence.</summary>
        public void Tick()
        {
            try
            {
                StackNoteHelper.Push("BasicEngine - Update Physics", PhysicsWorld);
                PhysicsWorld.Tick((float)Delta);
            }
            finally
            {
                StackNoteHelper.Pop();
            }
            try
            {
                StackNoteHelper.Push("BasicEngine - Tick all entities", this);
                // Dup list, to ensure ents can despawn themselves in the tick method!
                IReadOnlyList<T> ents = EntityListDuplicate();
                for (int i = 0; i < ents.Count; i++)
                {
                    if (ents[i].Ticks)
                    {
                        try
                        {
                            StackNoteHelper.Push("BasicEngine - Tick specific entity", ents[i]);
                            ents[i].TickThis();
                        }
                        finally
                        {
                            StackNoteHelper.Pop();
                        }
                    }
                }
            }
            finally
            {
                StackNoteHelper.Pop();
            }
        }
    }
}
