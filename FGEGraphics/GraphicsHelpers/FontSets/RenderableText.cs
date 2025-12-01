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

/// <summary>Represents a section of renderable text.</summary>
public class RenderableText()
{
    /// <summary>An empty <see cref="RenderableText"/> instance.</summary>
    public static readonly RenderableText Empty = new() { Lines = [], Width = 0 };

    /// <summary>An array of all lines of text.</summary>
    public RenderableTextLine[] Lines;

    /// <summary>The maximum width of the text.</summary>
    public int Width;

    /// <summary>Constructs renderable text from a single line.</summary>
    /// <param name="lines">The text lines.</param>
    public RenderableText(params RenderableTextLine[] lines) : this()
    {
        Lines = lines;
        Width = lines.Length == 0 ? 0 : lines.Max(line => line.Width);
    }

    /// <summary>Implements <see cref="Object.ToString"/> to make a "\n" separated string of the contents.</summary>
    public override string ToString() => string.Join<RenderableTextLine>('\n', Lines);
}