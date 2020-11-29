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
using System.Drawing;
using System.Threading.Tasks;
using OpenTK;
using OpenTK.Mathematics;
using OpenTK.Graphics.OpenGL4;
using FreneticUtilities.FreneticExtensions;
using FreneticUtilities.FreneticToolkit;
using FGECore.MathHelpers;
using FGECore.UtilitySystems;
using FGECore.ConsoleHelpers;

using MathHelper = FreneticUtilities.FreneticToolkit.MathHelper;

namespace FGEGraphics.GraphicsHelpers.FontSets
{
    /// <summary>
    /// Contains various GLFonts needed to render fancy text.
    /// </summary>
    public class FontSet
    {
        /// <summary>
        /// The backing engine.
        /// </summary>
        public FontSetEngine Engine;

        /// <summary>
        /// Default font.
        /// </summary>
        public GLFont FontDefault;

        /// <summary>
        /// Bold font.
        /// </summary>
        public GLFont FontBold;

        /// <summary>
        /// Italic font.
        /// </summary>
        public GLFont FontItalic;

        /// <summary>
        /// Bold+Italic font.
        /// </summary>
        public GLFont FontBoldItalic;

        /// <summary>
        /// Half-size font.
        /// </summary>
        public GLFont FontHalf;

        /// <summary>
        /// Half-size bold font.
        /// </summary>
        public GLFont FontBoldHalf;

        /// <summary>
        /// Half-size italic font.
        /// </summary>
        public GLFont FontItalicHalf;

        /// <summary>
        /// Half-size bold+italic font.
        /// </summary>
        public GLFont FontBoldItalicHalf;

        /// <summary>
        /// Name of the font set.
        /// </summary>
        public string Name;

        /// <summary>
        /// Prepares a font set but does not load it,
        /// </summary>
        /// <param name="_name">The name of the set.</param>
        /// <param name="engine">The backing engine.</param>
        public FontSet(string _name, FontSetEngine engine)
        {
            Name = _name.ToLowerFast();
            Engine = engine;
        }

        /// <summary>
        /// Loads the font set.
        /// </summary>
        /// <param name="fontname">The name of the font.</param>
        /// <param name="fontsize">The size of the font.</param>
        public void Load(string fontname, int fontsize)
        {
            FontDefault = Engine.GLFonts.GetFont(fontname, false, false, fontsize);
            FontBold = Engine.GLFonts.GetFont(fontname, true, false, fontsize);
            FontItalic = Engine.GLFonts.GetFont(fontname, false, true, fontsize);
            FontBoldItalic = Engine.GLFonts.GetFont(fontname, true, true, fontsize);
            FontHalf = Engine.GLFonts.GetFont(fontname, false, false, fontsize / 2);
            FontBoldHalf = Engine.GLFonts.GetFont(fontname, true, false, fontsize / 2);
            FontItalicHalf = Engine.GLFonts.GetFont(fontname, false, true, fontsize / 2);
            FontBoldItalicHalf = Engine.GLFonts.GetFont(fontname, true, true, fontsize / 2);
        }

        /// <summary>
        /// The default color of text.
        /// </summary>
        public const int DefaultColor = 7;

        /// <summary>
        /// All colors used by the different font set options.
        /// </summary>
        public static readonly Color[] COLORS = new Color[] {
            Color.FromArgb(0, 0, 0),      // 0  // 0 // Black
            Color.FromArgb(255, 0, 0),    // 1  // 1 // Red
            Color.FromArgb(0, 255, 0),    // 2  // 2 // Green
            Color.FromArgb(255, 255, 0),  // 3  // 3 // Yellow
            Color.FromArgb(0, 0, 255),    // 4  // 4 // Blue
            Color.FromArgb(0, 255, 255),  // 5  // 5 // Cyan
            Color.FromArgb(255, 0, 255),  // 6  // 6 // Magenta
            Color.FromArgb(255, 255, 255),// 7  // 7 // White
            Color.FromArgb(128,0,255),    // 8  // 8 // Purple
            Color.FromArgb(0, 128, 90),   // 9  // 9 // Torqoise
            Color.FromArgb(122, 77, 35),  // 10 // a // Brown
            Color.FromArgb(128, 0, 0),    // 11 // ! // DarkRed
            Color.FromArgb(0, 128, 0),    // 12 // @ // DarkGreen
            Color.FromArgb(128, 128, 0),  // 13 // # // DarkYellow
            Color.FromArgb(0, 0, 128),    // 14 // $ // DarkBlue
            Color.FromArgb(0, 128, 128),  // 15 // % // DarkCyan
            Color.FromArgb(128, 0, 128),  // 16 // - // DarkMagenta
            Color.FromArgb(128, 128, 128),// 17 // & // LightGray
            Color.FromArgb(64, 0, 128),   // 18 // * // DarkPurple
            Color.FromArgb(0, 64, 40),    // 19 // ( // DarkTorqoise
            Color.FromArgb(64, 64, 64),   // 20 // ) // DarkGray
            Color.FromArgb(61, 38, 17),   // 21 // A // DarkBrown
        };

        private readonly static Point[] ShadowPoints = new Point[] {
            new Point(0, 1),
            new Point(1, 0),
            new Point(1, 1),
        };
        private readonly static Point[] BetterShadowPoints = new Point[] {
            new Point(0, 2),
            new Point(1, 2),
            new Point(2, 0),
            new Point(2, 1),
            new Point(2, 2),
        };
        private readonly static Point[] EmphasisPoints = new Point[] {
            new Point(0, -1),
            new Point(0, 1),
            new Point(1, 0),
            new Point(-1, 0),
        };
        private readonly static Point[] BetterEmphasisPoints = new Point[] {
            new Point(-1, -1),
            new Point(-1, 1),
            new Point(1, -1),
            new Point(1, 1),
            new Point(0, -2),
            new Point(0, 2),
            new Point(2, 0),
            new Point(-2, 0),
        };

        /// <summary>
        /// Represents the 'base' custom color, ie one that will be recognized as ignorable (in favor of the standard color code).
        /// </summary>
        private static readonly Color BaseCustomColor = Color.FromArgb(0, 0, 0, 0);

        /// <summary>
        /// Correctly forms a Color object for the color number and transparency amount, for use by RenderColoredText
        /// </summary>
        /// <param name="color">The color number.</param>
        /// <param name="trans">Transparency value, 0-255.</param>
        /// <returns>A correctly formed color object.</returns>
        public static Color ColorFor(int color, int trans)
        {
            return Color.FromArgb(trans, COLORS[color].R, COLORS[color].G, COLORS[color].B);
        }

        /// <summary>
        /// Fully renders fancy text.
        /// <para>Consider using <see cref="SplitAppropriately(string, int)"/> to split the input for a maximum width.</para>
        /// <para>Fancy text is normal text with special color and format markings, in the form of a caret symbol '^' followed by a case-sensitive single character indicating the format or color to apply.</para>
        /// <para>Includes the following format codes:</para>
        /// <para> 0-9: simple color, refer to <see cref="COLORS"/>. The shift variant of these keys, as found on a US-QWERTY keyboard, apply a darker variant of the same color, with the exception of '^' which is instead represented by '-'.</para>
        /// <para>b: Toggles bold.</para>
        /// <para>i: Toggles italic.</para>
        /// <para>u: Toggles underlining. Preserves color at time of usage for the underline.</para>
        /// <para>s: Toggles strike-through. Preserves color at time of usage for the strike-through line.</para>
        /// <para>O: Toggles overlining. Preserves color at time of usage for the overline.</para>
        /// <para>h: Toggles highlighting. Preserves color at time of usage for the highlight.</para>
        /// <para>e: Toggles 'emphasis'. This is a colored glow around the text, making it extremely visible. Preserves color at time of usage for the emphasis. Take care of what color you apply to the emphasis vs. the text being emphasized, only some combinations look good.</para>
        /// <para>t,T,o: Changes transparency level. 't' is 50%, 'T' is 25% (VERY transparent). 'o' is opaque.</para>
        /// <para>S: Toggles super-script (text that's smaller and higher).</para>
        /// <para>l: Toggles sub-script ('lower' text. Similar to super-script, but lower rather than higher).</para>
        /// <para>d: Toggles drop-shadowing.</para>
        /// <para>j: Toggles 'jelly' mode. Text in this mode appears to slightly shake in place.</para>
        /// <para>U: Toggles 'unreadable' mode. Text in this mode will randomly shift through characters, becoming impossible to read.</para>
        /// <para>R: Toggles randomly changing color mode, AKA Rainbow mode.</para>
        /// <para>p: Toggles 'pseudo-random' color mode. Similar to rainbow mode, but colors are randomly chosen once per-character, and then do not change (using a pseudo-random algorithm seeded from the text input).</para>
        /// <para>f: Toggles flipped-text.</para>
        /// <para>B: Applies the base color/format.</para>
        /// <para>q: A Simply gets replaced by a quote symbol. May be useful for some escaping environments.</para>
        /// <para>r: Resets the basic format to none.</para>
        /// <para>n: No-op. Useful to break codes. A simple way to prevent users from entering format codes into text that will be rendered this way, is <code>text = text.Replace("^", "^^n");</code>. This is the methodology used by <see cref="EscapeFancyText(string)"/>.</para>
        /// <para>[: This is a special meta-symbol that indicates a longer input follows, of the format: ^[x=y].
        /// The 'x' input, for the sake of this method, can be 'color' (set a custom RGB color) or 'lang' (read text from a language file),
        /// however other portions of the engine may apply other options (like 'hover' or 'click').</para>
        /// </summary>
        /// <param name="text">The text to render.</param>
        /// <param name="position">The position on screen to render at.</param>
        /// <param name="maxY">Optional: The maximum Y value to keep drawing at (to prevent text from going past the end of a text-area).</param>
        /// <param name="transmod">Optional: Transparency modifier, from 0 to 1 (1 is opaque, lower is more transparent).</param>
        /// <param name="extraShadow">Optional: If set to true, will cause a drop shadow to be drawn behind all text (even if '^d' is flipped off).</param>
        /// <param name="baseColor">Optional: The 'base color', to be used when '^B' is used (note: it's often good to apply the baseColor to the start of the text, as it will not be applied automatically).</param>
        public void DrawFancyText(string text, Location position, int maxY = int.MaxValue, float transmod = 1, bool extraShadow = false, string baseColor = "^r^7")
        {
            DrawFancyText_InternalDetail(text, position, maxY, transmod, extraShadow, baseColor);
        }

        /// <summary>
        /// Internal call to handle fancy-text rendering, with direct parameters for all key settings.
        /// Generally not meant to be called from external code.
        /// Generally, prefer <see cref="DrawFancyText(string, Location, int, float, bool, string)"/>.
        /// </summary>
        public void DrawFancyText_InternalDetail(string text, Location position, int maxY = int.MaxValue, float transmod = 1, bool extraShadow = false, string baseColor = "^r^7",
            int _color = DefaultColor, bool _bold = false, bool _italic = false, bool _underline = false, bool _strike = false, bool _overline = false, bool _highlight = false, bool _emphasis = false,
            int _underlineColor = DefaultColor, int _strikeColor = DefaultColor, int _overlineColor = DefaultColor, int _highlightColor = DefaultColor, int _emphasisColor = DefaultColor,
            bool _superScript = false, bool _subScript = false, bool _flip = false, bool _pseudoRandom = false, bool _jello = false, bool _unreadable = false, bool _random = false, bool _shadow = false, GLFont _font = null)
        {
            GraphicsUtil.CheckError("Render FontSet - Pre");
            r_depth++;
            if (r_depth >= 100 && text != "{{Recursion error}}")
            {
                DrawFancyText("{{Recursion error}}", position);
                r_depth--;
                return;
            }
            text = text.ApplyBaseColor(baseColor);
            string[] lines = text.Replace('\r', ' ').Replace(' ', (char)0x00A0).Replace("^q", "\"").SplitFast('\n');
            void render(string line, float Y, TextVBOBuilder vbo)
            {
                int color = _color;
                bool bold = _bold;
                bool italic = _italic;
                bool underline = _underline;
                bool strike = _strike;
                bool overline = _overline;
                bool highlight = _highlight;
                bool emphasis = _emphasis;
                int underlineColor = _underlineColor;
                int strikeColor = _strikeColor;
                int overlineColor = _overlineColor;
                int highlightColor = _highlightColor;
                int emphasisColor = _emphasisColor;
                bool superScript = _superScript;
                bool subScript = _subScript;
                bool flip = _flip;
                bool pseudoRandom = _pseudoRandom;
                bool jello = _jello;
                bool unreadable = _unreadable;
                bool random = _random;
                bool shadow = _shadow;
                GLFont font = _font;
                int transparency = (int)(255 * transmod);
                int overlineTransparency = (int)(255 * transmod);
                int emphasisTransparency = (int)(255 * transmod);
                int highlightTransparency = (int)(255 * transmod);
                int strikeTransparency = (int)(255 * transmod);
                int underlineTransparency = (int)(255 * transmod);
                float X = (float)position.X;
                Color customColor = BaseCustomColor;
                if (font == null)
                {
                    font = FontDefault;
                }
                int start = 0;
                for (int x = 0; x < line.Length; x++)
                {
                    if ((line[x] == '^' && x + 1 < line.Length && (IsFormatSymbol(line[x + 1]) || line[x + 1] == '[')) || (x + 1 == line.Length))
                    {
                        string drawme = line.Substring(start, (x - start) + ((x + 1 < line.Length) ? 0 : 1));
                        start = x + 2;
                        x++;
                        if (drawme.Length > 0 && Y >= -font.Height && Y - (subScript ? font.Height : 0) <= maxY)
                        {
                            float width = font.MeasureString(drawme);
                            if (highlight)
                            {
                                DrawRectangle(X, Y, width, FontDefault.Height, ColorFor(highlightColor, highlightTransparency), vbo);
                            }
                            if (underline)
                            {
                                DrawRectangle(X, Y + ((float)font.Height * 4f / 5f), width, 2, ColorFor(underlineColor, underlineTransparency), vbo);
                            }
                            if (overline)
                            {
                                DrawRectangle(X, Y + 2f, width, 2, ColorFor(overlineColor, overlineTransparency), vbo);
                            }
                            if (extraShadow)
                            {
                                foreach (Point point in ShadowPoints)
                                {
                                    RenderBaseText(vbo, X + point.X, Y + point.Y, drawme, font, 0, transparency / 2, flip);
                                }
                            }
                            if (shadow)
                            {
                                foreach (Point point in ShadowPoints)
                                {
                                    RenderBaseText(vbo, X + point.X, Y + point.Y, drawme, font, 0, transparency / 2, flip);
                                }
                                foreach (Point point in BetterShadowPoints)
                                {
                                    RenderBaseText(vbo, X + point.X, Y + point.Y, drawme, font, 0, transparency / 4, flip);
                                }
                            }
                            if (emphasis)
                            {
                                foreach (Point point in EmphasisPoints)
                                {
                                    RenderBaseText(vbo, X + point.X, Y + point.Y, drawme, font, emphasisColor, emphasisTransparency, flip);
                                }
                                foreach (Point point in BetterEmphasisPoints)
                                {
                                    RenderBaseText(vbo, X + point.X, Y + point.Y, drawme, font, emphasisColor, emphasisTransparency, flip);
                                }
                            }
                            RenderBaseText(vbo, X, Y, drawme, font, color, transparency, flip, pseudoRandom, random, jello, unreadable, customColor);
                            if (strike)
                            {
                                DrawRectangle(X, Y + (font.Height / 2), width, 2, ColorFor(strikeColor, strikeTransparency), vbo);
                            }
                            X += width;
                        }
                        if (x < line.Length)
                        {
                            switch (line[x])
                            {
                                case '[':
                                    {
                                        StringBuilder sb = new StringBuilder();
                                        x++;
                                        int c = 0;
                                        while (x < line.Length)
                                        {
                                            if (line[x] == '[')
                                            {
                                                c++;
                                            }
                                            if (line[x] == ']')
                                            {
                                                c--;
                                                if (c == -1)
                                                {
                                                    break;
                                                }
                                            }
                                            sb.Append(line[x]);
                                            x++;
                                        }
                                        bool highl = true;
                                        string ttext;
                                        if (x == line.Length)
                                        {
                                            ttext = "^[" + sb.ToString();
                                        }
                                        else
                                        {
                                            string sbt = sb.ToString();
                                            string sbl = sbt.ToLowerFast();
                                            if (sbl.StartsWith("lang="))
                                            {
                                                string langinfo = sbl.After("lang=");
                                                string[] subdats = CSplit(langinfo).ToArray();
                                                ttext = Engine.GetLanguageHelper(subdats);
                                                highl = false;
                                            }
                                            else if (sbl.StartsWith("color="))
                                            {
                                                string[] coldat = sbl.After("color=").SplitFast(',');
                                                if (coldat.Length == 4)
                                                {
                                                    int r = StringConversionHelper.StringToInt(coldat[0]);
                                                    int g = StringConversionHelper.StringToInt(coldat[1]);
                                                    int b = StringConversionHelper.StringToInt(coldat[2]);
                                                    int a = StringConversionHelper.StringToInt(coldat[3]);
                                                    customColor = Color.FromArgb((byte)a, (byte)r, (byte)g, (byte)b);
                                                    ttext = "";
                                                    highl = false;
                                                }
                                                else
                                                {
                                                    ttext = "^[" + sb.ToString();
                                                }
                                            }
                                            else if (sbl == "lb")
                                            {
                                                ttext = "[";
                                                highl = false;
                                            }
                                            else if (sbl == "rb")
                                            {
                                                ttext = "]";
                                                highl = false;
                                            }
                                            else
                                            {
                                                ttext = sbt.After("|");
                                            }
                                        }
                                        if (highl)
                                        {
                                            float widt = FontDefault.MeasureString(ttext);
                                            DrawRectangle(X, Y, widt, FontDefault.Height, Color.Black, vbo);
                                            RenderBaseText(vbo, X, Y, ttext, FontDefault, 5);
                                            DrawRectangle(X, Y + ((float)FontDefault.Height * 4f / 5f), widt, 2, Color.Blue, vbo);
                                            X += widt;
                                        }
                                        else
                                        {
                                            float widt = MeasureFancyText(ttext);
                                            DrawFancyText_InternalDetail(ttext, new Location(X, Y, 0), maxY, transmod, extraShadow, baseColor,
                                                color, bold, italic, underline, strike, overline, highlight, emphasis, underlineColor, strikeColor, overlineColor, highlightColor, emphasisColor, superScript,
                                                subScript, flip, pseudoRandom, jello, unreadable, random, shadow, font);
                                            X += widt;
                                        }
                                        start = x + 1;
                                    }
                                    break;
                                case '1': color = 1; customColor = BaseCustomColor; break;
                                case '!': color = 11; customColor = BaseCustomColor; break;
                                case '2': color = 2; customColor = BaseCustomColor; break;
                                case '@': color = 12; customColor = BaseCustomColor; break;
                                case '3': color = 3; customColor = BaseCustomColor; break;
                                case '#': color = 13; customColor = BaseCustomColor; break;
                                case '4': color = 4; customColor = BaseCustomColor; break;
                                case '$': color = 14; customColor = BaseCustomColor; break;
                                case '5': color = 5; customColor = BaseCustomColor; break;
                                case '%': color = 15; customColor = BaseCustomColor; break;
                                case '6': color = 6; customColor = BaseCustomColor; break;
                                case '-': color = 16; customColor = BaseCustomColor; break;
                                case '7': color = 7; customColor = BaseCustomColor; break;
                                case '&': color = 17; customColor = BaseCustomColor; break;
                                case '8': color = 8; customColor = BaseCustomColor; break;
                                case '*': color = 18; customColor = BaseCustomColor; break;
                                case '9': color = 9; customColor = BaseCustomColor; break;
                                case '(': color = 19; customColor = BaseCustomColor; break;
                                case '0': color = 0; customColor = BaseCustomColor; break;
                                case ')': color = 20; customColor = BaseCustomColor; break;
                                case 'a': color = 10; customColor = BaseCustomColor; break;
                                case 'A': color = 21; customColor = BaseCustomColor; break;
                                case 'i':
                                    {
                                        italic = true;
                                        GLFont nfont = (superScript || subScript) ? (bold ? FontBoldItalicHalf : FontItalicHalf) :
                                            (bold ? FontBoldItalic : FontItalic);
                                        if (nfont != font)
                                        {
                                            font = nfont;
                                        }
                                    }
                                    break;
                                case 'b':
                                    {
                                        bold = true;
                                        GLFont nfont = (superScript || subScript) ? (italic ? FontBoldItalicHalf : FontBoldHalf) :
                                            (italic ? FontBoldItalic : FontBold);
                                        if (nfont != font)
                                        {
                                            font = nfont;
                                        }
                                    }
                                    break;
                                case 'u': underlineTransparency = transparency; underline = true; underlineColor = color; break;
                                case 's': strikeTransparency = transparency; strike = true; strikeColor = color; break;
                                case 'h': highlightTransparency = transparency; highlight = true; highlightColor = color; break;
                                case 'e': emphasisTransparency = transparency; emphasis = true; emphasisColor = color; break;
                                case 'O': overlineTransparency = transparency; overline = true; overlineColor = color; break;
                                case 't': transparency = (int)(128 * transmod); break;
                                case 'T': transparency = (int)(64 * transmod); break;
                                case 'o': transparency = (int)(255 * transmod); break;
                                case 'S':
                                    if (!superScript)
                                    {
                                        if (subScript)
                                        {
                                            subScript = false;
                                            Y -= font.Height / 2;
                                        }
                                        GLFont nfont = bold && italic ? FontBoldItalicHalf : bold ? FontBoldHalf :
                                            italic ? FontItalicHalf : FontHalf;
                                        if (nfont != font)
                                        {
                                            font = nfont;
                                        }
                                    }
                                    superScript = true;
                                    break;
                                case 'l':
                                    if (!subScript)
                                    {
                                        if (superScript)
                                        {
                                            superScript = false;
                                        }
                                        Y += FontDefault.Height / 2;
                                        GLFont nfont = bold && italic ? FontBoldItalicHalf : bold ? FontBoldHalf :
                                            italic ? FontItalicHalf : FontHalf;
                                        if (nfont != font)
                                        {
                                            font = nfont;
                                        }
                                    }
                                    subScript = true;
                                    break;
                                case 'd': shadow = true; break;
                                case 'j': jello = true; break;
                                case 'U': unreadable = true; break;
                                case 'R': random = true; break;
                                case 'p': pseudoRandom = true; break;
                                case 'f': flip = true; break;
                                case 'n':
                                    break;
                                case 'r':
                                    {
                                        GLFont nfont = FontDefault;
                                        if (nfont != font)
                                        {
                                            font = nfont;
                                        }
                                        if (subScript)
                                        {
                                            Y -= FontDefault.Height / 2;
                                        }
                                        subScript = false;
                                        superScript = false;
                                        flip = false;
                                        random = false;
                                        pseudoRandom = false;
                                        jello = false;
                                        unreadable = false;
                                        shadow = false;
                                        bold = false;
                                        italic = false;
                                        underline = false;
                                        strike = false;
                                        emphasis = false;
                                        highlight = false;
                                        transparency = (int)(255 * transmod);
                                        overline = false;
                                        break;
                                    }
                                default:
                                    break;
                            }
                        }
                    }
                }
            }
            if (lines.Length == 1)
            {
                render(lines[0], (float)position.Y, ReusableTextVBO);
            }
            else
            {
                float Y = (float)position.Y;
                string tcol = "";
                for (int i = 0; i < lines.Length; i++)
                {
                    string line = lines[i];
                    if (line.Length > 0)
                    {
                        float ty = Y;
                        string tcc = tcol;
                        render(tcc + line, ty, ReusableTextVBO);
                        tcol += GrabAllFormats(line);
                    }
                    Y += FontDefault.Height;
                }
            }
            Engine.GLFonts.Shaders.TextCleanerShader.Bind();
            Matrix4 ortho = Engine.GetOrtho();
            GL.UniformMatrix4(1, false, ref ortho);
            //Matrix4 ident = Matrix4.Identity;
            //GL.UniformMatrix4(2, false, ref ident);
            Vector3 col = new Vector3(1, 1, 1);
            GL.Uniform3(3, ref col);
            ReusableTextVBO.Build();
            ReusableTextVBO.Render(Engine.GLFonts);
            if (Engine.FixToShader == null)
            {
                Engine.GLFonts.Shaders.ColorMultShader.Bind();
            }
            else
            {
                Engine.FixToShader.Bind();
            }
            r_depth--;
            GraphicsUtil.CheckError("Render FontSet");
        }

        /// <summary>
        /// The <see cref="TextVBOBuilder"/> that's reused for text rendering.
        /// </summary>
        public TextVBOBuilder ReusableTextVBO = new TextVBOBuilder();

        /// <summary>
        /// The Font Engine's Task Factory.
        /// </summary>
        public TaskFactory TFactory = new TaskFactory();

        /// <summary>
        /// Grabs a string containing only formats/colors from the string containing text.
        /// </summary>
        /// <param name="input">The input string.</param>
        /// <returns>The color set.</returns>
        public static string GrabAllFormats(string input)
        {
            StringBuilder res = new StringBuilder();
            int cap = input.Length - 1;
            for (int i = 0; i < cap; i++)
            {
                if (input[i] == '^' && IsFormatSymbol(input[i + 1]))
                {
                    res.Append("^" + input[i + 1]);
                }
            }
            return res.ToString();
        }

        /// <summary>
        /// Escapes fancy text to render as plain text.
        /// </summary>
        /// <param name="input">Unescaped text.</param>
        /// <returns>Escaped text.</returns>
        public static string EscapeFancyText(string input)
        {
            return input.Replace("^", "^^n");
        }

        const double RAND_DIV = 40.0;

        /// <summary>
        /// Semi-internal rendering of text strings.
        /// <para>Generally, external code should use <see cref="DrawFancyText(string, Location, int, float, bool, string)"/>.</para>
        /// </summary>
        /// <param name="vbo">The VBO to render with.</param>
        /// <param name="X">The X location to render at.</param>
        /// <param name="Y">The Y location to render at.</param>
        /// <param name="text">The text to render.</param>
        /// <param name="font">The font to use.</param>
        /// <param name="color">The color ID number to use.</param>
        /// <param name="trans">Transparency.</param>
        /// <param name="flip">Whether to flip the text.</param>
        /// <param name="pseudo">Whether to use pseudo-random color.</param>
        /// <param name="random">Whether to use real-random color.</param>
        /// <param name="jello">Whether to use a jello effect.</param>
        /// <param name="unreadable">Whether to randomize letters.</param>
        /// <param name="currentColor">The current color.</param>
        /// <returns>The length of the rendered text in pixels.</returns>
        public float RenderBaseText(TextVBOBuilder vbo, float X, float Y, string text, GLFont font, int color,
            int trans = 255, bool flip = false, bool pseudo = false, bool random = false, bool jello = false, bool unreadable = false, Color currentColor = default)
        {
            if (unreadable || pseudo || random || jello)
            {
                float nX = 0;
                foreach (string txt in font.SeparateEmojiAndSpecialChars(text))
                {
                    string chr = txt;
                    // int col = color;
                    Color tcol = ColorFor(color, trans);
                    if (random)
                    {
                        double ttime = Engine.GetGlobalTickTime();
                        double tempR = SimplexNoise.Generate((X + nX) / RAND_DIV + ttime * 0.4, Y / RAND_DIV);
                        double tempG = SimplexNoise.Generate((X + nX) / RAND_DIV + ttime * 0.4, Y / RAND_DIV + 7.6f);
                        double tempB = SimplexNoise.Generate((X + nX) / RAND_DIV + ttime * 0.4, Y / RAND_DIV + 18.42f);
                        tcol = Color.FromArgb((int)(tempR * 255), (int)(tempG * 255), (int)(tempB * 255));
                    }
                    else if (pseudo)
                    {
                        tcol = ColorFor((chr[0] % (COLORS.Length - 1)) + 1, trans);
                    }
                    else if (currentColor.A > 0)
                    {
                        tcol = currentColor;
                    }
                    if (unreadable)
                    {
                        chr = ((char)Engine.RandomHelper.Next(33, 126)).ToString();
                    }
                    int iX = 0;
                    int iY = 0;
                    if (jello)
                    {
                        iX = Engine.RandomHelper.Next(-1, 1);
                        iY = Engine.RandomHelper.Next(-1, 1);
                    }
                    Vector4 col = new Vector4((float)tcol.R / 255f, (float)tcol.G / 255f, (float)tcol.B / 255f, (float)tcol.A / 255f);
                    if (flip)
                    {
                        font.DrawSingleCharacterFlipped(chr, X + iX + nX, Y + iY, vbo, col);
                    }
                    else
                    {
                        font.DrawSingleCharacter(chr, X + iX + nX, Y + iY, vbo, col);
                    }
                    nX += font.RectForSymbol(txt).Width;
                }
                return nX;
            }
            else
            {
                Color tcol = currentColor.A > 0 ? currentColor : ColorFor(color, trans);
                return font.DrawString(text, X, Y, new Vector4((float)tcol.R / 255f, (float)tcol.G / 255f, (float)tcol.B / 255f, (float)tcol.A / 255f), vbo, flip);
            }
        }

        /// <summary>
        /// Measures several lines of text.
        /// </summary>
        /// <param name="text">The text.</param>
        /// <param name="bcolor">The base color.</param>
        /// <returns>The size.</returns>
        public Location MeasureFancyLinesOfText(string text, string bcolor = "^r^7")
        {
            string[] data = text.SplitFast('\n');
            float len = 0;
            for (int i = 0; i < data.Length; i++)
            {
                float newlen = MeasureFancyText(data[i], bcolor);
                if (newlen > len)
                {
                    len = newlen;
                }
            }
            return new Location(len, data.Length * FontDefault.Height, 0);
        }

        /// <summary>
        /// Measures fancy notated text strings.
        /// Note: Do not include newlines!
        /// </summary>
        /// <param name="line">The text to measure.</param>
        /// <param name="bcolor">The base color.</param>
        /// <param name="pushStr">Whether to push the string's contents to the render set.</param>
        /// <returns>the X-width of the text.</returns>
        public float MeasureFancyText(string line, string bcolor = "^r^7", bool pushStr = false)
        {
            return MeasureFancyText(line, out List<KeyValuePair<string, Rectangle2F>> _, bcolor, pushStr: pushStr);
        }

        /// <summary>
        /// Helper to split strings.
        /// </summary>
        /// <param name="input">The original string.</param>
        /// <returns>The split string.</returns>
        public static List<string> CSplit(string input)
        {
            List<string> temp = new List<string>();
            int start = 0;
            int c = 0;
            for (int i = 0; i < input.Length; i++)
            {
                if (input[i] == '[')
                {
                    c++;
                }
                if (input[i] == ']')
                {
                    c--;
                }
                if (c == 0 && input[i] == '|')
                {
                    temp.Add(input[start..i]);
                    start = i + 1;
                }
            }
            temp.Add(input[start..]);
            return temp;
        }

        [ThreadStatic]
        static int m_depth = 0;

        [ThreadStatic]
        static int r_depth = 0;

        /// <summary>
        /// Measures fancy text.
        /// </summary>
        /// <param name="line">The line of text.</param>
        /// <param name="links">Output for any hover/click links.</param>
        /// <param name="bcolor">The base color.</param>
        /// <param name="bold">Whether it is bold.</param>
        /// <param name="italic">Whether it is italic.</param>
        /// <param name="sub">Whether it is half-size.</param>
        /// <param name="font">The font to start with.</param>
        /// <param name="pushStr">Whether to push text to the underlying engine (ie, to make sure the underlying characters are recognizable and valid).</param>
        /// <returns>The width.</returns>
        public float MeasureFancyText(string line, out List<KeyValuePair<string, Rectangle2F>> links, string bcolor = "^r^7", bool bold = false, bool italic = false, bool sub = false, GLFont font = null, bool pushStr = false)
        {
            List<KeyValuePair<string, Rectangle2F>> tlinks = new List<KeyValuePair<string, Rectangle2F>>();
            m_depth++;
            if (m_depth >= 100)
            {
                m_depth--;
                links = tlinks;
                return font.MeasureString("{{Recursion error}}");
            }
            float MeasWidth = 0;
            if (font == null)
            {
                font = FontDefault;
            }
            int start = 0;
            line = line.Replace("^q", "\"").ApplyBaseColor(bcolor); // TODO: Effic of replace usage? And of per-line replaces?
            for (int x = 0; x < line.Length; x++)
            {
                if ((line[x] == '^' && x + 1 < line.Length && (IsFormatSymbol(line[x + 1]) || line[x + 1] == '[')) || (x + 1 == line.Length))
                {
                    string drawme = line.Substring(start, (x - start) + ((x + 1 < line.Length) ? 0 : 1));
                    start = x + 2;
                    x++;
                    if (drawme.Length > 0)
                    {
                        if (pushStr)
                        {
                            font.RecognizeCharacters(drawme);
                        }
                        MeasWidth += font.MeasureString(drawme);
                    }
                    if (x < line.Length)
                    {
                        switch (line[x])
                        {
                            case '[':
                                {
                                    StringBuilder sb = new StringBuilder();
                                    x++;
                                    int c = 0;
                                    while (x < line.Length)
                                    {
                                        if (line[x] == '[')
                                        {
                                            c++;
                                        }
                                        if (line[x] == ']')
                                        {
                                            c--;
                                            if (c == -1)
                                            {
                                                break;
                                            }
                                        }
                                        sb.Append(line[x]);
                                        x++;
                                    }
                                    bool highl = true;
                                    string ttext;
                                    if (x == line.Length)
                                    {
                                        ttext = "^[" + sb.ToString();
                                    }
                                    else
                                    {
                                        string sbt = sb.ToString();
                                        string sbl = sbt.ToLowerFast();
                                        if (sbl.StartsWith("lang="))
                                        {
                                            string langinfo = sbl.After("lang=");
                                            string[] subdats = CSplit(langinfo).ToArray();
                                            ttext = Engine.GetLanguageHelper(subdats);
                                            highl = false;
                                        }
                                        else if (sbl.StartsWith("color="))
                                        {
                                            ttext = "";
                                            highl = false;
                                        }
                                        else if (sbl == "lb")
                                        {
                                            ttext = "[";
                                            highl = false;
                                        }
                                        else if (sbl == "rb")
                                        {
                                            ttext = "]";
                                            highl = false;
                                        }
                                        else
                                        {
                                            ttext = sbt.After("|");
                                        }
                                    }
                                    if (highl)
                                    {
                                        if (pushStr)
                                        {
                                            FontDefault.RecognizeCharacters(ttext);
                                        }
                                        float widt = FontDefault.MeasureString(ttext);
                                        tlinks.Add(new KeyValuePair<string, Rectangle2F>(sb.ToString().Before("|"), new Rectangle2F() { X = MeasWidth, Y = 0, Width = widt, Height = FontDefault.Height }));
                                        MeasWidth += widt;
                                    }
                                    else
                                    {
                                        float widt = MeasureFancyText(ttext, out _, bcolor, bold, italic, sub, font);
                                        MeasWidth += widt;
                                    }
                                    start = x + 1;
                                }
                                break;
                            case 'r':
                                font = FontDefault;
                                bold = false;
                                sub = false;
                                italic = false;
                                break;
                            case 'S':
                            case 'l':
                                font = bold && italic ? FontBoldItalicHalf : bold ? FontBoldHalf :
                                    italic ? FontItalicHalf : FontHalf;
                                sub = true;
                                break;
                            case 'i':
                                italic = true;
                                font = (sub) ? (bold ? FontBoldItalicHalf : FontItalicHalf) :
                                    (bold ? FontBoldItalic : FontItalic);
                                break;
                            case 'b':
                                bold = true;
                                font = (sub) ? (italic ? FontBoldItalicHalf : FontBoldHalf) :
                                    (italic ? FontBoldItalic : FontBold);
                                break;
                            default:
                                break;
                        }
                    }
                }
            }
            links = tlinks;
            m_depth--;
            return MeasWidth;
        }

        /// <summary>
        /// Splits a string at a maximum render width.
        /// </summary>
        /// <param name="text">The base text.</param>
        /// <param name="maxX">The maximum width.</param>
        /// <returns>The split string.</returns>
        public string SplitAppropriately(string text, int maxX)
        {
            StringBuilder resultBuilder = new StringBuilder(text.Length + 50);
            string[] lines = text.Split('\n');
            foreach (string fullLine in lines)
            {
                while (true)
                {
                    string line = fullLine;
                    float width = MeasureFancyText(line);
                    if (width <= maxX)
                    {
                        resultBuilder.Append(line).Append('\n');
                        break;
                    }
                    float expectedSegments = width / maxX;
                    int expectedCharacterCount = (int)(line.Length / expectedSegments);
                    float subWidth = MeasureFancyText(line.Substring(0, expectedCharacterCount));
                    int target = expectedCharacterCount;
                    if (subWidth <= maxX)
                    {
                        for (int i = expectedCharacterCount; i < line.Length; i++)
                        {
                            if (MeasureFancyText(line.Substring(0, i)) > maxX)
                            {
                                target = i - 1;
                                break;
                            }
                        }
                    }
                    else // width > maxX
                    {
                        for (int i = expectedCharacterCount; i >= 0; i--)
                        {
                            if (MeasureFancyText(line.Substring(0, i)) <= maxX)
                            {
                                target = i;
                                break;
                            }
                        }
                    }
                    int lastSpace = MathHelper.Clamp(line.IndexOf(' '), 1, target);
                    resultBuilder.Append(line, 0, lastSpace);
                    //line = line.Substring(lastSpace + 1);
                }
            }
            resultBuilder.Length -= 1; // Trim off the final newline.
            return resultBuilder.ToString();
        }

        /// <summary>
        /// Draws a rectangle to screen.
        /// </summary>
        /// <param name="X">The starting X.</param>
        /// <param name="Y">The starting Y.</param>
        /// <param name="width">The width.</param>
        /// <param name="height">The height.</param>
        /// <param name="c">The color to use.</param>
        /// <param name="vbo">The VBO to render with.</param>
        public void DrawRectangle(float X, float Y, float width, float height, Color c, TextVBOBuilder vbo)
        {
            TextVBOBuilder.AddQuad(X, Y, X + width, Y + height, 2f / GLFontEngine.DEFAULT_TEXTURE_SIZE_WIDTH, 2f / Engine.GLFonts.CurrentHeight, 4f / GLFontEngine.DEFAULT_TEXTURE_SIZE_WIDTH, 4f / Engine.GLFonts.CurrentHeight,
                new Vector4((float)c.R / 255f, (float)c.G / 255f, (float)c.B / 255f, (float)c.A / 255f));
        }

        /// <summary>
        /// Matcher object to recognize color/format codes.
        /// </summary>
        public static AsciiMatcher FORMAT_CODES_MATCHER = new AsciiMatcher("0123456789" + "ab" + "def" + "hij" + "l" + "nopqrstu" + "AB" + "RSTU" + "!@#$%&*()-");

        /// <summary>
        /// Used to identify if an input character is a valid color/format symbol (generally the character that follows a '^'), for use by <see cref="DrawFancyText(string, Location, int, float, bool, string)"/>.
        /// <para>Does not return true for '[' as that is not a formatter but a long-block format adjuster.</para>
        /// </summary>
        /// <param name="c">The character to check.</param>
        /// <returns>whether the character is a valid color symbol.</returns>
        public static bool IsFormatSymbol(char c)
        {
            return FORMAT_CODES_MATCHER.IsMatch(c);
        }
    }
}
