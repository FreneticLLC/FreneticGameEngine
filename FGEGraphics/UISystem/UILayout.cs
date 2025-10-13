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

namespace FGEGraphics.UISystem;

/// <summary>Represents the positioning, sizing, and rotation logic for a <see cref="UIElement"/>.</summary>
public class UILayout
{
    /// <summary>The element this layout is applied to.</summary>
    public UIElement Element;

    /// <summary>The positional anchor.</summary>
    public UIAnchor Anchor = UIAnchor.TOP_LEFT;

    /// <summary>Internal data for <see cref="UILayout"/>.</summary>
    public struct InternalData
    {
        /// <summary>Represents either a constant or dynamic value.</summary>
        public struct Value<T> where T : unmanaged
        {
            /// <summary>Constant value.</summary>
            public T Constant;

            /// <summary>Dynamic value.</summary>
            public Func<T> Dynamic;

            /// <summary>Gets the resulting value.</summary>
            public readonly T Get() => Dynamic?.Invoke() ?? Constant;
        }

        /// <summary>Internal coordinate data. Generally, do not use.</summary>
        public Value<int> X, Y, Width, Height;

        /// <summary>Internal coordinate data. Generally, do not use.</summary>
        public Value<float> Rotation, Scale;
    }

    /// <summary>Internal data that should usually not be accessed directly.</summary>
    public InternalData Internal;

    /// <summary>Constructs an empty layout.</summary>
    public UILayout()
    {
        SetPosition(0, 0);
        SetSize(0, 0);
        SetRotation(0f);
        SetScale(1f);
    }

    /// <summary>Constructs a new layout as a copy of another layout without the attached <see cref="Element"/>.</summary>
    /// <param name="layout">The layout to copy.</param>
    public UILayout(UILayout layout)
    {
        Anchor = layout.Anchor;
        Internal = layout.Internal;
    }

    /// <summary>Sets the anchor.</summary>
    /// <returns>This object.</returns>
    public UILayout SetAnchor(UIAnchor anchor)
    {
        Anchor = anchor;
        return this;
    }

    /// <summary>Sets a constant X value.</summary>
    /// <returns>This object.</returns>
    public UILayout SetX(int x)
    {
        Internal.X = new() { Constant = x };
        return this;
    }

    /// <summary>Sets a constant Y value.</summary>
    /// <returns>This object.</returns>
    public UILayout SetY(int y)
    {
        Internal.Y = new() { Constant = y };
        return this;
    }

    /// <summary>Sets a constant position.</summary>
    /// <returns>This object.</returns>
    public UILayout SetPosition(int x, int y)
    {
        Internal.X = new() { Constant = x };
        Internal.Y = new() { Constant = y };
        return this;
    }

    /// <summary>Sets a constant position.</summary>
    /// <returns>This object.</returns>
    public UILayout SetPosition(Location location) => SetPosition((int)location.X, (int)location.Y);

    /// <summary>Sets a constant width.</summary>
    /// <returns>This object.</returns>
    public UILayout SetWidth(int width)
    {
        Internal.Width = new() { Constant = width };
        return this;
    }

    /// <summary>Sets a constant height.</summary>
    /// <returns>This object.</returns>
    public UILayout SetHeight(int height)
    {
        Internal.Height = new() { Constant = height };
        return this;
    }

    /// <summary>Sets a constant size.</summary>
    /// <returns>This object.</returns>
    public UILayout SetSize(int width, int height)
    {
        Internal.Width = new() { Constant = width };
        Internal.Height = new() { Constant = height };
        return this;
    }

    /// <summary>Sets a constant rotation.</summary>
    /// <returns>This object.</returns>
    public UILayout SetRotation(float rotation)
    {
        Internal.Rotation = new() { Constant = rotation };
        return this;
    }

    /// <summary>Sets a constant scale.</summary>
    /// <returns>This object.</returns>
    public UILayout SetScale(float scale)
    {
        Internal.Scale = new() { Constant = scale };
        return this;
    }

    /// <summary>Sets a dynamic X value.</summary>
    /// <returns>This object.</returns>
    public UILayout SetX(Func<int> x)
    {
        Internal.X = new() { Dynamic = x };
        return this;
    }

    /// <summary>Sets a dynamic Y value.</summary>
    /// <returns>This object.</returns>
    public UILayout SetY(Func<int> y)
    {
        Internal.Y = new() { Dynamic = y };
        return this;
    }

    /// <summary>Sets a dynamic position.</summary>
    /// <returns>This object.</returns>
    public UILayout SetPosition(Func<int> x, Func<int> y)
    {
        Internal.X = new() { Dynamic = x };
        Internal.Y = new() { Dynamic = y };
        return this;
    }

    /// <summary>Sets a dynamic position.</summary>
    /// <returns>This object.</returns>
    public UILayout SetPosition(Func<Location> location) => SetPosition(() => (int)location().X, () => (int)location().Y);

    /// <summary>Sets a dynamic width.</summary>
    /// <returns>This object.</returns>
    public UILayout SetWidth(Func<int> width)
    {
        Internal.Width = new() { Dynamic = width };
        return this;
    }

    /// <summary>Sets a dynamic height.</summary>
    /// <returns>This object.</returns>
    public UILayout SetHeight(Func<int> height)
    {
        Internal.Height = new() { Dynamic = height };
        return this;
    }

    /// <summary>Sets a dynamic size.</summary>
    /// <returns>This object.</returns>
    public UILayout SetSize(Func<int> width, Func<int> height)
    {
        Internal.Width = new() { Dynamic = width };
        Internal.Height = new() { Dynamic = height };
        return this;
    }

    /// <summary>Sets a dynamic rotation.</summary>
    /// <returns>This object.</returns>
    public UILayout SetRotation(Func<float> rotation)
    {
        Internal.Rotation = new() { Dynamic = rotation };
        return this;
    }

    /// <summary>Sets a dynamic scale.</summary>
    /// <returns>This object.</returns>
    public UILayout SetScale(Func<float> scale)
    {
        Internal.Scale = new() { Dynamic = scale };
        return this;
    }

    /// <summary>Fixes the position at the top-left origin.</summary>
    /// <returns>This object.</returns>
    public UILayout SetOrigin() => SetAnchor(UIAnchor.TOP_LEFT).SetPosition(0, 0);

    /// <summary>Returns a copy of this layout fixed at the top-left origin.</summary>
    public UILayout AtOrigin() => new UILayout(this).SetOrigin();

    /// <summary>Gets the X value.</summary>
    public int X => Internal.X.Get() + (Element.Parent != null ? Anchor.GetX(Element) : 0);

    /// <summary>Gets the Y value.</summary>
    public int Y => Internal.Y.Get() + (Element.Parent != null ? Anchor.GetY(Element) : 0);

    /// <summary>Gets the width.</summary>
    public int Width => Internal.Width.Get();

    /// <summary>Gets the height.</summary>
    public int Height => Internal.Height.Get();

    /// <summary>Gets the rotation.</summary>
    public float Rotation => Internal.Rotation.Get();

    /// <summary>Gets the scale.</summary>
    public float Scale => Internal.Scale.Get() * (Element.Parent?.Scale ?? 1f);

    /// <summary>Gets the position vector.</summary>
    public Vector2i Position => new(X, Y);

    /// <summary>Gets the size vector.</summary>
    public Vector2i Size => new(Width, Height);

    /// <summary>Converts this layout's present state to a simple debug string.</summary>
    public override string ToString() => $"UILayout {{ Position: {Position}, Size: {Size}, Rotation: {Rotation} }}";
}
