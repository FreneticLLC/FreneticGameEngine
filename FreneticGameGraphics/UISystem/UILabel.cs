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
        /// The maximum width of this label.
        /// </summary>
        public Func<int> MaxX = null;

        /// <summary>
        /// The background color for this label.
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
        /// <param name="anchor">The anchor the label will be relative to.</param>
        /// <param name="xOff">The function to get the X offset.</param>
        /// <param name="yOff">The function to get the Y offset.</param>
        /// <param name="maxx">The function to get the maximum width.</param>
        public UILabel(string btext, FontSet font, UIAnchor anchor, Func<int> xOff, Func<int> yOff, Func<int> maxx = null)
            : base(anchor, () => 0, () => 0, xOff, yOff)
        {
            Text = btext;
            TextFont = font;
            Width = () => (float)TextFont.MeasureFancyLinesOfText(MaxX != null ? TextFont.SplitAppropriately(Text, MaxX()) : Text, BColor).X;
            Height = () => (float)TextFont.MeasureFancyLinesOfText(MaxX != null ? TextFont.SplitAppropriately(Text, MaxX()) : Text, BColor).Y;
            MaxX = maxx;
        }

        /// <summary>
        /// Renders this label on the screen.
        /// </summary>
        /// <param name="view">The UI view.</param>
        /// <param name="delta">The time since the last render.</param>
        /// <param name="xoff">The X offset of this label's parent.</param>
        /// <param name="yoff">The Y offset of this label's parent.</param>
        protected override void Render(ViewUI2D view, double delta, int xoff, int yoff)
        {
            string tex = MaxX != null ? TextFont.SplitAppropriately(Text, MaxX()) : Text;
            float bx = GetX() + xoff;
            float by = GetY() + yoff;
            if (BackColor.W > 0)
            {
                Location meas = TextFont.MeasureFancyLinesOfText(tex);
                view.Renderer.SetColor(BackColor);
                view.Renderer.RenderRectangle(view.UIContext, bx, by, bx + (float)meas.X, by + (float)meas.Y);
                view.Renderer.SetColor(Vector4.One);
            }
            TextFont.DrawColoredText(tex, new Location(bx, by, 0), int.MaxValue, 1, false, BColor);
        }
    }
}
