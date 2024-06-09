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

public class UIListGroup(UIPositionHelper pos, int spacing) : UIGroup(pos.ConstantWidthHeight(0, 0))
{
    public int Spacing = spacing;

    public override void AddChild(UIElement child, bool priority = true)
    {
        base.AddChild(child, priority);
        child.Position.Anchor(UIAnchor.TOP_CENTER).ConstantXY(0, Position.Height > 0 ? Position.Height + Spacing : 0);
        Position.ConstantHeight(child.Position.Y + child.Position.Height);
    }
}
