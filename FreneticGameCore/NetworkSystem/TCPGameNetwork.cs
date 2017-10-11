using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;

namespace FreneticGameCore.NetworkSystem
{
    /// <summary>
    /// A TCP-based game network engine (Server).
    /// </summary>
    public class TCPGameNetwork : GameNetwork
    {
        /// <summary>
        /// The TCP listening backend.
        /// </summary>
        public TcpListener Listening = null;

        /// <summary>
        /// All present connections.
        /// </summary>
        public List<TCPConnection> Connections = new List<TCPConnection>();

        /// <summary>
        /// All presently registered channels.
        /// </summary>
        public HashSet<int> Channels = new HashSet<int>() { 0 };

        /// <summary>
        /// Start the network engine.
        /// </summary>
        public override void Start()
        {
            if (Socket.OSSupportsIPv6)
            {
                try
                {
                    Listening = new TcpListener(IPAddress.IPv6Any, Port);
                    Listening.Server.SetSocketOption(SocketOptionLevel.IPv6, SocketOptionName.IPv6Only, false);
                }
                catch (Exception ex)
                {
                    SysConsole.Output("Opening IPv6/IPv4 combo-socket", ex);
                    Listening = new TcpListener(IPAddress.Any, Port);
                }
            }
            else
            {
                Listening = new TcpListener(IPAddress.Any, Port);
            }
            Listening.Start(100);
        }

        /// <summary>
        /// Tick the network engine.
        /// </summary>
        public override void Tick()
        {
            while (Listening.Pending())
            {
                Socket s = Listening.AcceptSocket();
                Connections.Add(new TCPConnection() { RelevantSocket = s, Network = this });
            }
            for (int i = Connections.Count - 1; i >= 0; i--)
            {
                Connections[i].Tick();
                if (Connections[i].RelevantSocket == null)
                {
                    Connections.RemoveAt(i);
                }
            }
        }

        /// <summary>
        /// Tell the network engine to prepare a channel for all future clients.
        /// <para>Generally set this first, before enabling the engine.</para>
        /// </summary>
        /// <param name="id">The channel ID.</param>
        public override void RegisterChannel(int id)
        {
            Channels.Add(id);
        }

        /// <summary>
        /// Maximum size of a header data.
        /// </summary>
        public int HeaderLimit = 1024 * 10;

        /// <summary>
        /// Set this to control whether a game connection is permitted or denied.
        /// </summary>
        public Func<TCPConnection, bool> WantsReady;
    }
}
