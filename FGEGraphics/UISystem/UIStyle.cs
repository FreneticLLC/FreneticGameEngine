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
    public Color4F BaseColor = Color4F.Transparent;

    /// <summary>What texture to display (or <c>null</c> for none).</summary>
    public Texture BaseTexture;

    /// <summary>What border outline color to use (or <see cref="Color4F.Transparent"/> for none).</summary>
    public Color4F BorderColor = Color4F.Transparent;

    /// <summary>How thick the border outline should be (or <c>0</c> for none).</summary>
    public int BorderThickness = 0;

    /// <summary>How big the drop-shadow effect should be (or <c>0</c> for none).</summary>
    public int DropShadowLength = 0;

    /// <summary>The text font (or <c>null</c> for none).</summary>
    public FontSet TextFont;

    // TODO: Does the usage of 'Func' work properly in the context of a 'record' that's going into a Dictionary (ie hashcode/equality checks)?
    /// <summary>The styling effect for text.</summary>
    public Func<string, string> TextStyling = str => str;

    /// <summary>The base color effect for text (consider <see cref="TextStyle.Simple"/> if unsure).</summary>
    public string TextBaseColor = TextStyle.Simple;

    /// <summary>The name of the element style (for debug info).</summary>
    public string Name;

    /// <summary>Constructs a default element style.</summary>
    public UIStyle()
    {
    }

    /// <summary>Constructs a new style as a copy of another style.</summary>
    /// <param name="style">The style to copy.</param>
    public UIStyle(UIStyle style)
    {
        BaseColor = style.BaseColor;
        BaseTexture = style.BaseTexture;
        BorderColor = style.BorderColor;
        BorderThickness = style.BorderThickness;
        DropShadowLength = style.DropShadowLength;
        TextFont = style.TextFont;
        TextStyling = style.TextStyling;
        TextBaseColor = style.TextBaseColor;
    }

    /// <summary>Returns the font height, or <c>0</c> if <see cref="TextFont"/> is <c>null</c>.</summary>
    public int FontHeight => TextFont?.Height ?? 0;

    /// <summary>Returns whether this style can render text in general.</summary>
    public bool CanRenderText() => TextFont is not null;

    /// <summary>Returns whether this style can render the specified text.</summary>
    /// <param name="text">The UI text object to check.</param>
    public bool CanRenderText(UIText text) => !text.Empty && CanRenderText() && (text.Internal.Style == this || (text.Internal.Renderables?.ContainsKey(this) ?? false));

    public static implicit operator Func<UIElement, UIStyle>(UIStyle style) => element => style;
}
