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

/// <summary>Represents an interactable button on a screen.</summary>
public class UIButton : UIElement
{
    /// <summary>The pressable area of this button.</summary>
    public UIBox Box;

    /// <summary>The text to render with this button.</summary>
    public UIText Text;

    /// <summary>Constructs a new style-based button.</summary>
    /// <param name="text">The text to display.</param>
    /// <param name="clicked">The action to run when clicked.</param>
    /// <param name="layout">The layout of the element.</param>
    public UIButton(string text, Action clicked, UIStyling styling, UILayout layout) : base(styling,layout)
    {
        OnClick += clicked;
        AddChild(Box = new UIBox(UIStyle.Empty, layout.AtOrigin()) { RenderSelf = false, IsEnabled = false });
        Text = new(this, text);
    }

    /// <inheritdoc/>
    public override void Render(double delta, UIStyle style)
    {
        Box.Render(delta, style);
        if (style.CanRenderText(Text))
        {
            style.TextFont.DrawFancyText(Text, new Location(X + Width / 2 - Text.Width / 2, Y + Height / 2 - Text.Height / 2, 0));
        }
    }
}
