//
// This file is created by Frenetic LLC.
// This code is Copyright (C) 2017 Frenetic LLC under the terms of a strict license.
// See README.md or LICENSE.txt in the source root for the contents of the license.
// If neither of these are available, assume that neither you nor anyone other than the copyright holder
// hold any right or permission to use this software until such time as the official license is identified.
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FreneticGameGraphics.ClientSystem;
using FreneticGameGraphics.GraphicsHelpers;
using OpenTK;
using OpenTK.Input;

namespace FreneticGameGraphics.UISystem
{
    /// <summary>
    /// Represents a single generic item in a UI.
    /// <para>Sub-classes implement rendering and general logic for a specific type of UI element.</para>
    /// </summary>
    public abstract class UIElement
    {
        /// <summary>
        /// Do not access directly, except for debugging.
        /// </summary>
        public List<UIElement> Children;
        
        /// <summary>
        /// The parent of this element.
        /// </summary>
        public UIElement Parent;

        /// <summary>
        /// Gets the client game window used to render this element.
        /// </summary>
        public virtual GameClientWindow Client
        {
            get
            {
                return Parent.Client;
            }
        }

        /// <summary>
        /// Gets the client game engine used to render this element.
        /// </summary>
        public virtual GameEngineBase Engine
        {
            get
            {
                return Parent.Engine;
            }
        }

        /// <summary>
        /// The position and size of this element.
        /// </summary>
        public UIPositionHelper Position;

        /// <summary>
        /// Internal use only.
        /// </summary>
        public bool HoverInternal;

        /// <summary>
        /// Constructs a new element to be placed on a <see cref="UIScreen"/>.
        /// </summary>
        /// <param name="pos">The position of the element.</param>
        public UIElement(UIPositionHelper pos)
        {
            Children = new List<UIElement>();
            Position = pos;
            Position.For = this;
        }

        /// <summary>
        /// Adds a child to this element.
        /// </summary>
        /// <param name="child">The element to be parented.</param>
        public void AddChild(UIElement child)
        {
            if (child.Parent != null)
            {
                throw new Exception("Tried to add a child that already has a parent!");
            }
            if (!Children.Contains(child))
            {
                ToAdd.Add(child);
            }
            else
            {
                throw new Exception("Tried to add a child that already belongs to this element!");
            }
        }

        /// <summary>
        /// Removes a child from this element.
        /// </summary>
        /// <param name="child">The element to be unparented.</param>
        public void RemoveChild(UIElement child)
        {
            if (Children.Contains(child))
            {
                if (!ToRemove.Contains(child))
                {
                    ToRemove.Add(child);
                }
            }
            else if (ToAdd.Contains(child))
            {
                ToAdd.Remove(child);
            }
            else
            {
                throw new Exception("Tried to remove a child that does not belong to this element!");
            }
        }

        /// <summary>
        /// Removes all children from this element.
        /// </summary>
        public void RemoveAllChildren()
        {
            foreach (UIElement child in Children)
            {
                RemoveChild(child);
            }
        }

        /// <summary>
        /// Checks if this element has the specified child.
        /// </summary>
        /// <param name="element">The possible child.</param>
        /// <returns></returns>
        public bool HasChild(UIElement element)
        {
            return Children.Contains(element) && !ToRemove.Contains(element);
        }
        
        /// <summary>
        /// Gets the current X position of this element on the screen based on the anchor and X offset.
        /// </summary>
        /// <returns>The current X value.</returns>
        public int GetX()
        {
            return Position.X;
        }

        /// <summary>
        /// Gets the current Y position of this element on the screen based on the anchor and Y offset.
        /// </summary>
        /// <returns>The current Y value.</returns>
        public int GetY()
        {
            return Position.Y;
        }

        /// <summary>
        /// Gets the current width of this element.
        /// </summary>
        /// <returns>The current width.</returns>
        public int GetWidth()
        {
            return Position.Width;
        }

        /// <summary>
        /// Gets the current height of this element.
        /// </summary>
        /// <returns>The current height.</returns>
        public int GetHeight()
        {
            return Position.Height;
        }

        /// <summary>
        /// Gets the current rotation of this element.
        /// </summary>
        /// <returns>The current rotation.</returns>
        public float GetRotation()
        {
            return Position.Rotation;
        }

        /// <summary>
        /// Checks if this element's boundaries (or any of its children's boundaries) contain the position on the screen.
        /// </summary>
        /// <param name="x">The X position to check for.</param>
        /// <param name="y">The Y position to check for.</param>
        /// <returns>Whether the position is within any of the boundaries.</returns>
        public bool Contains(int x, int y)
        {
            foreach (UIElement child in Children)
            {
                if (child.Contains(x, y))
                {
                    return true;
                }
            }
            return SelfContains(x, y);
        }

        /// <summary>
        /// Checks if this element's boundaries contain the position on the screen.
        /// </summary>
        /// <param name="x">The X position to check for.</param>
        /// <param name="y">The Y position to check for.</param>
        /// <returns>Whether the position is within any of the boundaries.</returns>
        protected bool SelfContains(int x, int y)
        {
            int lowX = GetX();
            int lowY = GetY();
            int highX = lowX + (int)GetWidth();
            int highY = lowY + (int)GetHeight();
            return x > lowX && x < highX
                && y > lowY && y < highY;
        }

        /// <summary>
        /// Elements queued to be added as children.
        /// </summary>
        private List<UIElement> ToAdd = new List<UIElement>();

        /// <summary>
        /// Elements queued to be removed as children.
        /// </summary>
        private List<UIElement> ToRemove = new List<UIElement>();

        /// <summary>
        /// Adds and removes any queued children.
        /// </summary>
        public void CheckChildren()
        {
            foreach (UIElement element in ToAdd)
            {
                if (!Children.Contains(element))
                {
                    Children.Add(element);
                    element.Parent = this;
                    element.Init();
                }
                else
                {
                    throw new Exception("Failed to add a child!");
                }
            }
            foreach (UIElement element in ToRemove)
            {
                if (Children.Remove(element))
                {
                    element.Destroy();
                    element.Parent = null;
                }
                else
                {
                    throw new Exception("Failed to remove a child!");
                }
            }
            ToAdd.Clear();
            ToRemove.Clear();
        }

        /// <summary>
        /// Performs a tick on this element and its children.
        /// </summary>
        /// <param name="delta">The time since the last tick.</param>
        public void FullTick(double delta)
        {
            CheckChildren();
            Tick(delta);
            TickChildren(delta);
        }

        /// <summary>
        /// Performs a tick on this element.
        /// </summary>
        /// <param name="delta">The time since the last tick.</param>
        protected virtual void Tick(double delta)
        {
        }

        /// <summary>
        /// Whether the mouse left button was previously down.
        /// </summary>
        private bool pDown;

        /// <summary>
        /// Performs a tick on this element's children.
        /// </summary>
        /// <param name="delta">The time since the last tick.</param>
        protected virtual void TickChildren(double delta)
        {
            int mX = Client.MouseX;
            int mY = Client.MouseY;
            bool mDown = Client.CurrentMouse.IsButtonDown(MouseButton.Left);
            foreach (UIElement element in Children)
            {
                if (element.Contains(mX, mY))
                {
                    if (!element.HoverInternal)
                    {
                        element.HoverInternal = true;
                        element.MouseEnter(mX, mY);
                    }
                    if (mDown && !pDown)
                    {
                        element.MouseLeftDown(mX, mY);
                    }
                    else if (!mDown && pDown)
                    {
                        element.MouseLeftUp(mX, mY);
                    }
                }
                else if (element.HoverInternal)
                {
                    element.HoverInternal = false;
                    element.MouseLeave(mX, mY);
                    if (mDown && !pDown)
                    {
                        element.MouseLeftDownOutside(mX, mY);
                    }
                }
                else if (mDown && !pDown)
                {
                    element.MouseLeftDownOutside(mX, mY);
                }
                element.FullTick(delta);
            }
            pDown = mDown;
            foreach (UIElement element in Children)
            {
                element.FullTick(delta);
            }
        }

        /// <summary>
        /// Performs a render on this element and its children.
        /// </summary>
        /// <param name="view">The UI view.</param>
        /// <param name="delta">The time since the last render.</param>
        /// <param name="xoff">The X offset of this element's parent.</param>
        /// <param name="yoff">The Y offset of this element's parent.</param>
        /// <param name="lastRot">The last rotation made in the render chain.</param>
        public virtual void FullRender(ViewUI2D view, double delta, int xoff, int yoff, Vector3 lastRot)
        {
            if (Parent == null || !Parent.ToRemove.Contains(this))
            {
                int x;
                int y;
                if (Math.Abs(lastRot.Z) < 0.001f)
                {
                    x = GetX() + xoff;
                    y = GetY() + yoff;
                    lastRot = new Vector3(GetWidth() * -0.5f, GetHeight() * -0.5f, GetRotation());
                }
                else
                {
                    x = GetX();
                    y = GetY();
                    int cwx = (Parent == null ? 0 : Position.MainAnchor.GetX(this));
                    int chy = (Parent == null ? 0 : Position.MainAnchor.GetY(this));
                    float half_wid = GetWidth() * 0.5f;
                    float half_hei = GetHeight() * 0.5f;
                    float tx = x + lastRot.X + cwx - half_wid;
                    float ty = y + lastRot.Y + chy - half_hei;
                    float cosRot = (float)Math.Cos(-lastRot.Z);
                    float sinRot = (float)Math.Sin(-lastRot.Z);
                    float tx2 = tx * cosRot - ty * sinRot - lastRot.X - cwx * 2 + half_wid;
                    float ty2 = ty * cosRot + tx * sinRot - lastRot.Y - chy * 2 + half_hei;
                    lastRot = new Vector3(-half_wid, -half_hei, lastRot.Z + GetRotation());
                    int bx = (int)tx2 + xoff;
                    int by = (int)ty2 + yoff;
                    xoff = bx - x;
                    yoff = by - y;
                    x = bx;
                    y = by;
                }
                Render(view, delta, xoff, yoff, lastRot.Z);
                RenderChildren(view, delta, x, y, lastRot);
            }
        }

        /// <summary>
        /// Performs a render on this element.
        /// </summary>
        /// <param name="view">The UI view.</param>
        /// <param name="delta">The time since the last render.</param>
        /// <param name="xoff">The X offset of this element's parent.</param>
        /// <param name="yoff">The Y offset of this element's parent.</param>
        /// <param name="rotation">The calculated rotation to make in this render call.</param>
        protected virtual void Render(ViewUI2D view, double delta, int xoff, int yoff, float rotation)
        {
        }

        /// <summary>
        /// Performs a render on this element's children.
        /// </summary>
        /// <param name="view">The UI view.</param>
        /// <param name="delta">The time since the last render.</param>
        /// <param name="xoff">The X offset of this element's parent.</param>
        /// <param name="yoff">The Y offset of this element's parent.</param>
        /// <param name="lastRot">The last rotation made in the render chain.</param>
        protected virtual void RenderChildren(ViewUI2D view, double delta, int xoff, int yoff, Vector3 lastRot)
        {
            CheckChildren();
            GraphicsUtil.CheckError("RenderElement - Children - Pre");
            foreach (UIElement element in Children)
            {
                element.FullRender(view, delta, xoff, yoff, lastRot);
            }
            GraphicsUtil.CheckError("RenderElement - Children - Post");
        }

        /// <summary>
        /// Fires <see cref="MouseEnter()"/>.
        /// </summary>
        /// <param name="x">The X position of the mouse.</param>
        /// <param name="y">The Y position of the mouse.</param>
        public void MouseEnter(int x, int y)
        {
            MouseEnter();
        }

        /// <summary>
        /// Fires <see cref="MouseLeave()"/>.
        /// </summary>
        /// <param name="x">The X position of the mouse.</param>
        /// <param name="y">The Y position of the mouse.</param>
        public void MouseLeave(int x, int y)
        {
            MouseLeave();
        }

        /// <summary>
        /// Fires <see cref="MouseLeftDown()"/> for all children included in the position.
        /// </summary>
        /// <param name="x">The X position of the mouse.</param>
        /// <param name="y">The Y position of the mouse.</param>
        public void MouseLeftDown(int x, int y)
        {
            MouseLeftDown();
            foreach (UIElement child in GetAllAt(x, y))
            {
                child.MouseLeftDown(x, y);
            }
        }

        /// <summary>
        /// <see cref="MouseLeftDownOutside()"/> for all children included in the position.
        /// </summary>
        /// <param name="x">The X position of the mouse.</param>
        /// <param name="y">The Y position of the mouse.</param>
        public void MouseLeftDownOutside(int x, int y)
        {
            MouseLeftDownOutside();
            foreach (UIElement child in GetAllNotAt(x, y))
            {
                child.MouseLeftDownOutside(x, y);
            }
        }

        /// <summary>
        /// Fires <see cref="MouseLeftUp()"/> for all children included in the position.
        /// </summary>
        /// <param name="x">The X position of the mouse.</param>
        /// <param name="y">The Y position of the mouse.</param>
        public void MouseLeftUp(int x, int y)
        {
            MouseLeftUp();
            foreach (UIElement child in GetAllAt(x, y))
            {
                child.MouseLeftUp(x, y);
            }
        }

        /// <summary>
        /// Ran when the mouse enters the boundaries of this element.
        /// </summary>
        protected virtual void MouseEnter()
        {
        }

        /// <summary>
        /// Ran when the mouse exits the boundaries of this element.
        /// </summary>
        protected virtual void MouseLeave()
        {
        }

        /// <summary>
        /// Ran when the left mouse button is pressed down within the boundaries of this element or its children.
        /// </summary>
        protected virtual void MouseLeftDown()
        {
        }

        /// <summary>
        /// Ran when the left mouse button is pressed down outside of the boundaries of this element or its children.
        /// </summary>
        protected virtual void MouseLeftDownOutside()
        {
        }

        /// <summary>
        /// Ran when the left mouse button is released within the boundaries of this element or its children.
        /// </summary>
        protected virtual void MouseLeftUp()
        {
        }

        /// <summary>
        /// Gets all children that contain the position on the screen.
        /// </summary>
        /// <param name="x">The X position to check for.</param>
        /// <param name="y">The Y position to check for.</param>
        /// <returns>A list of child elements containing the position.</returns>
        protected virtual List<UIElement> GetAllAt(int x, int y)
        {
            List<UIElement> found = new List<UIElement>();
            foreach (UIElement element in Children)
            {
                if (element.Contains(x, y))
                {
                    found.Add(element);
                }
            }
            return found;
        }

        /// <summary>
        /// Gets all children that do not contain the position on the screen.
        /// </summary>
        /// <param name="x">The X position to check for.</param>
        /// <param name="y">The Y position to check for.</param>
        /// <returns>A list of child elements not containing the position.</returns>
        protected virtual List<UIElement> GetAllNotAt(int x, int y)
        {
            List<UIElement> found = new List<UIElement>();
            foreach (UIElement element in Children)
            {
                if (!element.Contains(x, y))
                {
                    found.Add(element);
                }
            }
            return found;
        }

        /// <summary>
        /// Preps the element.
        /// </summary>
        protected virtual void Init()
        {
        }

        /// <summary>
        /// Destroys any data tracked by the element.
        /// </summary>
        protected virtual void Destroy()
        {
        }
    }
}
