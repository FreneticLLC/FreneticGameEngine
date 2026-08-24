//
// This file is part of the Frenetic Game Engine, created by Frenetic LLC.
// This code is Copyright (C) Frenetic LLC under the terms of a strict license.
// See README.md or LICENSE.txt in the FreneticGameEngine source root for the contents of the license.
// If neither of these are available, assume that neither you nor anyone other than the copyright holder
// hold any right or permission to use this software until such time as the official license is identified.
//

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FreneticUtilities.FreneticExtensions;
using FGECore.MathHelpers;
using FGEGraphics.GraphicsHelpers.Textures;
using OpenTK.Graphics.OpenGL4;
using SkiaSharp;

namespace FGEGraphics.GraphicsHelpers.FontSets;


/// <summary>A class for rendering text within OpenGL.</summary>
public class GLFont : IDisposable, IEquatable<GLFont>
{
    /// <summary>The base Font engine.</summary>
    public GLFontEngine Engine;

    /// <summary>The texture containing all character images.</summary>
    public Texture BaseTexture;

    /// <summary>Info about a single symbol.</summary>
    public struct SymbolInfo
    {
        /// <summary>Bounding rectangle on the character sheet.</summary>
        public Rectangle2F Rectangle;

        /// <summary>Amount to advance the cursor by (used for kerning).</summary>
        public float Advance;
    }

    /// <summary>A list of all symbol locations on the base texture.</summary>
    public Dictionary<string, SymbolInfo> SymbolLocations;

    /// <summary>A list of all character locations on the base texture.</summary>
    public Dictionary<char, SymbolInfo> CharacterLocations;

    /// <summary>The name of the font.</summary>
    public string Name;

    /// <summary>The size of the font.</summary>
    public int Size;

    /// <summary>Whether the font is bold.</summary>
    public bool Bold;

    /// <summary>Whether the font is italic.</summary>
    public bool Italic;

    /// <summary>The font used to create this GLFont.</summary>
    public SKFont Internal_Font;

    /// <summary>The backup font to use when the main font lacks a symbol.</summary>
    public SKFont BackupFont;

    /// <summary>The point size used to create this GLFont.</summary>
    public float PointSize;

    /// <summary>How tall a rendered symbol is.</summary>
    public int Height;

    /// <summary>Internal data for <see cref="GLFont"/>.</summary>
    public struct InternalData()
    {
        /// <summary>The size of <see cref="LowCodepointLocs"/>.</summary>
        public const int LOW_CODEPOINT_RANGE_CAP = 8192;

        // TODO: Internal struct
        /// <summary>Low code-point range symbol rectangle locations.</summary>
        public readonly SymbolInfo[] LowCodepointLocs = new SymbolInfo[LOW_CODEPOINT_RANGE_CAP];
    }

    /// <summary>Internal data for <see cref="GLFont"/>.</summary>
    public InternalData Internal = new();

    /// <summary>Constructs a GLFont.</summary>
    /// <param name="font">The font family (typeface) to use.</param>
    /// <param name="pointSize">The font size in points.</param>
    /// <param name="bold">Whether the font is bold.</param>
    /// <param name="italic">Whether the font is italic.</param>
    /// <param name="eng">The backing engine.</param>
    public GLFont(SKTypeface font, float pointSize, bool bold, bool italic, GLFontEngine eng)
    {
        Engine = eng;
        GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
        Name = font.FamilyName;
        Size = (int)(pointSize * eng.DPIScale);
        Bold = bold;
        Italic = italic;
        PointSize = pointSize;
        Internal_Font = MakeSkFont(font, pointSize, bold, italic);
        BackupFont = MakeSkFont(Engine.BackupFontFamily, pointSize, false, false);
        Height = (int)Math.Ceiling(Internal_Font.Spacing);
        SymbolLocations = new Dictionary<string, SymbolInfo>(InternalData.LOW_CODEPOINT_RANGE_CAP);
        CharacterLocations = new Dictionary<char, SymbolInfo>(InternalData.LOW_CODEPOINT_RANGE_CAP);
        RecognizeCharacters(Engine.CoreTextFileCharacters);
    }

    static SKFont MakeSkFont(SKTypeface typeface, float pointSize, bool bold, bool italic)
    {
        SKFont font = new(typeface, pointSize * (96f / 72f))
        {
            Edging = SKFontEdging.SubpixelAntialias,
            Hinting = SKFontHinting.Full,
            Subpixel = true
        };
        if (bold && typeface.FontWeight < (int)SKFontStyleWeight.SemiBold)
        {
            font.Embolden = true;
        }
        if (italic && typeface.FontSlant == SKFontStyleSlant.Upright)
        {
            font.SkewX = -0.25f;
        }
        return font;
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
            while ((needsAdding = AddAll(needsAdding)) is not null)
            {
                Engine.Expand();
            }
            Engine.UpdateTexture();
        }
    }

    /// <summary>Adds all the symbols to the GLFont mega texture.</summary>
    /// <param name="input">The list of symbols.</param>
    /// <returns>The list of symbols not able to added without expanding, if any.</returns>
    private IEnumerable<string> AddAll(IEnumerable<string> input)
    {
        using SKSurface surface = Engine.CreateAtlasSurface();
        SKCanvas canvas = surface.Canvas;
        using SKPaint paint = new() { Color = SKColors.White, IsAntialias = true, Style = SKPaintStyle.Fill };
        int X = Engine.CX;
        int Y = Engine.CY;
        Engine.CMinHeight = Math.Max(Height + 8, Engine.CMinHeight); // TODO: 8 -> ???
        int processed = 0;
        foreach (string inputSymbol in input)
        {
            bool isEmoji = inputSymbol.Length > 2 && inputSymbol.StartsWith(':') && inputSymbol.EndsWith(':');
            SKFont fnt = inputSymbol.Length == 1 ? Internal_Font : BackupFont;
            string chr = inputSymbol == "\t" ? "    " : inputSymbol;
            int nwidth = Height;
            float rawHeight = Height;
            SKRect bounds = default;
            if (!isEmoji)
            {
                float measured = fnt.MeasureText(chr, out bounds, paint);
                nwidth = (int)Math.Ceiling(measured);
                rawHeight = (-fnt.Metrics.Ascent + fnt.Metrics.Descent) + Math.Min(6, PointSize * 0.3f);
                if (fnt == Internal_Font && Italic)
                {
                    //nwidth += (int)(PointSize * 0.17);
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
                    List<string> toret = [];
                    return input.Skip(processed);
                }
            }
            if (isEmoji)
            {
                using SKBitmap bmp = Engine.Textures.LoadBitmapForTexture($"emoji/{inputSymbol[1..^1]}", nwidth);
                canvas.DrawBitmap(bmp, SKRect.Create(X, Y, nwidth, nwidth), new SKSamplingOptions(SKFilterMode.Linear), null);
            }
            else
            {
                canvas.DrawText(chr, X, Y - fnt.Metrics.Ascent, SKTextAlign.Left, fnt, paint);
            }
            processed++;
            Rectangle2F rect = new(X, Y, bounds.Right, rawHeight);
            SymbolInfo info = new() { Rectangle = rect, Advance = nwidth };
            SymbolLocations[inputSymbol] = info;
            if (chr.Length == 1)
            {
                CharacterLocations[inputSymbol[0]] = info;
                if (chr[0] < InternalData.LOW_CODEPOINT_RANGE_CAP)
                {
                    Internal.LowCodepointLocs[inputSymbol[0]] = info;
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

    /// <summary>Gets the location of a symbol.</summary>
    /// <param name="symbol">The symbol to find.</param>
    /// <returns>A rectangle containing the precise location of a symbol.</returns>
    public SymbolInfo RectForSymbol(string symbol)
    {
        if (symbol.Length == 1 && symbol[0] < InternalData.LOW_CODEPOINT_RANGE_CAP)
        {
            return Internal.LowCodepointLocs[symbol[0]];
        }
        if (SymbolLocations.TryGetValue(symbol, out SymbolInfo rect))
        {
            return rect;
        }
        return Internal.LowCodepointLocs['?'];
    }

    /// <summary>Gets the location of a symbol.</summary>
    /// <param name="symbol">The symbol to find.</param>
    /// <returns>A rectangle containing the precise location of a symbol.</returns>
    public SymbolInfo RectForSymbol(char symbol)
    {
        if (symbol < InternalData.LOW_CODEPOINT_RANGE_CAP)
        {
            return Internal.LowCodepointLocs[symbol];
        }
        if (CharacterLocations.TryGetValue(symbol, out SymbolInfo rect))
        {
            return rect;
        }
        return Internal.LowCodepointLocs['?'];
    }

    /// <summary>Draws a single symbol at a specified location.</summary>
    /// <param name="symbol">The symbol to draw.</param>
    /// <param name="X">The X location to draw it at.</param>
    /// <param name="Y">The Y location to draw it at.</param>
    /// <param name="vbo">The VBO to render with.</param>
    /// <param name="color">The color of the character.</param>
    /// <param name="flip">Whether to flip the character.</param>
    /// <returns>The length of the character in pixels.</returns>
    public float DrawSingleCharacter(string symbol, float X, float Y, TextVBOBuilder vbo, Color4F color, bool flip)
    {
        SymbolInfo info = RectForSymbol(symbol);
        Rectangle2F rec = info.Rectangle;
        TextVBOBuilder.AddQuad(X, Y, X + rec.Width, Y + rec.Height, rec.X / GLFontEngine.DEFAULT_TEXTURE_SIZE_WIDTH, (flip ? rec.Y + rec.Height : rec.Y) / Engine.CurrentHeight,
            (rec.X + rec.Width) / GLFontEngine.DEFAULT_TEXTURE_SIZE_WIDTH, (flip ? rec.Y : rec.Y + rec.Height) / Engine.CurrentHeight, color);
        return info.Advance;
    }

    /// <summary>Draws a single character at a specified location.</summary>
    /// <param name="character">The character to draw.</param>
    /// <param name="X">The X location to draw it at.</param>
    /// <param name="Y">The Y location to draw it at.</param>
    /// <param name="vbo">The VBO to render with.</param>
    /// <param name="color">The color of the character.</param>
    /// <param name="flip">Whether to flip the character.</param>
    /// <returns>The length of the character in pixels.</returns>
    public float DrawSingleCharacter(char character, float X, float Y, TextVBOBuilder vbo, Color4F color, bool flip)
    {
        SymbolInfo info = RectForSymbol(character);
        Rectangle2F rec = info.Rectangle;
        TextVBOBuilder.AddQuad(X, Y, X + rec.Width, Y + rec.Height, rec.X / GLFontEngine.DEFAULT_TEXTURE_SIZE_WIDTH, (flip ? rec.Y + rec.Height : rec.Y) / Engine.CurrentHeight,
            (rec.X + rec.Width) / GLFontEngine.DEFAULT_TEXTURE_SIZE_WIDTH, (flip ? rec.Y : rec.Y + rec.Height) / Engine.CurrentHeight, color);
        return info.Advance;
    }

    /// <summary>Draws a string at a specified location.</summary>
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
                X += RectForSymbol(symbol).Advance;
            }
        }
        else
        {
            foreach (char c in text)
            {
                X += RectForSymbol(c).Advance;
            }
        }
        return X;
    }

    /// <summary>Already-tested emoji names, with a boolean indicating whether they are valid.</summary>
    public Dictionary<string, bool> TestedEmoji = [];

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

    /// <summary>Determines if the specified object is equal to this <see cref="GLFont"/>.</summary>
    public override bool Equals(object obj)
    {
        return obj is GLFont font && Equals(font);
    }

    /// <summary>Determines if the specified <see cref="GLFont"/> is equal to this <see cref="GLFont"/>.</summary>
    public bool Equals(GLFont other)
    {
        return Name == other.Name &&
               Size == other.Size &&
               Bold == other.Bold &&
               Italic == other.Italic;
    }

    /// <summary>Returns a hash code for this <see cref="GLFont"/>.</summary>
    public override int GetHashCode()
    {
        return HashCode.Combine(Name, Size, Bold, Italic);
    }
}
