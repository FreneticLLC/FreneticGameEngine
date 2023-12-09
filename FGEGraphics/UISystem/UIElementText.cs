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
    /// <summary>Data internal to a <see cref="UIElementText"/> instance.</summary>
    public struct InternalData
    {
        /// <summary>The parent UI element.</summary>
        public UIElement ParentElement;

        /// <summary>The raw string content of this text.</summary>
        public string RawContent;
        
        /// <summary>A cache mapping a UI element's styles to renderable text.</summary>
        // TODO: The only relevant data here is fontset/styling. Could make that an internal class on
        // UIElementStyle -- just those two values -- and use that here as the key instead (with proper
        // hashing/eq implementation). This would solve redundant entries with styles that don't differ
        // in font.
        public Dictionary<UIElementStyle, RenderableText> RenderableContent;
    }

    /// <summary>Data internal to a <see cref="UIElementText"/> instance.</summary>
    public InternalData Internal;

    /// <summary>
    /// Creates and returns a <see cref="UIElementText"/> instance.
    /// Generally, prefer calling <see cref="UIElement.CreateText(string)"/> instead.
    /// </summary>
    /// <param name="parent">The parent UI element.</param>
    /// <param name="content">The initial text content.</param>
    /// <returns>The UI text instance.</returns>
    public UIElementText(UIElement parent, string content)
    {
        Internal = new InternalData()
        {
            ParentElement = parent,
            RawContent = content,
            RenderableContent = new Dictionary<UIElementStyle, RenderableText>()
        };
        RefreshRenderables();
    }

    /// <summary>Updates the renderable cache based on an element's registered styles.</summary>
    private void RefreshRenderables()
    {
        foreach (UIElementStyle style in Internal.ParentElement.ElementInternal.Styles)
        {
            if (style.TextFont is not null)
            {
                Internal.RenderableContent[style] = style.TextFont.ParseFancyText(Internal.RawContent, style.TextStyling);
            }
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

    /// <summary>
    /// The <see cref="RenderableText"/> object corresponding to the parent element's current style.
    /// Check <see cref="UIElementStyle.CanRenderText(UIElementText)"/> first.
    /// </summary>
    public RenderableText Renderable => Internal.RenderableContent[Internal.ParentElement.GetStyle()];

    /// <summary>
    /// Returns <see cref="Renderable"/>.
    /// Check <see cref="UIElementStyle.CanRenderText(UIElementText)"/> first.
    /// </summary>
    public static implicit operator RenderableText(UIElementText text)
    {
        return text.Renderable;
    }
}
