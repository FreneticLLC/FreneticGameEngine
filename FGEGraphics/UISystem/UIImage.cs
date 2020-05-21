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

namespace FGEGraphics.UISystem
{
    /// <summary>
    /// Represents a simple image on a screen.
    /// </summary>
    public class UIImage : UIElement
    {
        /// <summary>
        /// The image to display.
        /// </summary>
        public Texture Image;

        /// <summary>
        /// Constructs an image.
        /// </summary>
        /// <param name="image">The image to display.</param>
        /// <param name="pos">The position of the element.</param>
        public UIImage(Texture image, UIPositionHelper pos)
            : base(pos)
        {
            Image = image;
        }

        /// <summary>
        /// Renders the image on a screen.
        /// </summary>
        /// <param name="view">The UI view.</param>
        /// <param name="delta">The time since the last render.</param>
        public override void Render(ViewUI2D view, double delta)
        {
            if (Image != null)
            {
                Image.Bind();
                int x = LastAbsolutePosition.X;
                int y = LastAbsolutePosition.Y;
                float width = LastAbsoluteSize.X;
                float height = LastAbsoluteSize.Y;
                view.Rendering.RenderRectangle(view.UIContext, x, y, x + width, y + height, new Vector3(-0.5f, -0.5f, LastAbsoluteRotation));
            }
        }
    }
}
