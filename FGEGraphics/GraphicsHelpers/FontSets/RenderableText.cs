//
// This file is part of the Frenetic Game Engine, created by Frenetic LLC.
// This code is Copyright (C) Frenetic LLC under the terms of a strict license.
// See README.md or LICENSE.txt in the FreneticGameEngine source root for the contents of the license.
// If neither of these are available, assume that neither you nor anyone other than the copyright holder
// hold any right or permission to use this software until such time as the official license is identified.
//

using FGECore.CoreSystems;
using FGECore.MathHelpers;
using FGEGraphics.UISystem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FGEGraphics.GraphicsHelpers.FontSets;

/// <summary>Represents a section of renderable text.</summary>
public record RenderableText(RenderableTextLine[] Lines, int Width, int Height)
{
    /// <summary>An empty <see cref="RenderableText"/> instance.</summary>
    public static readonly RenderableText Empty = new();

    // TODO: add to other renderable classes
    public bool IsEmpty => Lines is null || Lines.Length == 0 || Width == 0 || Height == 0;
    public RenderableText() : this([], 0, 0)
    { }

    /// <param name="lines">The text lines.</param>
    public RenderableText(RenderableTextLine[] lines) :
        this(lines,
            lines.Length > 0 ? lines.Max(line => line?.Width ?? 0) : 0,
            lines.Sum(line => line?.Height ?? 0))
    { }

    public Vector2i GetTrueSize(FontSet fontSet)
    {
        if (IsEmpty)
        {
            return Vector2i.Zero;
        }
        int trueHeight = fontSet.Height * Lines.Length;
        int trueWidth = (int)((float)trueHeight / Height * Width);
        return new Vector2i(trueWidth, trueHeight);
    }

    /// <summary>Implements <see cref="Object.ToString"/> to make a "\n" separated string of the contents.</summary>
    public override string ToString() => string.Join<RenderableTextLine>('\n', Lines);
}