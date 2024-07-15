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

namespace FGEGraphics.UISystem;

/// <summary>Represents a scrollable box containing other elements.</summary>
// TODO: handle navigational scroll
public class UIScrollGroup : UIScissorGroup
{
    public class ScrollDirection(bool vertical, Func<int> rangeLength)
    {
        public bool Vertical = vertical;

        public int Value = 0;

        public int MaxValue = -1;

        public int ScrollSpeed = 10;

        public UIButton ScrollBar = null;

        public int BarHeldOffset = -1;

        public int RangeLength => rangeLength();

        public int BarLength => MaxValue > 0 ? (int)((double)RangeLength / (MaxValue + RangeLength) * RangeLength) : 0;

        public int BarPosition => MaxValue > 0 ? (int)((RangeLength - BarLength) * ((double)Value / MaxValue)) : 0;

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

    /// <summary>The current scroll position.</summary>
    //public Vector2i Values = Vector2i.Zero;

    /// <summary>An upper limit on how far the group can be scrolled. -1 for unlimited scrolling, 0 for no scrolling.</summary>
    //public Vector2i MaxValues = new(-1, -1);

    /// <summary>How fast the group can be scrolled (in position units per scroll tick).</summary>
    //public Vector2i ScrollSpeeds = new(10, 10);

    public ScrollDirection ScrollY;

    public ScrollDirection ScrollX;

    /// <summary>The vertical scroll bar button.</summary>
    //public UIButton VerticalScrollBar;

    /// <summary>The horizontal scroll bar button.</summary>
    //public UIButton HorizontalScrollBar;

    /// <summary>Data internal to a <see cref="UIScrollGroup"/> instance.</summary>
    //public InternalData Internal = new();

    /// <summary>Data internal to a <see cref="UIScrollGroup"/> instance.</summary>
    //public struct InternalData()
    //{
        /// <summary>The held Y offset of the vertical scroll bar.</summary>
    //    public int HeldY = -1;

        /// <summary>The held X offset of the horizontal scroll bar.</summary>
    //    public int HeldX = -1;
    //}

    /// <summary>Constructs the UI scroll group.</summary>
    /// <param name="pos">The position of the element.</param>
    /// <param name="barStyles">The <see cref="VerticalScrollBar"/> styles.</param>
    /// <param name="barWidth">The width of the <see cref="VerticalScrollBar"/>.</param>
    public UIScrollGroup(UIPositionHelper pos, UIClickableElement.StyleGroup barStyles = null, int barWidth = 0, bool verticalBar = false, bool horizontalBar = false) : base(pos)
    {
        ScrollY = new(true, () => Position.Height, verticalBar, barWidth, barStyles, new UIPositionHelper(pos.View).Anchor(UIAnchor.TOP_RIGHT));
        ScrollX = new(false, () => Position.Width, horizontalBar, barWidth, barStyles, new UIPositionHelper(pos.View).Anchor(UIAnchor.BOTTOM_LEFT));
        if (verticalBar)
        {
            base.AddChild(ScrollY.ScrollBar);
        }
        if (horizontalBar)
        {
            base.AddChild(ScrollX.ScrollBar);
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
        if (ScrollY.ScrollBar is not null)
        {
            pressed |= ScrollY.TickMouseDrag(Window.MouseY, Position.Y);
        }
        if (ScrollX.ScrollBar is not null)
        {
            pressed |= ScrollX.TickMouseDrag(Window.MouseX, Position.X);
        }
        return pressed;
    }

    /// <summary>Ticks the scroll wheel and modifies the scroll value.</summary>
    // TODO: Handle horizontal scroll
    public void TickMouseScroll()
    {
        ScrollY.TickMouseScroll(Window.CurrentMouse.ScrollDelta.Y);
        ScrollX.TickMouseScroll(Window.CurrentMouse.ScrollDelta.X);
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
