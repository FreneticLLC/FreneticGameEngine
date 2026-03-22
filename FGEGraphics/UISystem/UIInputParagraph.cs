//
// This file is part of the Frenetic Game Engine, created by Frenetic LLC.
// This code is Copyright (C) Frenetic LLC under the terms of a strict license.
// See README.md or LICENSE.txt in the FreneticGameEngine source root for the contents of the license.
// If neither of these are available, assume that neither you nor anyone other than the copyright holder
// hold any right or permission to use this software until such time as the official license is identified.
//

using FGECore.MathHelpers;
using FGEGraphics.GraphicsHelpers;
using FGEGraphics.GraphicsHelpers.FontSets;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FGEGraphics.UISystem;

/// <summary>Represents a <see cref="UIParagraph"/> designed to display the state of text during editing. Primarily used for <see cref="UIInputLabel"/>s</summary>
/// <remarks>
/// This paragraph contains three <see cref="UILabel"/>s. 
/// The left and right labels display content on either side of the cursor position, while the center label specifically displays highlighted text.
/// </remarks>
public class UIInputParagraph : UIParagraph
{
    /// <inheritdoc/>
    public override string Name => "Input Paragraph";

    /// <summary>The styling for text outside of a selection.</summary>
    public UIStyling TextStyling;

    /// <summary>The styling for text inside of a selection.</summary>
    public UIStyling HighlightStyling;

    /// <summary>Gets or sets the content of this paragraph.</summary>
    /// <remarks>This operation calls <see cref="UpdateRenderState"/>.</remarks>
    public new string Content
    {
        get => InputInternal.Content;
        set
        {
            SetContent(value);
            UpdateRenderState();
        }
    }

    /// <summary>The start cursor position. Acts as an anchorpoint for the end cursor.</summary>
    public int CursorStart = 0;

    /// <summary>The end cursor position.</summary>
    public int CursorEnd = 0;

    /// <summary>Whether to render the cursor when no text is selected.</summary>
    public bool RenderCursor = false;

    /// <summary>The minimum cursor position.</summary>
    public int CursorLeft => CursorStart < CursorEnd ? CursorStart : CursorEnd;

    /// <summary>The maximum cursor position.</summary>
    public int CursorRight => CursorEnd > CursorStart ? CursorEnd : CursorStart;

    /// <summary>Whether a non-empty string of text is selected between the cursor positions.</summary>
    public bool HasSelection => CursorStart != CursorEnd;

    // TODO: Selection?

    /// <summary>Data internal to a <see cref="UIInputParagraph"/> instance.</summary>
    public new struct InternalData()
    {
        /// <summary>The paragraph text content.</summary>
        public string Content = "";

        /// <summary>The label to the left of the cursor.</summary>
        public UILabel LabelLeft;

        /// <summary>The label between the left and right cursor positions. This label is empty if both cursor positions are equal</summary>
        public UILabel LabelCenter;

        /// <summary>The label to the right of the cursor.</summary>
        public UILabel LabelRight;

        /// <summary>The screen-space position of the cursor relative to the paragraph, or <see cref="Location.NaN"/> if no cursor should be drawn.</summary>
        public Location CursorRenderOffset = Location.NaN;
    }

    /// <summary>Data internal to a <see cref="UIInputParagraph"/> instance.</summary>
    public InternalData InputInternal = new();

    /// <summary>Constructs a new <see cref="UIInputParagraph"/>.</summary>
    /// <param name="styling">The styling of the paragraph, used when drawing the cursor.</param>
    /// <param name="textStyling">The styling for non-selected text.</param>
    /// <param name="highlightStyling">The styling for selected, or highlighted, text.</param>
    /// <param name="layout">The layout of this element.</param>
    public UIInputParagraph(UIStyling styling, UIStyling textStyling, UIStyling highlightStyling, UILayout layout) : base(layout)
    {
        Styling = styling;
        TextStyling = textStyling;
        HighlightStyling = highlightStyling;
        AddLabel(InputInternal.LabelLeft = new UILabel("", textStyling, new UILayout()));
        AddLabel(InputInternal.LabelCenter = new UILabel("", highlightStyling, new UILayout()));
        AddLabel(InputInternal.LabelRight = new UILabel("", textStyling, new UILayout()));
    }

    /// <summary>Ensures the cursor positions lie within the current paragraph <see cref="Content"/>.</summary>
    public void ClampCursorPositions()
    {
        CursorStart = Math.Clamp(CursorStart, 0, Content.Length);
        CursorEnd = Math.Clamp(CursorEnd, 0, Content.Length);
    }

    /// <summary>Sets both cursor positions to one index.</summary>
    public void SetCursorPosition(int cursorPosition)
    {
        CursorStart = CursorEnd = cursorPosition;
    }

    /// <summary>Sets the raw paragraph text content.</summary>
    /// <remarks>This operation does <b>not</b> call <see cref="UpdateRenderState"/>.</remarks>
    public void SetContent(string content)
    {
        InputInternal.Content = content ?? "";
        ClampCursorPositions();
    }

    /// <summary>Updates the text content of the three internal labels based on the cursor positions.</summary>
    public void UpdateLabelContent()
    {
        InputInternal.LabelLeft.Content = Content[..CursorLeft];
        InputInternal.LabelCenter.Content = Content[CursorLeft..CursorRight];
        InputInternal.LabelRight.Content = Content[CursorRight..];
    }

    /// <summary>Prepares this paragraph for rendering.</summary>
    public void UpdateRenderState()
    {
        UpdateLabelContent();
        UpdateRenderables();
        InputInternal.CursorRenderOffset = RenderCursor ? GetCursorRenderOffset() : Location.NaN;
    }

    // TODO: Account for formatting codes
    // TODO: out of range exception here somewhere
    /// <summary>Returns the cursor's screen-space position relative to this element based on the cursor index position.</summary>
    public Location GetCursorRenderOffset()
    {
        double xOffset = 0;
        int cursorIndex = CursorEnd;
        int currentIndex = 0;
        cursorIndex -= Internal.Renderables.Sum(piece => piece.SkippedIndices.Count(index => index <= cursorIndex));
        for (int i = 0; i < Internal.Renderables.Count; i++)
        {
            UIParagraph.InternalData.Renderable piece = Internal.Renderables[i];
            for (int j = 0; j < piece.Text.Lines.Length; j++)
            {
                RenderableTextPart[] parts = piece.Text.Lines[j].Parts;
                if (parts.Length == 0 && i == Internal.Renderables.Count - 1)
                {
                    return new Location(0, piece.YOffset, 0);
                }
                foreach (RenderableTextPart part in parts)
                {
                    if (currentIndex + part.Text.Length < cursorIndex)
                    {
                        currentIndex += part.Text.Length;
                        xOffset += part.Width;
                        continue;
                    }
                    int relIndex = cursorIndex - currentIndex;
                    double x = xOffset + (part.Text.Length > 0 ? piece.Font.MeasureFancyText(part.Text[..relIndex]) : 0);
                    double y = piece.YOffset + j * piece.Font.Height;
                    return new Location(x, y, 0);
                }
                xOffset = 0;
            }
            currentIndex++;
        }
        return Location.Zero;
    }

    /// <inheritdoc/>
    public override void Render(double delta, UIStyle style)
    {
        base.Render(delta, style);
        if (!RenderCursor || InputInternal.CursorRenderOffset.IsNaN())
        {
            return;
        }
        View.Engine.Textures.White.Bind();
        Renderer2D.SetColor(style.BorderColor);
        int lineWidth = style.BorderThickness / 2;
        int lineHeight = style.TextFont.Height;
        View.Rendering.RenderRectangle(View.UIContext, X + InputInternal.CursorRenderOffset.XF - lineWidth, Y + InputInternal.CursorRenderOffset.YF, X + InputInternal.CursorRenderOffset.XF + lineWidth, Y + InputInternal.CursorRenderOffset.YF + lineHeight);
        Renderer2D.SetColor(Color4.White);
    }

    /// <inheritdoc/>
    public override List<string> GetDebugInfo() => [$"^7Indices: ^3[{CursorLeft} {CursorRight}] ^&| ^7Cursors: ^3[{CursorStart} {CursorEnd}] ^&| ^7Cursor Offset: ^3{InputInternal.CursorRenderOffset}"];
}
