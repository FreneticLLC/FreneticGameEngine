//
// This file is created by Frenetic LLC.
// This code is Copyright (C) 2017 Frenetic LLC under the terms of a strict license.
// See README.md or LICENSE.txt in the source root for the contents of the license.
// If neither of these are available, assume that neither you nor anyone other than the copyright holder
// hold any right or permission to use this software until such time as the official license is identified.
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FreneticGameCore;
using FreneticGameCore.EntitySystem;

namespace FreneticGameGraphics.ClientSystem.EntitySystem
{
    /// <summary>
    /// Represents a property on a client-side entity.
    /// </summary>
    public class ClientEntityProperty : BasicEntityProperty<ClientEntity, GameEngineBase>
    {
        /// <summary>
        /// Gets the associated client game engine in 2D form.
        /// </summary>
        public GameEngine2D Engine2D
        {
            get
            {
                return Entity.Engine as GameEngine2D;
            }
        }

        /// <summary>
        /// Gets the associated client game engine in 3D form.
        /// </summary>
        public GameEngine3D Engine3D
        {
            get
            {
                return Entity.Engine as GameEngine3D;
            }
        }
    }
}
