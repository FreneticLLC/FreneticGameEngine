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
using FGEGraphics.GraphicsHelpers.Textures;
using OpenTK;
using OpenTK.Mathematics;

namespace FGEGraphics.UISystem;

/// <summary>Represents a simple image on a screen.</summary>
/// <remarks>Constructs an image.</remarks>
/// <param name="image">The image to display.</param>
/// <param name="pos">The position of the element.</param>
public class UIImage(Texture image, UIPositionHelper pos) : UIElement(pos)
{
    /// <summary>The image to display.</summary>
    public Texture Image = image;

    /// <summary>Renders the image on a screen.</summary>
    /// <param name="view">The UI view.</param>
    /// <param name="delta">The time since the last render.</param>
    /// <param name="style">The current element style.</param>
    public override void Render(ViewUI2D view, double delta, UIElementStyle style)
    {
        Image.Bind();
        view.Rendering.RenderRectangle(view.UIContext, X, Y, X + Width, Y + Height, new Vector3(-0.5f, -0.5f, LastAbsoluteRotation));
    }
}
