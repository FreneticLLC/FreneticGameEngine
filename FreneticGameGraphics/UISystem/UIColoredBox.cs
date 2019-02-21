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
        /// <param name="pos">The position of the element.</param>
        /// <param name="texture">The function to use to get the texture ID, if any.</param>
        public UIColoredBox(Vector4 color, UIPositionHelper pos, Func<int> texture = null)
            : base(pos)
        {
            Color = color;
            GetTexture = texture;
        }

        /// <summary>
        /// Renders this box on the screen.
        /// </summary>
        /// <param name="view">The UI view.</param>
        /// <param name="delta">The time since the last render.</param>
        public override void Render(ViewUI2D view, double delta)
        {
            int x = LastAbsolutePosition.X;
            int y = LastAbsolutePosition.Y;
            float w = LastAbsoluteSize.X;
            float h = LastAbsoluteSize.Y;
            GameEngineBase engine = view.Engine;
            if (Color.W > 0.0f)
            {
                view.Rendering.SetColor(Color);
                engine.Textures.White.Bind();
            }
            else if (GetTexture != null)
            {
                view.Rendering.SetColor(Vector4.One);
                GL.BindTexture(TextureTarget.Texture2D, GetTexture());
            }
            else
            {
                return;
            }
            if (Flip)
            {
                view.Rendering.RenderRectangle(view.UIContext, x, y + h, x + w, y, new Vector3(-0.5f, -0.5f, LastAbsoluteRotation));
            }
            else
            {
                view.Rendering.RenderRectangle(view.UIContext, x, y, x + w, y + h, new Vector3(-0.5f, -0.5f, LastAbsoluteRotation));
            }
            view.Rendering.SetColor(Vector4.One);
            engine.Textures.White.Bind();
        }
    }
}
