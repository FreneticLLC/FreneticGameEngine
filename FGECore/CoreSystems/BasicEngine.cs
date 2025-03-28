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
using System.Threading;
using FGECore.EntitySystem;
using FGECore.EntitySystem.JointSystems;
using FGECore.PhysicsSystem;
using FGECore.PropertySystem;
using FGECore.StackNoteSystem;
using FGECore.UtilitySystems;

namespace FGECore.CoreSystems;

/// <summary>Represents the common code shared by a server and client engine.</summary>
public abstract class BasicEngine
{
    /// <summary>Random helper object.</summary>
    public MTRandom RandomHelper = new();

    /// <summary>The source object for this engine.</summary>
    public object Source => OwningInstanceGeneric.Source;

    /// <summary>Current frame delta (seconds).</summary>
    public double Delta;

    /// <summary>How long the game has run (seconds).</summary>
    public double GlobalTickTime = 1.0;

    /// <summary>The current highest EntityID value.</summary>
    public long CurrentEntityID = 1;

    /// <summary>The current highest JointID value.</summary>
    public long CurrentJointID = 1;

    /// <summary>The instance that owns this engine.</summary>
    public abstract GameInstance OwningInstanceGeneric { get; }

    /// <summary>The general-purpose physics world.</summary>
    public abstract PhysicsSpace PhysicsWorldGeneric { get; }

    /// <summary>Gets the scheduler from the backing GameInstance.</summary>
    public Scheduler Schedule => OwningInstanceGeneric.Schedule;

    /// <summary>Any/all joints currently registered into this engine.</summary>
    public Dictionary<long, GenericBaseJoint> Joints = new(512);

    /// <summary>Any/all non-physics joints currently registered into this engine.</summary>
    public List<NonPhysicalJointBase> NonPhysicalJoints = new(64);

    /// <summary>A cancel token source that can be used to indicate this engine instance is intended to shutdown. Many internal functions actively disable if this is set.</summary>
    public CancellationTokenSource EngineShutdownToken = new();

    /// <summary>Add and activate a joint into this engine.</summary>
    public void AddJoint(GenericBaseJoint joint)
    {
        if (joint.Added)
        {
            throw new InvalidOperationException("Cannot add a joint that is already added to an engine.");
        }
        joint.JointID = CurrentJointID++;
        AddJoint_WithJID(joint);
    }

    /// <summary>Add and activate a joint into this engine with an existing JointID.</summary>
    public void AddJoint_WithJID(GenericBaseJoint joint)
    {
        Joints.Add(joint.JointID, joint);
        if (joint is NonPhysicalJointBase nonPhysJoint)
        {
            NonPhysicalJoints.Add(nonPhysJoint);
        }
        joint.Added = true;
        joint.Enable();
        joint.EntityOne.Joints.Add(joint);
        joint.EntityTwo.Joints.Add(joint);
    }

    /// <summary>Remove and deactivate a joint from this engine, by ID. Returns true if a joint was removed, or false if the ID is invalid.</summary>
    public bool RemoveJoint(long jointID)
    {
        if (Joints.TryGetValue(jointID, out GenericBaseJoint joint))
        {
            RemoveJoint(joint);
            return true;
        }
        return false;
    }

    /// <summary>Remove and deactivate a joint from this engine.</summary>
    public void RemoveJoint(GenericBaseJoint joint)
    {
        if (!joint.Added || !Joints.Remove(joint.JointID))
        {
            throw new InvalidOperationException("Cannot remove a joint that is not added to an engine.");
        }
        if (joint is NonPhysicalJointBase nonPhysJoint)
        {
            NonPhysicalJoints.Remove(nonPhysJoint);
        }
        joint.Added = false;
        joint.Disable();
        joint.EntityOne.Joints.Remove(joint);
        joint.EntityTwo.Joints.Remove(joint);
    }

    /// <summary>Shuts down the <see cref="BasicEngine"/> and disposes any used resources.</summary>
    public virtual void Shutdown()
    {
        EngineShutdownToken.Cancel();
        Logs.Debug($"[BasicEngine/Shutdown] [In {OwningInstanceGeneric.Name}] Closing physics simulation...");
        PhysicsWorldGeneric.Shutdown();
    }
}

/// <summary>Represents the common code shared by a server and client engine, with generic types refering the implementation type.</summary>
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
        SysConsole.Output(OwningInstance.InitOutputType, "BasicEngine prepping physics helper...");
        PhysicsWorld = new PhysicsSpace<T, T2>(this);
    }

    /// <summary>All entities currently spawned in this engine.</summary>
    public Dictionary<long, T> Entities = new(8192);

    /// <summary>All entities currently spawned in the engine.</summary>
    public List<T> EntityList = new(8192);

    /// <summary>Returns a duplicate of the entity list, for when you expect the master list to change.</summary>
    public List<T> EntityListDuplicate()
    {
        return [.. EntityList];
    }

    /// <summary>Adds an entity to the server, quick and deadly. Prefer <see cref="SpawnEntity(Property[])"/> over this.</summary>
    /// <returns>True if added, false if add failed.</returns>
    public bool AddEntity(T entity)
    {
        if (Entities.TryAdd(entity.EID, entity))
        {
            EntityList.Add(entity);
            return true;
        }
        return false;
    }

    /// <summary>Removes an entity from the list, quick and deadly. Prefer <see cref="DespawnEntity(T)"/> over this.</summary>
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
    public abstract T CreateEntity();

    /// <summary>Spawns an entity into the world.</summary>
    /// <param name="configure">A method to configure the entity prior to spawn or property add, if one applies.</param>
    /// <param name="props">Any properties to apply.</param>
    public T SpawnEntity(Action<T> configure, params Property[] props)
    {
        try
        {
            StackNoteHelper.Push("BasicEngine - Spawn Entity", this);
            T ce = CreateEntity();
            ce.EID = CurrentEntityID++;
            try
            {
                StackNoteHelper.Push("BasicEngine - Configure Entity", ce);
                configure?.Invoke(ce);
                for (int i = 0; i < props.Length; i++)
                {
                    ce.AddProperty(props[i]);
                }
                while (!AddEntity(ce))
                {
                    Logs.Warning($"Entity with newly generated EID {ce.EID} failed to add - EID tracker may be corrupt, or save data may have been mixed. Re-attempting...");
                    ce.EID = CurrentEntityID++;
                }
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
    /// <param name="props">Any properties to apply.</param>
    public T SpawnEntity(params Property[] props)
    {
        return SpawnEntity(null, props);
    }

    /// <summary>Removes an entity from the world.</summary>
    public void DespawnEntity(T ent)
    {
        if (!ent.IsSpawned)
        {
            Logs.Warning("Despawing non-spawned entity.");
            return;
        }
        try
        {
            StackNoteHelper.Push("BasicEngine - Despawn Entity", ent);
            foreach (GenericBaseJoint joint in new List<GenericBaseJoint>(ent.Joints))
            {
                RemoveJoint(joint);
            }
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
        if (EngineShutdownToken.IsCancellationRequested)
        {
            return;
        }
        try
        {
            StackNoteHelper.Push("BasicEngine - Update Physics", PhysicsWorld);
            PhysicsWorld.Tick(Delta);
        }
        finally
        {
            StackNoteHelper.Pop();
        }
        try
        {
            StackNoteHelper.Push("BasicEngine - Update Joints", this);
            foreach (NonPhysicalJointBase joint in NonPhysicalJoints)
            {
                joint.Solve();
            }
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
            foreach (T ent in ents)
            {
                if (ent.OnTick is not null)
                {
                    try // TODO: This try/finally is a bit heavy to be running on *every* entity, can extra outside the loop possibly?
                    {
                        StackNoteHelper.Push("BasicEngine - Tick specific entity", ent);
                        ent.OnTick();
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

    /// <inheritdoc/>
    public override void Shutdown()
    {
        base.Shutdown();
        OwningInstance.Engines.Remove(this as T2);
    }
}
