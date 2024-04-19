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

        /// <summary>A style/renderable pair internal to this text object.</summary>
        public (UIElementStyle Style, RenderableText Text) InternalRenderable;

        /// <summary>A cache mapping a UI element's text styles to renderable text.</summary>
        public Dictionary<UIElementStyle, RenderableText> RenderableContent;
    }

    // TODO: Get rid of text alignments
    /// <summary>The horizontal alignment of the text, if any.</summary>
    public TextAlignment HorizontalAlignment;

    /// <summary>The vertical alignment of the text, if any.</summary>
    public TextAlignment VerticalAlignment;

    /// <summary>Whether the text is empty and shouldn't be rendered.</summary>
    public bool Empty => (Internal.Content?.Length ?? 0) == 0;

    /// <summary>The UI style to use for rendering this text.</summary>
    public UIElementStyle CurrentStyle => Internal.InternalRenderable.Style ?? Internal.ParentElement.ElementInternal.CurrentStyle;

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
    /// <param name="horizontalAlignment">The horizontal text alignment, if any.</param>
    /// <param name="verticalAlignment">The vertical text alignment, if any.</param>
    /// <returns>The UI text instance.</returns>
    public UIElementText(UIElement parent, string content, bool required = false, int maxWidth = -1, UIElementStyle style = null, TextAlignment horizontalAlignment = TextAlignment.LEFT, TextAlignment verticalAlignment = TextAlignment.TOP)
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
            InternalRenderable = (style, null)
        };
        Required = required;
        HorizontalAlignment = horizontalAlignment;
        VerticalAlignment = verticalAlignment;
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
        if (Internal.InternalRenderable.Style is UIElementStyle internalStyle)
        {
            Internal.InternalRenderable.Text = CreateRenderable(internalStyle);
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
                Internal.InternalRenderable.Text = null;
                Internal.RenderableContent = null;
            }
            else
            {
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
    /// The <see cref="RenderableText"/> object corresponding to the current style.
    /// If <see cref="UIElementStyle.CanRenderText(UIElementText)"/> returns false, this returns <see cref="RenderableText.Empty"/>.
    /// </summary>
    public RenderableText Renderable => !Empty 
        ? Internal.InternalRenderable.Text ?? Internal.RenderableContent?.GetValueOrDefault(Internal.ParentElement.ElementInternal.CurrentStyle, RenderableText.Empty) 
        : RenderableText.Empty;

    /// <summary>The total width of the text.</summary>
    public int Width => Renderable?.Width ?? 0;

    /// <summary>The total height of the text.</summary>
    public int Height => Renderable?.Lines?.Length * CurrentStyle.TextFont?.FontDefault.Height ?? 0;

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

    /// <summary>An individual UI text chain piece, guaranteed to be single-line.</summary>
    /// <param name="Text">The text object this piece takes from.</param>
    /// <param name="Line">The chain piece line.</param>
    /// <param name="XOffset">The x-offset relative to the first piece.</param>
    /// <param name="YOffset">The y-offset relative to the first piece.</param>
    public record ChainPiece(UIElementText Text, RenderableTextLine Line, float XOffset, float YOffset);

    /// <summary>
    /// Iterates through some UI text objects and returns <see cref="ChainPiece"/>s, where each chain piece contains a single line.
    /// This properly handles consecutive text objects even spanning multiple lines.
    /// </summary>
    /// <param name="chain">The UI text objects.</param>
    /// <returns>The text chain.</returns>
    public static IEnumerable<ChainPiece> IterateChain(IEnumerable<UIElementText> chain)
    {
        float x = 0, y = 0;
        foreach (UIElementText text in chain)
        {
            if (!text.CurrentStyle.CanRenderText(text))
            {
                continue;
            }
            for (int i = 0; i < text.Renderable.Lines.Length; i++)
            {
                if (i != 0)
                {
                    x = 0;
                    y += text.CurrentStyle.TextFont.FontDefault.Height;
                }
                RenderableTextLine line = text.Renderable.Lines[i];
                yield return new(text, line, x, y);
                if (i == text.Renderable.Lines.Length - 1)
                {
                    x += line.Width;
                }
            }
        }
    }

    /// <summary>Renders a text chain.</summary>
    /// <seealso cref="IterateChain(IEnumerable{UIElementText})"/>
    /// <param name="chain">The UI text objects.</param>
    /// <param name="x">The starting x position.</param>
    /// <param name="y">The starting y position.</param>
    public static void RenderChain(IEnumerable<UIElementText> chain, float x, float y)
    {
        foreach (ChainPiece piece in IterateChain(chain))
        {
            // TODO: DrawFancyText variant that takes one RenderableTextLine
            RenderableText renderable = new() { Lines = [piece.Line], Width = piece.Line.Width };
            piece.Text.CurrentStyle.TextFont.DrawFancyText(renderable, piece.Text.GetPosition(x + piece.XOffset, y + piece.YOffset));
        }
    }
}
