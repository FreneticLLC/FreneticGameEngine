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
    }
}
