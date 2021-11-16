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

namespace FGEGraphics.GraphicsHelpers.Textures
{
    /// <summary>Event arguments for a texture being loaded.</summary>
    public class TextureLoadedEventArgs : EventArgs
    {
        /// <summary>Constructs a texture loaded event argument set.</summary>
        /// <param name="_texture">The texture that was loaded.</param>
        public TextureLoadedEventArgs(Texture _texture)
        {
            LoadedTexture = _texture;
        }

        /// <summary>The texture that was loaded.</summary>
        public Texture LoadedTexture;
    }
}
