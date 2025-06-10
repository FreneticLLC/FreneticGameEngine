//
// This file is part of the Frenetic Game Engine, created by Frenetic LLC.
// This code is Copyright (C) Frenetic LLC under the terms of a strict license.
// See README.md or LICENSE.txt in the FreneticGameEngine source root for the contents of the license.
// If neither of these are available, assume that neither you nor anyone other than the copyright holder
// hold any right or permission to use this software until such time as the official license is identified.
//

using FGECore.MathHelpers;
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

    public struct InternalData()
    {
        public List<UIElement> Items = [];
        public Dictionary<UIElement, int> Offsets = [];
    }

    public InternalData Internal = new();

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
        if (Vertical)
        {
            Layout.SetHeight(() => Internal.Items.Count > 0 ? Internal.Offsets[Internal.Items[^1]] + Internal.Items[^1].Height : 0);
            Layout.SetWidth(() => Internal.Items.Count > 0 ? Internal.Items.Max(item => item.Width) : 0);
        }
        else
        {
            Layout.SetWidth(() => Internal.Items.Count > 0 ? Internal.Offsets[Internal.Items[^1]] + Internal.Items[^1].Width : 0);
            Layout.SetHeight(() => Internal.Items.Count > 0 ? Internal.Items.Max(item => item.Height) : 0);
        }
    }

    /// <summary>Adds and positions an element within the list.</summary>
    /// <param name="item">The element to add.</param>
    /// <param name="addChild">Whether to add <paramref name="item"/> as a child.</param>
    public void AddListItem(UIElement item, bool addChild = true)
    {
        if (addChild)
        {
            base.AddChild(item);
        }
        item.Layout.SetAnchor(Anchor);
        Internal.Offsets[item] = Internal.Items.Count > 0 ? Internal.Offsets[Internal.Items[^1]] + (Vertical ? Internal.Items[^1].Height : Internal.Items[^1].Width) + Spacing : 0;
        Internal.Items.Add(item);
        if (Vertical)
        {
            item.Layout.SetY(() => Anchor.AlignmentY == UIAlignment.TOP ? Internal.Offsets[item] : -Internal.Offsets[item]).SetX(0);
        }
        else
        {
            item.Layout.SetX(() => Anchor.AlignmentX == UIAlignment.LEFT ? Internal.Offsets[item] : -Internal.Offsets[item]).SetY(0);
        }
        item.OnSizeChange += (oldSize, newSize) =>
        {
            int listIndex = Internal.Items.IndexOf(item);
            if (listIndex < 0)
            {
                return;
            }
            for (int i = listIndex + 1; i < Internal.Items.Count; i++)
            {
                Vector2i difference = newSize - oldSize;
                Internal.Offsets[Internal.Items[i]] += Vertical ? difference.Y : difference.X;
            }
        };
    }

    public void RemoveListItem(UIElement item)
    {
        if (!Internal.Items.Contains(item))
        {
            throw new Exception("Tried to remove an item that does not belong to this list!");
        }
        Internal.Items.Remove(item);
        Internal.Offsets.Remove(item);
    }

    public override void RemoveChild(UIElement child)
    {
        if (Internal.Items.Contains(child))
        {
            RemoveListItem(child);
        }
        base.RemoveChild(child);
    }
}
