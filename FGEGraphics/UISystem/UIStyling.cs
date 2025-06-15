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

public struct UIStyling
{
    public static readonly UIStyling Empty = new(null);

    public UIStyle Constant;

    public Func<UIElement, UIStyle> Dynamic;

    public UIStyling(UIStyle style)
    {
        Constant = style;
    }

    public UIStyling(Func<UIElement, UIStyle> styling)
    {
        Dynamic = styling;
    }

    public UIStyle Get(UIElement element) => Constant ?? Dynamic?.Invoke(element) ?? UIStyle.Empty;

    public UIStyling For(UIElement element)
    {
        if (Constant is not null)
        {
            return this;
        }
        Func<UIElement, UIStyle> dynamic = Dynamic;
        return new(element => dynamic?.Invoke(element));
    }

    public static implicit operator UIStyling(UIStyle style) => new(style);

    public static implicit operator UIStyling(Func<UIElement, UIStyle> styling) => new(styling);
}
