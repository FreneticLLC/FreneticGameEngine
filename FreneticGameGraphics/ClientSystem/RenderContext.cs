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

namespace FreneticGameGraphics.ClientSystem
{
    /// <summary>
    /// Represents the context in which something is being rendered (in 3D).
    /// </summary>
    public class RenderContext
    {
        /// <summary>
        /// The relevant owning game engine (3D).
        /// </summary>
        public GameEngine3D Engine;
    }
}
