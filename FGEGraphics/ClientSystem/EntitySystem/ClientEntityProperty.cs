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

namespace FGEGraphics.ClientSystem.EntitySystem;

/// <summary>Represents a property on a client-side entity.</summary>
public class ClientEntityProperty : BasicEntityProperty<ClientEntity, GameEngineBase>
{
    /// <summary>Gets the associated client game engine, as a <see cref="GameEngine2D"/> (only works when the engine is 3D, of course)</summary>
    public GameEngine2D Engine2D
    {
        get
        {
            return Entity.Engine as GameEngine2D;
        }
    }

    /// <summary>Gets the associated client game engine, as a <see cref="GameEngine3D"/> (only works when the engine is 3D, of course).</summary>
    public GameEngine3D Engine3D
    {
        get
        {
            return Entity.Engine as GameEngine3D;
        }
    }
}
