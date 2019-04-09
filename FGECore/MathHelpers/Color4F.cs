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
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;
using FGECore.UtilitySystems;
using FreneticUtilities.FreneticToolkit;

namespace FGECore.MathHelpers
{
    /// <summary>
    /// Represents a 4-piece floating point color.
    /// Occupies 16 bytes, calculated as 4 * 4, as it has 4 fields (R, G, B, A) each occupying 4 bytes (a float).
    /// </summary>
    [StructLayout(LayoutKind.Explicit)]
    public struct Color4F
    {
        /// <summary>
        /// Constructs the color 4F with full alpha.
        /// </summary>
        /// <param name="_r">Red.</param>
        /// <param name="_g">Green.</param>
        /// <param name="_b">Blue.</param>
        public Color4F(float _r, float _g, float _b)
        {
            R = _r;
            G = _g;
            B = _b;
            A = 1;
        }

        /// <summary>
        /// Constructs the color 4F with specific alpha.
        /// </summary>
        /// <param name="_r">Red.</param>
        /// <param name="_g">Green.</param>
        /// <param name="_b">Blue.</param>
        /// <param name="_a">Alpha.</param>
        public Color4F(float _r, float _g, float _b ,float _a)
        {
            R = _r;
            G = _g;
            B = _b;
            A = _a;
        }

        /// <summary>
        /// Constructs the color 4F with full alpha.
        /// </summary>
        /// <param name="color">The 3-piece color.</param>
        public Color4F(Color3F color)
        {
            R = color.R;
            G = color.G;
            B = color.B;
            A = 1;
        }

        /// <summary>
        /// Constructs the color 4F with specific alpha.
        /// </summary>
        /// <param name="color">The 3-piece color.</param>
        /// <param name="_a">Alpha.</param>
        public Color4F(Color3F color, float _a)
        {
            R = color.R;
            G = color.G;
            B = color.B;
            A = _a;
        }

        /// <summary>
        /// The red component.
        /// </summary>
        [FieldOffset(0)]
        public float R;

        /// <summary>
        /// The green component.
        /// </summary>
        [FieldOffset(4)]
        public float G;

        /// <summary>
        /// The blue component.
        /// </summary>
        [FieldOffset(8)]
        public float B;

        /// <summary>
        /// The alpha component.
        /// </summary>
        [FieldOffset(12)]
        public float A;

        /// <summary>
        /// Integer R.
        /// </summary>
        public int IR
        {
            get
            {
                return (int)(R * 255);
            }
            set
            {
                R = value * BYTE_TO_FLOAT;
            }
        }

        /// <summary>
        /// Integer G.
        /// </summary>
        public int IG
        {
            get
            {
                return (int)(G * 255);
            }
            set
            {
                G = value * BYTE_TO_FLOAT;
            }
        }

        /// <summary>
        /// Integer B.
        /// </summary>
        public int IB
        {
            get
            {
                return (int)(B * 255);
            }
            set
            {
                B = value * BYTE_TO_FLOAT;
            }
        }

        /// <summary>
        /// Integer A.
        /// </summary>
        public int IA
        {
            get
            {
                return (int)(A * 255);
            }
            set
            {
                A = value * BYTE_TO_FLOAT;
            }
        }

        /// <summary>
        /// Gets or sets the RGB color object for this color.
        /// </summary>
        public Color3F RGB
        {
            get
            {
                return new Color3F(R, G, B);
            }
            set
            {
                R = value.R;
                G = value.G;
                B = value.B;
            }
        }

        /// <summary>
        /// Returns a string form of this color.
        /// </summary>
        /// <returns>The string form.</returns>
        public override string ToString()
        {
            return "(" + R + ", " + G + ", " + B + ", " + A + ")";
        }

        /// <summary>
        /// Returns a 16-byte set representation of this color.
        /// </summary>
        /// <returns>The color bytes.</returns>
        public byte[] ToBytes()
        {
            byte[] b = new byte[16];
            ToBytes(b, 0);
            return b;
        }

        /// <summary>
        /// Returns a 16-byte set representation of this color.
        /// Inverts <see cref="FromBytes(byte[], int)"/>.
        /// </summary>
        /// <param name="outputBytes">The output byte array.</param>
        /// <param name="offset">The starting offset in the output array.</param>
        public void ToBytes(byte[] outputBytes, int offset)
        {
            PrimitiveConversionHelper.Float32ToBytes(R, outputBytes, offset);
            PrimitiveConversionHelper.Float32ToBytes(G, outputBytes, offset + 4);
            PrimitiveConversionHelper.Float32ToBytes(B, outputBytes, offset + (4 + 4));
            PrimitiveConversionHelper.Float32ToBytes(A, outputBytes, offset + (4 + 4 + 4));
        }

        /// <summary>
        /// Converts a 16-byte set to a color.
        /// Inverts <see cref="ToBytes(byte[], int)"/>.
        /// </summary>
        /// <param name="b">The byte input.</param>
        /// <param name="offset">The offset in the byte array.</param>
        /// <returns>The color.</returns>
        public static Color4F FromBytes(byte[] b, int offset = 0)
        {
            return new Color4F(
                PrimitiveConversionHelper.BytesToFloat32(b, offset),
                PrimitiveConversionHelper.BytesToFloat32(b, offset + 4),
                PrimitiveConversionHelper.BytesToFloat32(b, offset + (4 + 4)),
                PrimitiveConversionHelper.BytesToFloat32(b, offset + (4 + 4 + 4))
                );
        }

        /// <summary>
        /// A float of 1/255.
        /// </summary>
        public const float BYTE_TO_FLOAT = 1f / 255f;

        /// <summary>
        /// Constructs a Color4F from 4 bytes.
        /// Built for quick conversion of byte-based color types, EG System.Drawing.Color!
        /// </summary>
        /// <param name="r">Red.</param>
        /// <param name="g">Green.</param>
        /// <param name="b">Blue.</param>
        /// <param name="a">Alpha.</param>
        /// <returns>The color.</returns>
        public static Color4F FromArgb(int a, int r, int g, int b)
        {
            return new Color4F(r * BYTE_TO_FLOAT, g * BYTE_TO_FLOAT, b * BYTE_TO_FLOAT, a * BYTE_TO_FLOAT);
        }

        /// <summary>
        /// Constructs a Color4F from 3 bytes.
        /// Built for quick conversion of byte-based color types, EG System.Drawing.Color!
        /// </summary>
        /// <param name="r">Red.</param>
        /// <param name="g">Green.</param>
        /// <param name="b">Blue.</param>
        /// <returns>The color.</returns>
        public static Color4F FromArgb(int r, int g, int b)
        {
            return new Color4F(r * BYTE_TO_FLOAT, g * BYTE_TO_FLOAT, b * BYTE_TO_FLOAT, 1);
        }

        /// <summary>
        /// Sample Color4F (1, 1, 1).
        /// </summary>
        public static readonly Color4F White = new Color4F(1, 1, 1);

        /// <summary>
        /// Sample Color4F (0, 0, 0).
        /// </summary>
        public static readonly Color4F Black = new Color4F(0, 0, 0);

        /// <summary>
        /// Sample Color4F (1, 0, 0).
        /// </summary>
        public static readonly Color4F Red = new Color4F(1, 0, 0);

        /// <summary>
        /// Sample Color4F (0, 1, 0).
        /// </summary>
        public static readonly Color4F Green = new Color4F(0, 1, 0);

        /// <summary>
        /// Sample Color4F (0, 0, 1).
        /// </summary>
        public static readonly Color4F Blue = new Color4F(0, 0, 1);
    }
}
