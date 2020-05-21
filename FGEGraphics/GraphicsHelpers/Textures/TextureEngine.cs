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
using System.IO;
using System.Threading.Tasks;
using System.Drawing;
using System.Drawing.Imaging;
using OpenTK.Graphics.OpenGL4;
using FGECore;
using FGECore.CoreSystems;
using FGECore.MathHelpers;
using FGECore.FileSystems;
using FGECore.ConsoleHelpers;

namespace FGEGraphics.GraphicsHelpers.Textures
{
    /// <summary>
    /// The primary engine for textures.
    /// </summary>
    public class TextureEngine : IDisposable
    {
        /// <summary>
        /// Dumb MS logic dispose method.
        /// </summary>
        /// <param name="disposing">Whether to dispose managed resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                GenericGraphicsObject.Dispose();
                EmptyBitmap.Dispose();
            }
        }

        /// <summary>
        /// Disposes the window client.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
        }

        /// <summary>
        /// What texture widths/heights are allowed.
        /// </summary>
        public static int[] AcceptableWidths = new int[] { 2, 4, 8, 16, 32, 64, 128, 256, 512, 1024, 2048, 4096, 8192, 16384 };

        /// <summary>
        /// A full list of currently loaded textures.
        /// </summary>
        public Dictionary<string, Texture> LoadedTextures;

        /// <summary>
        /// A default white texture.
        /// </summary>
        public Texture White = null;

        /// <summary>
        /// A default clear texture.
        /// </summary>
        public Texture Clear = null;

        /// <summary>
        /// A default black texture.
        /// </summary>
        public Texture Black = null;

        /// <summary>
        /// A default normal plane texture.
        /// </summary>
        public Texture NormalDef = null;

        /// <summary>
        /// An empty bitmap, for regular use.
        /// </summary>
        public Bitmap EmptyBitmap = null;

        /// <summary>
        /// A single graphics object for regular use.
        /// </summary>
        public Graphics GenericGraphicsObject = null;

        /// <summary>
        /// The relevant file helper.
        /// </summary>
        public FileEngine Files;

        /// <summary>
        /// The relevant asset streaming helper.
        /// </summary>
        public AssetStreamingEngine AssetStreaming;

        /// <summary>
        /// The relevant scheduler.
        /// </summary>
        public Scheduler Schedule;

        /// <summary>
        /// Starts or restarts the texture system.
        /// </summary>
        /// <param name="files">The relevant file helper.</param>
        /// <param name="assetStreaming">The relevant asset streaming helper.</param>
        /// <param name="schedule">The relevant scheduler.</param>
        public void InitTextureSystem(FileEngine files, AssetStreamingEngine assetStreaming, Scheduler schedule)
        {
            Files = files;
            AssetStreaming = assetStreaming;
            Schedule = schedule;
            // Create a generic graphics object for later use
            EmptyBitmap = new Bitmap(1, 1);
            GenericGraphicsObject = Graphics.FromImage(EmptyBitmap);
            // Reset texture list
            LoadedTextures = new Dictionary<string, Texture>(256);
            // Pregenerate a few needed textures
            White = GenerateForColor(Color.White, "white");
            LoadedTextures.Add("white", White);
            Black = GenerateForColor(Color.Black, "black");
            LoadedTextures.Add("black", Black);
            Clear = GenerateForColor(Color.Transparent, "clear");
            LoadedTextures.Add("clear", Clear);
            NormalDef = GenerateForColor(Color.FromArgb(255, 127, 127, 255), "normal_def");
            LoadedTextures.Add("normal_def", NormalDef);
        }

        /// <summary>
        /// Clears away all current textures.
        /// </summary>
        public void Empty()
        {
            foreach (Texture texture in LoadedTextures.Values)
            {
                texture.Destroy();
                texture.InternalTexture = -1;
                texture.OriginalInternalID = -1;
            }
            LoadedTextures.Clear();
        }

        /// <summary>
        /// Updates the timestamp on the engine.
        /// </summary>
        /// <param name="time">The current time stamp.</param>
        public void Update(double time)
        {
            cTime = time;
        }

        /// <summary>
        /// The current game tick time.
        /// </summary>
        public double cTime = 1.0;

        /// <summary>
        /// Gets a texture that already exists by name.
        /// <para>Note: Most users should not use this method. Instead, use <see cref="GetTexture(string)"/>.</para>
        /// </summary>
        /// <param name="texturename">The name of the texture.</param>
        /// <returns>The texture, if it exists.</returns>
        public Texture GetExistingTexture(string texturename)
        {
            if (LoadedTextures.TryGetValue(texturename, out Texture foundTexture))
            {
                return foundTexture;
            }
            return null;
        }

        /// <summary>
        /// Gets the texture object for a specific texture name.
        /// If the relevant texture exists but is not yet loaded, will load it from file.
        /// </summary>
        /// <param name="textureName">The name of the texture.</param>
        /// <returns>A valid texture object.</returns>
        public Texture GetTexture(string textureName)
        {
            textureName = FileEngine.CleanFileName(textureName);
            if (LoadedTextures.TryGetValue(textureName, out Texture foundTexture))
            {
                return foundTexture;
            }
            Texture loaded = DynamicLoadTexture(textureName);
            LoadedTextures.Add(textureName, loaded);
            OnTextureLoaded?.Invoke(this, new TextureLoadedEventArgs(loaded));
            return loaded;
        }

        /// <summary>
        /// Dynamically loads a texture (returns a temporary copy of 'White', then fills it in when possible).
        /// </summary>
        /// <param name="textureName">The texture name to load.</param>
        /// <returns>The texture object.</returns>
        public Texture DynamicLoadTexture(string textureName)
        {
            textureName = FileEngine.CleanFileName(textureName);
            Texture texture = new Texture()
            {
                Engine = this,
                Name = textureName,
                OriginalInternalID = White.OriginalInternalID,
                InternalTexture = White.InternalTexture,
                LoadedProperly = false,
                Width = White.Width,
                Height = White.Height
            };
            void processLoad(byte[] data)
            {
                Bitmap bmp = BitmapForBytes(data);
                Schedule.ScheduleSyncTask(() =>
                {
                    TextureFromBitMap(texture, bmp);
                    texture.LoadedProperly = true;
                    bmp.Dispose();
                });
            }
            void fileMissing()
            {
                SysConsole.Output(OutputType.WARNING, $"Cannot load texture, file '{TextStyle.Standout}textures/{textureName}.png{TextStyle.Base}' does not exist.");
                texture.LoadedProperly = false;
            }
            void handleError(string message)
            {
                SysConsole.Output(OutputType.ERROR, $"Failed to load texture from filename '{TextStyle.Standout}textures/{textureName}.png{TextStyle.Error}': {message}");
                texture.LoadedProperly = false;
            }
            AssetStreaming.AddGoal($"textures/{textureName}.png", false, processLoad, fileMissing, handleError);
            return texture;
        }

        /// <summary>
        /// Gets a <see cref="Bitmap"/> for some data, with size correction.
        /// </summary>
        /// <param name="data">The raw file data.</param>
        /// <param name="textureWidth">The texture width (or 0 for any-valid).</param>
        /// <returns>The bitmap.</returns>
        public Bitmap BitmapForBytes(byte[] data, int textureWidth = 0)
        {
            Bitmap bmp = new Bitmap(new MemoryStream(data));
#if DEBUG
            if (bmp.Width <= 0 || bmp.Height <= 0)
            {
                throw new Exception("Failed to load texture: bitmap loading failed (bad output size)!");
            }
#endif
            if (textureWidth <= 0)
            {
                if (!AcceptableWidths.Contains(bmp.Width) || !AcceptableWidths.Contains(bmp.Height))
                {
                    int wid = GetNextPOTValue(bmp.Width);
                    int hei = GetNextPOTValue(bmp.Height);
                    Bitmap bmp_fixed = new Bitmap(bmp, new Size(wid, hei));
                    bmp.Dispose();
                    return bmp_fixed;
                }
                return bmp;
            }
            else if (bmp.Width == textureWidth && bmp.Height == textureWidth)
            {
                return bmp;
            }
            else
            {
                Bitmap bmp2 = new Bitmap(bmp, new Size(textureWidth, textureWidth));
                bmp.Dispose();
                return bmp2;
            }
        }

        /// <summary>
        /// Gets the a bitmap object for a texture by name.
        /// </summary>
        /// <param name="texturename">The name of the texture.</param>
        /// <param name="twidth">The texture width, if any.</param>
        /// <returns>A valid bitmap object, or null.</returns>
        public Bitmap GetTextureBitmapWithWidth(string texturename, int twidth)
        {
            texturename = FileEngine.CleanFileName(texturename);
            if (LoadedTextures.TryGetValue(texturename, out Texture foundTexture) && foundTexture.LoadedProperly)
            {
                if (foundTexture.Width == twidth && foundTexture.Height == twidth)
                {
                    return foundTexture.SaveToBMP();
                }
            }
            return LoadBitmapForTexture(texturename, twidth);
        }

        /// <summary>
        /// Fired when a texture is loaded.
        /// </summary>
        public event EventHandler<TextureLoadedEventArgs> OnTextureLoaded;

        /// <summary>
        /// Whether textures should use linear mode usually.
        /// </summary>
        public bool DefaultLinear = true;

        /// <summary>
        /// Gets the next Power Of Two value.
        /// </summary>
        /// <param name="input">Input.</param>
        /// <returns>Output.</returns>
        public static int GetNextPOTValue(int input)
        {
            int x = 1;
            while (input > x)
            {
                x *= 2;
            }
            return x;
        }

        /// <summary>
        /// Loads a texture's bitmap from file.
        /// </summary>
        /// <param name="filename">The name of the file to use.</param>
        /// <param name="twidth">The texture width, if any.</param>
        /// <returns>The loaded texture bitmap, or null if it does not exist.</returns>
        public Bitmap LoadBitmapForTexture(string filename, int twidth)
        {
            try
            {
                filename = FileEngine.CleanFileName(filename);
                if (!Files.TryReadFileData($"textures/{filename}.png", out byte[] textureFile))
                {
                    SysConsole.Output(OutputType.WARNING, $"Cannot load texture, file '{TextStyle.Standout}textures/{filename}.png{TextStyle.Base}' does not exist.");
                    return null;
                }
                return BitmapForBytes(textureFile, twidth);
            }
            catch (Exception ex)
            {
                SysConsole.Output(OutputType.ERROR, $"Failed to load texture from filename '{TextStyle.Standout}textures/{filename}.png{TextStyle.Error}': {ex}");
                return null;
            }
        }

        /// <summary>
        /// Loads a texture from file.
        /// <para>Note: Most users should not use this method. Instead, use <see cref="GetTexture(string)"/>.</para>
        /// </summary>
        /// <param name="filename">The name of the file to use.</param>
        /// <param name="twidth">The texture width, if any.</param>
        /// <returns>The loaded texture, or null if it does not exist.</returns>
        public Texture LoadTexture(string filename, int twidth = 0)
        {
            Texture texture = new Texture()
            {
                Engine = this,
                Name = filename
            };
            using Bitmap bmp = LoadBitmapForTexture(filename, twidth);
            if (bmp == null)
            {
                return null;
            }
            TextureFromBitMap(texture, bmp);
            texture.LoadedProperly = true;
            return texture;
        }

        private void TextureFromBitMap(Texture texture, Bitmap bmp)
        {
            texture.Width = bmp.Width;
            texture.Height = bmp.Height;
            GL.GenTextures(1, out texture.OriginalInternalID);
            texture.InternalTexture = texture.OriginalInternalID;
            texture.Bind();
            LockBitmapToTexture(bmp, DefaultLinear);
        }

        /// <summary>
        /// loads a texture by name and puts it into a texture array.
        /// </summary>
        /// <param name="filename">The texture array.</param>
        /// <param name="depth">The depth in the array.</param>
        /// <param name="twidth">The texture width.</param>
        public void LoadTextureIntoArray(string filename, int depth, int twidth)
        {
            try
            {
                // TODO: store!
                filename = FileEngine.CleanFileName(filename);
                if (!Files.TryReadFileData($"textures/{filename}.png", out byte[] textureData))
                {
                    SysConsole.Output(OutputType.WARNING, $"Cannot load texture, file '{TextStyle.Standout}textures/{filename}.png{TextStyle.Base}' does not exist.");
                    return;
                }
                using Bitmap bmp = BitmapForBytes(textureData, twidth);
                LockBitmapToTexture(bmp, depth);
            }
            catch (Exception ex)
            {
                SysConsole.Output(OutputType.ERROR, $"Failed to load texture from filename '{TextStyle.Standout}textures/{filename}.png{TextStyle.Error}': {ex}");
                return;
            }
        }

        /// <summary>
        /// Creates a Texture object for a specific color.
        /// </summary>
        /// <param name="c">The color to use.</param>
        /// <param name="name">The name of the texture.</param>
        /// <returns>The generated texture.</returns>
        public Texture GenerateForColor(Color c, string name)
        {
            Texture texture = new Texture()
            {
                Engine = this,
                Name = name,
                Width = 2,
                Height = 2
            };
            GL.GenTextures(1, out texture.OriginalInternalID);
            texture.InternalTexture = texture.OriginalInternalID;
            texture.Bind();
            using (Bitmap bmp = new Bitmap(2, 2))
            {
                bmp.SetPixel(0, 0, c);
                bmp.SetPixel(0, 1, c);
                bmp.SetPixel(1, 0, c);
                bmp.SetPixel(1, 1, c);
                LockBitmapToTexture(bmp, false);
            }
            texture.LoadedProperly = true;
            return texture;
        }

        /// <summary>
        /// Locks a bitmap file's data to a GL texture.
        /// </summary>
        /// <param name="bmp">The bitmap to use.</param>
        /// <param name="linear">Whether to use linear filtering for the texture (otherwise, "Nearest" filtering mode).</param>
        public void LockBitmapToTexture(Bitmap bmp, bool linear)
        {
            BitmapData bmp_data = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, bmp_data.Width, bmp_data.Height, 0, OpenTK.Graphics.OpenGL4.PixelFormat.Bgra, PixelType.UnsignedByte, bmp_data.Scan0);
            GL.Flush();
            bmp.UnlockBits(bmp_data);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, linear ? (int)TextureMinFilter.Linear : (int)TextureMinFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, linear ? (int)TextureMagFilter.Linear : (int)TextureMagFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat); // TODO: Is Repeat best here?
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureCompareMode, (int)TextureCompareMode.CompareRefToTexture);
        }

        /// <summary>
        /// Locks a bitmap file's data to a GL texture array.
        /// </summary>
        /// <param name="bmp">The bitmap to use.</param>
        /// <param name="depth">The depth in a 3D texture.</param>
        public void LockBitmapToTexture(Bitmap bmp, int depth)
        {
            BitmapData bmp_data = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            GL.TexSubImage3D(TextureTarget.Texture2DArray, 0, 0, 0, depth, bmp.Width, bmp.Height, 1, OpenTK.Graphics.OpenGL4.PixelFormat.Bgra, PixelType.UnsignedByte, bmp_data.Scan0);
            GL.Flush();
            bmp.UnlockBits(bmp_data);
        }
    }
}
