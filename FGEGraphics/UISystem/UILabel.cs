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
using FGECore;
using FGECore.ConsoleHelpers;
using FGECore.CoreSystems;
using FGECore.MathHelpers;
using FGEGraphics.ClientSystem;
using FGEGraphics.GraphicsHelpers;
using FGEGraphics.GraphicsHelpers.FontSets;
using FGEGraphics.GraphicsHelpers.Textures;
using OpenTK;
using OpenTK.Mathematics;

namespace FGEGraphics.UISystem;

/// <summary>Represents a simple piece of text on a screen.</summary>
public class UILabel : UIElement
{
    /// <summary>The text to display on this label.</summary>
    public UIText Text;

    /// <summary>Constructs a new label.</summary>
    /// <param name="text">The text to display on the label.</param>
    /// <param name="styling">The style of the label.</param>
    /// <param name="layout">The layout of the element.</param>
    public UILabel(string text, UIStyling styling, UILayout layout) : base(styling, layout)
    {
        Text = new UIText(this, text, true, Layout.Width);
        Layout.SetSize(() => Text.Width, () => Text.Height); // TODO: padding
    }

    /// <inheritdoc/>
    public override void Render(double delta, UIStyle style)
    {
        style.TextFont.DrawFancyText(Text, new Location(X, Y, 0));
    }

    public static (UILabel Label, UIImage Icon, UIListGroup List) WithIcon(string text, Texture icon, int spacing, UIStyling styling, UILayout layout, UIAnchor listAnchor = null)
    {
        UIListGroup list = new(spacing, layout, vertical: false, anchor: listAnchor ?? UIAnchor.TOP_LEFT);
        UILabel label = new(text, styling, layout.AtOrigin());
        UIImage image = new(icon, new UILayout().SetSize(() => label.Height, () => label.Height));
        list.AddChild(label);
        list.AddChild(image);
        return (label, image, list);
    }
}
