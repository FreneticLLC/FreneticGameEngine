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
using OpenTK.Mathematics;

namespace FGEGraphics.UISystem;

/// <summary>Represents a scrollable box containing other elements.</summary>
// TODO: handle navigational scroll
public class UIScrollGroup : UIScissorGroup
{
    /// <summary>The current scroll position.</summary>
    public int Value = 0;

    /// <summary>An upper limit on how far the group can be scrolled. 0 for unlimited scrolling.</summary>
    public int MaxValue = 0;

    /// <summary>How fast the group can be scrolled (in position units per scroll tick).</summary>
    public int ScrollSpeed = 10;

    /// <summary>The scroll bar box.</summary>
    public UIBox ScrollBar;

    /// <summary>Constructs the UI scroll group.</summary>
    /// <param name="pos">The position of the element.</param>
    /// <param name="barStyle">The <see cref="ScrollBar"/> style.</param>
    /// <param name="barWidth">The width of the <see cref="ScrollBar"/>.</param>
    public UIScrollGroup(UIPositionHelper pos, UIElementStyle barStyle = null, int barWidth = 0) : base(pos)
    {
        if (barStyle is not null && barWidth > 0)
        {
            int BarHeight() => (int)((double)pos.Height / (MaxValue + pos.Height) * pos.Height);
            base.AddChild(ScrollBar = new(barStyle, new UIPositionHelper(pos.View).Anchor(UIAnchor.TOP_RIGHT).ConstantWidth(barWidth)
                .GetterHeight(BarHeight)
                .GetterY(() => (int) ((pos.Height - BarHeight()) * ((double) Value / MaxValue)))));
        }
    }

    /// <inheritdoc/>
    public override void AddChild(UIElement child, bool priority = true)
    {
        UIPositionHelper original = new(child.Position);
        child.Position.GetterY(() => original.Internal.Y.Get() - Value);
        base.AddChild(child, priority);
    }

    /// <summary>Checks the mouse scroll wheel if necessary and changes the scroll position.</summary>
    /// <param name="delta">The time since the last tick.</param>
    // TODO: Handle horizontal scroll
    // TODO: Click/hold scroll bar
    public override void Tick(double delta)
    {
        if (!ElementInternal.HoverInternal)
        {
            return;
        }
        Value -= (int)Window.CurrentMouse.ScrollDelta.Y * ScrollSpeed;
        if (Value < 0)
        {
            Value = 0;
        }
        if (MaxValue != 0 && Value > MaxValue)
        {
            Value = MaxValue;
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
