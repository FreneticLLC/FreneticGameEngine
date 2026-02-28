//
// This file is part of the Frenetic Game Engine, created by Frenetic LLC.
// This code is Copyright (C) Frenetic LLC under the terms of a strict license.
// See README.md or LICENSE.txt in the FreneticGameEngine source root for the contents of the license.
// If neither of these are available, assume that neither you nor anyone other than the copyright holder
// hold any right or permission to use this software until such time as the official license is identified.
//

using System.Collections.Generic;

namespace FGEGraphics.GraphicsHelpers.FontSets;

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
