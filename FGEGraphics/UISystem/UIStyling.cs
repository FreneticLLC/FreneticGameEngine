using FGECore.ConsoleHelpers;
using FGECore.MathHelpers;
using FGEGraphics.GraphicsHelpers.FontSets;
using FGEGraphics.GraphicsHelpers.Textures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FGEGraphics.UISystem;

public record UIStyling
{
    public UIElement Element = null;

    public UIStyleValue<Color4F> Fill = Color4F.Transparent;

    public UIStyleValue<Texture> Texture = default;

    public UIStyleValue<Color4F> Stroke = Color4F.Transparent;

    public UIStyleValue<int> StrokeWeight = 0;

    public UIStyleValue<int> Padding = 0;

    public UIStyleValue<int> ShadowSize = 0;

    public UIStyleValue<FontSet> TextFont = default;

    public UIStyleValue<Func<string, string>> TextStyling = default;

    public UIStyleValue<string> TextBaseColor = TextStyle.Simple;

    public UIStyle Get(UIElement element) {
        element = Element ?? element;
        return new()
        {
            Fill = Fill.Get(element),
            Texture = Texture.Get(element),
            Stroke = Stroke.Get(element),
            StrokeWeight = StrokeWeight.Get(element),
            Padding = Padding.Get(element),
            ShadowSize = ShadowSize.Get(element),
            TextFont = TextFont.Get(element),
            TextStyling = TextStyling.Get(element),
            TextBaseColor = TextBaseColor.Get(element)
        };
    }

    public UIStyling Bind(UIElement element) => this with { Element = element };
}
