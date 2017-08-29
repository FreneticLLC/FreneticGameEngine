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
using System.IO;

namespace FreneticGameCore.Files
{
    /// <summary>
    /// A simplified in-memory stream object.
    /// </summary>
    public sealed class DataStream : Stream
    {
        /// <summary>
        /// Wrapped internal stream.
        /// </summary>
        public byte[] Wrapped;

        /// <summary>
        /// Current data length.
        /// </summary>
        public long Len = 0;

        /// <summary>
        /// Current index.
        /// </summary>
        public long Ind = 0;

        /// <summary>
        /// Returns true.
        /// </summary>
        public override bool CanRead
        {
            get
            {
                return true;
            }
        }

        /// <summary>
        /// Returns true.
        /// </summary>
        public override bool CanSeek
        {
            get
            {
                return true;
            }
        }

        /// <summary>
        /// Returns true.
        /// </summary>
        public override bool CanWrite
        {
            get
            {
                return true;
            }
        }

        /// <summary>
        /// Get length.
        /// </summary>
        public override long Length
        {
            get
            {
                return Len + Ind;
            }
        }

        /// <summary>
        /// Get or set position index.
        /// </summary>
        public override long Position
        {
            get
            {
                return Ind;
            }
            set
            {
                Ind = value;
            }
        }

        /// <summary>
        /// Constructs a data stream with bytes pre-loaded.
        /// </summary>
        /// <param name="bytes">The bytes to pre-load.</param>
        public DataStream(byte[] bytes)
        {
            Wrapped = bytes;
            Len = bytes.LongLength;
            Ind = 0;
        }

        /// <summary>
        /// Constructs an empty data stream.
        /// </summary>
        public DataStream()
        {
            Wrapped = new byte[0];
            Len = 0;
            Ind = 0;
        }

        /// <summary>
        /// Constructs a datastream with a specific capacity.
        /// </summary>
        /// <param name="capacity">The capacity.</param>
        public DataStream(int capacity)
        {
            Wrapped = new byte[capacity];
            Len = 0;
            Ind = 0;
        }

        /// <summary>
        /// Does nothing!
        /// </summary>
        public override void Flush()
        {
        }

        /// <summary>
        /// Set to a specific location.
        /// </summary>
        /// <param name="offset">Where to move to.</param>
        /// <param name="origin">What position to seek relative to.</param>
        /// <returns>New index.</returns>
        public override long Seek(long offset, SeekOrigin origin)
        {
            if (origin == SeekOrigin.Begin)
            {
                Ind = offset;
            }
            else if (origin == SeekOrigin.Current)
            {
                Ind += offset;
            }
            else if (origin == SeekOrigin.End)
            {
                Ind = Len - offset;
            }
            return Ind;
        }

        /// <summary>
        /// Set the length of the stream.
        /// </summary>
        /// <param name="res_len">The resultant length.</param>
        public override void SetLength(long res_len)
        {
            Len = res_len;
        }

        /// <summary>
        /// Set the length of the underlying buffer.
        /// </summary>
        /// <param name="res_len">The resultant length.</param>
        public void SetCapacity(long res_len)
        {
            byte[] t = new byte[res_len];
            Array.Copy(Wrapped, Ind, t, 0, Math.Min(res_len, Len));
            Wrapped = t;
        }

        /// <summary>
        /// Reads a single byte, or returns -1.
        /// </summary>
        /// <returns>The byte read.</returns>
        public override int ReadByte()
        {
            if (Len == 0)
            {
                return -1;
            }
            Len--;
            return Wrapped[Ind++];
        }

        /// <summary>
        /// Read some data.
        /// </summary>
        /// <param name="buffer">Data read buffer.</param>
        /// <param name="offset">Start index.</param>
        /// <param name="count">Length.</param>
        /// <returns>Bytes read.</returns>
        public override int Read(byte[] buffer, int offset, int count)
        {
            if (Len >= count)
            {
                for (long i = 0; i < count; i++)
                {
                    buffer[offset + i] = Wrapped[Ind++];
                }
                Len -= count;
                return count;
            }
            if (Len == 0)
            {
                return -1;
            }
            long validlen = Math.Min(Len, count);
            for (long i = 0; i < validlen; i++)
            {
                buffer[offset + i] = Wrapped[Ind++];
            }
            Len -= validlen;
            return (int)validlen;
        }

        /// <summary>
        /// Write some data.
        /// </summary>
        /// <param name="buffer">Data to write.</param>
        /// <param name="offset">Start index.</param>
        /// <param name="count">Length.</param>
        public override void Write(byte[] buffer, int offset, int count)
        {
            if (Wrapped.Length - (Len + Ind) < count)
            {
                SetCapacity((Len + count) * 2);
            }
            for (int i = 0; i < count; i++)
            {
                Wrapped[Ind + Len++] = buffer[offset + i];
            }
        }

        /// <summary>
        /// Returns the internal data array.
        /// </summary>
        /// <returns>Bytes.</returns>
        public byte[] ToArray()
        {
            byte[] b = new byte[Len];
            Array.Copy(Wrapped, Ind, b, 0, Len);
            return b;
        }
    }
}
