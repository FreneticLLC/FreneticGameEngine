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
public class UITextLink : UIClickableElement
{
    /// <summary>The text to display for this link.</summary>
    public UIElementText Text;

    /// <summary>The icon to display alongside this link.</summary>
    // TODO: UIImage
    public Texture Icon;

    /// <summary>Constructs an interactable text link.</summary>
    /// <param name="text">The text to display.</param>
    /// <param name="icon">The icon to display alongside the text.</param>
    /// <param name="clicked">The action to run when clicked.</param>
    /// <param name="styles">The clickable styles.</param>
    /// <param name="pos">The position of the element.</param>
    public UITextLink(string text, Texture icon, Action clicked, StyleGroup styles, UIPositionHelper pos) : base(styles, pos, true, clicked)
    {
        Text = new(this, text, true);
        Icon = icon;
        UpdateStyle();
        Position.GetterWidthHeight(() => Text.Width, () => Text.Height);
    }

    /// <summary>Performs a render on this link.</summary>
    /// <param name="view">The UI view.</param>
    /// <param name="delta">The time since the last render.</param>
    /// <param name="style">The current element style.</param>
    public override void Render(ViewUI2D view, double delta, UIElementStyle style)
    {
        Location textLocation = new(X, Y, 0);
        if (Icon is null)
        {
            style.TextFont.DrawFancyText(Text, textLocation);
            return;
        }
        int fontHeight = style.TextFont.Height;
        textLocation += new Location(fontHeight, fontHeight, 0);
        Icon.Bind();
        Renderer2D.SetColor(style.BaseColor);
        view.Rendering.RenderRectangle(view.UIContext, X, Y, textLocation.XF, textLocation.YF, new Vector3(-0.5f, -0.5f, LastAbsoluteRotation));
        style.TextFont.DrawFancyText(Text, textLocation);
        Renderer2D.SetColor(Color4F.White);
    }
}
