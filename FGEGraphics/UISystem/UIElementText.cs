//
// This file is part of the Frenetic Game Engine, created by Frenetic LLC.
// This code is Copyright (C) Frenetic LLC under the terms of a strict license.
// See README.md or LICENSE.txt in the FreneticGameEngine source root for the contents of the license.
// If neither of these are available, assume that neither you nor anyone other than the copyright holder
// hold any right or permission to use this software until such time as the official license is identified.
//

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using FGECore.ConsoleHelpers;
using FGECore.CoreSystems;
using FGECore.MathHelpers;
using FGEGraphics.GraphicsHelpers.FontSets;

namespace FGEGraphics.UISystem;

/// <summary>A text object that automatically updates its renderable content based on a <see cref="UIElement"/>'s <see cref="UIElementStyle"/>s.</summary>
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

        /// <summary>A renderable text object internal to this text.</summary>
        public RenderableText InternalRenderable;

        /// <summary>An element style internal to this text.</summary>
        public UIElementStyle InternalStyle;

        /// <summary>A cache mapping a UI element's text styles to renderable text.</summary>
        public Dictionary<UIElementStyle, RenderableText> RenderableContent;
    }

    /// <summary>Whether the text is empty and shouldn't be rendered.</summary>
    public bool Empty => (Internal.Content?.Length ?? 0) == 0;

    /// <summary>The UI style to use for rendering this text.</summary>
    public UIElementStyle CurrentStyle => Internal.InternalStyle ?? Internal.ParentElement.ElementInternal.CurrentStyle;

    /// <summary>Whether the text is required to display some content.</summary>
    public bool Required;

    /// <summary>Data internal to a <see cref="UIElementText"/> instance.</summary>
    public InternalData Internal;

    /// <summary>Constructs a <see cref="UIElementText"/> instance.</summary>
    /// <param name="parent">The parent UI element.</param>
    /// <param name="content">The initial text content.</param>
    /// <param name="required">Whether the text is required to display, even if empty.</param>
    /// <param name="maxWidth">The maximum total width, if any.</param>
    /// <param name="style">An internal style to use instead of the parent element's.</param>
    /// <returns>The UI text instance.</returns>
    public UIElementText(UIElement parent, string content, bool required = false, int maxWidth = -1, UIElementStyle style = null)
    {
        content ??= (required ? Null : null);
        if (style is not null && !style.CanRenderText())
        {
            throw new Exception("Internal text style must support text rendering");
        }
        Internal = new InternalData()
        {
            ParentElement = parent,
            Content = content,
            MaxWidth = maxWidth,
            InternalStyle = style,
            InternalRenderable = null
        };
        Required = required;
        if (!Empty)
        {
            RefreshRenderables();
        }
        parent.ElementInternal.Texts.Add(this);
    }

    /// <summary>Creates a <see cref="RenderableText"/> of the text <see cref="Content"/> given a style.</summary>
    /// <param name="style">The UI style to use.</param>
    /// <returns>The resulting renderable object.</returns>
    public RenderableText CreateRenderable(UIElementStyle style)
    {
        string styled = style.TextStyling(Content);
        RenderableText renderable = style.TextFont.ParseFancyText(styled, style.TextBaseColor);
        if (Internal.MaxWidth > 0)
        {
            renderable = FontSet.SplitAppropriately(renderable, Internal.MaxWidth);
        }
        return renderable;
    }

    /// <summary>Updates the renderable cache based on the registered styles.</summary>
    public void RefreshRenderables()
    {
        if (Empty)
        {
            return;
        }
        if (Internal.InternalStyle is UIElementStyle internalStyle)
        {
            Internal.InternalRenderable = CreateRenderable(internalStyle);
            return;
        }
        Internal.RenderableContent = [];
        foreach (UIElementStyle style in Internal.ParentElement.ElementInternal.Styles)
        {
            if (style.CanRenderText())
            {
                Internal.RenderableContent[style] = CreateRenderable(style);
            }
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
                Internal.InternalRenderable = null;
                Internal.RenderableContent = null;
            }
            else
            {
                RefreshRenderables();
            }
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

    // TODO: make these not NPE when empty
    /// <summary>
    /// The <see cref="RenderableText"/> object corresponding to the current style.
    /// If <see cref="UIElementStyle.CanRenderText(UIElementText)"/> returns false, this returns <see cref="RenderableText.Empty"/>.
    /// </summary>
    public RenderableText Renderable => !Empty 
        ? Internal.InternalRenderable ?? Internal.RenderableContent?.GetValueOrDefault(Internal.ParentElement.ElementInternal.CurrentStyle, RenderableText.Empty) 
        : RenderableText.Empty;

    /// <summary>The total width of the text.</summary>
    public int Width => Renderable?.Width ?? 0;

    /// <summary>The total height of the text.</summary>
    public int Height => Renderable?.Lines?.Length * CurrentStyle.TextFont?.Height ?? 0;

    /// <summary>Returns <see cref="Renderable"/>.</summary>
    public static implicit operator RenderableText(UIElementText text) => text.Renderable;

    /// <summary>An individual UI text chain piece.</summary>
    /// <param name="Font">The font to render the chain piece with.</param>
    /// <param name="Text">The chain piece text.</param>
    /// <param name="YOffset">The y-offset relative to the first piece.</param>
    /// <param name="SkippedIndices">A list of character indices ignored in <see cref="FontSet.SplitLineAppropriately(RenderableTextLine, float, out List{int})"/>.</param>
    public record ChainPiece(FontSet Font, RenderableText Text, float YOffset, List<int> SkippedIndices);

    /// <summary>
    /// Iterates through some UI text objects and returns <see cref="ChainPiece"/>s, where each chain piece contains a single line.
    /// This properly handles consecutive text objects even spanning multiple lines.
    /// </summary>
    /// <param name="chain">The UI text objects.</param>
    /// <param name="maxWidth">The wrapping width of the chain.</param>
    /// <returns>The text chain.</returns>
    // TODO: Fix blank lines not being counted
    public static IEnumerable<ChainPiece> IterateChain(IEnumerable<UIElementText> chain, float maxWidth = -1)
    {
        List<(FontSet Font, RenderableTextLine Line)> lines = [];
        foreach (UIElementText text in chain)
        {
            if (!text.CurrentStyle.CanRenderText(text))
            {
                continue;
            }
            List<RenderableTextLine> textLines = [.. text.Renderable.Lines];
            if (lines.Count != 0)
            {
                RenderableTextLine combinedLine = new()
                {
                    Parts = [.. lines[^1].Line.Parts, .. textLines[0].Parts],
                    Width = lines[^1].Line.Width + textLines[0].Width
                };
                lines[^1] = (lines[^1].Font, combinedLine);
                textLines.RemoveAt(0);
            }
            foreach (RenderableTextLine line in textLines)
            {
                lines.Add((text.CurrentStyle.TextFont, line));
            }
        }
        float y = 0;
        foreach ((FontSet font, RenderableTextLine line) in lines)
        {
            List<int> skippedIndices = null;
            RenderableText splitText = maxWidth > 0 ? FontSet.SplitLineAppropriately(line, maxWidth, out skippedIndices) : new(line);
            yield return new(font, splitText, y, skippedIndices ?? []);
            y += font.Height * splitText.Lines.Length;
        }
    }

    /// <summary>Renders a text chain.</summary>
    /// <seealso cref="IterateChain(IEnumerable{UIElementText}, float)"/>
    /// <param name="chain">The UI text objects.</param>
    /// <param name="x">The starting x position.</param>
    /// <param name="y">The starting y position.</param>
    public static void RenderChain(IEnumerable<ChainPiece> chain, float x, float y)
    {
        foreach (ChainPiece piece in chain)
        {
            piece.Font.DrawFancyText(piece.Text, new Location(x, y + piece.YOffset, 0));
        }
    }
}
