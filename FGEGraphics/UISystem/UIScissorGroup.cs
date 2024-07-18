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
// TODO: disable elements outside of the scissor view
public class UIScissorGroup(UIPositionHelper pos) : UIGroup(pos)
{
    /// <inheritdoc/>
    public override IEnumerable<UIElement> GetChildrenAt(int x, int y) => SelfContains(x, y) ? base.GetChildrenAt(x, y) : [];

    /// <inheritdoc/>
    public override void AddChild(UIElement child, bool priority = true)
    {
        base.AddChild(child, priority);
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
}
