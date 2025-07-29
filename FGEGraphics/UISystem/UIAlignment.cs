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
    /// <summary>Returns the positional offset relative to the <paramref name="parentDimension"/> for the given alignment.</summary>
    /// <param name="alignment">The UI alignment.</param>
    /// <param name="parentDimension">The parent spatial dimension.</param>
    /// <param name="childDimension">The child spatial dimension.</param>
    public static int GetPosition(this UIAlignment alignment, int parentDimension, int childDimension)
        => alignment switch
        {
            UIAlignment.LEFT => 0,
            UIAlignment.CENTER => parentDimension / 2 - childDimension / 2,
            UIAlignment.RIGHT => parentDimension - childDimension,
            _ => throw new NotImplementedException(),
        };

    /// <summary>Returns the horizontal offset relative to the <paramref name="element"/>'s parent for the given alignment.</summary>
    /// <param name="alignment">The UI alignment.</param>
    /// <param name="element">The child element.</param>
    public static int GetX(this UIAlignment alignment, UIElement element) => alignment.GetPosition(element.Parent.Layout.Width, element.Layout.Width);

    /// <summary>Returns the vertical offset relative to the <paramref name="element"/>'s parent for the given alignment.</summary>
    /// <param name="alignment">The UI alignment.</param>
    /// <param name="element">The child element.</param>
    public static int GetY(this UIAlignment alignment, UIElement element) => alignment.GetPosition(element.Parent.Layout.Height, element.Layout.Height);
}
