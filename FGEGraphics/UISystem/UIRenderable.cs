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
using FGEGraphics.ClientSystem;

namespace FGEGraphics.UISystem;

/// <summary>Represents a simple renderer that can be attached to any element.</summary>
/// <param name="renderer">The renderer method. See <see cref="UIElement.Render(double, UIStyle)"/>.</param>
public class UIRenderable(Action<UIElement, double, UIStyle> renderer) : UIElement(UIStyling.Empty, new UILayout())
{
    /// <inheritdoc/>
    public override void Render(double delta, UIStyle style) => renderer(this, delta, style);
}
