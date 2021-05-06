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
using FGECore.MathHelpers;
using FGEGraphics.GraphicsHelpers;
using FGEGraphics.GraphicsHelpers.Models;
using FGEGraphics.GraphicsHelpers.Textures;
using OpenTK;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace FGEGraphics.ClientSystem.EntitySystem
{
    /// <summary>Renders a 3D model.</summary>
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
        public Color4F Color = Color4F.White;

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
        /// PRIMARILY FOR INTERNAL USAGE.
        /// Caps to disable for this render.
        /// </summary>
        public HashSet<EnableCap> DisabledCaps = new HashSet<EnableCap>();

        /// <summary>
        /// Gets or sets whether the object is always visible through walls.
        /// <para>WILL LIKELY CAUSE VISUAL GLITCHES. MAY BE ABLE TO SEE PARTS OF IT THROUGH ITSELF.</para>
        /// </summary>
        public bool VisibleThroughWalls
        {
            get
            {
                return DisabledCaps.Contains(EnableCap.DepthTest);
            }
            set
            {
                if (value)
                {
                    DisabledCaps.Add(EnableCap.DepthTest);
                }
                else
                {
                    DisabledCaps.Remove(EnableCap.DepthTest);
                }
            }
        }

        /// <summary>
        /// Render the entity as seen normally, in 3D.
        /// </summary>
        /// <param name="context">The render context.</param>
        public override void RenderStandard(RenderContext context)
        {
            foreach (EnableCap ec in DisabledCaps)
            {
                GL.Disable(ec);
            }
            if (DiffuseTexture != null)
            {
                GL.ActiveTexture(TextureUnit.Texture0);
                DiffuseTexture.Bind();
            }
            context.Engine.Rendering.SetColor(Color, context.Engine.MainView);
            Matrix4d mat = Matrix4d.Scale(Scale.ToOpenTK3D()) * Matrix4d.CreateFromQuaternion(RenderOrientation.ToOpenTKDoubles()) * Matrix4d.CreateTranslation(RenderAt.ToOpenTK3D());
            context.Engine.MainView.SetMatrix(ShaderLocations.Common.WORLD, mat);
            EntityModel.Draw(context);
            foreach (EnableCap ec in DisabledCaps)
            {
                GL.Enable(ec);
            }
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
