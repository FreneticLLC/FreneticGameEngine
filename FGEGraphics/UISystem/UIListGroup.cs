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
public class UIListGroup : UIGroup
{
    public bool Vertical;

    /// <summary>The spacing between each child.</summary>
    public int Spacing;

    /// <summary>Constructs a new list group.</summary>
    /// <param name="spacing">The spacing between each child.</param>
    /// <param name="pos">The position of the element.</param>
    public UIListGroup(bool vertical, int spacing, UIPositionHelper pos) : base(pos.ConstantWidthHeight(0, 0))
    {
        if ((vertical && Position.MainAnchor.VerticalAlignment == UIAlignment.CENTER) || (!vertical && Position.MainAnchor.HorizontalAlignment == UIAlignment.CENTER))
        {
            throw new Exception("UIListGroup must have a non-central expansion direction");
        }
        Vertical = vertical;
        Spacing = spacing;
    }

    /// <inheritdoc/>
    public override void AddChild(UIElement child, bool priority = true)
    {
        base.AddChild(child, priority);
        child.Position.Anchor(Position.MainAnchor);
        if (Vertical)
        {
            int maxY = Position.Height > 0 ? Position.Height + Spacing : 0;
            if (Position.MainAnchor.VerticalAlignment == UIAlignment.BOTTOM)
            {
                maxY = -maxY;
            }
            child.Position.ConstantXY(0, maxY);
            Position.ConstantHeight(maxY + child.Position.Height);
            if (child.Position.Width > Position.Width)
            {
                Position.ConstantWidth(child.Position.Width);
            }
        }
        else
        {
            int maxX = Position.Width > 0 ? Position.Width + Spacing : 0;
            if (Position.MainAnchor.HorizontalAlignment == UIAlignment.RIGHT)
            {
                maxX = -maxX;
            }
            child.Position.ConstantXY(maxX, 0);
            Position.ConstantWidth(maxX + child.Position.Width);
            if (child.Position.Height > Position.Height)
            {
                Position.ConstantHeight(child.Position.Height);
            }
        }
    }
}
