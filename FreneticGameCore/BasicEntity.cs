using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FreneticScript;

namespace FreneticGameCore
{
    /// <summary>
    /// The base most class of an entity in the FreneticGameEngine.
    /// </summary>
    public abstract class BasicEntity : PropertyHolder
    {
        /// <summary>
        /// Whether the entity should tick normally.
        /// </summary>
        public readonly bool Ticks;

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
        /// Fired when this entity is spawned into a world.
        /// </summary>
        public FreneticScriptEventHandler<EntitySpawnEventArgs> OnSpawn = new FreneticScriptEventHandler<EntitySpawnEventArgs>();

        /// <summary>
        /// Fired when this entity is de-spawned out of a world.
        /// </summary>
        public FreneticScriptEventHandler<EntityDeSpawnEventArgs> OnDeSpawn = new FreneticScriptEventHandler<EntityDeSpawnEventArgs>();

        /// <summary>
        /// Construct the basic Entity.
        /// </summary>
        /// <param name="_ticks">Whether the entity ticks.</param>
        public BasicEntity(bool _ticks)
        {
            Ticks = _ticks;
        }

        /// <summary>
        /// Tick the entity. Default implementation throws an exception!
        /// </summary>
        public virtual void Tick()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Gets a string debug helper for this entity.
        /// </summary>
        /// <returns>A string.</returns>
        public override string ToString()
        {
            return "BasicEntity of type: " + GetType().Name;
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
    public class EntityDeSpawnEventArgs : EventArgs
    {
    }
}
