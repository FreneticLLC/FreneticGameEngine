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
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;
using FreneticGameCore;
using FreneticGameGraphics.GraphicsHelpers;

namespace FreneticGameGraphics.UISystem
{
    /// <summary>
    /// Represents a 3D sub-engine within a UI.
    /// </summary>
    public class UI3DSubEngine : UIElement
    {
        /// <summary>
        /// The held sub-engine.
        /// </summary>
        public GameEngine3D SubEngine;

        /// <summary>
        /// Constructs a new 3D sub-engine.
        /// </summary>
        /// <param name="pos">The position of the element.</param>
        public UI3DSubEngine(UIPositionHelper pos)
            : base(pos)
        {
            SubEngine = new GameEngine3D()
            {
                IsSubEngine = true,
                SubSize = new FreneticGameCore.Collision.Vector2i(TextureEngine.GetNextPOTValue(GetWidth()), TextureEngine.GetNextPOTValue(GetHeight()))
            };
        }

        /// <summary>
        /// Initializes the subengine.
        /// </summary>
        protected override void Init()
        {
            SubEngine.Client = Client;
            SubEngine.Load();
        }

        /// <summary>
        /// Destroys the subengine.
        /// </summary>
        protected override void Destroy()
        {
            SubEngine.MainView.Destroy();
        }

        /// <summary>
        /// Ticks the element.
        /// </summary>
        /// <param name="delta">Delta.</param>
        protected override void Tick(double delta)
        {
            SubEngine.Delta = delta;
            // TODO: Check for resize need?
            SubEngine.RenderSingleFrame();
            SubEngine.Tick();
        }

        /// <summary>
        /// Renders the view on-screen.
        /// </summary>
        /// <param name="view">The UI view.</param>
        /// <param name="delta">Delta time.</param>
        /// <param name="xoff">X Offset.</param>
        /// <param name="yoff">Y Offset.</param>
        protected override void Render(ViewUI2D view, double delta, int xoff, int yoff)
        {
            int x = GetX() + xoff;
            int y = GetY() + yoff;
            int w = (int)GetWidth();
            int h = (int)GetHeight();
            GL.BindTexture(TextureTarget.Texture2D, SubEngine.MainView.CurrentFBO);
            view.Rendering.RenderRectangle(view.UIContext, x, y + h, x + w, y);
        }
    }
}
