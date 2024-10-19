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

/// <summary>Helper for positioning of UI elements.</summary>
public class UIPositionHelper
{
    /// <summary>Constructs the UI Position Helper.</summary>
    /// <param name="uiview">The backing view.</param>
    public UIPositionHelper(ViewUI2D uiview)
    {
        View = uiview;
        ConstantXY(0, 0);
        ConstantWidthHeight(0, 0);
        ConstantRotation(0f);
    }

    /// <summary>Constructs a UI Position Helper as a copy of another one.</summary>
    /// <param name="pos">The position helper to copy.</param>
    public UIPositionHelper(UIPositionHelper pos)
    {
        View = pos.View;
        MainAnchor = pos.MainAnchor;
        Internal = pos.Internal;
    }

    // TODO: Remove? (use UIListGroup)
    /// <summary>Returns a <see cref="UIPositionHelper"/> that is automatically below another one, by adding to the Y value, with an optional additional padding.</summary>
    public static UIPositionHelper Below(UIPositionHelper prior, int yPad)
    {
        UIPositionHelper newPos = new UIPositionHelper(prior.View).Anchor(prior.MainAnchor).GetterY(() => prior.Y + prior.Height + yPad);
        newPos.Internal.X = prior.Internal.X;
        return newPos;
    }

    /// <summary>The view backing this element's positioning logic.</summary>
    public ViewUI2D View;

    /// <summary>The element this is the position for.</summary>
    public UIElement For;

    /// <summary>The main positional anchor.</summary>
    public UIAnchor MainAnchor = UIAnchor.CENTER;

    /// <summary>Internal data for <see cref="UIPositionHelper"/>. Generally, do not access this directly.</summary>
    public struct InternalData
    {
        /// <summary>Helper to represent the data for a single coordinate, dynamically as a constant or getter.</summary>
        public struct Coordinate<T> where T : unmanaged
        {
            /// <summary>What mode this coordinate uses (eg constant or getter).</summary>
            public UIPosMode Mode;

            /// <summary>Constant value, if <see cref="Mode"/> is set to <see cref="UIPosMode.CONSTANT"/>.</summary>
            public T ConstVal;

            /// <summary>Getter value, if <see cref="Mode"/> is set to <see cref="UIPosMode.GETTER"/>.</summary>
            public Func<T> GetterVal;

            /// <summary>Gets the current value for this coordinate.</summary>
            public readonly T Get()
            {
                if (Mode == UIPosMode.CONSTANT)
                {
                    return ConstVal;
                }
                if (Mode == UIPosMode.GETTER)
                {
                    return GetterVal();
                }
                return default;
            }
        }

        /// <summary>Internal coordinate data. Generally, do not use.</summary>
        public Coordinate<int> X, Y, Width, Height;

        /// <summary>Internal coordinate data. Generally, do not use.</summary>
        public Coordinate<float> Rotation;
    }

    /// <summary>Internal data that should usually not be accessed directly.</summary>
    public InternalData Internal;

    /// <summary>Sets an anchor.</summary>
    /// <returns>This object.</returns>
    public UIPositionHelper Anchor(UIAnchor anchor)
    {
        MainAnchor = anchor;
        return this;
    }

    /// <summary>Sets a constant X value.</summary>
    /// <returns>This object.</returns>
    public UIPositionHelper ConstantX(int x)
    {
        Internal.X = new() { Mode = UIPosMode.CONSTANT, ConstVal = x };
        return this;
    }

    /// <summary>Sets a constant Y value.</summary>
    /// <returns>This object.</returns>
    public UIPositionHelper ConstantY(int y)
    {
        Internal.Y = new() { Mode = UIPosMode.CONSTANT, ConstVal = y };
        return this;
    }

    /// <summary>Sets a constant X and Y value.</summary>
    /// <returns>This object.</returns>
    public UIPositionHelper ConstantXY(int x, int y)
    {
        Internal.X = new() { Mode = UIPosMode.CONSTANT, ConstVal = x };
        Internal.Y = new() { Mode = UIPosMode.CONSTANT, ConstVal = y };
        return this;
    }

    /// <summary>Sets a constant X and Y value using a <see cref="Location"/>.</summary>
    /// <returns>This object.</returns>
    public UIPositionHelper ConstantXY(Location location) => ConstantXY((int)location.X, (int)location.Y);

    /// <summary>Sets a constant Width value.</summary>
    /// <returns>This object.</returns>
    public UIPositionHelper ConstantWidth(int width)
    {
        Internal.Width = new() { Mode = UIPosMode.CONSTANT, ConstVal = width };
        return this;
    }

    /// <summary>Sets a constant Height value.</summary>
    /// <returns>This object.</returns>
    public UIPositionHelper ConstantHeight(int height)
    {
        Internal.Height = new() { Mode = UIPosMode.CONSTANT, ConstVal = height };
        return this;
    }

    /// <summary>Sets a constant Width and Height value.</summary>
    /// <returns>This object.</returns>
    public UIPositionHelper ConstantWidthHeight(int width, int height)
    {
        Internal.Width = new() { Mode = UIPosMode.CONSTANT, ConstVal = width };
        Internal.Height = new() { Mode = UIPosMode.CONSTANT, ConstVal = height };
        return this;
    }

    /// <summary>Sets a constant Rotation value.</summary>
    /// <returns>This object.</returns>
    public UIPositionHelper ConstantRotation(float rotation)
    {
        Internal.Rotation = new() { Mode = UIPosMode.CONSTANT, ConstVal = rotation };
        return this;
    }

    /// <summary>Sets a getter X value.</summary>
    /// <returns>This object.</returns>
    public UIPositionHelper GetterX(Func<int> x)
    {
        Internal.X = new() { Mode = UIPosMode.GETTER, GetterVal = x };
        return this;
    }

    /// <summary>Sets a getter Y value.</summary>
    /// <returns>This object.</returns>
    public UIPositionHelper GetterY(Func<int> y)
    {
        Internal.Y = new() { Mode = UIPosMode.GETTER, GetterVal = y };
        return this;
    }

    /// <summary>Sets a getter X and Y value.</summary>
    /// <returns>This object.</returns>
    public UIPositionHelper GetterXY(Func<int> x, Func<int> y)
    {
        Internal.X = new() { Mode = UIPosMode.GETTER, GetterVal = x };
        Internal.Y = new() { Mode = UIPosMode.GETTER, GetterVal = y };
        return this;
    }

    /// <summary>Sets getter X and Y values using a <see cref="Location"/>.</summary>
    /// <returns>This object.</returns>
    public UIPositionHelper GetterXY(Func<Location> location) => GetterXY(() => (int)location().X, () => (int)location().Y);

    /// <summary>Sets a getter Width value.</summary>
    /// <returns>This object.</returns>
    public UIPositionHelper GetterWidth(Func<int> width)
    {
        Internal.Width = new() { Mode = UIPosMode.GETTER, GetterVal = width };
        return this;
    }

    /// <summary>Sets a getter Height value.</summary>
    /// <returns>This object.</returns>
    public UIPositionHelper GetterHeight(Func<int> height)
    {
        Internal.Height = new() { Mode = UIPosMode.GETTER, GetterVal = height };
        return this;
    }

    /// <summary>Sets a constant Width and Height value.</summary>
    /// <returns>This object.</returns>
    public UIPositionHelper GetterWidthHeight(Func<int> width, Func<int> height)
    {
        Internal.Width = new() { Mode = UIPosMode.GETTER, GetterVal = width };
        Internal.Height = new() { Mode = UIPosMode.GETTER, GetterVal = height };
        return this;
    }

    /// <summary>Sets a getter Rotation value.</summary>
    /// <returns>This object.</returns>
    public UIPositionHelper GetterRotation(Func<float> rotation)
    {
        Internal.Rotation = new() { Mode = UIPosMode.GETTER, GetterVal = rotation };
        return this;
    }

    /// <summary>Fixes the position at the top-left origin.</summary>
    /// <returns>This object.</returns>
    public UIPositionHelper SetOrigin() => Anchor(UIAnchor.TOP_LEFT).ConstantXY(0, 0);

    /// <summary>Returns a copy of the position fixed at the top-left origin.</summary>
    public UIPositionHelper AtOrigin() => new UIPositionHelper(this).SetOrigin();

    /// <summary>Gets the X coordinate.</summary>
    public int X => Internal.X.Get() + (For.Parent != null ? MainAnchor.GetX(For) : 0);

    /// <summary>Gets the Y coordinate.</summary>
    public int Y => Internal.Y.Get() + (For.Parent != null ? MainAnchor.GetY(For) : 0);

    /// <summary>Gets the width.</summary>
    public int Width => Internal.Width.Get();

    /// <summary>Gets the height.</summary>
    public int Height => Internal.Height.Get();

    /// <summary>Gets the local Rotation value.</summary>
    public float Rotation => Internal.Rotation.Get();

    /// <summary>Gets the X/Y coordinate pair.</summary>
    public Vector2i Position => new(X, Y);

    /// <summary>Gets the Width/Height coordinate pair.</summary>
    public Vector2i Size => new(Width, Height);

    /// <summary>Converts this position helper's present data to a simplified debug string.</summary>
    public override string ToString() => $"UIPositionHelper:PresentState(XY: {X}, {Y} / WH: {Width}, {Height} / Rot: {Rotation})";
}

/// <summary>Modes for the <see cref="UIPositionHelper"/>.</summary>
public enum UIPosMode : byte
{
    /// <summary>A constant position.</summary>
    CONSTANT = 0,
    /// <summary>A getter function.</summary>
    GETTER = 1
    // TODO: More modes?
}
