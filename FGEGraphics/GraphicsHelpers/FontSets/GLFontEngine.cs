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
using FreneticUtilities.FreneticExtensions;
using FGECore.CoreSystems;
using FGECore.FileSystems;
using FGEGraphics.GraphicsHelpers.Shaders;
using FGEGraphics.GraphicsHelpers.Textures;
using OpenTK.Graphics.OpenGL4;
using SkiaSharp;

namespace FGEGraphics.GraphicsHelpers.FontSets;

/// <summary>
/// Handles rendering of fonts.
/// Most users should not interact with this directly. Instead, use <see cref="FontSetEngine"/>.
/// </summary>
/// <param name="teng">The texture system.</param>
/// <param name="sengine">The shader system.</param>
public class GLFontEngine(TextureEngine teng, ShaderEngine sengine) : IDisposable
{
    /// <summary>The texture system.</summary>
    public TextureEngine Textures = teng;

    /// <summary>The shader system.</summary>
    public ShaderEngine Shaders = sengine;

    /// <summary>The default font.</summary>
    public GLFont Standard;

    /// <summary>A full list of loaded GLFonts.</summary>
    public List<GLFont> Fonts;

    /// <summary>Set this to modify the DPI scaling (Particularly if the user screen has a non-100% DPI).</summary>
    public float DPIScale = 1f;

    /// <summary>The default width of the GLFont mega texture.</summary>
    public const int DEFAULT_TEXTURE_SIZE_WIDTH = 2048;

    /// <summary>The default height of the GLFont mega texture.</summary>
    public const int DEFAULT_TEXTURE_SIZE_HEIGHT = 2048;

    /// <summary>Expands the CPU-Side GLFont mega texture. Does not update to the GPU.</summary>
    public void Expand()
    {
        CurrentHeight *= 2;
        SKBitmap bmp2 = new(DEFAULT_TEXTURE_SIZE_WIDTH, CurrentHeight, SKColorType.Bgra8888, SKAlphaType.Opaque);
        using (SKSurface surface = CreateAtlasSurface(bmp2))
        {
            SKCanvas canvas = surface.Canvas;
            canvas.Clear(SKColors.Black);
            canvas.DrawBitmap(CurrentBMP, 0, 0, new SKSamplingOptions(SKFilterMode.Nearest), null);
        }
        CurrentBMP.Dispose();
        CurrentBMP = bmp2;
    }

    /// <summary>The current height of the GLFont mega texture.</summary>
    public int CurrentHeight = DEFAULT_TEXTURE_SIZE_HEIGHT;

    /// <summary>The currently used CPU-Side GLFont mega texture.</summary>
    public SKBitmap CurrentBMP;

    static readonly SKSurfaceProperties AtlasSurfaceProps = new(SKSurfacePropsFlags.None, SKPixelGeometry.RgbHorizontal);

    /// <summary>Creates a canvas surface for the font atlas.</summary>
    public SKSurface CreateAtlasSurface() => CreateAtlasSurface(CurrentBMP);

    /// <summary>Creates a canvas surface for a font atlas bitmap.</summary>
    public static SKSurface CreateAtlasSurface(SKBitmap bmp) => SKSurface.Create(bmp.Info, bmp.GetPixels(), bmp.RowBytes, AtlasSurfaceProps);

    /// <summary>The GPU-Side mega texture.</summary>
    public GraphicsUtil.TrackedTexture TextureMain;

    /// <summary>The current X coordinate in the GLFont mega texture.</summary>
    public int CX = 26;

    /// <summary>The current Y coordinate in the GLFont mega texture.</summary>
    public int CY = 6;

    /// <summary>The current minimum height of the GLFont mega texture.</summary>
    public int CMinHeight = 20;

    /// <summary>Update the CPU-Side mega texture onto the GPU.</summary>
    public void UpdateTexture()
    {
        TextureMain?.Dispose();
        TextureMain = new("GLFontEngine_TextureMain", TextureTarget.Texture2D);
        GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba8, DEFAULT_TEXTURE_SIZE_WIDTH, CurrentHeight, 0, OpenTK.Graphics.OpenGL4.PixelFormat.Bgra, PixelType.UnsignedByte, CurrentBMP.GetPixels());
        GraphicsUtil.TexParamLinearClamp();
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureCompareMode, (int)TextureCompareMode.CompareRefToTexture);
        GraphicsUtil.BindTexture(TextureTarget.Texture2D, 0);
    }

    /// <summary>Main backing font (typeface) for internal prerendering.</summary>
    public SKTypeface CoreFontFamily;

    /// <summary>The backup font that contains emojis, etc.</summary>
    public SKTypeface BackupFontFamily;

    /// <summary>The backing file system.</summary>
    public FileEngine Files;

    /// <summary>The core font name to use that has a .ttf file.</summary>
    public string CoreFontPreference = "sourcecodepro";

    /// <summary>Prepares the font system.</summary>
    /// <param name="files">The backing file system.</param>
    public void Init(FileEngine files)
    {
        Files = files;
        if (Fonts != null)
        {
            for (int i = 0; i < Fonts.Count; i++)
            {
                Fonts[i].Remove();
                i--;
            }
        }
        // Generate the texture
        CurrentBMP = new SKBitmap(DEFAULT_TEXTURE_SIZE_WIDTH, DEFAULT_TEXTURE_SIZE_HEIGHT, SKColorType.Bgra8888, SKAlphaType.Opaque);
        using (SKSurface surface = CreateAtlasSurface(CurrentBMP))
        {
            SKCanvas canvas = surface.Canvas;
            canvas.Clear(SKColors.Black);
            canvas.DrawRect(SKRect.Create(0, 0, 20, 20), new SKPaint() { Color = SKColors.White, Style = SKPaintStyle.Fill });
        }
        // Load other stuff
        LoadTextFile();
        Fonts = [];
        // Choose a default font.
        SKFontManager fontManager = SKFontManager.Default;
        string[] families = fontManager.GetFontFamilies();
        SKTypeface family = SKTypeface.Default;
        int family_priority = 0;
        for (int i = 0; i < families.Length; i++)
        {
            string familyName = families[i].ToLowerFast();
            if (family_priority < 20 && familyName == "segoe ui emoji")
            {
                family = fontManager.MatchFamily(families[i]);
                family_priority = 20;
            }
            else if (family_priority < 10 && familyName == "segoe ui")
            {
                family = fontManager.MatchFamily(families[i]);
                family_priority = 10;
            }
            else if (family_priority < 5 && familyName == "arial")
            {
                family = fontManager.MatchFamily(families[i]);
                family_priority = 5;
            }
            else if (family_priority < 2 && familyName == "calibri")
            {
                family = fontManager.MatchFamily(families[i]);
                family_priority = 2;
            }
            else if (family_priority < 1 && familyName == "dejavu serif")
            {
                family = fontManager.MatchFamily(families[i]);
                family_priority = 1;
            }
        }
        BackupFontFamily = family;
        Logs.ClientInit($"Select backup font: {BackupFontFamily.FamilyName}");
        if (!string.IsNullOrWhiteSpace(CoreFontPreference))
        {
            try
            {
                // TODO: Move out of data directory, as we don't use the file handler at all anyway?
                CoreFontFamily = SKTypeface.FromFile($"{Environment.CurrentDirectory}/data/fonts/{CoreFontPreference}.ttf");
                family = CoreFontFamily;
                family_priority = 100;
            }
            catch (Exception ex)
            {
                Logs.Warning($"Loading {CoreFontPreference}: {ex}");
            }
        }
        Standard = new GLFont(family, 12, false, false, this);
        Fonts.Add(Standard);
        Logs.ClientInit($"Select main font: {family.FamilyName}");
        UpdateTexture();
    }

    /// <summary>The text file string to base letters on.</summary>
    public string CoreTextFileCharacters;

    /// <summary>Loads the character list file.</summary>
    public void LoadTextFile()
    {
        CoreTextFileCharacters = "";
        string[] datas;
        if (Files.TryReadFileText("info/characters.dat", out string charsFile))
        {
            datas = charsFile.Replace("\r", "").SplitFast('\n');
        }
        else
        {
            datas = [" abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890!@#$%^&*()_+-=~`[]{};:'\",./<>?\\|\x00A0"];
        }
        for (int i = 0; i < datas.Length; i++)
        {
            if (datas[i].Length > 0 && !datas[i].StartsWith("//"))
            {
                CoreTextFileCharacters += datas[i];
            }
        }
        string tempfile = "?";
        for (int i = 0; i < CoreTextFileCharacters.Length; i++)
        {
            if (!tempfile.Contains(CoreTextFileCharacters[i]))
            {
                tempfile += CoreTextFileCharacters[i].ToString();
            }
        }
        CoreTextFileCharacters = tempfile;
    }

    /// <summary>
    /// Gets the font matching the specified settings.
    /// If the relevant Font exists but is not yet loaded, will load it from file.
    /// </summary>
    /// <param name="name">The name of the font.</param>
    /// <param name="bold">Whether it's bold.</param>
    /// <param name="italic">Whether it's italic.</param>
    /// <param name="size">The font size.</param>
    /// <returns>A valid font object.</returns>
    public GLFont GetFont(string name, bool bold, bool italic, int size)
    {
        string namelow = name.ToLowerFast();
        for (int i = 0; i < Fonts.Count; i++)
        {
            if (Fonts[i].Name.ToLowerFast() == namelow && bold == Fonts[i].Bold && italic == Fonts[i].Italic && size == Fonts[i].Size)
            {
                return Fonts[i];
            }
        }
        GLFont Loaded = LoadFont(name, bold, italic, size);
        if (Loaded == null)
        {
            return Standard;
        }
        Fonts.Add(Loaded);
        return Loaded;
    }

    /// <summary>
    /// Loads a font matching the specified settings.
    /// <para>Note: Most users should not use this method. Instead, use <see cref="GetFont(string, bool, bool, int)"/>.</para>
    /// </summary>
    /// <param name="name">The name of the font.</param>
    /// <param name="bold">Whether it's bold.</param>
    /// <param name="italic">Whether it's italic.</param>
    /// <param name="size">The font size.</param>
    /// <returns>A valid font object, or null if there was no match.</returns>
    public GLFont LoadFont(string name, bool bold, bool italic, int size)
    {
        SKFontStyle style = bold && italic ? SKFontStyle.BoldItalic : bold ? SKFontStyle.Bold : italic ? SKFontStyle.Italic : SKFontStyle.Normal;
        SKTypeface typeface = null;
        if (CoreFontFamily is not null && CoreFontFamily.FamilyName.Equals(name, StringComparison.OrdinalIgnoreCase))
        {
            typeface = SKFontManager.Default.MatchFamily(CoreFontFamily.FamilyName, style);
            if (typeface is null || !typeface.FamilyName.Equals(CoreFontFamily.FamilyName, StringComparison.OrdinalIgnoreCase))
            {
                typeface = CoreFontFamily;
            }
        }
        typeface ??= SKTypeface.FromFamilyName(name, style);
        GLFont f = new(typeface, size / DPIScale, bold, italic, this);
        UpdateTexture();
        return f;
    }

    /// <summary>Dumb MS logic dispose method.</summary>
    /// <param name="disposing">Whether to dispose managed resources.</param>
    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            Standard.Dispose();
            CurrentBMP.Dispose();
            CoreFontFamily?.Dispose();
        }
    }

    /// <summary>Disposes the font engine.</summary>
    public void Dispose()
    {
        GC.SuppressFinalize(this);
        Dispose(true);
    }
}
