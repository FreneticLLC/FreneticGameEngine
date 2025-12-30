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
using FGECore.MathHelpers;
using FGEGraphics.ClientSystem;
using FGEGraphics.GraphicsHelpers;
using FGEGraphics.UISystem.InputSystems;
using OpenTK.Mathematics;

using Vector2i = FGECore.MathHelpers.Vector2i;

namespace FGEGraphics.UISystem;

/// <summary>
/// Represents a single generic item in a UI.
/// <para>Sub-classes implement rendering and general logic for a specific type of UI element.</para>
/// </summary>
// TODO: Hover text
public abstract class UIElement
{
    /// <summary>The parent element, <c>null</c> if this element is the root or hasn't been added as a child.</summary>
    public UIElement Parent;

    /// <summary>Gets the UI view this element is attached to.</summary>
    public ViewUI2D View;

    /// <summary>Styling logic for this element.</summary>
    public UIStyling Styling;

    /// <summary>Gets the current element style.</summary>
    public UIStyle Style => ElementInternal.Style;

    /// <summary>The positioning, sizing, and rotation logic for this element.</summary>
    public UILayout Layout;

    /// <summary>Whether this element has been added to a parent element, <c>false</c> if not yet added or already removed.</summary>
    public bool IsValid;

    /// <summary>Whether this element can be interacted with.</summary>
    public bool IsEnabled = true;

    /// <summary>Whether the visual state of this element should be locked from interacted changes.</summary>
    public bool IsStateLocked = false;

    /// <summary>Whether the user is hovering over this element.</summary>
    public bool IsHovered = false;

    /// <summary>Whether the user is pressing this element.</summary>
    public bool IsPressed = false;

    /// <summary>Whether this is the last element the user has interacted with.</summary>
    public bool IsFocused = false;

    /// <summary>This absolute position.</summary>
    /// <seealso cref="X"/>
    /// <seealso cref="Y"/>
    public Vector2i Position;

    /// <summary>The absolute size.</summary>
    /// <seealso cref="Width"/>
    /// <seealso cref="Height"/>
    public Vector2i Size;

    /// <summary>The absolute rotation.</summary>
    public float Rotation;

    /// <summary>The absolute scale.</summary>
    public float Scale;

    /// <summary>Gets the absolute X coordinate.</summary>
    /// <seealso cref="Position"/>
    public int X => Position.X;

    /// <summary>Gets the absolute Y coordinate.</summary>
    /// <seealso cref="Position"/>
    public int Y => Position.Y;

    /// <summary>Gets the absolute width value.</summary>
    /// <seealso cref="Size"/>
    public int Width => Size.X;

    /// <summary>Gets the absolute height value.</summary>
    /// <seealso cref="Size"/>
    public int Height => Size.Y;

    /// <summary>An arbitrary value attached to this element.</summary>
    public object Tag = null;

    /// <summary>Whether this element should render itself. If <c>false</c>, <see cref="Render(double, UIStyle)"/> may be called manually.</summary>
    public bool RenderSelf = true;

    /// <summary>
    /// Whether this element should scale its width based on <see cref="Scale"/>.
    /// Even if this value is <c>false</c>, this element's scale still applies to its children.
    /// </summary>
    public bool ScaleSize = true;

    /// <summary>Fired when the user interacts with this element using a mouse, keyboard, or controller.</summary>
    public Action OnClick;

    /// <summary>Fired when the user focuses this element.</summary>
    public Action OnFocus;

    /// <summary>Fired when the user unfocuses this element.</summary>
    public Action OnUnfocus;

    /// <summary>Fired when <see cref="Style"/> changes value.</summary>
    public Action<UIStyle, UIStyle> OnStyleChange;

    /// <summary>Fired when <see cref="Position"/> changes value.</summary>
    public Action<Vector2i, Vector2i> OnPositionChange;

    /// <summary>Fired when <see cref="Size"/> changes value.</summary>
    public Action<Vector2i, Vector2i> OnSizeChange;

    /// <summary>Fired when <see cref="Rotation"/> changes value.</summary>
    public Action<float, float> OnRotationChange;

    /// <summary>Fired when <see cref="Scale"/> changes value.</summary>
    public Action<float, float> OnScaleChange;

    /// <summary>Data internal to a <see cref="UIElement"/> instance.</summary>
    public struct ElementInternalData()
    {
        /// <summary>This element's children.</summary>
        public List<UIElement> Children = [];

        /// <summary>Elements queued to be added as children.</summary>
        public List<UIElement> ToAdd = [];

        /// <summary>Child elements queued to be removed.</summary>
        public List<UIElement> ToRemove = [];

        /// <summary>Whether the mouse is hovering over this element.</summary>
        public bool IsMouseHovered;

        /// <summary>The last absolute position.</summary>
        public Vector2i LastPosition;

        /// <summary>The last absolute size.</summary>
        public Vector2i LastSize;

        /// <summary>The last absolute rotation.</summary>
        public float LastRotation;

        /// <summary>The last absolute scale.</summary>
        public float LastScale;

        /// <summary>The current style of this element.</summary>
        public UIStyle Style = UIStyle.Empty;

        /// <summary>Styles registered on this element.</summary>
        public HashSet<UIStyle> Styles = [];

        /// <summary>Text objects registered on this element.</summary>
        public List<UIText> Texts = [];
    }

    /// <summary>Data internal to a <see cref="UIElement"/> instance.</summary>
    public ElementInternalData ElementInternal = new();

    /// <summary>Constructs a new element to be placed on a <see cref="UIScreen"/>.</summary>
    /// <param name="styling">The styling logic of the element.</param>
    /// <param name="layout">The layout of the element.</param>
    public UIElement(UIStyling styling, UILayout layout)
    {
        Styling = styling;
        if (layout is not null)
        {
            Layout = layout;
            Layout.Element = this;
        }
    }

    /// <summary>Adds a child to this element.</summary>
    /// <param name="child">The element to be parented.</param>
    public virtual void AddChild(UIElement child)
    {
        if (child.IsValid)
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
        child.IsValid = true;
    }

    /// <summary>Removes a child from this element.</summary>
    /// <param name="child">The child to be removed.</param>
    public virtual void RemoveChild(UIElement child)
    {
        if (!child.IsValid)
        {
            return;
        }
        if (ElementInternal.Children.Contains(child))
        {
            ElementInternal.ToRemove.Add(child);
        }
        else if (!ElementInternal.ToAdd.Remove(child))
        {
            throw new Exception("Tried to remove a child that does not belong to this element!");
        }
        child.IsValid = false;
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

    /// <summary>Returns whether this element is the parent of another element.</summary>
    /// <param name="element">The possible child element.</param>
    public bool HasChild(UIElement element) => element.IsValid && (ElementInternal.Children.Contains(element) || ElementInternal.ToAdd.Contains(element)) && !ElementInternal.ToRemove.Contains(element);

    /// <summary>Adds and removes any queued children.</summary>
    public void UpdateChildren()
    {
        foreach (UIElement element in ElementInternal.ToAdd)
        {
            ElementInternal.Children.Add(element);
            element.Parent = this;
            if (View is not null)
            {
                foreach (UIElement child in element.AllChildren(toAdd: true))
                {
                    child.View = View;
                }
            }
            element.UpdateStyle();
            element.Init();
        }
        foreach (UIElement element in ElementInternal.ToRemove)
        {
            if (!ElementInternal.Children.Remove(element))
            {
                Logs.Error($"UIElement: Failed to remove a child element '{element}' from '{this}'!");
            }
            element.Destroy();
            element.Parent = null;
            element.View = null;
        }
        ElementInternal.ToAdd.Clear();
        ElementInternal.ToRemove.Clear();
    }

    // TODO: 'filter' predicate parameter
    /// <summary>Yields this element and all child elements recursively.</summary>
    /// <param name="includeSelf">Whether to include this element.</param>
    /// <param name="toAdd">Whether to include elements that are queued to be children.</param>
    public IEnumerable<UIElement> AllChildren(bool includeSelf = true, bool toAdd = false)
    {
        if (includeSelf)
        {
            yield return this;
        }
        foreach (UIElement element in ElementInternal.Children)
        {
            if (element.IsValid) // TODO: is this a good check?
            {
                foreach (UIElement child in element.AllChildren(true, toAdd))
                {
                    yield return child;
                }
            }
        }
        if (toAdd)
        {
            foreach (UIElement element in ElementInternal.ToAdd)
            {
                foreach (UIElement child in element.AllChildren(true, toAdd))
                {
                    yield return child;
                }
            }
        }
    }

    // TODO: remove these?
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

    /// <summary>Returns whether this element's boundaries contain the position on the screen.</summary>
    /// <param name="x">The X position to check for.</param>
    /// <param name="y">The Y position to check for.</param>
    public bool SelfContains(int x, int y) => x >= X && x < X + Width && y >= Y && y < Y + Height;

    /// <summary>Returns whether this element's boundaries, or any of its childrens' boundaries, contain the position on the screen.</summary>
    /// <param name="x">The X position to check for.</param>
    /// <param name="y">The Y position to check for.</param>
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

    /// <summary>
    /// Sets the style of this element.
    /// <para>Additionally, updates any <see cref="UIText"/> objects attached to this element.</para>
    /// <para>Fires <see cref="OnStyleChange"/> and <see cref="StyleChanged(UIStyle, UIStyle)"/>.</para>
    /// </summary>
    /// <param name="style">The new style. Defaults to <see cref="UIStyle.Empty"/> if <c>null</c>.</param>
    public void SetStyle(UIStyle style)
    {
        style ??= UIStyle.Empty;
        UIStyle previousStyle = ElementInternal.Style;
        if (previousStyle == style)
        {
            return;
        }
        if (ElementInternal.Styles.Add(style))
        {
            if (style.CanRenderText)
            {
                foreach (UIText text in ElementInternal.Texts)
                {
                    if (!text.Empty && text.Internal.Style is null)
                    {
                        text.Internal.Renderables[style] = text.CreateRenderable(style);
                    }
                }
            }
        }
        StyleChanged(previousStyle, ElementInternal.Style = style);
        OnStyleChange?.Invoke(previousStyle, style);
    }

    /// <summary>If a <see cref="Styling"/> is present, attempts to update the current style.</summary>
    public void UpdateStyle(bool updateText = false)
    {
        SetStyle(Styling.Get(this));
        if (updateText)
        {
            foreach (UIText text in ElementInternal.Texts)
            {
                text.UpdateRenderables();
            }
        }
    }

    /// <summary>
    /// Returns whether this element can be interacted with at the specified position.
    /// This constraint also affects this element's children.
    /// </summary>
    /// <param name="x">The X position to check for.</param>
    /// <param name="y">The Y position to check for.</param>
    public virtual bool CanInteract(int x, int y) => Parent?.CanInteract(x, y) ?? false;

    /// <summary>
    /// Ticks this element's interaction state. Should be called in the reverse of the rendering order.
    /// Elements with <see cref="IsEnabled"/> set to <c>false</c> are ignored by the interaction system.
    /// </summary>
    /// <param name="mouseX">The X position of the mouse.</param>
    /// <param name="mouseY">The Y position of the mouse.</param>
    /// <param name="scrollDelta">The scroll wheel change.</param>
    public virtual void TickInteraction(int mouseX, int mouseY, Vector2 scrollDelta)
    {
        if (SelfContains(mouseX, mouseY) && CanInteract(mouseX, mouseY))
        {
            if (ElementInternal.IsMouseHovered && !View.Internal.Scrolled)
            {
                if (scrollDelta.X != 0 || scrollDelta.Y != 0)
                {
                    View.Internal.Scrolled = MouseScrolled(scrollDelta.X, scrollDelta.Y);
                }
            }
            if (!ElementInternal.IsMouseHovered && View.HeldElement is null)
            {
                ElementInternal.IsMouseHovered = true;
                if (IsEnabled && !IsStateLocked)
                {
                    IsHovered = true;
                }
                MouseEntered();
            }
            if (View.MouseDown && !View.MousePreviouslyDown && View.HeldElement is null)
            {
                if (IsEnabled)
                {
                    if (!IsStateLocked)
                    {
                        IsPressed = true;
                    }
                    View.HeldElement = this;
                    Focus();
                }
                MousePressed();
            }
            else if (!View.MouseDown && View.MousePreviouslyDown && View.HeldElement == this)
            {
                if (IsEnabled)
                {
                    if (!IsStateLocked)
                    {
                        IsPressed = false;
                    }
                    OnClick?.Invoke();
                    Clicked();
                    View.HeldElement = null;
                }
                MouseReleased();
            }
        }
        else if (ElementInternal.IsMouseHovered && (!View.MouseDown || View.HeldElement != this))
        {
            ElementInternal.IsMouseHovered = false;
            if (IsEnabled)
            {
                if (!IsStateLocked)
                {
                    IsHovered = false;
                    IsPressed = false;
                }
                if (View.HeldElement == this)
                {
                    View.HeldElement = null;
                }
            }
            MouseExited();
            if (View.MousePreviouslyDown)
            {
                MouseReleasedOutside();
            }
        }
        if (View.MouseDown && View.HeldElement != this)
        {
            if (IsFocused)
            {
                Unfocus();
            }
            MousePressedOutside();
        }
    }

    // TODO: Gamepad input
    /// <summary>Ticks the <see cref="KeyHandler.BuildingState"/> on this element.</summary>
    public void TickInput()
    {
        if (!IsFocused)
        {
            return;
        }
        KeyHandlerState keys = View.Client.Keyboard.BuildingState;
        if (keys.Escaped)
        {
            Unfocus();
        }
        if (keys.LeftRights != 0 || keys.Scrolls != 0)
        {
            Navigated(keys.LeftRights, keys.Scrolls);
        }
    }

    /// <summary>Performs a tick on this element.</summary>
    /// <param name="delta">The time since the last tick.</param>
    public virtual void Tick(double delta)
    {
    }

    /// <summary>Performs a tick on this element and its children.</summary>
    /// <param name="delta">The time since the last tick.</param>
    public void TickAll(double delta)
    {
        foreach (UIElement element in AllChildren())
        {
            element.UpdateChildren();
            element.TickInput();
            element.Tick(delta);
        }
    }

    // TODO: Support rotations
    /// <summary>
    /// Updates the absolute layout values for this element in the following order:
    /// <list type="number">
    /// <item><see cref="Scale"/>, multiplied with all parent values</item>
    /// <item><see cref="Size"/>, dependent on scale if <see cref="ScaleSize"/> is <c>true</c></item>
    /// <item><see cref="Position"/>, computed based on size, rotation, and relative position to the parent, if any</item>
    /// <item><see cref="Rotation"/></item>
    /// </list>
    /// </summary>
    /// <param name="delta">The time since the last render.</param>
    /// <param name="rotation">The last rotation made in the render chain.</param>
    public virtual void UpdateTransforms(double delta, Vector3 rotation)
    {
        ElementInternal.LastPosition = Position;
        ElementInternal.LastSize = Size;
        ElementInternal.LastRotation = Rotation;
        ElementInternal.LastScale = Scale;
        Scale = Layout.Scale;
        if (ScaleSize)
        {
            Size = new((int)(Layout.Width * Scale), (int)(Layout.Height * Scale));
        }
        else
        {
            Size = new(Layout.Width, Layout.Height);
        }
        int x = Layout.X;
        int y = Layout.Y;
        if (Math.Abs(rotation.Z) < 0.001f)
        {
            if (Parent is not null)
            {
                x += Parent.X;
                y += Parent.Y;
            }
            rotation = new Vector3(Layout.Width * -0.5f, Layout.Height * -0.5f, Layout.Rotation);
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
        Position = new(x, y);
        Rotation = rotation.Z;
    }

    /// <summary>Fires relevant events if this element's transforms have changed.</summary>
    public bool HandleTransforms()
    {
        bool anyFired = false;
        if (ElementInternal.LastPosition != Position)
        {
            OnPositionChange?.Invoke(ElementInternal.LastPosition, Position);
            PositionChanged(ElementInternal.LastPosition, Position);
            anyFired |= true;
        }
        if (ElementInternal.LastSize != Size)
        {
            OnSizeChange?.Invoke(ElementInternal.LastSize, Size);
            SizeChanged(ElementInternal.LastSize, Size);
            anyFired |= true;
        }
        if (ElementInternal.LastRotation != Rotation)
        {
            OnRotationChange?.Invoke(ElementInternal.LastRotation, Rotation);
            RotationChanged(ElementInternal.LastRotation, Rotation);
            anyFired |= true;
        }
        if (ElementInternal.LastScale != Scale)
        {
            OnScaleChange?.Invoke(ElementInternal.LastScale, Scale);
            ScaleChanged(ElementInternal.LastScale, Scale);
            foreach (UIText text in ElementInternal.Texts)
            {
                text.UpdateRenderables();
            }
            anyFired = true;
        }
        return anyFired;
    }

    /// <summary>Renders this element.</summary>
    /// <param name="delta">The time since the last render.</param>
    /// <param name="style">The rendering style.</param>
    public virtual void Render(double delta, UIStyle style)
    {
    }

    /// <summary>Renders this element with the current <see cref="Style"/>.</summary>
    /// <param name="delta">The time since the last render.</param>
    public void Render(double delta) => Render(delta, Style);

    /// <summary>Renders this element and all of its children recursively.</summary>
    /// <param name="delta">The time since the last render.</param>
    public virtual void RenderAll(double delta)
    {
        GraphicsUtil.CheckError("UIElement - PreRender");
        Render(delta);
        GraphicsUtil.CheckError("UIElement - PostRenderSelf", this);
        foreach (UIElement child in ElementInternal.Children)
        {
            if (child.IsValid && child.RenderSelf)
            {
                child.RenderAll(delta);
            }
        }
        GraphicsUtil.CheckError("UIElement - PostRenderAll", this);
    }

    /// <summary>Ran when the mouse enters the boundaries of this element.</summary>
    public virtual void MouseEntered()
    {
    }

    /// <summary>Ran when the mouse exits the boundaries of this element.</summary>
    public virtual void MouseExited()
    {
    }

    /// <summary>Ran when the left mouse button is pressed down within the boundaries of this element or its children.</summary>
    public virtual void MousePressed()
    {
    }

    /// <summary>Ran when the left mouse button is pressed down outside of the boundaries of this element or its children.</summary>
    public virtual void MousePressedOutside()
    {
    }

    /// <summary>Ran when the left mouse button is released within the boundaries of this element or its children.</summary>
    public virtual void MouseReleased()
    {
    }

    /// <summary>Ran when the left mouse button is released outside of the boundaries of this element or its children.</summary>
    public virtual void MouseReleasedOutside()
    {
    }

    /// <summary>Ran when the user scrolls the mouse while hovering the element.</summary>
    /// <param name="horizontal">The horizontal scroll (positive for right, negative for left).</param>
    /// <param name="vertical">The vertical scroll (positive for up, negative for down).</param>
    /// <returns>Whether to consume mouse scroll for this interaction step (such that no other elements receive this event).</returns>
    public virtual bool MouseScrolled(float horizontal, float vertical) => false;

    /// <summary>Ran when the user navigates directionally while the element is <see cref="IsFocused"/>.</summary>
    /// <param name="horizontal">The horizontal shift (positive for right, negative for left).</param>
    /// <param name="vertical">The vertical shift (positive for up, negative for down).</param>
    public virtual void Navigated(int horizontal, int vertical)
    {
    }

    /// <summary>Ran when the user interacts with this element using a mouse, keyboard, or controller.</summary>
    public virtual void Clicked()
    { 
    }

    /// <summary>Ran when <see cref="Style"/> changes value.</summary>
    /// <param name="from">The previous style.</param>
    /// <param name="to">The new style.</param>
    public virtual void StyleChanged(UIStyle from, UIStyle to)
    {
    }

    /// <summary>Ran when <see cref="Position"/> changes value.</summary>
    /// <param name="from">The previous position.</param>
    /// <param name="to">The new position.</param>
    public virtual void PositionChanged(Vector2i from, Vector2i to)
    {
    }

    /// <summary>Ran when <see cref="Size"/> changes value.</summary>
    /// <param name="from">The previous size.</param>
    /// <param name="to">The new size.</param>
    public virtual void SizeChanged(Vector2i from, Vector2i to)
    {
    }

    /// <summary>Ran when <see cref="Rotation"/> changes value.</summary>
    /// <param name="from">The previous rotation.</param>
    /// <param name="to">The new rotation.</param>
    public virtual void RotationChanged(float from, float to)
    {
    }

    /// <summary>Ran when <see cref="Scale"/> changes value.</summary>
    /// <param name="from">The previous scale.</param>
    /// <param name="to">The new scale.</param>
    public virtual void ScaleChanged(float from, float to)
    {
    }

    /// <summary>Focuses on the element and fires <see cref="Focused"/>.</summary>
    public void Focus()
    {
        IsFocused = true;
        OnFocus?.Invoke();
        Focused();
    }

    /// <summary>Unfocuses the element and fires <see cref="Unfocused"/>.</summary>
    public void Unfocus()
    {
        IsFocused = false;
        OnUnfocus?.Invoke();
        Unfocused();
    }

    /// <summary>Ran when the user begins interacting with the element.</summary>
    public virtual void Focused()
    {
    }

    /// <summary>Ran when the user stops interacting with the element.</summary>
    public virtual void Unfocused()
    {
    }

    /// <summary>Preps the element.</summary>
    public virtual void Init()
    {
    }

    /// <summary>Destroys any data tracked by the element.</summary>
    public virtual void Destroy()
    {
    }

    /// <summary>Returns debug text to display when <see cref="ViewUI2D.IsDebug"/> mode is enabled.</summary>
    public virtual List<string> GetDebugInfo()
    {
        List<string> info = new(4)
        {
            $"^t^0^h^{(this == View.HeldElement ? "2" : "5")}^u{GetType()}",
            $"^r^t^0^h^o^e^7Position: ^3({X}, {Y}) ^&| ^7Dimensions: ^3({Width}w, {Height}h) ^&| ^7Rotation: ^3{Rotation}",
            $"^7Enabled: ^{(IsEnabled ? "2" : "1")}{IsEnabled} ^&| ^7Hovered: ^{(IsHovered ? "2" : "1")}{IsHovered} ^&| ^7Pressed: ^{(IsPressed ? "2" : "1")}{IsPressed} ^&| ^7Selected: ^{(IsFocused ? "2" : "1")}{IsFocused}"
        };
        if (ElementInternal.Styles.Count > 0)
        {
            List<string> styleNames = [.. ElementInternal.Styles.Select(style => style.Name is not null ? $"^{(style == ElementInternal.Style ? "3" : "7")}{style.Name}" : "^1unnamed")];
            info.Add($"^7Styles: {string.Join("^&, ", styleNames)}");
        }
        return info;
    }
}
