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

    public List<UIElement> Items = [];

    public struct InternalData()
    {
        public Dictionary<UIElement, int> Offsets = [];
        public Dictionary<UIElement, Action<Vector2i, Vector2i>> Updaters = [];
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
            Layout.SetHeight(() => Items.Count > 0 ? Internal.Offsets[Items[^1]] + Items[^1].Height : 0);
            Layout.SetWidth(() => Items.Count > 0 ? Items.Max(item => item.Width) : 0);
        }
        else
        {
            Layout.SetWidth(() => Items.Count > 0 ? Internal.Offsets[Items[^1]] + Items[^1].Width : 0);
            Layout.SetHeight(() => Items.Count > 0 ? Items.Max(item => item.Height) : 0);
        }
    }

    /// <summary>Adds and positions an element within the list.</summary>
    /// <param name="item">The element to add.</param>
    /// <param name="addChild">Whether to add <paramref name="item"/> as a child.</param>
    public void AddListItem(UIElement item, int index = -1, bool addChild = true)
    {
        if (addChild)
        {
            base.AddChild(item);
        }
        if (index > Items.Count)
        {
            throw new IndexOutOfRangeException(nameof(index));
        }
        if (index < 0)
        {
            index = Items.Count;
        }
        item.Layout.SetAnchor(Anchor);
        Internal.Offsets[item] = index > 0 ? GetItemOffset(Items[index - 1]) : 0;
        Items.Insert(index, item);
        if (index < Items.Count - 1)
        {
            UpdateOffsets(index, GetItemSize(item));
        }
        if (Vertical)
        {
            item.Layout.SetY(() => Anchor.AlignmentY == UIAlignment.TOP ? Internal.Offsets[item] : -Internal.Offsets[item]).SetX(0);
        }
        else
        {
            item.Layout.SetX(() => Anchor.AlignmentX == UIAlignment.LEFT ? Internal.Offsets[item] : -Internal.Offsets[item]).SetY(0);
        }
        Internal.Updaters[item] = item.OnSizeChange += (oldSize, newSize) =>
        {
            Vector2i difference = newSize - oldSize;
            UpdateOffsets(item, Vertical ? difference.Y : difference.X);
        };
    }

    public int GetItemSize(UIElement item) => (Vertical ? item.Height : item.Width) + Spacing;

    public int GetItemOffset(UIElement lastItem) => Internal.Offsets[lastItem] + GetItemSize(lastItem);

    public void UpdateOffsets(int index, int difference)
    {
        for (int i = index + 1; i < Items.Count; i++)
        {
            Internal.Offsets[Items[i]] += difference;
        }
    }

    public void UpdateOffsets(UIElement item, int difference)
    {
        int index = Items.IndexOf(item);
        if (index >= 0)
        {
            UpdateOffsets(index, difference);
        }
    }

    public void RemoveListItem(UIElement item, bool removeChild = true)
    {
        if (!Items.Contains(item))
        {
            throw new Exception("Tried to remove an item that does not belong to this list!");
        }
        UpdateOffsets(item, -GetItemSize(item));
        item.Layout.SetPosition(0, 0);
        item.OnPositionChange -= Internal.Updaters[item];
        Items.Remove(item);
        Internal.Offsets.Remove(item);
        Internal.Updaters.Remove(item);
        if (removeChild)
        {
            RemoveChild(item);
        }
    }
}
