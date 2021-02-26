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
using FGECore.EntitySystem;
using FGECore;
using FGECore.CoreSystems;
using FGECore.MathHelpers;

namespace FGECore.ServerSystem.EntitySystem
{
    /// <summary>Represents an entity on the server side.</summary>
    public class ServerEntity : BasicEntity<ServerEntity, ServerEngine>
    {
        /// <summary>Whether this entity should be sent over the network.</summary>
        public bool ShouldNetwork;
        
        /// <summary>Gets the backing server engine.</summary>
        public ServerEngine Server
        {
            get
            {
                return Engine as ServerEngine;
            }
        }
        
        /// <summary>
        /// Constructs a server-side entity.
        /// </summary>
        /// <param name="engine">The owning engine.</param>
        /// <param name="_ticks">Whether it should tick.</param>
        /// <param name="_shouldNetwork">Whether it should network.</param>
        public ServerEntity(ServerEngine engine, bool _ticks, bool _shouldNetwork = false) 
            : base(engine, _ticks)
        {
            ShouldNetwork = _shouldNetwork;
        }

        /// <summary>Gets a string debug helper for this entity.</summary>
        public override string ToString()
        {
            return $"ServerEntity of type: {GetType().Name}, ID: {EID}, with properties: {PropertyList()}";
        }
    }
}
