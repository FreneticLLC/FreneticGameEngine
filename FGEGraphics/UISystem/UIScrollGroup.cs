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
using FGECore.MathHelpers;
using FGEGraphics.ClientSystem;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace FGEGraphics.UISystem;

/// <summary>Represents a scrollable box containing other elements.</summary>
// TODO: handle navigational scroll
public class UIScrollGroup : UIElement
{
    /// <summary>The horizontal scroll axis.</summary>
    public Axis ScrollX;

    /// <summary>The vertical scroll axis.</summary>
    public Axis ScrollY;

    /// <summary>The scissor layer for child elements.</summary>
    public UIScissorGroup ScissorLayer;

    /// <summary>The scroll bar layer (above the scissor layer).</summary>
    public UIGroup ScrollBarLayer;

    /// <summary>Whether either of the scroll bars are pressed.</summary>
    public bool ScrollBarPressed => ScrollX.ScrollBar?.Pressed ?? ScrollY.ScrollBar?.Pressed ?? false;

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
        ScrollX = new(false, () => Width/* - (barY ? barWidth : 0)*/, barX, barWidth, barStyles, new UIPositionHelper(pos.View).Anchor(barXAnchor ?? UIAnchor.BOTTOM_LEFT));
        ScrollY = new(true, () => Height/* - (barX ? barWidth : 0)*/, barY, barWidth, barStyles, new UIPositionHelper(pos.View).Anchor(barYAnchor ?? UIAnchor.TOP_RIGHT));
        if (ScrollX.ScrollBar is not null || ScrollY.ScrollBar is not null)
        {
            base.AddChild(ScrollBarLayer = new(new UIPositionHelper(pos.AtOrigin())));
            if (ScrollX.ScrollBar is not null)
            {
                ScrollBarLayer.AddChild(ScrollX.ScrollBar);
            }
            if (ScrollY.ScrollBar is not null)
            {
                ScrollBarLayer.AddChild(ScrollY.ScrollBar);
            }
        }
        base.AddChild(ScissorLayer = new(new UIPositionHelper(pos.AtOrigin())));
    }

    /// <inheritdoc/>
    public override void AddChild(UIElement child)
    {
        UIPositionHelper original = new(child.Position);
        child.Position.GetterXY(() => original.Internal.X.Get() - ScrollX.Value, () => original.Internal.Y.Get() - ScrollY.Value);
        ScissorLayer.AddChild(child);
    }

    /// <inheritdoc/>
    public override void Tick(double delta)
    {
        base.Tick(delta);
        if (ScrollX.ScrollBar is not null)
        {
            ScrollX.TickMouseDrag(Window.MouseX, X);
        }
        if (ScrollY.ScrollBar is not null)
        {
            ScrollY.TickMouseDrag(Window.MouseY, Y);
        }
    }

    /// <inheritdoc/>
    public override void NavigateDirection(int horizontal, int vertical)
    {
        if (!ScrollBarPressed)
        {
            ScrollX.TickMouseScroll(horizontal);
            ScrollY.TickMouseScroll(vertical);
        }
    }

    /// <inheritdoc/>
    public override bool ScrollDirection(float horizontal, float vertical)
    {
        if (ScrollBarPressed)
        {
            return true; // TODO: or false, which better UX?
        }
        if (ScrollY.MaxValue == 0 || Window.Window.KeyboardState.IsKeyDown(Keys.LeftShift))
        {
            horizontal = vertical;
            vertical = 0;
        }
        ScrollX.TickMouseScroll(horizontal);
        ScrollY.TickMouseScroll(vertical);
        return true;
    }

    /// <summary>Contains scroll state for a direction.</summary>
    /// <param name="vertical">Whether the direction is vertical or horizontal.</param>
    /// <param name="rangeLength">The length of the outer group's relevant dimension.</param>
    public class Axis(bool vertical, Func<int> rangeLength)
    {
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
        public Axis(bool vertical, Func<int> rangeLength, bool hasBar, int width, UIClickableElement.StyleGroup styles, UIPositionHelper pos) : this(vertical, rangeLength)
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

        /// <summary>Sets the <see cref="Value"/> and <see cref="MaxValue"/> to 0.</summary>
        public void Reset()
        {
            Value = 0;
            MaxValue = 0;
        }

        /// <summary>Scrolls to encompass a min/max offset pair.</summary>
        /// <param name="min">The min offset.</param>
        /// <param name="max">The max offset.</param>
        public void ScrollToPos(int min, int max)
        {
            if (min < Value)
            {
                Value = min;
            }
            else if (max > RangeLength)
            {
                Value += max - RangeLength;
            }
        }

        /// <summary>Ticks the mouse dragging the <see cref="ScrollBar"/>.</summary>
        /// <param name="mousePos">The relevant mouse position.</param>
        /// <param name="groupPos">The relevant scroll group position.</param>
        public void TickMouseDrag(float mousePos, int groupPos)
        {
            if (!ScrollBar.Pressed)
            {
                BarHeldOffset = -1;
                return;
            }
            if (BarHeldOffset == -1)
            {
                BarHeldOffset = (int)mousePos - (vertical ? ScrollBar.Y : ScrollBar.X);
            }
            Value = (int)((double)(mousePos - groupPos - BarHeldOffset) / (RangeLength - BarLength) * MaxValue);
            Value = Math.Clamp(Value, 0, MaxValue);
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

    /// <inheritdoc/>
    public override List<string> GetDebugInfo()
    {
        List<string> info = base.GetDebugInfo();
        info.Add($"^7Scroll: ^3({ScrollX.Value}, {ScrollY.Value}) ^&| ^7Max Scroll: ^3({ScrollX.MaxValue}, {ScrollY.MaxValue})");
        return info;
    }
}
