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
using FreneticUtilities.FreneticToolkit;
using FGECore.CoreSystems;
using FGECore.EntitySystem.JointSystems;
using FGECore.FileSystems;
using FGECore.MathHelpers;
using FGECore.PropertySystem;

namespace FGECore.EntitySystem;

/// <summary>The base most class of an entity in the FreneticGameEngine.</summary>
public abstract class BasicEntity : PropertyHolder
{
    /// <summary>The internal Entity ID (Identifier) number for this entity.</summary>
    public long EID;

    /// <summary>Whether the entity is spawned into the world.</summary>
    public bool IsSpawned;

    /// <summary>The entity is removed from the owning world, or will be momentarily.</summary>
    public bool Removed = false;

    /// <summary>
    /// Fired when the entity is being ticked.
    /// TODO: Actual event?
    /// </summary>
    public Action OnTick;

    /// <summary>Last position known that this entity was or is exactly upon.</summary>
    public Location LastKnownPosition;

    /// <summary>Last orientation known that this entity was or is exactly upon.</summary>
    public Quaternion LastKnownOrientation = Quaternion.Identity;

    /// <summary>
    /// Fired when the entity is moved.
    /// TODO: Actual event?
    /// </summary>
    public Action<Location> OnPositionChanged;

    /// <summary>
    /// Fired when the entity's orientation is changed.
    /// TODO: Actual event?
    /// </summary>
    public Action<Quaternion> OnOrientationChanged;

    /// <summary>Fired when this entity is spawned into a world.</summary>
    public FreneticEvent<EntitySpawnEventArgs> OnSpawnEvent;

    /// <summary>Fired when this entity is despawned out of a world.</summary>
    public FreneticEvent<EntityDespawnEventArgs> OnDespawnEvent;

    /// <summary>All joints attached to this entity.</summary>
    public HashSet<GenericBaseJoint> Joints = [];

    /// <summary>The owning engine.</summary>
    public abstract BasicEngine EngineGeneric { get; }

    /// <summary>Constructs the <see cref="BasicEntity"/>.</summary>
    public BasicEntity()
    {
    }

    /// <summary>
    /// Rotates the entity around a normalized axis by an angle, relative to its current orientation (based on <see cref="LastKnownOrientation"/>).
    /// <para>Updates only the orientation. May act differently than expected for some types of entities (such as physics based entities with a center offset).</para>
    /// </summary>
    /// <param name="axis">The normalized axis.</param>
    /// <param name="angle">The angle.</param>
    public void RotateAround(Location axis, double angle)
    {
        Rotate(Quaternion.FromAxisAngle(axis, angle));
    }

    /// <summary>Rotates the entity relative to its current orientation (based on <see cref="LastKnownOrientation"/>).</summary>
    /// <param name="rotation">The rotation to take, described as a relative offset quaternion.</param>
    public void Rotate(Quaternion rotation)
    {
        SetOrientation(LastKnownOrientation * rotation);
    }

    /// <summary>Moves the entity relative to its current position (based on <see cref="LastKnownPosition"/>).</summary>
    /// <param name="x">X motion.</param>
    /// <param name="y">Y motion.</param>
    /// <param name="z">Z motion, if any.</param>
    public void MoveRelative(double x, double y, double z = 0)
    {
        SetPosition(LastKnownPosition + new Location(x, y, z));
    }

    /// <summary>Moves the entity relative to its current position (based on <see cref="LastKnownPosition"/>).</summary>
    /// <param name="motion">The motion to make, described as a relative offset vector.</param>
    public void MoveRelative(Location motion)
    {
        SetPosition(LastKnownPosition + motion);
    }

    /// <summary>Sets the orientation of the entity.</summary>
    /// <param name="orientation">The new orientation quaternion.</param>
    public void SetOrientation(Quaternion orientation)
    {
        LastKnownOrientation = orientation;
        OnOrientationChanged?.Invoke(orientation);
    }

    /// <summary>Sets the position of the entity.</summary>
    /// <param name="position">New position.</param>
    public void SetPosition(Location position)
    {
        LastKnownPosition = position;
        OnPositionChanged?.Invoke(position);
    }

    /// <summary>Gets a string debug helper for this entity.</summary>
    public override string ToString()
    {
        return $"BasicEntity of type: {GetType().Name}, ID: {EID}, with properties: {PropertyList()}";
    }

    /// <summary>Gets a string list of all properties, with debug informational output.</summary>
    public string DebugPropList()
    {
        return string.Join(" | ", EnumerateAllProperties().Select((p) => p.GetPropertyName() + ": {" + string.Join(", ", p.GetDebuggable().Select((k) => k.Key + ": " + k.Value)) + "}"));
    }

    /// <summary>Gets a string list of all property names, pipe separated.</summary>
    public string PropertyList()
    {
        return string.Join(" | ", EnumerateAllPropertyTypes().Select(t => t.Name));
    }

    /// <summary>
    /// Saves the entity's data to a DataWriter, appending its generated strings to a string list and lookup table.
    /// <para>Is not a compiled method (Meaning, this method is reflection-driven)!</para>
    /// </summary>
    /// <param name="dw">Data writer to use.</param>
    /// <param name="strs">Strings to reference.</param>
    /// <param name="strMap">The string lookup table.</param>
    public void SaveNC(DataWriter dw, List<string> strs, Dictionary<string, int> strMap)
    {
        dw.WriteLong(EID);
        dw.WriteBool(IsSpawned);
        IEnumerable<Property> props = EnumerateAllProperties();
        dw.WriteVarInt(props.Count());
        foreach (Property saveme in props)
        {
            string nm = saveme.GetType().FullName;
            if (!strMap.TryGetValue(nm, out int id))
            {
                id = strs.Count;
                strs.Add(nm);
                strMap[nm] = id;
            }
            dw.WriteVarInt(id);
            saveme.Helper.SaveNC(saveme, dw, strs, strMap);
        }
    }

    /// <summary>Fired when a property is added.</summary>
    public override void OnAdded(Property property)
    {
        if (IsSpawned && property is BasicEntityProperty bep)
        {
            bep.OnSpawn();
        }
    }

    /// <summary>Fired when a property is removed.</summary>
    public override void OnRemoved(Property property)
    {
        if (IsSpawned && property is BasicEntityProperty bep)
        {
            bep.OnDespawn();
        }
    }

    /// <summary>Rotates the entity to be "Z-Up" if it was originally "Y-Up".</summary>
    public void RotateYToZ()
    {
        SetOrientation(Y2Z);
    }

    /// <summary>Used with <see cref="RotateYToZ"/>.</summary>
    private static Quaternion Y2Z = Quaternion.GetQuaternionBetween(Location.UnitY, Location.UnitZ);
}

/// <summary>The base most class of an entity in the FreneticGameEngine, with generic types refering the implementation type.</summary>
public abstract class BasicEntity<T, T2> : BasicEntity where T : BasicEntity<T, T2> where T2 : BasicEngine<T, T2>
{
    /// <summary>The owning engine.</summary>
    public T2 Engine;

    /// <summary>The owning engine.</summary>
    public override BasicEngine EngineGeneric => Engine;

    /// <summary>Construct the basic Entity.</summary>
    /// <param name="eng">The owning engine.</param>
    public BasicEntity(T2 eng)
    {
        Engine = eng;
        OnSpawnEvent = new FreneticEvent<EntitySpawnEventArgs>(Engine.Schedule.EventHelper);
        OnDespawnEvent = new FreneticEvent<EntityDespawnEventArgs>(Engine.Schedule.EventHelper);

    }
}

/// <summary>Represents the arguments to an entity spawn event.</summary>
public class EntitySpawnEventArgs : FreneticEventArgs
{
}

/// <summary>Represents the arguments to an entity de-spawn event.</summary>
public class EntityDespawnEventArgs : FreneticEventArgs
{
}
