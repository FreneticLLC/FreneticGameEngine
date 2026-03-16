//
// This file is part of the Frenetic Game Engine, created by Frenetic LLC.
// This code is Copyright (C) Frenetic LLC under the terms of a strict license.
// See README.md or LICENSE.txt in the FreneticGameEngine source root for the contents of the license.
// If neither of these are available, assume that neither you nor anyone other than the copyright holder
// hold any right or permission to use this software until such time as the official license is identified.
//

using FGECore.CoreSystems;
using FGECore.MathHelpers;
using FGEGraphics.GraphicsHelpers;
using FGEGraphics.GraphicsHelpers.FontSets;
using FreneticUtilities.FreneticExtensions;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace FGEGraphics.UISystem;

public class UIParagraph(UIStyling styling, UILayout layout) : UIElement(styling, layout)
{
    public List<UILabel> Labels = [];

    public float MaxWidth = -1;

    // TODO: Content
    public string Content => string.Join("", Labels.Select(label => label.Content));

    public struct InternalData()
    {
        public List<Renderable> Renderables = [];

        /// <summary>An individual UI text chain piece.</summary>
        /// <param name="Font">The font to render the chain piece with.</param>
        /// <param name="Text">The chain piece text.</param>
        /// <param name="YOffset">The y-offset relative to the first piece.</param>
        /// <param name="SkippedIndices">A list of character indices ignored in <see cref="FontSet.SplitLineAppropriately(RenderableTextLine, float, out List{int})"/>.</param>
        public record Renderable(FontSet Font, RenderableText Text, float YOffset, List<int> SkippedIndices);
    }

    public InternalData Internal = new();

    public void AddLabel(UILabel label)
    {
        Labels.Add(label);
        AddChild(label);
        label.RenderSelf = false;
        //label.Internal.OnRenderablesUpdate += UpdateRenderables;
    }

    // old docs:
    /// <summary>
    /// Iterates through some UI text objects and returns <see cref="ChainPiece"/>s, where each chain piece contains a single line.
    /// This properly handles consecutive text objects even spanning multiple lines.
    /// </summary>
    /// <param name="chain">The UI text objects.</param>
    /// <param name="maxWidth">The wrapping width of the chain.</param>
    /// <returns>The text chain.</returns>

    /// <summary>
    /// 
    /// </summary>
    public void UpdateRenderables()
    {
        Internal.Renderables.Clear();
        Layout.SetSize(0, 0);
        List<(FontSet Font, RenderableTextLine Line)> lines = [];
        foreach (UILabel label in Labels)
        {
            if (label.GetRenderable(label.Style) is not RenderableText renderable)
            {
                continue;
            }
            List<RenderableTextLine> textLines = [.. renderable.Lines];
            if (lines.Count != 0)
            {
                RenderableTextLine combinedLine = new([.. lines[^1].Line.Parts, .. textLines[0].Parts]);
                lines[^1] = (lines[^1].Font, combinedLine);
                textLines.RemoveAt(0);
            }
            foreach (RenderableTextLine line in textLines)
            {
                lines.Add((label.Style.TextFont, line));
            }
        }
        int width = 0;
        float y = 0;
        foreach ((FontSet font, RenderableTextLine line) in lines)
        {
            List<int> skippedIndices = null;
            RenderableText splitText = MaxWidth > 0 ? FontSet.SplitLineAppropriately(line, MaxWidth, out skippedIndices) : new([line]);
            Internal.Renderables.Add(new(font, splitText, y, skippedIndices ?? []));
            y += font.Height * splitText.Lines.Length;
            if (splitText.Width > width)
            {
                width = splitText.Width;
            }
        }
        if (Internal.Renderables.Count > 0)
        {
            Layout.SetWidth(width).SetHeight((int) y);
        }
    }

    /// <inheritdoc/>
    public override void Render(double delta, UIStyle style)
    {
        GraphicsUtil.CheckError("UIElementText - PreRenderChain");
        foreach (InternalData.Renderable renderable in Internal.Renderables)
        {
            renderable.Font.DrawFancyText(renderable.Text, new Location(X, Y + renderable.YOffset, 0));
        }
    }

    public int GetIndexForLocation(int relX, int relY)
    {
        InternalData.Renderable lastRenderable = Internal.Renderables[^1];
        if (lastRenderable.YOffset + (lastRenderable.Font.Height * lastRenderable.Text.Lines.Length) < relY)
        {
            return Content.Length;
        }
        int indexOffset = 0;
        foreach (InternalData.Renderable renderable in Internal.Renderables)
        {
            for (int i = 0; i < renderable.Text.Lines.Length; i++)
            {
                RenderableTextLine line = renderable.Text.Lines[i];
                string content = line.ToString();
                if (renderable.YOffset + (i + 1) * renderable.Font.Height >= relY)
                {
                    if (line.Width < relX)
                    {
                        return indexOffset + content.Length;
                    }
                    float lastWidth = 0;
                    for (int j = 0; j <= content.Length; j++)
                    {
                        float width = renderable.Font.MeasureFancyText(content[..j]);
                        if (width >= relX)
                        {
                            int diff = relX - lastWidth >= width - relX ? 0 : 1;
                            return indexOffset + j - diff;
                        }
                        lastWidth = width;
                    }
                }
                indexOffset += content.Length;
            }
            indexOffset++;
        }
        return -1;
    }
}
