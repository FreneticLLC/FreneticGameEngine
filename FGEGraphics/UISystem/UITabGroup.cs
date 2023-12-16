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

public class UITabGroup
{
    public EventHandler<UIScreen> TabSwitched;

    public struct InternalData
    {
        public UIClickableElement CurrentButton;
        public UIScreen CurrentTab;
    }

    public InternalData Internal;

    public UITabGroup(Action<UIScreen> onSwitch)
    {
        TabSwitched += (_, tab) => onSwitch(tab);
    }

    public void Add(UIClickableElement button, UIScreen tab)
    {
        if (Internal.CurrentButton is null)
        {
            button.Enabled = false;
            Internal.CurrentButton = button;
            Internal.CurrentTab = tab;
        }
        button.Clicked += (_, _) =>
        {
            Internal.CurrentButton.Enabled = true;
            Internal.CurrentTab.SwitchFrom();
            button.Enabled = false;
            tab.SwitchTo();
            Internal.CurrentButton = button;
            Internal.CurrentTab = tab;
            TabSwitched.Invoke(this, tab);
        };
    }
}
