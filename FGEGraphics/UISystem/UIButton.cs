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
public class UIButton : UIClickableElement
{
    /// <summary>The pressable area of this button.</summary>
    public UIBox Box;

    /// <summary>The text to render on this button.</summary>
    public UIElementText Text;

    /// <summary>Constructs a new style-based button.</summary>
    /// <param name="text">The text to display.</param>
    /// <param name="clicked">The action to run when clicked.</param>
    /// <param name="styles">The clickable styles.</param>
    /// <param name="pos">The position of the element.</param>
    public UIButton(string text, Action clicked, StyleGroup styles, UILayout pos) : base(styles, pos, false, clicked)
    {
        AddChild(Box = new UIBox(UIElementStyle.Empty, pos.AtOrigin()) { ShouldRender = false, Enabled = false });
        Text = new(this, text);
    }

    /// <summary>Constructs a new button based on a standard texture set.</summary>
    /// <param name="style">The base button style.</param>
    /// <param name="textures">The texture engine to get textures from.</param>
    /// <param name="textureSet">The name of the texture set to use.</param>
    /// <param name="text">The text to display.</param>
    /// <param name="clicked">The action to run when clicked.</param>
    /// <param name="pos">The position of the element.</param>
    public static UIButton Textured(string text, TextureEngine textures, string textureSet, Action clicked, UIElementStyle style, UILayout pos)
    {
        // TODO: Move this to a method on UIClickableElement.StyleGroup
        UIElementStyle normal = new(style) { BaseTexture = textures.GetTexture($"{textureSet}_none") };
        UIElementStyle hover = new(style) { BaseTexture = textures.GetTexture($"{textureSet}_hover") };
        UIElementStyle click = new(style) { BaseTexture = textures.GetTexture($"{textureSet}_click") };
        UIElementStyle disabled = new(style) { BaseTexture = textures.GetTexture($"{textureSet}_disabled") };
        return new UIButton(text, clicked, new(normal, hover, click, disabled), pos);
    }

    /// <summary>Renders this button on the screen.</summary>
    /// <param name="view">The UI view.</param>
    /// <param name="delta">The time since the last render.</param>
    /// <param name="style">The current element style.</param>
    public override void Render(ViewUI2D view, double delta, UIElementStyle style)
    {
        Box.Render(view, delta, style);
        if (style.CanRenderText(Text))
        {
            style.TextFont.DrawFancyText(Text, new Location(X + Width / 2 - Text.Width / 2, Y + Height / 2 - Text.Height / 2, 0));
        }
    }
}
