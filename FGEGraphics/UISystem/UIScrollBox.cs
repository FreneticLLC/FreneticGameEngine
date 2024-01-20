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
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace FGEGraphics.UISystem;

/// <summary>Represents a scrollable box containing other elements.</summary>
/// <param name="style">The style of the element.</param>
/// <param name="pos">The position of the element.</param>
/// <param name="shouldRender">Whether the element should render automatically.</param>
public class UIScrollBox(UIElementStyle style, UIPositionHelper pos, bool shouldRender = true) : UIBox(style, pos, shouldRender)
{
    /// <summary>The current scroll position.</summary>
    public int Scroll = 0;

    // TODO: height or something
    /// <summary>
    /// An upper limit on how far the scroll box can be scrolled.
    /// 0 for unlimited scrolling.
    /// </summary>
    public int MaxScroll = 0;

    /// <summary>(Internal) Whether to watch the mouse scroll wheel.</summary>
    public bool WatchMouse = false;

    /// <summary>Begins watching the mouse.</summary>
    public override void MouseEnter()
    {
        WatchMouse = true;
    }

    /// <summary>Stops watching the mouse.</summary>
    public override void MouseLeave()
    {
        WatchMouse = false;
    }

    /// <summary>Adds a child to the scroll box.</summary>
    /// <param name="child">The element to be parented.</param>
    /// <param name="priority">The child's render priority. This has no effect on a scroll box.</param>
    public override void AddChild(UIElement child, bool priority = true)
    {
        base.AddChild(child, priority);
        int originalY = child.Position.Y;
        child.Position.GetterY(() => originalY - Scroll + MaxScroll);
        child.ShouldRender = false;
    }

    /// <summary>Gets all visible children that contain the position on the screen within this scroll box.</summary>
    /// <param name="x">The X position to check for.</param>
    /// <param name="y">The Y position to check for.</param>
    /// <returns>A list of visible child elements containing the position.</returns>
    public override IEnumerable<UIElement> GetChildrenAt(int x, int y)
    {
        if (SelfContains(x, y))
        {
            foreach (UIElement element in ElementInternal.Children)
            {
                if (element.Contains(x, y))
                {
                    yield return element;
                }
            }
        }
    }

    /// <summary>Checks the mouse scroll wheel if necessary and changes the scroll position.</summary>
    /// <param name="delta">The time since the last tick.</param>
    public override void Tick(double delta)
    {
        if (!WatchMouse)
        {
            return;
        }
        Scroll -= (int)Window.CurrentMouse.ScrollDelta.Y * 10;
        if (Scroll < 0)
        {
            Scroll = 0;
        }
        if (MaxScroll != 0 && Scroll > MaxScroll)
        {
            Scroll = MaxScroll;
        }
    }

    /// <summary>Renders this scroll box on the screen.</summary>
    /// <param name="view">The UI view.</param>
    /// <param name="delta">The time since the last render.</param>
    /// <param name="style">The current element style.</param>
    public override void Render(ViewUI2D view, double delta, UIElementStyle style)
    {
        base.Render(view, delta, style);
        GL.Enable(EnableCap.ScissorTest);
        GL.Scissor(X, Engine.Window.ClientSize.Y - (Y + Height), Width, Height);
        foreach (UIElement child in ElementInternal.Children)
        {
            child.Render(view, delta);
        }
        GL.Scissor(0, 0, Engine.Window.ClientSize.X, Engine.Window.ClientSize.Y); // TODO: Bump around a stack, for embedded scroll groups?
        GL.Disable(EnableCap.ScissorTest);
    }
}
