using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FreneticGameCore.Files;

namespace FreneticGameCore.NetworkSystem
{
    /// <summary>
    /// Represents a data packet that was received by the network.
    /// </summary>
    public abstract class DataPacketIn
    {
        /// <summary>
        /// The source object. Generally, the connection object is held by this.
        /// </summary>
        public abstract Object Source { get; }

        /// <summary>
        /// Process and run the data input.
        /// </summary>
        /// <param name="reader">The data reader.</param>
        public abstract void ProcessAndRun(DataReader reader);
    }

    /// <summary>
    /// Represents a data packet that was received by the server.
    /// </summary>
    public abstract class ServerDataPacketIn : DataPacketIn
    {
    }

    /// <summary>
    /// Represents a data packet that was received by the TCP server.
    /// </summary>
    public abstract class ServerTCPDataPacketIn : ServerDataPacketIn
    {
        /// <summary>
        /// The TCP connection object.
        /// </summary>
        public TCPConnection Connection = null;

        /// <summary>
        /// Gets the source. Prefer <see cref="Connection"/>.
        /// </summary>
        public override Object Source
        {
            get
            {
                return Connection;
            }
        }
    }
}
