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
    /// Helper to read data from a stream.
    /// </summary>
    public class DataReader
    {
        /// <summary>
        /// The internal stream.
        /// </summary>
        public DataStream Internal;

        /// <summary>
        /// Constructs the data reader.
        /// </summary>
        /// <param name="stream">The base stream.</param>
        public DataReader(DataStream stream)
        {
            Internal = stream;
        }

        /// <summary>
        /// Reads a single byte from the stream.
        /// </summary>
        public byte ReadByte()
        {
            int r = Internal.ReadByte();
            if (r < 0)
            {
                throw new EndOfStreamException("Failed to read from stream, " + Internal.Length + " bytes were available (now none)...");
            }
            return (byte)r;
        }

        /// <summary>
        /// Gets the amount of data available.
        /// </summary>
        public int Available
        {
            get
            {
                return (int)(Internal.Length - Internal.Position);
            }
        }

        /// <summary>
        /// Read a set of bytes.
        /// </summary>
        /// <param name="count">The number of bytes.</param>
        public byte[] ReadBytes(int count)
        {
            byte[] b = new byte[count];
            for (int i = 0; i < count; i++)
            {
                b[i] = ReadByte();
            }
            return b;
        }

        /// <summary>
        /// Read a signed byte.
        /// </summary>
        /// <returns></returns>
        public sbyte ReadSByte()
        {
            int r = Internal.ReadByte();
            if (r < 0)
            {
                throw new EndOfStreamException("Failed to read from stream, " + Internal.Length + " bytes were available (now none)...");
            }
            return (sbyte)r;
        }

        /// <summary>
        /// Read a boolean (1 byte).
        /// </summary>
        /// <returns></returns>
        public bool ReadBool()
        {
            return ReadByte() == 1;
        }

        /// <summary>
        /// Read a location object (24 bytes).
        /// </summary>
        public Location ReadLocation()
        {
            return Location.FromDoubleBytes(ReadBytes(24), 0);
        }

        /// <summary>
        /// Read a character (2 bytes).
        /// </summary>
        /// <returns></returns>
        public char ReadChar()
        {
            return Utilities.BytesToChar(ReadBytes(2));
        }

        /// <summary>
        /// Read a short integer (2 bytes).
        /// </summary>
        public short ReadShort()
        {
            return Utilities.BytesToShort(ReadBytes(2));
        }

        /// <summary>
        /// Read an unsigned short integer (2 bytes).
        /// </summary>
        public ushort ReadUShort()
        {
            return Utilities.BytesToUShort(ReadBytes(2));
        }

        /// <summary>
        /// Read an integer (4 bytes).
        /// </summary>
        public int ReadInt()
        {
            return Utilities.BytesToInt(ReadBytes(4));
        }

        /// <summary>
        /// Read an unsigned integer (4 bytes).
        /// </summary>
        public uint ReadUInt()
        {
            return Utilities.BytesToUInt(ReadBytes(4));
        }

        /// <summary>
        /// Read a long integer (8 bytes).
        /// </summary>
        public long ReadLong()
        {
            return Utilities.BytesToLong(ReadBytes(8));
        }

        /// <summary>
        /// Read an unsigned long integer (8 bytes).
        /// </summary>
        public ulong ReadULong()
        {
            return Utilities.BytesToULong(ReadBytes(8));
        }

        /// <summary>
        /// Read a float (4 bytes).
        /// </summary>
        public float ReadFloat()
        {
            return Utilities.BytesToFloat(ReadBytes(4));
        }

        /// <summary>
        /// Read a double (8 bytes).
        /// </summary>
        public double ReadDouble()
        {
            return Utilities.BytesToDouble(ReadBytes(8));
        }

        /// <summary>
        /// Read a string with a specified length.
        /// </summary>
        public string ReadString(int length)
        {
            return FileHandler.encoding.GetString(ReadBytes(length));
        }

        /// <summary>
        /// Read a "full set" of bytes: specified by a 4-byte length at the start of data.
        /// </summary>
        public byte[] ReadFullBytes()
        {
            int len = ReadInt();
            return ReadBytes(len);
        }

        /// <summary>
        /// Read a "full" string: specified by a 4-byte length at the start of data.
        /// </summary>
        public string ReadFullString()
        {
            int len = ReadInt();
            return ReadString(len);
        }

        /// <summary>
        /// Close the underlying stream.
        /// </summary>
        public void Close()
        {
            Internal.Close();
        }
    }
}
