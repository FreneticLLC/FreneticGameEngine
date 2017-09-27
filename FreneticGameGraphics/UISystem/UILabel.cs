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
using OpenTK;

namespace FreneticGameGraphics.UISystem
{
    /// <summary>
    /// Represents a simple text box on a screen.
    /// </summary>
    public class UILabel : UIElement
    {
        /// <summary>
        /// The text to display on this label.
        /// </summary>
        public string Text;

        /// <summary>
        /// The font to use.
        /// </summary>
        public FontSet TextFont;
        
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
            Text = btext;
            TextFont = font;
            // TODO: Dynamic scaling support?
            int pwidth = Position.Width;
            CustomWidth = pwidth > 0;
            Location scale = TextFont.MeasureFancyLinesOfText(CustomWidth ? TextFont.SplitAppropriately(Text, pwidth) : Text, BColor);
            pos.ConstantWidthHeight((int)scale.X, (int)scale.Y);
        }

        /// <summary>
        /// Whether to custom-limit the width.
        /// </summary>
        public bool CustomWidth;

        /// <summary>
        /// Renders this label on the screen.
        /// </summary>
        /// <param name="view">The UI view.</param>
        /// <param name="delta">The time since the last render.</param>
        public override void Render(ViewUI2D view, double delta)
        {
            string tex = CustomWidth ? TextFont.SplitAppropriately(Text, LastAbsoluteSize.X) : Text;
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
