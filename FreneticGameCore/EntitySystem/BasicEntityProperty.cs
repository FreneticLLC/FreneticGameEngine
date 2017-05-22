using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FreneticGameCore.EntitySystem
{
    /// <summary>
    /// Represents a property on a basic entity.
    /// </summary>
    public class BasicEntityProperty : Property
    {
        /// <summary>
        /// Gets the basic entity associated with a property.
        /// </summary>
        public BasicEntity BEntity
        {
            get
            {
                return Holder as BasicEntity;
            }
        }
    }
}
