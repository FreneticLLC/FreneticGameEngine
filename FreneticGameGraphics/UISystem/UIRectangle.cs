using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FreneticGameGraphics.GraphicsHelpers;
using FreneticGameGraphics.ClientSystem;
using OpenTK;

namespace FreneticGameGraphics.UISystem
{
    /// <summary>
    /// A rectangular UI element. Renders a simple box.
    /// </summary>
    public class UIRectangle : UIElement
    {
        /// <summary>
        /// The texture of this box.
        /// </summary>
        public Texture BoxTexture;
        
        /// <summary>
        /// Minimum coordinates of the box.
        /// </summary>
        public Vector2 Min;

        /// <summary>
        /// Maximum coordinates of the box.
        /// </summary>
        public Vector2 Max;

        /// <summary>
        /// Any rotation of relevance.
        /// </summary>
        public Vector3 Rotation = Vector3.Zero;

        /// <summary>
        /// Render the element.
        /// </summary>
        /// <param name="view">The UI view.</param>
        public override void Render(ViewUI2D view)
        {
            if (BoxTexture == null)
            {
                BoxTexture = view.Engine.Textures.White;
            }
            BoxTexture.Bind();
            view.Renderer.RenderRectangle(view.UIContext, Min.X, Min.Y, Max.X, Max.Y);
        }
    }
}
