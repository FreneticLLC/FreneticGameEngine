using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using FreneticGameCore.Files;

namespace FreneticGameCore.NetworkSystem
{
    /// <summary>
    /// Represents a present TCP Connection.
    /// </summary>
    public class TCPConnection
    {
        /// <summary>
        /// The backing socket.
        /// </summary>
        public Socket RelevantSocket = null;

        /// <summary>
        /// The owning network system.
        /// </summary>
        public TCPGameNetwork Network;

        /// <summary>
        /// Whether the connection is fully ready to send and receive standard data.
        /// </summary>
        public bool IsReady = false;

        /// <summary>
        /// A temporary buffer for data handling.
        /// </summary>
        public DataStream ReadData = new DataStream();

        private byte[] OneByteHolder = new byte[1];

        /// <summary>
        /// Any information used for the connection period.
        /// </summary>
        public string OpeningInformation;

        /// <summary>
        /// The static constant header value.
        /// </summary>
        public const string HEADER = "FRENETIC_GAME_ENGINE__CLIENT_CONNECT\n";

        /// <summary>
        /// The channel this Connection is on.
        /// </summary>
        public int Channel;

        /// <summary>
        /// Run every frame to tick any network updates.
        /// </summary>
        public void Tick()
        {
            try
            {
                int avail = RelevantSocket.Available;
                if (!IsReady)
                {
                    while (avail > 0)
                    {
                        int rd = RelevantSocket.Receive(OneByteHolder, 1, SocketFlags.None);
                        if (rd <= 0)
                        {
                            break;
                        }
                        if (OneByteHolder[0] == 0)
                        {
                            string str = FileHandler.DefaultEncoding.GetString(ReadData.ToArray());
                            if (!str.StartsWith(HEADER))
                            {
                                throw new Exception("Connection refused: Not a game engine proper connection.");
                            }
                            str = str.Substring(HEADER.Length);
                            int nextLine = str.IndexOf('\n');
                            string FGEData = str.Substring(0, nextLine);
                            string[] keys = FGEData.Split(':');
                            int chan = int.Parse(keys[0]);
                            if (!Network.Channels.Contains(chan))
                            {
                                throw new Exception("Connection refused: Invalid (non-registered) channel.");
                            }
                            Channel = chan;
                            str = str.Substring(nextLine + 1);
                            OpeningInformation = str;
                            if (Network.WantsReady?.Invoke(this) ?? true)
                            {
                                IsReady = true;
                            }
                            else
                            {
                                throw new Exception("Connection refused: Rejected by game systems.");
                            }
                            return;
                        }
                        ReadData.WriteByte(OneByteHolder[0]);
                        if (ReadData.Length > Network.HeaderLimit)
                        {
                            throw new Exception("Connection refused: Too much header data.");
                        }
                    }
                    return;
                }
                // If we reached here, IsReady = true.

            }
            catch (Exception ex)
            {
                Utilities.CheckException(ex);
                RelevantSocket = null;
            }
        }
    }
}
