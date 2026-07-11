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

/// <summary>Represents a single value of a <see cref="UIStyling"/> object that resolves to a <see cref="UIStyle"/> value each frame.</summary>
public readonly struct UIStyleValue<T>
{
    /// <summary>A constant value.</summary>
    public readonly T Constant;

    /// <summary>A dynamic style.</summary>
    public readonly Func<UIElement, T> Dynamic;

    /// <summary>Constructs a constant style value.</summary>
    /// <param name="constant">The constant value.</param>
    public UIStyleValue(T constant)
    {
        Constant = constant;
    }

    /// <summary>Constructs a dynamic style value.</summary>
    /// <param name="dynamic">The dynamic value.</param>
    public UIStyleValue(Func<UIElement, T> dynamic)
    {
        Dynamic = dynamic;
    }

    /// <summary>Returns the style value for the specified <paramref name="element"/>.</summary>
    /// <param name="element">The element to be styled.</param>
    public readonly T Get(UIElement element) => Dynamic is not null ? Dynamic.Invoke(element) : Constant;

    /// <summary>Calls <see cref="UIStyleValue{T}(T)"/>.</summary>
    public static implicit operator UIStyleValue<T>(T constant) => new(constant);

    /// <summary>Calls <see cref="UIStyleValue{T}(Func{UIElement, T})"/>.</summary>
    public static implicit operator UIStyleValue<T>(Func<UIElement, T> dynamic) => new(dynamic);

    /// <summary>Constructs a style value dependent on the interaction state of an element.</summary>
    /// <param name="idle">The value to use when the element is not being interacted with.</param>
    /// <param name="hovered">The value to use when the element is hovered.</param>
    /// <param name="pressed">The value to use when the element is pressed.</param>
    /// <param name="disabled">The value to use when the element is disabled.</param>
    public static UIStyleValue<T> Interactive(T idle, T hovered, T pressed, T disabled) => new(
        element => element.IsPressed ? pressed
            : element.IsHovered ? hovered
            : !element.IsEnabled ? disabled
            : idle
    );
}
