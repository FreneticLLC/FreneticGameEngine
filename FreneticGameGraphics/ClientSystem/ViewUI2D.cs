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
        /// The backing engine.
        /// </summary>
        public GameEngineBase Engine;

        /// <summary>
        /// Gets the renderer for the engine.
        /// </summary>
        public Renderer2D Renderer
        {
            get
            {
                return (Engine is GameEngine2D ge2d) ? ge2d.RenderHelper : (Engine as GameEngine3D).RenderingUI;
            }
        }

        /// <summary>
        /// Constructs the view.
        /// </summary>
        /// <param name="gameEngine">Backing engine.</param>
        public ViewUI2D(GameEngineBase gameEngine)
        {
            Engine = gameEngine;
            UIContext = new RenderContext2D();
        }

        /// <summary>
        /// Top-level UI elements.
        /// </summary>
        public List<UIElement> Elements = new List<UIElement>();

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
                UIContext.ZoomMultiplier = Engine.Window.Width * 0.5f;
                UIContext.Width = Engine.Window.Width;
                UIContext.Height = Engine.Window.Height;
                float aspect = UIContext.Width / (float)UIContext.Height;
                float sc = 1.0f / (UIContext.Zoom * UIContext.ZoomMultiplier);
                UIContext.Scaler = new Vector2(sc, -sc * aspect);
                UIContext.ViewCenter = new Vector2(-Engine.Window.Width * 0.5f, -Engine.Window.Height * 0.5f);
                UIContext.Adder = UIContext.ViewCenter;
            }
            Engine.Shaders.ColorMult2DShader.Bind();
            GL.Uniform2(1, ref UIContext.Scaler);
            GL.Uniform2(2, ref UIContext.Adder);
            GL.Disable(EnableCap.DepthTest);
            GL.Disable(EnableCap.CullFace);
            foreach (UIElement elem in Elements)
            {
                elem.Render(this);
            }
        }
    }
}
