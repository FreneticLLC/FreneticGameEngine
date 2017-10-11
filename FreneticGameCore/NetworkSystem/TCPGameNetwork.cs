﻿using System;
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
        /// A mapping of packet names to their packet data.
        /// </summary>
        public Dictionary<string, KeyValuePair<long, Func<ServerTCPDataPacketIn>>> NamesToPackets = new Dictionary<string, KeyValuePair<long, Func<ServerTCPDataPacketIn>>>();

        /// <summary>
        /// Packets, listed by ID number.
        /// </summary>
        public List<Func<ServerTCPDataPacketIn>> Packets = new List<Func<ServerTCPDataPacketIn>>();

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
        /// Constructs the TCP Game Network (server).
        /// </summary>
        public TCPGameNetwork()
        {
            // TODO: Register default packets.
        }

        /// <summary>
        /// Registers a packet, returning the new ID. May not register the same name twice!
        /// </summary>
        /// <param name="name">The name of the packet.</param>
        /// <param name="packetGetter">The packet getter function.</param>
        /// <returns>The ID.</returns>
        public long RegisterPacket(string name, Func<ServerTCPDataPacketIn> packetGetter)
        {
            long id = Packets.Count;
            NamesToPackets.Add(name, new KeyValuePair<long, Func<ServerTCPDataPacketIn>>(id, packetGetter));
            Packets.Add(packetGetter);
            return id;
        }

        /// <summary>
        /// Maximum size of a header data.
        /// </summary>
        public int HeaderLimit = 1024 * 10;

        /// <summary>
        /// Maximum size of a waiting packet.
        /// </summary>
        public int MaxPacketWaiting = 1024 * 1024 * 10;

        /// <summary>
        /// Set this to control whether a game connection is permitted or denied.
        /// <para>If return is true, the connection will be marked ready and loaded in.</para>
        /// <para>If intending to return true, this is a good spot to set <see cref="TCPConnection.Tag"/> and spawn any relevant objects.</para>
        /// </summary>
        public Func<TCPConnection, bool> WantsReady;
    }
}