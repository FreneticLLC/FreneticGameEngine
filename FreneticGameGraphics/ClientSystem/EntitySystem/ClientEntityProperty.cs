using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FreneticGameCore;

namespace FreneticGameGraphics.ClientSystem.EntitySystem
{
    /// <summary>
    /// Represents a property on a client-side entity.
    /// </summary>
    public class ClientEntityProperty : Property
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
