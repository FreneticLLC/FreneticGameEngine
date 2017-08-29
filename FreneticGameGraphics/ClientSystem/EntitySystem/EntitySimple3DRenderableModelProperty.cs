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
using FreneticGameGraphics.GraphicsHelpers;
using FreneticGameCore;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;

namespace FreneticGameGraphics.ClientSystem.EntitySystem
{
    /// <summary>
    /// Renders a 3D model.
    /// </summary>
    public class EntitySimple3DRenderableModelProperty : EntityRenderableProperty
    {
        /// <summary>
        /// The model for this entity.
        /// </summary>
        public Model EntityModel;

        /// <summary>
        /// The render scale.
        /// </summary>
        public Location Scale;

        /// <summary>
        /// The diffuse color texture.
        /// </summary>
        public Texture DiffuseTexture;

        /// <summary>
        /// The color of the model.
        /// </summary>
        public Vector4 Color = Vector4.One;

        /// <summary>
        /// Render the entity as seen by a top-down map.
        /// </summary>
        /// <param name="context">The render context.</param>
        public override void RenderForTopMap(RenderContext context)
        {
            // TODO: More efficient? Render top only, not irrelevant sides.
            // TODO: Needs scaling
            EntityModel.DrawLOD(Scale, context.Engine.MainView);
        }

        /// <summary>
        /// Render the entity as seen normally, in 3D.
        /// </summary>
        /// <param name="context">The render context.</param>
        public override void RenderStandard(RenderContext context)
        {
            if (DiffuseTexture != null)
            {
                GL.ActiveTexture(TextureUnit.Texture0);
                DiffuseTexture.Bind();
            }
            context.Engine.Rendering.SetColor(Color, context.Engine.MainView);
            Matrix4d mat = Matrix4d.Scale(Scale.ToOpenTK3D()) * Matrix4d.CreateFromQuaternion(RenderOrientation.ToDoubles()) * Matrix4d.CreateTranslation(RenderAt.ToOpenTK3D());
            context.Engine.MainView.SetMatrix(ShaderLocations.Common.WORLD, mat);
            EntityModel.Draw();
        }

        /// <summary>
        /// Non-implemented 2D option.
        /// </summary>
        /// <param name="context">The 2D render context.</param>
        public override void RenderStandard2D(RenderContext2D context)
        {
            throw new NotImplementedException();
        }
    }
}
