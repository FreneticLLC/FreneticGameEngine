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
using FreneticUtilities.FreneticExtensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FGEGraphics.UISystem;

/// <summary>Represents a simple piece of text on a screen.</summary>
public class UILabel2 : UIElement
{
    /// <summary>Whether the label is empty and shouldn't be rendered.</summary>
    public bool IsEmpty => Internal.Content.Length == 0;

    /// <summary>Data internal to a <see cref="UILabel2"/> instance.</summary>
    public struct InternalData()
    {
        /// <summary>The label text content.</summary>
        public string Content;

        /// <summary>The maximum width of the text content.</summary>
        public int MaxWidth;

        /// <summary>A cache of UI styles and their corresponding renderable objects.</summary>
        public Dictionary<UIStyle, RenderableText> Renderables = [];
    }

    /// <summary>Data internal to a <see cref="UILabel2"/> instance.</summary>
    public InternalData Internal;

    /// <summary>
    /// Gets or sets the label text content.
    /// <b>Note:</b> setting this value recomputes the <see cref="RenderableText"/> cache.
    /// </summary>
    public string Content
    {
        get => Internal.Content;
        set
        {
            Internal.Content = value ?? "";
            UpdateRenderables();
        }
    }

    /// <summary>
    /// Gets or sets the maximum width of the text content.
    /// <b>Note:</b> setting this value recomputes the <see cref="RenderableText"/> cache.
    /// </summary>
    public int MaxWidth
    {
        get => Internal.MaxWidth;
        set
        {
            Internal.MaxWidth = value;
            UpdateRenderables();
        }
    }

    /// <summary>Constructs a new label.</summary>
    /// <param name="text">The text to display on the label.</param>
    /// <param name="styling">The style of the label.</param>
    /// <param name="layout">The layout of the element.</param>
    public UILabel2(string text, UIStyling styling, UILayout layout) : base(styling, layout)
    {
        Internal = new() { Content = text ?? "" };
    }

    /// <summary>Creates a <see cref="RenderableText"/> object from <see cref="Content"/> given a style.</summary>
    /// <param name="style">The UI style to use.</param>
    /// <returns>The resulting renderable object.</returns>
    public RenderableText CreateRenderable(UIStyle style)
    {
        int fontSize = (int)(style.TextFont.Size * Scale);
        if (fontSize == 0)
        {
            return RenderableText.Empty;
        }
        // TODO: cache this somewhere, as it's likely for many elements with text to have the same scale value
        IEnumerable<KeyValuePair<(string, int), FontSet>> fontVariants = style.TextFont.Engine.Fonts.Where(pair => pair.Value.Name == style.TextFont.Name);
        if (!fontVariants.Any())
        {
            return RenderableText.Empty;
        }
        IEnumerable<KeyValuePair<(string, int), FontSet>> fittingFonts = fontVariants.Where(pair => pair.Key.Item2 <= fontSize);
        ((string, int) _, FontSet font) = fittingFonts.Any() ? fittingFonts.MinBy(pair => fontSize - pair.Key.Item2) : fontVariants.MinBy(pair => Math.Abs(fontSize - pair.Key.Item2));
        string styledContent = style.TextStyling(Internal.Content); // FIXME: this doesn't play well with translatable text.
        RenderableText renderable = font.ParseFancyText(styledContent, style.TextBaseColor);
        if (Internal.MaxWidth > 0)
        {
            renderable = FontSet.SplitAppropriately(renderable, Internal.MaxWidth);
        }
        return renderable;
    }

    /// <summary>
    /// Updates the <see cref="RenderableText"/> cache. 
    /// If <see cref="IsEmpty"/> is <c>true</c>, clears the cache.
    /// Otherwise, recreates all renderable objects in the cache.
    /// </summary>
    public void UpdateRenderables()
    {
        if (IsEmpty)
        {
            Internal.Renderables.Clear();
        }
        else
        {
            Internal.Renderables = Internal.Renderables.Keys
                .Select(style => (style, CreateRenderable(style)))
                .ToDictionary();
        }
    }

    /// <inheritdoc/>
    public override void StyleChanged(UIStyle from, UIStyle to)
    {
        if (!IsEmpty && to.CanRenderText && !Internal.Renderables.ContainsKey(to))
        {
            Internal.Renderables[to] = CreateRenderable(to);
        }
        if (Scale != 0 && Internal.Renderables.TryGetValue(to, out RenderableText renderable))
        {
            Layout.SetSize(renderable.GetTrueSize(to.TextFont));
        }
    }

    /// <inheritdoc/>
    public override void ScaleChanged(float from, float to)
    {
        UpdateRenderables();
        if (from == 0 && GetRenderable(Style) is RenderableText renderable)
        {
            Layout.SetSize(renderable.GetTrueSize(Style.TextFont));
        }
    }

    /// <summary>Returns the cached <see cref="RenderableText"/> object corresponding to the given style, or <c>null</c> if none is present.</summary>
    /// <param name="style">The UI style to query.</param>
    public RenderableText GetRenderable(UIStyle style) => !IsEmpty && Internal.Renderables.TryGetValue(style, out RenderableText renderable) ? renderable : null;

    /// <inheritdoc/>
    public override void Render(double delta, UIStyle style)
    {
        if (GetRenderable(style) is RenderableText renderable)
        {
            int trueX = X + Layout.Anchor.AlignmentX.GetPosition(Width, renderable.Width);
            int trueY = Y + Layout.Anchor.AlignmentY.GetPosition(Height, renderable.Height);
            style.TextFont.DrawFancyText(renderable, new Location(trueX, trueY, 0));
        }
    }

    /// <summary>Constructs a label with an icon attached at the side.</summary>
    /// <param name="text">The text to display on the label.</param>
    /// <param name="icon">The texture of the icon.</param>
    /// <param name="spacing">The space between the label and icon.</param>
    /// <param name="styling">The styling of the label.</param>
    /// <param name="layout">The layout of the element.</param>
    /// <param name="listAnchor">The anchor to use when positioning the label and the icon in a list.</param>
    /// <returns>A tuple of the label, icon, and their list container.</returns>
    public static (UILabel2 Label, UIImage Icon, UIListGroup List) WithIcon(string text, Texture icon, int spacing, UIStyling styling, UILayout layout, UIAnchor listAnchor = null)
    {
        UIListGroup list = new(spacing, layout, vertical: false, anchor: listAnchor ?? UIAnchor.TOP_LEFT);
        UILabel2 label = new(text, styling, layout.AtOrigin());
        UIImage image = new(icon, new UILayout().SetSize(() => label.Height, () => label.Height));
        list.AddListItem(label);
        list.AddListItem(image);
        return (label, image, list);
    }
}