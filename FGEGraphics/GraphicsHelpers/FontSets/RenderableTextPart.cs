//
// This file is part of the Frenetic Game Engine, created by Frenetic LLC.
// This code is Copyright (C) Frenetic LLC under the terms of a strict license.
// See README.md or LICENSE.txt in the FreneticGameEngine source root for the contents of the license.
// If neither of these are available, assume that neither you nor anyone other than the copyright holder
// hold any right or permission to use this software until such time as the official license is identified.
//

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FGECore.MathHelpers;

namespace FGEGraphics.GraphicsHelpers.FontSets
{
    /// <summary>Represents a single section of text with a single format across all characters.</summary>
    public class RenderableTextPart
    {
        /// <summary>The actual characters of text to render.</summary>
        public string Text;

        /// <summary>The horizontal width of the text, in pixels.</summary>
        public float Width;

        /// <summary>The color of the text.</summary>
        public Color4F TextColor = Color4F.White;

        /// <summary>Whether the text is bold.</summary>
        public bool Bold = false;

        /// <summary>Whether the text is italic.</summary>
        public bool Italic = false;

        /// <summary>Whether the text is underlined.</summary>
        public bool Underline = false;

        /// <summary>Whether the text has a strike-through.</summary>
        public bool Strike = false;

        /// <summary>Whether the text is overlined.</summary>
        public bool Overline = false;

        /// <summary>Whether the text is highlighted.</summary>
        public bool Highlight = false;

        /// <summary>Whether the text is emphasized.</summary>
        public bool Emphasis = false;

        /// <summary>The color of the underline (if any).</summary>
        public Color4F UnderlineColor = Color4F.White;

        /// <summary>The color of the strike-through (if any).</summary>
        public Color4F StrikeColor = Color4F.White;

        /// <summary>The color of the overline (if any).</summary>
        public Color4F OverlineColor = Color4F.White;

        /// <summary>The color of the highlight box (if any).</summary>
        public Color4F HighlightColor = Color4F.White;

        /// <summary>The color of the emphasis (if any).</summary>
        public Color4F EmphasisColor = Color4F.White;

        /// <summary>Whether the text is a superscript (raised and half height).</summary>
        public bool SuperScript = false;

        /// <summary>Whether the text is a subscript (lowered and half height).</summary>
        public bool SubScript = false;

        /// <summary>Whether the text is vertically flipped.</summary>
        public bool Flip = false;

        /// <summary>Whether the text is pseudo-random (random color per character that doesn't change with time).</summary>
        public bool PseudoRandom = false;

        /// <summary>Whether the text is 'jello' (shakes in place).</summary>
        public bool Jello = false;

        /// <summary>Whether the text is 'unreadable' (characters change randomly).</summary>
        public bool Unreadable = false;

        /// <summary>Whether the text is colored randomly in a way that changes over time.</summary>
        public bool Random = false;

        /// <summary>Whether the text has a drop-shadow.</summary>
        public bool Shadow = false;

        /// <summary>What font this text renders with.</summary>
        public GLFont Font = null;

        /// <summary>A URL to open when this text is clicked.</summary>
        public string ClickURL = null;

        /// <summary>Text to display when a mouse is hovered over this text.</summary>
        public RenderableText HoverText = null;

        /// <summary>Applies the correct font from a font set.</summary>
        public void SetFontFrom(FontSet set)
        {
            if (SuperScript || SubScript)
            {
                if (Bold && Italic)
                {
                    Font = set.FontBoldItalicHalf;
                }
                else if (Bold)
                {
                    Font = set.FontBoldHalf;
                }
                else if (Italic)
                {
                    Font = set.FontItalicHalf;
                }
                else
                {
                    Font = set.FontHalf;
                }
            }
            else
            {
                if (Bold && Italic)
                {
                    Font = set.FontBoldItalic;
                }
                else if (Bold)
                {
                    Font = set.FontBold;
                }
                else if (Italic)
                {
                    Font = set.FontItalic;
                }
                else
                {
                    Font = set.FontDefault;
                }
            }
        }

        /// <summary>Returns a perfect copy of the part.</summary>
        public RenderableTextPart Clone() => MemberwiseClone() as RenderableTextPart;

        /// <summary>Implements <see cref="Object.ToString"/> to return the raw text.</summary>
        public override string ToString() => Text;
    }

    /// <summary>Represents a single line of renderable text.</summary>
    public class RenderableTextLine
    {
        /// <summary>An array of all parts within the line.</summary>
        public RenderableTextPart[] Parts;

        /// <summary>The total width of the line.</summary>
        public int Width;

        /// <summary>Implements <see cref="Object.ToString"/> to make an un-separated string of the contents.</summary>
        public override string ToString() => string.Concat<RenderableTextPart>(Parts);
    }

    /// <summary>Represents a section of renderable text.</summary>
    public class RenderableText
    {
        /// <summary>An array of all lines of text.</summary>
        public RenderableTextLine[] Lines;

        /// <summary>The maximum width of the text.</summary>
        public int Width;

        /// <summary>Implements <see cref="Object.ToString"/> to make a "\n" separated string of the contents.</summary>
        public override string ToString() => string.Join<RenderableTextLine>('\n', Lines);
    }
}
