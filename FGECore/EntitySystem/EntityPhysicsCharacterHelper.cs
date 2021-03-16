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
using BEPUphysics.Character;
using FGECore.MathHelpers;
using FGECore.PropertySystem;
using FGECore.PhysicsSystem;

namespace FGECore.EntitySystem
{
    /// <summary>
    /// Helper for character shapes on a physics entity.
    /// </summary>
    public class EntityPhysicsCharacterHelper
    {
        /// <summary>
        /// The internal character controller for this entity.
        /// </summary>
        public CharacterController Internal;
        
        /// <summary>
        /// Gets or sets the view direction of the character.
        /// </summary>
        [PropertyDebuggable]
        [PropertyAutoSavable]
        public Location ViewDirection
        {
            get
            {
                return Internal.ViewDirection.ToLocation();
            }
            set
            {
                Internal.ViewDirection = value.ToNumerics();
            }
        }
    }
}
