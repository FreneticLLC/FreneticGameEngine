using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FGEGraphics.UISystem;

namespace FGEGraphics.UISystem;

/// <summary>Represents a styled clickable UI element on the screen.</summary>
public abstract class UIClickableElement : UIElement
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
    public UIClickableElement(UIElementStyle normal, UIElementStyle hover, UIElementStyle click, UIPositionHelper pos, bool requireText = false, Action onClick = null) : base(pos)
    {
        Clicked += onClick;
        StyleNormal = AddStyle(normal, requireText);
        StyleHover = AddStyle(hover, requireText);
        StyleClick = AddStyle(click, requireText);
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
