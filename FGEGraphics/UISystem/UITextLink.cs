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
using FGECore.CoreSystems;
using FGECore.MathHelpers;
using FGEGraphics.ClientSystem;
using FGEGraphics.GraphicsHelpers;
using FGEGraphics.GraphicsHelpers.FontSets;
using FGEGraphics.GraphicsHelpers.Textures;
using OpenTK;
using OpenTK.Mathematics;

namespace FGEGraphics.UISystem;

/// <summary>Represents an interactable text link on a screen.</summary>
public class UITextLink : UIElement
{
    /// <summary>The text to display for this link.</summary>
    public UIText Text;

    /// <summary>The icon to display alongside this link.</summary>
    // TODO: UIImage
    public Texture Icon;

    /// <summary>Constructs an interactable text link.</summary>
    /// <param name="text">The text to display.</param>
    /// <param name="icon">The icon to display alongside the text.</param>
    /// <param name="clicked">The action to run when clicked.</param>
    /// <param name="styles">The clickable styles.</param>
    /// <param name="layout">The layout of the element.</param>;
    // TODO: default values
    public UITextLink(string text, Texture icon, Action clicked, UIInteractionStyles styles, UILayout layout) : base(layout)
    {
        Styler = styles.Styler;
        Text = new(this, text, true);
        Icon = icon;
        Layout.SetSize(() => Text.Width + (Icon is not null ? Height : 0), () => Text.Height); // TODO: padding
        OnClick += clicked;
    }

    /// <inheritdoc/>
    public override void Render(double delta, UIStyle style)
    {
        if (Icon is null)
        {
            style.TextFont.DrawFancyText(Text, new(X, Y, 0));
            return;
        }
        View.Rendering.RenderFittedTextureRectangle(View.UIContext, Icon, X, Y, X + Height, Y + Height, TextureFit.STRETCH);
        Renderer2D.SetColor(style.BaseColor);
        style.TextFont.DrawFancyText(Text, new(X + Height, Y, 0));
        Renderer2D.SetColor(Color4F.White);
    }
}
