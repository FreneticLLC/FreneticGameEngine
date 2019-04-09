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

namespace FGECore.UtilitySystems
{
    /// <summary>
    /// Mersenne-Twister Random implementation.
    /// Based on a few sources, mostly wikipedia for some reason.
    /// </summary>
    public class MTRandom
    {
        /// <summary>
        /// Holder for internal data for <see cref="MTRandom"/> instances.
        /// </summary>
        public struct InternalData
        {
            /// <summary>
            /// The default or reference buffer size.
            /// </summary>
            public const ulong REF_BUF_SIZE = 624;

            /// <summary>
            /// A lower integer bit mask.
            /// </summary>
            public const ulong LOWER_MASK = 0x7FFFFFFF;

            /// <summary>
            /// A higher integer bit mask.
            /// </summary>
            public const ulong UPPER_MASK = ~LOWER_MASK;

            /// <summary>
            /// The current buffer.
            /// </summary>
            public ulong[] Buffer;

            /// <summary>
            /// The current index in the buffer.
            /// </summary>
            public ulong BufferIndex;
        }

        /// <summary>
        /// Internal data for this random instance.
        /// </summary>
        public InternalData Internal;

        /// <summary>
        /// Constructs the MT Random with a current-time-based seed, and a default buffer size (of <see cref="InternalData.REF_BUF_SIZE"/>).
        /// </summary>
        public MTRandom()
            : this(InternalData.REF_BUF_SIZE, (ulong)DateTime.UtcNow.ToBinary())
        {
        }

        /// <summary>
        /// Constructs the MT Random with a specific seed, and a default buffer size (of <see cref="InternalData.REF_BUF_SIZE"/>).
        /// </summary>
        /// <param name="seed">The seed.</param>
        public MTRandom(ulong seed)
            : this(InternalData.REF_BUF_SIZE, seed)
        {
        }

        /// <summary>
        /// Constructs the MT Random with a specific seed and specific buffer size, and a default buffer size.
        /// </summary>
        /// <param name="bufferSize">The buffer size.</param>
        /// <param name="seed">The seed.</param>
        public MTRandom(ulong bufferSize, ulong seed)
        {
            Internal.Buffer = new ulong[bufferSize];
            Internal.BufferIndex = bufferSize;
            Internal.Buffer[0] = seed;
            for (ulong i = 1; i < bufferSize; i++)
            {
                Internal.Buffer[i] = 6364136223846793005UL * (Internal.Buffer[i - 1] ^ (Internal.Buffer[i - 1] >> 62)) + i;
            }
        }

        /// <summary>
        /// Gets a random positive integer (from 0 to <see cref="int.MaxValue"/>).
        /// </summary>
        public int Next()
        {
            return (int)(NextUL() & InternalData.LOWER_MASK);
        }

        /// <summary>
        /// Gets a random integer from 0 (inclusive) up to a cap (exclusive).
        /// </summary>
        public int Next(int cap)
        {
            return Next() % cap;
        }

        /// <summary>
        /// Gets a random integer from a min (inclusive) up to a max (exclusive).
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
        /// Gets a random float, between 0 and 1.
        /// </summary>
        public float NextFloat()
        {
            return NextUL() / ((float)ulong.MaxValue);
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
        /// Gets a random float, between 0 and cap.
        /// </summary>
        /// <param name="cap">The upper limit.</param>
        public float NextFloat(float cap)
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
        /// Gets a random float, between two bounds.
        /// </summary>
        /// <param name="min">The lower limit.</param>
        /// <param name="cap">The upper limit.</param>
        public float NextFloat(float min, float cap)
        {
            return (NextUL() * ((cap - min) / ulong.MaxValue)) + min;
        }

        /// <summary>
        /// Gets a random entry from a list.
        /// The list must not be empty.
        /// </summary>
        /// <typeparam name="T">The object type in the list.</typeparam>
        /// <param name="list">The list of values.</param>
        /// <exception cref="InvalidOperationException">When the input list is empty.</exception>
        /// <returns>A random entry of type <typeparamref name="T"/>.</returns>
        public T NextElement<T>(IList<T> list)
        {
            if (list.Count == 0)
            {
                throw new InvalidOperationException("List is empty.");
            }
            return list[Next(list.Count)];
        }

        /// <summary>
        /// Gets a random entry from an enumerable.
        /// Returns a default value if the enumerable is empty.
        /// <para>This is a special optimized method for LINQ-like enumerables. For known-size lists, <see cref="NextElement{T}(IList{T})"/> is preferable.</para>
        /// </summary>
        /// <typeparam name="T">The object type in the enumerable.</typeparam>
        /// <param name="enumerable">The dynamically generated enumerable of values.</param>
        /// <exception cref="InvalidOperationException">When the input enumerable is empty.</exception>
        /// <returns>A random element.</returns>
        public T NextElement<T>(IEnumerable<T> enumerable)
        {
            int count = 0;
            T result = default;
            foreach (T element in enumerable)
            {
                count++;
                if (Next(count) == 0)
                {
                    result = element;
                }
            }
            if (count == 0)
            {
                throw new InvalidOperationException("Enumerable is empty.");
            }
            return result;
        }

        /// <summary>
        /// Gets a random unsigned long (from 0 to <see cref="ulong.MaxValue"/>).
        /// </summary>
        public ulong NextUL()
        {
            ulong n = (ulong)Internal.Buffer.LongLength;
            if (Internal.BufferIndex >= n)
            {
                for (ulong i = 0; i < n; i++)
                {
                    ulong x = (Internal.Buffer[i] & InternalData.UPPER_MASK) + (Internal.Buffer[(i + 1) % n] & InternalData.LOWER_MASK);
                    ulong xA = x >> 1;
                    if (x % 2 != 0)
                    {
                        xA = xA ^ 0xB5026F5AA96619E9UL;
                    }
                    Internal.Buffer[i] = Internal.Buffer[(i + 156) % n] ^ xA;
                }
                Internal.BufferIndex = 0;
            }
            ulong y = Internal.Buffer[Internal.BufferIndex++];
            y = y ^ ((y >> 29) & 0x5555555555555555UL);
            y = y ^ ((y << 17) & 0x71D67FFFEDA60000UL);
            y = y ^ ((y << 37) & 0xFFF7EEE000000000UL);
            y = y ^ (y >> 43);
            return y;
        }
    }
}
