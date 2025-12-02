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
using System.Text;
using FGECore.ConsoleHelpers;
using FGECore.CoreSystems;
using FGECore.MathHelpers;
using FGEGraphics.GraphicsHelpers;
using FGEGraphics.GraphicsHelpers.FontSets;
using FreneticUtilities.FreneticExtensions;

namespace FGEGraphics.UISystem;

/// <summary>A text object that automatically updates its renderable content based on a <see cref="UIElement"/>'s <see cref="UIStyle"/>s.</summary>
public class UIText
{
    /// <summary>The state to display when a required text value is empty.</summary>
    public const string Null = "null";

    /// <summary>Whether the text is empty and shouldn't be rendered.</summary>
    public bool Empty => (Internal.Content?.Length ?? 0) == 0 || Internal.Element.Scale == 0;

    /// <summary>The UI style to use for rendering this text.</summary>
    public UIStyle Style => Internal.Style ?? Internal.Element.Style;

    /// <summary>Whether the text is required to display some content.</summary>
    public bool Required;

    /// <summary>Data internal to a <see cref="UIText"/> instance.</summary>
    public struct InternalData
    {
        /// <summary>The parent UI element.</summary>
        public UIElement Element;

        /// <summary>The raw string content of this text.</summary>
        public string Content;

        /// <summary>The maximum total width of this text, if any.</summary>
        public int MaxWidth;

        /// <summary>An element style internal to this text.</summary>
        public UIStyle Style;

        /// <summary>A renderable text object internal to this text.</summary>
        public RenderableText Renderable;

        /// <summary>A cache mapping a UI element's text styles to renderable text.</summary>
        public Dictionary<UIStyle, RenderableText> Renderables;
    }

    /// <summary>Data internal to a <see cref="UIText"/> instance.</summary>
    public InternalData Internal;

    /// <summary>Constructs a <see cref="UIText"/> instance.</summary>
    /// <param name="element">The parent UI element.</param>
    /// <param name="content">The initial text content.</param>
    /// <param name="required">Whether the text is required to display, even if empty.</param>
    /// <param name="maxWidth">The maximum total width, if any.</param>
    /// <param name="style">An internal style to use instead of the parent element's.</param>
    public UIText(UIElement element, string content, bool required = false, int maxWidth = -1, UIStyle style = null /* TODO: UIStyling? */)
    {
        content ??= (required ? Null : null);
        if (style is not null && !style.CanRenderText())
        {
            throw new Exception("Internal text style must support text rendering");
        }
        Required = required;
        Internal = new InternalData()
        {
            Element = element,
            Content = content,
            MaxWidth = maxWidth,
            Style = style,
            Renderable = null
        };
        UpdateRenderables();
        element.ElementInternal.Texts.Add(this);
    }

    /// <summary>Creates a <see cref="RenderableText"/> of the text <see cref="Content"/> given a style.</summary>
    /// <param name="style">The UI style to use.</param>
    /// <returns>The resulting renderable object.</returns>
    public RenderableText CreateRenderable(UIStyle style)
    {
        string styledContent = style.TextStyling(Content); // FIXME: this doesn't play well with translatable text.
        int fontSize = (int)(style.TextFont.Size * Internal.Element.Scale);
        // TODO: cache this somewhere, as it's likely for many elements with text to have the same scale value
        FontSet font = style.TextFont.Engine.Fonts
            .Where(pair => pair.Value.Name == style.TextFont.Name)
            .MinBy(pair => Math.Abs(pair.Key.Item2 - fontSize))
            .Value;
        RenderableText renderable = font.ParseFancyText(styledContent, style.TextBaseColor);
        if (Internal.MaxWidth > 0)
        {
            renderable = FontSet.SplitAppropriately(renderable, Internal.MaxWidth);
        }
        return renderable;
    }

    /// <summary>Updates the renderable cache based on the registered styles.</summary>
    public void UpdateRenderables()
    {
        if (Empty)
        {
            Internal.Renderable = null;
            Internal.Renderables = null;
            return;
        }
        if (Internal.Style is UIStyle internalStyle)
        {
            Internal.Renderable = CreateRenderable(internalStyle);
            return;
        }
        Internal.Renderables ??= [];
        foreach (UIStyle style in Internal.Element.ElementInternal.Styles)
        {
            if (style.CanRenderText())
            {
                Internal.Renderables[style] = CreateRenderable(style);
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
            UpdateRenderables();
        }
    }

    /// <summary>Gets or sets the maximum total width of the text.</summary>
    public int MaxWidth
    {
        get => Internal.MaxWidth;
        set
        {
            Internal.MaxWidth = value;
            UpdateRenderables();
        }
    }

    /// <summary>
    /// The <see cref="RenderableText"/> object corresponding to the current style.
    /// If <see cref="UIStyle.CanRenderText(UIText)"/> returns <c>false</c>, this returns <see cref="RenderableText.Empty"/>.
    /// </summary>
    public RenderableText Renderable => !Empty 
        ? Internal.Renderable ?? Internal.Renderables?.GetValueOrDefault(Internal.Element.Style, RenderableText.Empty) 
        : RenderableText.Empty;

    /// <summary>The total width of the text.</summary>
    public int Width => Renderable?.Width ?? 0;

    /// <summary>The total height of the text.</summary>
    public int Height => Renderable?.Height ?? 0;

    /// <summary>Returns <see cref="Renderable"/>.</summary>
    public static implicit operator RenderableText(UIText text) => text.Renderable;

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
    public static IEnumerable<ChainPiece> IterateChain(IEnumerable<UIText> chain, float maxWidth = -1)
    {
        List<(FontSet Font, RenderableTextLine Line)> lines = [];
        foreach (UIText text in chain)
        {
            if (!text.Style.CanRenderText(text))
            {
                continue;
            }
            List<RenderableTextLine> textLines = [.. text.Renderable.Lines];
            if (lines.Count != 0)
            {
                RenderableTextLine combinedLine = new([.. lines[^1].Line.Parts, .. textLines[0].Parts]);
                lines[^1] = (lines[^1].Font, combinedLine);
                textLines.RemoveAt(0);
            }
            foreach (RenderableTextLine line in textLines)
            {
                lines.Add((text.Style.TextFont, line));
            }
        }
        float y = 0;
        foreach ((FontSet font, RenderableTextLine line) in lines)
        {
            List<int> skippedIndices = null;
            RenderableText splitText = maxWidth > 0 ? FontSet.SplitLineAppropriately(line, maxWidth, out skippedIndices) : new([line]);
            yield return new(font, splitText, y, skippedIndices ?? []);
            y += font.Height * splitText.Lines.Length;
        }
    }

    /// <summary>Renders a text chain.</summary>
    /// <seealso cref="IterateChain(IEnumerable{UIText}, float)"/>
    /// <param name="chain">The UI text objects.</param>
    /// <param name="x">The starting x position.</param>
    /// <param name="y">The starting y position.</param>
    public static void RenderChain(IEnumerable<ChainPiece> chain, float x, float y)
    {
        GraphicsUtil.CheckError("UIElementText - PreRenderChain");
        foreach (ChainPiece piece in chain)
        {
            piece.Font.DrawFancyText(piece.Text, new Location(x, y + piece.YOffset, 0));
        }
    }
}
