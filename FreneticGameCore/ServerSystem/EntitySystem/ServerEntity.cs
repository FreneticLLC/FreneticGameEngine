using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FreneticGameCore.EntitySystem;
using FreneticGameCore;

namespace FreneticGameCore.ServerSystem.EntitySystem
{
    /// <summary>
    /// Represents an entity on the server side.
    /// </summary>
    public class ServerEntity : BasicEntity
    {
        /// <summary>
        /// Whether this entity should be sent over the network.
        /// </summary>
        public bool ShouldNetwork;

        // TODO: ServerEngine Server;

        /// <summary>
        /// Constructs a server-side entity.
        /// </summary>
        /// <param name="engine">The owning engine.</param>
        /// <param name="_ticks">Whether it should tick.</param>
        /// <param name="_shouldNetwork">Whether it should network.</param>
        public ServerEntity(BasicEngine engine, bool _ticks, bool _shouldNetwork = false) 
            : base(engine, _ticks)
        {
            ShouldNetwork = _shouldNetwork;
        }

        /// <summary>
        /// Gets a string debug helper for this entity.
        /// </summary>
        /// <returns>A string.</returns>
        public override string ToString()
        {
            return "ServerEntity of type: " + GetType().Name + ", ID: " + EID;
        }
    }
}
