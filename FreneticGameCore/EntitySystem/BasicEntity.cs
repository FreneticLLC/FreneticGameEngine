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
using BEPUutilities;

namespace FreneticGameCore.EntitySystem
{
    /// <summary>
    /// The base most class of an entity in the FreneticGameEngine.
    /// </summary>
    public abstract class BasicEntity<T> : PropertyHolder
    {
        /// <summary>
        /// The owning engine.
        /// </summary>
        public T Engine;

        /// <summary>
        /// Whether the entity should tick normally.
        /// <para>Note: Setting this after it's spawned is not guaranteed to validly modify its value.</para>
        /// </summary>
        public bool Ticks;

        /// <summary>
        /// The internal Entity ID (Identifier) number for this entity.
        /// </summary>
        public long EID;

        /// <summary>
        /// Whether the entity is spawned into the world.
        /// </summary>
        public bool IsSpawned;

        /// <summary>
        /// The entity is removed from the owning world, or will be momentarily.
        /// </summary>
        public bool Removed = false;

        /// <summary>
        /// Fired when the entity is being ticked.
        /// TODO: Actual event?
        /// </summary>
        public Action OnTick;

        /// <summary>
        /// Last position known that this entity was or is exactly upon.
        /// </summary>
        public Location LastKnownPosition;

        /// <summary>
        /// Last orientation known that this entity was or is exactly upon.
        /// </summary>
        public Quaternion LastKnownOrientation;

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

        /// <summary>
        /// Fired when this entity is spawned into a world.
        /// </summary>
        public FreneticEvent<EntitySpawnEventArgs> OnSpawnEvent = new FreneticEvent<EntitySpawnEventArgs>();

        /// <summary>
        /// Fired when this entity is despawned out of a world.
        /// </summary>
        public FreneticEvent<EntityDespawnEventArgs> OnDespawnEvent = new FreneticEvent<EntityDespawnEventArgs>();

        /// <summary>
        /// Rotates the entity around a normalized axis by an angle.
        /// <para>Updates only the orientation.</para>
        /// </summary>
        /// <param name="axis">The normalized axis.</param>
        /// <param name="angle">The angle.</param>
        public void RotateAround(Location axis, double angle)
        {
            SetOrientation(LastKnownOrientation * Quaternion.FromAxisAngle(axis, angle));
        }

        /// <summary>
        /// Moves the entity relative to its current position.
        /// </summary>
        /// <param name="x">X motion.</param>
        /// <param name="y">Y motion.</param>
        /// <param name="z">Z motion, if any..</param>
        public void MoveRelative(double x, double y, double z = 0)
        {
            SetPosition(LastKnownPosition + new Location(x, y, z));
        }

        /// <summary>
        /// Moves the entity relative to its current position.
        /// </summary>
        /// <param name="motion">The motion to make.</param>
        public void MoveRelative(Location motion)
        {
            SetPosition(LastKnownPosition + motion);
        }

        /// <summary>
        /// Sets the orientation of the entity.
        /// </summary>
        /// <param name="q">The new orientation quaternion.</param>
        public void SetOrientation(Quaternion q)
        {
            OnOrientationChanged?.Invoke(q);
        }

        /// <summary>
        /// Sets the position of the entity.
        /// </summary>
        /// <param name="p">New position.</param>
        public void SetPosition(Location p)
        {
            OnPositionChanged?.Invoke(p);
        }

        /// <summary>
        /// Sets the last known orientation to the input value.
        /// </summary>
        /// <param name="q">The last known orientation.</param>
        private void SetLKO(Quaternion q)
        {
            LastKnownOrientation = q;
        }

        /// <summary>
        /// Sets the last known position to the input value.
        /// </summary>
        /// <param name="p">The last known position.</param>
        private void SetLKP(Location p)
        {
            LastKnownPosition = p;
        }

        /// <summary>
        /// Construct the basic Entity.
        /// </summary>
        /// <param name="eng">The owning engine.</param>
        /// <param name="_ticks">Whether the entity ticks.</param>
        public BasicEntity(T eng, bool _ticks)
        {
            Engine = eng;
            Ticks = _ticks;
            OnPositionChanged += SetLKP;
            OnOrientationChanged += SetLKO;
        }

        /// <summary>
        /// Tick the entity. Just fires the tick event!
        /// </summary>
        public void TickThis()
        {
            OnTick?.Invoke();
        }

        /// <summary>
        /// Gets a string debug helper for this entity.
        /// </summary>
        /// <returns>A string.</returns>
        public override string ToString()
        {
            return "BasicEntity of type: " + GetType().Name + ", ID: " + EID + ", with properties: " + PropertyList();
        }

        /// <summary>
        /// Gets a string list of all properties, with debug informational output.
        /// </summary>
        /// <returns>The string list.</returns>
        public string DebugPropList()
        {
            return string.Join(" | ", GetAllProperties().ConvertStream((p) => p.GetPropertyName() + ": {" + string.Join(", ", p.GetDebuggable().ConvertStream((k) => k.Key + ": " + k.Value)) + "}"));
        }

        /// <summary>
        /// Gets a string list of all properties.
        /// </summary>
        /// <returns>The string list.</returns>
        public string PropertyList()
        {
            StringBuilder sb = new StringBuilder();
            foreach (Type t in GetAllPropertyTypes())
            {
                sb.Append(t.Name).Append(" | ");
            }
            return sb.Length > 0 ? sb.ToString().Substring(0, sb.Length - 3) : "";
        }
    }

    /// <summary>
    /// Represents the arguments to an entity spawn event.
    /// </summary>
    public class EntitySpawnEventArgs : EventArgs
    {
    }

    /// <summary>
    /// Represents the arguments to an entity de-spawn event.
    /// </summary>
    public class EntityDespawnEventArgs : EventArgs
    {
    }
}
