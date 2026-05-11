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
// TODO: option to automatically calculate max scroll based on added elements
// TODO: add utility list + scrollgroup class
public class UIScrollGroup : UIElement
{
    /// <inheritdoc/>
    public override string Name => "Scroll Group";

    /// <summary>The horizontal scroll axis.</summary>
    [UIDebug]
    public ScrollAxis XAxis;

    /// <summary>The vertical scroll axis.</summary>
    [UIDebug]
    public ScrollAxis YAxis;

    /// <summary>The scroll bar layer (above the scissor layer).</summary>
    public UIGroup ScrollBarLayer;

    /// <summary>The scrollable scissor layer for child elements.</summary>
    public UIScissorGroup ScrollableLayer;

    /// <summary>Whether either of the scroll bars are pressed.</summary>
    public bool ScrollBarPressed => XAxis.ScrollBar?.IsPressed ?? YAxis.ScrollBar?.IsPressed ?? false;

    /// <summary>Constructs the UI scroll group.</summary>
    /// <param name="layout">The layout of the element.</param>
    public UIScrollGroup(UILayout layout) : base(UIStyling.Empty, layout)
    {
        // TODO: Fix scroll bar overlap
        XAxis = new(this, false);
        YAxis = new(this, true);
        AddChild(ScrollBarLayer = new UIGroup(layout.AtOrigin()));
        AddChild(ScrollableLayer = new UIScissorGroup(layout.AtOrigin()));
    }

    /// <inheritdoc/>
    public void AddScrollableChild(UIElement child)
    {
        UILayout original = new(child.Layout);
        child.Layout.SetPosition(() => original.Internal.X.Get() - XAxis.Value, () => original.Internal.Y.Get() - YAxis.Value);
        ScrollableLayer.AddChild(child);
    }

    /// <inheritdoc/>
    public override void Tick(double delta)
    {
        base.Tick(delta);
        if (XAxis.ScrollBar is not null)
        {
            XAxis.TickMouseDrag(View.Client.MouseX, X);
        }
        if (YAxis.ScrollBar is not null)
        {
            YAxis.TickMouseDrag(View.Client.MouseY, Y);
        }
    }

    /// <inheritdoc/>
    public override void Navigated(int horizontal, int vertical)
    {
        if (!ScrollBarPressed)
        {
            XAxis.TickMouseScroll(horizontal);
            YAxis.TickMouseScroll(vertical);
        }
    }

    /// <inheritdoc/>
    public override bool MouseScrolled(float horizontal, float vertical)
    {
        if (ScrollBarPressed)
        {
            return true;
        }
        if (YAxis.MaxValue == 0 || View.Client.Window.KeyboardState.IsKeyDown(Keys.LeftShift))
        {
            horizontal = vertical;
            vertical = 0;
        }
        XAxis.TickMouseScroll(horizontal);
        YAxis.TickMouseScroll(vertical);
        return true;
    }

    /// <inheritdoc/>
    public override void ScaleChanged(float from, float to)
    {
        XAxis.Clamp();
        YAxis.Clamp();
    }

    /// <summary>Contains scroll state for a direction.</summary>
    /// <param name="vertical">Whether the direction is vertical or horizontal.</param>
    /// <param name="length">The length of the outer group's relevant dimension.</param>
    public class ScrollAxis(UIScrollGroup scrollGroup, bool vertical)
    {
        public UIScrollGroup ScrollGroup = scrollGroup;

        public bool Vertical = vertical;

        /// <summary>The current scroll position.</summary>
        public int Value = 0;

        /// <summary>An upper limit on how far the direction can be scrolled. -1 for unlimited scrolling, 0 for no scrolling.</summary>
        public int MaxValue = -1;

        /// <summary>How fast the direction can be scrolled (in position units per scroll tick).</summary>
        public int ScrollSpeed = 30;

        /// <summary>The scroll bar button, if any.</summary>
        public UIBox ScrollBar = null;

        // TODO: maybe explain more? this is the mouse position relative to the bar position when first clicking / dragging it
        /// <summary>The held position offset of the scroll bar.</summary>
        public int BarHeldOffset = -1;

        /// <summary>The length of the outer group's relevant dimension.</summary>
        public int Length => Vertical ? ScrollGroup.Height : ScrollGroup.Width;

        /// <summary>The length of the scroll bar.</summary>
        public int BarLength => MaxValue > 0 ? (int)((double)Length / (MaxValue + Length) * Length) : 0;

        /// <summary>The scroll bar's position offset.</summary>
        public int BarPosition => MaxValue > 0 ? (int)((Length - BarLength) * ((double)Value / MaxValue)) : 0;

        public void AddScrollBar(UIStyling styling, int width, UIAnchor anchor = null)
        {
            if ((Vertical && anchor?.AlignmentY == UIAlignment.CENTER) || (!Vertical && anchor?.AlignmentX == UIAlignment.CENTER))
            {
                throw new Exception($"Tried to add a scroll bar with a central scroll direction: {anchor}");
            }
            if (ScrollBar is not null)
            {
                throw new Exception("TODO");
            }
            anchor ??= Vertical ? UIAnchor.TOP_RIGHT : UIAnchor.BOTTOM_LEFT;
            UILayout layout = new UILayout().SetAnchor(anchor);
            // TODO: this seems like a good "transpose" opportunity
            if (Vertical)
            {
                layout.SetY(() => BarPosition).SetHeight(() => BarLength).SetWidth(() => (int)(width * ScrollBar.Scale));
            }
            else
            {
                layout.SetX(() => BarPosition).SetWidth(() => BarLength).SetHeight(() => (int)(width * ScrollBar.Scale));
            }
            ScrollBar = new(styling, layout) { ScaleSize = false };
            ScrollGroup.ScrollBarLayer.AddChild(ScrollBar);
        }

        public void RemoveScrollBar()
        {
            ScrollGroup.ScrollBarLayer.RemoveChild(ScrollBar);
            ScrollBar = null;
        }

        // TODO: why is this needed??
        /// <summary>Sets the <see cref="Value"/> and <see cref="MaxValue"/> to 0.</summary>
        public void Reset()
        {
            Value = 0;
            MaxValue = 0;
        }

        /// <summary>Clamps the <see cref="Value"/> between <c>0</c> and <see cref="MaxValue"/>.</summary>
        public void Clamp()
        {
            if (MaxValue >= 0)
            {
                Value = Math.Clamp(Value, 0, MaxValue);
            }
        }

        // TODO: what does this do??
        /// <summary>Scrolls to encompass a min/max offset pair.</summary>
        /// <param name="min">The min offset.</param>
        /// <param name="max">The max offset.</param>
        public void ScrollToPos(int min, int max)
        {
            if (min < Value)
            {
                Value = min;
            }
            else if (max > Length)
            {
                Value += max - Length;
            }
        }

        /// <summary>Ticks the mouse dragging the <see cref="ScrollBar"/>.</summary>
        /// <param name="mousePos">The relevant mouse position.</param>
        /// <param name="groupPos">The relevant scroll group position.</param>
        public void TickMouseDrag(float mousePos, int groupPos)
        {
            if (!ScrollBar.IsPressed)
            {
                BarHeldOffset = -1;
                return;
            }
            if (BarHeldOffset == -1)
            {
                BarHeldOffset = (int)mousePos - (Vertical ? ScrollBar.Y : ScrollBar.X);
            }
            Value = (int)((double)(mousePos - groupPos - BarHeldOffset) / (Length - BarLength) * MaxValue);
            Clamp();
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

        /// <inheritdoc/>
        public override string ToString() => $"(Value: {Value}, Max: {MaxValue}, Speed: {ScrollSpeed}u/tick)";
    }
}
