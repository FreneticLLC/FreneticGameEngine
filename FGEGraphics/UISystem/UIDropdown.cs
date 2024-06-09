//
// This file is part of the Frenetic Game Engine, created by Frenetic LLC.
// This code is Copyright (C) Frenetic LLC under the terms of a strict license.
// See README.md or LICENSE.txt in the FreneticGameEngine source root for the contents of the license.
// If neither of these are available, assume that neither you nor anyone other than the copyright holder
// hold any right or permission to use this software until such time as the official license is identified.
//

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FGEGraphics.UISystem;

public class UIDropdown : UIElement
{
    public Dictionary<UIClickableElement, string> Choices = [];

    public UIButton Button;
    public UIListGroup List;
    public UIBox Box;

    public UIClickableElement SelectedElement;
    public string SelectedValue;

    public UIDropdown(string text, int maxHeight, UIClickableElement.StyleGroup buttonStyles, UIElementStyle boxStyle, UIPositionHelper pos, bool shouldRender = true, bool enabled = true) : base(pos, shouldRender, enabled)
    {
        AddChild(Button = new UIButton(text, HandleOpen, buttonStyles, pos.AtOrigin()));
        Box = new UIBox(boxStyle, pos.AtOrigin());
        Box.AddChild(List = new(new UIPositionHelper(pos.View), 10));
        Box.Position.GetterHeight(() => Math.Min(List.Position.Height, maxHeight));
    }

    public void HandleOpen()
    {
        RemoveChild(Button);
        AddChild(Box);
    }

    public void HandleSelect(UIClickableElement element)
    {
        SelectedElement = element;
        SelectedValue = Choices[element];
        RemoveChild(Box);
        AddChild(Button);
        Button.Text.Content = SelectedValue;
    }

    public void AddChoice(string choice, UIClickableElement element)
    {
        List.AddChild(element);
        Choices[element] = choice;
        element.Clicked += () => HandleSelect(element);
    }

    public void AddTextLinkChoice(string choice, UIClickableElement.StyleGroup linkStyles)
    {
        UITextLink link = new(choice, null, null, linkStyles, new UIPositionHelper(Position.View));
        AddChoice(choice, link);
    }
}
