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
using FGECore.FileSystems;

namespace FGECore.NetworkSystem
{
    /// <summary>
    /// Represents a data packet that was received by the network.
    /// </summary>
    public abstract class DataPacketIn
    {
        /// <summary>
        /// The source object. Generally, the connection object is held by this.
        /// </summary>
        public abstract object Source { get; }

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
        public override object Source
        {
            get
            {
                return Connection;
            }
        }
    }
}
