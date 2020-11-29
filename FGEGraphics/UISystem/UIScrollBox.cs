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
using FGECore;
using FGECore.CoreSystems;
using FGECore.MathHelpers;
using FGEGraphics.ClientSystem;
using OpenTK;
using OpenTK.Mathematics;
using OpenTK.Graphics.OpenGL4;

namespace FGEGraphics.UISystem
{
    /// <summary>
    /// Represents a scrollable box containing other elements.
    /// </summary>
    public class UIScrollBox : UIElement
    {
        /// <summary>
        /// The current scroll position.
        /// </summary>
        public int Scroll = 0;

        /// <summary>
        /// An upper limit on how far the scroll box can be scrolled.
        /// 0 for unlimited scrolling.
        /// </summary>
        public int MaxScroll = 0;

        /// <summary>
        /// Construcsts the UI scroll box.
        /// </summary>
        /// <param name="pos">The position of the element.</param>
        public UIScrollBox(UIPositionHelper pos)
            : base(pos)
        {
        }

        /// <summary>
        /// Whether to watch the mouse scroll wheel.
        /// </summary>
        private bool WatchMouse = false;

        /// <summary>
        /// Begins watching the mouse.
        /// </summary>
        protected override void MouseEnter()
        {
            WatchMouse = true;
        }

        /// <summary>
        /// Stops watching the mouse.
        /// </summary>
        protected override void MouseLeave()
        {
            WatchMouse = false;
        }

        /// <summary>
        /// Gets all visible children that contain the position on the screen within this scroll box.
        /// </summary>
        /// <param name="x">The X position to check for.</param>
        /// <param name="y">The Y position to check for.</param>
        /// <returns>A list of visible child elements containing the position.</returns>
        protected override List<UIElement> GetAllAt(int x, int y)
        {
            List<UIElement> found = new List<UIElement>();
            if (SelfContains(x, y))
            {
                foreach (UIElement element in Children)
                {
                    if (element.Contains(x, y))
                    {
                        found.Add(element);
                    }
                }
            }
            return found;
        }

        /// <summary>
        /// Checks the mouse scroll wheel if necessary and changes the scroll position.
        /// </summary>
        /// <param name="delta">The time since the last tick.</param>
        protected override void Tick(double delta)
        {
            if (WatchMouse)
            {
                Scroll -= (int)Client.CurrentMouse.Scroll.Y * 10; // TODO: Why is scroll a Vector2?
                if (Scroll < 0)
                {
                    Scroll = 0;
                }
                if (MaxScroll != 0 && Scroll > MaxScroll)
                {
                    Scroll = MaxScroll;
                }
            }
        }

        /// <summary>
        /// The color of the background of the scroll box (set to Alpha 0 to remove).
        /// </summary>
        public Color4F Color = new Color4F(0f, 0.5f, 0.6f, 0.3f);

        /// <summary>
        /// Renders this scroll box on the screen.
        /// </summary>
        /// <param name="view">The UI view.</param>
        /// <param name="delta">The time since the last render.</param>
        public override void Render(ViewUI2D view, double delta)
        {
            if (Color.A > 0)
            {
                float x = LastAbsolutePosition.X;
                float y = LastAbsolutePosition.Y;
                float w = LastAbsoluteSize.X;
                float h = LastAbsoluteSize.Y;
                view.Rendering.SetColor(Color);
                view.Rendering.RenderRectangle(view.UIContext, x, y, x + w, y + h, new Vector3(-0.5f, -0.5f, LastAbsoluteRotation));
                view.Rendering.SetColor(new Vector4(1f));
            }
        }

        // TODO: Fix!
        /*
        private void RenderChildren(ViewUI2D view, double delta, int xoff, int yoff, Vector3 lastRot)
        {
            int h = LastAbsoluteSize.X;
            int w = LastAbsoluteSize.Y;
            GameEngineBase engine = Engine;
            GL.Enable(EnableCap.ScissorTest);
            GL.Scissor(xoff, engine.Window.Height - (yoff + h), w, h);
            base.RenderChildren(view, delta, xoff, yoff - Scroll, lastRot);
            GL.Scissor(0, 0, engine.Window.Width, engine.Window.Height); // TODO: Bump around a stack, for embedded scroll groups?
            GL.Disable(EnableCap.ScissorTest);
        }
        */
    }
}
