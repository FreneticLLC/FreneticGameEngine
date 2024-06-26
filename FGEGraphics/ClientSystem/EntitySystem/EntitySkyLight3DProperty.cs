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
using FGECore.CoreSystems;
using FGECore.EntitySystem;
using FGECore.MathHelpers;
using FGECore.PropertySystem;
using FGEGraphics.LightingSystem;

namespace FGEGraphics.ClientSystem.EntitySystem;

/// <summary>Represents a 3D sky light.</summary>
public class EntitySkyLight3DProperty : ClientEntityProperty
{
    /// <summary>
    /// Fixes the position of the light to match a new location.
    /// Automatically called by <see cref="BasicEntity.OnPositionChanged"/>.
    /// </summary>
    /// <param name="pos">The new position.</param>
    public void FixPosition(Location pos)
    {
        LightPosition = pos;
        InternalLight?.Reposition(pos);
    }

    /// <summary>
    /// Whether to automatically place the sky light where the camera's at every tick.
    /// Disable if you have a better method of keeping it positioned.
    /// </summary>
    [PropertyDebuggable]
    [PropertyAutoSavable]
    public bool AutoCorrectPlacement = true;

    /// <summary>The current position of the light.</summary>
    [PropertyDebuggable]
    [PropertyAutoSavable]
    public Location LightPosition = Location.Zero;

    /// <summary>The direction of the light.</summary>
    [PropertyDebuggable]
    [PropertyAutoSavable]
    public Location Direction = -Location.UnitZ;

    /// <summary>The vertical size of the sky light. How far it reaches.</summary>
    [PropertyDebuggable]
    [PropertyAutoSavable]
    public float Radius = 512f;

    /// <summary>The horizontal radius of the sky light. How wide of an area it reaches.</summary>
    [PropertyDebuggable]
    [PropertyAutoSavable]
    public float Size = 128f;

    /// <summary>The current color of the light.</summary>
    [PropertyDebuggable]
    [PropertyAutoSavable]
    public Color3F LightColor = Color3F.White * 0.75f;

    /// <summary>The represented 3D sky light.</summary>
    public SkyLight InternalLight;

    /// <summary>Ticks the sky light, correcting its position.</summary>
    public void Tick()
    {
        if (AutoCorrectPlacement)
        {
            Entity.SetPosition(Engine3D.MainCamera.Position - Direction * (Radius * 0.5));
        }
    }

    /// <summary>Fired when the entity is spawned.</summary>
    public override void OnSpawn()
    {
        if (Entity.Engine is GameEngine3D eng)
        {
            InternalLight = new SkyLight(LightPosition, Radius, LightColor, Direction, Size, false, Engine3D.MainView.Config.ShadowTexSize());
            eng.MainView.Config.Lights.Add(InternalLight);
            Entity.OnPositionChanged += FixPosition;
            Entity.OnTick += Tick;
        }
        else
        {
            Logs.Warning("3D light spawned into a non-3D-engine-based game!");
        }
    }

    /// <summary>Fired when the entity is despawned.</summary>
    public override void OnDespawn()
    {
        if (Entity.Engine is GameEngine3D eng)
        {
            eng.MainView.Config.Lights.Remove(InternalLight);
            InternalLight.Destroy();
            InternalLight = null;
            Entity.OnPositionChanged -= FixPosition;
            Entity.OnTick -= Tick;
        }
        else
        {
            Logs.Warning("3D light despawned from a non-3D-engine-based game!");
        }
    }
}
