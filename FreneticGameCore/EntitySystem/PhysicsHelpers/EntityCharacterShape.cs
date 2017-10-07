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
using BEPUphysics.Character;
using BEPUphysics.CollisionShapes;

namespace FreneticGameCore.EntitySystem.PhysicsHelpers
{
    /// <summary>
    /// Represents a physical 'character'.
    /// </summary>
    public class EntityCharacterShape : EntityShapeHelper
    {
        /// <summary>
        /// Upward axis.
        /// </summary>
        [PropertyDebuggable]
        [PropertyAutoSavable]
        public Location Up = Location.UnitZ;

        /// <summary>
        /// View direction of the character.
        /// </summary>
        [PropertyDebuggable]
        [PropertyAutoSavable]
        public Location ViewDirection = -Location.UnitY;

        /// <summary>
        /// Half-width of the character.
        /// </summary>
        [PropertyDebuggable]
        [PropertyAutoSavable]
        public double Radius = 0.6;

        /// <summary>
        /// Height of the character.
        /// </summary>
        [PropertyDebuggable]
        [PropertyAutoSavable]
        public double Height = 1.7;

        /// <summary>
        /// Standing speed of the character.
        /// </summary>
        [PropertyDebuggable]
        [PropertyAutoSavable]
        public double Speed = 8;

        /// <summary>
        /// Jumping speed of the character.
        /// </summary>
        [PropertyDebuggable]
        [PropertyAutoSavable]
        public double JumpSpeed = 4.5;

        /// <summary>
        /// Returns the BEPU version of the character.
        /// </summary>
        /// <returns>The BEPU character.</returns>
        public CharacterController GetBEPUCharacter()
        {
            return new CharacterController(height: Height, crouchingHeight: Height * 0.7, proneHeight: Radius, radius: Radius,
                standingSpeed: Speed, crouchingSpeed: Speed * 0.4, proneSpeed: Speed * 0.1, airSpeed: Speed * 0.05, jumpSpeed: JumpSpeed, slidingJumpSpeed: JumpSpeed * 0.5)
            {
                Down = -Up.ToBVector(),
                ViewDirection = ViewDirection.ToBVector()
            };
        }

        /// <summary>
        /// Stringifies the character shape.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return "CharacterShape";
        }

        /// <summary>
        /// Returns null as a special case to the internal shape options.
        /// </summary>
        /// <returns>Null.</returns>
        public override EntityShape GetBEPUShape()
        {
            return null;
        }
    }
}
