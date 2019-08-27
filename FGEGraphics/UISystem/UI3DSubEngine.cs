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
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;
using FGECore;
using FGECore.CoreSystems;
using FGECore.MathHelpers;
using FGEGraphics.GraphicsHelpers;

namespace FGEGraphics.UISystem
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
        /// <param name="alphaBack">Whether to have an alpha background.</param>
        public UI3DSubEngine(UIPositionHelper pos, bool alphaBack)
            : base(pos)
        {
            SubEngine = new GameEngine3D()
            {
                IsSubEngine = true,
                SubSize = new Vector2i(Position.Width, Position.Height)
            };
            SubEngine.OwningInstance = pos.View.Client;
            if (alphaBack)
            {
                SubEngine.MainView.ClearColor = new float[] { 0f, 0f, 0f, 0f };
            }
        }

        /// <summary>
        /// Initializes the subengine.
        /// </summary>
        protected override void Init()
        {
            SubEngine.OwningInstance = Client;
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
        public override void Render(ViewUI2D view, double delta)
        {
            int x = LastAbsolutePosition.X;
            int y = LastAbsolutePosition.Y;
            int w = LastAbsoluteSize.X;
            int h = LastAbsoluteSize.Y;
            GL.BindTexture(TextureTarget.Texture2D, SubEngine.MainView.CurrentFBOTexture);
            view.Rendering.RenderRectangle(view.UIContext, x, y + h, x + w, y, new Vector3(-0.5f, -0.5f, LastAbsoluteRotation));
        }
    }
}
