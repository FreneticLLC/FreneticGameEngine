using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FreneticGameCore;
using FreneticGameCore.EntitySystem;
using FreneticGameGraphics.LightingSystem;
using OpenTK;

namespace FreneticGameGraphics.ClientSystem.EntitySystem
{
    /// <summary>
    /// Represents the fact that the entity casts 2D light.
    /// </summary>
    public class EntityLight2DCasterProperty : ClientEntityProperty
    {
        /// <summary>
        /// Fired when the property is added to an entity.
        /// </summary>
        public override void OnAdded()
        {
            base.OnAdded();
            Entity.OnSpawn += OnSpawned;
            Entity.OnDeSpawn += OnDeSpawned;
            Entity.OnPositionChanged += FixPosition;
        }

        /// <summary>
        /// Fired when the property is removed from an entity.
        /// </summary>
        public override void OnRemoved()
        {
            base.OnRemoved();
            Entity.OnSpawn -= OnSpawned;
            Entity.OnDeSpawn -= OnDeSpawned;
            Entity.OnPositionChanged -= FixPosition;
        }

        /// <summary>
        /// Fixes the position of the light to match a new location.
        /// Automatically called by <see cref="BasicEntity.OnPositionChanged"/>.
        /// </summary>
        /// <param name="pos">The new position.</param>
        public void FixPosition(Location pos)
        {
            LightPosition = new Vector2((float)pos.X, (float)pos.Y);
        }

        /// <summary>
        /// The current position of the light.
        /// </summary>
        public Vector2 LightPosition;


        /// <summary>
        /// The current strength of the light.
        /// </summary>
        public float LightStrength = 256;

        /// <summary>
        /// The actual light object.
        /// </summary>
        public PointLight2D ActualLight;

        /// <summary>
        /// Fired when the entity is spawned.
        /// </summary>
        public void OnSpawned(EntitySpawnEventArgs e)
        {
            if (Entity.Engine is GameEngine2D eng)
            {
                ActualLight = new PointLight2D(LightPosition, LightStrength);
                eng.Lights.Add(ActualLight);
            }
            else
            {
                SysConsole.Output(OutputType.WARNING, "2D light spawned into a non-2D-engine-based game!");
            }
        }

        /// <summary>
        /// Fired when the entity is despawned.
        /// </summary>
        public void OnDeSpawned(EntityDeSpawnEventArgs e)
        {
            if (Entity.Engine is GameEngine2D eng)
            {
                eng.Lights.Remove(ActualLight);
                ActualLight.Destroy();
                ActualLight = null;
            }
            else
            {
                SysConsole.Output(OutputType.WARNING, "2D light despawned from a non-2D-engine-based game!");
            }
        }
    }
}
