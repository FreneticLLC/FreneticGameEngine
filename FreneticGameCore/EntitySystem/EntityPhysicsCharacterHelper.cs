using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BEPUphysics.Character;
using FreneticGameCore.MathHelpers;
using FreneticGameCore.CoreSystems;

namespace FreneticGameCore.EntitySystem
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
                return new Location(Internal.ViewDirection);
            }
            set
            {
                Internal.ViewDirection = value.ToBVector();
            }
        }
    }
}
