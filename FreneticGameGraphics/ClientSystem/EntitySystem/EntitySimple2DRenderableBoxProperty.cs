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
using FreneticGameCore;
using FreneticGameCore.CoreSystems;
using FreneticGameCore.MathHelpers;
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
        /// Get or set the size of the box. Setting will align the box to its own center.
        /// </summary>
        public Vector2 BoxSize
        {
            get
            {
                return new Vector2(BoxDownRight.X - BoxUpLeft.X, BoxUpLeft.Y - BoxDownRight.Y);
            }
            set
            {
                BoxUpLeft = new Vector2(-value.X * 0.5f, value.Y * 0.5f);
                BoxDownRight = new Vector2(value.X * 0.5f, -value.Y * 0.5f);
            }
        }

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
        public Color4F BoxColor = Color4F.White;

        /// <summary>
        /// Render the entity as seen normally, in 2D.
        /// </summary>
        /// <param name="context">The render context.</param>
        public override void RenderStandard2D(RenderContext2D context)
        {
            if (context.CalcShadows && context.Engine.OneDLights)
            {
                context.Engine.Textures.White.Bind();
            }
            else
            {
                BoxTexture.Bind();
            }
            context.Engine.RenderHelper.SetColor(BoxColor);
            Vector2 sz = BoxSize;
            context.Engine.RenderHelper.RenderRectangle(context, (float)RenderAt.X + BoxUpLeft.X, (float)RenderAt.Y + BoxUpLeft.Y,
                (float)RenderAt.X + BoxDownRight.X, (float)RenderAt.Y + BoxDownRight.Y, new Vector3(BoxUpLeft.X / sz.X, BoxDownRight.Y / sz.Y, RenderAngle));
        }
    }
}
