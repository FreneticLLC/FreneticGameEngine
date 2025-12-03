//
// This file is part of the Frenetic Game Engine, created by Frenetic LLC.
// This code is Copyright (C) Frenetic LLC under the terms of a strict license.
// See README.md or LICENSE.txt in the FreneticGameEngine source root for the contents of the license.
// If neither of these are available, assume that neither you nor anyone other than the copyright holder
// hold any right or permission to use this software until such time as the official license is identified.
//

using FGECore.CoreSystems;
using FGECore.MathHelpers;
using FGEGraphics.GraphicsHelpers.FontSets;
using FGEGraphics.GraphicsHelpers.Textures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FGEGraphics.UISystem;

/// <summary>Represents a simple piece of text on a screen.</summary>
public class UILabel2 : UIElement
{
    /// <summary>The text to display on this label.</summary>
    //public UIText Text;

    public string Content
    {
        get => Internal.Content;
        set
        {
            Internal.Content = value ?? "";
            UpdateRenderables();
        }
    }

    /// <summary>Whether the text is empty and shouldn't be rendered.</summary>
    public bool IsEmpty => Internal.Content.Length == 0;

    /// <summary>Data internal to a <see cref="UILabel2"/> instance.</summary>
    public struct InternalData()
    {
        public string Content;

        /// <summary>The maximum total width of this text, if any.</summary>
        public int MaxWidth;

        public Dictionary<UIStyle, RenderableText> Renderables = [];
    }

    public InternalData Internal;

    /// <summary>Constructs a new label.</summary>
    /// <param name="text">The text to display on the label.</param>
    /// <param name="styling">The style of the label.</param>
    /// <param name="layout">The layout of the element.</param>
    public UILabel2(string text, UIStyling styling, UILayout layout) : base(styling, layout)
    {
        ScaleSize = false;
        Internal = new() { Content = text ?? "" };
        //Text = new UIText(this, text, true, Layout.Width);
        // TODO: cache size
        Layout.SetSize(() => GetSize().X, () => GetSize().Y); // TODO: padding
    }

    /// <summary>Creates a <see cref="RenderableText"/> of the text <see cref="Content"/> given a style.</summary>
    /// <param name="style">The UI style to use.</param>
    /// <returns>The resulting renderable object.</returns>
    public RenderableText CreateRenderable(UIStyle style)
    {
        string styledContent = style.TextStyling(Internal.Content); // FIXME: this doesn't play well with translatable text.
        int fontSize = (int)(style.TextFont.Size * Scale);
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

    public void UpdateRenderables()
    {
        if (IsEmpty)
        {
            Internal.Renderables.Clear();
            return;
        }
        Internal.Renderables = Internal.Renderables.Keys
            .Select(style => (style, CreateRenderable(style)))
            .ToDictionary();
    }

    public override void StyleChanged(UIStyle from, UIStyle to)
    {
        Logs.Debug($"style changed; {Content} {IsEmpty} {to.CanRenderText} {Internal.Renderables.ContainsKey(to)}");
        if (!IsEmpty && to.CanRenderText && !Internal.Renderables.ContainsKey(to))
        {
            Logs.Debug("- creating renderable");
            Internal.Renderables[to] = CreateRenderable(to);
        }
    }

    public override void ScaleChanged(float from, float to)
    {
        UpdateRenderables();
    }

    public RenderableText Renderable => !IsEmpty && Internal.Renderables.TryGetValue(Style, out RenderableText renderable) ? renderable : RenderableText.Empty;

    //public bool CanRender => !IsEmpty && Style.CanRenderText && (Style == style || (Internal.Renderables?.ContainsKey(style) ?? false));

    public Vector2i GetSize()
    {
        RenderableText renderable = Renderable;
        if (renderable.IsEmpty)
        {
            return Vector2i.Zero;
        }
        int trueHeight = (int)(Style.FontHeight * renderable.Lines.Length * Scale);
        int trueWidth = (int)((float)trueHeight / renderable.Height * Renderable.Width);
        return new Vector2i(trueWidth, trueHeight);
    }

    /// <inheritdoc/>
    public override void Render(double delta, UIStyle style)
    {
        if (!IsEmpty)
        {
            Vector2i trueSize = GetSize();
            int trueX = X + (trueSize.X - Renderable.Width) / 2;
            int trueY = Y + (trueSize.Y - Renderable.Height) / 2;
            style.TextFont.DrawFancyText(Renderable, new Location(trueX, trueY, 0));
        }
        //if (Text.CanBeRenderedBy(style))
        //{
        //    Text.Render(style, X, Y);
        //}
    }
}