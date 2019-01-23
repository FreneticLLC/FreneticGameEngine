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
    /// Represents a simple text box on a screen.
    /// </summary>
    public class UILabel : UIElement
    {
        /// <summary>
        /// The internal text value.
        /// <para>This will not update this label's width or height.</para>
        /// </summary>
        public string InternalText;

        /// <summary>
        /// The text to display on this label.
        /// <para>Setting this automatically adjusts this label's width and height as necessary.</para>
        /// </summary>
        public string Text
        {
            get
            {
                return InternalText;
            }
            set
            {
                InternalText = value;
                FixScale();
            }
        }

        /// <summary>
        /// The internal text font value.
        /// <para>This will not update this label's width or height.</para>
        /// </summary>
        public FontSet InternalTextFont;

        /// <summary>
        /// The font to use.
        /// <para>Setting this automatically adjusts this label's width and height as necessary.</para>
        /// </summary>
        public FontSet TextFont
        {
            get
            {
                return InternalTextFont;
            }
            set
            {
                InternalTextFont = value;
                FixScale();
            }
        }

        /// <summary>
        /// The background color for this label.
        /// <para>Set to Vector4.Zero (or any values with W=0) to disable the background color.</para>
        /// </summary>
        public Vector4 BackColor = Vector4.Zero;

        /// <summary>
        /// The base text color for this label.
        /// </summary>
        public string BColor = "^r^7";

        /// <summary>
        /// Constructs a new label.
        /// </summary>
        /// <param name="btext">The text to display on the label.</param>
        /// <param name="font">The font to use.</param>
        /// <param name="pos">The position of the element.</param>
        public UILabel(string btext, FontSet font, UIPositionHelper pos)
            : base(pos)
        {
            InternalText = btext;
            InternalTextFont = font;
            // TODO: Dynamic scaling support?
            CustomWidthValue = Position.Width;
            FixScale();
        }

        /// <summary>
        /// Fixes this label's width and height based on <see cref="Text"/> and <see cref="TextFont"/>.
        /// </summary>
        public void FixScale()
        {
            Location scale = TextFont.MeasureFancyLinesOfText(CustomWidthValue > 0 ? TextFont.SplitAppropriately(Text, CustomWidthValue) : Text, BColor);
            Position.ConstantWidthHeight((int)scale.X, (int)scale.Y);
        }

        /// <summary>
        /// The custom-limit width.
        /// </summary>
        public int CustomWidthValue;

        /// <summary>
        /// Renders this label on the screen.
        /// </summary>
        /// <param name="view">The UI view.</param>
        /// <param name="delta">The time since the last render.</param>
        public override void Render(ViewUI2D view, double delta)
        {
            string tex = CustomWidthValue > 0 ? TextFont.SplitAppropriately(Text, LastAbsoluteSize.X) : Text;
            int bx = LastAbsolutePosition.X;
            int by = LastAbsolutePosition.Y;
            if (BackColor.W > 0)
            {
                Location meas = TextFont.MeasureFancyLinesOfText(tex);
                view.Rendering.SetColor(BackColor);
                view.Rendering.RenderRectangle(view.UIContext, bx, by, bx + (float)meas.X, by + (float)meas.Y, new Vector3(-0.5f, -0.5f, LastAbsoluteRotation));
                view.Rendering.SetColor(Vector4.One);
            }
            TextFont.DrawColoredText(tex, new Location(bx, by, 0), bcolor: BColor);
        }
    }
}
