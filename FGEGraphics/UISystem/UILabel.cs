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
using OpenTK;
using OpenTK.Mathematics;

namespace FGEGraphics.UISystem;

/// <summary>Represents a simple piece of text on a screen.</summary>
public class UILabel : UIElement
{
    /// <summary>The text to display on this label.</summary>
    public UIElementText Text;

    /// <summary>Constructs a new label.</summary>
    /// <param name="text">The text to display on the label.</param>
    /// <param name="style">The style of the label.</param>
    /// <param name="pos">The position of the element.</param>
    public UILabel(string text, UIElementStyle style, UIPositionHelper pos) : base(pos)
    {
        Style = AddStyle(style, true);
        Text = new(this, text, true, Position.Width);
        UpdateStyle();
        Position.GetterWidthHeight(() => Text.Width, () => Text.Height);
    }

    /// <summary>Renders this label on the screen.</summary>
    /// <param name="view">The UI view.</param>
    /// <param name="delta">The time since the last render.</param>
    /// <param name="style">The current element style.</param>
    public override void Render(ViewUI2D view, double delta, UIElementStyle style)
    {
        if (style.BaseColor.A > 0)
        {
            Renderer2D.SetColor(style.BaseColor);
            view.Rendering.RenderRectangle(view.UIContext, X, Y, X + Width, Y + Height, new Vector3(-0.5f, -0.5f, LastAbsoluteRotation));
            Renderer2D.SetColor(Color4F.White);
        }
        style.TextFont.DrawFancyText(Text, new Location(X, Y, 0));
    }
}
