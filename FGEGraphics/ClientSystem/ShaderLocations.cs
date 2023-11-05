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

namespace FGEGraphics.ClientSystem;

/// <summary>Represents shader target locations.</summary>
public static class ShaderLocations
{
    /// <summary>Locations shared by most shaders (3D mainly).</summary>
    public static class Common
    {
        /// <summary>The general screen projection and view together.</summary>
        public const int PROJECTION = 1;

        /// <summary>The general world offset.</summary>
        public const int WORLD = 2;
    }

    /// <summary>Locations shared by most 2D shaders.</summary>
    public static class Common2D
    {
        /// <summary>The scaler value.</summary>
        public const int SCALER = 1;

        /// <summary>The adder value.</summary>
        public const int ADDER = 2;

        /// <summary>The color multiplier to add.</summary>
        public const int COLOR = 3;

        /// <summary>The rotation effect to apply.</summary>
        public const int ROTATION = 4;
    }
}
