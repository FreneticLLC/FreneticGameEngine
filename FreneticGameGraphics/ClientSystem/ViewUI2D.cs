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
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;
using FreneticGameGraphics.UISystem;
using FreneticGameGraphics.GraphicsHelpers;

namespace FreneticGameGraphics.ClientSystem
{
    /// <summary>
    /// A 2D UI view.
    /// </summary>
    public class ViewUI2D
    {
        /// <summary>
        /// The backing client window.
        /// </summary>
        public GameClientWindow Client;

        /// <summary>
        /// Gets the primary engine.
        /// </summary>
        public GameEngineBase Engine
        {
            get
            {
                return Client.CurrentEngine;
            }
        }

        /// <summary>
        /// Gets the rendering helper for the engine.
        /// </summary>
        public Renderer2D Rendering
        {
            get
            {
                return Client.Rendering2D;
            }
        }

        /// <summary>
        /// The default basic UI screen.
        /// </summary>
        public UIScreen DefaultScreen;

        /// <summary>
        /// Constructs the view.
        /// </summary>
        /// <param name="gameClient">Backing client window.</param>
        public ViewUI2D(GameClientWindow gameClient)
        {
            Client = gameClient;
            UIContext = new RenderContext2D();
            DefaultScreen = new UIScreen(this);
            CurrentScreen = DefaultScreen;
        }

        /// <summary>
        /// The current main screen.
        /// </summary>
        public UIScreen CurrentScreen;

        /// <summary>
        /// The render context (2D) for the UI.
        /// </summary>
        public RenderContext2D UIContext;

        /// <summary>
        /// Whether this UI is displayed directly onto the screen (as opposed to a temporary GL buffer).
        /// </summary>
        public bool DirectToScreen = true;

        /// <summary>
        /// Draw the menu to the relevant back buffer.
        /// </summary>
        public void Draw()
        {
            if (DirectToScreen)
            {
                UIContext.ZoomMultiplier = Client.Window.Width * 0.5f;
                UIContext.Width = Client.Window.Width;
                UIContext.Height = Client.Window.Height;
                float aspect = UIContext.Width / (float)UIContext.Height;
                float sc = 1.0f / (UIContext.Zoom * UIContext.ZoomMultiplier);
                UIContext.Scaler = new Vector2(sc, -sc * aspect);
                UIContext.ViewCenter = new Vector2(-Client.Window.Width * 0.5f, -Client.Window.Height * 0.5f);
                UIContext.Adder = UIContext.ViewCenter;
                UIContext.AspectHelper = UIContext.Width / (float)UIContext.Height;
                Client.Ortho = Matrix4.CreateOrthographicOffCenter(0, Client.Window.Width, Client.Window.Height, 0, -1, 1);
            }
            // TODO: alternate Ortho setting from scaler/adder def!
            Client.Shaders.ColorMult2DShader.Bind();
            GL.Uniform3(ShaderLocations.Common2D.SCALER, new Vector3(UIContext.Scaler.X, UIContext.Scaler.Y, UIContext.AspectHelper));
            GL.Uniform2(2, ref UIContext.Adder);
            GL.Disable(EnableCap.DepthTest);
            GL.Disable(EnableCap.CullFace);
            CurrentScreen.FullRender(this, Client.Delta, 0, 0);
        }

        /// <summary>
        /// Ticks all elements attached to this view.
        /// </summary>
        public void Tick()
        {
            CurrentScreen.FullTick(Client.Delta);
        }
    }
}
