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
using FGEGraphics.GraphicsHelpers.FontSets;
using FreneticUtilities.FreneticExtensions;

namespace FGEGraphics.UISystem;

/// <summary>
/// A text object that automatically updates its renderable content
/// based on a <see cref="UIElement"/>'s <see cref="UIElementStyle"/>s.
/// </summary>
public class UIElementText
{
    /// <summary>Represents a hashable UI text style instance.</summary>
    /// <param name="Font">The text font (or <c>null</c> for none).</param>
    /// <param name="Styling">The base color effect for text (consider <see cref="TextStyle.Simple"/> if unsure).</param>
    public record struct StyleInstance(FontSet Font, string Styling);

    /// <summary>Data internal to a <see cref="UIElementText"/> instance.</summary>
    public struct InternalData
    {
        /// <summary>The parent UI element.</summary>
        public UIElement ParentElement;

        /// <summary>The raw string content of this text.</summary>
        public string RawContent;

        /// <summary>The custom width for this text, if any.</summary>
        public int Width;

        /// <summary>The current style of the parent element.</summary>
        public UIElementStyle CurrentStyle;

        /// <summary>A cache mapping a UI element's text styles to renderable text.</summary>
        public Dictionary<UIElementStyle, RenderableText> RenderableContent;
    }

    /// <summary>Data internal to a <see cref="UIElementText"/> instance.</summary>
    public InternalData Internal;

    /// <summary>
    /// Creates and returns a <see cref="UIElementText"/> instance.
    /// Generally, prefer calling <see cref="UIElement.CreateText(string, int)"/> instead.
    /// </summary>
    /// <param name="parent">The parent UI element.</param>
    /// <param name="content">The initial text content.</param>
    /// <param name="width">The custom maximum width, if any.</param>
    /// <returns>The UI text instance.</returns>
    public UIElementText(UIElement parent, string content, int width = -1)
    {
        Internal = new InternalData()
        {
            ParentElement = parent,
            RawContent = content,
            Width = width,
            RenderableContent = new Dictionary<UIElementStyle, RenderableText>()
        };
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
            string styling = style.TextStyling(Internal.RawContent);
            StyleInstance instance = new(style.TextFont, styling);
            if (!instances.Add(instance))
            {
                continue;
            }
            RenderableText text = style.TextFont.ParseFancyText(Internal.RawContent, styling);
            if (Internal.Width > 0)
            {
                text = FontSet.SplitAppropriately(text, Internal.Width);
            }
            Internal.RenderableContent[style] = text;
        }
    }

    /// <summary>Gets or sets the raw text content.</summary>
    public string Content
    {
        get => Internal.RawContent;
        set
        {
            Internal.RawContent = value;
            RefreshRenderables();
        }
    }

    /// <summary>Gets or sets the maximum width of the text.</summary>
    public int Width
    {
        get => Internal.Width;
        set
        {
            Internal.Width = value;
            RefreshRenderables();
        }
    }

    /// <summary>
    /// The <see cref="RenderableText"/> object corresponding to the parent element's current style.
    /// Check <see cref="UIElementStyle.CanRenderText(UIElementText)"/> first.
    /// </summary>
    public RenderableText Renderable => Internal.RenderableContent[Internal.ParentElement.ElementInternal.CurrentStyle];

    /// <summary>
    /// Returns <see cref="Renderable"/>.
    /// Check <see cref="UIElementStyle.CanRenderText(UIElementText)"/> first.
    /// </summary>
    public static implicit operator RenderableText(UIElementText text)
    {
        return text.Renderable;
    }
}
