//
// This file is created by Frenetic LLC.
// This code is Copyright (C) 2017 Frenetic LLC under the terms of a strict license.
// See README.md or LICENSE.txt in the source root for the contents of the license.
// If neither of these are available, assume that neither you nor anyone other than the copyright holder
// hold any right or permission to use this software until such time as the official license is identified.
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FreneticGameCore
{
    /// <summary>
    /// Mersenne-Twister Random implementation.
    /// Based on a few sources, mostly wikipedia for some reason.
    /// </summary>
    public class MTRandom
    {
        /// <summary>
        /// A lower integer bit mask.
        /// </summary>
        private const ulong lower_mask = 0x7FFFFFFF;

        /// <summary>
        /// A higher integer bit mask.
        /// </summary>
        private const ulong upper_mask = ~lower_mask;

        /// <summary>
        /// The current buffer.
        /// </summary>
        private ulong[] mt;

        /// <summary>
        /// The current index in the buffer.
        /// </summary>
        private ulong index;

        /// <summary>
        /// Constructs the MT Random with a current-time-based seed.
        /// </summary>
        public MTRandom()
            : this(624, (ulong)DateTime.UtcNow.ToBinary())
        {
        }

        /// <summary>
        /// Constructs the MT Random with a specific seed.
        /// </summary>
        /// <param name="seed">The seed.</param>
        public MTRandom(ulong seed)
            : this(624, seed)
        {
        }

        /// <summary>
        /// Constructs the MT Random with a specific seed and specific buffer size.
        /// </summary>
        /// <param name="n">The buffer size.</param>
        /// <param name="seed">The seed.</param>
        public MTRandom(ulong n, ulong seed)
        {
            mt = new ulong[n];
            index = n;
            mt[0] = seed;
            for (ulong i = 1; i < n; i++)
            {
                mt[i] = (6364136223846793005UL * (mt[i - 1] ^ (mt[i - 1] >> 62)) + i);
            }
        }

        /// <summary>
        /// Gets a random integer.
        /// </summary>
        public int Next()
        {
            return (int)(NextUL() & lower_mask);
        }

        /// <summary>
        /// Gets a random integer up to a cap.
        /// </summary>
        public int Next(int cap)
        {
            // TODO: Maybe just a modulo?
            return (int)(Next() * ((double)cap / int.MaxValue)); // TODO: Sanity!
        }

        /// <summary>
        /// Gets a random integer between two bounds.
        /// </summary>
        public int Next(int min, int max)
        {
            return Next(max - min) + min;
        }

        /// <summary>
        /// Gets a random double, between 0 and 1.
        /// </summary>
        public double NextDouble()
        {
            return NextUL() / ((double)ulong.MaxValue);
        }

        /// <summary>
        /// Gets a random double, between 0 and cap.
        /// </summary>
        /// <param name="cap">The upper limit.</param>
        public double NextDouble(double cap)
        {
            return NextUL() * (cap / ulong.MaxValue);
        }

        /// <summary>
        /// Gets a random double, between two bounds.
        /// </summary>
        /// <param name="min">The lower limit.</param>
        /// <param name="cap">The upper limit.</param>
        public double NextDouble(double min, double cap)
        {
            return (NextUL() * ((cap - min) / ulong.MaxValue)) + min;
        }

        /// <summary>
        /// Gets a random unsigned long.
        /// </summary>
        public ulong NextUL()
        {
            ulong n = (ulong)mt.LongLength;
            if (index >= n)
            {
                for (ulong i = 0; i < n; i++)
                {
                    ulong x = (mt[i] & upper_mask) + (mt[(i + 1) % n] & lower_mask);
                    ulong xA = x >> 1;

                    if (x % 2 != 0)
                    {
                        xA = xA ^ 0xB5026F5AA96619E9UL;
                    }

                    mt[i] = mt[(i + 156) % n] ^ xA;
                }

                index = 0;
            }

            ulong y = mt[index++];
            y = y ^ ((y >> 29) & 0x5555555555555555UL);
            y = y ^ ((y << 17) & 0x71D67FFFEDA60000UL);
            y = y ^ ((y << 37) & 0xFFF7EEE000000000UL);
            y = y ^ (y >> 43);
            return y;
        }
    }
}
