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
        /// <param name="anchor">The anchor the button will be relative to.</param>
        /// <param name="width">The function to get the width.</param>
        /// <param name="height">The function to get the height.</param>
        /// <param name="xOff">The function to get the X offset.</param>
        /// <param name="yOff">The function to get the Y offset.</param>
        public UI3DSubEngine(UIAnchor anchor, Func<float> width, Func<float> height, Func<int> xOff, Func<int> yOff)
            : base(anchor, width, height, xOff, yOff)
        {
            SubEngine = new GameEngine3D()
            {
                IsSubEngine = true,
                SubSize = new FreneticGameCore.Collision.Vector2i(TextureEngine.GetNextPOTValue((int)width()), TextureEngine.GetNextPOTValue((int)height()))
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
