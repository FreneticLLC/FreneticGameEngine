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
using FGEGraphics.GraphicsHelpers;
using FGEGraphics.GraphicsHelpers.Models;

namespace FGEGraphics.ClientSystem;

/// <summary>Represents the context in which something is being rendered (in 3D).</summary>
public class RenderContext
{
    /// <summary>The relevant owning game engine (3D).</summary>
    public GameEngine3D Engine;

    /// <summary>How many <see cref="Model"/>s have been rendered this frame in this context.</summary>
    public int ModelsRendered = 0;

    /// <summary>How many <see cref="Renderable"/>s have been rendered this frame in this context.</summary>
    public int ObjectsRendered = 0;

    /// <summary>How many particles have been rendered this frame in this context.</summary>
    public int ParticlesRendered = 0;

    /// <summary>How many decals have been rendered this frame in this context.</summary>
    public int DecalsRendered = 0;

    /// <summary>How many sprites (like grass) have been rendered this frame in this context.</summary>
    public int SpritesRendered = 0;

    /// <summary>How many vertices have been rendered this frame in this context.</summary>
    public int VerticesRendered = 0;

    /// <summary>Resets the counters.</summary>
    public void ResetCounters()
    {
        ModelsRendered = 0;
        ObjectsRendered = 0;
        ParticlesRendered = 0;
        DecalsRendered = 0;
        SpritesRendered = 0;
        VerticesRendered = 0;
    }
}
