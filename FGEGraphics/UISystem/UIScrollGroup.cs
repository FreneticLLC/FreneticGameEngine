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
using OpenTK.Mathematics;

namespace FGEGraphics.UISystem;

/// <summary>Represents a scrollable box containing other elements.</summary>
// TODO: handle navigational scroll
public class UIScrollGroup : UIScissorGroup
{
    /// <summary>The current scroll position.</summary>
    public int Value = 0;

    /// <summary>An upper limit on how far the group can be scrolled. -1 for unlimited scrolling, 0 for no scrolling.</summary>
    public int MaxValue = -1;

    /// <summary>How fast the group can be scrolled (in position units per scroll tick).</summary>
    public int ScrollSpeed = 10;

    /// <summary>The scroll bar button.</summary>
    public UIButton ScrollBar;

    /// <summary>Data internal to a <see cref="UIScrollGroup"/> instance.</summary>
    public InternalData Internal = new();

    /// <summary>The height of the <see cref="ScrollBar"/>.</summary>
    public int ScrollBarHeight => MaxValue > 0 ? (int)((double)Position.Height / (MaxValue + Position.Height) * Position.Height) : 0;

    /// <summary>Data internal to a <see cref="UIScrollGroup"/> instance.</summary>
    public struct InternalData()
    {
        /// <summary>The held Y offset of the vertical scroll bar.</summary>
        public int HeldY = -1;
    }

    /// <summary>Constructs the UI scroll group.</summary>
    /// <param name="pos">The position of the element.</param>
    /// <param name="barStyles">The <see cref="ScrollBar"/> styles.</param>
    /// <param name="barWidth">The width of the <see cref="ScrollBar"/>.</param>
    public UIScrollGroup(UIPositionHelper pos, UIClickableElement.StyleGroup barStyles = null, int barWidth = 0) : base(pos)
    {
        if (barStyles is not null && barWidth > 0)
        {
            base.AddChild(ScrollBar = new(null, null, barStyles, new UIPositionHelper(pos.View).Anchor(UIAnchor.TOP_RIGHT).ConstantWidth(barWidth)
                .GetterHeight(() => ScrollBarHeight)
                .GetterY(() => MaxValue > 0 ? (int) ((pos.Height - ScrollBarHeight) * ((double) Value / MaxValue)) : 0)));
        }
    }

    /// <inheritdoc/>
    public override void AddChild(UIElement child, bool priority = true)
    {
        UIPositionHelper original = new(child.Position);
        child.Position.GetterY(() => original.Internal.Y.Get() - Value);
        base.AddChild(child, priority);
    }

    /// <summary>Ticks the mouse dragging the scroll bar.</summary>
    public void TickMouseDrag()
    {
        if (!ScrollBar.Pressed)
        {
            Internal.HeldY = -1;
            return;
        }
        if (Internal.HeldY == -1)
        {
            Internal.HeldY = (int)Window.MouseY - ScrollBar.Y;
        }
        Value = (int)((double)(Window.MouseY - Y - Internal.HeldY) / (Position.Height - ScrollBarHeight) * MaxValue);
        Value = Math.Clamp(Value, 0, MaxValue);
    }

    /// <summary>Ticks the scroll wheel and modifies the scroll value.</summary>
    // TODO: Handle horizontal scroll
    public void TickMouseScroll()
    {
        Value -= (int)Window.CurrentMouse.ScrollDelta.Y * ScrollSpeed;
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
    public override void Tick(double delta)
    {
        base.Tick(delta);
        if (ScrollBar is not null)
        {
            TickMouseDrag();
        }
        if (ElementInternal.HoverInternal && (!ScrollBar?.Pressed ?? true))
        {
            TickMouseScroll();
        }
    }

    /// <inheritdoc/>
    public override List<string> GetDebugInfo()
    {
        List<string> info = base.GetDebugInfo();
        info.Add($"^7Scroll: ^3{Value}");
        return info;
    }
}
