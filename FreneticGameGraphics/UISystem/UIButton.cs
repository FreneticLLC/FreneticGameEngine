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
using FreneticGameGraphics.ClientSystem;
using FreneticGameGraphics.GraphicsHelpers;

namespace FreneticGameGraphics.UISystem
{
    /// <summary>
    /// Represents an interactable button on a screen.
    /// </summary>
    public class UIButton : UIElement
    {
        /// <summary>
        /// The name of the texture for this button.
        /// </summary>
        private string tName;

        /// <summary>
        /// The text to display on this button.
        /// </summary>
        public string Text;

        /// <summary>
        /// The font to use.
        /// </summary>
        public FontSet TextFont;

        /// <summary>
        /// Ran when this button is clicked.
        /// </summary>
        public Action ClickedTask;

        /// <summary>
        /// The standard texture.
        /// </summary>
        public Texture Tex_None;

        /// <summary>
        /// The texture used when hovering over this button.
        /// </summary>
        public Texture Tex_Hover;

        /// <summary>
        /// The texture used when this button is being clicked.
        /// </summary>
        public Texture Tex_Click;

        /// <summary>
        /// Whether the mouse is hovering over this button.
        /// </summary>
        public bool Hovered = false;

        /// <summary>
        /// Whether this button is being clicked.
        /// </summary>
        public bool Clicked = false;

        /// <summary>
        /// Constructs a new button.
        /// </summary>
        /// <param name="buttontexname">The name of the texture to use.</param>
        /// <param name="buttontext">The text to display.</param>
        /// <param name="font">The font to use.</param>
        /// <param name="clicked">The action to run when clicked.</param>
        /// <param name="pos">The position of the element.</param>
        public UIButton(string buttontexname, string buttontext, FontSet font, Action clicked, UIPositionHelper pos)
            : base(pos)
        {
            tName = buttontexname;
            Text = buttontext;
            TextFont = font;
            ClickedTask = clicked;
        }

        /// <summary>
        /// Preps the button's textures.
        /// </summary>
        protected override void Init()
        {
            TextureEngine Textures = Engine.Textures;
            Tex_None = Textures.GetTexture(tName + "_none");
            Tex_Hover = Textures.GetTexture(tName + "_hover");
            Tex_Click = Textures.GetTexture(tName + "_click");
        }

        /// <summary>
        /// Ran when the mouse enters the boundaries of this button.
        /// </summary>
        protected override void MouseEnter()
        {
            Hovered = true;
        }

        /// <summary>
        /// Ran when the mouse exits the boundaries of this button.
        /// </summary>
        protected override void MouseLeave()
        {
            Hovered = false;
            Clicked = false;
        }

        /// <summary>
        /// Ran when the left mouse button is pressed down within the boundaries of this button.
        /// </summary>
        protected override void MouseLeftDown()
        {
            Hovered = true;
            Clicked = true;
        }

        /// <summary>
        /// Ran when the left mouse button is released within the boundaries of this button.
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
        /// Renders this button on the screen.
        /// </summary>
        /// <param name="view">The UI view.</param>
        /// <param name="delta">The time since the last render.</param>
        /// <param name="xoff">The X offset of this button's parent.</param>
        /// <param name="yoff">The Y offset of this button's parent.</param>
        protected override void Render(ViewUI2D view, double delta, int xoff, int yoff)
        {
            if (Clicked)
            {
                Tex_Click.Bind();
            }
            else if (Hovered)
            {
                Tex_Hover.Bind();
            }
            else
            {
                Tex_None.Bind();
            }
            int x = GetX() + xoff;
            int y = GetY() + yoff;
            float width = GetWidth();
            float height = GetHeight();
            view.Rendering.RenderRectangle(view.UIContext, x, y, x + width, y + height);
            float len = TextFont.MeasureFancyText(Text);
            float hei = TextFont.font_default.Height;
            TextFont.DrawColoredText(Text, new Location(x + width / 2 - len / 2, y + height / 2 - hei / 2, 0));
        }
    }
}
