using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FGEGraphics.UISystem;

public readonly struct UIStyleValue<T>
{
    public readonly T Constant;

    public readonly Func<UIElement, T> Dynamic;

    public readonly bool IsEmpty => Constant is null && Dynamic is null;

    public UIStyleValue(T constant)
    {
        Constant = constant;
    }

    public UIStyleValue(Func<UIElement, T> dynamic)
    {
        Dynamic = dynamic;
    }

    public readonly T Get(UIElement element) => Constant ?? (Dynamic is not null ? Dynamic.Invoke(element) : default);

    public static implicit operator UIStyleValue<T>(T constant) => new(constant);

    public static implicit operator UIStyleValue<T>(Func<UIElement, T> dynamic) => new(dynamic);

    public static UIStyleValue<T> Interactive(T idle, T hovered, T pressed, T disabled) => new(
        element => element.IsPressed ? pressed
            : element.IsHovered ? hovered
            : !element.IsEnabled ? disabled
            : idle
    );
}
