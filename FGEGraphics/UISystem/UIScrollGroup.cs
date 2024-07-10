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
/// <remarks>Constructs the UI scroll box.</remarks>
/// <param name="pos">The position of the element.</param>
// TODO: handle navigational scroll
public class UIScrollGroup(UIPositionHelper pos) : UIScissorGroup(pos)
{
    /// <summary>The current scroll position.</summary>
    public int Scroll = 0;

    /// <summary>An upper limit on how far the scroll box can be scrolled. 0 for unlimited scrolling.</summary>
    public int MaxScroll = 0;

    /// <inheritdoc/>
    public override void AddChild(UIElement child, bool priority = true)
    {
        UIPositionHelper original = new(child.Position);
        child.Position.GetterY(() => original.Internal.Y.Get() - Scroll);
        base.AddChild(child, priority);
    }

    /// <summary>Checks the mouse scroll wheel if necessary and changes the scroll position.</summary>
    /// <param name="delta">The time since the last tick.</param>
    // TODO: Handle horizontal scroll
    public override void Tick(double delta)
    {
        if (!ElementInternal.HoverInternal)
        {
            return;
        }
        Scroll -= (int)Window.CurrentMouse.ScrollDelta.Y * 10;
        if (Scroll < 0)
        {
            Scroll = 0;
        }
        if (MaxScroll != 0 && Scroll > MaxScroll)
        {
            Scroll = MaxScroll;
        }
    }

    /// <inheritdoc/>
    public override List<string> GetDebugInfo()
    {
        List<string> info = base.GetDebugInfo();
        info.Add($"^7Scroll: ^3{Scroll}");
        return info;
    }
}
