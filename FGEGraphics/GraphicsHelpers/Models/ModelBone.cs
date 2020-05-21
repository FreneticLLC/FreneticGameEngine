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
using OpenTK;

namespace FGEGraphics.GraphicsHelpers.Models
{
    /// <summary>
    /// Represents a model's bone.
    /// </summary>
    public class ModelBone
    {
        /// <summary>
        /// The transform of the bone.
        /// </summary>
        public Matrix4 Transform = Matrix4.Identity;

        /// <summary>
        /// The offset of the bone.
        /// </summary>
        public Matrix4 Offset;
    }
}
