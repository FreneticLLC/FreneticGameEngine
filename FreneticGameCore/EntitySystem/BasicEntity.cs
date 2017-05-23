using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FreneticScript;
using BEPUutilities;

namespace FreneticGameCore.EntitySystem
{
    /// <summary>
    /// The base most class of an entity in the FreneticGameEngine.
    /// </summary>
    public abstract class BasicEntity : PropertyHolder
    {
        /// <summary>
        /// The owning engine.
        /// </summary>
        public BasicEngine Engine;

        /// <summary>
        /// Whether the entity should tick normally.
        /// <para>Note: Setting this after it's spawned is not required to validly modify its value.</para>
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
        /// </summary>
        public Action OnTick;

        /// <summary>
        /// Fired when the entity is moved.
        /// </summary>
        public Action<Location> OnPositionChanged;

        /// <summary>
        /// Fired when the entity's orientation is changed.
        /// </summary>
        public Action<Quaternion> OnOrientationChanged;

        /// <summary>
        /// Fired when this entity is spawned into a world.
        /// </summary>
        public FreneticScriptEventHandler<EntitySpawnEventArgs> OnSpawnEvent = new FreneticScriptEventHandler<EntitySpawnEventArgs>();

        /// <summary>
        /// Fired when this entity is de-spawned out of a world.
        /// </summary>
        public FreneticScriptEventHandler<EntityDeSpawnEventArgs> OnDeSpawnEvent = new FreneticScriptEventHandler<EntityDeSpawnEventArgs>();

        /// <summary>
        /// Construct the basic Entity.
        /// </summary>
        /// <param name="eng">The owning engine.</param>
        /// <param name="_ticks">Whether the entity ticks.</param>
        public BasicEntity(BasicEngine eng, bool _ticks)
        {
            Engine = eng;
            Ticks = _ticks;
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
    public class EntitySpawnEventArgs : FreneticEventArgs
    {
    }

    /// <summary>
    /// Represents the arguments to an entity de-spawn event.
    /// </summary>
    public class EntityDeSpawnEventArgs : FreneticEventArgs
    {
    }
}
