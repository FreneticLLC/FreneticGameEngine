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

namespace FGEGraphics.UISystem;

/// <summary>Arguments for a <see cref="UITabGroup.OnTabSwitch"/> event handler.</summary>
/// <param name="From">The tab screen being switched from.</param>
/// <param name="To">The tab screen being switched to.</param>
public record TabSwitchedArgs(UIScreen From, UIScreen To);

/// <summary>
/// Represents a container of elements supporting clickable <see cref="UIElement"/>s that lead to <see cref="UIScreen"/>s,
/// automatically handling the <see cref="UIElement.Enabled"/> state.
/// <param name="pos">The position of the element.</param>
/// <param name="onSwitch">Ran when the tab is switched.</param>
/// </summary>
public class UITabGroup(UIPositionHelper pos, Action<TabSwitchedArgs> onSwitch = null) : UIGroup(pos)
{
    /// <summary>Ran when the tab is switched.</summary>
    public Action<TabSwitchedArgs> OnTabSwitch = onSwitch;

    /// <summary>The button leading to the currently selected tab.</summary>
    public UIElement SelectedButton;

    /// <summary>The currently selected tab.</summary>
    public UIScreen SelectedTab;

    /// <summary>Switches the selected tab and fires <see cref="OnTabSwitch"/>.</summary>
    /// <param name="button">The button to disable or <c>null</c> if not necessary.</param>
    /// <param name="tab">The tab to switch to.</param>
    public void SwitchTab(UIElement button, UIScreen tab)
    {
        if (SelectedButton is not null)
        {
            SelectedButton.Enabled = true;
            SelectedButton.Pressed = false;
            SelectedTab.SwitchFrom();
        }
        if (button is not null)
        {
            button.Enabled = false;
            button.Pressed = true;
        }
        tab.SwitchTo();
        OnTabSwitch(new(SelectedTab, tab));
        SelectedButton = button;
        SelectedTab = tab;
    }

    /// <summary>Adds a button and a screen as a tab to the group.</summary>
    /// <param name="button">The button linked to the screen.</param>
    /// <param name="tab">The screen to switch to when the button is pressed.</param>
    /// <param name="main">Whether this tab should be selected by default.</param>
    /// <param name="addChild">Whether to add the button as a child element of the group.</param>
    public void AddTab(UIElement button, UIScreen tab, bool main = false, bool addChild = true)
    {
        if (main)
        {
            button.Enabled = false;
            button.Pressed = true;
            SelectedButton = button;
            SelectedTab = tab;
        }
        button.OnClick += () => SwitchTab(button, tab);
        if (addChild)
        {
            AddChild(button);
        }
    }
}
