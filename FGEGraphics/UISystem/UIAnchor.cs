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

/// <summary>Helper class for placing UI elements relative to sections of the screen.</summary>
/// <remarks>Generally don't construct directly. Use the static objects instead.</remarks>
public class UIAnchor(Func<UIElement, int> x, Func<UIElement, int> y)
{
    /// <summary>Function for getting the relative X value.</summary>
    public readonly Func<UIElement, int> GetX = x;

    /// <summary>Function for getting the relative Y value.</summary>
    public readonly Func<UIElement, int> GetY = y;

    /// <summary>Helper function objects for the public static values.</summary>
    private static readonly Func<UIElement, int> LEFT_X = (element) => 0,
        CENTER_X = (element) => (int)(element.Parent.Position.Width / 2 - element.Position.Width / 2),
        RIGHT_X = (element) => (int)(element.Parent.Position.Width - element.Position.Width),
        TOP_Y = (element) => 0,
        CENTER_Y = (element) => (int)(element.Parent.Position.Height / 2 - element.Position.Height / 2),
        BOTTOM_Y = (element) => (int)(element.Parent.Position.Height - element.Position.Height);

    /// <summary>Top left UI Anchor. See <see cref="UIAnchor"/>.</summary>
    public static readonly UIAnchor TOP_LEFT = new(LEFT_X, TOP_Y);

    /// <summary>Top center UI Anchor. See <see cref="UIAnchor"/>.</summary>
    public static readonly UIAnchor TOP_CENTER = new(CENTER_X, TOP_Y);

    /// <summary>Top right UI Anchor. See <see cref="UIAnchor"/>.</summary>
    public static readonly UIAnchor TOP_RIGHT = new(RIGHT_X, TOP_Y);

    /// <summary>Center left UI Anchor. See <see cref="UIAnchor"/>.</summary>
    public static readonly UIAnchor CENTER_LEFT = new(LEFT_X, CENTER_Y);

    /// <summary>Center UI Anchor. See <see cref="UIAnchor"/>.</summary>
    public static readonly UIAnchor CENTER = new(CENTER_X, CENTER_Y);

    /// <summary>Center right UI Anchor. See <see cref="UIAnchor"/>.</summary>
    public static readonly UIAnchor CENTER_RIGHT = new(RIGHT_X, CENTER_Y);

    /// <summary>Bottom left UI Anchor. See <see cref="UIAnchor"/>.</summary>
    public static readonly UIAnchor BOTTOM_LEFT = new(LEFT_X, BOTTOM_Y);

    /// <summary>Bottom center UI Anchor. See <see cref="UIAnchor"/>.</summary>
    public static readonly UIAnchor BOTTOM_CENTER = new(CENTER_X, BOTTOM_Y);

    /// <summary>Bottom right UI Anchor. See <see cref="UIAnchor"/>.</summary>
    public static readonly UIAnchor BOTTOM_RIGHT = new(RIGHT_X, BOTTOM_Y);

    /// <summary>Relative UI Anchor. See <see cref="UIAnchor"/>.</summary>
    public static readonly UIAnchor RELATIVE = new((_) => 0, (_) => 0);
}
