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
    /// Helper for writing data to a stream.
    /// </summary>
    public class DataWriter
    {
        /// <summary>
        /// The internal stream.
        /// </summary>
        public DataStream Internal;

        /// <summary>
        /// Constructs the data writer.
        /// </summary>
        /// <param name="stream">The base stream.</param>
        public DataWriter(DataStream stream)
        {
            Internal = stream;
        }

        /// <summary>
        /// Write a location object (24 bytes).
        /// </summary>
        /// <param name="loc">The data.</param>
        public void WriteLocation(Location loc)
        {
            Internal.Write(loc.ToDoubleBytes(), 0, 24);
        }

        /// <summary>
        /// Write a byte.
        /// </summary>
        /// <param name="x">The data.</param>
        public void WriteByte(byte x)
        {
            Internal.WriteByte(x);
        }

        /// <summary>
        /// Write a signed byte.
        /// </summary>
        /// <param name="x">The data.</param>
        public void WriteSByte(sbyte x)
        {
            Internal.WriteByte((byte)x);
        }

        /// <summary>
        /// Write a bool.
        /// </summary>
        /// <param name="x">The data.</param>
        public void WriteBool(bool x)
        {
            WriteByte((byte)(x ? 1 : 0));
        }

        /// <summary>
        /// Write a character (2 bytes).
        /// </summary>
        /// <param name="x"></param>
        public void WriteChar(char x)
        {
            Internal.Write(Utilities.CharToBytes(x), 0, 2);
        }

        /// <summary>
        /// Write a short integer (2 bytes).
        /// </summary>
        /// <param name="x">The data.</param>
        public void WriteShort(short x)
        {
            Internal.Write(Utilities.ShortToBytes(x), 0, 2);
        }

        /// <summary>
        /// Write an unsigned short integer (2 bytes).
        /// </summary>
        /// <param name="x">The data.</param>
        public void WriteUShort(ushort x)
        {
            Internal.Write(Utilities.UShortToBytes(x), 0, 2);
        }

        /// <summary>
        /// Write an integer (4 bytes).
        /// </summary>
        /// <param name="x">The data.</param>
        public void WriteInt(int x)
        {
            Internal.Write(Utilities.IntToBytes(x), 0, 4);
        }

        /// <summary>
        /// Write an unsigned integer (4 bytes).
        /// </summary>
        /// <param name="x">The data.</param>
        public void WriteUInt(uint x)
        {
            Internal.Write(Utilities.UIntToBytes(x), 0, 4);
        }

        /// <summary>
        /// Write a float (4 bytes).
        /// </summary>
        /// <param name="x">The data.</param>
        public void WriteFloat(float x)
        {
            Internal.Write(Utilities.FloatToBytes(x), 0, 4);
        }

        /// <summary>
        /// Write a double (8 bytes).
        /// </summary>
        /// <param name="x">The data.</param>
        public void WriteDouble(double x)
        {
            Internal.Write(Utilities.DoubleToBytes(x), 0, 8);
        }

        /// <summary>
        /// Write a long integer (8 bytes).
        /// </summary>
        /// <param name="x">The data.</param>
        public void WriteLong(long x)
        {
            Internal.Write(Utilities.LongToBytes(x), 0, 8);
        }

        /// <summary>
        /// Write an unsigned long integer (8 bytes).
        /// </summary>
        /// <param name="x">The data.</param>
        public void WriteULong(ulong x)
        {
            Internal.Write(Utilities.ULongToBytes(x), 0, 8);
        }

        /// <summary>
        /// Write a set of bytes directly to the stream.
        /// </summary>
        /// <param name="bits">The bytes.</param>
        public void WriteBytes(byte[] bits)
        {
            Internal.Write(bits, 0, bits.Length);
        }

        /// <summary>
        /// Write a "full set" of bytes to the stream: prefixing the bytes with a 4-byte length indicator.
        /// </summary>
        /// <param name="data">The data.</param>
        public void WriteFullBytes(byte[] data)
        {
            WriteInt(data.Length);
            WriteBytes(data);
        }

        /// <summary>
        /// Write a "full" string to the stream: prefixing the string with a 4-byte length indicator.
        /// </summary>
        /// <param name="str">The data.</param>
        public void WriteFullString(string str)
        {
            byte[] data = FileHandler.encoding.GetBytes(str);
            WriteInt(data.Length);
            WriteBytes(data);
        }
    }
}
