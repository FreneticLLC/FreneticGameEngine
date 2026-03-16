//
// This file is part of the Frenetic Game Engine, created by Frenetic LLC.
// This code is Copyright (C) Frenetic LLC under the terms of a strict license.
// See README.md or LICENSE.txt in the FreneticGameEngine source root for the contents of the license.
// If neither of these are available, assume that neither you nor anyone other than the copyright holder
// hold any right or permission to use this software until such time as the official license is identified.
//

using BepuUtilities.Collections;
using FGECore.CoreSystems;
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

public class UIInputParagraph : UIParagraph
{
    public override string Name => "Input Paragraph";

    public UIStyling TextStyling;

    public UIStyling HighlightStyling;

    public new string Content
    {
        get => InputInternal.Content;
        set
        {
            InputInternal.Content = value ?? "";
            UpdateRenderState();
        }
    }

    /// <summary>The start cursor position. Acts as an anchorpoint for the end cursor.</summary>
    public int CursorStart = 0;

    /// <summary>The end cursor position.</summary>
    public int CursorEnd = 0;

    public bool RenderCursor = false;

    /// <summary>The minimum cursor position.</summary>
    public int CursorLeft => CursorStart < CursorEnd ? CursorStart : CursorEnd;

    /// <summary>The maximum cursor position.</summary>
    public int CursorRight => CursorEnd > CursorStart ? CursorEnd : CursorStart;

    /// <summary>Whether a string of text is selected between the indices.</summary>
    public bool HasSelection => CursorStart != CursorEnd;

    public struct InternalData()
    {
        public string Content = "";

        public UILabel LabelLeft;

        public UILabel LabelCenter;

        public UILabel LabelRight;

        public Location CursorRenderOffset = Location.NaN;
    }

    public InternalData InputInternal = new();

    public UIInputParagraph(UIStyling styling, UIStyling textStyling, UIStyling highlightStyling, UILayout layout) : base(styling, layout)
    {
        TextStyling = textStyling;
        HighlightStyling = highlightStyling;
        AddLabel(InputInternal.LabelLeft = new UILabel("", textStyling, new UILayout()));
        AddLabel(InputInternal.LabelCenter = new UILabel("", highlightStyling, new UILayout()));
        AddLabel(InputInternal.LabelRight = new UILabel("", textStyling, new UILayout()));
    }

    public void ClampCursorPositions()
    {
        CursorStart = Math.Clamp(CursorStart, 0, Content.Length);
        CursorEnd = Math.Clamp(CursorEnd, 0, Content.Length);
    }

    public void SetCursorPosition(int cursorPosition)
    {
        CursorStart = CursorEnd = cursorPosition;
    }

    public void SetContent(string content)
    {
        InputInternal.Content = content;
        ClampCursorPositions();
    }

    public void UpdateLabelContent()
    {
        InputInternal.LabelLeft.Content = Content[..CursorLeft];
        InputInternal.LabelCenter.Content = Content[CursorLeft..CursorRight];
        InputInternal.LabelRight.Content = Content[CursorRight..];
    }

    public void UpdateRenderState()
    {
        UpdateLabelContent();
        UpdateRenderables();
        InputInternal.CursorRenderOffset = RenderCursor ? GetCursorRenderOffset() : Location.NaN;
    }

    // TODO: update docs
    /// <summary>Calculates a screen cursor offset given the current <see cref="TextChain"/>, or <see cref="Location.NaN"/> if none.</summary>
    // TODO: Account for formatting codes
    // TODO: out of range exception here somewhere
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
