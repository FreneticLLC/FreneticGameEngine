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

/// <summary>Represents logic for placing <see cref="UIElement"/>s on a screen.</summary>
/// <param name="AlignmentX">The horizontal position alignment.</param>
/// <param name="AlignmentY">The vertical position alignment.</param>
public record UIAnchor(UIAlignment AlignmentX, UIAlignment AlignmentY)
{
    /// <summary>Returns the horizontal offset relative to the <paramref name="element"/>'s parent.</summary>
    /// <param name="element">The child element.</param>
    public int GetX(UIElement element) => AlignmentX.GetX(element);

    /// <summary>Returns the vertical offset relative to the <paramref name="element"/>'s parent.</summary>
    /// <param name="element">The child element.</param>
    public int GetY(UIElement element) => AlignmentY.GetY(element);

    /// <summary>Top left UI Anchor. See <see cref="UIAnchor"/>.</summary>
    public static readonly UIAnchor TOP_LEFT = new(UIAlignment.LEFT, UIAlignment.TOP);

    /// <summary>Top center UI Anchor. See <see cref="UIAnchor"/>.</summary>
    public static readonly UIAnchor TOP_CENTER = new(UIAlignment.CENTER, UIAlignment.TOP);

    /// <summary>Top right UI Anchor. See <see cref="UIAnchor"/>.</summary>
    public static readonly UIAnchor TOP_RIGHT = new(UIAlignment.RIGHT, UIAlignment.TOP);

    /// <summary>Center left UI Anchor. See <see cref="UIAnchor"/>.</summary>
    public static readonly UIAnchor CENTER_LEFT = new(UIAlignment.LEFT, UIAlignment.CENTER);

    /// <summary>Center UI Anchor. See <see cref="UIAnchor"/>.</summary>
    public static readonly UIAnchor CENTER = new(UIAlignment.CENTER, UIAlignment.CENTER);

    /// <summary>Center right UI Anchor. See <see cref="UIAnchor"/>.</summary>
    public static readonly UIAnchor CENTER_RIGHT = new(UIAlignment.RIGHT, UIAlignment.CENTER);

    /// <summary>Bottom left UI Anchor. See <see cref="UIAnchor"/>.</summary>
    public static readonly UIAnchor BOTTOM_LEFT = new(UIAlignment.LEFT, UIAlignment.BOTTOM);

    /// <summary>Bottom center UI Anchor. See <see cref="UIAnchor"/>.</summary>
    public static readonly UIAnchor BOTTOM_CENTER = new(UIAlignment.CENTER, UIAlignment.BOTTOM);

    /// <summary>Bottom right UI Anchor. See <see cref="UIAnchor"/>.</summary>
    public static readonly UIAnchor BOTTOM_RIGHT = new(UIAlignment.RIGHT, UIAlignment.BOTTOM);

    /// <summary>An array of all standard UI Anchors. See <see cref="UIAnchor"/>.</summary>
    public static readonly UIAnchor[] VALUES = [TOP_LEFT, TOP_CENTER, TOP_RIGHT, CENTER_LEFT, CENTER, CENTER_RIGHT, BOTTOM_LEFT, BOTTOM_CENTER, BOTTOM_RIGHT];

    /// <inheritdoc/>
    public override string ToString() => AlignmentX == AlignmentY ? AlignmentX.ToString() : $"{AlignmentX} {AlignmentY}";
}
