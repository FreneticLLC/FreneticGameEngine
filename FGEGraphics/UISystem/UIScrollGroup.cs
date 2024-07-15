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
using FGECore.CoreSystems;
using FGECore.MathHelpers;
using FGEGraphics.ClientSystem;

namespace FGEGraphics.UISystem;

/// <summary>Represents a scrollable box containing other elements.</summary>
// TODO: handle navigational scroll
public class UIScrollGroup : UIScissorGroup
{
    /// <summary>The current scroll position.</summary>
    public Vector2i Values = Vector2i.Zero;

    /// <summary>An upper limit on how far the group can be scrolled. -1 for unlimited scrolling, 0 for no scrolling.</summary>
    public Vector2i MaxValues = new(-1, -1);

    /// <summary>How fast the group can be scrolled (in position units per scroll tick).</summary>
    public Vector2i ScrollSpeeds = new(10, 10);

    /// <summary>The vertical scroll bar button.</summary>
    public UIButton VerticalScrollBar;

    /// <summary>The horizontal scroll bar button.</summary>
    public UIButton HorizontalScrollBar;

    /// <summary>Data internal to a <see cref="UIScrollGroup"/> instance.</summary>
    public InternalData Internal = new();

    /// <summary>Data internal to a <see cref="UIScrollGroup"/> instance.</summary>
    public struct InternalData()
    {
        /// <summary>The held Y offset of the vertical scroll bar.</summary>
        public int HeldY = -1;

        /// <summary>The held X offset of the horizontal scroll bar.</summary>
        public int HeldX = -1;
    }

    /// <summary>Constructs the UI scroll group.</summary>
    /// <param name="pos">The position of the element.</param>
    /// <param name="barStyles">The <see cref="VerticalScrollBar"/> styles.</param>
    /// <param name="barWidth">The width of the <see cref="VerticalScrollBar"/>.</param>
    public UIScrollGroup(UIPositionHelper pos, UIClickableElement.StyleGroup barStyles = null, int barWidth = 0, bool verticalBar = false, bool horizontalBar = false) : base(pos)
    {
        if (barStyles is null || barWidth == 0)
        {
            return;
        }
        if (verticalBar)
        {
            base.AddChild(VerticalScrollBar = new(null, null, barStyles, new UIPositionHelper(pos.View).Anchor(UIAnchor.TOP_RIGHT).ConstantWidth(barWidth)
                .GetterHeight(() => GetScrollBarSize(true))
                .GetterY(() => MaxValues.Y > 0 ? (int)((pos.Height - GetScrollBarSize(true)) * ((double)Values.Y / MaxValues.Y)) : 0)));
        }
        if (horizontalBar)
        {
            base.AddChild(HorizontalScrollBar = new(null, null, barStyles, new UIPositionHelper(pos.View).Anchor(UIAnchor.BOTTOM_LEFT).ConstantHeight(barWidth)
                .GetterWidth(() => GetScrollBarSize(false))
                .GetterX(() => MaxValues.X > 0 ? (int)((pos.Width - GetScrollBarSize(false)) * ((double)Values.X / MaxValues.X)) : 0)));
        }
    }

    /// <summary>Returns the size of the vertical/horizontal scroll bar.</summary>
    /// <param name="vertical">True for the vertical scroll bar, false for the horizontal scroll bar.</param>
    public int GetScrollBarSize(bool vertical)
    {
        int maxValue = vertical ? MaxValues.Y : MaxValues.X;
        int groupSize = vertical ? Position.Height : Position.Width;
        return maxValue > 0 ? (int)((double)groupSize / (maxValue + groupSize) * groupSize) : 0;
    }

    /// <inheritdoc/>
    public override void AddChild(UIElement child, bool priority = true)
    {
        UIPositionHelper original = new(child.Position);
        child.Position.GetterXY(() => original.Internal.X.Get() - Values.X, () => original.Internal.Y.Get() - Values.Y);
        base.AddChild(child, priority);
    }

    /// <summary>Ticks the mouse dragging the scroll bar.</summary>
    public void TickMouseDrag()
    {
        if (!VerticalScrollBar.Pressed)
        {
            Internal.HeldY = -1;
        }
        else
        {
            if (Internal.HeldY == -1)
            {
                Internal.HeldY = (int)Window.MouseY - VerticalScrollBar.Y;
            }
            Values.Y = (int)((double)(Window.MouseY - Y - Internal.HeldY) / (Position.Height - GetScrollBarSize(true)) * MaxValues.Y);
            Values.Y = Math.Clamp(Values.Y, 0, MaxValues.Y);
        }
        if (!HorizontalScrollBar.Pressed)
        {
            Internal.HeldX = -1;
        }
        else
        {
            if (Internal.HeldX == -1)
            {
                Internal.HeldX = (int)Window.MouseX - HorizontalScrollBar.X;
            }
            Values.X = (int)((double)(Window.MouseX - X - Internal.HeldX) / (Position.Height - GetScrollBarSize(false)) * MaxValues.X);
            Values.X = Math.Clamp(Values.X, 0, MaxValues.X);
        }
    }

    /// <summary>Ticks the scroll wheel and modifies the scroll value.</summary>
    // TODO: Handle horizontal scroll
    public void TickMouseScroll()
    {
        Values.Y -= (int)Window.CurrentMouse.ScrollDelta.Y * ScrollSpeeds.Y;
        if (Values.Y < 0)
        {
            Values.Y = 0;
        }
        if (MaxValues.Y != -1 && Values.Y > MaxValues.Y)
        {
            Values.Y = MaxValues.Y;
        }
        Values.X -= (int)Window.CurrentMouse.ScrollDelta.X * ScrollSpeeds.X;
        if (Values.X < 0)
        {
            Values.X = 0;
        }
        if (MaxValues.X != -1 && Values.X > MaxValues.X)
        {
            Values.X = MaxValues.X;
        }
    }

    /// <inheritdoc/>
    public override void Tick(double delta)
    {
        base.Tick(delta);
        if (VerticalScrollBar is not null)
        {
            TickMouseDrag();
        }
        if (ElementInternal.HoverInternal && (!VerticalScrollBar?.Pressed ?? true))
        {
            TickMouseScroll();
        }
    }

    /// <inheritdoc/>
    public override List<string> GetDebugInfo()
    {
        List<string> info = base.GetDebugInfo();
        info.Add($"^7Scroll: ^3{Values}");
        return info;
    }
}
