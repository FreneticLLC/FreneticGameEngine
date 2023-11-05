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
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using FreneticUtilities.FreneticToolkit;
using FGECore.CoreSystems;
using FGECore.FileSystems;
using FGECore.UtilitySystems;

namespace FGECore.NetworkSystem;

/// <summary>Represents a present TCP Connection.</summary>
public class TCPConnection : IDisposable
{
    /// <summary>The backing socket.</summary>
    public Socket RelevantSocket = null;

    /// <summary>The owning network system.</summary>
    public TCPGameNetwork Network;

    /// <summary>Whether the connection is fully ready to send and receive standard data.</summary>
    public bool IsReady = false;

    /// <summary>A temporary buffer for data handling.</summary>
    public DataStream ReadData = new();

    private readonly byte[] OneByteHolder = new byte[1];

    private readonly byte[] KiloByteHolder = new byte[1024];

    /// <summary>Any information used for the connection period.</summary>
    public string OpeningInformation;

    /// <summary>The static constant header value.</summary>
    public const string HEADER = "FRENETIC_GAME_ENGINE__CLIENT_CONNECT\n";

    /// <summary>The channel this Connection is on.</summary>
    public int Channel;

    /// <summary>Any tag applied to this Object by the game.</summary>
    public object Tag;

    /// <summary>Processes a received packet.</summary>
    /// <param name="pid">The packet ID.</param>
    /// <param name="reader">The data reader.</param>
    public void ProcessPacket(long pid, DataReader reader)
    {
        // TODO
    }

    /// <summary>The number of bytes waiting to send.</summary>
    public int SentWaiting = 0;

    /// <summary>Sends a packet through the socket.</summary>
    /// <param name="packID">The packet ID.</param>
    /// <param name="data">The data.</param>
    public void SendPacket(long packID, byte[] data)
    {
        if (RelevantSocket == null)
        {
            return;
        }
        DataStream outp = new();
        DataWriter dw = new(outp);
        dw.WriteInt(data.Length);
        dw.WriteVarInt(packID);
        dw.WriteBytes(data);
        SentWaiting += data.Length;
        try
        {
            RelevantSocket.BeginSend(data, 0, data.Length, SocketFlags.None, SendComplete, data.Length);
        }
        catch (Exception ex)
        {
            CommonUtilities.CheckException(ex);
            RelevantSocket.Close();
            RelevantSocket = null;
            OutputType.SERVERINFO.Output($"[Connections:Error] {ex}");
        }
    }

    /// <summary>Called when a send operation completes.</summary>
    /// <param name="res"></param>
    public void SendComplete(IAsyncResult res)
    {
        SentWaiting -= (int)res.AsyncState;
    }

    /// <summary>Run every frame to tick any network updates.</summary>
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
                        string str = StringConversionHelper.UTF8Encoding.GetString(ReadData.ToArray());
                        if (!str.StartsWith(HEADER))
                        {
                            throw new Exception("Connection refused: Not a game engine proper connection.");
                        }
                        str = str[HEADER.Length..];
                        int nextLine = str.IndexOf('\n', StringComparison.Ordinal);
                        string FGEData = str[..nextLine];
                        string[] keys = FGEData.Split(':');
                        int chan = int.Parse(keys[0]);
                        if (!Network.Channels.Contains(chan))
                        {
                            throw new Exception("Connection refused: Invalid (non-registered) channel.");
                        }
                        Channel = chan;
                        str = str[(nextLine + 1)..];
                        OpeningInformation = str;
                        if (Network.WantsReady?.Invoke(this) ?? true)
                        {
                            // TODO: Send counter header, all default packets, etc.
                            IsReady = true;
                        }
                        else
                        {
                            throw new Exception("Connection refused: Rejected by game systems.");
                        }
                        ReadData.SetLength(0);
                        return;
                    }
                    ReadData.WriteByte(OneByteHolder[0]);
                    if (ReadData.Length > Network.HeaderLimit)
                    {
                        throw new Exception("Connection refused: Too much header data.");
                    }
                    avail = RelevantSocket.Available;
                }
                return;
            }
            // If we reached here, IsReady = true.
            while (avail > 0)
            {
                int rd = RelevantSocket.Receive(KiloByteHolder, Math.Min(avail, KiloByteHolder.Length), SocketFlags.None);
                if (rd <= 0)
                {
                    return;
                }
                ReadData.Write(KiloByteHolder, 0, rd);
                if (ReadData.Length > Network.MaxPacketWaiting)
                {
                    throw new Exception("Connection quick-closed: massive packet!");
                }
                if (ReadData.Length > 5)
                {
                    rd = ReadData.Read(KiloByteHolder, 0, 4);
                    int d = BitConverter.ToInt32(KiloByteHolder, 0);
                    if (ReadData.Length >= d)
                    {
                        byte[] packet = new byte[d];
                        rd = 0;
                        while (rd < d)
                        {
                            int trd = ReadData.Read(packet, rd, d);
                            if (trd <= 0)
                            {
                                throw new Exception("Connection quick-closed: streaming error.");
                            }
                            rd += trd;
                        }
                        DataStream packStr = new(packet);
                        DataReader reader = new(packStr);
                        long pid = reader.ReadVarInt();
                        ProcessPacket(pid, reader);
                        if (ReadData.Length == 0)
                        {
                            ReadData.Ind = 0;
                        }
                    }
                    else
                    {
                        ReadData.Ind -= rd;
                    }
                }
                avail = RelevantSocket.Available;
            }
        }
        catch (Exception ex)
        {
            CommonUtilities.CheckException(ex);
            RelevantSocket.Close();
            RelevantSocket = null;
            OutputType.SERVERINFO.Output($"[Connections:Error] {ex.Message}");
        }
    }

    /// <summary>Dumb MS logic dispose method.</summary>
    /// <param name="disposing">Whether to dispose managed resources.</param>
    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            ReadData.Dispose();
            RelevantSocket.Dispose();
        }
    }

    /// <summary>Disposes the window client.</summary>
    public void Dispose()
    {
        GC.SuppressFinalize(this);
        Dispose(true);
    }
}
