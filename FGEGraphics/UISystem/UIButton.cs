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
using FGECore.ConsoleHelpers;
using FGECore.CoreSystems;
using FGECore.MathHelpers;
using FGEGraphics.ClientSystem;
using FGEGraphics.GraphicsHelpers;
using FGEGraphics.GraphicsHelpers.FontSets;
using FGEGraphics.GraphicsHelpers.Textures;
using OpenTK;
using OpenTK.Mathematics;

namespace FGEGraphics.UISystem;

/// <summary>Represents an interactable button on a screen.</summary>
public class UIButton : UIElement
{
    /// <summary>Represents the rendering style of a <see cref="UIButton"/>.</summary>
    public class Style
    {
        /// <summary>Constructs a default <see cref="Style"/> instance.</summary>
        public Style()
        {
        }

        /// <summary>Constructs a new style as a copy of another style.</summary>
        public Style(Style style)
        {
            BackColor = style.BackColor;
            BorderColor = style.BorderColor;
            BorderWidth = style.BorderWidth;
            DisplayTexture = style.DisplayTexture;
            DropShadowLength = style.DropShadowLength;
            TextBaseColor = style.TextBaseColor;
        }

        /// <summary>What background box color to use (or <see cref="Color4F.Transparent"/> for none).</summary>
        public Color4F BackColor = Color4F.Transparent;

        /// <summary>What background box border outline color to use (or <see cref="Color4F.Transparent"/> for none).</summary>
        public Color4F BorderColor = Color4F.Transparent;

        /// <summary>How large the background box's border should be (or 0 for none).</summary>
        public int BorderWidth = 0;

        /// <summary>What texture to display (or null for none).</summary>
        public Texture DisplayTexture;

        /// <summary>How big the drop-shadow effect should be (or 0 for none).</summary>
        public int DropShadowLength = 0;

        /// <summary>The base color effect to use for text (consider <see cref="TextStyle.Simple"/> if unsure).</summary>
        public string TextBaseColor = TextStyle.Simple;

        /// <summary>(Don't modify directly) the raw actual renderable text of the button.
        /// <para>Use <see cref="Text"/> to modify the text value.</para></summary>
        public RenderableText Internal_ActualText;
    }

    /// <summary>The render style to use when the button is not being interacted with.</summary>
    public Style StyleNormal
    {
        get
        {
            return Internal.StyleNormal;
        }
        set
        {
            Internal.StyleNormal = new Style(value);
            if (Internal.RawText is not null)
            {
                Internal.StyleNormal.Internal_ActualText = TextFont.ParseFancyText(Internal.RawText, Internal.StyleNormal.TextBaseColor);
            }
        }
    }

    /// <summary>The render style to use when the user is hovering their mouse cursor over this button.</summary>
    public Style StyleHover
    {
        get
        {
            return Internal.StyleHover;
        }
        set
        {
            Internal.StyleHover = new Style(value);
            if (Internal.RawText is not null)
            {
                Internal.StyleHover.Internal_ActualText = TextFont.ParseFancyText(Internal.RawText, Internal.StyleHover.TextBaseColor);
            }
        }
    }

    /// <summary>The render style to use when the user is clicking on this button.</summary>
    public Style StyleClick
    {
        get
        {
            return Internal.StyleClick;
        }
        set
        {
            Internal.StyleClick = new Style(value);
            if (Internal.RawText is not null)
            {
                Internal.StyleClick.Internal_ActualText = TextFont.ParseFancyText(Internal.RawText, Internal.StyleClick.TextBaseColor);
            }
        }
    }

    /// <summary>Holds internal data for <see cref="UIButton"/>.</summary>
    public struct InternalData
    {
        /// <summary>The raw text of the button as input by the user.</summary>
        public string RawText;

        /// <summary>The render style to use when the button is not being interacted with.</summary>
        public Style StyleNormal;

        /// <summary>The render style to use when the user is hovering their mouse cursor over this button.</summary>
        public Style StyleHover;

        /// <summary>The render style to use when the user is clicking on this button.</summary>
        public Style StyleClick;
    }

    /// <summary>Internal data for this button.</summary>
    public InternalData Internal;

    /// <summary>Gets or sets the text to render on this button.</summary>
    public string Text
    {
        get => Internal.RawText;
        set
        {
            Internal.RawText = value;
            Internal.StyleNormal.Internal_ActualText = TextFont.ParseFancyText(value, Internal.StyleNormal.TextBaseColor);
            Internal.StyleHover.Internal_ActualText = TextFont.ParseFancyText(value, Internal.StyleHover.TextBaseColor);
            Internal.StyleClick.Internal_ActualText = TextFont.ParseFancyText(value, Internal.StyleClick.TextBaseColor);
        }
    }

    /// <summary>The font to use.</summary>
    public FontSet TextFont;

    /// <summary>Ran when this button is clicked.</summary>
    public Action ClickedTask;

    /// <summary>Whether the mouse is hovering over this button.</summary>
    public bool Hovered = false;

    /// <summary>Whether this button is being clicked.</summary>
    public bool Clicked = false;

    /// <summary>Constructs a new button based on a render style.</summary>
    /// <param name="normal">The style to display when neither hovered nor clicked.</param>
    /// <param name="hover">The style to display when hovered.</param>
    /// <param name="click">The style to display when clicked.</param>
    /// <param name="text">The text to display.</param>
    /// <param name="font">The font to use.</param>
    /// <param name="clicked">The action to run when clicked.</param>
    /// <param name="pos">The position of the element.</param>
    public UIButton(Style normal, Style hover, Style click, string text, FontSet font, Action clicked, UIPositionHelper pos)
        : base(pos)
    {
        StyleNormal = new Style(normal);
        StyleHover = new Style(hover);
        StyleClick = new Style(click);
        TextFont = font;
        ClickedTask = clicked;
        Text = text;
    }

    /// <summary>Constructs a new button based on a standard texture set.</summary>
    /// <param name="buttontexname">The name of the texture set to use.</param>
    /// <param name="text">The text to display.</param>
    /// <param name="font">The font to use.</param>
    /// <param name="clicked">The action to run when clicked.</param>
    /// <param name="pos">The position of the element.</param>
    public UIButton(string buttontexname, string text, FontSet font, Action clicked, UIPositionHelper pos)
        : this(new Style(), new Style(), new Style(), text, font, clicked, pos)
    {
        if (buttontexname is not null)
        {
            TextureEngine Textures = TextFont.Engine.GLFonts.Textures;
            StyleNormal.DisplayTexture = Textures.GetTexture(buttontexname + "_none");
            StyleHover.DisplayTexture = Textures.GetTexture(buttontexname + "_hover");
            StyleClick.DisplayTexture = Textures.GetTexture(buttontexname + "_click");
        }
    }

    /// <summary>Ran when the mouse enters the boundaries of this button.</summary>
    public override void MouseEnter()
    {
        Hovered = true;
    }

    /// <summary>Ran when the mouse exits the boundaries of this button.</summary>
    public override void MouseLeave()
    {
        Hovered = false;
        Clicked = false;
    }

    /// <summary>Ran when the left mouse button is pressed down within the boundaries of this button.</summary>
    public override void MouseLeftDown()
    {
        Hovered = true;
        Clicked = true;
    }

    /// <summary>Ran when the left mouse button is released within the boundaries of this button.</summary>
    public override void MouseLeftUp()
    {
        if (Clicked && Hovered)
        {
            ClickedTask.Invoke();
        }
        Clicked = false;
    }

    /// <summary>Renders this button on the screen.</summary>
    /// <param name="view">The UI view.</param>
    /// <param name="delta">The time since the last render.</param>
    public override void Render(ViewUI2D view, double delta)
    {
        Style style = StyleNormal;
        if (Clicked)
        {
            style = StyleClick;
        }
        else if (Hovered)
        {
            style = StyleHover;
        }
        int x = LastAbsolutePosition.X;
        int y = LastAbsolutePosition.Y;
        float width = LastAbsoluteSize.X;
        float height = LastAbsoluteSize.Y;
        Vector3 rotation = new(-0.5f, -0.5f, LastAbsoluteRotation);
        bool any = style.DropShadowLength > 0 || style.BorderColor.A > 0 || style.BackColor.A > 0;
        if (any)
        {
            Engine.Textures.White.Bind();
        }
        if (style.DropShadowLength > 0)
        {
            Renderer2D.SetColor(new Color4F(0, 0, 0, 0.5f));
            view.Rendering.RenderRectangle(view.UIContext, x, y, x + width + style.DropShadowLength, y + height + style.DropShadowLength, rotation);
        }
        if (style.BorderColor.A > 0 && style.BorderWidth > 0)
        {
            Renderer2D.SetColor(style.BorderColor);
            view.Rendering.RenderRectangle(view.UIContext, x, y, x + width, y + height, rotation);
        }
        if (style.BackColor.A > 0)
        {
            Renderer2D.SetColor(style.BackColor);
            view.Rendering.RenderRectangle(view.UIContext, x + style.BorderWidth, y + style.BorderWidth, x + width - style.BorderWidth, y + height - style.BorderWidth, rotation);
        }
        if (any)
        {
            Renderer2D.SetColor(Color4F.White);
        }
        if (style.DisplayTexture is not null)
        {
            style.DisplayTexture.Bind();
            view.Rendering.RenderRectangle(view.UIContext, x, y, x + width, y + height, rotation);
        }
        float textHeight = TextFont.FontDefault.Height * style.Internal_ActualText.Lines.Length;
        TextFont.DrawFancyText(style.Internal_ActualText, new Location(Math.Round(x + width / 2 - style.Internal_ActualText.Width / 2), Math.Round(y + height / 2 - textHeight / 2), 0));
    }
}
