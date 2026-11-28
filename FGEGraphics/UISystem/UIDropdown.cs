//
// This file is part of the Frenetic Game Engine, created by Frenetic LLC.
// This code is Copyright (C) Frenetic LLC under the terms of a strict license.
// See README.md or LICENSE.txt in the FreneticGameEngine source root for the contents of the license.
// If neither of these are available, assume that neither you nor anyone other than the copyright holder
// hold any right or permission to use this software until such time as the official license is identified.
//

using FGECore.MathHelpers;
using FGEGraphics.UISystem.Elements;
using System;
using System.Collections.Generic;

namespace FGEGraphics.UISystem;

/// <summary>Represents an interactable dropdown of different choices.</summary>
// TODO: Choice search
// TODO: Arrow key + ENTER navigation
// TODO: Scroll if exceed max width
// TODO: Dropdown icon next to placeholder info?
// TODO: Different style for placeholder info
// TODO: Add choice at index, remove choice
public class UIDropdown : UIElement, IStylingAcceptor<UIDropdown.StylingOptions>
{
    public record StylingOptions : StylingComponent
    {
        public UIStyleValue<UIStyling> ChoiceStyling;
    }

    /// <inheritdoc/>
    public override string Name => "Dropdown";

    /// <summary>The text to display when no choice is selected.</summary>
    [UIDebug]
    public string PlaceholderInfo;

    /// <summary>The button to open the dropdown.</summary>
    public UIButton Button;

    /// <summary>The layer to place the list container on.</summary>
    public UIElement Layer;

    public UISelectionGroup Selections;

    public UIList2 Entries;

    /// <summary>Data internal to a <see cref="UIDropdown"/> instance.</summary>
    public struct InternalData()
    {
        /// <summary>Maps choices to their string representations.</summary>
        public Dictionary<UIElement, Func<string>> ToStrings = [];
    }

    /// <summary>Data internal to a <see cref="UIDropdown"/> instance.</summary>
    public InternalData Internal = new();

    public UIStyling ChoiceStyling = UIStyling.Empty; // TODO: sensible default

    /// <summary>Constructs a new UI dropdown.</summary>
    /// <param name="layout">The layout of the element.</param>
    /// <param name="text">The text to display when no choice is selected.</param>
    public UIDropdown(string text, UIStyling styling = null, UILayout layout = null) : base(null, layout)
    {
        PlaceholderInfo = text ?? "...";
        Layer = this;
        AddChild(Button = new UIButton(text, null, layout.Container()) { OnClick = Open });
        Entries = new UIList2(null, new UILayout().SetAnchor(UIAnchor.TOP_LEFT)) { IsStateLocked = true, IsEnabled = false };
        Entries.Layout.SetPosition(() => X - Layer.X, () => Y - Layer.Y);
        Selections = new UISelectionGroup(new UILayout()) { MinSelections = 0, MaxSelections = 1 };
        Selections.OnSelectElement += (_, element) =>
        {
            if (Layer.HasChild(Entries))
            {
                Close();
            }
            Button.Label.Content = Internal.ToStrings[element]();
        };
        Selections.OnDeselectElement += element => Button.Label.Content = PlaceholderInfo;
        SetStyling(styling);
    }

    /// <summary>Opens the dropdown list.</summary>
    public void Open()
    {
        RemoveChild(Button);
        Layer.AddChild(Entries);
        Entries.Focus();
    }

    /// <summary>Closes the dropdown list.</summary>
    public void Close()
    {
        Layer.RemoveChild(Entries);
        AddChild(Button);
        Button.Focus();
    }

    /// <summary>Selects a dropdown choice, closing the container if necessary.</summary>
    /// <param name="choice">The choice to select.</param>
    /*public void SelectChoice(UIElement choice)
    {
        SelectedChoice = choice;
        if (Layer.HasChild(Entries))
        {
            Close();
        }
        Button.Label.Content = choice is not null ? Internal.ToStrings[choice]() : PlaceholderInfo;
        if (choice is not null)
        {
            OnChoiceSelect?.Invoke(choice);
        }
    }*/

    /// <summary>Reverts the dropdown to its pre-chosen state.</summary>
    /*public void DeselectChoice()
    {
        SelectChoice(null);
    }*/

    /// <summary>Adds a choice to the dropdown.</summary>
    /// <param name="choice">The choice element.</param>
    /// <param name="toString">The string representation of the choice.</param>
    public void AddChoice(UIElement choice, Func<string> toString)
    {
        // TODO: configurable appearance
        UIStyling containerStyle = Entries.Children.Count % 2 == 0 ? null : new UIStyling { Fill = new Color4F(0, 0, 0, 0.25f), ShowBackground = true };
        UIElement container = new(containerStyle, new UILayout().SetSize(() => Width, () => choice.Height)) { IsEnabled = false };
        choice.Layout.SetAnchor(UIAnchor.TOP_CENTER);
        choice.SetStyling(ChoiceStyling);
        container.AddChild(choice);
        Entries.AddChild(container);
        Selections.AddElement(choice, addChild: false);
        Internal.ToStrings[choice] = toString;
    }

    /// <summary>Adds a <see cref="UILabel"/> as a choice to the dropdown.</summary>
    /// <param name="choice">The choice text.</param>
    /// <param name="styling">The label styles.</param>
    /// <param name="tag">An optional value attached to the resulting label.</param>
    /// <returns>The created label.</returns>
    public UILabel AddChoice(string choice, UIStyling styling, object tag = null)
    {
        UILabel label = new(choice, ChoiceStyling, new UILayout()) { Tag = tag };
        AddChoice(label, () => label.Content);
        return label;
    }

    // TODO: remove choice

    public override UIStyling AcceptStyling(UIStyling styling) => styling with { ShowBackground = false };

    public override void StylingChanged(UIStyling from, UIStyling to)
    {
        to = to with { ShowBackground = true };
        Button.SetStyling(to);
        Entries.SetStyling(to);
    }

    public void AcceptStyling(StylingOptions options)
    {
        ChoiceStyling = options.ChoiceStyling.Get(this);
        foreach (UIElement choice in Selections.Elements)
        {
            choice.SetStyling(ChoiceStyling);
        }
    }
}
