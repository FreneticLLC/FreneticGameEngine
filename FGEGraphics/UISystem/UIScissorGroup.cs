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
using FGECore.CoreSystems;
using FGEGraphics.ClientSystem;
using OpenTK.Graphics.OpenGL4;

namespace FGEGraphics.UISystem;

/// <summary>Represents a container of elements that only renders children within its bounds.</summary>
/// <param name="pos">The position of the element.</param>
public class UIScissorGroup(UIPositionHelper pos) : UIGroup(pos)
{
    /// <inheritdoc/>
    public override IEnumerable<UIElement> GetChildrenAt(int x, int y) => SelfContains(x, y) ? base.GetChildrenAt(x, y) : [];

    /// <inheritdoc/>
    public override void AddChild(UIElement child)
    {
        base.AddChild(child);
        foreach (UIElement element in child.AllChildren(toAdd: true))
        {
            element.ShouldRender = false;
        }
    }

    /// <inheritdoc/>
    public override void Render(ViewUI2D view, double delta, UIElementStyle style)
    {
        GL.Enable(EnableCap.ScissorTest);
        GL.Scissor(X, Engine.Window.ClientSize.Y - Y - Height, Width, Height);
        foreach (UIElement child in ElementInternal.Children)
        {
            foreach (UIElement element in child.AllChildren())
            {
                element.Render(view, delta);
            }
        }
        GL.Scissor(0, 0, Engine.Window.ClientSize.X, Engine.Window.ClientSize.Y); // TODO: Bump around a stack, for embedded scroll groups?
        GL.Disable(EnableCap.ScissorTest);
    }

    /// <summary>Constrains child interactions to the scissor boundaries.</summary>
    public override bool CanInteract(int x, int y) => SelfContains(x, y);
}
