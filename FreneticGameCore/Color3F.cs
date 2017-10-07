using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FreneticGameCore
{
    /// <summary>
    /// Represents a 3-piece floating point color.
    /// </summary>
    public struct Color3F
    {
        /// <summary>
        /// Constructs the color 3F.
        /// </summary>
        /// <param name="_r">Red.</param>
        /// <param name="_g">Green.</param>
        /// <param name="_b">Blue.</param>
        public Color3F(float _r, float _g, float _b)
        {
            R = _r;
            G = _g;
            B = _b;
        }

        /// <summary>
        /// The red component.
        /// </summary>
        public float R;

        /// <summary>
        /// The green component.
        /// </summary>
        public float G;

        /// <summary>
        /// The blue component.
        /// </summary>
        public float B;

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
                R = value / 255f;
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
                G = value / 255f;
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
                B = value / 255f;
            }
        }

        /// <summary>
        /// Returns a 12-byte set representation of this color.
        /// </summary>
        /// <returns>The color bytes.</returns>
        public byte[] ToBytes()
        {
            byte[] b = new byte[12];
            Utilities.FloatToBytes(R).CopyTo(b, 0);
            Utilities.FloatToBytes(G).CopyTo(b, 4);
            Utilities.FloatToBytes(B).CopyTo(b, 8);
            return b;
        }

        /// <summary>
        /// Converts a 12-byte set to a color.
        /// </summary>
        /// <param name="b">The byte input.</param>
        /// <returns>The color.</returns>
        public static Color3F FromBytes(byte[] b)
        {
            return new Color3F(
                Utilities.BytesToFloat(Utilities.BytesPartial(b, 0, 4)),
                Utilities.BytesToFloat(Utilities.BytesPartial(b, 4, 4)),
                Utilities.BytesToFloat(Utilities.BytesPartial(b, 8, 4))
                );
        }

        /// <summary>
        /// Returns a string form of this color.
        /// </summary>
        /// <returns>The string form.</returns>
        public override string ToString()
        {
            return "(" + R + ", " + G + ", " + B + ")";
        }

        /// <summary>
        /// Multiplies a color by a scale.
        /// </summary>
        /// <param name="v">The color.</param>
        /// <param name="scale">The scale.</param>
        /// <returns>Result.</returns>
        public static Color3F operator *(Color3F v, float scale)
        {
            return new Color3F(v.R * scale, v.G * scale, v.B * scale);
        }

        /// <summary>
        /// Sample Color3F (1, 1, 1).
        /// </summary>
        public static readonly Color3F White = new Color3F(1, 1, 1);

        /// <summary>
        /// Sample Color3F (0, 0, 0).
        /// </summary>
        public static readonly Color3F Black = new Color3F(0, 0, 0);

        /// <summary>
        /// Sample Color3F (1, 0, 0).
        /// </summary>
        public static readonly Color3F Red = new Color3F(1, 0, 0);

        /// <summary>
        /// Sample Color3F (0, 1, 0).
        /// </summary>
        public static readonly Color3F Green = new Color3F(0, 1, 0);

        /// <summary>
        /// Sample Color3F (0, 0, 1).
        /// </summary>
        public static readonly Color3F Blue = new Color3F(0, 0, 1);
    }
}
