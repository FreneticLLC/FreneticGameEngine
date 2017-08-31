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
    public class ServerEngine : BasicEngine
    {
        /// <summary>
        /// All entities currently on this server, if EIDs are used.
        /// </summary>
        public Dictionary<long, ServerEntity> Entities = new Dictionary<long, ServerEntity>(8192);

        /// <summary>
        /// All entities currently on the server.
        /// </summary>
        public List<ServerEntity> EntityList = new List<ServerEntity>(8192);

        /// <summary>
        /// Adds an entity to the server.
        /// </summary>
        /// <param name="be">The entity.</param>
        public override void AddEntity(BasicEntity be)
        {
            EntityList.Add(be as ServerEntity);
            Entities.Add(be.EID, be as ServerEntity);
        }

        /// <summary>
        /// Creates an entity.
        /// </summary>
        /// <param name="ticks">Whether it ticks.</param>
        /// <returns>The entity.</returns>
        public override BasicEntity CreateEntity(bool ticks)
        {
            return new ServerEntity(this, ticks);
        }

        /// <summary>
        /// Creates a duplicate list of entities on the server.
        /// </summary>
        /// <returns>The entity list.</returns>
        public override IReadOnlyList<BasicEntity> EntityListDuplicate()
        {
            return new List<BasicEntity>(Entities.Values.Cast<BasicEntity>());
        }

        /// <summary>
        /// Removes an entity from the list.
        /// </summary>
        /// <param name="be">The entity to remove.</param>
        public override void RemoveEntity(BasicEntity be)
        {
            EntityList.Remove(be as ServerEntity);
            Entities.Remove(be.EID);
        }
    }
}
