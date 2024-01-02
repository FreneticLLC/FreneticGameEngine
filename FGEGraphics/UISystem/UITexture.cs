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
using FGEGraphics.UISystem;
using OpenTK.Graphics.OpenGL4;

namespace FGEGraphics.UISystem;

/// <summary>
/// Represents a GL native texture on a screen.
/// <para>To render <see cref="Texture"/> instances, use <see cref="UIImage"/>.</para>
/// </summary>
/// <param name="texture">The texture to display.</param>
/// <param name="pos">The position of the element.</param>
/// <param name="shouldRender">Whether the element should render automatically.</param>
public class UITexture(Func<int> texture, UIPositionHelper pos, bool shouldRender = true) : UIElement(pos, shouldRender)
{
    /// <summary>The texture to display.</summary>
    public Func<int> Texture = texture;

    /// <summary>Renders the texture on a screen.</summary>
    /// <param name="view">The UI view.</param>
    /// <param name="delta">The time since the last render.</param>
    /// <param name="style">The current element style.</param>
    public override void Render(ViewUI2D view, double delta, UIElementStyle style)
    {
        GL.BindTexture(TextureTarget.Texture2D, Texture());
        view.Rendering.RenderRectangle(view.UIContext, X, Y, X + Width, Y + Height, new OpenTK.Mathematics.Vector3(-0.5f, -0.5f, LastAbsoluteRotation));
    }
}
