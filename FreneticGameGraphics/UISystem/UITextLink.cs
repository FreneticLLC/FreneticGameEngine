//
// This file is created by Frenetic LLC.
// This code is Copyright (C) 2017 Frenetic LLC under the terms of a strict license.
// See README.md or LICENSE.txt in the source root for the contents of the license.
// If neither of these are available, assume that neither you nor anyone other than the copyright holder
// hold any right or permission to use this software until such time as the official license is identified.
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FreneticGameCore;
using FreneticGameCore.CoreSystems;
using FreneticGameCore.MathHelpers;
using FreneticGameGraphics.ClientSystem;
using FreneticGameGraphics.GraphicsHelpers;
using OpenTK;

namespace FreneticGameGraphics.UISystem
{
    /// <summary>
    /// Represents an interactable text link on a screen.
    /// </summary>
    public class UITextLink : UIElement
    {
        /// <summary>
        /// Action to perform when this link is clicked.
        /// </summary>
        public Action ClickedTask;

        /// <summary>
        /// The text to display for this link.
        /// </summary>
        public string Text;

        /// <summary>
        /// The text to display when hovering over this link.
        /// </summary>
        public string TextHover;

        /// <summary>
        /// The text to display when clicking this link.
        /// </summary>
        public string TextClick;

        /// <summary>
        /// The base text color for this link.
        /// </summary>
        public string BColor = "^r^7";

        /// <summary>
        /// Whether the mouse is hovering over this link.
        /// </summary>
        public bool Hovered = false;

        /// <summary>
        /// Whether this link is being clicked.
        /// </summary>
        public bool Clicked = false;

        /// <summary>
        /// The font to use.
        /// </summary>
        public FontSet TextFont;

        /// <summary>
        /// The icon to display alongside this link.
        /// </summary>
        public Texture Icon;

        /// <summary>
        /// The color of the icon.
        /// </summary>
        public Color4F IconColor = Color4F.White;

        /// <summary>
        /// Constructs an interactable text link.
        /// </summary>
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
            Icon = ico;
            ClickedTask = clicked;
            Text = btext;

            TextHover = btexthover;
            TextClick = btextclick;
            TextFont = font;
            Position.ConstantWidth((int)(font.MeasureFancyText(Text, BColor) + (Icon == null ? 0 : font.font_default.Height)));
            Position.ConstantHeight((int)TextFont.font_default.Height);
        }

        /// <summary>
        /// Detects hovering.
        /// </summary>
        protected override void MouseEnter()
        {
            Hovered = true;
        }

        /// <summary>
        /// Detects hover cancellation.
        /// </summary>
        protected override void MouseLeave()
        {
            Hovered = false;
            Clicked = false;
        }

        /// <summary>
        /// Detects clicking.
        /// </summary>
        protected override void MouseLeftDown()
        {
            Hovered = true;
            Clicked = true;
        }

        /// <summary>
        /// Detects releasing clicks.
        /// </summary>
        protected override void MouseLeftUp()
        {
            if (Clicked && Hovered)
            {
                ClickedTask.Invoke();
            }
            Clicked = false;
        }

        /// <summary>
        /// Performs a render on this link.
        /// </summary>
        /// <param name="view">The UI view.</param>
        /// <param name="delta">The time since the last render.</param>
        public override void Render(ViewUI2D view, double delta)
        {
            string tt = Text;
            if (Clicked)
            {
                tt = TextClick;
            }
            else if (Hovered)
            {
                tt = TextHover;
            }
            if (Icon != null)
            {
                float x = LastAbsolutePosition.X;
                float y = LastAbsolutePosition.Y;
                Icon.Bind();
                view.Rendering.SetColor(IconColor);
                view.Rendering.RenderRectangle(view.UIContext, x, y, x + TextFont.font_default.Height, y + TextFont.font_default.Height, new Vector3(-0.5f, -0.5f, LastAbsoluteRotation));
                TextFont.DrawColoredText(tt, new Location(x + TextFont.font_default.Height, y, 0), int.MaxValue, 1, false, BColor);
                view.Rendering.SetColor(OpenTK.Vector4.One);
            }
            else
            {
                TextFont.DrawColoredText(tt, new Location(LastAbsolutePosition.X, LastAbsolutePosition.Y, 0), bcolor: BColor);
            }
        }
    }
}
