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
// TODO: generalize beyond an 'X' when toggled
public class UIToggleButton : UIElement
{
    /// <summary>The button for this checkbox.</summary>
    public UIButton Button;

    /// <summary>The label to render alongside this checkbox.</summary>
    public UILabel Label;

    /// <summary>Whether this checkbox is toggled on.</summary>
    public bool Toggled = false;

    /// <summary>Fired when the checkbox changes state.</summary>
    public Action<bool> OnToggle;

    /// <summary>Constructs a new toggle button.</summary>
    /// <param name="text">The text to display.</param>
    /// <param name="labelStyle">The text label style.</param>
    /// <param name="boxStyles">The clickable styles.</param>
    /// <param name="layout">The layout of the element.</param>
    public UIToggleButton(string text, UIInteractionStyles boxStyles, UILayout layout, bool toggled = false, UIStyle labelStyle = null) : base(layout)
    {
        AddChild(Button = new UIButton(null, Toggle, boxStyles, layout.AtOrigin()));
        AddChild(Label = new UILabel(text, labelStyle ?? boxStyles.Normal, layout.AtOrigin().SetWidth(-1)));
        Label.Layout.SetX(() => Button.Width * 3 / 2).SetY(() => (Height - Label.Height) / 2);
        Layout.SetWidth(() => Label.Layout.X + Label.Width); // TODO generalize
        SetToggled(toggled);
    }

    public void SetToggled(bool toggled)
    {
        Toggled = toggled;
        Button.Text.Content = Toggled ? "X" : null;
    }

    /// <summary>Toggles this checkbox.</summary>
    public void Toggle()
    {
        SetToggled(!Toggled);
        OnToggle?.Invoke(Toggled);
    }
}
