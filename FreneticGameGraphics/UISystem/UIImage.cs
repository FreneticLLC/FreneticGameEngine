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
        /// <param name="anchor">The anchor the image will be relative to.</param>
        /// <param name="width">The function to get the width.</param>
        /// <param name="height">The function to get the height.</param>
        /// <param name="xOff">The function to get the X offset.</param>
        /// <param name="yOff">The function to get the Y offset.</param>
        public UIImage(Texture image, UIAnchor anchor, Func<float> width, Func<float> height, Func<int> xOff, Func<int> yOff)
            : base(anchor, width, height, xOff, yOff)
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
        protected override void Render(ViewUI2D view, double delta, int xoff, int yoff)
        {
            Image.Bind();
            int x = GetX() + xoff;
            int y = GetY() + yoff;
            view.Renderer.RenderRectangle(view.UIContext, x, y, x + GetWidth(), y + GetHeight());
        }
    }
}
