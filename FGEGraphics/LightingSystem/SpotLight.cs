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
using FGECore.MathHelpers;
using OpenTK;
using OpenTK.Mathematics;

namespace FGEGraphics.LightingSystem;

/// <summary>Represents a directed spot light in 3D.</summary>
public class SpotLight : LightObject
{
    /// <summary>The range of the light.</summary>
    public float Radius;

    /// <summary>Color of the light.</summary>
    public Location Color;

    /// <summary>Direction of the light.</summary>
    public Location Direction;

    /// <summary>Width of the light (FOV).</summary>
    public float Width;

    /// <summary>Constructs the spot light.</summary>
    /// <param name="pos">Position.</param>
    /// <param name="radius">Radius.</param>
    /// <param name="col">Color.</param>
    /// <param name="dir">Direction.</param>
    /// <param name="size">FOV.</param>
    public SpotLight(Location pos, float radius, Location col, Location dir, float size)
    {
        EyePos = pos;
        Radius = radius;
        Color = col;
        Width = size;
        InternalLights.Add(new Light());
        if (dir.Z >= 1 || dir.Z <= -1)
        {
            InternalLights[0].UpVector = new Vector3(0, 1, 0);
        }
        else
        {
            InternalLights[0].UpVector = new Vector3(0, 0, 1);
        }
        Direction = dir;
        InternalLights[0].Create(pos.ToOpenTK3D(), (pos + dir).ToOpenTK3D(), Width, Radius, Color.ToOpenTK());
        MaxDistance = radius;
    }

    /// <summary>Destroys the spot light.</summary>
    public void Destroy()
    {
        InternalLights[0].Destroy();
    }

    /// <summary>Reposition the light.</summary>
    /// <param name="pos">New position.</param>
    public override void Reposition(Location pos)
    {
        EyePos = pos;
        InternalLights[0].NeedsUpdate = true;
        InternalLights[0].EyePosition = EyePos.ToOpenTK3D();
        InternalLights[0].TargetPosition = (EyePos + Direction).ToOpenTK3D();
    }
}
