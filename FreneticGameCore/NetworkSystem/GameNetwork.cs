using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FreneticGameCore.NetworkSystem
{
    /// <summary>
    /// Represents a game networking solution (Server).
    /// </summary>
    public abstract class GameNetwork
    {
        /// <summary>
        /// Tick the network engine.
        /// </summary>
        public abstract void Tick();

        /// <summary>
        /// Start the network engine.
        /// </summary>
        public abstract void Start();

        /// <summary>
        /// Tell the network engine to prepare a channel for all future clients.
        /// <para>Generally set this first, before enabling the engine.</para>
        /// </summary>
        /// <param name="id">The channel ID.</param>
        public abstract void RegisterChannel(int id);

        /// <summary>
        /// The primary port in use by this GameNetwork.
        /// <para>A network engine is not expected to honor late-changes to this value: set it before the engine starts!</para>
        /// </summary>
        public int Port = 28010;
    }
}
