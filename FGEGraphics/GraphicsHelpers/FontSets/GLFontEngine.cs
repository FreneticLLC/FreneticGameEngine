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
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FreneticUtilities.FreneticExtensions;
using FGECore.CoreSystems;
using FGECore.FileSystems;
using FGEGraphics.GraphicsHelpers.Shaders;
using FGEGraphics.GraphicsHelpers.Textures;
using OpenTK.Graphics.OpenGL4;

namespace FGEGraphics.GraphicsHelpers.FontSets
{
    /// <summary>
    /// Handles rendering of fonts.
    /// Most users should not interact with this directly. Instead, use <see cref="FontSetEngine"/>.
    /// </summary>
    public class GLFontEngine : IDisposable
    {
        /// <summary>Constructs a GLFontEngine. Does not initialize.</summary>
        /// <param name="teng">The texture system.</param>
        /// <param name="sengine">The shader system.</param>
        public GLFontEngine(TextureEngine teng, ShaderEngine sengine)
        {
            Textures = teng;
            Shaders = sengine;
        }

        /// <summary>The texture system.</summary>
        public TextureEngine Textures;

        /// <summary>The shader system.</summary>
        public ShaderEngine Shaders;

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
            Bitmap bmp2 = new Bitmap(DEFAULT_TEXTURE_SIZE_WIDTH, CurrentHeight);
            using (Graphics gfx = Graphics.FromImage(bmp2))
            {
                gfx.Clear(Color.Transparent);
                gfx.DrawImage(CurrentBMP, new Point(0, 0));
            }
            CurrentBMP.Dispose();
            CurrentBMP = bmp2;
        }

        /// <summary>The current height of the GLFont mega texture.</summary>
        public int CurrentHeight = DEFAULT_TEXTURE_SIZE_HEIGHT;

        /// <summary>The currently used CPU-Side GLFont mega texture.</summary>
        public Bitmap CurrentBMP;

        /// <summary>The GPU-Side mega texture ID.</summary>
        public int TextureMain = -1;

        /// <summary>The current X coordinate in the GLFont mega texture.</summary>
        public int CX = 26;

        /// <summary>The current Y coordinate in the GLFont mega texture.</summary>
        public int CY = 6;

        /// <summary>The current minimum height of the GLFont mega texture.</summary>
        public int CMinHeight = 20;

        /// <summary>Update the CPU-Side mega texture onto the GPU.</summary>
        public void UpdateTexture()
        {
            if (TextureMain != -1)
            {
                GL.DeleteTexture(TextureMain);
            }
            TextureMain = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, TextureMain);
            BitmapData bmp_data = CurrentBMP.LockBits(new Rectangle(0, 0, DEFAULT_TEXTURE_SIZE_WIDTH, CurrentHeight), ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba8, DEFAULT_TEXTURE_SIZE_WIDTH, CurrentHeight, 0, OpenTK.Graphics.OpenGL4.PixelFormat.Bgra, PixelType.UnsignedByte, bmp_data.Scan0);
            CurrentBMP.UnlockBits(bmp_data);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureCompareMode, (int)TextureCompareMode.CompareRefToTexture);
            GL.BindTexture(TextureTarget.Texture2D, 0);
        }

        /// <summary>Keep this public and valid: if it gets released by the GC, the fonts it contains are lost for some reason!</summary>
        public PrivateFontCollection InternalFontCollection;

        /// <summary>The backup font that contains emojis, etc.</summary>
        public FontFamily BackupFontFamily;

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
            CurrentBMP = new Bitmap(DEFAULT_TEXTURE_SIZE_WIDTH, DEFAULT_TEXTURE_SIZE_HEIGHT);
            using (Graphics gfx = Graphics.FromImage(CurrentBMP))
            {
                gfx.Clear(Color.Transparent);
                gfx.FillRectangle(new SolidBrush(Color.White), new Rectangle(0, 0, 20, 20));
            }
            // Load other stuff
            LoadTextFile();
            Fonts = new List<GLFont>();
            // Choose a default font.
            FontFamily[] families = FontFamily.Families;
            FontFamily family = FontFamily.GenericMonospace;
            int family_priority = 0;
            for (int i = 0; i < families.Length; i++)
            {
                string familyName = families[i].Name.ToLowerFast();
                if (family_priority < 20 && familyName == "segoe ui emoji")
                {
                    family = families[i];
                    family_priority = 20;
                }
                else if (family_priority < 10 && familyName == "segoe ui")
                {
                    family = families[i];
                    family_priority = 10;
                }
                else if (family_priority < 5 && familyName == "arial")
                {
                    family = families[i];
                    family_priority = 5;
                }
                else if (family_priority < 2 && familyName == "calibri")
                {
                    family = families[i];
                    family_priority = 2;
                }
                else if (family_priority < 1 && familyName == "dejavu serif")
                {
                    family = families[i];
                    family_priority = 1;
                }
            }
            BackupFontFamily = family;
            SysConsole.Output(OutputType.INIT, $"Select backup font: {BackupFontFamily.Name}");
            if (!string.IsNullOrWhiteSpace(CoreFontPreference))
            {
                try
                {
                    InternalFontCollection = new PrivateFontCollection();
                    // TODO: Move out of data directory, as we don't use the file handler at all anyway?
                    InternalFontCollection.AddFontFile($"{Environment.CurrentDirectory}/data/fonts/{CoreFontPreference}.ttf");
                    family = InternalFontCollection.Families[0];
                    family_priority = 100;
                }
                catch (Exception ex)
                {
                    SysConsole.Output(OutputType.WARNING, $"Loading {CoreFontPreference}: {ex}");
                }
            }
            Font def = new Font(family, 12);
            Standard = new GLFont(def, this);
            Fonts.Add(Standard);
            SysConsole.Output(OutputType.INIT, $"Select main font: {family.Name}");
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
                datas = new string[] { " abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890!@#$%^&*()_+-=~`[]{};:'\",./<>?\\|\x00A0" };
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
            Font font = new Font(name, size / DPIScale, (bold ? FontStyle.Bold : 0) | (italic ? FontStyle.Italic : 0));
            GLFont f = new GLFont(font, this);
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
                if (InternalFontCollection != null)
                {
                    InternalFontCollection.Dispose();
                }
            }
        }

        /// <summary>Disposes the font engine.</summary>
        public void Dispose()
        {
            GC.SuppressFinalize(this);
            Dispose(true);
        }
    }
}
