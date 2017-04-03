using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FreneticGameCore
{
    // mcmonkey - NOTE TO READERS: This class was found elsewhere. Can't locate a link to it.
    // mcmonkey - Original header is below, though I removed some unicode from the name to avoid file errors.

    // mcmonkey - ORIGINAL HEADER BEGIN

    // SimplexNoise for C#
    // Author: Heikki Tormala

    //This is free and unencumbered software released into the public domain.

    //Anyone is free to copy, modify, publish, use, compile, sell, or
    //distribute this software, either in source code form or as a compiled
    //binary, for any purpose, commercial or non-commercial, and by any
    //means.

    //In jurisdictions that recognize copyright laws, the author or authors
    //of this software dedicate any and all copyright interest in the
    //software to the public domain. We make this dedication for the benefit
    //of the public at large and to the detriment of our heirs and
    //successors. We intend this dedication to be an overt act of
    //relinquishment in perpetuity of all present and future rights to this
    //software under copyright law.

    //THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
    //EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
    //MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.
    //IN NO EVENT SHALL THE AUTHORS BE LIABLE FOR ANY CLAIM, DAMAGES OR
    //OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE,
    //ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
    //OTHER DEALINGS IN THE SOFTWARE.

    //For more information, please refer to http://unlicense.org/

    // mcmonkey - ORIGINAL HEADER END

    // This class all mcmonkey
    /// <summary>
    /// Simplex noise helper.
    /// </summary>
    public static class SimplexNoise
    {
        /// <summary>
        /// Generate 2D simplex noise.
        /// </summary>
        /// <param name="x">The X coordinate.</param>
        /// <param name="y">The Y coordinate.</param>
        /// <returns>The noise value.</returns>
        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public static double Generate(double x, double y)
        {
            return (SimplexNoiseInternal.Generate(Math.Abs(x), Math.Abs(y)) + 1.0) * 0.5;
        }

        /// <summary>
        /// Generate 3D simplex noise.
        /// </summary>
        /// <param name="x">The X coordinate.</param>
        /// <param name="y">The Y coordinate.</param>
        /// <param name="z">The Z coordinate.</param>
        /// <returns>The noise value.</returns>
        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public static double Generate(double x, double y, double z)
        {
            return (SimplexNoiseInternal.Generate(Math.Abs(x), Math.Abs(y), Math.Abs(z)) + 1.0) * 0.5;
        }
    }

    // mcmonkey - Below code changed from float to double.

    /// <summary>
    /// Implementation of the Perlin simplex noise, an improved Perlin noise algorithm.
    /// Based loosely on SimplexNoise1234 by Stefan Gustavson http://staffwww.itn.liu.se/~stegu/aqsis/aqsis-newnoise/
    /// </summary>
    public class SimplexNoiseInternal // mcmonkey - class rename
    {
        /// <summary>
        /// 1D simplex noise
        /// </summary>
        /// <param name="x">.</param>
        /// <returns>.</returns>
        public static double Generate(double x)
        {
            long i0 = FastFloor(x);
            long i1 = i0 + 1;
            double x0 = x - i0;
            double x1 = x0 - 1.0f;

            double n0, n1;

            double t0 = 1.0f - x0 * x0;
            t0 *= t0;
            n0 = t0 * t0 * Grad(perm[i0 & 0xff], x0);

            double t1 = 1.0f - x1 * x1;
            t1 *= t1;
            n1 = t1 * t1 * Grad(perm[i1 & 0xff], x1);
            // The maximum value of this noise is 8*(3/4)^4 = 2.53125
            // A factor of 0.395 scales to fit exactly within [-1,1]
            return 0.395f * (n0 + n1);
        }

        /// <summary>
        /// 2D simplex noise
        /// </summary>
        /// <param name="x">.</param>
        /// <param name="y">.</param>
        /// <returns>.</returns>
        public static double Generate(double x, double y)
        {
            const double F2 = 0.366025403f; // F2 = 0.5*(sqrt(3.0)-1.0)
            const double G2 = 0.211324865f; // G2 = (3.0-Math.sqrt(3.0))/6.0

            double n0, n1, n2; // Noise contributions from the three corners

            // Skew the input space to determine which simplex cell we're in
            double s = (x + y) * F2; // Hairy factor for 2D
            double xs = x + s;
            double ys = y + s;
            long i = FastFloor(xs);
            long j = FastFloor(ys);

            double t = (double)(i + j) * G2;
            double X0 = i - t; // Unskew the cell origin back to (x,y) space
            double Y0 = j - t;
            double x0 = x - X0; // The x,y distances from the cell origin
            double y0 = y - Y0;

            // For the 2D case, the simplex shape is an equilateral triangle.
            // Determine which simplex we are in.
            int i1, j1; // Offsets for second (middle) corner of simplex in (i,j) coords
            if (x0 > y0) { i1 = 1; j1 = 0; } // lower triangle, XY order: (0,0)->(1,0)->(1,1)
            else { i1 = 0; j1 = 1; }      // upper triangle, YX order: (0,0)->(0,1)->(1,1)

            // A step of (1,0) in (i,j) means a step of (1-c,-c) in (x,y), and
            // a step of (0,1) in (i,j) means a step of (-c,1-c) in (x,y), where
            // c = (3-sqrt(3))/6

            double x1 = x0 - i1 + G2; // Offsets for middle corner in (x,y) unskewed coords
            double y1 = y0 - j1 + G2;
            double x2 = x0 - 1.0f + 2.0f * G2; // Offsets for last corner in (x,y) unskewed coords
            double y2 = y0 - 1.0f + 2.0f * G2;

            // Wrap the integer indices at 256, to avoid indexing perm[] out of bounds
            long ii = i % 256;
            long jj = j % 256;

            // Calculate the contribution from the three corners
            double t0 = 0.5f - x0 * x0 - y0 * y0;
            if (t0 < 0.0f) n0 = 0.0f;
            else
            {
                t0 *= t0;
                n0 = t0 * t0 * Grad(perm[ii + perm[jj]], x0, y0);
            }

            double t1 = 0.5f - x1 * x1 - y1 * y1;
            if (t1 < 0.0f) n1 = 0.0f;
            else
            {
                t1 *= t1;
                n1 = t1 * t1 * Grad(perm[ii + i1 + perm[jj + j1]], x1, y1);
            }

            double t2 = 0.5f - x2 * x2 - y2 * y2;
            if (t2 < 0.0f) n2 = 0.0f;
            else
            {
                t2 *= t2;
                n2 = t2 * t2 * Grad(perm[ii + 1 + perm[jj + 1]], x2, y2);
            }

            // Add contributions from each corner to get the final noise value.
            // The result is scaled to return values in the interval [-1,1].
            return 45.1f * (n0 + n1 + n2); // TODO: The scale factor is preliminary! // mcmonkey - improve scaling factor from 40.0
        }


        /// <summary>
        /// 3D simplex noise
        /// </summary>
        /// <param name="x">.</param>
        /// <param name="y">.</param>
        /// <param name="z">.</param>
        /// <returns>.</returns>
        public static double Generate(double x, double y, double z)
        {
            // Simple skewing factors for the 3D case
            const double F3 = 0.333333333f;
            const double G3 = 0.166666667f;

            double n0, n1, n2, n3; // Noise contributions from the four corners

            // Skew the input space to determine which simplex cell we're in
            double s = (x + y + z) * F3; // Very nice and simple skew factor for 3D
            double xs = x + s;
            double ys = y + s;
            double zs = z + s;
            long i = FastFloor(xs);
            long j = FastFloor(ys);
            long k = FastFloor(zs);

            double t = (double)(i + j + k) * G3;
            double X0 = i - t; // Unskew the cell origin back to (x,y,z) space
            double Y0 = j - t;
            double Z0 = k - t;
            double x0 = x - X0; // The x,y,z distances from the cell origin
            double y0 = y - Y0;
            double z0 = z - Z0;

            // For the 3D case, the simplex shape is a slightly irregular tetrahedron.
            // Determine which simplex we are in.
            long i1, j1, k1; // Offsets for second corner of simplex in (i,j,k) coords
            long i2, j2, k2; // Offsets for third corner of simplex in (i,j,k) coords

            /* This code would benefit from a backport from the GLSL version! */
            if (x0 >= y0)
            {
                if (y0 >= z0)
                { i1 = 1; j1 = 0; k1 = 0; i2 = 1; j2 = 1; k2 = 0; } // X Y Z order
                else if (x0 >= z0) { i1 = 1; j1 = 0; k1 = 0; i2 = 1; j2 = 0; k2 = 1; } // X Z Y order
                else { i1 = 0; j1 = 0; k1 = 1; i2 = 1; j2 = 0; k2 = 1; } // Z X Y order
            }
            else
            { // x0<y0
                if (y0 < z0) { i1 = 0; j1 = 0; k1 = 1; i2 = 0; j2 = 1; k2 = 1; } // Z Y X order
                else if (x0 < z0) { i1 = 0; j1 = 1; k1 = 0; i2 = 0; j2 = 1; k2 = 1; } // Y Z X order
                else { i1 = 0; j1 = 1; k1 = 0; i2 = 1; j2 = 1; k2 = 0; } // Y X Z order
            }

            // A step of (1,0,0) in (i,j,k) means a step of (1-c,-c,-c) in (x,y,z),
            // a step of (0,1,0) in (i,j,k) means a step of (-c,1-c,-c) in (x,y,z), and
            // a step of (0,0,1) in (i,j,k) means a step of (-c,-c,1-c) in (x,y,z), where
            // c = 1/6.

            double x1 = x0 - i1 + G3; // Offsets for second corner in (x,y,z) coords
            double y1 = y0 - j1 + G3;
            double z1 = z0 - k1 + G3;
            double x2 = x0 - i2 + 2.0f * G3; // Offsets for third corner in (x,y,z) coords
            double y2 = y0 - j2 + 2.0f * G3;
            double z2 = z0 - k2 + 2.0f * G3;
            double x3 = x0 - 1.0f + 3.0f * G3; // Offsets for last corner in (x,y,z) coords
            double y3 = y0 - 1.0f + 3.0f * G3;
            double z3 = z0 - 1.0f + 3.0f * G3;

            // Wrap the integer indices at 256, to avoid indexing perm[] out of bounds
            long ii = Mod(i, 256);
            long jj = Mod(j, 256);
            long kk = Mod(k, 256);

            // Calculate the contribution from the four corners
            double t0 = 0.6f - x0 * x0 - y0 * y0 - z0 * z0;
            if (t0 < 0.0f) n0 = 0.0f;
            else
            {
                t0 *= t0;
                n0 = t0 * t0 * Grad(perm[ii + perm[jj + perm[kk]]], x0, y0, z0);
            }

            double t1 = 0.6f - x1 * x1 - y1 * y1 - z1 * z1;
            if (t1 < 0.0f) n1 = 0.0f;
            else
            {
                t1 *= t1;
                n1 = t1 * t1 * Grad(perm[ii + i1 + perm[jj + j1 + perm[kk + k1]]], x1, y1, z1);
            }

            double t2 = 0.6f - x2 * x2 - y2 * y2 - z2 * z2;
            if (t2 < 0.0f) n2 = 0.0f;
            else
            {
                t2 *= t2;
                n2 = t2 * t2 * Grad(perm[ii + i2 + perm[jj + j2 + perm[kk + k2]]], x2, y2, z2);
            }

            double t3 = 0.6f - x3 * x3 - y3 * y3 - z3 * z3;
            if (t3 < 0.0f) n3 = 0.0f;
            else
            {
                t3 *= t3;
                n3 = t3 * t3 * Grad(perm[ii + 1 + perm[jj + 1 + perm[kk + 1]]], x3, y3, z3);
            }

            // Add contributions from each corner to get the final noise value.
            // The result is scaled to stay just inside [-1,1]
            return 32.4f * (n0 + n1 + n2 + n3); // TODO: The scale factor is preliminary! // mcmonkey - improve scaling factor from 32.0
        }

        private static byte[] perm = new byte[512] { 151,160,137,91,90,15,
              131,13,201,95,96,53,194,233,7,225,140,36,103,30,69,142,8,99,37,240,21,10,23,
              190, 6,148,247,120,234,75,0,26,197,62,94,252,219,203,117,35,11,32,57,177,33,
              88,237,149,56,87,174,20,125,136,171,168, 68,175,74,165,71,134,139,48,27,166,
              77,146,158,231,83,111,229,122,60,211,133,230,220,105,92,41,55,46,245,40,244,
              102,143,54, 65,25,63,161, 1,216,80,73,209,76,132,187,208, 89,18,169,200,196,
              135,130,116,188,159,86,164,100,109,198,173,186, 3,64,52,217,226,250,124,123,
              5,202,38,147,118,126,255,82,85,212,207,206,59,227,47,16,58,17,182,189,28,42,
              223,183,170,213,119,248,152, 2,44,154,163, 70,221,153,101,155,167, 43,172,9,
              129,22,39,253, 19,98,108,110,79,113,224,232,178,185, 112,104,218,246,97,228,
              251,34,242,193,238,210,144,12,191,179,162,241, 81,51,145,235,249,14,239,107,
              49,192,214, 31,181,199,106,157,184, 84,204,176,115,121,50,45,127, 4,150,254,
              138,236,205,93,222,114,67,29,24,72,243,141,128,195,78,66,215,61,156,180,
              151,160,137,91,90,15,
              131,13,201,95,96,53,194,233,7,225,140,36,103,30,69,142,8,99,37,240,21,10,23,
              190, 6,148,247,120,234,75,0,26,197,62,94,252,219,203,117,35,11,32,57,177,33,
              88,237,149,56,87,174,20,125,136,171,168, 68,175,74,165,71,134,139,48,27,166,
              77,146,158,231,83,111,229,122,60,211,133,230,220,105,92,41,55,46,245,40,244,
              102,143,54, 65,25,63,161, 1,216,80,73,209,76,132,187,208, 89,18,169,200,196,
              135,130,116,188,159,86,164,100,109,198,173,186, 3,64,52,217,226,250,124,123,
              5,202,38,147,118,126,255,82,85,212,207,206,59,227,47,16,58,17,182,189,28,42,
              223,183,170,213,119,248,152, 2,44,154,163, 70,221,153,101,155,167, 43,172,9,
              129,22,39,253, 19,98,108,110,79,113,224,232,178,185, 112,104,218,246,97,228,
              251,34,242,193,238,210,144,12,191,179,162,241, 81,51,145,235,249,14,239,107,
              49,192,214, 31,181,199,106,157,184, 84,204,176,115,121,50,45,127, 4,150,254,
              138,236,205,93,222,114,67,29,24,72,243,141,128,195,78,66,215,61,156,180
            };

        private static long FastFloor(double x)
        {
            return (x > 0) ? ((long)x) : (((long)x) - 1);
        }

        private static long Mod(long x, long m)
        {
            long a = x % m;
            return a < 0 ? a + m : a;
        }

        private static double Grad(long hash, double x) // mcmonkey - Name fix
        {
            long h = hash & 15;
            double grad = 1.0f + (h & 7);   // Gradient value 1.0, 2.0, ..., 8.0
            if ((h & 8) != 0) grad = -grad;         // Set a random sign for the gradient
            return (grad * x);           // Multiply the gradient with the distance
        }

        private static double Grad(long hash, double x, double y) // mcmonkey - Name fix
        {
            long h = hash & 7;      // Convert low 3 bits of hash code
            double u = h < 4 ? x : y;  // into 8 simple gradient directions,
            double v = h < 4 ? y : x;  // and compute the dot product with (x,y).
            return ((h & 1) != 0 ? -u : u) + ((h & 2) != 0 ? -2.0f * v : 2.0f * v);
        }

        private static double Grad(long hash, double x, double y, double z) // mcmonkey - Name fix
        {
            long h = hash & 15;     // Convert low 4 bits of hash code into 12 simple
            double u = h < 8 ? x : y; // gradient directions, and compute dot product.
            double v = h < 4 ? y : h == 12 || h == 14 ? x : z; // Fix repeats at h = 12 to 15
            return ((h & 1) != 0 ? -u : u) + ((h & 2) != 0 ? -v : v);
        }

        private static double Grad(long hash, double x, double y, double z, double t) // mcmonkey - Name fix
        {
            long h = hash & 31;      // Convert low 5 bits of hash code into 32 simple
            double u = h < 24 ? x : y; // gradient directions, and compute dot product.
            double v = h < 16 ? y : z;
            double w = h < 8 ? z : t;
            return ((h & 1) != 0 ? -u : u) + ((h & 2) != 0 ? -v : v) + ((h & 4) != 0 ? -w : w);
        }
    }
}
