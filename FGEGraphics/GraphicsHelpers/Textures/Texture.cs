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
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FGECore;
using FGECore.ConsoleHelpers;
using FGECore.CoreSystems;
using FGECore.FileSystems;
using FGECore.MathHelpers;
using OpenTK.Graphics.OpenGL4;

namespace FGEGraphics.GraphicsHelpers.Textures;

/// <summary>Wraps an OpenGL texture.</summary>
public class Texture : IEquatable<Texture>
{
    /// <summary>The texture engine that owns this texture.</summary>
    public TextureEngine Engine;

    /// <summary>The full name of the texture.</summary>
    public string Name;

    /// <summary>The texture that this texture was remapped to, if any.</summary>
    public Texture RemappedTo;

    /// <summary>The internal OpenGL texture ID.</summary>
    public int InternalTexture = -1;

    /// <summary>The original OpenGL texture ID that formed this texture.</summary>
    public int OriginalInternalID = -1;

    /// <summary>Whether the texture loaded properly.</summary>
    public bool LoadedProperly = false;

    /// <summary>The width of the texture.</summary>
    public int Width;

    /// <summary>The height of the texture.</summary>
    public int Height;

    /// <summary>Removes the texture from OpenGL.</summary>
    public void Destroy()
    {
        if (LoadedProperly && OriginalInternalID > -1 && GL.IsTexture(OriginalInternalID))
        {
            GL.DeleteTexture(OriginalInternalID);
        }
        LoadedProperly = false;
        InternalTexture = -1;
        OriginalInternalID = -1;
    }

    /// <summary>Removes the texture from the system.</summary>
    public void Remove()
    {
        Destroy();
        if (Engine.LoadedTextures.TryGetValue(Name, out Texture text) && text == this)
        {
            Engine.LoadedTextures.Remove(Name);
        }
    }

    /// <summary>Saves the texture to a bitmap.</summary>
    /// <param name="flip">Whether to flip the Y.</param>
    public Bitmap SaveToBMP(bool flip = false)
    {
        GL.BindTexture(TextureTarget.Texture2D, OriginalInternalID);
        Bitmap bmp = new(Width, Height);
        BitmapData data = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), System.Drawing.Imaging.ImageLockMode.WriteOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
        GL.GetTexImage(TextureTarget.Texture2D, 0, OpenTK.Graphics.OpenGL4.PixelFormat.Bgra, PixelType.UnsignedByte, data.Scan0);
        bmp.UnlockBits(data);
        if (flip)
        {
            bmp.RotateFlip(RotateFlipType.RotateNoneFlipY);
        }
        return bmp;
    }

    /// <summary>The tick time this texture was last bound.</summary>
    public double LastBindTime = 0;

    /// <summary>Checks if the texture is valid, and replaces it if needed.</summary>
    public void CheckValid()
    {
        if (OriginalInternalID == -1)
        {
            if (RemappedTo is not null)
            {
                RemappedTo.CheckValid();
                InternalTexture = RemappedTo.OriginalInternalID;
            }
            else
            {
                LoadedProperly = false;
                Texture temp = Engine.GetTexture(Name);
                InternalTexture = temp.InternalTexture;
                if (temp.LoadedProperly)
                {
                    OriginalInternalID = temp.OriginalInternalID;
                }
            }
        }
    }

    /// <summary>Binds this texture to OpenGL.</summary>
    public void Bind()
    {
        LastBindTime = Engine.CurrentTime;
        CheckValid();
        GL.BindTexture(TextureTarget.Texture2D, InternalTexture);
    }

    /// <summary>Gets the name of the texture.</summary>
    /// <returns>The name.</returns>
    public override string ToString()
    {
        return Name;
    }

    /// <summary>Returns a hash code for this <see cref="Texture"/>.</summary>
    public override int GetHashCode()
    {
        return Name.GetHashCode();
    }

    /// <summary>Determines if the specified <see cref="Texture"/> is equal to this <see cref="Texture"/>.</summary>
    public override bool Equals(object obj)
    {
        return obj is Texture texture && Equals(texture);
    }

    /// <summary>Determines if the specified object is equal to this <see cref="Texture"/>.</summary>
    public bool Equals(Texture other)
    {
        return Name == other.Name;
    }
}
