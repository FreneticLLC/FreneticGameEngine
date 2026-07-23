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
using FGECore.MathHelpers;
using FGEGraphics.ClientSystem;
using FGEGraphics.GraphicsHelpers;
using FGEGraphics.GraphicsHelpers.Textures;
using OpenTK;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace FGEGraphics.UISystem.Elements;

/// <summary>Represents a simple button with text.</summary>
public class UIButton : UIElement
{
    /// <inheritdoc/>
    public override string Name => "Button";

    /// <summary>The button label.</summary>
    public UILabel Label;

    /// <summary>Constructs a <see cref="UIButton"/>.</summary>
    /// <param name="text">The button text.</param>
    /// <param name="styling">The styling of the element.</param>
    /// <param name="layout">The layout of the element.</param>
    public UIButton(string text, UIStyling styling = null, UILayout layout = null) : base(styling, layout)
    {
        AddChild(Label = new UILabel(text, Styling.Bind(this) with { ShowBackground = false }, new UILayout().SetAnchor(UIAnchor.CENTER)) { IsEnabled = false });
    }
}
