using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FreneticGameCore
{
    /// <summary>
    /// Represents a 4-piece floating point color.
    /// </summary>
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
        /// The alpha component.
        /// </summary>
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
                A = value / 255f;
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
