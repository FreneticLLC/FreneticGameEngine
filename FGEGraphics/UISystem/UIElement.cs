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
using FGECore.CoreSystems;
using FGEGraphics.ClientSystem;
using FGEGraphics.UISystem.InputSystems;
using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace FGEGraphics.UISystem;

/// <summary>
/// Represents a single generic item in a UI.
/// <para>Sub-classes implement rendering and general logic for a specific type of UI element.</para>
/// </summary>
// TODO: Hover text
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

    /// <summary>The UI view this element is attached to.</summary>
    public ViewUI2D View => Position.View;

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

    /// <summary>Last known aboslute rotation.</summary>
    public float LastAbsoluteRotation;

    /// <summary>The position and size of this element.</summary>
    public UIPositionHelper Position;

    /// <summary>Returns the <b>current</b> element style.</summary>
    public virtual UIElementStyle Style { get; set; }

    /// <summary>Whether this element should render automatically.</summary>
    public bool ShouldRender = true;

    /// <summary>Whether this element can be interacted with.</summary>
    public bool Enabled = true;

    /// <summary>Whether the mouse is hovering over this element.</summary>
    public bool Hovered = false;

    /// <summary>Whether this element is being clicked.</summary>
    public bool Pressed = false;

    /// <summary>Whether the element is the last being interacted with.</summary>
    public bool Selected = false;

    /// <summary>Ran when this element is clicked.</summary>
    public Action OnClick;

    /// <summary>Data internal to a <see cref="UIElement"/> instance.</summary>
    public struct ElementInternalData()
    {
        /// <summary>Current child elements.</summary>
        public HashSet<UIElement> Children = [];

        /// <summary>Elements queued to be added as children.</summary>
        public HashSet<UIElement> ToAdd = [];

        /// <summary>Elements queued to be removed as children.</summary>
        public HashSet<UIElement> ToRemove = [];

        /// <summary>Internal use only.</summary>
        public bool HoverInternal;

        /// <summary>Styles registered on this element.</summary>
        public HashSet<UIElementStyle> Styles = [];

        /// <summary>Text objects registered on this element.</summary>
        public List<UIElementText> Texts = [];

        /// <summary>The current style of this element.</summary>
        public UIElementStyle CurrentStyle = UIElementStyle.Empty;
    }

    /// <summary>Data internal to a <see cref="UIElement"/> instance.</summary>
    public ElementInternalData ElementInternal = new();

    /// <summary>Constructs a new element to be placed on a <see cref="UIScreen"/>.</summary>
    /// <param name="pos">The position of the element.</param>
    public UIElement(UIPositionHelper pos)
    {
        Position = pos;
        Position.For = this;
        //UpdatePosition(Window.Delta, Vector3.Zero);
    }

    /// <summary>Adds a child to this element.</summary>
    /// <param name="child">The element to be parented.</param>
    public virtual void AddChild(UIElement child)
    {
        if (child.Parent is not null)
        {
            throw new Exception("Tried to add a child that already has a parent!");
        }
        if (!ElementInternal.Children.Contains(child))
        {
            ElementInternal.ToAdd.Add(child);
        }
        else if (!ElementInternal.ToRemove.Remove(child))
        {
            throw new Exception("Tried to add a child that already belongs to this element!");
        }
        child.Parent = this;
        child.IsValid = true;
        child.Init();
    }

    /// <summary>Removes a child from this element.</summary>
    /// <param name="child">The element to be unparented.</param>
    public void RemoveChild(UIElement child)
    {
        if (ElementInternal.Children.Contains(child))
        {
            ElementInternal.ToRemove.Add(child);
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

    /// <summary>Adds and removes any queued children.</summary>
    public void CheckChildren()
    {
        foreach (UIElement element in ElementInternal.ToAdd)
        {
            if (!ElementInternal.Children.Add(element))
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

    // TODO: 'filter' predicate parameter?
    /// <summary>Yields this element and all child elements recursively.</summary>
    /// <param name="toAdd">Whether to include elements that are queued to be children.</param>
    public IEnumerable<UIElement> AllChildren(Func<UIElement, bool> filter = null, bool toAdd = false)
    {
        yield return this;
        foreach (UIElement element in ElementInternal.Children)
        {
            if (element.IsValid && (filter?.Invoke(element) ?? true))
            {
                foreach (UIElement child in element.AllChildren(filter, toAdd))
                {
                    yield return child;
                }
            }
        }
        if (toAdd)
        {
            foreach (UIElement element in ElementInternal.ToAdd)
            {
                if (filter?.Invoke(element) ?? true)
                {
                    foreach (UIElement child in element.AllChildren(filter, toAdd))
                    {
                        yield return child;
                    }
                }
            }
        }
    }

    // TODO: replace with AllChildren?
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

    /// <summary>Checks if this element's boundaries contain the position on the screen.</summary>
    /// <param name="x">The X position to check for.</param>
    /// <param name="y">The Y position to check for.</param>
    /// <returns>Whether the position is within any of the boundaries.</returns>
    public bool SelfContains(int x, int y) => x >= X && x < X + Width && y >= Y && y < Y + Height;

    /// <summary>Checks if this element's boundaries (or any of its children's boundaries) contain the position on the screen.</summary>
    /// <param name="x">The X position to check for.</param>
    /// <param name="y">The Y position to check for.</param>
    /// <returns>Whether the position is within any of the boundaries.</returns>
    public bool Contains(int x, int y)
    {
        foreach (UIElement element in AllChildren())
        {
            if (element.SelfContains(x, y))
            {
                return true;
            }
        }
        return false;
    }

    /// <summary>Registers a style to this element instance. Necessary when this element contains <see cref="UIElementText"/>.</summary>
    /// <param name="style">The style to register.</param>
    /// <param name="requireText">Whether the style must support text rendering.</param>
    public UIElementStyle AddStyle(UIElementStyle style, bool requireText = false)
    {
        if (ElementInternal.Styles.Contains(style))
        {
            return style;
        }
        if (requireText && !style.CanRenderText())
        {
            throw new Exception("Style must support text rendering when 'requireText' is true");
        }
        ElementInternal.Styles.Add(style);
        // TODO: avoid doing this on each call
        if (style.CanRenderText())
        {
            foreach (UIElementText text in ElementInternal.Texts)
            {
                text.RefreshRenderables();
            }
        }
        return style;
    }

    /// <summary>Updates the current style and fires relevant events if it has changed.</summary>
    public void UpdateStyle()
    {
        UIElementStyle newStyle = Style ?? UIElementStyle.Empty;
        if (newStyle != ElementInternal.CurrentStyle)
        {
            if (ElementInternal.CurrentStyle is not null)
            {
                SwitchFromStyle(ElementInternal.CurrentStyle);
            }
            SwitchToStyle(ElementInternal.CurrentStyle = newStyle);
        }
    }

    /// <summary>Ran when this element switches from the relevant <see cref="UIElementStyle"/>.</summary>
    public virtual void SwitchFromStyle(UIElementStyle style)
    {
    }

    /// <summary>Ran when this element switches to the relevant <see cref="UIElementStyle"/>.</summary>
    public virtual void SwitchToStyle(UIElementStyle style)
    {
    }

    /// <summary>
    /// Ticks this element's interaction state. Should be called in the reverse of the rendering order.
    /// Elements with <see cref="Enabled"/> set to <c>false</c> are ignored by the interaction system.
    /// </summary>
    /// <param name="mouseX">The X position of the mouse.</param>
    /// <param name="mouseY">The Y position of the mouse.</param>
    public void TickInteraction(int mouseX, int mouseY, Vector2 scrollDelta)
    {
        if (SelfContains(mouseX, mouseY) && CanInteract(mouseX, mouseY))
        {
            if (ElementInternal.HoverInternal && !View.Internal.Scrolled)
            {
                if (scrollDelta.X != 0 || scrollDelta.Y != 0)
                {
                    View.Internal.Scrolled = ScrollDirection(scrollDelta.X, scrollDelta.Y);
                }
            }
            if (!ElementInternal.HoverInternal && View.HeldElement is null)
            {
                ElementInternal.HoverInternal = true;
                if (Enabled)
                {
                    Hovered = true;
                }
                MouseEnter();
            }
            if (View.MouseDown && !View.MousePreviouslyDown && View.HeldElement is null)
            {
                if (Enabled)
                {
                    Pressed = true;
                    View.HeldElement = this;
                    Select();
                }
                MouseLeftDown(mouseX, mouseY);
            }
            else if (!View.MouseDown && View.MousePreviouslyDown && View.HeldElement == this)
            {
                if (Enabled)
                {
                    Pressed = false;
                    OnClick?.Invoke();
                    View.HeldElement = null;
                }
                MouseLeftUp(mouseX, mouseY);
            }
        }
        else if (ElementInternal.HoverInternal && (!View.MouseDown || View.HeldElement != this))
        {
            ElementInternal.HoverInternal = false;
            if (Enabled)
            {
                Hovered = false;
                Pressed = false;
                if (View.HeldElement == this)
                {
                    View.HeldElement = null;
                }
            }
            if (View.MousePreviouslyDown)
            {
                MouseLeftUpOutside(mouseX, mouseY);
            }
        }
        if (View.MouseDown && View.HeldElement != this)
        {
            if (Selected)
            {
                Deselect();
            }
            MouseLeftDownOutside(mouseX, mouseY);
        }
    }

    // TODO: Gamepad input
    /// <summary>Ticks the <see cref="KeyHandler.BuildingState"/> on this element.</summary>
    public void TickInput()
    {
        if (!Selected)
        {
            return;
        }
        KeyHandlerState keys = Window.Keyboard.BuildingState;
        if (keys.Escaped)
        {
            Deselect();
        }
        if (keys.LeftRights != 0 || keys.Scrolls != 0)
        {
            NavigateDirection(keys.LeftRights, keys.Scrolls);
        }
    }

    /// <summary>Performs a tick on this element.</summary>
    /// <param name="delta">The time since the last tick.</param>
    public virtual void Tick(double delta)
    {
    }

    /// <summary>Performs a tick on this element and its children.</summary>
    /// <param name="delta">The time since the last tick.</param>
    public void FullTick(double delta)
    {
        foreach (UIElement element in AllChildren())
        {
            element.CheckChildren();
            element.UpdateStyle();
            element.TickInput();
            element.Tick(delta);
        }
    }

    /// <summary>Updates positions of this element and its children.</summary>
    /// <param name="delta">The time since the last render.</param>
    /// <param name="rotation">The last rotation made in the render chain.</param>
    public virtual void UpdatePosition(double delta, Vector3 rotation)
    {
        int x = Position.X;
        int y = Position.Y;
        if (Math.Abs(rotation.Z) < 0.001f)
        {
            if (Parent is not null)
            {
                x += Parent.X;
                y += Parent.Y;
            }
            rotation = new Vector3(Position.Width * -0.5f, Position.Height * -0.5f, Position.Rotation);
        }
        // TODO: Clean up!
        /*else 
        {
            int cwx = Parent is null ? 0 : Position.MainAnchor.GetX(this);
            int chy = Parent is null ? 0 : Position.MainAnchor.GetY(this);
            float half_wid = Position.Width * 0.5f;
            float half_hei = Position.Height * 0.5f;
            float tx = x + rotation.X + cwx - half_wid;
            float ty = y + rotation.Y + chy - half_hei;
            float cosRot = (float)Math.Cos(-rotation.Z);
            float sinRot = (float)Math.Sin(-rotation.Z);
            float tx2 = tx * cosRot - ty * sinRot - rotation.X - cwx * 2 + half_wid;
            float ty2 = ty * cosRot + tx * sinRot - rotation.Y - chy * 2 + half_hei;
            rotation = new Vector3(-half_wid, -half_hei, rotation.Z + Position.Rotation);
            int bx = (int)tx2 + xoff;
            int by = (int)ty2 + yoff;
            x = bx;
            y = by;
        }*/
        LastAbsolutePosition = new FGECore.MathHelpers.Vector2i(x, y);
        LastAbsoluteRotation = rotation.Z;
        LastAbsoluteSize = new FGECore.MathHelpers.Vector2i(Position.Width, Position.Height);
        /*CheckChildren();
        foreach (UIElement child in ElementInternal.Children)
        {
            child.UpdatePosition(output, delta, x, y, lastRot);
        }*/
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
        foreach (UIElement element in AllChildren())
        {
            element.Render(view, delta, element.ElementInternal.CurrentStyle);
        }
    }

    public virtual bool CanRenderChild(UIElement child) => true;

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

    /// <summary>Fires <see cref="MouseLeftUpOutside()"/> for all children included in the position.</summary>
    /// <param name="x">The X position of the mouse.</param>
    /// <param name="y">The Y position of the mouse.</param>
    public void MouseLeftUpOutside(int x, int y)
    {
        MouseLeftUpOutside();
        foreach (UIElement child in GetChildrenNotAt(x, y))
        {
            child.MouseLeftUpOutside(x, y);
        }
    }

    /// <summary>Ran when the mouse enters the boundaries of this element.</summary>
    public virtual void MouseEnter()
    {
    }

    /// <summary>Ran when the mouse exits the boundaries of this element.</summary>
    // TODO: Account for in interaction logic
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

    /// <summary>Ran when the left mouse button is released outside of the boundaries of this element or its children.</summary>
    public virtual void MouseLeftUpOutside()
    {
    }

    /// <summary>Ran when the user navigates directionally while the element is <see cref="Selected"/>.</summary>
    /// <param name="horizontal">The horizontal shift (positive for right, negative for left).</param>
    /// <param name="vertical">The vertical shift (positive for up, negative for down).</param>
    public virtual void NavigateDirection(int horizontal, int vertical)
    {
    }

    /// <summary>Ran when the user scrolls the mouse while hovering the element.</summary>
    /// <param name="horizontal">The horizontal scroll (positive for right, negative for left).</param>
    /// <param name="vertical">The vertical scroll (positive for up, negative for down).</param>
    /// <returns>Whether to consume mouse scroll for this interaction step (such that no other elements receive this event).</returns>
    public virtual bool ScrollDirection(float horizontal, float vertical) => false;

    /// <summary>Selects the element and fires <see cref="OnSelect"/>.</summary>
    public void Select()
    {
        Selected = true;
        OnSelect();
    }

    /// <summary>Deselects the element and fires <see cref="OnDeselect"/>.</summary>
    public void Deselect()
    {
        Selected = false;
        OnDeselect();
    }

    /// <summary>Ran when the user has begun interacting with the element.</summary>
    public virtual void OnSelect()
    {
    }

    /// <summary>Ran when the user has stopped interacting with the element.</summary>
    public virtual void OnDeselect()
    {
    }

    /// <summary>
    /// Returns whether this element can be interacted with at the specified position.
    /// This constraint also affects this element's children.
    /// </summary>
    /// <param name="x">The X position to check for.</param>
    /// <param name="y">The Y position to check for.</param>
    public virtual bool CanInteract(int x, int y) => Parent?.CanInteract(x, y) ?? false;

    /// <summary>Preps the element.</summary>
    public virtual void Init()
    {
    }

    /// <summary>Destroys any data tracked by the element.</summary>
    public virtual void Destroy()
    {
    }

    /// <summary>Returns debug text to add to <see cref="ViewUI2D.InternalData.DebugInfo"/>.</summary>
    public virtual List<string> GetDebugInfo()
    {
        List<string> info = new(4)
        {
            $"^t^0^h^{(this == Position.View.HeldElement ? "2" : "5")}^u{GetType()}",
            $"^r^t^0^h^o^e^7Position: ^3({X}, {Y}) ^&| ^7Dimensions: ^3({Width}w, {Height}h) ^&| ^7Rotation: ^3{LastAbsoluteRotation}",
            $"^7Enabled: ^{(Enabled ? "2" : "1")}{Enabled} ^&| ^7Hovered: ^{(Hovered ? "2" : "1")}{Hovered} ^&| ^7Pressed: ^{(Pressed ? "2" : "1")}{Pressed} ^&| ^7Selected: ^{(Selected ? "2" : "1")}{Selected}"
        };
        if (ElementInternal.Styles.Count > 0)
        {
            List<string> styleNames = [.. ElementInternal.Styles.Select(style => style.Name is not null ? $"^{(style == ElementInternal.CurrentStyle ? "3" : "7")}{style.Name}" : "^1unnamed")];
            info.Add($"^7Styles: {string.Join("^&, ", styleNames)}");
        }
        return info;
    }
}
