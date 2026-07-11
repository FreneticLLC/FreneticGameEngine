//
// This file is part of the Frenetic Game Engine, created by Frenetic LLC.
// This code is Copyright (C) Frenetic LLC under the terms of a strict license.
// See README.md or LICENSE.txt in the FreneticGameEngine source root for the contents of the license.
// If neither of these are available, assume that neither you nor anyone other than the copyright holder
// hold any right or permission to use this software until such time as the official license is identified.
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FGECore.MathHelpers;
using FGEGraphics.ClientSystem;
using FGEGraphics.GraphicsHelpers;
using FGEGraphics.GraphicsHelpers.Textures;
using OpenTK;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace FGEGraphics.UISystem;

/// <summary>Represents a simple box on the screen.</summary>
public class UIBox : UIElement
{
    /// <inheritdoc/>
    public override string Name => OnClick is not null ? "Button" : "Box";

    // TODO: move to UILayout
    /// <summary>Whether this box is vertically flipped.</summary>
    public bool Flip = false;

    /// <summary>The text to display inside this box.</summary>
    public UILabel Label;

    /// <summary>Constructs a <see cref="UIBox"/>.</summary>
    /// <param name="styling">The styling of the element.</param>
    /// <param name="layout">The layout of the element.</param>
    /// <param name="text">Text to display inside the box.</param>
    public UIBox(UIStyling styling, UILayout layout, string text = null) : base(styling, layout)
    {
        AddChild(Label = new UILabel(text, styling?.Bind(this), new UILayout().SetAnchor(UIAnchor.CENTER)) { IsEnabled = false });
    }

    /// <inheritdoc/>
    public override void Render(double delta, UIStyle style)
    {
        Vector3 rotation = new(-0.5f, -0.5f, Rotation);
        bool any = style.ShadowSize > 0 || style.Stroke.A > 0 || style.Fill.A > 0;
        if (any)
        {
            View.Engine.Textures.White.Bind();
            if (style.ShadowSize > 0)
            {
                Renderer2D.SetColor(new Color4F(0, 0, 0, 0.5f));
                View.Rendering.RenderRectangle(View.UIContext, X, Y, X + Width + style.ShadowSize, Y + Height + style.ShadowSize, rotation);
            }
            if (style.Stroke.A > 0 && style.StrokeWeight > 0)
            {
                Renderer2D.SetColor(style.Stroke);
                View.Rendering.RenderRectangle(View.UIContext, X, Y, X + Width, Y + Height, rotation);
            }
            if (style.Fill.A > 0)
            {
                Renderer2D.SetColor(style.Fill);
                View.Rendering.RenderRectangle(View.UIContext, X + style.StrokeWeight, Y + style.StrokeWeight, X + Width - style.StrokeWeight, Y + Height - style.StrokeWeight, rotation);
            }
            Renderer2D.SetColor(Color4F.White);
        }
        if (style.Texture is not null)
        {
            style.Texture.Bind();
            float ymin = Flip ? Y + Height : Y;
            float ymax = Flip ? Y : Y + Height;
            View.Rendering.RenderRectangle(View.UIContext, X, ymin, X + Width, ymax, rotation);
        }
    }
}
