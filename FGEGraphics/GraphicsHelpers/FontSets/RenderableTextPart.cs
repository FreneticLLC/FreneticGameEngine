//
// This file is part of the Frenetic Game Engine, created by Frenetic LLC.
// This code is Copyright (C) Frenetic LLC under the terms of a strict license.
// See README.md or LICENSE.txt in the FreneticGameEngine source root for the contents of the license.
// If neither of these are available, assume that neither you nor anyone other than the copyright holder
// hold any right or permission to use this software until such time as the official license is identified.
//

using FGECore.CoreSystems;
using FGECore.MathHelpers;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FGEGraphics.GraphicsHelpers.FontSets;

/// <summary>Represents a single section of text with a single format across all characters.</summary>
public class RenderableTextPart
{
    /// <summary>The actual characters of text to render.</summary>
    public string Text;

    // TODO: should this be a float?
    /// <summary>The horizontal width of the text, in pixels.</summary>
    public float Width;

    /// <summary>The color of the text.</summary>
    public Color4F TextColor = Color4F.White;

    /// <summary>Whether the text is bold.</summary>
    public bool Bold = false;

    /// <summary>Whether the text is italic.</summary>
    public bool Italic = false;

    /// <summary>Whether the text is underlined.</summary>
    public bool Underline = false;

    /// <summary>Whether the text has a strike-through.</summary>
    public bool Strike = false;

    /// <summary>Whether the text is overlined.</summary>
    public bool Overline = false;

    /// <summary>Whether the text is highlighted.</summary>
    public bool Highlight = false;

    /// <summary>Whether the text is emphasized.</summary>
    public bool Emphasis = false;

    /// <summary>The color of the underline (if any).</summary>
    public Color4F UnderlineColor = Color4F.White;

    /// <summary>The color of the strike-through (if any).</summary>
    public Color4F StrikeColor = Color4F.White;

    /// <summary>The color of the overline (if any).</summary>
    public Color4F OverlineColor = Color4F.White;

    /// <summary>The color of the highlight box (if any).</summary>
    public Color4F HighlightColor = Color4F.White;

    /// <summary>The color of the emphasis (if any).</summary>
    public Color4F EmphasisColor = Color4F.White;

    /// <summary>Whether the text is a superscript (raised and half height).</summary>
    public bool SuperScript = false;

    /// <summary>Whether the text is a subscript (lowered and half height).</summary>
    public bool SubScript = false;

    /// <summary>Whether the text is vertically flipped.</summary>
    public bool Flip = false;

    /// <summary>Whether the text is pseudo-random (random color per character that doesn't change with time).</summary>
    public bool PseudoRandom = false;

    /// <summary>Whether the text is 'jello' (shakes in place).</summary>
    public bool Jello = false;

    /// <summary>Whether the text is 'unreadable' (characters change randomly).</summary>
    public bool Unreadable = false;

    /// <summary>Whether the text is colored randomly in a way that changes over time.</summary>
    public bool Random = false;

    /// <summary>Whether the text has a drop-shadow.</summary>
    public bool Shadow = false;

    /// <summary>What font this text renders with.</summary>
    public GLFont Font = null;

    /// <summary>A URL to open when this text is clicked.</summary>
    public string ClickURL = null;

    /// <summary>Text to display when a mouse is hovered over this text.</summary>
    public RenderableText HoverText = null;

    /// <summary>Applies the correct font from a font set.</summary>
    public void SetFontFrom(FontSet set)
    {
        if (SuperScript || SubScript)
        {
            if (Bold && Italic)
            {
                Font = set.FontBoldItalicHalf;
            }
            else if (Bold)
            {
                Font = set.FontBoldHalf;
            }
            else if (Italic)
            {
                Font = set.FontItalicHalf;
            }
            else
            {
                Font = set.FontHalf;
            }
        }
        else
        {
            if (Bold && Italic)
            {
                Font = set.FontBoldItalic;
            }
            else if (Bold)
            {
                Font = set.FontBold;
            }
            else if (Italic)
            {
                Font = set.FontItalic;
            }
            else
            {
                Font = set.FontDefault;
            }
        }
    }

    /// <summary>Returns a perfect copy of the part.</summary>
    public RenderableTextPart Clone() => MemberwiseClone() as RenderableTextPart;

    /// <summary>Returns a copy of the part with different text.</summary>
    /// <param name="text">The new part text.</param>
    public RenderableTextPart CloneWithText(string text)
    {
        RenderableTextPart cloned = Clone();
        cloned.Text = text;
        cloned.Width = Font.MeasureString(text);
        return cloned;
    }

    /// <summary>Implements <see cref="Object.ToString"/> to return the raw text.</summary>
    public override string ToString() => Text;
}

/// <summary>A mutable <see cref="RenderableTextLine"/> builder.</summary>
/// <param name="parts">A list of parts within the line.</param>
/// <param name="width">The line width.</param>
/// <param name="length">The total text length.</param>
/// <param name="whitespace">Whether the line is empty or whitespace.</param>
public class EditableTextLine(List<RenderableTextPart> parts, float width, int length, bool whitespace)
{
    /// <summary>The current list of parts within the line.</summary>
    public List<RenderableTextPart> Parts = parts;

    /// <summary>The current line width.</summary>
    public float Width = width;

    /// <summary>The current total text length.</summary>
    public int Length = length;

    /// <summary>Whether the line is empty or whitespace.</summary>
    public bool IsWhitespace = whitespace;

    /// <summary>Constructs an empty <see cref="EditableTextLine"/>.</summary>
    /// <param name="whitespace">Whether the line is empty or whitespace.</param>
    public EditableTextLine(bool whitespace = false) : this([], 0, 0, whitespace)
    {
    }

    /// <summary>Adds a text part to the line.</summary>
    /// <param name="part">The text part to add.</param>
    public void AddPart(RenderableTextPart part)
    {
        Parts.Add(part);
        Length += part.Text.Length;
        Width += part.Width;
        if (IsWhitespace && !string.IsNullOrWhiteSpace(part.Text))
        {
            IsWhitespace = false;
        }
    }

    /// <summary>Appends another text line.</summary>
    /// <param name="line">The text line to add.</param>
    public void AddLine(EditableTextLine line)
    {
        Parts.AddRange(line.Parts);
        Length += line.Length;
        Width += line.Width;
        if (IsWhitespace && !line.IsWhitespace)
        {
            IsWhitespace = false;
        }
    }

    /// <summary>Builds a new <see cref="RenderableTextLine"/> from the editable values.</summary>
    public RenderableTextLine ToRenderable() => new([.. Parts]);
}
