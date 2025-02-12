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

/// <summary>
/// Represents an expandable list container of UI elements.
/// List expansion is in the opposite direction of the supplied <see cref="UIAnchor"/>. 
/// </summary>
// TODO: Getter positions instead of constants
public class UIListGroup : UIGroup
{
    /// <summary>Whether the list should expand vertically.</summary>
    public bool Vertical;

    /// <summary>The spacing between each child.</summary>
    public int Spacing;

    /// <summary>The anchor that the list will expand from.</summary>
    public UIAnchor Anchor;

    /// <summary>Constructs a new list group.</summary>
    /// <param name="spacing">The spacing between each child.</param>
    /// <param name="pos">The position of the element.</param>
    /// <param name="vertical">Whether the list should expand vertically.</param>
    /// <param name="anchor">The anchor the list will expand from. If <c>null</c>, defaults to the <paramref name="pos"/> anchor.</param>
    public UIListGroup(int spacing, UIPositionHelper pos, bool vertical = true, UIAnchor anchor = null) : base(pos.ConstantWidthHeight(0, 0))
    {
        Anchor = anchor ?? Position.MainAnchor;
        if ((vertical && Anchor.AlignmentY == UIAlignment.CENTER) || (!vertical && Anchor.AlignmentX == UIAlignment.CENTER))
        {
            throw new Exception("UIListGroup must have a non-central expansion direction");
        }
        Vertical = vertical;
        Spacing = spacing;
    }

    /// <summary>Adds and positions an element within the list.</summary>
    /// <param name="element">The element to add.</param>
    /// <param name="addChild">Whether to add <paramref name="element"/> as a child.</param>
    public void AddListItem(UIElement element, bool addChild = true)
    {
        if (addChild)
        {
            base.AddChild(element);
        }
        element.Position.Anchor(Anchor);
        if (Vertical)
        {
            int elementY = Position.Height > 0 ? Position.Height + Spacing : 0;
            element.Position.ConstantXY(0, Anchor.AlignmentY == UIAlignment.TOP ? elementY : -elementY);
            Position.ConstantHeight(elementY + element.Position.Height);
            if (element.Position.Width > Position.Width)
            {
                Position.ConstantWidth(element.Position.Width);
            }
        }
        else
        {
            int elementX = Position.Width > 0 ? Position.Width + Spacing : 0;
            element.Position.ConstantXY(Anchor.AlignmentX == UIAlignment.LEFT ? elementX : -elementX, 0);
            Position.ConstantWidth(elementX + element.Position.Width);
            if (element.Position.Height > Position.Height)
            {
                Position.ConstantHeight(element.Position.Height);
            }
        }
    }
}
