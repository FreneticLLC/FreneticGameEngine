using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;
using FreneticGameCore;
using FreneticGameGraphics.GraphicsHelpers;

namespace FreneticGameGraphics.ClientSystem.EntitySystem
{
    /// <summary>
    /// Renders a simple 2D box.
    /// </summary>
    public class EntitySimple2DRenderableBoxProperty : Entity2DRenderableProperty
    {
        /// <summary>
        /// How far the box extends up and left.
        /// </summary>
        [PropertyAutoSavable]
        [PropertyDebuggable]
        public Vector2 BoxUpLeft;

        /// <summary>
        /// How far the box extends down and right.
        /// </summary>
        [PropertyAutoSavable]
        [PropertyDebuggable]
        public Vector2 BoxDownRight;

        /// <summary>
        /// The texture for this rendered box.
        /// </summary>
        [PropertyAutoSavable]
        [PropertyDebuggable]
        public Texture BoxTexture;

        /// <summary>
        /// What color to render the box as.
        /// </summary>
        [PropertyAutoSavable]
        [PropertyDebuggable]
        public Vector4 BoxColor = Vector4.One;

        /// <summary>
        /// Render the entity as seen normally, in 2D.
        /// </summary>
        /// <param name="context">The render context.</param>
        public override void RenderStandard2D(RenderContext2D context)
        {
            BoxTexture.Bind();
            context.Engine.RenderHelper.SetColor(BoxColor);
            context.Engine.RenderHelper.RenderRectangle(context, RenderAt.X + BoxUpLeft.X, RenderAt.Y + BoxDownRight.Y, RenderAt.X + BoxDownRight.X, RenderAt.Y + BoxUpLeft.Y);
        }
    }
}
