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
using FreneticUtilities.FreneticExtensions;
using FGEGraphics.ClientSystem;

namespace FGEGraphics.GraphicsHelpers.Models
{

    /// <summary>Represents a mesh within a model.</summary>
    public class ModelMesh
    {
        /// <summary>The name of this mesh.</summary>
        public string Name;

        /// <summary>The bones relevant to this mesh.</summary>
        public List<ModelBone> Bones = new();

        /// <summary>Constructs the model mesh.</summary>
        /// <param name="_name">The name of it.</param>
        public ModelMesh(string _name)
        {
            Name = _name.ToLowerFast();
            if (Name.EndsWith(".001"))
            {
                Name = Name[..^".001".Length];
            }
            BaseRenderable = new Renderable();
        }

        /// <summary>The VBO for this mesh.</summary>
        public Renderable BaseRenderable;

        /// <summary>Destroys the backing <see cref="Renderable"/>.</summary>
        public void Destroy()
        {
            BaseRenderable.Destroy();
        }

        /// <summary>Renders the mesh.</summary>
        /// <param name="context">The sourcing render context.</param>
        public void Draw(RenderContext context)
        {
            context.ModelsRendered++;
            BaseRenderable.Render(context, true);
        }
    }
}
