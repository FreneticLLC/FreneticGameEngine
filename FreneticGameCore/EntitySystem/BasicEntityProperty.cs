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

namespace FreneticGameCore.EntitySystem
{
    /// <summary>
    /// Represents a property on a basic entity.
    /// </summary>
    public class BasicEntityProperty : Property
    {
        /// <summary>
        /// Run when the entity is spawned.
        /// </summary>
        public virtual void OnSpawn()
        {
        }

        /// <summary>
        /// Run when the entity is de-spawned.
        /// </summary>
        public virtual void OnDeSpawn()
        {
        }
        
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

        /// <summary>
        /// Gets the basic engine associated with a property.
        /// </summary>
        public BasicEngine BEngine
        {
            get
            {
                return BEntity.Engine;
            }
        }
    }
}
