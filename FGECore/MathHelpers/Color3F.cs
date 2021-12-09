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
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using FreneticUtilities.FreneticToolkit;
using FGECore.UtilitySystems;

namespace FGECore.MathHelpers
{
    /// <summary>
    /// Represents a 3-piece floating point color.
    /// Occupies 12 bytes, calculated as 4 * 3, as it has 3 fields (R, G, B) each occupying 4 bytes (a float).
    /// </summary>
    [StructLayout(LayoutKind.Explicit)]
    public struct Color3F
    {
        /// <summary>Constructs the color 3F.</summary>
        /// <param name="_r">Red.</param>
        /// <param name="_g">Green.</param>
        /// <param name="_b">Blue.</param>
        public Color3F(float _r, float _g, float _b)
        {
            R = _r;
            G = _g;
            B = _b;
        }

        /// <summary>The red component.</summary>
        [FieldOffset(0)]
        public float R;

        /// <summary>The green component.</summary>
        [FieldOffset(4)]
        public float G;

        /// <summary>The blue component.</summary>
        [FieldOffset(8)]
        public float B;

        /// <summary>Integer R.</summary>
        public int IR
        {
            get
            {
                return (int)(R * 255);
            }
            set
            {
                R = value / 255f;
            }
        }

        /// <summary>Integer G.</summary>
        public int IG
        {
            get
            {
                return (int)(G * 255);
            }
            set
            {
                G = value / 255f;
            }
        }

        /// <summary>Integer B.</summary>
        public int IB
        {
            get
            {
                return (int)(B * 255);
            }
            set
            {
                B = value / 255f;
            }
        }

        /// <summary>Returns a 12-byte set representation of this color.</summary>
        /// <returns>The color bytes.</returns>
        public byte[] ToBytes()
        {
            byte[] b = new byte[12];
            ToBytes(b, 0);
            return b;
        }

        /// <summary>Returns a <see cref="Location"/> containing the R,G,B float values of this <see cref="Color3F"/>.</summary>
        /// <returns>The location value.</returns>
        public Location ToLocation()
        {
            return new Location(R, G, B);
        }

        /// <summary>
        /// Returns a 12-byte set representation of this color.
        /// Inverts <see cref="FromBytes(byte[], int)"/>.
        /// </summary>
        /// <param name="outputBytes">The output byte array.</param>
        /// <param name="offset">The starting offset in the output array.</param>
        public void ToBytes(byte[] outputBytes, int offset)
        {
            PrimitiveConversionHelper.Float32ToBytes(R, outputBytes, offset);
            PrimitiveConversionHelper.Float32ToBytes(G, outputBytes, offset + 4);
            PrimitiveConversionHelper.Float32ToBytes(B, outputBytes, offset + (4 + 4));
        }

        /// <summary>
        /// Converts a 12-byte set to a color.
        /// Inverts <see cref="ToBytes(byte[], int)"/>.
        /// </summary>
        /// <param name="b">The byte input.</param>
        /// <param name="offset">The offset in the byte array.</param>
        /// <returns>The color.</returns>
        public static Color3F FromBytes(byte[] b, int offset = 0)
        {
            return new Color3F(
                PrimitiveConversionHelper.BytesToFloat32(b, offset),
                PrimitiveConversionHelper.BytesToFloat32(b, offset + 4),
                PrimitiveConversionHelper.BytesToFloat32(b, offset + (4 + 4))
                );
        }

        /// <summary>Returns a string form of this color.</summary>
        /// <returns>The string form.</returns>
        public override string ToString()
        {
            return "(" + R + ", " + G + ", " + B + ")";
        }

        /// <summary>Multiplies a color by a scale.</summary>
        /// <param name="v">The color.</param>
        /// <param name="scale">The scale.</param>
        /// <returns>Result.</returns>
        public static Color3F operator *(Color3F v, float scale)
        {
            return new Color3F(v.R * scale, v.G * scale, v.B * scale);
        }

        /// <summary>Sample Color3F (1, 1, 1).</summary>
        public static readonly Color3F White = new(1, 1, 1);

        /// <summary>Sample Color3F (0, 0, 0).</summary>
        public static readonly Color3F Black = new(0, 0, 0);

        /// <summary>Sample Color3F (1, 0, 0).</summary>
        public static readonly Color3F Red = new(1, 0, 0);

        /// <summary>Sample Color3F (0, 1, 0).</summary>
        public static readonly Color3F Green = new(0, 1, 0);

        /// <summary>Sample Color3F (0, 0, 1).</summary>
        public static readonly Color3F Blue = new(0, 0, 1);
    }
}
