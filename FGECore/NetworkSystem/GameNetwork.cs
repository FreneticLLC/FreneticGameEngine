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

namespace FGECore.NetworkSystem;

/// <summary>Represents a game networking solution (Server).</summary>
public abstract class GameNetwork
{
    /// <summary>Tick the network engine.</summary>
    public abstract void Tick();

    /// <summary>Start the network engine.</summary>
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
