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
    /// <summary>Represents a hashable UI text style instance.</summary>
    /// <param name="Font">The text font (or <c>null</c> for none).</param>
    /// <param name="Styled">The styled text string.</param>
    /// <param name="BaseColor">The base text color.</param>
    public record struct StyleInstance(FontSet Font, string Styled, string BaseColor);

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

    /// <summary>The alignment of the text, if any.</summary>
    public TextAlignment Alignment;

    /// <summary>Data internal to a <see cref="UIElementText"/> instance.</summary>
    public InternalData Internal;

    /// <summary>
    /// Creates and returns a <see cref="UIElementText"/> instance.
    /// Generally, prefer calling <see cref="UIElement.CreateText(string, int, TextAlignment)"/> instead.
    /// </summary>
    /// <param name="parent">The parent UI element.</param>
    /// <param name="content">The initial text content.</param>
    /// <param name="maxWidth">The maximum total width, if any.</param>
    /// <param name="alignment">The text alignment, if any.</param>
    /// <returns>The UI text instance.</returns>
    public UIElementText(UIElement parent, string content, int maxWidth = -1, TextAlignment alignment = TextAlignment.LEFT)
    {
        Internal = new InternalData()
        {
            ParentElement = parent,
            Content = content,
            MaxWidth = maxWidth,
            RenderableContent = new Dictionary<UIElementStyle, RenderableText>()
        };
        Alignment = alignment;
        RefreshRenderables();
    }

    /// <summary>Updates the renderable cache based on the parent element's registered styles.</summary>
    private void RefreshRenderables()
    {
        Internal.RenderableContent.Clear();
        HashSet<StyleInstance> instances = new();
        foreach (UIElementStyle style in Internal.ParentElement.ElementInternal.Styles)
        {
            if (!style.CanRenderText())
            {
                continue;
            }
            string styled = style.TextStyling(Internal.Content);
            StyleInstance instance = new(style.TextFont, styled, style.TextBaseColor);
            if (!instances.Add(instance))
            {
                continue;
            }
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
            Internal.Content = value;
            RefreshRenderables();
        }
    }

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
    /// Check <see cref="UIElementStyle.CanRenderText(UIElementText)"/> first.
    /// </summary>
    public RenderableText Renderable => Internal.RenderableContent[Internal.ParentElement.ElementInternal.CurrentStyle];

    /// <summary>The total width of the text.</summary>
    public int Width => Renderable.Width;

    /// <summary>The total height of the text.</summary>
    public int Height => Renderable.Lines.Length * Internal.ParentElement.ElementInternal.CurrentStyle.TextFont.FontDefault.Height;

    /// <summary>Returns the render position of the text given a starting position.</summary>
    /// <param name="startX">The left-oriented anchor X value.</param>
    /// <param name="startY">The top-oriented anchor Y value.</param>
    public Location GetPosition(double startX, double startY)
    {
        float sizeMul = Alignment.SizeMultiplier();
        double x = Math.Round(startX + sizeMul * -Width);
        double y = Math.Round(startY + sizeMul * -Height);
        return new(x, y, 0);
    }

    /// <summary>
    /// Returns <see cref="Renderable"/>.
    /// Check <see cref="UIElementStyle.CanRenderText(UIElementText)"/> first.
    /// </summary>
    public static implicit operator RenderableText(UIElementText text)
    {
        return text.Renderable;
    }
}
