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
    /// <param name="layout">The layout of the element.</param>
    /// <param name="vertical">Whether the list should expand vertically.</param>
    /// <param name="anchor">The anchor the list will expand from. If <c>null</c>, defaults to the <paramref name="layout"/> anchor.</param>
    public UIListGroup(int spacing, UILayout layout, bool vertical = true, UIAnchor anchor = null) : base(layout.SetSize(0, 0))
    {
        Anchor = anchor ?? Layout.Anchor;
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
        element.Layout.SetAnchor(Anchor);
        if (Vertical)
        {
            int elementY = Layout.Height > 0 ? Layout.Height + Spacing : 0;
            element.Layout.SetPosition(0, Anchor.AlignmentY == UIAlignment.TOP ? elementY : -elementY);
            Layout.SetHeight(elementY + element.Layout.Height);
            if (element.Layout.Width > Layout.Width)
            {
                Layout.SetWidth(element.Layout.Width);
            }
        }
        else
        {
            int elementX = Layout.Width > 0 ? Layout.Width + Spacing : 0;
            element.Layout.SetPosition(Anchor.AlignmentX == UIAlignment.LEFT ? elementX : -elementX, 0);
            Layout.SetWidth(elementX + element.Layout.Width);
            if (element.Layout.Height > Layout.Height)
            {
                Layout.SetHeight(element.Layout.Height);
            }
        }
    }
}
