using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BEPUphysics.Character;

namespace FreneticGameCore.EntitySystem
{
    /// <summary>
    /// Helper for character shapes on a physics entity. Check <see cref="IsValid"/> before using anything else.
    /// </summary>
    public struct EntityPhysicsCharacterHelper
    {
        /// <summary>
        /// The internal character controller for this entity.
        /// </summary>
        public CharacterController Internal;

        /// <summary>
        /// Gets whether the character is valid.
        /// </summary>
        [PropertyDebuggable]
        [PropertyRequiredBool]
        public bool IsValid
        {
            get
            {
                return Internal != null;
            }
        }

        /// <summary>
        /// Gets or sets the view direction of the character.
        /// </summary>
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
