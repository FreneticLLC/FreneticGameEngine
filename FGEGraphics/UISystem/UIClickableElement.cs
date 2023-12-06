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
    public Action ClickedTask;

    /// <summary>Whether the mouse is hovering over this element.</summary>
    public bool Hovered = false;

    /// <summary>Whether this element is being clicked.</summary>
    public bool Clicked = false;

    public UIClickableElement(UIPositionHelper pos, Action clickedTask = null) : base(pos)
    {
        ClickedTask = clickedTask;
    }

    /// <summary>Ran when the mouse enters the boundaries of this element.</summary>
    public override void MouseEnter()
    {
        Hovered = true;
    }

    /// <summary>Ran when the mouse exits the boundaries of this element.</summary>
    public override void MouseLeave()
    {
        Hovered = false;
        Clicked = false;
    }

    /// <summary>Ran when the left mouse button is pressed down within the boundaries of this element.</summary>
    public override void MouseLeftDown()
    {
        Hovered = true;
        Clicked = true;
    }

    /// <summary>Ran when the left mouse button is released within the boundaries of this element.</summary>
    public override void MouseLeftUp()
    {
        if (ClickedTask is not null && Clicked && Hovered)
        {
            ClickedTask.Invoke();
        }
        Clicked = false;
    }
}
