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

/// <summary>Represents an interactable dropdown of different choices.</summary>
// TODO: Choice search
// TODO: Arrow key + ENTER navigation
// TODO: Scroll if exceed max width
// TODO: Dropdown icon next to placeholder info?
// TODO: Different style for placeholder info
public class UIDropdown : UIElement
{
    /// <summary>A mapping of <see cref="ListGroup"/> clickables to their string values.</summary>
    public Dictionary<UIClickableElement, string> Choices = [];

    /// <summary>The text to display when no choice is selected.</summary>
    public string PlaceholderInfo;

    /// <summary>The button to open the dropdown.</summary>
    public UIButton Button;

    /// <summary>The dropdown list of choices.</summary>
    public UIListGroup ListGroup;

    /// <summary>The box container surrounding the <see cref="ListGroup"/>.</summary>
    public UIBox Box;

    /// <summary>The currently selected entry in the <see cref="ListGroup"/>.</summary>
    public UIClickableElement SelectedElement;

    /// <summary>The currently selected dropdown value.</summary>
    public string SelectedValue;

    /// <summary>Fired when a choice is selected.</summary>
    public Action<string> OnChoiceSelect;

    /// <summary>Constructs a new UI dropdown.</summary>
    /// <param name="text">The text to display when no choice is selected.</param>
    /// <param name="boxPadding">The padding between the <see cref="Box"/> and <see cref="ListGroup"/> entries.</param>
    /// <param name="listSpacing">The spacing betwene <see cref="ListGroup"/> entries.</param>
    /// <param name="buttonStyles">The <see cref="Button"/> element styles.</param>
    /// <param name="boxStyle">The <see cref="Box"/> element styles.</param>
    /// <param name="layout">The layout of the element.</param>
    public UIDropdown(string text, int boxPadding, int listSpacing, UIClickableElement.StyleGroup buttonStyles, UIStyle boxStyle, UILayout layout) : base(layout)
    {
        PlaceholderInfo = text;
        AddChild(Button = new UIButton(text, HandleOpen, buttonStyles, layout.AtOrigin()));
        Box = new UIBox(boxStyle, layout.AtOrigin());
        Box.AddChild(ListGroup = new(listSpacing, new UILayout().SetAnchor(UIAnchor.TOP_CENTER).SetPosition(0, boxPadding)));
        Box.Layout.SetHeight(() => ListGroup.Layout.Height + boxPadding * 2);
    }

    /// <summary>Opens the dropdown list.</summary>
    public void HandleOpen()
    {
        RemoveChild(Button);
        AddChild(Box);
    }

    /// <summary>Selects one of the choices and closes the dropdown list.</summary>
    /// <param name="element">The selected choice.</param>
    public void HandleSelect(UIClickableElement element)
    {
        SelectedElement = element;
        SelectedValue = Choices[element];
        RemoveChild(Box);
        AddChild(Button);
        Button.Text.Content = SelectedValue;
        OnChoiceSelect?.Invoke(SelectedValue);
    }

    /// <summary>Reverts the dropdown to its pre-chosen state.</summary>
    public void DeselectChoice()
    {
        Button.Text.Content = PlaceholderInfo;
        SelectedElement = null;
        SelectedValue = null;
    }

    /// <summary>Adds a choice to the dropdown.</summary>
    /// <param name="choice">The choice text.</param>
    /// <param name="element">The clickable choice element.</param>
    public void AddChoice(string choice, UIClickableElement element)
    {
        ListGroup.AddListItem(element);
        Choices[element] = choice;
        element.OnClick += () => HandleSelect(element);
    }

    /// <summary>Adds a <see cref="UITextLink"/> as a choice to the dropdown.</summary>
    /// <param name="choice">The choice text.</param>
    /// <param name="linkStyles">The link styles.</param>
    /// <returns>The added text link.</returns>
    public UITextLink AddTextLinkChoice(string choice, UIClickableElement.StyleGroup linkStyles)
    {
        UITextLink link = new(choice, null, null, linkStyles, new UILayout());
        AddChoice(choice, link);
        return link;
    }
}
