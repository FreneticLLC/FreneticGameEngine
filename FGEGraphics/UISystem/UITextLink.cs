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
using FGECore.CoreSystems;
using FGECore.MathHelpers;
using FGEGraphics.ClientSystem;
using FGEGraphics.GraphicsHelpers;
using FGEGraphics.GraphicsHelpers.FontSets;
using FGEGraphics.GraphicsHelpers.Textures;
using OpenTK;
using OpenTK.Mathematics;

namespace FGEGraphics.UISystem
{
    /// <summary>Represents an interactable text link on a screen.</summary>
    public class UITextLink : UIElement
    {
        /// <summary>Action to perform when this link is clicked.</summary>
        public Action ClickedTask;

        /// <summary>The text to display for this link.</summary>
        public RenderableText Text;

        /// <summary>The text to display when hovering over this link.</summary>
        public RenderableText TextHover;

        /// <summary>The text to display when clicking this link.</summary>
        public RenderableText TextClick;

        /// <summary>The base text color for this link.</summary>
        public string BColor = "^r^7";

        /// <summary>Whether the mouse is hovering over this link.</summary>
        public bool Hovered = false;

        /// <summary>Whether this link is being clicked.</summary>
        public bool Clicked = false;

        /// <summary>The font to use.</summary>
        public FontSet TextFont;

        /// <summary>The icon to display alongside this link.</summary>
        public Texture Icon;

        /// <summary>The color of the icon.</summary>
        public Color4F IconColor = Color4F.White;

        /// <summary>Constructs an interactable text link.</summary>
        /// <param name="ico">The icon to display.</param>
        /// <param name="btext">The text to display.</param>
        /// <param name="btexthover">The text to display while hovering.</param>
        /// <param name="btextclick">The text to display while clicking.</param>
        /// <param name="font">The font to use.</param>
        /// <param name="clicked">The action to run when clicked.</param>
        /// <param name="pos">The position of the element.</param>
        public UITextLink(Texture ico, string btext, string btexthover, string btextclick, FontSet font, Action clicked, UIPositionHelper pos)
            : base(pos)
        {
            TextFont = font;
            Icon = ico;
            ClickedTask = clicked;
            Text = font.ParseFancyText(btext, BColor);
            TextHover = font.ParseFancyText(btexthover, BColor);
            TextClick = font.ParseFancyText(btextclick, BColor);
            Position.ConstantWidth(Text.Width + (Icon == null ? 0 : font.FontDefault.Height));
            Position.ConstantHeight(TextFont.FontDefault.Height * Text.Lines.Length);
        }

        /// <summary>Detects hovering.</summary>
        public override void MouseEnter()
        {
            Hovered = true;
        }

        /// <summary>Detects hover cancellation.</summary>
        public override void MouseLeave()
        {
            Hovered = false;
            Clicked = false;
        }

        /// <summary>Detects clicking.</summary>
        public override void MouseLeftDown()
        {
            Hovered = true;
            Clicked = true;
        }

        /// <summary>Detects releasing clicks.</summary>
        public override void MouseLeftUp()
        {
            if (!Clicked)
            {
                return;
            }
            Clicked = false;
            if (Hovered)
            {
                ClickedTask.Invoke();
            }
        }

        /// <summary>Performs a render on this link.</summary>
        /// <param name="view">The UI view.</param>
        /// <param name="delta">The time since the last render.</param>
        public override void Render(ViewUI2D view, double delta)
        {
            RenderableText tt = Text;
            if (Clicked)
            {
                tt = TextClick;
            }
            else if (Hovered)
            {
                tt = TextHover;
            }
            int x = LastAbsolutePosition.X;
            int y = LastAbsolutePosition.Y;
            if (Icon != null)
            {
                Icon.Bind();
                Renderer2D.SetColor(IconColor);
                view.Rendering.RenderRectangle(view.UIContext, x, y, x + TextFont.FontDefault.Height, y + TextFont.FontDefault.Height, new Vector3(-0.5f, -0.5f, LastAbsoluteRotation));
                TextFont.DrawFancyText(tt, new Location(x + TextFont.FontDefault.Height, y, 0));
                Renderer2D.SetColor(Vector4.One);
            }
            else
            {
                TextFont.DrawFancyText(tt, new Location(x, y, 0));
            }
        }
    }
}
