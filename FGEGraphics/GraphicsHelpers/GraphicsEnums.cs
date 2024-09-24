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

namespace FGEGraphics.GraphicsHelpers;

/// <summary>How to adjust a rectangle shape to make a texture fit in it cleanly.</summary>
public enum TextureFit
{
    /// <summary>No adjustment, just stretch the texture out.</summary>
    STRETCH = 0,
    /// <summary>Shrink the texture to fit within the box, allowing for it to be slightly smaller than intended, and preserving the texture's aspect ratio. Will center the shrunken side.</summary>
    CONTAIN = 1,
    /// <summary>Stretch the texture to fit the box, allowing for it to be slightly larger than intended, and preserving the texture's aspect ratio. Will center the extended side.</summary>
    OVEREXTEND = 2
}
