using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FreneticGameCore.EntitySystem;
using FreneticGameCore.ServerSystem.EntitySystem;

namespace FreneticGameCore.ServerSystem
{
    /// <summary>
    /// Represents an engine on the server side.
    /// </summary>
    public class ServerEngine : BasicEngine<ServerEntity, ServerEngine>
    {
        /// <summary>
        /// Creates an entity.
        /// </summary>
        /// <param name="ticks">Whether it ticks.</param>
        /// <returns>The entity.</returns>
        public override ServerEntity CreateEntity(bool ticks)
        {
            return new ServerEntity(this, ticks);
        }
    }
}
