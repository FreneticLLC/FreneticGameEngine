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
using FreneticGameCore.EntitySystem;

namespace FreneticGameCore.ServerSystem.EntitySystem
{
    /// <summary>
    /// Represents an entity physics-2D-limit property on the server.
    /// </summary>
    public class ServerEntityPhysics2DLimitProperty : EntityPhysics2DLimitProperty<ServerEntity, ServerEngine, ServerEntityPhysicsProperty>
    {
    }
}
