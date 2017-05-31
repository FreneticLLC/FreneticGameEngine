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
    /// Represents the fact that the entity casts 2D light.
    /// </summary>
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

        /// <summary>
        /// The current position of the light.
        /// </summary>
        [PropertyDebuggable]
        [PropertyAutoSavable]
        public Vector2 LightPosition;


        /// <summary>
        /// The current strength of the light.
        /// </summary>
        [PropertyDebuggable]
        [PropertyAutoSavable]
        public float LightStrength = 256;

        /// <summary>
        /// The current color of the light.
        /// </summary>
        [PropertyDebuggable]
        [PropertyAutoSavable]
        public Color4 LightColor = Color4.White;

        /// <summary>
        /// The current subdivider-scale of the light.
        /// </summary>
        [PropertyDebuggable]
        [PropertyAutoSavable]
        public float LightSDScale = 0.5f;

        /// <summary>
        /// The actual light object.
        /// </summary>
        public PointLight2D ActualLight;

        /// <summary>
        /// Whether an entity will cast shadows from this light.
        /// </summary>
        /// <param name="ent">The entity in question.</param>
        /// <returns>Whether to cast shadows.</returns>
        public bool CastShadow(ClientEntity ent)
        {
            return ent.Renderer != null && ent.Renderer.CastShadows && ent.EID != BEntity.EID;
        }

        /// <summary>
        /// Fired when the entity is spawned.
        /// </summary>
        public override void OnSpawn()
        {
            if (Entity.Engine is GameEngine2D eng)
            {
                ActualLight = new PointLight2D(LightPosition, LightStrength, LightSDScale) { Color = LightColor, ShouldShadow = CastShadow };
                eng.Lights.Add(ActualLight);
                Entity.OnPositionChanged += FixPosition;
            }
            else
            {
                SysConsole.Output(OutputType.WARNING, "2D light spawned into a non-2D-engine-based game!");
            }
        }

        /// <summary>
        /// Fired when the entity is despawned.
        /// </summary>
        public override void OnDeSpawn()
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
                SysConsole.Output(OutputType.WARNING, "2D light despawned from a non-2D-engine-based game!");
            }
        }
    }
}
