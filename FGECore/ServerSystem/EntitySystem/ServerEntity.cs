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
using FGECore;
using FGECore.CoreSystems;
using FGECore.EntitySystem;
using FGECore.MathHelpers;

namespace FGECore.ServerSystem.EntitySystem;

/// <summary>Represents an entity on the server side.</summary>
/// <param name="engine">The owning engine.</param>
/// <param name="_shouldNetwork">Whether it should network.</param>
public class ServerEntity(ServerEngine engine, bool _shouldNetwork = false) : BasicEntity<ServerEntity, ServerEngine>(engine)
{
    /// <summary>Whether this entity should be sent over the network.</summary>
    public bool ShouldNetwork = _shouldNetwork;

    /// <summary>Gets the backing server engine.</summary>
    public ServerEngine Server
    {
        get
        {
            return Engine as ServerEngine;
        }
    }

    /// <summary>Gets a string debug helper for this entity.</summary>
    public override string ToString()
    {
        return $"ServerEntity of type: {GetType().Name}, ID: {EID}, with properties: {PropertyList()}";
    }
}
