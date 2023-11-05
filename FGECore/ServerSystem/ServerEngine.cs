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
using FGECore.CoreSystems;
using FGECore.EntitySystem;
using FGECore.ServerSystem.EntitySystem;

namespace FGECore.ServerSystem;

/// <summary>Represents an engine on the server side.</summary>
public class ServerEngine : BasicEngine<ServerEntity, ServerEngine>
{
    /// <summary>Creates an entity.</summary>
    /// <param name="ticks">Whether it ticks.</param>
    /// <returns>The entity.</returns>
    public override ServerEntity CreateEntity(bool ticks)
    {
        return new ServerEntity(this, ticks);
    }
}
