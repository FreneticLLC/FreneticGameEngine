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
public class UIDropdown : UIElement
{
    /// <summary>A mapping of <see cref="List"/> clickables to their string values.</summary>
    public Dictionary<UIClickableElement, string> Choices = [];

    /// <summary>The text to display when no choice is selected.</summary>
    public string PlaceholderInfo;

    /// <summary>The button to open the dropdown.</summary>
    public UIButton Button;

    /// <summary>The dropdown list of choices.</summary>
    public UIListGroup List;

    /// <summary>The box container surrounding the <see cref="List"/>.</summary>
    public UIBox Box;

    /// <summary>The currently selected entry in the <see cref="List"/>.</summary>
    public UIClickableElement SelectedElement;

    /// <summary>The currently selected dropdown value.</summary>
    public string SelectedValue;

    /// <summary>Fired when a choice is selected.</summary>
    public Action<string> ChoiceSelected;

    /// <summary>Constructs a new UI dropdown.</summary>
    /// <param name="text">The text to display when no choice is selected.</param>
    /// <param name="boxPadding">The padding between the <see cref="Box"/> and <see cref="List"/> entries.</param>
    /// <param name="listSpacing">The spacing betwene <see cref="List"/> entries.</param>
    /// <param name="buttonStyles">The <see cref="Button"/> element styles.</param>
    /// <param name="boxStyle">The <see cref="Box"/> element styles.</param>
    /// <param name="pos">The position of the element.</param>
    public UIDropdown(string text, int boxPadding, int listSpacing, UIClickableElement.StyleGroup buttonStyles, UIElementStyle boxStyle, UIPositionHelper pos) : base(pos)
    {
        PlaceholderInfo = text;
        AddChild(Button = new UIButton(text, HandleOpen, buttonStyles, pos.AtOrigin()));
        Box = new UIBox(boxStyle, pos.AtOrigin());
        Box.AddChild(List = new(listSpacing, new UIPositionHelper(pos.View).Anchor(UIAnchor.TOP_CENTER).ConstantXY(0, boxPadding)));
        Box.Position.GetterHeight(() => List.Position.Height + boxPadding * 2);
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
        ChoiceSelected?.Invoke(SelectedValue);
    }

    /// <summary>Reverts the dropdown to its pre-chosen state.</summary>
    public void Deselect()
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
        List.AddChild(element);
        Choices[element] = choice;
        element.OnClick += () => HandleSelect(element);
    }

    /// <summary>Adds a <see cref="UITextLink"/> as a choice to the dropdown.</summary>
    /// <param name="choice">The choice text.</param>
    /// <param name="linkStyles">The link styles.</param>
    /// <returns>The added text link.</returns>
    public UITextLink AddTextLinkChoice(string choice, UIClickableElement.StyleGroup linkStyles)
    {
        UITextLink link = new(choice, null, null, linkStyles, new UIPositionHelper(Position.View));
        AddChoice(choice, link);
        return link;
    }
}
