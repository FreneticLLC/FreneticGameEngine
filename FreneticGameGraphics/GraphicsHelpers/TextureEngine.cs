//
// This file is created by Frenetic LLC.
// This code is Copyright (C) 2017 Frenetic LLC under the terms of a strict license.
// See README.md or LICENSE.txt in the source root for the contents of the license.
// If neither of these are available, assume that neither you nor anyone other than the copyright holder
// hold any right or permission to use this software until such time as the official license is identified.
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Drawing.Imaging;
using OpenTK.Graphics.OpenGL4;
using FreneticGameCore;
using FreneticGameCore.Files;

namespace FreneticGameGraphics.GraphicsHelpers
{
    /// <summary>
    /// The primary engine for textures.
    /// </summary>
    public class TextureEngine : IDisposable
    {
        /// <summary>
        /// Dumb MS logic dispoe method.
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
        /// TODO: List->Dictionary?
        /// </summary>
        public List<Texture> LoadedTextures = null;

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
        /// The relevant game client.
        /// </summary>
        public FileHandler Files;

        /// <summary>
        /// Starts or restarts the texture system.
        /// </summary>
        public void InitTextureSystem(FileHandler files)
        {
            Files = files;
            // Create a generic graphics object for later use
            EmptyBitmap = new Bitmap(1, 1);
            GenericGraphicsObject = Graphics.FromImage(EmptyBitmap);
            // Reset texture list
            LoadedTextures = new List<Texture>();
            // Pregenerate a few needed textures
            White = GenerateForColor(Color.White, "white");
            LoadedTextures.Add(White);
            Black = GenerateForColor(Color.Black, "black");
            LoadedTextures.Add(Black);
            Clear = GenerateForColor(Color.Transparent, "clear");
            LoadedTextures.Add(Clear);
            NormalDef = GetTexture("normal_def");
        }

        /// <summary>
        /// Clears away all current textures.
        /// </summary>
        public void Empty()
        {
            for (int i = 0; i < LoadedTextures.Count; i++)
            {
                LoadedTextures[i].Destroy();
                LoadedTextures[i].Internal_Texture = -1;
                LoadedTextures[i].Original_InternalID = -1;
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
        /// </summary>
        /// <param name="texturename">The name of the texture.</param>
        /// <returns>The texture, if it exists.</returns>
        public Texture GetExistingTexture(string texturename)
        {
            for (int i = 0; i < LoadedTextures.Count; i++)
            {
                if (LoadedTextures[i].Name == texturename)
                {
                    return LoadedTextures[i];
                }
            }
            return null;
        }

        /// <summary>
        /// Gets the texture object for a specific texture name.
        /// </summary>
        /// <param name="texturename">The name of the texture.</param>
        /// <param name="twidth">The texture width, if any.</param>
        /// <returns>A valid texture object.</returns>
        public Texture GetTexture(string texturename, int twidth = 0)
        {
            texturename = FileHandler.CleanFileName(texturename);
            for (int i = 0; i < LoadedTextures.Count; i++)
            {
                if (LoadedTextures[i].Name == texturename && (twidth == 0 || LoadedTextures[i].Width == twidth))
                {
                    return LoadedTextures[i];
                }
            }
            Texture Loaded = LoadTexture(texturename, twidth);
            if (Loaded == null && twidth == 0)
            {
                Loaded = new Texture()
                {
                    Engine = this,
                    Name = texturename,
                    Internal_Texture = White.Original_InternalID,
                    Original_InternalID = White.Original_InternalID,
                    LoadedProperly = false
                };
            }
            if (Loaded == null)
            {
                Loaded = LoadTexture("white", twidth);
                Loaded.Name = texturename;
                Loaded.LoadedProperly = false;
            }
            LoadedTextures.Add(Loaded);
            OnTextureLoaded?.Invoke(this, new TextureLoadedEventArgs(Loaded));
            return Loaded;
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
        /// Loads a texture from file.
        /// </summary>
        /// <param name="filename">The name of the file to use.</param>
        /// <param name="twidth">The texture width, if any.</param>
        /// <returns>The loaded texture, or null if it does not exist.</returns>
        // Shut up the CodeAnalysis (It doesn't like the BMP variable handling below).
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2202:Do not dispose objects multiple times")]
        public Texture LoadTexture(string filename, int twidth = 0)
        {
            try
            {
                filename = FileHandler.CleanFileName(filename);
                if (!Files.Exists("textures/" + filename + ".png"))
                {
                    SysConsole.Output(OutputType.ERROR, "Cannot load texture, file '" +
                        TextStyle.Color_Standout + "textures/" + filename + ".png" + TextStyle.Color_Error +
                        "' does not exist.");
                    return null;
                }
                Bitmap bmp = new Bitmap(Files.ReadToStream("textures/" + filename + ".png"));
                if (twidth <= 0 && !AcceptableWidths.Contains(bmp.Width) || !AcceptableWidths.Contains(bmp.Height))
                {
                    int wid = GetNextPOTValue(bmp.Width);
                    int hei = GetNextPOTValue(bmp.Height);
                    Bitmap bmp_fixed = new Bitmap(bmp, new Size(wid, hei));
                    bmp.Dispose();
                    bmp = bmp_fixed;
                }
                Bitmap bmp2 = twidth <= 0 ? bmp : new Bitmap(bmp, new Size(twidth, twidth));
                Texture texture = new Texture()
                {
                    Engine = this,
                    Name = filename,
                    Width = bmp2.Width,
                    Height = bmp2.Height
                };
                GL.GenTextures(1, out texture.Original_InternalID);
                texture.Internal_Texture = texture.Original_InternalID;
                texture.Bind();
                LockBitmapToTexture(bmp2, DefaultLinear);
                if (bmp2 != bmp)
                {
                    bmp2.Dispose();
                }
                bmp.Dispose();
                texture.LoadedProperly = true;
                return texture;
            }
            catch (Exception ex)
            {
                SysConsole.Output(OutputType.ERROR, "Failed to load texture from filename '" +
                    TextStyle.Color_Standout + "textures/" + filename + ".png" + TextStyle.Color_Error + "': " + ex.ToString());
                return null;
            }
        }

        /// <summary>
        /// Gets the ID number for a texture, loading it uniquely (won't be in the main engine!).
        /// </summary>
        /// <param name="name">The name of the texture.</param>
        /// <param name="twidth">The texture width, if needed.</param>
        /// <returns>The texture ID.</returns>
        public int GetTextureID(string name, int twidth = 0)
        {
            return (LoadTexture(name, twidth) ?? LoadTexture("white", twidth)).Original_InternalID;
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
                filename = FileHandler.CleanFileName(filename);
                if (!Files.Exists("textures/" + filename + ".png"))
                {
                    SysConsole.Output(OutputType.ERROR, "Cannot load texture, file '" +
                        TextStyle.Color_Standout + "textures/" + filename + ".png" + TextStyle.Color_Error +
                        "' does not exist.");
                    return;
                }
                Bitmap bmp = new Bitmap(Files.ReadToStream("textures/" + filename + ".png"));
                Bitmap bmp2 = new Bitmap(bmp, new Size(twidth, twidth));
                LockBitmapToTexture(bmp2, depth);
                bmp2.Dispose();
                bmp.Dispose();
            }
            catch (Exception ex)
            {
                SysConsole.Output(OutputType.ERROR, "Failed to load texture from filename '" +
                    TextStyle.Color_Standout + "textures/" + filename + ".png" + TextStyle.Color_Error + "': " + ex.ToString());
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
            GL.GenTextures(1, out texture.Original_InternalID);
            texture.Internal_Texture = texture.Original_InternalID;
            texture.Bind();
            Bitmap bmp = new Bitmap(2, 2);
            bmp.SetPixel(0, 0, c);
            bmp.SetPixel(0, 1, c);
            bmp.SetPixel(1, 0, c);
            bmp.SetPixel(1, 1, c);
            LockBitmapToTexture(bmp, false);
            bmp.Dispose();
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
            BitmapData bmp_data = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height),
                ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            GL.TexSubImage3D(TextureTarget.Texture2DArray, 0, 0, 0, depth, bmp.Width, bmp.Height, 1, OpenTK.Graphics.OpenGL4.PixelFormat.Bgra, PixelType.UnsignedByte, bmp_data.Scan0);
            bmp.UnlockBits(bmp_data);
        }

    }

    /// <summary>
    /// Event arguments for a texture being loaded.
    /// </summary>
    public class TextureLoadedEventArgs : EventArgs
    {
        /// <summary>
        /// Constructs a texture loaded event argument set.
        /// </summary>
        /// <param name="t">The texture that was loaded.</param>
        public TextureLoadedEventArgs(Texture t)
        {
            Tex = t;
        }

        /// <summary>
        /// The texture that was loaded.
        /// </summary>
        public Texture Tex;
    }

    /// <summary>
    /// Wraps an OpenGL texture.
    /// </summary>
    public class Texture
    {
        /// <summary>
        /// The texture engine that owns this texture.
        /// </summary>
        public TextureEngine Engine;

        /// <summary>
        /// The full name of the texture.
        /// </summary>
        public string Name;

        /// <summary>
        /// The texture that this texture was remapped to, if any.
        /// </summary>
        public Texture RemappedTo;

        /// <summary>
        /// The internal OpenGL texture ID.
        /// </summary>
        public int Internal_Texture = 0;

        /// <summary>
        /// The original OpenGL texture ID that formed this texture.
        /// </summary>
        public int Original_InternalID = 0;

        /// <summary>
        /// Whether the texture loaded properly.
        /// </summary>
        public bool LoadedProperly = false;

        /// <summary>
        /// The width of the texture.
        /// </summary>
        public int Width;

        /// <summary>
        /// The height of the texture.
        /// </summary>
        public int Height;

        /// <summary>
        /// Removes the texture from OpenGL.
        /// </summary>
        public void Destroy()
        {
            if (Original_InternalID > -1 && GL.IsTexture(Original_InternalID))
            {
                GL.DeleteTexture(Original_InternalID);
            }
        }

        /// <summary>
        /// Removes the texture from the system.
        /// </summary>
        public void Remove()
        {
            Destroy();
            Engine.LoadedTextures.Remove(this);
        }

        /// <summary>
        /// Saves the texture to a bitmap.
        /// </summary>
        /// <param name="flip">Whether to flip the Y.</param>
        public Bitmap SaveToBMP(bool flip = false)
        {
            GL.BindTexture(TextureTarget.Texture2D, Original_InternalID);
            Bitmap bmp = new Bitmap(Width, Height);
            BitmapData data = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), System.Drawing.Imaging.ImageLockMode.WriteOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            GL.GetTexImage(TextureTarget.Texture2D, 0, OpenTK.Graphics.OpenGL4.PixelFormat.Bgra, PixelType.UnsignedByte, data.Scan0);
            bmp.UnlockBits(data);
            if (flip)
            {
                bmp.RotateFlip(RotateFlipType.RotateNoneFlipY);
            }
            return bmp;
        }

        /// <summary>
        /// The tick time this texture was last bound.
        /// </summary>
        public double LastBindTime = 0;

        /// <summary>
        /// Checks if the texture is valid, and replaces it if needed.
        /// </summary>
        public void CheckValid()
        {
            if (Internal_Texture == -1)
            {
                Texture temp = Engine.GetTexture(Name);
                Original_InternalID = temp.Original_InternalID;
                Internal_Texture = Original_InternalID;
                if (RemappedTo != null)
                {
                    RemappedTo.CheckValid();
                    Internal_Texture = RemappedTo.Original_InternalID;
                }
            }
        }

        /// <summary>
        /// Binds this texture to OpenGL.
        /// </summary>
        public void Bind()
        {
            LastBindTime = Engine.cTime;
            CheckValid();
            GL.BindTexture(TextureTarget.Texture2D, Internal_Texture);
        }
        
        /// <summary>
        /// Gets the name of the texture.
        /// </summary>
        /// <returns>The name.</returns>
        public override string ToString()
        {
            return Name;
        }
    }
}
