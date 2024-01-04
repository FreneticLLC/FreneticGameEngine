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
using FGEGraphics.ClientSystem;
using FGEGraphics.UISystem;

namespace FGEGraphics.UISystem;

/// <summary>Represents a toggleable button on the screen.</summary>
public class UICheckbox : UIElement
{
    /// <summary>The button for this checkbox.</summary>
    public UIButton Button;

    /// <summary>The text to render alongside this checkbox.</summary>
    public UIElementText Text;

    /// <summary>Whether this checkbox is toggled.</summary>
    public bool Toggled = false;

    /// <summary>Constructs a new button-based checkbox.</summary>
    /// <param name="text">The text to display.</param>
    /// <param name="normal">The style to display when neither hovered nor clicked.</param>
    /// <param name="hover">The style to display when hovered.</param>
    /// <param name="click">The style to display when clicked.</param>
    /// <param name="pos">The position of the element.</param>
    public UICheckbox(string text, UIElementStyle normal, UIElementStyle hover, UIElementStyle click, UIPositionHelper pos) : base(pos)
    {
        RegisterStyle(normal, true);
        AddChild(Button = new UIButton(null, Toggle, normal, hover, click, pos.AtOrigin()));
        // TODO: update pos w/h with text size
        Text = CreateText(text, true, alignment: TextAlignment.LEFT); // TODO: alignment switches button pos
    }

    /// <summary>Toggles this checkbox.</summary>
    public void Toggle()
    {
        Toggled = !Toggled;
        Button.Text.Content = Toggled ? "X" : null;
    }

    public override UIElementStyle GetStyle() => Button.StyleNormal;

    public override void Render(ViewUI2D view, double delta, UIElementStyle style)
    {
        style.TextFont.DrawFancyText(Text, Text.GetPosition(X + Width + 10, Y + Height / 2));
    }

    public static implicit operator UIClickableElement(UICheckbox box) => box.Button;
}
