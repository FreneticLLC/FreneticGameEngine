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
using FGECore;
using FGECore.CoreSystems;
using FGECore.EntitySystem;
using FGECore.MathHelpers;
using FGECore.PropertySystem;
using FGEGraphics.LightingSystem;
using OpenTK;
using OpenTK.Mathematics;

namespace FGEGraphics.ClientSystem.EntitySystem
{
    /// <summary>Represents the fact that the entity casts 2D light.</summary>
    public class EntityLight2DCasterProperty : ClientEntityProperty
    {
        /// <summary>
        /// Fixes the position of the light to match a new location.
        /// Automatically called by <see cref="BasicEntity.OnPositionChanged"/>.
        /// </summary>
        /// <param name="pos">The new position.</param>
        public void FixPosition(Location pos)
        {
            LightPosition = new Vector2(pos.XF, pos.YF);
            if (ActualLight != null)
            {
                ActualLight.Position = LightPosition;
            }
        }

        /// <summary>The current position of the light.</summary>
        [PropertyDebuggable]
        [PropertyAutoSavable]
        public Vector2 LightPosition;


        /// <summary>The current strength of the light.</summary>
        [PropertyDebuggable]
        [PropertyAutoSavable]
        public float LightStrength = 256;

        /// <summary>The current color of the light.</summary>
        [PropertyDebuggable]
        [PropertyAutoSavable]
        public Color4F LightColor = Color4F.White;

        /// <summary>The current subdivider-scale of the light.</summary>
        [PropertyDebuggable]
        [PropertyAutoSavable]
        public float LightSDScale = 0.5f;

        /// <summary>How deep into an object the light should go.</summary>
        [PropertyDebuggable]
        [PropertyAutoSavable]
        public float LightExtraDist = 50.0f;

        /// <summary>The lowest layer that can present a shadow.</summary>
        [PropertyDebuggable]
        [PropertyAutoSavable]
        public double ShadowMinLayer = -10E10;

        /// <summary>The highest layer that can present a shadow.</summary>
        [PropertyDebuggable]
        [PropertyAutoSavable]
        public double ShadowMaxLayer = 10E10;

        /// <summary>Whether the light source is a 'sky' light.</summary>
        [PropertyDebuggable]
        [PropertyAutoSavable]
        public bool IsSkyLight = false;

        /// <summary>The actual light object.</summary>
        public PointLight2D ActualLight;

        /// <summary>Whether an entity will cast shadows from this light.</summary>
        /// <param name="ent">The entity in question.</param>
        /// <returns>Whether to cast shadows.</returns>
        public bool CastShadow(ClientEntity ent)
        {
            return ent.EID != Entity.EID && ent.Renderer != null && ent.Renderer.CastShadows && ent.Renderer.RenderingPriorityOrder >= ShadowMinLayer && ent.Renderer.RenderingPriorityOrder <= ShadowMaxLayer;
        }

        /// <summary>Fired when the entity is spawned.</summary>
        public override void OnSpawn()
        {
            if (Entity.Engine is GameEngine2D eng)
            {
                ActualLight = new PointLight2D(LightPosition, LightStrength, LightSDScale, eng)
                {
                    Color = LightColor,
                    ShouldShadow = CastShadow,
                    ExtraLightDist = LightExtraDist,
                    IsSkyLight = IsSkyLight
                };
                eng.Lights.Add(ActualLight);
                Entity.OnPositionChanged += FixPosition;
            }
            else
            {
                OutputType.WARNING.Output("2D light spawned into a non-2D-engine-based game!");
            }
        }

        /// <summary>Fired when the entity is despawned.</summary>
        public override void OnDespawn()
        {
            if (Entity.Engine is GameEngine2D eng)
            {
                eng.Lights.Remove(ActualLight);
                ActualLight.Destroy();
                ActualLight = null;
                Entity.OnPositionChanged -= FixPosition;
            }
            else
            {
                OutputType.WARNING.Output("2D light despawned from a non-2D-engine-based game!");
            }
        }
    }
}
