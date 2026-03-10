using FGECore.CoreSystems;
using FGECore.MathHelpers;
using FGEGraphics.GraphicsHelpers;
using FGEGraphics.GraphicsHelpers.FontSets;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace FGEGraphics.UISystem;

public class UILabelChain(UIStyling styling, UILayout layout) : UIElement(styling, layout)
{
    public List<UILabel> Labels = [];

    public float MaxWidth = -1;

    public struct InternalData()
    {
        public List<Renderable> Renderables = [];

        public Location CursorOffset = Location.Zero;

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
    // TODO: Fix blank lines not being counted

    /// <summary>
    /// 
    /// </summary>
    public void UpdateRenderables()
    {
        Internal.Renderables.Clear();
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
        // todo: guard behind 'selected' or smth
        View.Engine.Textures.White.Bind();
        Renderer2D.SetColor(style.BorderColor);
        int lineWidth = style.BorderThickness / 2;
        int lineHeight = style.TextFont.Height;
        View.Rendering.RenderRectangle(View.UIContext, X + Internal.CursorOffset.XF - lineWidth, Y + Internal.CursorOffset.YF, X + Internal.CursorOffset.XF + lineWidth, Y + Internal.CursorOffset.YF + lineHeight);
        Renderer2D.SetColor(Color4.White);
    }
}
