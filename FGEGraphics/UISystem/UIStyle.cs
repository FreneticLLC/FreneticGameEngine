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
using FGECore.ConsoleHelpers;
using FGECore.MathHelpers;
using FGEGraphics.GraphicsHelpers.FontSets;
using FGEGraphics.GraphicsHelpers.Textures;

namespace FGEGraphics.UISystem;

/// <summary>Represents the rendering style of a <see cref="UIElement"/>.</summary>
public record UIStyle
{
    /// <summary>An empty element style.</summary>
    public static readonly UIStyle Empty = new() { Name = "Empty" };

    /// <summary>The color to fill an element's interior with.</summary>
    public Color4F Fill;

    /// <summary>The texture to draw on an element.</summary>
    public Texture Texture;

    /// <summary>The color to draw an element's outline with.</summary>
    public Color4F Stroke;

    /// <summary>The thickness to draw an element's outline with.</summary>
    public int StrokeWeight;

    /// <summary>The distance between an element's outline and its interior content.</summary>
    public int Padding;

    /// <summary>The size of the drop shadow on an element.</summary>
    public int ShadowSize;

    /// <summary>The text font.</summary>
    public FontSet TextFont;

    /// <summary>The text styling effect.</summary>
    public Func<string, string> TextStyling;

    /// <summary>The base color effect for text.</summary>
    public string TextBaseColor;

    /// <summary>The name of the style (for debug info).</summary>
    public string Name;

    /// <summary>The distance between an element's boundary and its interior content.</summary>
    public int Inset => StrokeWeight + Padding;
}
