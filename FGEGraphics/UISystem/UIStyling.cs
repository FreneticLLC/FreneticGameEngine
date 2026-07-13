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

/// <summary>Represents the styling logic of a <see cref="UIElement"/>.</summary>
public record UIStyling
{
    public static readonly UIStyling Empty = new() { Constant = UIStyle.Empty };

    /// <summary>
    /// The element bound to this style.
    /// <para>If present, calls to <see cref="UIStyleValue{T}.Get(UIElement)"/> will use this element rather than the one passed as an argument.</para>
    /// </summary>
    public UIElement Element = null;

    public UIStyle Constant = null;

    /// <summary>The color to fill an element's interior with.</summary>
    public UIStyleValue<Color4F> Fill = Color4F.Transparent;

    /// <summary>The texture to draw on an element.</summary>
    public UIStyleValue<Texture> Texture = default;

    /// <summary>The color to draw an element's outline with.</summary>
    public UIStyleValue<Color4F> Stroke = Color4F.Transparent;

    /// <summary>The thickness to draw an element's outline with.</summary>
    public UIStyleValue<int> StrokeWeight = 0;

    // TODO: this may be structural enough to be on UIElement instead of style
    public UIStyleValue<bool> ShowBackground = false;

    /// <summary>The distance between an element's outline and its interior content.</summary>
    public UIStyleValue<int> Padding = 0;

    /// <summary>The size of the drop shadow on an element.</summary>
    public UIStyleValue<int> ShadowSize = 0;

    /// <summary>The text font.</summary>
    public UIStyleValue<FontSet> TextFont = default;

    /// <summary>The text styling effect.</summary>
    public UIStyleValue<Func<string, string>> TextStyling = default;

    /// <summary>The base color effect for text.</summary>
    public UIStyleValue<string> TextBaseColor = TextStyle.Simple;

    public List<StylingComponent> Components;

    /// <summary>Returns a new <see cref="UIStyle"/> using style values based on the given <paramref name="element"/>.</summary>
    public UIStyle Get(UIElement element) 
    {
        if (Constant is not null)
        {
            return null;
        }
        element = Element ?? element;
        return new()
        {
            Fill = Fill.Get(element),
            Texture = Texture.Get(element),
            Stroke = Stroke.Get(element),
            StrokeWeight = StrokeWeight.Get(element),
            ShowBackground = ShowBackground.Get(element),
            Padding = Padding.Get(element),
            ShadowSize = ShadowSize.Get(element),
            TextFont = TextFont.Get(element),
            TextStyling = TextStyling.Get(element),
            TextBaseColor = TextBaseColor.Get(element)
        };
    }

    public UIStyling Copy() => this with { };

    /// <summary>Returns this styling instance with <see cref="Element"/> set to <paramref name="element"/>.</summary>
    public UIStyling Bind(UIElement element) => this with { Element = element };
}
