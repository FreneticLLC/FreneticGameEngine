//
// This file is part of the Frenetic Game Engine, created by Frenetic LLC.
// This code is Copyright (C) Frenetic LLC under the terms of a strict license.
// See README.md or LICENSE.txt in the FreneticGameEngine source root for the contents of the license.
// If neither of these are available, assume that neither you nor anyone other than the copyright holder
// hold any right or permission to use this software until such time as the official license is identified.
//

using System;
using System.Collections.Generic;
using FGEGraphics.ClientSystem;
using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace FGEGraphics.UISystem;

/// <summary>
/// Represents a single generic item in a UI.
/// <para>Sub-classes implement rendering and general logic for a specific type of UI element.</para>
/// </summary>
public abstract class UIElement
{
    /// <summary>The parent of this element.</summary>
    public UIElement Parent;

    /// <summary>True when the element is valid and usable, false when not-yet-added or already removed.</summary>
    public bool IsValid;

    /// <summary>Gets the client game window used to render this element.</summary>
    public virtual GameClientWindow Window => Parent.Window;

    /// <summary>Gets the client game engine used to render this element.</summary>
    public virtual GameEngineBase Engine => Parent.Engine;

    /// <summary>Last known absolute position.</summary>
    public FGECore.MathHelpers.Vector2i LastAbsolutePosition;

    /// <summary>Last known absolute size (Width and Height).</summary>
    public FGECore.MathHelpers.Vector2i LastAbsoluteSize;

    /// <summary>Last known absolute X position (from <see cref="LastAbsolutePosition"/>).</summary>
    public int X => LastAbsolutePosition.X;

    /// <summary>Last known absolute Y position (from <see cref="LastAbsolutePosition"/>).</summary>
    public int Y => LastAbsolutePosition.Y;

    /// <summary>Last known absolute width (from <see cref="LastAbsoluteSize"/>).</summary>
    public int Width => LastAbsoluteSize.X;

    /// <summary>Last known absolute height (from <see cref="LastAbsoluteSize"/>).</summary>
    public int Height => LastAbsoluteSize.Y;

    /// <summary>Whether the element has a parent element.</summary>
    public bool HasParent => Parent is not null && !Parent.ElementInternal.ToRemove.Contains(this);

    /// <summary>Last known aboslute rotation.</summary>
    public float LastAbsoluteRotation;

    /// <summary>The position and size of this element.</summary>
    public UIPositionHelper Position;

    /// <summary>Whether this element should render automatically.</summary>
    public bool ShouldRender;

    /// <summary>Gets or sets whether this element can be interacted with.</summary>
    public bool Enabled
    {
        get => ElementInternal.Enabled;
        set
        {
            ElementInternal.Enabled = value;
            if (!value)
            {
                Hovered = false;
                Pressed = false;
            }
        }
    }

    /// <summary>Whether the mouse is hovering over this element.</summary>
    public bool Hovered = false;

    /// <summary>Whether this element is being clicked.</summary>
    public bool Pressed = false;

    /// <summary>Ran when this element is clicked.</summary>
    public Action Clicked;

    /// <summary>
    /// Priority for rendering logic.
    /// <para>Only used if <see cref="ViewUI2D.SortToPriority"/> is enabled.</para>
    /// </summary>
    public double RenderPriority = 0;

    /// <summary>Whether this element has rendering priority over its parent, if any.</summary>
    public bool ChildPriority = true;

    /// <summary>Constructs a new element to be placed on a <see cref="UIScreen"/>.</summary>
    /// <param name="pos">The position of the element.</param>
    /// <param name="shouldRender">Whether the element should render automatically.</param>
    public UIElement(UIPositionHelper pos, bool shouldRender = true)
    {
        Position = pos;
        Position.For = this;
        ShouldRender = shouldRender;
        LastAbsolutePosition = new FGECore.MathHelpers.Vector2i(Position.X, Position.Y);
        LastAbsoluteSize = new FGECore.MathHelpers.Vector2i(Position.Width, Position.Height);
        LastAbsoluteRotation = Position.Rotation;
    }

    /// <summary>Adds a child to this element.</summary>
    /// <param name="child">The element to be parented.</param>
    /// <param name="priority">Whether the child element has render priority over this element.</param>
    public virtual void AddChild(UIElement child, bool priority = true)
    {
        if (child.Parent is not null)
        {
            throw new Exception("Tried to add a child that already has a parent!");
        }
        if (!ElementInternal.Children.Contains(child))
        {
            if (!ElementInternal.ToAdd.Contains(child))
            {
                ElementInternal.ToAdd.Add(child);
            }
        }
        else if (!ElementInternal.ToRemove.Remove(child))
        {
            throw new Exception("Tried to add a child that already belongs to this element!");
        }
        child.Parent = this;
        child.IsValid = true;
        child.ChildPriority = priority;
        child.Init();
    }

    /// <summary>Removes a child from this element.</summary>
    /// <param name="child">The element to be unparented.</param>
    public void RemoveChild(UIElement child)
    {
        if (ElementInternal.Children.Contains(child))
        {
            if (!ElementInternal.ToRemove.Contains(child))
            {
                ElementInternal.ToRemove.Add(child);
            }
        }
        else if (!ElementInternal.ToAdd.Remove(child))
        {
            throw new Exception("Tried to remove a child that does not belong to this element!");
        }
        child.IsValid = false;
        child.Parent = null;
        child.Destroy();
    }

    /// <summary>Removes all children from this element.</summary>
    public void RemoveAllChildren()
    {
        foreach (UIElement child in ElementInternal.Children)
        {
            RemoveChild(child);
        }
        ElementInternal.ToAdd.Clear();
    }

    /// <summary>Checks if this element has the specified child.</summary>
    /// <param name="element">The possible child.</param>
    public bool HasChild(UIElement element)
    {
        return element.IsValid && (ElementInternal.Children.Contains(element) || ElementInternal.ToAdd.Contains(element)) && !ElementInternal.ToRemove.Contains(element);
    }

    /// <summary>Checks if this element's boundaries (or any of its children's boundaries) contain the position on the screen.</summary>
    /// <param name="x">The X position to check for.</param>
    /// <param name="y">The Y position to check for.</param>
    /// <returns>Whether the position is within any of the boundaries.</returns>
    public bool Contains(int x, int y)
    {
        foreach (UIElement child in ElementInternal.Children)
        {
            if (child.IsValid && child.Contains(x, y))
            {
                return true;
            }
        }
        return SelfContains(x, y);
    }

    /// <summary>Checks if this element's boundaries contain the position on the screen.</summary>
    /// <param name="x">The X position to check for.</param>
    /// <param name="y">The Y position to check for.</param>
    /// <returns>Whether the position is within any of the boundaries.</returns>
    public bool SelfContains(int x, int y)
    {
        int lowX = LastAbsolutePosition.X;
        int lowY = LastAbsolutePosition.Y;
        int highX = lowX + LastAbsoluteSize.X;
        int highY = lowY + LastAbsoluteSize.Y;
        return x > lowX && x < highX
            && y > lowY && y < highY;
    }

    /// <summary>Data internal to a <see cref="UIElement"/> instance.</summary>
    public struct ElementInternalData()
    {
        /// <summary>Current child elements.</summary>
        public List<UIElement> Children = [];

        /// <summary>Elements queued to be added as children.</summary>
        public List<UIElement> ToAdd = [];

        /// <summary>Elements queued to be removed as children.</summary>
        public List<UIElement> ToRemove = [];

        /// <summary>Whether the mouse left button was previously down.</summary>
        public bool MousePreviouslyDown;

        /// <summary>Internal use only.</summary>
        public bool HoverInternal;

        /// <summary>Whether this element can be interacted with.</summary>
        public bool Enabled = true;

        /// <summary>Styles registered on this element.</summary>
        public List<UIElementStyle> Styles = [];

        /// <summary>Text objects registered on this element.</summary>
        public List<UIElementText> Texts = [];

        /// <summary>The current style of this element.</summary>
        public UIElementStyle CurrentStyle;
    }

    /// <summary>Data internal to a <see cref="UIElement"/> instance.</summary>
    public ElementInternalData ElementInternal = new();

    /// <summary>Adds and removes any queued children.</summary>
    public void CheckChildren()
    {
        foreach (UIElement element in ElementInternal.ToAdd)
        {
            if (!ElementInternal.Children.Contains(element))
            {
                ElementInternal.Children.Add(element);
            }
            else
            {
                throw new Exception($"UIElement: Failed to add a child element {element}!");
            }
        }
        foreach (UIElement element in ElementInternal.ToRemove)
        {
            if (!ElementInternal.Children.Remove(element))
            {
                throw new Exception($"UIElement: Failed to remove a child element {element}!");
            }
        }
        ElementInternal.ToAdd.Clear();
        ElementInternal.ToRemove.Clear();
    }

    /// <summary>Performs a tick on this element and its children.</summary>
    /// <param name="delta">The time since the last tick.</param>
    public void FullTick(double delta)
    {
        CheckChildren();
        UpdateStyle();
        Tick(delta);
        TickChildren(delta);
    }

    /// <summary>Registers a style to this element instance. Necessary when this element contains <see cref="UIElementText"/>.</summary>
    /// <param name="style">The style to register.</param>
    /// <param name="requireText">Whether the style must support text rendering.</param>
    public UIElementStyle AddStyle(UIElementStyle style, bool requireText = false)
    {
        if (requireText && !style.CanRenderText())
        {
            throw new Exception("Style must support text rendering when 'requireText' is true");
        }
        ElementInternal.Styles.Add(style);
        if (style.CanRenderText())
        {
            foreach (UIElementText text in ElementInternal.Texts)
            {
                text.RefreshRenderables();
            }
        }
        return style;
    }

    /// <summary>Returns the <b>current</b> element style.</summary>
    public virtual UIElementStyle Style => UIElementStyle.Empty;

    /// <summary>Ran when this element switches from the relevant <see cref="UIElementStyle"/>.</summary>
    public virtual void SwitchFromStyle(UIElementStyle style)
    {
    }

    /// <summary>Ran when this element switches to the relevant <see cref="UIElementStyle"/>.</summary>
    public virtual void SwitchToStyle(UIElementStyle style)
    {
    }

    /// <summary>Performs a tick on this element.</summary>
    /// <param name="delta">The time since the last tick.</param>
    public virtual void Tick(double delta)
    {
    }

    /// <summary>Performs a tick on this element's children.</summary>
    /// <param name="delta">The time since the last tick.</param>
    public virtual void TickChildren(double delta)
    {
        int mX = (int)Window.MouseX; // TODO: Propagate float support.
        int mY = (int)Window.MouseY;
        bool mDown = Window.CurrentMouse.IsButtonDown(MouseButton.Left);
        foreach (UIElement element in ElementInternal.Children)
        {
            if (!element.IsValid)
            {
                continue;
            }
            if (element.Contains(mX, mY))
            {
                if (!element.ElementInternal.HoverInternal)
                {
                    element.ElementInternal.HoverInternal = true;
                    if (element.Enabled)
                    {
                        element.Hovered = true;
                    }
                    element.MouseEnter();
                }
                if (mDown && !ElementInternal.MousePreviouslyDown)
                {
                    if (element.Enabled)
                    {
                        element.Pressed = true;
                    }
                    element.MouseLeftDown(mX, mY);
                }
                else if (!mDown && ElementInternal.MousePreviouslyDown)
                {
                    if (element.Enabled && element.Clicked is not null)
                    {
                        element.Pressed = false;
                        element.Clicked();
                    }
                    element.MouseLeftUp(mX, mY);
                }
            }
            else if (element.ElementInternal.HoverInternal)
            {
                element.ElementInternal.HoverInternal = false;
                if (element.Enabled)
                {
                    element.Hovered = false;
                    element.Pressed = false;
                }
                element.MouseLeave();
                if (mDown && !ElementInternal.MousePreviouslyDown)
                {
                    element.MouseLeftDownOutside(mX, mY);
                }
            }
            else if (mDown && !ElementInternal.MousePreviouslyDown)
            {
                element.MouseLeftDownOutside(mX, mY);
            }
            element.FullTick(delta);
        }
        ElementInternal.MousePreviouslyDown = mDown;
    }

    /// <summary>Updates positions of this element and its children.</summary>
    /// <param name="output">The UI elements created. Add all validly updated elements to list.</param>
    /// <param name="delta">The time since the last render.</param>
    /// <param name="xoff">The X offset of this element's parent.</param>
    /// <param name="yoff">The Y offset of this element's parent.</param>
    /// <param name="lastRot">The last rotation made in the render chain.</param>
    public virtual void UpdatePositions(IList<UIElement> output, double delta, int xoff, int yoff, Vector3 lastRot)
    {
        if (Parent is not null && Parent.ElementInternal.ToRemove.Contains(this))
        {
            return;
        }
        int x = Position.X;
        int y = Position.Y;
        if (Position.MainAnchor == UIAnchor.RELATIVE)
        {
            y += Position.View.RelativeYLast;
        }
        if (Math.Abs(lastRot.Z) < 0.001f)
        {
            x += xoff;
            y += yoff;
            lastRot = new Vector3(Position.Width * -0.5f, Position.Height * -0.5f, Position.Rotation);
        }
        else
        {
            int cwx = Parent is null ? 0 : Position.MainAnchor.GetX(this);
            int chy = Parent is null ? 0 : Position.MainAnchor.GetY(this);
            float half_wid = Position.Width * 0.5f;
            float half_hei = Position.Height * 0.5f;
            float tx = x + lastRot.X + cwx - half_wid;
            float ty = y + lastRot.Y + chy - half_hei;
            float cosRot = (float)Math.Cos(-lastRot.Z);
            float sinRot = (float)Math.Sin(-lastRot.Z);
            float tx2 = tx * cosRot - ty * sinRot - lastRot.X - cwx * 2 + half_wid;
            float ty2 = ty * cosRot + tx * sinRot - lastRot.Y - chy * 2 + half_hei;
            lastRot = new Vector3(-half_wid, -half_hei, lastRot.Z + Position.Rotation);
            int bx = (int)tx2 + xoff;
            int by = (int)ty2 + yoff;
            x = bx;
            y = by;
        }
        LastAbsolutePosition = new FGECore.MathHelpers.Vector2i(x, y);
        LastAbsoluteRotation = lastRot.Z;
        LastAbsoluteSize = new FGECore.MathHelpers.Vector2i(Position.Width, Position.Height);
        Position.View.RelativeYLast = y + LastAbsoluteSize.Y;
        CheckChildren();
        UpdateChildPositions(output, delta, x, y, lastRot, false);
        output.Add(this);
        UpdateChildPositions(output, delta, x, y, lastRot, true);
    }

    /// <summary>Updates the current style and fires relevant events if it has changed.</summary>
    public void UpdateStyle()
    {
        if (Style != ElementInternal.CurrentStyle)
        {
            if (ElementInternal.CurrentStyle is not null)
            {
                SwitchFromStyle(ElementInternal.CurrentStyle);
            }
            SwitchToStyle(ElementInternal.CurrentStyle = Style);
        }
    }

    /// <summary>Performs a render on this element.</summary>
    /// <param name="view">The UI view.</param>
    /// <param name="delta">The time since the last render.</param>
    /// <param name="style">The current element style.</param>
    public virtual void Render(ViewUI2D view, double delta, UIElementStyle style)
    {
    }

    /// <summary>Performs a render on this element using the current style.</summary>
    /// <param name="view">The UI view.</param>
    /// <param name="delta">The time since the last render.</param>
    public void Render(ViewUI2D view, double delta)
    {
        if (IsValid)
        {
            Render(view, delta, ElementInternal.CurrentStyle);
        }
    }

    /// <summary>Updates this element's child positions.</summary>
    /// <param name="output">The UI elements created. Add all validly updated elements to list.</param>
    /// <param name="delta">The time since the last render.</param>
    /// <param name="xoff">The X offset of this element's parent.</param>
    /// <param name="yoff">The Y offset of this element's parent.</param>
    /// <param name="lastRot">The last rotation made in the render chain.</param>
    /// <param name="childPriority">The priority value to target.</param>
    public virtual void UpdateChildPositions(IList<UIElement> output, double delta, int xoff, int yoff, Vector3 lastRot, bool childPriority)
    {
        foreach (UIElement element in ElementInternal.Children)
        {
            if (element.IsValid && element.ChildPriority == childPriority)
            {
                element.UpdatePositions(output, delta, xoff, yoff, lastRot);
            }
        }
    }

    /// <summary>Fires <see cref="MouseLeftDown()"/> for all children included in the position.</summary>
    /// <param name="x">The X position of the mouse.</param>
    /// <param name="y">The Y position of the mouse.</param>
    public void MouseLeftDown(int x, int y)
    {
        MouseLeftDown();
        foreach (UIElement child in GetChildrenAt(x, y))
        {
            child.MouseLeftDown(x, y);
        }
    }

    /// <summary>Fires <see cref="MouseLeftDownOutside()"/> for all children included in the position.</summary>
    /// <param name="x">The X position of the mouse.</param>
    /// <param name="y">The Y position of the mouse.</param>
    public void MouseLeftDownOutside(int x, int y)
    {
        MouseLeftDownOutside();
        foreach (UIElement child in GetChildrenNotAt(x, y))
        {
            child.MouseLeftDownOutside(x, y);
        }
    }

    /// <summary>Fires <see cref="MouseLeftUp()"/> for all children included in the position.</summary>
    /// <param name="x">The X position of the mouse.</param>
    /// <param name="y">The Y position of the mouse.</param>
    public void MouseLeftUp(int x, int y)
    {
        MouseLeftUp();
        foreach (UIElement child in GetChildrenAt(x, y))
        {
            child.MouseLeftUp(x, y);
        }
    }

    /// <summary>Ran when the mouse enters the boundaries of this element.</summary>
    public virtual void MouseEnter()
    {
    }

    /// <summary>Ran when the mouse exits the boundaries of this element.</summary>
    public virtual void MouseLeave()
    {
    }

    /// <summary>Ran when the left mouse button is pressed down within the boundaries of this element or its children.</summary>
    public virtual void MouseLeftDown()
    {
    }

    /// <summary>Ran when the left mouse button is pressed down outside of the boundaries of this element or its children.</summary>
    public virtual void MouseLeftDownOutside()
    {
    }

    /// <summary>Ran when the left mouse button is released within the boundaries of this element or its children.</summary>
    public virtual void MouseLeftUp()
    {
    }

    /// <summary>Gets all children that contain the position on the screen.</summary>
    /// <param name="x">The X position to check for.</param>
    /// <param name="y">The Y position to check for.</param>
    /// <returns>A list of child elements containing the position.</returns>
    public virtual IEnumerable<UIElement> GetChildrenAt(int x, int y)
    {
        foreach (UIElement element in ElementInternal.Children)
        {
            if (element.IsValid && element.Contains(x, y))
            {
                yield return element;
            }
        }
    }

    /// <summary>Gets all children that do not contain the position on the screen.</summary>
    /// <param name="x">The X position to check for.</param>
    /// <param name="y">The Y position to check for.</param>
    /// <returns>A list of child elements not containing the position.</returns>
    public virtual IEnumerable<UIElement> GetChildrenNotAt(int x, int y)
    {
        foreach (UIElement element in ElementInternal.Children)
        {
            if (element.IsValid && !element.Contains(x, y))
            {
                yield return element;
            }
        }
    }

    /// <summary>Preps the element.</summary>
    public virtual void Init()
    {
    }

    /// <summary>Destroys any data tracked by the element.</summary>
    public virtual void Destroy()
    {
    }
}
