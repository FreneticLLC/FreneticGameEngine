//
// This file is part of the Frenetic Game Engine, created by Frenetic LLC.
// This code is Copyright (C) Frenetic LLC under the terms of a strict license.
// See README.md or LICENSE.txt in the FreneticGameEngine source root for the contents of the license.
// If neither of these are available, assume that neither you nor anyone other than the copyright holder
// hold any right or permission to use this software until such time as the official license is identified.
//

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FGECore.MathHelpers;
using FGEGraphics.ClientSystem;
using FGEGraphics.UISystem;

namespace FGEGraphics.UISystem;

/// <summary>Represents a toggleable box on the screen.</summary>
// TODO: generalize beyond an 'X' when toggled
public class UIToggleBox : UIBox
{
    /// <summary>Whether this box is toggled on.</summary>
    public bool Toggled = false;

    /// <summary>Fired when this box is toggled on or off.</summary>
    public Action<bool> OnToggle;

    /// <summary>Constructs a new <see cref="UIToggleBox"/>.</summary>
    /// <param name="styling">The styling logic of the element.</param>
    /// <param name="layout">The layout of the element.</param>
    /// <param name="toggled">The initial toggle state.</param>
    public UIToggleBox(UIStyling styling, UILayout layout, bool toggled = false) : base(styling, layout)
    {
        SetToggled(toggled);
    }

    /// <summary>Sets the toggle state of this box.</summary>
    public void SetToggled(bool toggled)
    {
        Toggled = toggled;
        Text.Content = Toggled ? "X" : null;
    }

    /// <summary>Toggles this box.</summary>
    public void Toggle()
    {
        SetToggled(!Toggled);
        OnToggle?.Invoke(Toggled);
    }

    /// <inheritdoc/>
    public override void Clicked() => Toggle();

    public static (UIToggleBox Box, UILabel Label, UIListGroup List) WithLabel(string text, int spacing, UIStyling styling, UILayout layout, bool toggled = false, UIStyling labelStyling = default, UIAnchor listAnchor = null)
    {
        UIToggleBox box = new(styling, layout.AtOrigin(), toggled);
        UIListGroup list = new(spacing, layout, vertical: false, anchor: listAnchor ?? UIAnchor.TOP_LEFT);
        UILabel label = new(text, labelStyling.IsEmpty ? styling.Bind(box) : labelStyling, new UILayout());
        list.AddListItem(box);
        list.AddListItem(label);
        return (box, label, list);
    }
}
