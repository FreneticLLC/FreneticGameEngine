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

namespace FGEGraphics.GraphicsHelpers.FontSets;

/// <summary>Represents a single line of renderable text.</summary>
public record RenderableTextLine(RenderableTextPart[] Parts, int Width, int Height)
{
    /// <summary>An empty <see cref="RenderableTextLine"/> instance</summary>
    public static readonly RenderableTextLine Empty = new();

    /// <summary>Constructs an empty <see cref="RenderableTextLine"/> instance.</summary>
    public RenderableTextLine() : this([], 0, 0)
    { }

    /// <summary>
    /// Constructs a <see cref="RenderableTextLine"/> from an array of parts 
    /// that also describe the <see cref="Width"/> and <see cref="Height"/>.
    /// </summary>
    /// <param name="parts">The text parts.</param>
    public RenderableTextLine(RenderableTextPart[] parts)
        : this(parts,
              (int)parts.Sum(part => part?.Width),
              parts.Length > 0 ? parts.Max(part => part?.Font.Height ?? 0) : 0)
    { }

    /// <inheritdoc/>
    public override string ToString() => string.Concat<RenderableTextPart>(Parts);
}
