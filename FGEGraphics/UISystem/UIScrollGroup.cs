//
// This file is part of the Frenetic Game Engine, created by Frenetic LLC.
// This code is Copyright (C) Frenetic LLC under the terms of a strict license.
// See README.md or LICENSE.txt in the FreneticGameEngine source root for the contents of the license.
// If neither of these are available, assume that neither you nor anyone other than the copyright holder
// hold any right or permission to use this software until such time as the official license is identified.
//

using System;
using System.Collections.Generic;
using FGECore.MathHelpers;
using FGEGraphics.ClientSystem;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace FGEGraphics.UISystem;

/// <summary>Represents a scrollable box containing other elements.</summary>
// TODO: handle navigational scroll
public class UIScrollGroup : UIScissorGroup
{
    /// <summary>Contains scroll state for a direction.</summary>
    /// <param name="vertical">Whether the direction is vertical or horizontal.</param>
    /// <param name="rangeLength">The length of the outer group's relevant dimension.</param>
    public class ScrollDirection(bool vertical, Func<int> rangeLength)
    {
        /// <summary>Whether the direction is vertical or horizontal.</summary>
        public bool Vertical = vertical;

        /// <summary>The current scroll position.</summary>
        public int Value = 0;

        /// <summary>An upper limit on how far the direction can be scrolled. -1 for unlimited scrolling, 0 for no scrolling.</summary>
        public int MaxValue = -1;

        /// <summary>How fast the direction can be scrolled (in position units per scroll tick).</summary>
        public int ScrollSpeed = 10;

        /// <summary>The scroll bar button, if any.</summary>
        public UIButton ScrollBar = null;

        /// <summary>The held position offset of the scroll bar.</summary>
        public int BarHeldOffset = -1;

        /// <summary>The length of the outer group's relevant dimension.</summary>
        public int RangeLength => rangeLength();

        /// <summary>The length of the scroll bar.</summary>
        public int BarLength => MaxValue > 0 ? (int)((double)RangeLength / (MaxValue + RangeLength) * RangeLength) : 0;

        /// <summary>The scroll bar's position offset.</summary>
        public int BarPosition => MaxValue > 0 ? (int)((RangeLength - BarLength) * ((double)Value / MaxValue)) : 0;

        /// <summary>Constructs a scroll direction.</summary>
        /// <param name="vertical">Whether the direction is vertical or horizontal.</param>
        /// <param name="rangeLength">The length of the outer group's relevant dimension.</param>
        /// <param name="hasBar">Whether to create the <see cref="ScrollBar"/>.</param>
        /// <param name="width">The width of the <see cref="ScrollBar"/>.</param>
        /// <param name="styles">The <see cref="ScrollBar"/> styles.</param>
        /// <param name="pos">The base position for the <see cref="ScrollBar"/>.</param>
        public ScrollDirection(bool vertical, Func<int> rangeLength, bool hasBar, int width, UIClickableElement.StyleGroup styles, UIPositionHelper pos) : this(vertical, rangeLength)
        {
            if (!hasBar)
            {
                return;
            }
            if (vertical)
            {
                pos.GetterY(() => BarPosition).GetterHeight(() => BarLength).ConstantWidth(width);
            }
            else
            {
                pos.GetterX(() => BarPosition).GetterWidth(() => BarLength).ConstantHeight(width);
            }
            ScrollBar = new(null, null, styles, pos);
        }

        /// <summary>Ticks the mouse dragging the <see cref="ScrollBar"/>.</summary>
        /// <param name="mousePos">The relevant mouse position.</param>
        /// <param name="groupPos">The relevant scroll group position.</param>
        /// <returns></returns>
        public bool TickMouseDrag(float mousePos, int groupPos)
        {
            if (!ScrollBar.Pressed)
            {
                BarHeldOffset = -1;
                return false;
            }
            if (BarHeldOffset == -1)
            {
                BarHeldOffset = (int)mousePos - (Vertical ? ScrollBar.Y : ScrollBar.X);
            }
            Value = (int)((double)(mousePos - groupPos - BarHeldOffset) / (RangeLength - BarLength) * MaxValue);
            Value = Math.Clamp(Value, 0, MaxValue);
            return true;
        }

        /// <summary>Ticks the scroll value based on the <paramref name="scrollDelta"/>.</summary>
        /// <param name="scrollDelta">The relevant scroll change.</param>
        public void TickMouseScroll(float scrollDelta)
        {
            Value -= (int)scrollDelta * ScrollSpeed;
            if (Value < 0)
            {
                Value = 0;
            }
            if (MaxValue != -1 && Value > MaxValue)
            {
                Value = MaxValue;
            }
        }
    }

    /// <summary>The horizontal scroll direction.</summary>
    public ScrollDirection ScrollX;

    /// <summary>The vertical scroll direction.</summary>
    public ScrollDirection ScrollY;

    /// <summary>Constructs the UI scroll group.</summary>
    /// <param name="pos">The position of the element.</param>
    /// <param name="barStyles">The scroll bar styles.</param>
    /// <param name="barWidth">The width of the scroll bars.</param>
    /// <param name="barX">Whether to add a horizontal scroll bar.</param>
    /// <param name="barY">Whether to add a vertical scroll bar.</param>
    /// <param name="barXAnchor">The anchor of the horizontal scroll bar.</param>
    /// <param name="barYAnchor">The anchor of the vertical scroll bar.</param>
    public UIScrollGroup(UIPositionHelper pos, UIClickableElement.StyleGroup barStyles = null, int barWidth = 0, bool barX = false, bool barY = false, UIAnchor barXAnchor = null, UIAnchor barYAnchor = null) : base(pos)
    {
        if (barXAnchor?.AlignmentX == UIAlignment.CENTER || barYAnchor?.AlignmentY == UIAlignment.CENTER)
        {
            throw new Exception("UIScrollGroup scroll bars must have non-central scroll directions");
        }
        // TODO: Fix scroll bar overlap
        ScrollX = new(false, () => Position.Width/* - (barY ? barWidth : 0)*/, barX, barWidth, barStyles, new UIPositionHelper(pos.View).Anchor(barXAnchor ?? UIAnchor.BOTTOM_LEFT));
        ScrollY = new(true, () => Position.Height/* - (barX ? barWidth : 0)*/, barY, barWidth, barStyles, new UIPositionHelper(pos.View).Anchor(barYAnchor ?? UIAnchor.TOP_RIGHT));
        if (barX)
        {
            base.AddChild(ScrollX.ScrollBar);
        }
        if (barY)
        {
            base.AddChild(ScrollY.ScrollBar);
        }
    }

    /// <inheritdoc/>
    public override void AddChild(UIElement child, bool priority = true)
    {
        UIPositionHelper original = new(child.Position);
        child.Position.GetterXY(() => original.Internal.X.Get() - ScrollX.Value, () => original.Internal.Y.Get() - ScrollY.Value);
        base.AddChild(child, priority);
    }

    /// <summary>Ticks the mouse dragging the scroll bar.</summary>
    public bool TickMouseDrag()
    {
        bool pressed = false;
        if (ScrollX.ScrollBar is not null)
        {
            pressed |= ScrollX.TickMouseDrag(Window.MouseX, Position.X);
        }
        if (ScrollY.ScrollBar is not null)
        {
            pressed |= ScrollY.TickMouseDrag(Window.MouseY, Position.Y);
        }
        return pressed;
    }

    /// <summary>Ticks the scroll wheel and modifies the scroll value.</summary>
    public void TickMouseScroll()
    {
        float deltaX = Window.CurrentMouse.ScrollDelta.X;
        float deltaY = Window.CurrentMouse.ScrollDelta.Y;
        if (Window.Window.KeyboardState.IsKeyDown(Keys.LeftShift))
        {
            deltaX = deltaY;
            deltaY = 0;
        }
        ScrollX.TickMouseScroll(deltaX);
        ScrollY.TickMouseScroll(deltaY);
    }

    /// <inheritdoc/>
    public override void Tick(double delta)
    {
        base.Tick(delta);
        bool barPressed = TickMouseDrag();
        if (ElementInternal.HoverInternal && !barPressed)
        {
            TickMouseScroll();
        }
    }

    /// <inheritdoc/>
    public override List<string> GetDebugInfo()
    {
        List<string> info = base.GetDebugInfo();
        info.Add($"^7Scroll: ^3({ScrollX.Value}, {ScrollY.Value})");
        return info;
    }
}
