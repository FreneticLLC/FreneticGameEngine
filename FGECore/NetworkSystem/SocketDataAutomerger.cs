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
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace FGECore.NetworkSystem;

/// <summary>Simple handy-dandy helper: auto-merges packet data chunks meant to be sent down a socket, (to avoid nagles algorithm muckery via TCP_NODELAY).</summary>
public class SocketDataAutomerger(Socket socket)
{
    /// <summary>The relevant socket.</summary>
    public Socket Socket = socket;

    /// <summary>Holder for pending data.</summary>
    public byte[] Pending = new byte[1024]; // Most networks have a practical limit of 1460-ish bytes of data in a TCP packet, so we undercut that as our bulking limit.

    /// <summary>Number of bytes currently set in <see cref="Pending"/>.</summary>
    public int PendingLength = 0;

    /// <summary><see cref="Interlocked"/> accessible counter of bytes currently trying to send over network.</summary>
    public long NetworkSendingNow = 0;

    /// <summary>Internal route to directly send data to the backing socket.</summary>
    public void InternalSend(byte[] data)
    {
        Interlocked.Add(ref NetworkSendingNow, data.Length);
        // Very careful call here: use Async so we don't block the caller thread, and use a `Task` overload not `ValueTask` to prevent C# borkery
        _ = Socket.SendAsync(data).ContinueWith(_ =>
        {
            Interlocked.Add(ref NetworkSendingNow, -data.Length);
        });
        // TODO: Error handling?
    }

    /// <summary>If there is data pending, send it now. If not, do nothing.</summary>
    public void Autopush()
    {
        if (PendingLength > 0)
        {
            // (This makes a copy, which is most likely redundant to copies in the socket internals, but just for paranoia's sake we're doing it anyway)
            InternalSend(Pending[0..PendingLength]);
            PendingLength = 0;
        }
    }

    /// <summary>Send a data packet down the socket. May send now or later.</summary>
    public void Send(byte[] data)
    {
        if (data.Length + PendingLength >= Pending.Length)
        {
            Autopush();
        }
        if (data.Length >= Pending.Length)
        {
            InternalSend(data);
        }
        else
        {
            Array.Copy(data, 0, Pending, PendingLength, data.Length);
            PendingLength += data.Length;
        }
    }
}
