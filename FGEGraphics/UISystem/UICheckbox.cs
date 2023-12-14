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
using FGEGraphics.UISystem;

namespace FGEGraphics.UISystem;

public class UICheckbox : UIClickableElement
{
    public bool Checked = false;

    public UICheckbox(UIPositionHelper pos) : base(pos)
    {
        Clicked += (_, _) => Checked = !Checked;
    }

    public override void Render(ViewUI2D view, double delta, UIElementStyle style)
    {
        Engine.Textures.White.Bind();
        view.Rendering.RenderRectangle(view.UIContext, X, Y, Width, Height);
        if (Checked)
        {
            Engine.Textures.Black.Bind();
            view.Rendering.RenderRectangle(view.UIContext, X + 10, Y + 10, Width - 10, Height - 10);
        }
    }
}
