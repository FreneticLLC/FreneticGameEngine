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
using FGEGraphics.UISystem;

namespace FGEGraphics.UISystem;

/// <summary>Represents a styled clickable UI element on the screen.</summary>
public abstract class UIClickableElement : UIElement
{
    /// <summary>Grouped styles for a <see cref="UIClickableElement"/>.</summary>
    /// <param name="normal">The default style to use.</param>
    /// <param name="hover">The style to use on hover.</param>
    /// <param name="click">The style to use on click.</param>
    /// <param name="disabled">The style to use when disabled.</param>
    public class StyleGroup(UIElementStyle normal, UIElementStyle hover, UIElementStyle click, UIElementStyle disabled)
    {
        /// <summary>An empty style group.</summary>
        public static readonly StyleGroup Empty = new(UIElementStyle.Empty, UIElementStyle.Empty, UIElementStyle.Empty, UIElementStyle.Empty);

        /// <summary>The render style to use when the element is not being interacted with.</summary>
        public UIElementStyle Normal = normal;

        /// <summary>The render style to use when the user is hovering their mouse cursor over this element.</summary>
        public UIElementStyle Hover = hover;

        /// <summary>The render style to use when the user is clicking on this element.</summary>
        public UIElementStyle Click = click;

        /// <summary>The style to use when the element is disabled.</summary>
        public UIElementStyle Disabled = disabled;
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
        Styles = AddStyles(styles);
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
            if (!Enabled)
            {
                return Styles.Disabled;
            }
            return Styles.Normal;
        }
    }

    /// <summary>Calls <see cref="UIElement.AddStyle(UIElementStyle, bool)"/> on each member of the style group.</summary>
    /// <param name="styles">The clickable styles.</param>
    /// <param name="requireText">Whether each style must support text rendering.</param>
    public StyleGroup AddStyles(StyleGroup styles, bool requireText = false)
    {
        AddStyle(styles.Normal, requireText);
        AddStyle(styles.Hover, requireText);
        AddStyle(styles.Click, requireText);
        AddStyle(styles.Disabled, requireText);
        return styles;
    }
}
