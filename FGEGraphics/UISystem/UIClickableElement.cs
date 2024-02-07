using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FGEGraphics.UISystem;

namespace FGEGraphics.UISystem;

/// <summary>Represents a clickable UI element on the screen.</summary>
public abstract class UIClickableElement : UIElement
{
    /// <summary>Ran when this element is clicked.</summary>
    public event EventHandler Clicked;

    /// <summary>Whether the mouse is hovering over this element.</summary>
    public bool Hovered = false;

    /// <summary>Whether this element is being clicked.</summary>
    public bool Pressed = false;

    // TODO: Maybe factor out into "UIInteractableElement" or something?
    /// <summary>Whether this element can be interacted with. Don't set directly, use <see cref="Enabled"/>.</summary>
    public bool Internal_Enabled = true;

    /// <summary>Gets or sets whether this element can be interacted with.</summary>
    public bool Enabled
    {
        get => Internal_Enabled;
        set
        {
            Internal_Enabled = value;
            if (!value)
            {
                Hovered = false;
                Pressed = false;
            }
        }
    }

    /// <summary>Constructs the clickable element.</summary>
    /// <param name="pos">The position of the element.</param>
    /// <param name="onClick">Ran when the element is clicked.</param>
    public UIClickableElement(UIPositionHelper pos, Action onClick = null) : base(pos)
    {
        if (onClick is not null)
        {
            Clicked += (_, _) => onClick();
        }
    }

    /// <summary>Ran when the mouse enters the boundaries of this element.</summary>
    public override void MouseEnter()
    {
        if (Enabled)
        {
            Hovered = true;
        }
    }

    /// <summary>Ran when the mouse exits the boundaries of this element.</summary>
    public override void MouseLeave()
    {
        if (Enabled)
        {
            Hovered = false;
            Pressed = false;
        }
    }

    /// <summary>Ran when the left mouse button is pressed down within the boundaries of this element.</summary>
    public override void MouseLeftDown()
    {
        if (Enabled)
        {
            Hovered = true;
            Pressed = true;
        }
    }

    /// <summary>Ran when the left mouse button is released within the boundaries of this element.</summary>
    public override void MouseLeftUp()
    {
        if (!Enabled)
        {
            return;
        }
        if (Clicked is not null && Pressed && Hovered)
        {
            Clicked.Invoke(this, null);
        }
        Pressed = false;
    }

    /// <summary>Represents a clickable UI element with distinct normal, hovering, and clicking styles.</summary>
    // TODO: Style for when enabled is false?
    // TODO: StyleGroup (or why haven't I done this yet?)
    public abstract class Styled : UIClickableElement
    {
        /// <summary>The render style to use when the element is not being interacted with.</summary>
        public UIElementStyle StyleNormal;

        /// <summary>The render style to use when the user is hovering their mouse cursor over this element.</summary>
        public UIElementStyle StyleHover;

        /// <summary>The render style to use when the user is clicking on this element.</summary>
        public UIElementStyle StyleClick;

        /// <summary>Constructs the styled clickable element.</summary>
        /// <param name="normal">The style to display when neither hovered nor clicked.</param>
        /// <param name="hover">The style to display when hovered.</param>
        /// <param name="click">The style to display when clicked.</param>
        /// <param name="pos">The position of the element.</param>
        /// <param name="requireText">Whether the styles must support text rendering.</param>
        /// <param name="onClick">Ran when the element is clicked.</param>
        public Styled(UIElementStyle normal, UIElementStyle hover, UIElementStyle click, UIPositionHelper pos, bool requireText = false, Action onClick = null) : base(pos, onClick)
        {
            StyleNormal = RegisterStyle(normal, requireText);
            StyleHover = RegisterStyle(hover, requireText);
            StyleClick = RegisterStyle(click, requireText);
        }

        /// <summary>Returns the normal, hover, or click style based on the current element state.</summary>
        public override UIElementStyle Style
        {
            get
            {
                if (Pressed)
                {
                    return StyleClick;
                }
                if (Hovered)
                {
                    return StyleHover;
                }
                return StyleNormal;
            }
        }
    }
}
