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
using System.IO;
using FreneticGameCore.UtilitySystems;
using FreneticUtilities.FreneticToolkit;
using FreneticGameCore.MathHelpers;

namespace FreneticGameCore.FileSystems
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

        private readonly byte[] HelperBytes = new byte[32];

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
        /// <returns>The read bytes.</returns>
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
        /// Read a set of bytes.
        /// </summary>
        /// <param name="outputBytes">The byte array to read into.</param>
        /// <param name="offset">The starting offset.</param>
        /// <param name="count">The number of bytes.</param>
        public void ReadBytes(byte[] outputBytes, int offset, int count)
        {
            for (int i = 0; i < count; i++)
            {
                outputBytes[offset + i] = ReadByte();
            }
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
            ReadBytes(HelperBytes, 0, 24);
            return Location.FromDoubleBytes(HelperBytes, 0);
        }

        /// <summary>
        /// Read a location object (12 bytes).
        /// </summary>
        public Location ReadLocationFloat()
        {
            float x = ReadFloat();
            float y = ReadFloat();
            float z = ReadFloat();
            return new Location(x, y, z);
        }

        /// <summary>
        /// Read a quaternion object (32 bytes).
        /// </summary>
        public Quaternion ReadQuaternion()
        {
            ReadBytes(HelperBytes, 0, 32);
            return Quaternion.FromDoubleBytes(HelperBytes, 0);
        }

        /// <summary>
        /// Read a quaternion object (16 bytes).
        /// </summary>
        public Quaternion ReadQuaternionFloat()
        {
            float x = ReadFloat();
            float y = ReadFloat();
            float z = ReadFloat();
            float w = ReadFloat();
            return new Quaternion(x, y, z, w);
        }

        /// <summary>
        /// Read a view direction into a location object (4 bytes).
        /// </summary>
        /// <returns>The view direction location.</returns>
        public Location ReadViewDirection()
        {
            ushort yawS = ReadUShort();
            ushort pitchS = ReadUShort();
            float yaw = yawS * (360f / ushort.MaxValue);
            float pitch = pitchS * (180f / ushort.MaxValue);
            pitch -= 90f;
            return new Location
            {
                Yaw = yaw,
                Pitch = pitch
            };
        }

        /// <summary>
        /// Read a character (2 bytes).
        /// </summary>
        /// <returns></returns>
        public char ReadChar()
        {
            ReadBytes(HelperBytes, 0, 2);
            return (char) PrimitiveConversionHelper.BytesToUShort16(HelperBytes);
        }

        /// <summary>
        /// Read a short integer (2 bytes).
        /// </summary>
        public short ReadShort()
        {
            ReadBytes(HelperBytes, 0, 2);
            return PrimitiveConversionHelper.BytesToShort16(HelperBytes);
        }

        /// <summary>
        /// Read an unsigned short integer (2 bytes).
        /// </summary>
        public ushort ReadUShort()
        {
            ReadBytes(HelperBytes, 0, 2);
            return PrimitiveConversionHelper.BytesToUShort16(HelperBytes);
        }

        /// <summary>
        /// Read an integer (4 bytes).
        /// </summary>
        public int ReadInt()
        {
            ReadBytes(HelperBytes, 0, 4);
            return PrimitiveConversionHelper.BytesToInt32(HelperBytes);
        }

        /// <summary>
        /// Read an unsigned integer (4 bytes).
        /// </summary>
        public uint ReadUInt()
        {
            ReadBytes(HelperBytes, 0, 4);
            return PrimitiveConversionHelper.BytesToUInt32(HelperBytes);
        }

        /// <summary>
        /// Read a long integer (8 bytes).
        /// </summary>
        public long ReadLong()
        {
            ReadBytes(HelperBytes, 0, 8);
            return PrimitiveConversionHelper.BytesToLong64(HelperBytes);
        }

        /// <summary>
        /// Read an unsigned long integer (8 bytes).
        /// </summary>
        public ulong ReadULong()
        {
            ReadBytes(HelperBytes, 0, 8);
            return PrimitiveConversionHelper.BytesToULong64(HelperBytes);
        }

        /// <summary>
        /// Read a float (4 bytes).
        /// </summary>
        public float ReadFloat()
        {
            ReadBytes(HelperBytes, 0, 4);
            return PrimitiveConversionHelper.BytesToFloat32(HelperBytes);
        }

        /// <summary>
        /// Read a double (8 bytes).
        /// </summary>
        public double ReadDouble()
        {
            ReadBytes(HelperBytes, 0, 8);
            return PrimitiveConversionHelper.BytesToDouble64(HelperBytes);
        }

        /// <summary>
        /// Read a string with a specified length.
        /// </summary>
        public string ReadString(int length)
        {
            if (length <= 32)
            {
                ReadBytes(HelperBytes, 0, length);
                return FileHandler.DefaultEncoding.GetString(HelperBytes, 0, length);
            }
            return FileHandler.DefaultEncoding.GetString(ReadBytes(length), 0, length);
        }

        /// <summary>
        /// Read a "full set" of bytes: specified by a 4-byte length at the start of data.
        /// </summary>
        public byte[] ReadFullBytesVar()
        {
            int len = (int)ReadVarInt();
            return ReadBytes(len);
        }

        /// <summary>
        /// Read a "full" string: specified by a 4-byte length at the start of data.
        /// </summary>
        public string ReadFullStringVar()
        {
            int len = (int)ReadVarInt();
            return ReadString(len);
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
        /// Reads a variable integer from the stream.
        /// See <see cref="DataWriter.WriteVarInt(long)"/> for an explanation.
        /// </summary>
        /// <returns>The var int's value.</returns>
        public long ReadVarInt()
        {
            long res = 0;
            byte b = ReadByte();
            int shifts = 0;
            while ((b & 128) == 128)
            {
                res += (long)(b & 127) << shifts;
                shifts += 7;
                b = ReadByte();
            }
            res += (long)(b & 127) << shifts;
            if ((res & 1) == 1)
            {
                res = -((res & ~1L) >> 1);
            }
            else
            {
                res >>= 1;
            }
            return res;
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
