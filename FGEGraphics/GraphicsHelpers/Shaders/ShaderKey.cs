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

namespace FGEGraphics.GraphicsHelpers.Shaders
{
    /// <summary>
    /// Represents the identifier key for a unique shader.
    /// TODO: Use this in <see cref="ShaderEngine"/>
    /// </summary>
    public class ShaderKey : IEquatable<ShaderKey>
    {
        /// <summary>
        /// The filepath of the shader, excluding the "shaders/" prefix or the file extension suffix.
        /// </summary>
        public string ShaderPath;

        /// <summary>
        /// The filepath of the shader, akin to <see cref="ShaderPath"/>, but for an added geometry shader component.
        /// Often null.
        /// </summary>
        public string GeometryShaderPath;

        /// <summary>
        /// An array of specially defined pre-compiler variable keys.
        /// </summary>
        public string[] Defines = new string[0];

        /// <summary>Gets a unique hash code for the instance.</summary>
        public override int GetHashCode()
        {
            return ShaderPath.GetHashCode()
                + (GeometryShaderPath == null ? 0 : GeometryShaderPath.GetHashCode())
                + Defines.Sum(s => s.GetHashCode());
        }

        /// <summary>Compares the <see cref="ShaderKey"/> instance for equality with another instance.</summary>
        public override bool Equals(object obj)
        {
            if (obj is ShaderKey key)
            {
                return Equals(key);
            }
            return false;
        }

        /// <summary>Compares the <see cref="ShaderKey"/> instance for equality with another instance.</summary>
        public bool Equals(ShaderKey other)
        {
            if (ShaderPath != other.ShaderPath
                || GeometryShaderPath != other.GeometryShaderPath
                || Defines.Length != other.Defines.Length)
            {
                return false;
            }
            for (int i = 0; i < Defines.Length; i++)
            {
                if (Defines[i] != other.Defines[i])
                {
                    return false;
                }
            }
            return true;
        }
    }
}
