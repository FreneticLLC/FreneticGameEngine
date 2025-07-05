//
// This file is part of the Frenetic Game Engine, created by Frenetic LLC.
// This code is Copyright (C) Frenetic LLC under the terms of a strict license.
// See README.md or LICENSE.txt in the FreneticGameEngine source root for the contents of the license.
// If neither of these are available, assume that neither you nor anyone other than the copyright holder
// hold any right or permission to use this software until such time as the official license is identified.
//

using FGECore.CoreSystems;
using FGEGraphics.UISystem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class UISelectionGroup(UILayout layout) : UIGroup(layout)
{
    public int MaxSelections = -1;

    public bool IsCyclic = false;

    public bool IsLocked = false;

    public HashSet<UIElement> Elements = [];

    public List<UIElement> SelectedElements = [];

    public Action<UIElement, UIElement> OnSelect;

    public struct InternalData()
    {
        public Dictionary<UIElement, Action> Updaters = [];
    }

    public InternalData Internal = new();

    public UIElement FlushSelections()
    {
        if (MaxSelections <= 0 || SelectedElements.Count < MaxSelections)
        {
            return null;
        }
        if (SelectedElements.Count > MaxSelections)
        {
            SelectedElements.RemoveRange(0, SelectedElements.Count - MaxSelections);
        }
        UIElement first = SelectedElements[0];
        DeselectElement(first);
        return first;
    }

    public void SetLocked(UIElement element, bool locked)
    {
        element.IsEnabled = !locked;
        if (locked)
        {
            element.IsHovered = false;
            element.IsPressed = false;
        }
    }

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

    public void UpdateLock()
    {
        if (IsCyclic || MaxSelections <= 0)
        {
            return;
        }
        if (IsLocked && SelectedElements.Count < MaxSelections)
        {
            SetLocked(false);
        }
        else if (!IsLocked && SelectedElements.Count >= MaxSelections)
        {
            SetLocked(true);
        }
    }

    public void SetMaxSelections(int maxSelections)
    {
        MaxSelections = maxSelections;
        FlushSelections();
        UpdateLock();
    }

    public void SetSelected(UIElement element, bool selected)
    {
        if (!IsCyclic)
        {
            element.IsStateLocked = selected;
        }
        else
        {
            element.IsEnabled = !selected;
        }
    }

    public void SelectElement(UIElement element)
    {
        UIElement last = IsCyclic ? FlushSelections() : null;
        element.IsPressed = true;
        element.IsStateLocked = true;
        SelectedElements.Add(element);
        UpdateLock();
        OnSelect?.Invoke(last, element);
    }

    public void DeselectElement(UIElement element)
    {
        SelectedElements.Remove(element);
        element.IsHovered = false;
        element.IsPressed = false;
        element.IsStateLocked = false;
        UpdateLock();
    }

    public void AddElement(UIElement element, bool selected = false, bool addChild = true)
    {
        if (selected)
        {
            SelectElement(element);
        }
        Internal.Updaters[element] = element.OnClick += () =>
        {
            if (SelectedElements.Contains(element))
            {
                DeselectElement(element);
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

    public void RemoveElement(UIElement element, bool removeChild = true)
    {
        element.OnClick -= Internal.Updaters[element];
        Elements.Remove(element);
        UpdateLock();
        if (removeChild)
        {
            RemoveChild(element);
        }
    }
}
