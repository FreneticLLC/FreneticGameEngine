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
public class UIElementStyle
{
    /// <summary>The text styling for a <see cref="UIElementStyle"/></summary>
    /// <param name="Font">The text font to use (or <c>null</c> for none).</param>
    /// <param name="Styling">The base color effect to use for text (consider <see cref="TextStyle.Simple"/> if unsure).</param>
    public record struct UITextStyle(FontSet Font, string Styling)
    {
        /// <summary>The default UI text style.</summary>
        public static readonly UITextStyle Default = new(null, TextStyle.Simple);
    }

    /// <summary>An empty element style.</summary>
    public static readonly UIElementStyle Empty = new();

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

    /// <summary>The text styling for this element style.</summary>
    public UITextStyle Text = new();

    /// <summary>Gets or sets the text font (or <c>null</c> for none).</summary>
    public FontSet TextFont
    {
        get => Text.Font;
        set => Text.Font = value;
    }

    /// <summary>Gets or sets the base color effect for text (consider <see cref="FGECore.ConsoleHelpers.TextStyle.Simple"/> if unsure).</summary>
    public string TextStyling
    {
        get => Text.Styling;
        set => Text.Styling = value;
    }

    /// <summary>Constructs a default element style.</summary>
    public UIElementStyle()
    {
    }

    /// <summary>Constructs a new style as a copy of another style.</summary>
    /// <param name="style">The style to copy.</param>
    public UIElementStyle(UIElementStyle style)
    {
        BaseColor = style.BaseColor;
        BaseTexture = style.BaseTexture;
        BorderColor = style.BorderColor;
        BorderThickness = style.BorderThickness;
        DropShadowLength = style.DropShadowLength;
        Text = style.Text;
    }

    /// <summary>Returns whether this style can render the specified text.</summary>
    /// <param name="text">The UI text object to check.</param>
    /// <returns>Whether this style can render the specified text.</returns>
    public bool CanRenderText(UIElementText text)
        => TextFont is not null && text.Internal.RenderableContent.ContainsKey(this);
}
