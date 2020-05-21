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
    /// Event arguments for a texture being loaded.
    /// </summary>
    public class TextureLoadedEventArgs : EventArgs
    {
        /// <summary>
        /// Constructs a texture loaded event argument set.
        /// </summary>
        /// <param name="_texture">The texture that was loaded.</param>
        public TextureLoadedEventArgs(Texture _texture)
        {
            LoadedTexture = _texture;
        }

        /// <summary>
        /// The texture that was loaded.
        /// </summary>
        public Texture LoadedTexture;
    }
}
