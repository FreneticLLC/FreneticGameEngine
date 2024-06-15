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

namespace FGEGraphics.UISystem;

/// <summary>Represents an expandable list container of UI elements.</summary>
/// <param name="spacing">The spacing between each child.</param>
/// <param name="pos">The position of the element.</param>
// TODO: Support all UI directions
public class UIListGroup(int spacing, UIPositionHelper pos) : UIGroup(pos.ConstantWidthHeight(0, 0))
{
    /// <summary>The spacing between each child.</summary>
    public int Spacing = spacing;

    /// <inheritdoc/>
    public override void AddChild(UIElement child, bool priority = true)
    {
        base.AddChild(child, priority);
        child.Position.Anchor(UIAnchor.TOP_CENTER).ConstantXY(0, Position.Height > 0 ? Position.Height + Spacing : 0);
        Position.ConstantHeight(child.Position.Y + child.Position.Height);
    }
}
