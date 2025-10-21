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
using FGEGraphics.ClientSystem;
using FGEGraphics.GraphicsHelpers;
using FGEGraphics.UISystem;
using OpenTK.Graphics.OpenGL4;

namespace FGEGraphics.UISystem;

/// <summary>
/// Represents a GL native texture on a screen.
/// <para>To render <see cref="GraphicsHelpers.Textures.Texture"/> instances, use <see cref="UIImage"/>.</para>
/// </summary>
/// <param name="texture">The texture to display.</param>
/// <param name="layout">The layout of the element.</param>
public class UINativeTexture(Func<GraphicsUtil.TrackedTexture> texture, UILayout layout) : UIElement(UIStyling.Empty, layout)
{
    /// <summary>The texture to display.</summary>
    public Func<GraphicsUtil.TrackedTexture> Texture = texture;

    /// <summary>Whether the texture is flipped vertically.</summary>
    // TODO: Put transforms on UILayout
    public bool Flip;

    /// <inheritdoc/>
    public override void Render(double delta, UIStyle style)
    {
        Texture().Bind();
        float ymin = Flip ? Y + Height : Y;
        float ymax = Flip ? Y : Y + Height;
        View.Rendering.RenderRectangle(View.UIContext, X, ymin, X + Width, ymax, new OpenTK.Mathematics.Vector3(-0.5f, -0.5f, Rotation));
    }
}
