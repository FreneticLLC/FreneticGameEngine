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

    /// <summary>What base color to use (or <see cref="Color4F.Transparent"/> for none).</summary>
    public Color4F BaseColor;

    /// <summary>What texture to display (or <c>null</c> for none).</summary>
    public Texture BaseTexture;

    /// <summary>What border outline color to use (or <see cref="Color4F.Transparent"/> for none).</summary>
    public Color4F BorderColor;

    /// <summary>How thick the border outline should be (or <c>0</c> for none).</summary>
    public int BorderThickness;

    public int Padding;

    /// <summary>How big the drop-shadow effect should be (or <c>0</c> for none).</summary>
    public int DropShadowLength;

    /// <summary>The text font (or <c>null</c> for none).</summary>
    public FontSet TextFont;

    // TODO: Does the usage of 'Func' work properly in the context of a 'record' that's going into a Dictionary (ie hashcode/equality checks)?
    /// <summary>The styling effect for text.</summary>
    public Func<string, string> TextStyling;

    /// <summary>The base color effect for text (consider <see cref="TextStyle.Simple"/> if unsure).</summary>
    public string TextBaseColor;

    /// <summary>The name of the element style (for debug info).</summary>
    public string Name;

    public int Inset => BorderThickness + Padding;

    /// <summary>Returns the font height, or <c>0</c> if <see cref="TextFont"/> is <c>null</c>.</summary>
    public int FontHeight => TextFont?.Height ?? 0;

    /// <summary>Returns whether this style can render text in general.</summary>
    public bool CanRenderText => TextFont is not null;
}
