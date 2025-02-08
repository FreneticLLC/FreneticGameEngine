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
/// <remarks>Constructs a simple colored box.</remarks>
/// <param name="style">The style of the element.</param>
/// <param name="pos">The position of the element.</param>
public class UIBox(UIElementStyle style, UIPositionHelper pos) : UIElement(pos)
{
    /// <summary>The style of the box.</summary>
    public override UIElementStyle Style { get; set; } = style;
    
    // TODO: put this on UIPositionHelper or something
    /// <summary>Whether this box is vertically flipped.</summary>
    public bool Flip = false;

    /// <summary>Renders this box on the screen.</summary>
    /// <param name="view">The UI view.</param>
    /// <param name="delta">The time since the last render.</param>
    /// <param name="style">The current element style.</param>
    public override void Render(ViewUI2D view, double delta, UIElementStyle style)
    {
        Vector3 rotation = new(-0.5f, -0.5f, LastAbsoluteRotation);
        bool any = style.DropShadowLength > 0 || style.BorderColor.A > 0 || style.BaseColor.A > 0;
        if (any)
        {
            Engine.Textures.White.Bind();
            if (style.DropShadowLength > 0)
            {
                Renderer2D.SetColor(new Color4F(0, 0, 0, 0.5f));
                view.Rendering.RenderRectangle(view.UIContext, X, Y, X + Width + style.DropShadowLength, Y + Height + style.DropShadowLength, rotation);
            }
            if (style.BorderColor.A > 0 && style.BorderThickness > 0)
            {
                Renderer2D.SetColor(style.BorderColor);
                view.Rendering.RenderRectangle(view.UIContext, X, Y, X + Width, Y + Height, rotation);
            }
            if (style.BaseColor.A > 0)
            {
                Renderer2D.SetColor(style.BaseColor);
                view.Rendering.RenderRectangle(view.UIContext, X + style.BorderThickness, Y + style.BorderThickness, X + Width - style.BorderThickness, Y + Height - style.BorderThickness, rotation);
            }
            Renderer2D.SetColor(Color4F.White);
        }
        if (style.BaseTexture is not null)
        {
            style.BaseTexture.Bind();
            float ymin = Flip ? Y + Height : Y;
            float ymax = Flip ? Y : Y + Height;
            view.Rendering.RenderRectangle(view.UIContext, X, ymin, X + Width, ymax, rotation);
        }
    }
}
