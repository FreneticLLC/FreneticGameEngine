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
using System.Drawing;
using System.Drawing.Text;
using System.Globalization;
using OpenTK;
using OpenTK.Mathematics;
using OpenTK.Graphics.OpenGL4;
using FreneticUtilities.FreneticExtensions;
using FGEGraphics.GraphicsHelpers.Textures;

namespace FGEGraphics.GraphicsHelpers.FontSets
{

    /// <summary>
    /// A class for rendering text within OpenGL.
    /// </summary>
    public class GLFont : IDisposable
    {
        /// <summary>
        /// The base Font engine.
        /// </summary>
        public GLFontEngine Engine;

        /// <summary>
        /// The texture containing all character images.
        /// </summary>
        public Texture BaseTexture;

        /// <summary>
        /// A list of all character locations on the base texture.
        /// </summary>
        public Dictionary<string, RectangleF> CharacterLocations;

        /// <summary>
        /// The name of the font.
        /// </summary>
        public string Name;

        /// <summary>
        /// The size of the font.
        /// </summary>
        public int Size;

        /// <summary>
        /// Whether the font is bold.
        /// </summary>
        public bool Bold;

        /// <summary>
        /// Whether the font is italic.
        /// </summary>
        public bool Italic;

        /// <summary>
        /// The font used to create this GLFont.
        /// </summary>
        public Font Internal_Font;

        /// <summary>
        /// The backup font to use when the main font lacks a symbol.
        /// </summary>
        public Font BackupFont;

        /// <summary>
        /// How tall a rendered symbol is.
        /// </summary>
        public float Height;

        /// <summary>
        /// Constructs a GLFont.
        /// </summary>
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
            CharacterLocations = new Dictionary<string, RectangleF>(2048);
            Internal_Font = font;
            BackupFont = new Font(Engine.BackupFontFamily, font.SizeInPoints);
            AddAll(StringInfo.GetTextElementEnumerator(Engine.textfile).AsEnumerable<string>().ToList());
        }

        /// <summary>
        /// The format to render strings under.
        /// </summary>
        readonly static StringFormat RenderFormat;

        /// <summary>
        /// Prepares static helpers.
        /// </summary>
        static GLFont()
        {
            RenderFormat = new StringFormat(StringFormat.GenericTypographic);
            RenderFormat.FormatFlags |= StringFormatFlags.MeasureTrailingSpaces | StringFormatFlags.FitBlackBox | StringFormatFlags.NoWrap;
        }

        /// <summary>
        /// Causes the system to recognize any characters in the string, adding them to the GLFont mega texture if needed.
        /// </summary>
        /// <param name="inp">The text containing relevant characters.</param>
        public void RecognizeCharacters(string inp)
        {
            List<string> NeedsAdding = new List<string>();
            foreach (string str in SeparateEmojiAndSpecialChars(inp).Distinct())
            {
                if (!CharacterLocations.ContainsKey(str))
                {
                    NeedsAdding.Add(str);
                }
            }
            if (NeedsAdding.Count > 0)
            {
                List<string> toadd = NeedsAdding;
                while ((toadd = AddAll(toadd)) != null)
                {
                    Engine.Expand();
                }
                Engine.UpdateTexture();
            }
        }

        /// <summary>
        /// Adds all the symbols to the GLFont mega texture.
        /// </summary>
        /// <param name="inp">The list of symbols.</param>
        /// <returns>The list of symbols not able to added without expaninding, if any.</returns>
        private List<string> AddAll(List<string> inp) // TODO: Enumerable input?
        {
            using Graphics gfx = Graphics.FromImage(Engine.CurrentBMP);
            gfx.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;
            int X = Engine.CX;
            int Y = Engine.CY;
            Brush brush = new SolidBrush(Color.White);
            Engine.CMinHeight = Math.Max((int)Height + 8, Engine.CMinHeight); // TODO: 8 -> ???
            for (int i = 0; i < inp.Count; i++)
            {
                bool isEmoji = inp[i].Length > 2 && inp[i].StartsWith(":") && inp[i].EndsWith(":");
                Font fnt = (inp[i].Length == 1 ? Internal_Font : BackupFont);
                string chr = inp[i] == "\t" ? "    " : inp[i];
                float nwidth = Height;
                if (!isEmoji)
                {
                    nwidth = (float)Math.Ceiling(gfx.MeasureString(chr, fnt, new PointF(0, 0), RenderFormat).Width);
                    if (fnt.Italic)
                    {
                        nwidth += (int)(fnt.SizeInPoints * 0.17);
                    }
                }
                if (X + nwidth >= GLFontEngine.DEFAULT_TEXTURE_SIZE_WIDTH)
                {
                    Y += Engine.CMinHeight;
                    Engine.CMinHeight = (int)Height + 8; // TODO: 8 -> ???
                    X = 6;
                    if (Y + Engine.CMinHeight > Engine.CurrentHeight)
                    {
                        Engine.CX = X;
                        Engine.CY = Y;
                        List<string> toret = new List<string>();
                        for (int x = i; x < inp.Count; x++)
                        {
                            toret.Add(inp[x]);
                        }
                        return toret;
                    }
                }
                if (isEmoji)
                {
                    Texture t = Engine.Textures.GetTexture("emoji/" + inp[i][1..^1]);
                    using Bitmap bmp = t.SaveToBMP();
                    gfx.DrawImage(bmp, new Rectangle(X, Y, (int)nwidth, (int)nwidth));
                }
                else
                {
                    gfx.DrawString(chr, fnt, brush, new PointF(X, Y), RenderFormat);
                }
                RectangleF rect = new RectangleF(X, Y, nwidth, Height);
                CharacterLocations[inp[i]] = rect;
                if (chr.Length == 1 && chr[0] < 128)
                {
                    ASCIILocs[inp[i][0]] = rect;
                }
                X += (int)Math.Ceiling(nwidth + 8); // TODO: 8 -> ???
            }
            Engine.CX = X;
            Engine.CY = Y;
            return null;
        }

        /// <summary>
        /// Removes the GLFont.
        /// </summary>
        public void Remove()
        {
            Engine.Fonts.Remove(this);
        }

        /// <summary>
        /// ASCII range symbol rectangle locations.
        /// </summary>
        private readonly RectangleF[] ASCIILocs = new RectangleF[128];

        /// <summary>
        /// Gets the location of a symbol.
        /// </summary>
        /// <param name="symbol">The symbol to find.</param>
        /// <returns>A rectangle containing the precise location of a symbol.</returns>
        public RectangleF RectForSymbol(string symbol)
        {
            if (symbol.Length == 1 && symbol[0] < 128)
            {
                return ASCIILocs[symbol[0]];
            }
            if (CharacterLocations.TryGetValue(symbol, out RectangleF rect))
            {
                return rect;
            }
            return ASCIILocs['?'];
        }

        /// <summary>
        /// Draws a single symbol at a specified location.
        /// </summary>
        /// <param name="symbol">The symbol to draw..</param>
        /// <param name="X">The X location to draw it at.</param>
        /// <param name="Y">The Y location to draw it at.</param>
        /// <param name="vbo">The VBO to render with.</param>
        /// <param name="color">The color of the character.</param>
        /// <returns>The length of the character in pixels.</returns>
        public float DrawSingleCharacter(string symbol, float X, float Y, TextVBOBuilder vbo, Vector4 color)
        {
            RectangleF rec = RectForSymbol(symbol);
            TextVBOBuilder.AddQuad(X, Y, X + rec.Width, Y + rec.Height, rec.X / GLFontEngine.DEFAULT_TEXTURE_SIZE_WIDTH, rec.Y / GLFontEngine.DEFAULT_TEXTURE_SIZE_WIDTH,
                (rec.X + rec.Width) / Engine.CurrentHeight, (rec.Y + rec.Height) / Engine.CurrentHeight, color);
            return rec.Width;
        }

        /// <summary>
        /// Draws a single symbol at a specified location, flipped.
        /// </summary>
        /// <param name="symbol">The symbol to draw..</param>
        /// <param name="X">The X location to draw it at.</param>
        /// <param name="Y">The Y location to draw it at.</param>
        /// <param name="vbo">The VBO to render with.</param>
        /// <param name="color">The color.</param>
        /// <returns>The length of the character in pixels.</returns>
        public float DrawSingleCharacterFlipped(string symbol, float X, float Y, TextVBOBuilder vbo, Vector4 color)
        {
            RectangleF rec = RectForSymbol(symbol);
            TextVBOBuilder.AddQuad(X, Y, X + rec.Width, Y + rec.Height, rec.X / GLFontEngine.DEFAULT_TEXTURE_SIZE_WIDTH, rec.Y / GLFontEngine.DEFAULT_TEXTURE_SIZE_WIDTH,
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
        public float DrawString(string str, float X, float Y, Vector4 color, TextVBOBuilder vbo, bool flip = false)
        {
            IEnumerable<string> strs = SeparateEmojiAndSpecialChars(str);
            float nX = 0;
            if (flip)
            {
                foreach (string stri in strs)
                {
                    if (stri == "\n")
                    {
                        Y += Height;
                        nX = 0;
                    }
                    nX += DrawSingleCharacterFlipped(stri, X + nX, Y, vbo, color);
                }
            }
            else
            {
                foreach (string stri in strs)
                {
                    if (stri == "\n")
                    {
                        Y += Height;
                        nX = 0;
                    }
                    nX += DrawSingleCharacter(stri, X + nX, Y, vbo, color);
                }
            }
            return nX;
        }

        /// <summary>
        /// Measures the drawn length of a string.
        /// For monospaced fonts, this is (characterCount * width).
        /// This code assumes non-monospaced, and as such, grabs the width of each character before reading it.
        /// </summary>
        /// <param name="str">The string to measure.</param>
        /// <returns>The length of the string.</returns>
        public float MeasureString(string str)
        {
            float X = 0;
            foreach (string stx in SeparateEmojiAndSpecialChars(str))
            {
                X += RectForSymbol(stx).Width;
            }
            return X;
        }

        /// <summary>
        /// Definitely not valid emoji values.
        /// </summary>
        public HashSet<string> InvalidEmoji = new HashSet<string>();

        /// <summary>
        /// Returns whether the string is an emoji name.
        /// </summary>
        /// <param name="str">The string.</param>
        /// <returns>Whether it's an emoji name.</returns>
        public bool IsEmojiName(string str)
        {
            if (InvalidEmoji.Contains(str))
            {
                return false;
            }
            return Engine.Files.FileExists("textures/emoji/" + str + ".png");
        }

        /// <summary>
        /// Separates emoji and special characters from a complex string.
        /// </summary>
        /// <param name="inp">The input string.</param>
        /// <returns>The enumerable of emojis, characters, and special characters.</returns>
        public IEnumerable<string> SeparateEmojiAndSpecialChars(string inp)
        {
            int lstart = 0;
            for (int i = 0; i < inp.Length; i++)
            {
                if (inp[i] == ':')
                {
                    for (int x = i + 1; x < inp.Length; x++)
                    {
                        if (inp[x] == ' ')
                        {
                            break;
                        }
                        else if (inp[x] == ':')
                        {
                            string split = inp[(i + 1)..x];
                            if (!IsEmojiName(split))
                            {
                                InvalidEmoji.Add(split);
                                break;
                            }
                            string pre_pieces = inp[lstart..i];
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
            string final_pieces = inp[lstart..];
            foreach (string stx in StringInfo.GetTextElementEnumerator(final_pieces).AsEnumerable<string>())
            {
                yield return stx;
            }
        }

        /// <summary>
        /// Dumb MS logic dispose method.
        /// </summary>
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

        /// <summary>
        /// Disposes the font instance.
        /// </summary>
        public void Dispose()
        {
            GC.SuppressFinalize(this);
            Dispose(true);
        }
    }
}
