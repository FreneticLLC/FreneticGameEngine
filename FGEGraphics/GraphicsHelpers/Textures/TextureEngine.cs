//
// This file is part of the Frenetic Game Engine, created by Frenetic LLC.
// This code is Copyright (C) Frenetic LLC under the terms of a strict license.
// See README.md or LICENSE.txt in the FreneticGameEngine source root for the contents of the license.
// If neither of these are available, assume that neither you nor anyone other than the copyright holder
// hold any right or permission to use this software until such time as the official license is identified.
//

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using FGECore.ConsoleHelpers;
using FGECore.CoreSystems;
using FGECore.FileSystems;
using FGECore.MathHelpers;
using OpenTK.Graphics.OpenGL4;
using SkiaSharp;

namespace FGEGraphics.GraphicsHelpers.Textures;

/// <summary>The primary engine for textures.</summary>
public class TextureEngine
{
    /// <summary>A full list of currently loaded textures.</summary>
    public Dictionary<string, Texture> LoadedTextures;

    /// <summary>A default white texture.</summary>
    public Texture White = null;

    /// <summary>A default clear texture.</summary>
    public Texture Clear = null;

    /// <summary>A default black texture.</summary>
    public Texture Black = null;

    /// <summary>A default middle gray texture.</summary>
    public Texture Gray = null;

    /// <summary>A default normal plane texture.</summary>
    public Texture NormalDef = null;

    /// <summary>The relevant file helper.</summary>
    public FileEngine Files;

    /// <summary>The relevant asset streaming helper.</summary>
    public AssetStreamingEngine AssetStreaming;

    /// <summary>The relevant scheduler.</summary>
    public Scheduler Schedule;

    /// <summary>File extensions for textures that will be accepted, aside from default "png".</summary>
    public string[] AlternateImageFileExtensions = ["jpg"];

    /// <summary>Starts or restarts the texture system.</summary>
    /// <param name="files">The relevant file helper.</param>
    /// <param name="assetStreaming">The relevant asset streaming helper.</param>
    /// <param name="schedule">The relevant scheduler.</param>
    public void InitTextureSystem(FileEngine files, AssetStreamingEngine assetStreaming, Scheduler schedule)
    {
        Files = files;
        AssetStreaming = assetStreaming;
        Schedule = schedule;
        // Reset texture list
        LoadedTextures = new Dictionary<string, Texture>(256);
        // Pregenerate a few needed textures
        White = GenerateForColor(Color4F.White, "white");
        LoadedTextures.Add("white", White);
        Black = GenerateForColor(Color4F.Black, "black");
        LoadedTextures.Add("black", Black);
        Clear = GenerateForColor(Color4F.Transparent, "clear");
        LoadedTextures.Add("clear", Clear);
        Gray = GenerateForColor(Color4F.FromArgb(255, 127, 127, 127), "gray");
        LoadedTextures.Add("gray", Gray);
        NormalDef = GenerateForColor(Color4F.FromArgb(255, 127, 127, 255), "normal_def");
        LoadedTextures.Add("normal_def", NormalDef);
    }

    /// <summary>Clears away all current textures.</summary>
    public void Empty()
    {
        foreach (Texture texture in LoadedTextures.Values)
        {
            texture.Destroy();
        }
        LoadedTextures.Clear();
    }

    /// <summary>Updates the timestamp on the engine.</summary>
    /// <param name="time">The current time stamp.</param>
    public void Update(double time)
    {
        CurrentTime = time;
    }

    /// <summary>The current game tick time.</summary>
    public double CurrentTime = 1.0;

    /// <summary>
    /// Gets a texture that already exists by name.
    /// <para>Note: Most users should not use this method. Instead, use <see cref="GetTexture"/>.</para>
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
    /// <param name="highPriority">If true, load this texture with a high priority. If false, stream it slowly with a thumbnail first.</param>
    /// <returns>A valid texture object.</returns>
    public Texture GetTexture(string textureName, bool highPriority = false)
    {
        textureName = FileEngine.CleanFileName(textureName);
        if (LoadedTextures.TryGetValue(textureName, out Texture foundTexture))
        {
            return foundTexture;
        }
        Texture loaded = DynamicLoadTexture(textureName, highPriority);
        LoadedTextures.Add(textureName, loaded);
        return loaded;
    }

    /// <summary>Dynamically loads a texture (returns a temporary copy of 'White', then fills it to a lowres thumbnail soon, then fills it in when possible).</summary>
    /// <param name="textureName">The texture name to load.</param>
    /// <param name="highPriority">If true, load this texture with a high priority. If false, stream it slowly with a thumbnail first.</param>
    /// <returns>The texture object.</returns>
    public Texture DynamicLoadTexture(string textureName, bool highPriority = false)
    {
        textureName = FileEngine.CleanFileName(textureName);
        Texture texture = new()
        {
            Engine = this,
            Name = textureName,
            OriginalInternalID = White.OriginalInternalID,
            InternalTexture = White.InternalTexture,
            LoadedProperly = false,
            OwnsItsTextureId = false,
            Width = White.Width,
            Height = White.Height
        };
        string targetName = $"textures/{textureName}.png";
        void handleError(string message)
        {
            Logs.Error($"Failed to load texture from filename '{TextStyle.Standout}{targetName}{TextStyle.Base}': {message}");
            Schedule.ScheduleSyncTask(() =>
            {
                texture.Destroy();
                texture.InternalTexture = White.InternalTexture;
            });
        }
        void processThumb(byte[] data)
        {
            if (texture.LoadedProperly)
            {
                return;
            }
            SKBitmap bmp;
            try
            {
                bmp = BitmapForBytes(data);
            }
            catch (Exception ex)
            {
                handleError(ex.ToString());
                return;
            }
            Schedule.ScheduleSyncTask(() =>
            {
                try
                {
                    if (texture.LoadedProperly)
                    {
                        return;
                    }
                    InternalTextureFromBitMap(texture, bmp);
                }
                catch (Exception ex)
                {
                    handleError(ex.ToString());
                }
                bmp.Dispose();
            });
        }
        void processLoad(byte[] data)
        {
            SKBitmap bmp;
            try
            {
                bmp = BitmapForBytes(data);
            }
            catch (Exception ex)
            {
                handleError(ex.ToString());
                return;
            }
            Schedule.ScheduleSyncTask(() =>
            {
                try
                {
                    InternalTextureFromBitMap(texture, bmp);
                    texture.LoadedProperly = true;
                    OnTextureLoaded?.Invoke(this, new TextureLoadedEventArgs(texture));
                }
                catch (Exception ex)
                {
                    handleError(ex.ToString());
                }
                bmp.Dispose();
            });
        }
        void irrelevantMissing() { }
        void fileMissing()
        {
            Logs.Warning($"Cannot load non-existent texture '{TextStyle.Standout}{targetName}{TextStyle.Base}'. {Files.WarnWhyFileMissing(targetName)}");
            Schedule.ScheduleSyncTask(() =>
            {
                texture.Destroy();
                texture.InternalTexture = White.InternalTexture;
            });
        }
        if (highPriority)
        {
            AssetStreaming.AddGoal(targetName, false, processLoad, fileMissing, handleError, AlternateImageFileExtensions, priority: AssetStreamingEngine.GoalPriority.FASTEST);
        }
        else
        {
            AssetStreaming.AddGoal($"textures/{textureName}.thumb.jpg", false, processThumb, irrelevantMissing, handleError, priority: AssetStreamingEngine.GoalPriority.FAST);
            AssetStreaming.AddGoal(targetName, false, processLoad, fileMissing, handleError, AlternateImageFileExtensions, priority: AssetStreamingEngine.GoalPriority.SLOW);
        }
        return texture;
    }

    /// <summary>Copies <see cref="SKBitmap"/> pixels to a tightly packed BGRA byte array.</summary>
    public static byte[] BitmapBytes(SKBitmap bmp)
    {
        int width = bmp.Width, height = bmp.Height;
        int stride = width * 4;
        byte[] bytes = new byte[stride * height];
        IntPtr pixels = bmp.GetPixels();
        int rowBytes = bmp.RowBytes;
        if (rowBytes == stride)
        {
            Marshal.Copy(pixels, bytes, 0, bytes.Length);
        }
        else // unlikely to ever hit outside special cases
        {
            for (int y = 0; y < height; y++)
            {
                Marshal.Copy(pixels + y * rowBytes, bytes, y * stride, stride);
            }
        }
        return bytes;
    }

    /// <summary>
    /// Produces a copy of the given bitmap, with a new image size.
    /// Akin to "new Bitmap(bmp, width, height)" but forces certain quality options to prevent edge-errors.
    /// </summary>
    /// <param name="bmp">The original image.</param>
    /// <param name="width">The new output image's width (X) (in pixels).</param>
    /// <param name="height">The new output image's height (Y) (in pixels).</param>
    /// <returns>The resized image.</returns>
    public static SKBitmap RescaleBitmap(SKBitmap bmp, int width, int height)
    {
        SKBitmap output = new(width, height, SKColorType.Bgra8888, SKAlphaType.Unpremul);
        bmp.ScalePixels(output, new SKSamplingOptions(SKFilterMode.Nearest));
        return output;
    }

    /// <summary>Gets a <see cref="SKBitmap"/> for some data, with size correction.</summary>
    /// <param name="data">The raw file data.</param>
    /// <param name="textureWidth">The texture width (or 0 for any-valid).</param>
    /// <returns>The bitmap.</returns>
    public static SKBitmap BitmapForBytes(byte[] data, int textureWidth = 0)
    {
        if (data.Length < 1)
        {
            throw new Exception("Failed to load texture: bitmap loading failed (no data)!");
        }
        SKBitmap bmp;
        try
        {
            bmp = SKBitmap.Decode(data) ?? throw new ArgumentException("Generic failure, SKBitmap.Decode returned null.");
        }
        catch (Exception ex)
        {
            throw new ArgumentException($"{ex.Message} -- for texture data: {data.Length} bytes ({Convert.ToHexString(data[0..Math.Min(32, data.Length)])})", ex);
        }
#if DEBUG
        if (bmp.Width <= 0 || bmp.Height <= 0)
        {
            bmp.Dispose();
            throw new Exception("Failed to load texture: bitmap loading failed (bad output size)!");
        }
#endif
        if (bmp.ColorType != SKColorType.Bgra8888)
        {
            SKBitmap bmp2 = bmp.Copy(SKColorType.Bgra8888);
            bmp.Dispose();
            bmp = bmp2 ?? throw new Exception("Failed to load texture: BGRA8888 conversion failed!");
        }
        if (textureWidth <= 0 || (bmp.Width == textureWidth && bmp.Height == textureWidth))
        {
            return bmp;
        }
        else
        {
            SKBitmap bmp2 = RescaleBitmap(bmp, textureWidth, textureWidth);
            bmp.Dispose();
            return bmp2;
        }
    }

    /// <summary>Gets the a bitmap object for a texture by name.</summary>
    /// <param name="texturename">The name of the texture.</param>
    /// <param name="twidth">The texture width, if any.</param>
    /// <param name="docache">If true, use caching. If false, always fetch a fresh copy.</param>
    /// <returns>A valid bitmap object, or null.</returns>
    public SKBitmap GetTextureBitmapWithWidth(string texturename, int twidth, bool docache = false)
    {
        texturename = FileEngine.CleanFileName(texturename);
        if (LoadedTextures.TryGetValue(texturename, out Texture foundTexture) && foundTexture.LoadedProperly)
        {
            if (foundTexture.Width == twidth && foundTexture.Height == twidth)
            {
                return foundTexture.SaveToBMP();
            }
        }
        return LoadBitmapForTexture(texturename, twidth, docache);
    }

    /// <summary>Fired when a texture is loaded.</summary>
    public event EventHandler<TextureLoadedEventArgs> OnTextureLoaded;

    /// <summary>Whether textures should use linear mode usually.</summary>
    public bool DefaultLinear = true;

    /// <summary>Short-lived cached of texture image raw data, to allow rapid multi calls to not run the entire file loading engine.</summary>
    public Dictionary<string, byte[]> TempBitmapBytesCache = [];

    /// <summary>Short-lived cached of texture bitmaps, to allow rapid multi calls to not run the entire file loading engine.</summary>
    public Dictionary<(string, int), SKBitmap> TempBitmapCache = [];

    /// <summary>If true, the temp-cache will be cleared soon.</summary>
    public bool CacheHasClearScheduled = false;

    /// <summary>Schedules the temp-cache to be cleared soon, if needed.</summary>
    public void ScheduleClearCache()
    {
        if (CacheHasClearScheduled)
        {
            return;
        }
        CacheHasClearScheduled = true;
        Schedule.ScheduleSyncTask(() =>
        {
            CacheHasClearScheduled = false;
            if (TempBitmapCache.Count != 0)
            {
                SKBitmap[] bmps = [.. TempBitmapCache.Values];
                TempBitmapCache.Clear();
                Schedule.StartAsyncTask(() =>
                {
                    foreach (SKBitmap bmp in bmps)
                    {
                        bmp.Dispose();
                    }
                });
            }
            TempBitmapBytesCache.Clear();
        }, 0.1);
    }

    /// <summary>Loads a texture's bitmap from file.</summary>
    /// <param name="filename">The name of the file to use.</param>
    /// <param name="twidth">The texture width, if any.</param>
    /// <param name="docache">If true, use caching. If false, always create a fresh copy.</param>
    /// <param name="extension">Specific file extension (eg '.png'), if required. Null for automatic search.</param>
    /// <returns>The loaded texture bitmap, or null if it does not exist.</returns>
    public SKBitmap LoadBitmapForTexture(string filename, int twidth, bool docache = false, string extension = null)
    {
        filename = FileEngine.CleanFileName(filename);
        if (extension is not null)
        {
            filename += extension;
        }
        if (docache && TempBitmapCache.TryGetValue((filename, twidth), out SKBitmap cachedBitmap))
        {
            return cachedBitmap;
        }
        try
        {
            byte[] getBytes(string filename)
            {
                if (TempBitmapBytesCache.TryGetValue(filename, out byte[] textureFile))
                {
                    return textureFile;
                }
                if (Files.TryReadFileData(extension is null ? $"textures/{filename}.png" : $"textures/{filename}", out textureFile))
                {
                    TempBitmapBytesCache[filename] = textureFile;
                    ScheduleClearCache();
                    return textureFile;
                }
                if (extension is null)
                {
                    foreach (string ext in AlternateImageFileExtensions)
                    {
                        if (Files.TryReadFileData($"textures/{filename}.{ext}", out textureFile))
                        {
                            TempBitmapBytesCache[filename] = textureFile;
                            return textureFile;
                        }
                    }
                    Logs.Warning($"Cannot load texture, file '{TextStyle.Standout}textures/{filename}.png{TextStyle.Base}' does not exist.");
                }
                return null;
            }
            byte[] textureFile = getBytes(filename);
            if (textureFile is null)
            {
                return null;
            }
            SKBitmap result = BitmapForBytes(textureFile, twidth);
            if (docache)
            {
                TempBitmapCache[(filename, twidth)] = result;
                ScheduleClearCache();
            }
            return result;
        }
        catch (Exception ex)
        {
            Logs.Error($"Failed to load texture from filename '{TextStyle.Standout}textures/{filename}.png{TextStyle.Base}': {ex}");
            return null;
        }
    }

    /// <summary>
    /// Loads a texture from file.
    /// <para>Note: Most users should not use this method. Instead, use <see cref="GetTexture"/>.</para>
    /// </summary>
    /// <param name="filename">The name of the file to use.</param>
    /// <param name="twidth">The texture width, if any.</param>
    /// <returns>The loaded texture, or null if it does not exist.</returns>
    public Texture LoadTexture(string filename, int twidth = 0)
    {
        Texture texture = new()
        {
            Engine = this,
            Name = filename
        };
        SKBitmap bmp = LoadBitmapForTexture(filename, twidth);
        if (bmp is null)
        {
            return null;
        }
        InternalTextureFromBitMap(texture, bmp);
        texture.LoadedProperly = true;
        return texture;
    }

    /// <summary>Internal helper to make a GL texture from a bitmap.</summary>
    public void InternalTextureFromBitMap(Texture texture, SKBitmap bmp)
    {
        texture.Width = bmp.Width;
        texture.Height = bmp.Height;
        if (!texture.OwnsItsTextureId)
        {
            texture.OriginalInternalID = new($"FGETexture_FromBitmap_{texture.Name}", TextureTarget.Texture2D);
            texture.InternalTexture = texture.OriginalInternalID;
            texture.OwnsItsTextureId = true;
        }
        texture.Bind();
        LockBitmapToTexture(bmp.Width, bmp.Height, BitmapBytes(bmp), DefaultLinear);
    }

    /// <summary>loads a thumbnail texture by name and puts it into a texture array.</summary>
    /// <param name="filename">The texture array.</param>
    /// <param name="depth">The depth in the array.</param>
    /// <param name="twidth">The texture width.</param>
    public void LoadThumbIntoArray(string filename, int depth, int twidth)
    {
        try
        {
            SKBitmap bmp = LoadBitmapForTexture(filename, twidth, docache: true, extension: ".thumb.jpg");
            if (bmp is not null)
            {
                LockBitmapToTexture(bmp, depth);
            }
        }
        catch (Exception ex)
        {
            Logs.Error($"Failed to load texture thumbnail from filename '{TextStyle.Standout}textures/{filename}.thumb.jpg{TextStyle.Base}': {ex}");
            return;
        }
    }

    /// <summary>loads a texture by name and puts it into a texture array.</summary>
    /// <param name="filename">The texture array.</param>
    /// <param name="depth">The depth in the array.</param>
    /// <param name="twidth">The texture width.</param>
    public void LoadTextureIntoArray(string filename, int depth, int twidth)
    {
        try
        {
            SKBitmap bmp = LoadBitmapForTexture(filename, twidth, docache: true);
            if (bmp is not null)
            {
                LockBitmapToTexture(bmp, depth);
            }
        }
        catch (Exception ex)
        {
            Logs.Error($"Failed to load texture from filename '{TextStyle.Standout}textures/{filename}.png{TextStyle.Base}': {ex}");
            return;
        }
    }

    /// <summary>Creates a Texture object for a specific color.</summary>
    /// <param name="c">The color to use.</param>
    /// <param name="name">The name of the texture.</param>
    /// <returns>The generated texture.</returns>
    public Texture GenerateForColor(Color4F c, string name)
    {
        Texture texture = new()
        {
            Engine = this,
            Name = name,
            Width = 2,
            Height = 2,
            OriginalInternalID = new($"FGETexture_ForColor_{name}", TextureTarget.Texture2D)
        };
        texture.InternalTexture = texture.OriginalInternalID;
        texture.OwnsItsTextureId = true;
        texture.Bind();
        byte b = (byte)c.IB, g = (byte)c.IG, r = (byte)c.IR, a = (byte)c.IA;
        LockBitmapToTexture(2, 2, [b, g, r, a, b, g, r, a, b, g, r, a, b, g, r, a], false);
        texture.LoadedProperly = true;
        return texture;
    }

    /// <summary>Locks a bitmap file's data to a GL texture.</summary>
    /// <param name="width">The width of the bitmap image.</param>
    /// <param name="height">The height of the bitmap image.</param>
    /// <param name="rawBitmap">The raw bitmapdata to use, from <see cref="BitmapBytes(SKBitmap)"/>.</param>
    /// <param name="linear">Whether to use linear filtering for the texture (otherwise, "Nearest" filtering mode).</param>
    public static void LockBitmapToTexture(int width, int height, byte[] rawBitmap, bool linear)
    {
        GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, width, height, 0, OpenTK.Graphics.OpenGL4.PixelFormat.Bgra, PixelType.UnsignedByte, rawBitmap);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, linear ? (int)TextureMinFilter.Linear : (int)TextureMinFilter.Nearest);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, linear ? (int)TextureMagFilter.Linear : (int)TextureMagFilter.Nearest);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureCompareMode, (int)TextureCompareMode.CompareRefToTexture);
        GraphicsUtil.CheckError("LockBitmapToTexture");
    }

    /// <summary>Locks a bitmap file's data to a GL texture array.</summary>
    /// <param name="bmp">The bitmap to use.</param>
    /// <param name="depth">The depth in a 3D texture.</param>
    public static void LockBitmapToTexture(SKBitmap bmp, int depth)
    {
#if DEBUG
        if (bmp.Width <= 0 || bmp.Height <= 0 || bmp.Width > 1024 * 256 || bmp.Height > 1024 * 256)
        {
            throw new InvalidOperationException($"Bitmap contains invalid dimensions: {bmp.Width}x{bmp.Height}");
        }
#endif
        LockBitmapToTexture(bmp.Width, bmp.Height, BitmapBytes(bmp), depth);
    }

    /// <summary>Locks a bitmap file's data to a GL texture array.</summary>
    /// <param name="width">The width of the bitmap image.</param>
    /// <param name="height">The height of the bitmap image.</param>
    /// <param name="rawBitmap">The raw bitmapdata to use, from <see cref="BitmapBytes(SKBitmap)"/>.</param>
    /// <param name="depth">The depth in a 3D texture.</param>
    public static void LockBitmapToTexture(int width, int height, byte[] rawBitmap, int depth)
    {
        GL.TexSubImage3D(TextureTarget.Texture2DArray, 0, 0, 0, depth, width, height, 1, OpenTK.Graphics.OpenGL4.PixelFormat.Bgra, PixelType.UnsignedByte, rawBitmap);
        GraphicsUtil.CheckError("LockBitmapToTexture 3D");
    }
}
