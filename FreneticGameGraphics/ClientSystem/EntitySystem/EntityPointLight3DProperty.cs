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
using FreneticGameCore;
using FreneticGameCore.EntitySystem;
using FreneticGameGraphics.LightingSystem;
using OpenTK;
using OpenTK.Graphics;

namespace FreneticGameGraphics.ClientSystem.EntitySystem
{
    /// <summary>
    /// Represents a 3D point light.
    /// </summary>
    public class EntityPointLight3DProperty : ClientEntityProperty
    {
        /// <summary>
        /// Fixes the position of the light to match a new location.
        /// Automatically called by <see cref="BasicEntity{T}.OnPositionChanged"/>.
        /// </summary>
        /// <param name="pos">The new position.</param>
        public void FixPosition(Location pos)
        {
            LightPosition = pos;
            if (InternalLight != null)
            {
                InternalLight.Reposition(pos);
            }
        }

        /// <summary>
        /// The current position of the light.
        /// </summary>
        [PropertyDebuggable]
        [PropertyAutoSavable]
        public Location LightPosition;
        
        /// <summary>
        /// The current strength of the light.
        /// </summary>
        [PropertyDebuggable]
        [PropertyAutoSavable]
        public float LightStrength = 16;

        /// <summary>
        /// The current color of the light.
        /// </summary>
        [PropertyDebuggable]
        [PropertyAutoSavable]
        public Color3F LightColor = Color3F.White;

        /// <summary>
        /// The represented 3D point light.
        /// </summary>
        public PointLight InternalLight;

        /// <summary>
        /// Fired when the entity is spawned.
        /// </summary>
        public override void OnSpawn()
        {
            if (Entity.Engine is GameEngine3D eng)
            {
                InternalLight = new PointLight(LightPosition, LightStrength, LightColor);
                eng.MainView.Lights.Add(InternalLight);
                Entity.OnPositionChanged += FixPosition;
            }
            else
            {
                SysConsole.Output(OutputType.WARNING, "3D light spawned into a non-3D-engine-based game!");
            }
        }

        /// <summary>
        /// Fired when the entity is despawned.
        /// </summary>
        public override void OnDespawn()
        {
            if (Entity.Engine is GameEngine3D eng)
            {
                eng.MainView.Lights.Remove(InternalLight);
                InternalLight.Destroy();
                InternalLight = null;
                Entity.OnPositionChanged -= FixPosition;
            }
            else
            {
                SysConsole.Output(OutputType.WARNING, "3D light despawned from a non-3D-engine-based game!");
            }
        }
    }
}
