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
    /// <summary>Grouped styles for a <see cref="UIClickableElement"/>.</summary>
    /// <param name="normal">The default style to use.</param>
    /// <param name="hover">The style to use on hover.</param>
    /// <param name="click">The style to use on click.</param>
    public class StyleGroup(UIElementStyle normal, UIElementStyle hover, UIElementStyle click)
    {
        /// <summary>The render style to use when the element is not being interacted with.</summary>
        public UIElementStyle Normal = normal;

        /// <summary>The render style to use when the user is hovering their mouse cursor over this element.</summary>
        public UIElementStyle Hover = hover;

        /// <summary>The render style to use when the user is clicking on this element.</summary>
        public UIElementStyle Click = click;
    }

    /// <summary>The clickable style group.</summary>
    public StyleGroup Styles;

    /// <summary>Constructs the styled clickable element.</summary>
    /// <param name="styles">The clickable styles.</param>
    /// <param name="pos">The position of the element.</param>
    /// <param name="requireText">Whether the styles must support text rendering.</param>
    /// <param name="onClick">Ran when the element is clicked.</param>
    public UIClickableElement(StyleGroup styles, UIPositionHelper pos, bool requireText = false, Action onClick = null) : base(pos)
    {
        Clicked += onClick;
        Styles = styles;
        AddStyle(Styles.Normal, requireText);
        AddStyle(Styles.Hover, requireText);
        AddStyle(Styles.Click, requireText);
    }

    /// <summary>Returns the normal, hover, or click style based on the current element state.</summary>
    public override UIElementStyle Style
    {
        get
        {
            if (Pressed)
            {
                return Styles.Click;
            }
            if (Hovered)
            {
                return Styles.Hover;
            }
            return Styles.Normal;
        }
    }
}
