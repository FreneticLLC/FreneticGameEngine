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
using OpenTK.Graphics.OpenGL4;

namespace FreneticGameGraphics.UISystem
{
    /// <summary>
    /// Represents a simple colored box on a screen.
    /// </summary>
    public class UIColoredBox : UIElement
    {
        /// <summary>
        /// The color of this box.
        /// </summary>
        public Vector4 Color;

        /// <summary>
        /// Gets the texture ID this box uses.
        /// </summary>
        public Func<int> GetTexture;

        /// <summary>
        /// Whether this box is flipped over the X axis.
        /// </summary>
        public bool Flip = false;

        /// <summary>
        /// Constructs a simple colored box.
        /// </summary>
        /// <param name="color">The color to use.</param>
        /// <param name="anchor">The anchor the box will be relative to.</param>
        /// <param name="width">The function to get the width.</param>
        /// <param name="height">The function to get the height.</param>
        /// <param name="xOff">The function to get the X offset.</param>
        /// <param name="yOff">The function to get the Y offset.</param>
        /// <param name="texture">The function to use to get the texture ID, if any.</param>
        public UIColoredBox(Vector4 color, UIAnchor anchor, Func<float> width, Func<float> height, Func<int> xOff, Func<int> yOff, Func<int> texture = null)
            : base(anchor, width, height, xOff, yOff)
        {
            Color = color;
            GetTexture = texture;
        }

        /// <summary>
        /// Renders this box on the screen.
        /// </summary>
        /// <param name="view">The UI view.</param>
        /// <param name="delta">The time since the last render.</param>
        /// <param name="xoff">The X offset of this box's parent.</param>
        /// <param name="yoff">The Y offset of this box's parent.</param>
        protected override void Render(ViewUI2D view, double delta, int xoff, int yoff)
        {
            int x = GetX() + xoff;
            int y = GetY() + yoff;
            int w = (int)GetWidth();
            int h = (int)GetHeight();
            GameEngineBase engine = view.Engine;
            if (Color.W > 0.0f)
            {
                view.Renderer.SetColor(Color);
                engine.Textures.White.Bind();
                view.Renderer.RenderRectangle(view.UIContext, x, y, x + w, y + h);
            }
            if (GetTexture != null)
            {
                view.Renderer.SetColor(Vector4.One);
                GL.BindTexture(TextureTarget.Texture2D, GetTexture());
            }
            if (Flip)
            {
                view.Renderer.RenderRectangle(view.UIContext, x, y + h, x + w, y);
            }
            else
            {
                view.Renderer.RenderRectangle(view.UIContext, x, y, x + w, y + h);
            }
            engine.Textures.White.Bind();
        }
    }
}
