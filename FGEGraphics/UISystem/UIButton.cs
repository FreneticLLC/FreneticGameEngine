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
public class UIButton : UIClickableElement
{
    /// <summary>The text to render on this button.</summary>
    public UIElementText Text;

    /// <summary>The render style to use when the button is not being interacted with.</summary>
    public UIElementStyle StyleNormal;

    /// <summary>The render style to use when the user is hovering their mouse cursor over this button.</summary>
    public UIElementStyle StyleHover;

    /// <summary>The render style to use when the user is clicking on this button.</summary>
    public UIElementStyle StyleClick;

    /// <summary>Constructs a new button based on a render style.</summary>
    /// <param name="normal">The style to display when neither hovered nor clicked.</param>
    /// <param name="hover">The style to display when hovered.</param>
    /// <param name="click">The style to display when clicked.</param>
    /// <param name="text">The text to display.</param>
    /// <param name="clicked">The action to run when clicked.</param>
    /// <param name="pos">The position of the element.</param>
    public UIButton(UIElementStyle normal, UIElementStyle hover, UIElementStyle click, string text, Action clicked, UIPositionHelper pos)
        : base(pos, clicked)
    {
        StyleNormal = RegisterStyle(normal);
        StyleHover = RegisterStyle(hover);
        StyleClick = RegisterStyle(click);
        Text = CreateText(text);
    }

    /// <summary>Constructs a new button based on a standard texture set.</summary>
    /// <param name="buttontexname">The name of the texture set to use.</param>
    /// <param name="text">The text to display.</param>
    /// <param name="font">The font to use.</param>
    /// <param name="clicked">The action to run when clicked.</param>
    /// <param name="pos">The position of the element.</param>
    /*public UIButton(string buttontexname, string text, FontSet font, Action clicked, UIPositionHelper pos)
        : this(new Style(), new Style(), new Style(), text, font, clicked, pos)
    {
        if (buttontexname is not null)
        {
            TextureEngine Textures = TextFont.Engine.GLFonts.Textures;
            StyleNormal.DisplayTexture = Textures.GetTexture(buttontexname + "_none");
            StyleHover.DisplayTexture = Textures.GetTexture(buttontexname + "_hover");
            StyleClick.DisplayTexture = Textures.GetTexture(buttontexname + "_click");
        }
    }*/

    public override UIElementStyle GetStyle()
    {
        if (Clicked)
        {
            return StyleClick;
        }
        if (Hovered)
        {
            return StyleHover;
        }
        return StyleNormal;
    }

    /// <summary>Renders this button on the screen.</summary>
    /// <param name="view">The UI view.</param>
    /// <param name="delta">The time since the last render.</param>
    // TODO: maybe provide style as a parameter here
    public override void Render(ViewUI2D view, double delta)
    {
        UIElementStyle style = GetStyle();
        Vector3 rotation = new(-0.5f, -0.5f, LastAbsoluteRotation);
        bool any = style.DropShadowLength > 0 || style.BorderColor.A > 0 || style.BaseColor.A > 0;
        if (any)
        {
            Engine.Textures.White.Bind();
        }
        if (style.DropShadowLength > 0)
        {
            Renderer2D.SetColor(new Color4F(0, 0, 0, 0.5f));
            view.Rendering.RenderRectangle(view.UIContext, X, Y, X + Width + style.DropShadowLength, Y + Height + style.DropShadowLength, rotation);
        }
        if (style.BorderColor.A > 0 && style.BorderThickness > 0)
        {
            Renderer2D.SetColor(style.BorderColor);
            view.Rendering.RenderRectangle(view.UIContext, X, Y, X + Width, Y + Height, rotation);
        }
        if (style.BaseColor.A > 0)
        {
            Renderer2D.SetColor(style.BaseColor);
            view.Rendering.RenderRectangle(view.UIContext, X + style.BorderThickness, X + style.BorderThickness, X + Width - style.BorderThickness, Y + Height - style.BorderThickness, rotation);
        }
        if (any)
        {
            Renderer2D.SetColor(Color4F.White);
        }
        if (style.BaseTexture is not null)
        {
            style.BaseTexture.Bind();
            view.Rendering.RenderRectangle(view.UIContext, X, Y, X + Width, Y + Height, rotation);
        }
        if (style.CanRenderText(Text))
        {
            float textHeight = style.TextFont.FontDefault.Height * Text.Renderable.Lines.Length;
            style.TextFont.DrawFancyText(Text, new Location(Math.Round((double)(X + Width / 2 - Text.Renderable.Width / 2)), Math.Round(X + Height / 2 - textHeight / 2), 0));
        }
    }
}
