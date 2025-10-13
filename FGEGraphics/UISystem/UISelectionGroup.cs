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

/// <summary>Represents a container of <see cref="UIElement"/>s that can be configurably selected.</summary>
/// <param name="layout">The layout of the element.</param>
public class UISelectionGroup(UILayout layout) : UIGroup(layout)
{
    /// <summary>The minimum allowed number of selected elements, or <c>-1</c> for no lower bound.</summary>
    public int MinSelections = -1;

    /// <summary>The maximum allowed number of selected elements, or <c>-1</c> for no upper bound.</summary>
    public int MaxSelections
    {
        get => Internal.MaxSelections;
        set
        {
            Internal.MaxSelections = value;
            FlushSelections();
            UpdateLocks();
        }
    }

    /// <summary>Whether the oldest selected element should be dropped when the user selects a new element exceeding the <see cref="MaxSelections"/>.</summary>
    public bool IsCyclic = false;

    /// <summary>Whether non-selected elements are locked and cannot be selected.</summary>
    public bool IsLocked = false;

    /// <summary>The selectable elements managed by this group.</summary>
    public List<UIElement> Elements = [];

    /// <summary>The list of currently selected elements.</summary>
    public List<UIElement> SelectedElements = [];

    /// <summary>
    /// Fired when the user selects a new element. 
    /// <para>The second argument is the new element. The first argument is the dropped selection when <see cref="IsCyclic"/> is <c>true</c> and <see cref="MaxSelections"/> was exceeded, otherwise <c>null</c>.</para>
    /// </summary>
    public Action<UIElement, UIElement> OnSelectElement;

    /// <summary>Data internal to a <see cref="UISelectionGroup"/> instance.</summary>
    public struct InternalData()
    {
        /// <summary>The maximum allowed number of selected elements.</summary>
        public int MaxSelections = -1;

        /// <summary>Maps elements to their selection logic.</summary>
        public Dictionary<UIElement, Action> Updaters = [];
    }

    /// <summary>Data internal to a <see cref="UISelectionGroup"/> instance.</summary>
    public InternalData Internal = new();

    /// <summary>Removes old selections such that <see cref="SelectedElements"/> is bound by <see cref="MaxSelections"/> if needed.</summary>
    /// <returns>The last selection to be removed, or <c>null</c> if nothing was deselected.</returns>
    public UIElement FlushSelections()
    {
        if (MaxSelections <= 0 || SelectedElements.Count <= MaxSelections)
        {
            return null;
        }
        if (SelectedElements.Count > MaxSelections + 1)
        {
            SelectedElements.RemoveRange(0, SelectedElements.Count - MaxSelections - 1);
        }
        UIElement first = SelectedElements[0];
        DeselectElement(first);
        return first;
    }

    /// <summary>Locks or unlocks an element from being interacted with.</summary>
    /// <param name="element">The element to modify.</param>
    /// <param name="locked">Whether the element should be locked.</param>
    public void SetLocked(UIElement element, bool locked)
    {
        element.IsEnabled = !locked;
        if (locked)
        {
            element.IsHovered = false;
            element.IsPressed = false;
        }
    }

    /// <summary>Locks or unlocks non-selected elements from being interacted with.</summary>
    /// <param name="locked">Whether the elements should be locked.</param>
    public void SetLocked(bool locked)
    {
        foreach (UIElement element in Elements)
        {
            if (!SelectedElements.Contains(element))
            {
                SetLocked(element, locked);
            }
        }
        IsLocked = locked;
    }

    /// <summary>If <see cref="IsCyclic"/> is <c>false</c>, locks or unlocks this group to be bound to <see cref="MaxSelections"/>.</summary>
    /// <returns>Whether the lock was updated.</returns>
    public bool UpdateLocks()
    {
        if (IsCyclic || MaxSelections <= 0)
        {
            return false;
        }
        if (IsLocked && SelectedElements.Count < MaxSelections)
        {
            SetLocked(false);
            return true;
        }
        else if (!IsLocked && SelectedElements.Count >= MaxSelections)
        {
            SetLocked(true);
            return true;
        }
        return false;
    }

    /// <summary>Selects an element and updates the group state.</summary>
    /// <param name="element">The element to select.</param>
    public void SelectElement(UIElement element)
    {
        if (SelectedElements.Contains(element))
        {
            return;
        }
        element.IsPressed = true;
        element.IsStateLocked = true;
        SelectedElements.Add(element);
        UIElement last = IsCyclic ? FlushSelections() : null;
        if (last is null)
        {
            UpdateLocks();
        }
        OnSelectElement?.Invoke(last, element);
    }

    /// <summary>Deselects an element and updates the group state.</summary>
    /// <param name="element">The element to deselect.</param>
    public void DeselectElement(UIElement element)
    {
        if (!SelectedElements.Contains(element))
        {
            return;
        }
        SelectedElements.Remove(element);
        element.IsHovered = false;
        element.IsPressed = false;
        element.IsStateLocked = false;
        if (!UpdateLocks() && IsLocked)
        {
            SetLocked(element, true);
        }
    }

    /// <summary>Deselects all elements and updates the group state.</summary>
    public void DeselectAll()
    {
        foreach (UIElement selectedElement in SelectedElements)
        {
            selectedElement.IsHovered = false;
            selectedElement.IsPressed = false;
            selectedElement.IsStateLocked = false;
        }
        SelectedElements.Clear();
        UpdateLocks();
    }

    /// <summary>Adds a selectable element to this group.</summary>
    /// <param name="element">The element to add.</param>
    /// <param name="selected">Whether the element should already be selected.</param>
    /// <param name="addChild">Whether to add the element as a child.</param>
    public void AddElement(UIElement element, bool selected = false, bool addChild = true)
    {
        if (Elements.Contains(element))
        {
            return;
        }
        if (selected)
        {
            SelectElement(element);
        }
        Internal.Updaters[element] = element.OnClick += () =>
        {
            if (SelectedElements.Contains(element))
            {
                if (SelectedElements.Count > MinSelections)
                {
                    DeselectElement(element);
                }
            }
            else
            {
                SelectElement(element);
            }
        };
        Elements.Add(element);
        if (IsLocked && !selected)
        {
            SetLocked(element, true);
        }
        if (addChild)
        {
            AddChild(element);
        }
    }

    /// <summary>Removes a selectable element from this group.</summary>
    /// <param name="element">The element to remove.</param>
    /// <param name="removeChild">Whether to remove the element as a child.</param>
    public void RemoveElement(UIElement element, bool removeChild = true)
    {
        element.OnClick -= Internal.Updaters[element];
        Elements.Remove(element);
        if (SelectedElements.Contains(element))
        {
            DeselectElement(element);
        }
        UpdateLocks();
        if (removeChild)
        {
            RemoveChild(element);
        }
    }

    /// <summary>Constructs a selection group for tabbed screens.</summary>
    /// <param name="content">The group to display the selected screen in.</param>
    /// <param name="tabs">The list of tab buttons.</param>
    /// <param name="tabFactory">A function that takes a screen and returns its tab button.</param>
    /// <param name="layout">The layout of the element.</param>
    /// <returns>A tuple of the constructed selection group and a function to add screens to the tab list.</returns>
    public static (UISelectionGroup, Action<UIScreen, bool>) WithTabs(UIGroup content, UIListGroup tabs, Func<UIScreen, UIElement> tabFactory, UILayout layout)
    {
        UISelectionGroup result = new(layout)
        {
            MinSelections = 1,
            MaxSelections = 1,
            IsCyclic = true,
            OnSelectElement = (from, to) =>
            {
                if (from is not null)
                {
                    content.RemoveChild((UIScreen)from.Tag);
                }
                content.AddChild((UIScreen)to.Tag);
            }
        };
        return (result, (screen, main) =>
        {
            UIElement tab = tabFactory(screen);
            tab.Tag = screen;
            tabs.AddListItem(tab);
            result.AddElement(tab, selected: main, addChild: false);
        });
    }
}
