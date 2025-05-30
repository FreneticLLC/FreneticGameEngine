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
using FGECore.MathHelpers;
using FGEGraphics.ClientSystem;
using FGEGraphics.UISystem;

namespace FGEGraphics.UISystem;

/// <summary>Represents a toggleable button on the screen.</summary>
// TODO: orientation of label
public class UICheckbox : UIElement
{
    /// <summary>The button for this checkbox.</summary>
    public UIButton Button;

    /// <summary>The label to render alongside this checkbox.</summary>
    public UILabel Label;

    /// <summary>Whether this checkbox is toggled.</summary>
    public bool Toggled = false;

    /// <summary>Fired when the checkbox is toggled.</summary>
    public Action<bool> OnToggle;

    /// <summary>Constructs a new button-based checkbox.</summary>
    /// <param name="text">The text to display.</param>
    /// <param name="label">The text label style.</param>
    /// <param name="styles">The clickable styles.</param>
    /// <param name="pos">The position of the element.</param>
    public UICheckbox(string text, UIElementStyle label, UIClickableElement.StyleGroup styles, UIPositionHelper pos) : base(pos)
    {
        AddChild(Button = new UIButton(null, Toggle, styles, pos.AtOrigin()));
        AddChild(Label = new UILabel(text, label, pos.AtOrigin().ConstantWidth(-1)));
        Label.Position.GetterX(() => Button.Width * 3 / 2).GetterY(() => (Height - Label.Height) / 2);
        Position.GetterWidth(() => Label.Position.X + Label.Width); // TODO generalize
    }

    /// <summary>Constructs a new checkbox using the normal button style as the label style.</summary>
    public UICheckbox(string text, UIClickableElement.StyleGroup styles, UIPositionHelper pos) : this(text, new(styles.Normal) { BaseColor = Color4F.Transparent }, styles, pos)
    {
    }

    /// <summary>Toggles this checkbox.</summary>
    public void Toggle()
    {
        Toggled = !Toggled;
        Button.Text.Content = Toggled ? "X" : null;
        OnToggle(Toggled);
    }
}
