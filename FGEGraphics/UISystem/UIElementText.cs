//
// This file is part of the Frenetic Game Engine, created by Frenetic LLC.
// This code is Copyright (C) Frenetic LLC under the terms of a strict license.
// See README.md or LICENSE.txt in the FreneticGameEngine source root for the contents of the license.
// If neither of these are available, assume that neither you nor anyone other than the copyright holder
// hold any right or permission to use this software until such time as the official license is identified.
//

using System;
using System.Collections.Generic;
using FGECore.ConsoleHelpers;
using FGECore.MathHelpers;
using FGEGraphics.GraphicsHelpers.FontSets;

namespace FGEGraphics.UISystem;

/// <summary>
/// A text object that automatically updates its renderable content
/// based on a <see cref="UIElement"/>'s <see cref="UIElementStyle"/>s.
/// </summary>
public class UIElementText
{
    /// <summary>The state to display when a required text value is empty.</summary>
    public const string Null = "null";

    /// <summary>Data internal to a <see cref="UIElementText"/> instance.</summary>
    public struct InternalData
    {
        /// <summary>The parent UI element.</summary>
        public UIElement ParentElement;

        /// <summary>The raw string content of this text.</summary>
        public string Content;

        /// <summary>The maximum total width of this text, if any.</summary>
        public int MaxWidth;

        /// <summary>A cache mapping a UI element's text styles to renderable text.</summary>
        public Dictionary<UIElementStyle, RenderableText> RenderableContent;
    }

    /// <summary>The horizontal alignment of the text, if any.</summary>
    public TextAlignment HorizontalAlignment;

    /// <summary>The vertical alignment of the text, if any.</summary>
    public TextAlignment VerticalAlignment;

    /// <summary>Whether the text is empty and shouldn't be rendered.</summary>
    public bool Empty => (Internal.Content?.Length ?? 0) == 0;

    /// <summary>Whether the text is required to display some content.</summary>
    public bool Required;

    /// <summary>Data internal to a <see cref="UIElementText"/> instance.</summary>
    public InternalData Internal;

    /// <summary>Constructs a <see cref="UIElementText"/> instance.</summary>
    /// <param name="parent">The parent UI element.</param>
    /// <param name="content">The initial text content.</param>
    /// <param name="required">Whether the text is required to display, even if empty.</param>
    /// <param name="maxWidth">The maximum total width, if any.</param>
    /// <param name="horizontalAlignment">The horizontal text alignment, if any.</param>
    /// <param name="verticalAlignment">The vertical text alignment, if any.</param>
    /// <returns>The UI text instance.</returns>
    public UIElementText(UIElement parent, string content, bool required = false, int maxWidth = -1, TextAlignment horizontalAlignment = TextAlignment.LEFT, TextAlignment verticalAlignment = TextAlignment.TOP)
    {
        content ??= (required ? Null : null);
        Internal = new InternalData()
        {
            ParentElement = parent,
            Content = content,
            MaxWidth = maxWidth
        };
        Required = required;
        HorizontalAlignment = horizontalAlignment;
        VerticalAlignment = verticalAlignment;
        if (!Empty)
        {
            Internal.RenderableContent = [];
            RefreshRenderables();
        }
        parent.ElementInternal.Texts.Add(this);
    }

    /// <summary>Updates the renderable cache based on the parent element's registered styles.</summary>
    public void RefreshRenderables()
    {
        if (Empty)
        {
            return;
        }
        Internal.RenderableContent.Clear();
        foreach (UIElementStyle style in Internal.ParentElement.ElementInternal.Styles)
        {
            if (!style.CanRenderText())
            {
                continue;
            }
            string styled = style.TextStyling(Internal.Content);
            RenderableText text = style.TextFont.ParseFancyText(styled, style.TextBaseColor);
            if (Internal.MaxWidth > 0)
            {
                text = FontSet.SplitAppropriately(text, Internal.MaxWidth);
            }
            Internal.RenderableContent[style] = text;
        }
    }

    /// <summary>Gets or sets the raw text content.</summary>
    public string Content
    {
        get => Internal.Content;
        set
        {
            Internal.Content = value ?? (Required ? Null : null);
            if (Empty)
            {
                Internal.RenderableContent = null;
            }
            else
            {
                Internal.RenderableContent = [];
                RefreshRenderables();
            }
        }
    }

    // TODO: check for 0 or negative?
    /// <summary>Gets or sets the maximum total width of the text.</summary>
    public int MaxWidth
    {
        get => Internal.MaxWidth;
        set
        {
            Internal.MaxWidth = value;
            RefreshRenderables();
        }
    }

    /// <summary>
    /// The <see cref="RenderableText"/> object corresponding to the parent element's current style.
    /// If <see cref="UIElementStyle.CanRenderText(UIElementText)"/> returns false, this returns <see cref="RenderableText.Empty"/>.
    /// </summary>
    public RenderableText Renderable => !Empty ? Internal.RenderableContent?.GetValueOrDefault(Internal.ParentElement.ElementInternal.CurrentStyle, RenderableText.Empty) : RenderableText.Empty;

    /// <summary>The total width of the text.</summary>
    public int Width => Renderable?.Width ?? 0;

    /// <summary>The total height of the text.</summary>
    public int Height => Renderable?.Lines?.Length * Internal.ParentElement.ElementInternal.CurrentStyle.TextFont?.FontDefault.Height ?? 0;

    /// <summary>Returns the render position of the text given a starting position.</summary>
    /// <param name="startX">The left-oriented anchor X value.</param>
    /// <param name="startY">The top-oriented anchor Y value.</param>
    public Location GetPosition(double startX, double startY)
    {
        double x = Math.Round(startX + HorizontalAlignment.SizeMultiplier() * -Width);
        double y = Math.Round(startY + VerticalAlignment.SizeMultiplier() * -Height);
        return new(x, y, 0);
    }

    /// <summary>Returns <see cref="Renderable"/>.</summary>
    public static implicit operator RenderableText(UIElementText text) => text.Renderable;
}
