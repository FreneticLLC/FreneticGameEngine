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
using System.Numerics;
using FreneticUtilities.FreneticToolkit;

namespace FGECore.MathHelpers;

/// <summary>
/// Represents a 3-piece floating point color.
/// Occupies 12 bytes, calculated as 4 * 3, as it has 3 fields (R, G, B) each occupying 4 bytes (a float).
/// </summary>
[StructLayout(LayoutKind.Explicit)]
public struct Color3F
{

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
        readonly get
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
        readonly get
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
        readonly get
        {
            return (int)(B * 255);
        }
        set
        {
            B = value / 255f;
        }
    }

    /// <summary>Returns the average value of the R, G, and B components of this color. That is, (R+G+B)/3.</summary>
    public readonly float AverageValue => (R + G + B) * (1f / 3f);

    /// <summary>Returns the squared strength of this color, equivalent to LengthSquared of a vector. That is, R^2 + G^2 + B^2.</summary>
    public readonly float StrengthSquared => (R * R) + (G * G) + (B * B);

    /// <summary>Constructs a basic <see cref="Color3F"/>.</summary>
    public Color3F(float red, float green, float blue)
    {
        R = red;
        G = green;
        B = blue;
    }

    /// <summary>Constructs a <see cref="Color3F"/> from a <see cref="Location"/> (using X, Y, Z as R, G, B).</summary>
    public Color3F(Location loc)
    {
        R = loc.XF;
        G = loc.YF;
        B = loc.ZF;
    }

    /// <summary>Returns a 12-byte set representation of this color.</summary>
    public readonly byte[] ToBytes()
    {
        byte[] b = new byte[12];
        ToBytes(b, 0);
        return b;
    }

    /// <summary>Returns a <see cref="Location"/> containing the R,G,B float values of this <see cref="Color3F"/>.</summary>
    public readonly Location ToLocation() => new(R, G, B);

    /// <summary>Returns a <see cref="Vector3"/> containing the R,G,B float values of this <see cref="Color3F"/>.</summary>
    public readonly Vector3 ToNumerics() => new(R, G, B);

    /// <summary>
    /// Returns a 12-byte set representation of this color.
    /// Inverts <see cref="FromBytes(byte[], int)"/>.
    /// </summary>
    /// <param name="outputBytes">The output byte array.</param>
    /// <param name="offset">The starting offset in the output array.</param>
    public readonly void ToBytes(byte[] outputBytes, int offset)
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
    public static Color3F FromBytes(byte[] b, int offset = 0)
    {
        return new Color3F(
            PrimitiveConversionHelper.BytesToFloat32(b, offset),
            PrimitiveConversionHelper.BytesToFloat32(b, offset + 4),
            PrimitiveConversionHelper.BytesToFloat32(b, offset + (4 + 4))
            );
    }

    /// <summary>Returns a hex string form of this color, in the format #RRGGBB.</summary>
    public readonly string ToHexString() => $"#{IR:X2}{IG:X2}{IB:X2}";

    /// <summary>Parses a hex string form of a color, in the format #RRGGBB or RRGGBB.</summary>
    public static Color3F FromHexString(string hex)
    {
        if (hex.StartsWith('#'))
        {
            hex = hex[1..];
        }
        if (hex.Length != 6)
        {
            throw new FormatException("Hex color strings must be in the format #RRGGBB");
        }
        int r = Convert.ToInt32(hex[0..2], 16);
        int g = Convert.ToInt32(hex[2..4], 16);
        int b = Convert.ToInt32(hex[4..6], 16);
        return new Color3F(r / 255f, g / 255f, b / 255f);
    }

    /// <summary>Returns a string form of this color.</summary>
    public override readonly string ToString() => $"({R}, {G}, {B})";

    /// <summary>Adds two colors together.</summary>
    public static Color3F operator +(Color3F c1, Color3F c2) => new(c1.R + c2.R, c1.G + c2.G, c1.B + c2.B);

    /// <summary>Multiplies a color by a scale.</summary>
    public static Color3F operator *(Color3F v, float scale) => new(v.R * scale, v.G * scale, v.B * scale);

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
