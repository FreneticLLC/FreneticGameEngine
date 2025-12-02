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
using FreneticUtilities.FreneticExtensions;
using FreneticUtilities.FreneticToolkit;
using FGECore.ConsoleHelpers;
using FGECore.CoreSystems;
using FGECore.MathHelpers;
using FGECore.StackNoteSystem;
using FGECore.UtilitySystems;
using OpenTK;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace FGEGraphics.GraphicsHelpers.FontSets;

/// <summary>Contains various <see cref="GLFont"/>s needed to render fancy text.</summary>
/// <param name="_name">The name of the set.</param>
/// <param name="engine">The backing engine.</param>
public class FontSet(string _name, FontSetEngine engine) : IEquatable<FontSet>
{
    /// <summary>The backing engine.</summary>
    public FontSetEngine Engine = engine;

    /// <summary>Default font.</summary>
    public GLFont FontDefault;

    /// <summary>Bold font.</summary>
    public GLFont FontBold;

    /// <summary>Italic font.</summary>
    public GLFont FontItalic;

    /// <summary>Bold+Italic font.</summary>
    public GLFont FontBoldItalic;

    /// <summary> Half-size font.</summary>
    public GLFont FontHalf;

    /// <summary>Half-size bold font.</summary>
    public GLFont FontBoldHalf;

    /// <summary>Half-size italic font.</summary>
    public GLFont FontItalicHalf;

    /// <summary>Half-size bold+italic font.</summary>
    public GLFont FontBoldItalicHalf;

    /// <summary>Name of the font set.</summary>
    public string Name = _name.ToLowerFast();

    public int Size;// = fontsize;

    /// <summary>Height, in pixels, of this fontset (based on <see cref="FontDefault"/>'s height) (ie how tall a standard symbol is, or how wide the line gap needs to be).</summary>
    public int Height => FontDefault.Height;

    /// <summary>Loads the font set.</summary>
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
        Size = fontsize;
    }

    /// <summary>All colors used by the different font set options.</summary>
    public static readonly Color4F[] COLORS = [
        new Color4F(0, 0, 0),      // 0  // 0 // Black
        new Color4F(1, 0, 0),    // 1  // 1 // Red
        new Color4F(0, 1, 0),    // 2  // 2 // Green
        new Color4F(1, 1, 0),  // 3  // 3 // Yellow
        new Color4F(0, 0, 1),    // 4  // 4 // Blue
        new Color4F(0, 1, 1),  // 5  // 5 // Cyan
        new Color4F(1, 0, 1),  // 6  // 6 // Magenta
        new Color4F(1, 1, 1),// 7  // 7 // White
        new Color4F(0.5f, 0, 1),    // 8  // 8 // Purple
        Color4F.FromArgb(0, 128, 90),   // 9  // 9 // Torqoise
        Color4F.FromArgb(122, 77, 35),  // 10 // a // Brown
        new Color4F(0.5f, 0, 0),    // 11 // ! // DarkRed
        new Color4F(0, 0.5f, 0),    // 12 // @ // DarkGreen
        new Color4F(0.5f, 0.5f, 0),  // 13 // # // DarkYellow
        new Color4F(0, 0, 0.5f),    // 14 // $ // DarkBlue
        new Color4F(0, 0.5f, 0.5f),  // 15 // % // DarkCyan
        new Color4F(0.5f, 0, 0.5f),  // 16 // - // DarkMagenta
        new Color4F(0.5f, 0.5f, 0.5f),// 17 // & // LightGray
        new Color4F(0.25f, 0, 0.5f),   // 18 // * // DarkPurple
        Color4F.FromArgb(0, 64, 40),    // 19 // ( // DarkTorqoise
        Color4F.FromArgb(64, 64, 64),   // 20 // ) // DarkGray
        Color4F.FromArgb(61, 38, 17),   // 21 // A // DarkBrown
    ];

    private readonly static Point[] ShadowPoints = [
        new Point(0, 1),
        new Point(1, 0),
        new Point(1, 1),
    ];
    private readonly static Point[] BetterShadowPoints = [
        new Point(0, 2),
        new Point(1, 2),
        new Point(2, 0),
        new Point(2, 1),
        new Point(2, 2),
    ];
    private readonly static Point[] EmphasisPoints = [
        new Point(0, -1),
        new Point(0, 1),
        new Point(1, 0),
        new Point(-1, 0),
    ];
    private readonly static Point[] BetterEmphasisPoints = [
        new Point(-1, -1),
        new Point(-1, 1),
        new Point(1, -1),
        new Point(1, 1),
        new Point(0, -2),
        new Point(0, 2),
        new Point(2, 0),
        new Point(-2, 0),
    ];

    /// <summary>Correctly forms a Color object for the color number and transparency amount, for use by RenderColoredText.</summary>
    /// <param name="color">The color number.</param>
    /// <param name="trans">Transparency value, 0-1.</param>
    /// <returns>A correctly formed color object.</returns>
    public static Color4F ColorFor(int color, float trans)
    {
        return new Color4F(COLORS[color].RGB, trans);
    }

    [ThreadStatic]
    private static int ParseDepth;

    /// <summary>
    /// Helper cache to reduce over-parsing of reused fancy text.
    /// Key is (baseColor, text), value is renderable text.
    /// TODO: This should be auto-cleaned somehow to avoid wasting RAM.
    /// </summary>
    public Dictionary<(string, string), RenderableText> FancyTextCache = [];

    /// <summary>
    /// Parses fancy text from raw fancy-text input to renderable data objects.
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
    public RenderableText ParseFancyText(string originalText, string baseColor = "^r^7")
    {
        if (FancyTextCache.TryGetValue((baseColor, originalText), out RenderableText output))
        {
            return output;
        }
        using var _push = StackNoteHelper.UsePush("FontSet - Parse fancy text", originalText);
        try
        {
            ParseDepth++;
            if (ParseDepth >= 100 && originalText != "{{Recursion error}}")
            {
                return ParseFancyText("{{Recursion error}}", "");
            }
            string text = AutoTranslateFancyText(originalText.ApplyBaseColor(baseColor));
            string[] lines = text.Replace('\r', ' ').Replace("^q", "\"").SplitFast('\n');
            RenderableTextLine[] outLines = new RenderableTextLine[lines.Length];
            RenderableTextPart currentPart = new() { Font = FontDefault };
            for (int i = 0; i < lines.Length; i++)
            {
                string line = lines[i];
                List<RenderableTextPart> parts = new(line.CountCharacter('^') + 1);
                int start = 0;
                float X = 0;
                for (int x = 0; x < line.Length; x++)
                {
                    if ((line[x] == '^' && x + 1 < line.Length && (IsFormatSymbol(line[x + 1]) || line[x + 1] == '[')) || (x + 1 == line.Length))
                    {
                        string subLine = line.Substring(start, (x - start) + ((x + 1 < line.Length) ? 0 : 1));
                        start = x + 2;
                        x++;
                        if (subLine.Length > 0)
                        {
                            RenderableTextPart addedPart = currentPart.Clone();
                            addedPart.Font.RecognizeCharacters(subLine);
                            addedPart.Text = subLine;
                            addedPart.Width = addedPart.Font.MeasureString(addedPart.Text);
                            X += addedPart.Width;
                            parts.Add(addedPart);
                        }
                        if (x < line.Length)
                        {
                            switch (line[x])
                            {
                                case '[':
                                    {
                                        x++;
                                        int c = 0;
                                        int xStart = x;
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
                                            x++;
                                        }
                                        string subText = line[xStart..x];
                                        RenderableTextPart addedPart = currentPart.Clone();
                                        if (x == line.Length)
                                        {
                                            addedPart.Text = "^[" + subText.ToString();
                                            addedPart.Highlight = true;
                                            addedPart.HighlightColor = Color4F.Red;
                                        }
                                        else
                                        {
                                            string subTextLow = subText.ToLowerFast();
                                            if (subTextLow == "lb")
                                            {
                                                addedPart.Text = "[";
                                            }
                                            else if (subTextLow == "rb")
                                            {
                                                addedPart.Text = "]";
                                            }
                                            else
                                            {
                                                (string prefix, string input) = subText.BeforeAndAfter('=');
                                                if (Engine.TextAdvancedFormatters.TryGetValue(prefix.ToLowerFast(), out Action<string, FontSet, RenderableTextPart, RenderableTextPart> formatter))
                                                {
                                                    formatter(input, this, currentPart, addedPart);
                                                }
                                                else
                                                {
                                                    addedPart.Text = subText;
                                                    addedPart.Highlight = true;
                                                    addedPart.HighlightColor = Color4F.Red;
                                                }
                                            }
                                        }
                                        if (addedPart.Text.Length > 0)
                                        {
                                            addedPart.Font.RecognizeCharacters(addedPart.Text);
                                            addedPart.Width = addedPart.Font.MeasureString(addedPart.Text);
                                            X += addedPart.Width;
                                            parts.Add(addedPart);
                                        }
                                        start = x + 1;
                                    }
                                    break;
                                case '1': currentPart.TextColor = ColorFor(1, currentPart.TextColor.A); break;
                                case '!': currentPart.TextColor = ColorFor(11, currentPart.TextColor.A); break;
                                case '2': currentPart.TextColor = ColorFor(2, currentPart.TextColor.A); break;
                                case '@': currentPart.TextColor = ColorFor(12, currentPart.TextColor.A); break;
                                case '3': currentPart.TextColor = ColorFor(3, currentPart.TextColor.A); break;
                                case '#': currentPart.TextColor = ColorFor(13, currentPart.TextColor.A); break;
                                case '4': currentPart.TextColor = ColorFor(4, currentPart.TextColor.A); break;
                                case '$': currentPart.TextColor = ColorFor(14, currentPart.TextColor.A); break;
                                case '5': currentPart.TextColor = ColorFor(5, currentPart.TextColor.A); break;
                                case '%': currentPart.TextColor = ColorFor(15, currentPart.TextColor.A); break;
                                case '6': currentPart.TextColor = ColorFor(6, currentPart.TextColor.A); break;
                                case '-': currentPart.TextColor = ColorFor(16, currentPart.TextColor.A); break;
                                case '7': currentPart.TextColor = ColorFor(7, currentPart.TextColor.A); break;
                                case '&': currentPart.TextColor = ColorFor(17, currentPart.TextColor.A); break;
                                case '8': currentPart.TextColor = ColorFor(8, currentPart.TextColor.A); break;
                                case '*': currentPart.TextColor = ColorFor(18, currentPart.TextColor.A); break;
                                case '9': currentPart.TextColor = ColorFor(9, currentPart.TextColor.A); break;
                                case '(': currentPart.TextColor = ColorFor(19, currentPart.TextColor.A); break;
                                case '0': currentPart.TextColor = ColorFor(20, currentPart.TextColor.A); break;
                                case ')': currentPart.TextColor = ColorFor(20, currentPart.TextColor.A); break;
                                case 'a': currentPart.TextColor = ColorFor(10, currentPart.TextColor.A); break;
                                case 'A': currentPart.TextColor = ColorFor(21, currentPart.TextColor.A); break;
                                case 'i':
                                    {
                                        currentPart.Italic = true;
                                        currentPart.SetFontFrom(this);
                                    }
                                    break;
                                case 'b':
                                    {
                                        currentPart.Bold = true;
                                        currentPart.SetFontFrom(this);
                                    }
                                    break;
                                case 'u': currentPart.UnderlineColor = currentPart.TextColor; currentPart.Underline = true; break;
                                case 's': currentPart.StrikeColor = currentPart.TextColor; currentPart.Strike = true; break;
                                case 'h': currentPart.HighlightColor = currentPart.TextColor; currentPart.Highlight = true; break;
                                case 'e': currentPart.EmphasisColor = currentPart.TextColor; currentPart.Emphasis = true; break;
                                case 'O': currentPart.OverlineColor = currentPart.TextColor; currentPart.Overline = true; break;
                                case 't': currentPart.TextColor = new Color4F(currentPart.TextColor.RGB, 0.5f); break;
                                case 'T': currentPart.TextColor = new Color4F(currentPart.TextColor.RGB, 0.25f); break;
                                case 'o': currentPart.TextColor = new Color4F(currentPart.TextColor.RGB, 1f); break;
                                case 'S':
                                    if (!currentPart.SuperScript)
                                    {
                                        if (currentPart.SubScript)
                                        {
                                            currentPart.SubScript = false;
                                        }
                                        currentPart.SuperScript = true;
                                        currentPart.SetFontFrom(this);
                                    }
                                    break;
                                case 'l':
                                    if (!currentPart.SubScript)
                                    {
                                        if (currentPart.SuperScript)
                                        {
                                            currentPart.SuperScript = false;
                                        }
                                        currentPart.SubScript = true;
                                        currentPart.SetFontFrom(this);
                                    }
                                    break;
                                case 'd': currentPart.Shadow = true; break;
                                case 'j': currentPart.Jello = true; break;
                                case 'U': currentPart.Unreadable = true; break;
                                case 'R': currentPart.Random = true; break;
                                case 'p': currentPart.PseudoRandom = true; break;
                                case 'f': currentPart.Flip = true; break;
                                case 'n':
                                    break;
                                case 'r':
                                    {
                                        currentPart = new RenderableTextPart() { Font = FontDefault, TextColor = currentPart.TextColor };
                                        break;
                                    }
                                default:
                                    break;
                            }
                        }
                    }
                }
                outLines[i] = new RenderableTextLine([.. parts]);
            }
            RenderableText result = new([.. outLines]);
            FancyTextCache[(baseColor, originalText)] = result;
            return result;
        }
        finally
        {
            ParseDepth--;
        }
    }

    /// <summary>
    /// Fully renders fancy text.
    /// <para>Consider using <see cref="ParseFancyText(string, string)"/> to pre-parse the text into renderable format and cache the result.</para>
    /// </summary>
    /// <param name="text">The text to render.</param>
    /// <param name="position">The position on screen to render at.</param>
    /// <param name="maxY">Optional: The maximum Y value to keep drawing at (to prevent text from going past the end of a text-area).</param>
    /// <param name="transmod">Optional: Transparency modifier, from 0 to 1 (1 is opaque, lower is more transparent).</param>
    /// <param name="extraShadow">Optional: If set to true, will cause a drop shadow to be drawn behind all text (even if '^d' is flipped off).</param>
    /// <param name="baseColor">Optional: The 'base color', to be used when '^B' is used.</param>
    public void DrawFancyText(string text, Location position, int maxY = int.MaxValue, float transmod = 1, bool extraShadow = false, string baseColor = "^r^7")
    {
        DrawFancyText(ParseFancyText(text, baseColor), position, maxY, transmod, extraShadow);
    }

    private static Color4F TransModify(Color4F color, float transMod)
    {
        return new Color4F(color.RGB, color.A * transMod);
    }

    /// <summary>
    /// Fully renders fancy text.
    /// <para>Consider using <see cref="SplitAppropriately(RenderableText, int)"/> to split the input for a maximum width.</para>
    /// </summary>
    /// <param name="text">The text to render.</param>
    /// <param name="position">The position on screen to render at.</param>
    /// <param name="maxY">Optional: The maximum Y value to keep drawing at (to prevent text from going past the end of a text-area).</param>
    /// <param name="transmod">Optional: Transparency modifier, from 0 to 1 (1 is opaque, lower is more transparent).</param>
    /// <param name="extraShadow">Optional: If set to true, will cause a drop shadow to be drawn behind all text (even if '^d' is flipped off).</param>
    public void DrawFancyText(RenderableText text, Location position, int maxY = int.MaxValue, float transmod = 1, bool extraShadow = false)
    {
        if (text.Lines is null)
        {
            return;
        }
        using var _push = StackNoteHelper.UsePush("FontSet - Draw fancy text", text);
        GraphicsUtil.CheckError("FontSet - Render - PreParts");
        float lineY = (float)position.Y;
        for (int i = 0; i < text.Lines.Length; i++)
        {
            RenderableTextLine line = text.Lines[i];
            float X = (float)position.X;
            foreach (RenderableTextPart part in line.Parts)
            {
                float Y = lineY;
                if (part.SuperScript)
                {
                    Y += part.Font.Height * 0.25f;
                }
                else if (part.SubScript)
                {
                    Y += part.Font.Height;
                }
                if (Y >= -part.Font.Height && Y - part.Font.Height <= maxY)
                {
                    if (part.Highlight)
                    {
                        DrawRectangle(X, lineY, part.Width, Height, TransModify(part.HighlightColor, transmod), ReusableTextVBO);
                    }
                    if (part.Underline)
                    {
                        DrawRectangle(X, Y + (part.Font.Height * 4f / 5f), part.Width, 2, TransModify(part.UnderlineColor, transmod), ReusableTextVBO);
                    }
                    if (part.Overline)
                    {
                        DrawRectangle(X, Y + 2f, part.Width, 2, TransModify(part.OverlineColor, transmod), ReusableTextVBO);
                    }
                    GraphicsUtil.CheckError("FontSet - Render - Part - Boxes", line);
                    if (extraShadow)
                    {
                        foreach (Point point in ShadowPoints)
                        {
                            part.Font.DrawString(part.Text, X + point.X, Y + point.Y, ColorFor(0, part.TextColor.A * 0.5f * transmod), ReusableTextVBO, part.Flip);
                        }
                    }
                    if (part.Shadow)
                    {
                        foreach (Point point in ShadowPoints)
                        {
                            part.Font.DrawString(part.Text, X + point.X, Y + point.Y, ColorFor(0, part.TextColor.A * 0.5f * transmod), ReusableTextVBO, part.Flip);
                        }
                        foreach (Point point in BetterShadowPoints)
                        {
                            part.Font.DrawString(part.Text, X + point.X, Y + point.Y, ColorFor(0, part.TextColor.A * 0.25f * transmod), ReusableTextVBO, part.Flip);
                        }
                    }
                    if (part.Emphasis)
                    {
                        foreach (Point point in EmphasisPoints)
                        {
                            part.Font.DrawString(part.Text, X + point.X, Y + point.Y, TransModify(part.EmphasisColor, transmod), ReusableTextVBO, part.Flip);
                        }
                        foreach (Point point in BetterEmphasisPoints)
                        {
                            part.Font.DrawString(part.Text, X + point.X, Y + point.Y, TransModify(part.EmphasisColor, transmod), ReusableTextVBO, part.Flip);
                        }
                    }
                    GraphicsUtil.CheckError("FontSet - Render - Part - Wrap Strings", line);
                    RenderBaseText(ReusableTextVBO, X, Y, part, transmod);
                    GraphicsUtil.CheckError("FontSet - Render - Part - Text", line);
                    if (part.Strike)
                    {
                        DrawRectangle(X, Y + (part.Font.Height / 2), part.Width, 2, TransModify(part.StrikeColor, transmod), ReusableTextVBO);
                    }
                    X += part.Width;
                    GraphicsUtil.CheckError("FontSet - Render - Part - Strike", line);
                }
            }
            lineY += Height;
        }
        GraphicsUtil.CheckError("FontSet - Render - PostParts");
        Engine.GLFonts.Shaders.TextCleanerShader.Bind();
        Matrix4 ortho = Engine.GetOrtho();
        GL.UniformMatrix4(1, false, ref ortho);
        GL.Uniform3(3, Vector3.One);
        GraphicsUtil.CheckError("FontSet - Render - PreBuild");
        ReusableTextVBO.Build();
        GraphicsUtil.CheckError("FontSet - Render - PostBuild");
        ReusableTextVBO.Render(Engine.GLFonts);
        if (Engine.FixToShader is null)
        {
            Engine.GLFonts.Shaders.ColorMult2D.Bind();
        }
        else
        {
            Engine.FixToShader.Bind();
        }
        GraphicsUtil.CheckError("FontSet - Render - Post");
    }

    /// <summary>The <see cref="TextVBOBuilder"/> that's reused for text rendering.</summary>
    public TextVBOBuilder ReusableTextVBO = new();

    /// <summary>Grabs a string containing only formats/colors from the string containing text.</summary>
    /// <param name="input">The input string.</param>
    /// <returns>The color set.</returns>
    public static string GrabAllFormats(string input)
    {
        StringBuilder res = new();
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

    /// <summary>Escapes fancy text to render as plain text.</summary>
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
    /// <param name="transMod">Transparency modifier.</param>
    /// <param name="part">The text to render.</param>
    public float RenderBaseText(TextVBOBuilder vbo, float X, float Y, RenderableTextPart part, float transMod)
    {
        if (part.Unreadable || part.PseudoRandom || part.Random || part.Jello)
        {
            float nX = 0;
            foreach (string txt in part.Font.SeparateEmojiAndSpecialChars(part.Text))
            {
                string chr = txt;
                Color4F color = TransModify(part.TextColor, transMod);
                if (part.Random)
                {
                    double ttime = Engine.GetGlobalTickTime();
                    double tempR = SimplexNoise.Generate((X + nX) / RAND_DIV + ttime * 0.4, Y / RAND_DIV);
                    double tempG = SimplexNoise.Generate((X + nX) / RAND_DIV + ttime * 0.4, Y / RAND_DIV + 7.6f);
                    double tempB = SimplexNoise.Generate((X + nX) / RAND_DIV + ttime * 0.4, Y / RAND_DIV + 18.42f);
                    color = new Color4F((float)tempR, (float)tempG, (float)tempB, transMod);
                }
                else if (part.PseudoRandom)
                {
                    color = ColorFor((chr[0] % (COLORS.Length - 1)) + 1, part.TextColor.A * transMod);
                }
                if (part.Unreadable)
                {
                    chr = ((char)Engine.RandomHelper.Next(33, 126)).ToString();
                }
                int iX = 0;
                int iY = 0;
                if (part.Jello)
                {
                    iX = Engine.RandomHelper.Next(-1, 1);
                    iY = Engine.RandomHelper.Next(-1, 1);
                }
                part.Font.DrawSingleCharacter(chr, X + iX + nX, Y + iY, vbo, color, part.Flip);
                nX += part.Font.RectForSymbol(txt).Width;
            }
            return nX;
        }
        else
        {
            return part.Font.DrawString(part.Text, X, Y, TransModify(part.TextColor, transMod), vbo, part.Flip);
        }
    }

    /// <summary>Measures several lines of text.</summary>
    /// <param name="text">The text.</param>
    /// <param name="bcolor">The base color.</param>
    /// <returns>The size.</returns>
    public Location MeasureFancyLinesOfText(string text, string bcolor = "^r^7")
    {
        string[] data = text.SplitFast('\n');
        float width = 0;
        for (int i = 0; i < data.Length; i++)
        {
            width = Math.Max(width, MeasureFancyText(data[i], bcolor));
        }
        return new Location(width, data.Length * Height, 0);
    }

    /// <summary>Helper to split strings fancy-text complex-text strings, marked with [] around sections and | to separate values.</summary>
    /// <param name="input">The original string.</param>
    /// <returns>The split string.</returns>
    public static List<string> CSplit(string input)
    {
        List<string> temp = [];
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

    /// <summary>Translates fancy text language inputs to raw strings.</summary>
    public string AutoTranslateFancyText(string text)
    {
        int index = text.IndexOf("^[lang=", StringComparison.Ordinal);
        if (index == -1)
        {
            return text;
        }
        StringBuilder sb = new();
        List<string> parts = [];
        int c = 0;
        int i;
        for (i = index + "^[lang=".Length; i < text.Length; i++)
        {
            if (text[i] == '[')
            {
                c++;
            }
            else if (text[i] == ']')
            {
                c--;
                if (c == -1)
                {
                    parts.Add(sb.ToString());
                    break;
                }
            }
            if (text[i] == '|' && c == 0)
            {
                parts.Add(sb.ToString());
                sb.Clear();
            }
            else
            {
                sb.Append(text[i]);
            }
        }
        if (i == text.Length)
        {
            return text;
        }
        string translated = AutoTranslateFancyText(Engine.GetLanguageHelper([.. parts]));
        return text[..index] + translated + AutoTranslateFancyText(text[(i + 1)..]);
    }

    /// <summary>
    /// Measures the width of fancy text.
    /// Consider instead using <see cref="ParseFancyText(string, string)"/>.
    /// </summary>
    /// <param name="text">The line of text to measure.</param>
    /// <param name="bcolor">The base color.</param>
    /// <returns>The horizontal width.</returns>
    public float MeasureFancyText(string text, string bcolor = "^r^7")
    {
        return ParseFancyText(text, bcolor).Width;
    }

    /// <summary>Splits some text at a maximum render width.</summary>
    /// <param name="text">The original (un-split) text.</param>
    /// <param name="baseColor">The base color.</param>
    /// <param name="maxX">The maximum width.</param>
    /// <returns>The split text.</returns>
    public RenderableText SplitAppropriately(string text, string baseColor, int maxX)
    {
        return SplitAppropriately(ParseFancyText(text), maxX);
    }

    /// <summary>Splits a renderable text line into individual words.</summary>
    /// <param name="line">The line to split.</param>
    // TODO: Split hyphenated text into words as well
    public static List<EditableTextLine> SplitLineIntoWords(RenderableTextLine line)
    {
        List<EditableTextLine> words = [new()];
        foreach (RenderableTextPart part in line.Parts)
        {
            string[] textWords = part.Text.Split(' ');
            for (int i = 0; i < textWords.Length; i++)
            {
                EditableTextLine lastWord = words.Last();
                if (textWords[i].Length != 0)
                {
                    lastWord.AddPart(part.CloneWithText(textWords[i]));
                }
                if (i < textWords.Length - 1)
                {
                    RenderableTextPart space = part.CloneWithText(" ");
                    EditableTextLine spaceWord = new([space], space.Width, 1, true);
                    if (lastWord.Parts.Count == 0)
                    {
                        words[^1] = spaceWord;
                    }
                    else
                    {
                        words.Add(spaceWord);
                    }
                    words.Add(new());
                }
            }
        }
        return words;
    }

    /// <summary>Splits a single word onto multiple lines if it exceeds a maximum render width.</summary>
    /// <param name="word">The word to split.</param>
    /// <param name="maxWidth">The maximum render width.</param>
    public static List<EditableTextLine> SplitWordAppropriately(EditableTextLine word, float maxWidth)
    {
        List<EditableTextLine> result = [new()];
        List<RenderableTextPart> parts = [.. word.Parts];
        for (int i = 0; i < parts.Count; i++)
        {
            EditableTextLine lastWord = result.Last();
            RenderableTextPart part = parts[i];
            if (lastWord.Width + part.Width <= maxWidth)
            {
                lastWord.AddPart(part);
                continue;
            }
            float lastWidth = 0;
            for (int j = 1; j <= part.Text.Length; j++)
            {
                float width = part.Font.MeasureString(part.Text[..j]);
                if (lastWord.Width + width > maxWidth)
                {
                    RenderableTextPart firstPart = part.Clone();
                    firstPart.Text = part.Text[..(j - 1)];
                    firstPart.Width = lastWidth;
                    RenderableTextPart secondPart = part.Clone();
                    secondPart.Text = part.Text[(j - 1)..];
                    secondPart.Width = part.Width - lastWidth;
                    lastWord.AddPart(firstPart);
                    result.Add(new());
                    parts.Insert(i + 1, secondPart);
                    break;
                }
                lastWidth = width;
            }
        }
        return result;
    }

    /// <summary>Splits a single line of renderable text at a maximum render width, automatically wrapping to new lines to fit the given boundaries.</summary>
    /// <param name="line">The line to split.</param>
    /// <param name="maxWidth">The maximum width a line can span before it is auto split, in pixels.</param>
    /// <param name="skippedIndices">A list of character indices ignored in the final result.</param>
    /// <returns>The multiple-line renderable result.</returns>
    public static RenderableText SplitLineAppropriately(RenderableTextLine line, float maxWidth, out List<int> skippedIndices)
    {
        skippedIndices = [];
        if (line.Width < maxWidth)
        {
            return new([line]);
        }
        int charIndex = 0;
        float totalWidth = 0;
        EditableTextLine currentLine = new(true);
        List<RenderableTextLine> lines = [];
        void BuildLine()
        {
            if (currentLine.Parts.Count > 0)
            {
                lines.Add(currentLine.ToRenderable());
            }
            currentLine = new(true);
        }
        List<EditableTextLine> words = SplitLineIntoWords(line);
        for (int i = 0; i < words.Count; i++)
        {
            EditableTextLine word = words[i];
            bool needsSplit = word.Width > maxWidth;
            bool needsNewLine = currentLine.Width + word.Width > maxWidth;
            if (needsSplit || needsNewLine)
            {
                BuildLine();
                if (word.IsWhitespace)
                {
                    skippedIndices.Add(++charIndex);
                    continue;
                }
            }
            if (needsSplit)
            {
                List<EditableTextLine> splitWords = SplitWordAppropriately(word, maxWidth);
                for (int j = 0; j < splitWords.Count; j++)
                {
                    if (j < splitWords.Count - 1)
                    {
                        lines.Add(splitWords[j].ToRenderable());
                    }
                }
                currentLine = splitWords[^1];
            }
            else
            {
                currentLine.AddLine(word);
                if (currentLine.Width > totalWidth)
                {
                    totalWidth = currentLine.Width;
                }
            }
            charIndex += word.Length;
        }
        BuildLine();
        return new RenderableText([.. lines]);
    }

    /// <summary>Splits some text at a maximum render width, automatically wrapping to new lines to fit the given boundaries.</summary>
    /// <param name="text">The original (un-split) text.</param>
    /// <param name="maxWidth">The maximum width a line can span before it is auto split, in pixels.</param>
    /// <returns>The split text.</returns>
    public static RenderableText SplitAppropriately(RenderableText text, int maxWidth)
    {
        List<RenderableTextLine> lines = [];
        foreach (RenderableTextLine line in text.Lines)
        {
            RenderableText splitLine = SplitLineAppropriately(line, maxWidth, out _);
            lines.AddRange(splitLine.Lines);
        }
        return new RenderableText([.. lines]);
    }

    /// <summary>Draws a rectangle to a <see cref="TextVBOBuilder"/> to be displayed on screen.</summary>
    /// <param name="X">The starting X.</param>
    /// <param name="Y">The starting Y.</param>
    /// <param name="width">The width.</param>
    /// <param name="height">The height.</param>
    /// <param name="color">The color to use.</param>
    /// <param name="vbo">The VBO to render with.</param>
    public void DrawRectangle(float X, float Y, float width, float height, Color4F color, TextVBOBuilder vbo)
    {
        TextVBOBuilder.AddQuad(X, Y, X + width, Y + height, 2f / GLFontEngine.DEFAULT_TEXTURE_SIZE_WIDTH, 2f / Engine.GLFonts.CurrentHeight, 4f / GLFontEngine.DEFAULT_TEXTURE_SIZE_WIDTH, 4f / Engine.GLFonts.CurrentHeight, color);
    }

    /// <summary>Matcher object to recognize color/format codes.</summary>
    public static AsciiMatcher FORMAT_CODES_MATCHER = new("0123456789" + "ab" + "def" + "hij" + "l" + "nopqrstu" + "AB" + "RSTUO" + "!@#$%&*()-");

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

    /// <summary>Determines if the specified object is equal to this <see cref="FontSet"/>.</summary>
    public override bool Equals(object obj)
    {
        return obj is FontSet set && Equals(set);
    }

    /// <summary>Determines if the specified <see cref="FontSet"/> is equal to this <see cref="FontSet"/>.</summary>
    public bool Equals(FontSet other)
    {
        return FontDefault.Equals(other.FontDefault);
    }

    /// <summary>Returns a hash code for this <see cref="FontSet"/>.</summary>
    public override int GetHashCode()
    {
        return FontDefault.GetHashCode();
    }
}
