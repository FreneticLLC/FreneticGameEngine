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
using System.Drawing.Text;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FreneticUtilities.FreneticExtensions;
using FGECore.CoreSystems;
using FGECore.MathHelpers;
using FGEGraphics.GraphicsHelpers.Textures;
using OpenTK;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace FGEGraphics.GraphicsHelpers.FontSets
{

    /// <summary>A class for rendering text within OpenGL.</summary>
    public class GLFont : IDisposable
    {
        /// <summary>The base Font engine.</summary>
        public GLFontEngine Engine;

        /// <summary>The texture containing all character images.</summary>
        public Texture BaseTexture;

        /// <summary>A list of all symbol locations on the base texture.</summary>
        public Dictionary<string, RectangleF> SymbolLocations;

        /// <summary>A list of all character locations on the base texture.</summary>
        public Dictionary<char, RectangleF> CharacterLocations;

        /// <summary>The name of the font.</summary>
        public string Name;

        /// <summary>The size of the font.</summary>
        public int Size;

        /// <summary>Whether the font is bold.</summary>
        public bool Bold;

        /// <summary>Whether the font is italic.</summary>
        public bool Italic;

        /// <summary>The font used to create this GLFont.</summary>
        public Font Internal_Font;

        /// <summary>The backup font to use when the main font lacks a symbol.</summary>
        public Font BackupFont;

        /// <summary>How tall a rendered symbol is.</summary>
        public int Height;

        /// <summary>The size of <see cref="LowCodepointLocs"/>.</summary>
        public const int LOW_CODEPOINT_RANGE_CAP = 8192;

        /// <summary>Low code-point range symbol rectangle locations.</summary>
        private readonly RectangleF[] LowCodepointLocs = new RectangleF[LOW_CODEPOINT_RANGE_CAP];

        /// <summary>Constructs a GLFont.</summary>
        /// <param name="font">The CPU font to use.</param>
        /// <param name="eng">The backing engine.</param>
        public GLFont(Font font, GLFontEngine eng)
        {
            Engine = eng;
            GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
            Engine.Shaders.ColorMultShader.Bind();
            Name = font.Name;
            Size = (int)(font.Size * eng.DPIScale);
            Bold = font.Bold;
            Italic = font.Italic;
            Height = font.Height;
            SymbolLocations = new Dictionary<string, RectangleF>(LOW_CODEPOINT_RANGE_CAP);
            CharacterLocations = new Dictionary<char, RectangleF>(LOW_CODEPOINT_RANGE_CAP);
            Internal_Font = font;
            BackupFont = new Font(Engine.BackupFontFamily, font.SizeInPoints);
            AddAll(StringInfo.GetTextElementEnumerator(Engine.CoreTextFileCharacters).AsEnumerable<string>());
        }

        /// <summary>The format to render strings under.</summary>
        readonly static StringFormat RenderFormat;

        /// <summary>Prepares static helpers.</summary>
        static GLFont()
        {
            RenderFormat = new StringFormat(StringFormat.GenericTypographic);
            RenderFormat.FormatFlags |= StringFormatFlags.MeasureTrailingSpaces | StringFormatFlags.FitBlackBox | StringFormatFlags.NoWrap | StringFormatFlags.NoClip;
        }

        /// <summary>Returns 'true' if a <see cref="RecognizeCharacters(string)"/> call might be needed for the text (characters outside of quick-lookup range, or characters not already recognized). This call exists for opti reasons only.</summary>
        public bool AnyMightNeedAdding(string input)
        {
            if (StringHasHighOrderCharacters(input))
            {
                return true;
            }
            for (int i = 0; i < input.Length; i++)
            {
                if (!CharacterLocations.ContainsKey(input[i]))
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>Causes the system to recognize any characters in the string, adding them to the GLFont mega texture if needed.</summary>
        /// <param name="input">The text containing relevant characters.</param>
        public void RecognizeCharacters(string input)
        {
            if (!AnyMightNeedAdding(input))
            {
                return;
            }
            IEnumerable<string> needsAdding = SeparateEmojiAndSpecialChars(input).Distinct().Where(s => !SymbolLocations.ContainsKey(s));
            if (needsAdding.Any())
            {
                while ((needsAdding = AddAll(needsAdding)) != null)
                {
                    Engine.Expand();
                }
                Engine.UpdateTexture();
            }
        }

        /// <summary>Adds all the symbols to the GLFont mega texture.</summary>
        /// <param name="input">The list of symbols.</param>
        /// <returns>The list of symbols not able to added without expaninding, if any.</returns>
        private IEnumerable<string> AddAll(IEnumerable<string> input)
        {
            using Graphics gfx = Graphics.FromImage(Engine.CurrentBMP);
            gfx.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;
            gfx.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
            gfx.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.None;
            int X = Engine.CX;
            int Y = Engine.CY;
            Brush brush = new SolidBrush(Color.White);
            Engine.CMinHeight = Math.Max(Height + 8, Engine.CMinHeight); // TODO: 8 -> ???
            int processed = 0;
            foreach (string inputSymbol in input)
            {
                bool isEmoji = inputSymbol.Length > 2 && inputSymbol.StartsWith(":") && inputSymbol.EndsWith(":");
                Font fnt = (inputSymbol.Length == 1 ? Internal_Font : BackupFont);
                string chr = inputSymbol == "\t" ? "    " : inputSymbol;
                int nwidth = Height;
                float rawHeight = Height;
                if (!isEmoji)
                {
                    SizeF measured = gfx.MeasureString(chr, fnt, new PointF(0, 0), RenderFormat);
                    nwidth = (int)Math.Ceiling(measured.Width);
                    // TODO: These added values are hacks to compensate for font sizes not matching character sizes. A better measure method should be used instead.
                    rawHeight = measured.Height + fnt.SizeInPoints * 0.1f;
                    if (fnt.Italic)
                    {
                        nwidth += (int)(fnt.SizeInPoints * 0.17);
                    }
                }
                if (X + nwidth >= GLFontEngine.DEFAULT_TEXTURE_SIZE_WIDTH)
                {
                    Y += Engine.CMinHeight;
                    Engine.CMinHeight = Height + 8; // TODO: 8 -> ???
                    X = 6;
                    if (Y + Engine.CMinHeight > Engine.CurrentHeight)
                    {
                        Engine.CX = X;
                        Engine.CY = Y;
                        List<string> toret = new List<string>();
                        return input.Skip(processed);
                    }
                }
                if (isEmoji)
                {
                    Texture t = Engine.Textures.GetTexture("emoji/" + inputSymbol[1..^1]);
                    using Bitmap bmp = t.SaveToBMP();
                    gfx.DrawImage(bmp, new Rectangle(X, Y, nwidth, nwidth));
                }
                else
                {
                    gfx.DrawString(chr, fnt, brush, new PointF(X, Y), RenderFormat);
                }
                processed++;
                RectangleF rect = new RectangleF(X, Y, nwidth, rawHeight);
                SymbolLocations[inputSymbol] = rect;
                if (chr.Length == 1)
                {
                    CharacterLocations[inputSymbol[0]] = rect;
                    if (chr[0] < LOW_CODEPOINT_RANGE_CAP)
                    {
                        LowCodepointLocs[inputSymbol[0]] = rect;
                    }
                }
                X += nwidth + 8; // TODO: 8 -> ???
            }
            Engine.CX = X;
            Engine.CY = Y;
            return null;
        }

        /// <summary>Removes the GLFont.</summary>
        public void Remove()
        {
            Engine.Fonts.Remove(this);
        }

        /// <summary>
        /// Gets the location of a symbol.
        /// </summary>
        /// <param name="symbol">The symbol to find.</param>
        /// <returns>A rectangle containing the precise location of a symbol.</returns>
        public RectangleF RectForSymbol(string symbol)
        {
            if (symbol.Length == 1 && symbol[0] < LOW_CODEPOINT_RANGE_CAP)
            {
                return LowCodepointLocs[symbol[0]];
            }
            if (SymbolLocations.TryGetValue(symbol, out RectangleF rect))
            {
                return rect;
            }
            return LowCodepointLocs['?'];
        }

        /// <summary>
        /// Gets the location of a symbol.
        /// </summary>
        /// <param name="symbol">The symbol to find.</param>
        /// <returns>A rectangle containing the precise location of a symbol.</returns>
        public RectangleF RectForSymbol(char symbol)
        {
            if (symbol < LOW_CODEPOINT_RANGE_CAP)
            {
                return LowCodepointLocs[symbol];
            }
            if (CharacterLocations.TryGetValue(symbol, out RectangleF rect))
            {
                return rect;
            }
            return LowCodepointLocs['?'];
        }

        /// <summary>
        /// Draws a single symbol at a specified location.
        /// </summary>
        /// <param name="symbol">The symbol to draw.</param>
        /// <param name="X">The X location to draw it at.</param>
        /// <param name="Y">The Y location to draw it at.</param>
        /// <param name="vbo">The VBO to render with.</param>
        /// <param name="color">The color of the character.</param>
        /// <param name="flip">Whether to flip the character.</param>
        /// <returns>The length of the character in pixels.</returns>
        public float DrawSingleCharacter(string symbol, float X, float Y, TextVBOBuilder vbo, Color4F color, bool flip)
        {
            RectangleF rec = RectForSymbol(symbol);
            TextVBOBuilder.AddQuad(X, flip ? (Y + rec.Height) : Y, X + rec.Width, flip ? Y: (Y + rec.Height), rec.X / GLFontEngine.DEFAULT_TEXTURE_SIZE_WIDTH, rec.Y / GLFontEngine.DEFAULT_TEXTURE_SIZE_WIDTH,
                (rec.X + rec.Width) / Engine.CurrentHeight, (rec.Y + rec.Height) / Engine.CurrentHeight, color);
            return rec.Width;
        }

        /// <summary>
        /// Draws a single character at a specified location.
        /// </summary>
        /// <param name="character">The character to draw.</param>
        /// <param name="X">The X location to draw it at.</param>
        /// <param name="Y">The Y location to draw it at.</param>
        /// <param name="vbo">The VBO to render with.</param>
        /// <param name="color">The color of the character.</param>
        /// <param name="flip">Whether to flip the character.</param>
        /// <returns>The length of the character in pixels.</returns>
        public float DrawSingleCharacter(char character, float X, float Y, TextVBOBuilder vbo, Color4F color, bool flip)
        {
            RectangleF rec = RectForSymbol(character);
            TextVBOBuilder.AddQuad(X, flip ? (Y + rec.Height) : Y, X + rec.Width, flip ? Y : (Y + rec.Height), rec.X / GLFontEngine.DEFAULT_TEXTURE_SIZE_WIDTH, rec.Y / GLFontEngine.DEFAULT_TEXTURE_SIZE_WIDTH,
                (rec.X + rec.Width) / Engine.CurrentHeight, (rec.Y + rec.Height) / Engine.CurrentHeight, color);
            return rec.Width;
        }

        /// <summary>
        /// Draws a string at a specified location.
        /// </summary>
        /// <param name="str">The string to draw..</param>
        /// <param name="X">The X location to draw it at.</param>
        /// <param name="Y">The Y location to draw it at.</param>
        /// <param name="color">The color.</param>
        /// <param name="vbo">The VBO to render with.</param>
        /// <param name="flip">Whether to flip text upside-down.</param>
        /// <returns>The length of the string in pixels.</returns>
        public float DrawString(string str, float X, float Y, Color4F color, TextVBOBuilder vbo, bool flip = false)
        {
            float nX = 0;
            if (StringHasHighOrderCharacters(str))
            {
                foreach (string stri in SeparateEmojiAndSpecialChars(str))
                {
                    if (stri == "\n")
                    {
                        Y += Height;
                        nX = 0;
                    }
                    nX += DrawSingleCharacter(stri, X + nX, Y, vbo, color, flip);
                }
            }
            else
            {
                foreach (char c in str)
                {
                    if (c == '\n')
                    {
                        Y += Height;
                        nX = 0;
                    }
                    nX += DrawSingleCharacter(c, X + nX, Y, vbo, color, flip);
                }
            }
            return nX;
        }

        /// <summary>
        /// Measures the drawn length of a string.
        /// For monospaced fonts, this is (characterCount * width).
        /// This code assumes non-monospaced, and as such, grabs the width of each character before reading it.
        /// </summary>
        /// <param name="text">The string to measure.</param>
        /// <returns>The length of the string.</returns>
        public float MeasureString(string text)
        {
            float X = 0;
            // Opti: don't do advanced separation if not needed, as character based lookup is faster.
            if (StringHasHighOrderCharacters(text))
            {
                foreach (string symbol in SeparateEmojiAndSpecialChars(text))
                {
                    X += RectForSymbol(symbol).Width;
                }
            }
            else
            {
                foreach (char c in text)
                {
                    X += RectForSymbol(c).Width;
                }
            }
            return X;
        }

        /// <summary>Already-tested emoji names, with a boolean indicating whether they are valid.</summary>
        public Dictionary<string, bool> TestedEmoji = new Dictionary<string, bool>();

        /// <summary>Returns whether the string is an emoji name.</summary>
        /// <param name="str">The string.</param>
        /// <returns>Whether it's an emoji name.</returns>
        public bool IsEmojiName(string str)
        {
            if (TestedEmoji.TryGetValue(str, out bool result))
            {
                return result;
            }
            result = Engine.Files.FileExists($"textures/emoji/{str}.png");
            TestedEmoji[str] = result;
            return result;
        }
        
        /// <summary>Returns 'true' if the string contains any high-order characters that require multiple 'char' instances per symbol, such as emoji or obscure languages. This is mainly used for opti reasons.</summary>
        public static bool StringHasHighOrderCharacters(string text)
        {
            int colon = -1;
            for (int i = 0; i < text.Length; i++)
            {
                char c = text[i];
                if (c == ':')
                {
                    if (colon != -1)
                    {
                        return true;
                    }
                    colon = i;
                }
                else if (c == ' ' || c == '\n')
                {
                    colon = -1;
                }
                // Note: This range is "surrogate code points", which is how .NET 5.0 encodes multi-character symbols (per UTF-16 standard).
                if (c >= 0xD800 && c <= 0xDFFF)
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>Separates emoji and special characters from a complex string.</summary>
        /// <param name="input">The input string.</param>
        /// <returns>The enumerable of emojis, characters, and special characters.</returns>
        public IEnumerable<string> SeparateEmojiAndSpecialChars(string input)
        {
            int lstart = 0;
            for (int i = 0; i < input.Length; i++)
            {
                if (input[i] == ':')
                {
                    for (int x = i + 1; x < input.Length; x++)
                    {
                        if (input[x] == ' ')
                        {
                            break;
                        }
                        else if (input[x] == ':')
                        {
                            string split = input[(i + 1)..x];
                            if (!IsEmojiName(split))
                            {
                                break;
                            }
                            string pre_pieces = input[lstart..i];
                            foreach (string stx in StringInfo.GetTextElementEnumerator(pre_pieces).AsEnumerable<string>())
                            {
                                yield return stx;
                            }
                            yield return ":" + split + ":";
                            i = x;
                            lstart = x + 1;
                            break;
                        }
                    }
                }
            }
            string final_pieces = input[lstart..];
            foreach (string stx in StringInfo.GetTextElementEnumerator(final_pieces).AsEnumerable<string>())
            {
                yield return stx;
            }
        }

        /// <summary>Dumb MS logic dispose method.</summary>
        /// <param name="disposing">Whether to dispose managed resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                RenderFormat.Dispose();
                BackupFont.Dispose();
                Internal_Font.Dispose();
            }
        }

        /// <summary>Disposes the font instance.</summary>
        public void Dispose()
        {
            GC.SuppressFinalize(this);
            Dispose(true);
        }
    }
}
