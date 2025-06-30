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

/// <summary>Represents the styling logic for a <see cref="UIElement"/>.</summary>
public struct UIStyling
{
    /// <summary>Empty styling logic. Resolves to <see cref="UIStyle.Empty"/>.</summary>
    public static readonly UIStyling Empty = new((UIStyle)null);

    /// <summary>A constant style.</summary>
    public UIStyle Constant;

    /// <summary>A dynamic style. If present, updates the relevant <see cref="UIElement.Style"/> every frame.</summary>
    public Func<UIElement, UIStyle> Dynamic;

    // TODO: renames
    public readonly bool IsEmpty => Constant is null && Dynamic is null;

    /// <summary>Constructs styling logic using a constant style.</summary>
    /// <param name="style">The constant style.</param>
    public UIStyling(UIStyle style)
    {
        Constant = style;
    }

    /// <summary>Constructs styling logic using a dynamic style.</summary>
    /// <param name="styling">The dynamic style.</param>
    public UIStyling(Func<UIElement, UIStyle> styling)
    {
        Dynamic = styling;
    }

    /// <summary>
    /// Returns the style for the specified <paramref name="element"/> based on this styling logic.
    /// <para>Tries to evaluate <see cref="Constant"/> then <see cref="Dynamic"/>. If neither are present, resolves to <see cref="UIStyle.Empty"/>.</para>
    /// </summary>
    /// <param name="element">The element to be styled.</param>
    public readonly UIStyle Get(UIElement element) => Constant ?? Dynamic?.Invoke(element) ?? UIStyle.Empty;

    /// <summary>
    /// If <see cref="Dynamic"/> is present, returns a new <see cref="UIStyling"/> instance with the specified <paramref name="element"/> bound to the dynamic logic.
    /// Otherwise, returns this instance unaltered.
    /// </summary>
    /// <param name="element">The element to bind.</param>
    public readonly UIStyling Bind(UIElement element)
    {
        if (Dynamic is not null)
        {
            Func<UIElement, UIStyle> dynamic = Dynamic;
            return new(element => dynamic(element));
        }
        return this;
    }

    /// <summary>Calls <see cref="UIStyling(UIStyle)"/>.</summary>
    public static implicit operator UIStyling(UIStyle style) => new(style);

    /// <summary>Calls <see cref="UIStyling(Func{UIElement, UIStyle})"/>.</summary>
    public static implicit operator UIStyling(Func<UIElement, UIStyle> styling) => new(styling);
}
