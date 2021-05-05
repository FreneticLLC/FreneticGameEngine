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

namespace FGEGraphics.GraphicsHelpers.Models
{
    /// <summary>
    /// Represents a node in a model.
    /// </summary>
    public class ModelNode
    {
        /// <summary>
        /// The parent node.
        /// </summary>
        public ModelNode Parent = null;

        /// <summary>
        /// All children nodes.
        /// </summary>
        public List<ModelNode> Children = new List<ModelNode>();

        /// <summary>
        /// All relevant bones.
        /// </summary>
        public List<ModelBone> Bones = new List<ModelBone>();

        /// <summary>
        /// The mode ID.
        /// </summary>
        public byte Mode;

        /// <summary>
        /// The name of the node.
        /// </summary>
        public string Name;
    }
}
