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
using OpenTK;
using OpenTK.Mathematics;

namespace FGEGraphics.ClientSystem;

/// <summary>Represents a 2D rendering context.</summary>
public class RenderContext2D
{
    /// <summary>The backing engine.</summary>
    public GameEngine2D Engine;

    /// <summary>Width of the view.</summary>
    public int Width = 1024;

    /// <summary>Height of the view.</summary>
    public int Height = 768;

    /// <summary>The zoom of the view.</summary>
    public float Zoom = 1f;

    /// <summary>Width over height.</summary>
    public float AspectHelper = 1024f / 768f;

    /// <summary>Whether the system is currently calculating shadows.</summary>
    public bool CalcShadows = false;

    /// <summary>The multiplier for zoom effects.</summary>
    public float ZoomMultiplier = 1f;

    /// <summary>The center of the 2D view.</summary>
    public Vector2 ViewCenter = Vector2.Zero;

    /// <summary>The present Adder value.</summary>
    public Vector2 Adder;

    /// <summary>The present Scaler value.</summary>
    public Vector2 Scaler;
}
