//
// This file is created by Frenetic LLC.
// This code is Copyright (C) 2017 Frenetic LLC under the terms of a strict license.
// See README.md or LICENSE.txt in the source root for the contents of the license.
// If neither of these are available, assume that neither you nor anyone other than the copyright holder
// hold any right or permission to use this software until such time as the official license is identified.
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FreneticGameGraphics.ClientSystem;
using FreneticGameGraphics.GraphicsHelpers;
using OpenTK;

namespace FreneticGameGraphics.UISystem
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
        /// <param name="xoff">The X offset of this element's parent.</param>
        /// <param name="yoff">The Y offset of this element's parent.</param>
        /// <param name="rotation">The calculated rotation to make in this render call.</param>
        protected override void Render(ViewUI2D view, double delta, int xoff, int yoff, float rotation)
        {
            Image.Bind();
            int x = GetX() + xoff;
            int y = GetY() + yoff;
            view.Rendering.RenderRectangle(view.UIContext, x, y, x + GetWidth(), y + GetHeight(), new Vector3(-0.5f, -0.5f, rotation));
        }
    }
}
