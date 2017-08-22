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
    public class ClientEntityProperty : BasicEntityProperty
    {
        /// <summary>
        /// Gets the entity associated with this property.
        /// </summary>
        public ClientEntity Entity
        {
            get
            {
                return Holder as ClientEntity;
            }
        }

        /// <summary>
        /// Gets the associated client game engine.
        /// </summary>
        public GameEngineBase Engine
        {
            get
            {
                return Entity.ClientEngine;
            }
        }

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
