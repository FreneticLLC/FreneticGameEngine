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
        private const ulong lower_mask = 0x7FFFFFFF;

        private const ulong upper_mask = ~lower_mask;

        private ulong[] mt;

        private ulong index;

        public MTRandom()
            : this(624, (ulong)DateTime.UtcNow.ToBinary())
        {
        }

        public MTRandom(ulong seed)
            : this(624, seed)
        {
        }

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

        public int Next()
        {
            return (int)(NextUL() & lower_mask);
        }

        public int Next(int cap)
        {
            return (int)(Next() / (int.MaxValue / (double)cap)); // TODO: Sanity!
        }

        public int Next(int min, int max)
        {
            return Next(max - min) + min;
        }

        public double NextDouble()
        {
            return NextUL() / ((double)ulong.MaxValue);
        }

        public ulong NextUL()
        {
            ulong n = (ulong)mt.Length;
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
