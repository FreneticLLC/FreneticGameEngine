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

namespace FGEGraphics.UISystem;

/// <summary>Simple enumeration of UI alignment modes.</summary>
public enum UIAlignment
{
    /// <summary>(Horizontal only) Left alignment.</summary>
    LEFT = 0,
    /// <summary>(Vertical only) Top alignment.</summary>
    TOP = LEFT,
    /// <summary>Center alignment.</summary>
    CENTER = 1,
    /// <summary>(Horizontal only) Right alignment.</summary>
    RIGHT = 2,
    /// <summary>(Vertical only) Bottom alignment.</summary>
    BOTTOM = RIGHT
}

/// <summary>Helper methods for <see cref="UIAlignment"/>.</summary>
public static class UIAlignmentExtensions
{
    /// <summary>Returns the fraction of width or height to multiply in to get the proper offset for this alignment. Returns 0, 0.5, or 1.</summary>
    public static float SizeMultiplier(this UIAlignment align) => ((float)align) * 0.5f;
}
