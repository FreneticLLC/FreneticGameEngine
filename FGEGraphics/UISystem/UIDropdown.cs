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
using FGECore.MathHelpers;
using OpenTK.Mathematics;

namespace FGEGraphics.UISystem;

/// <summary>Represents an interactable dropdown of different choices.</summary>
// TODO: Choice search
// TODO: Arrow key + ENTER navigation
// TODO: Scroll if exceed max width
// TODO: Dropdown icon next to placeholder info?
// TODO: Different style for placeholder info
// TODO: Add choice at index, remove choice
public class UIDropdown : UIElement
{
    /// <summary>The text to display when no choice is selected.</summary>
    public string PlaceholderInfo;

    /// <summary>The button to open the dropdown.</summary>
    public UIBox Button;

    /// <summary>The box container surrounding the <see cref="Entries"/>.</summary>
    public UIBox Box;

    /// <summary>The dropdown list of choice entries.</summary>
    public UIListGroup Entries;

    /// <summary>The list of selectable choices in the dropdown.</summary>
    public List<UIElement> Choices = [];

    /// <summary>The currently selected entry in the <see cref="Entries"/>.</summary>
    public UIElement SelectedChoice; // TODO: selection group

    /// <summary>Fired when a choice is selected.</summary>
    public Action<UIElement> OnChoiceSelect;

    /// <summary>Data internal to a <see cref="UIDropdown"/> instance.</summary>
    public struct InternalData()
    {
        /// <summary>The layer to place the list container on, if any.</summary>
        public UIElement Layer;

        /// <summary>Maps choices to their string representations.</summary>
        public Dictionary<UIElement, Func<string>> ToStrings = [];
    }

    /// <summary>Data internal to a <see cref="UIDropdown"/> instance.</summary>
    public InternalData Internal = new();

    /// <summary>Constructs a new UI dropdown.</summary>
    /// <param name="boxPadding">The padding between the <see cref="Box"/> and <see cref="Entries"/> entries.</param>
    /// <param name="listSpacing">The spacing betwene <see cref="Entries"/> entries.</param>
    /// <param name="buttonStyling">The <see cref="Button"/> element styling.</param>
    /// <param name="boxStyling">The <see cref="Box"/> element styling.</param>
    /// <param name="layout">The layout of the element.</param>
    /// <param name="text">The text to display when no choice is selected.</param>
    /// <param name="layer">An optional layer to place the dropdown on. If <c>null</c>, uses this element's layer.</param>
    public UIDropdown(int boxPadding, int listSpacing, UIStyling buttonStyling, UIStyling boxStyling, UILayout layout, string text = null, UIElement layer = null) : base(buttonStyling, layout)
    {
        PlaceholderInfo = text ?? "null";
        AddChild(Button = new UIBox(buttonStyling, layout.AtOrigin(), text) { OnClick = Open });
        Box = new UIBox(boxStyling, layout.AtOrigin());
        Box.AddChild(Entries = new UIListGroup(listSpacing, new UILayout().SetAnchor(UIAnchor.TOP_CENTER).SetPosition(0, boxPadding)));
        Box.Layout.SetHeight(() => Entries.Layout.Height + boxPadding * 2);
        Internal.Layer = layer ?? this;
        if (layer is not null)
        {
            Box.Layout.SetPosition(() => X - Internal.Layer.X, () => Y - Internal.Layer.Y);
        }
        // TODO
        //Box.OnUnfocus += Close;
    }

    /// <summary>Opens the dropdown list.</summary>
    public void Open()
    {
        RemoveChild(Button);
        Internal.Layer.AddChild(Box);
        Box.Focus();
    }

    /// <summary>Closes the dropdown list.</summary>
    public void Close()
    {
        Internal.Layer.RemoveChild(Box);
        AddChild(Button);
        Button.Focus();
    }

    /// <summary>Selects a dropdown choice, closing the container if necessary.</summary>
    /// <param name="choice">The choice to select.</param>
    public void SelectChoice(UIElement choice)
    {
        SelectedChoice = choice;
        if (Internal.Layer.HasChild(Box))
        {
            Close();
        }
        Button.Label.Content = choice is not null ? Internal.ToStrings[choice]() : PlaceholderInfo;
        if (choice is not null)
        {
            OnChoiceSelect?.Invoke(choice);
        }
    }

    /// <summary>Reverts the dropdown to its pre-chosen state.</summary>
    public void DeselectChoice()
    {
        SelectChoice(null);
    }

    /// <summary>Adds a choice to the dropdown.</summary>
    /// <param name="choice">The choice element.</param>
    /// <param name="label">The string representation of the choice.</param>
    public void AddChoice(UIElement choice, Func<string> label)
    {
        // TODO: configurable appearance
        UIStyle containerStyle = Entries.Items.Count % 2 == 0 ? UIStyle.Empty : new UIStyle { BaseColor = new(0, 0, 0, 0.25f) };
        UIBox container = new(containerStyle, new UILayout().SetSize(() => Box.Width, () => choice.Height));
        choice.Layout.SetAnchor(UIAnchor.TOP_CENTER);
        container.AddChild(choice);
        Entries.AddListItem(container);
        Choices.Add(choice);
        Internal.ToStrings[choice] = label;
        choice.OnClick += () => SelectChoice(choice);
    }

    /// <summary>Adds a <see cref="UILabel2"/> as a choice to the dropdown.</summary>
    /// <param name="choice">The choice text.</param>
    /// <param name="styling">The label styles.</param>
    /// <param name="tag">An optional value attached to the resulting label.</param>
    /// <returns>The created label.</returns>
    public UILabel2 AddLabelChoice(string choice, UIStyling styling, object tag = null)
    {
        UILabel2 label = new(choice, styling, new UILayout()) { Tag = tag };
        AddChoice(label, () => label.Content);
        return label;
    }
}
